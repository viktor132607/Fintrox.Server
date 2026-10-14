using System.Globalization;
using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

/// <summary>
/// Strict four-column CSV parser for the neutral Fintrox bank import schema.
/// Uses ISO dates, dot-decimal amounts and standard quoted CSV fields.
/// </summary>
public static class BankStatementCsvParser
{
    private const int MaxSize = 1_048_576;
    private static readonly string[] Header =
        ["externalId", "valueDate", "signedAmount", "description"];

    public static ImportBankStatementRequest Parse(ImportCsvBankStatementRequest request)
    {
        if (string.IsNullOrEmpty(request.Csv) ||
            System.Text.Encoding.UTF8.GetByteCount(request.Csv) > MaxSize)
            throw new BankReconciliationConflictException(
                "CSV must be present and no larger than 1 MiB.");

        var rows = ParseRecords(request.Csv);
        if (rows.Count is < 2 or > 10_001 ||
            !rows[0].SequenceEqual(Header, StringComparer.Ordinal))
            throw new BankReconciliationConflictException(
                "CSV must start with externalId,valueDate,signedAmount,description and contain 1-10000 rows.");

        var items = new List<BankStatementLineImportRequest>();
        foreach (var row in rows.Skip(1))
        {
            if (row.Count != 4 ||
                !DateOnly.TryParseExact(row[1], "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                !decimal.TryParse(row[2],
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var amount))
                throw new BankReconciliationConflictException(
                    "Bank CSV row must contain four valid fields with ISO dates and dot-decimal amounts.");

            items.Add(new BankStatementLineImportRequest(
                row[0], date, amount, row[3]));
        }

        return new ImportBankStatementRequest(
            request.ExternalId, request.FromDate, request.ToDate,
            request.OpeningBalance, request.ClosingBalance, items);
    }

    private static List<List<string>> ParseRecords(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new System.Text.StringBuilder();
        var insideQuotes = false;
        var afterQuote = false;
        var atFieldStart = true;

        void FinishField()
        {
            row.Add(field.ToString());
            field.Clear();
            atFieldStart = true;
            afterQuote = false;
        }

        void FinishRow()
        {
            FinishField();
            rows.Add(row);
            if (rows.Count > 10_001)
                throw new BankReconciliationConflictException("CSV has too many rows.");
            row = new List<string>();
        }

        var initial = csv.Length > 0 && csv[0] == (char)65279 ? 1 : 0;
        for (var i = initial; i < csv.Length; i++)
        {
            var ch = csv[i];
            if (insideQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = false;
                        afterQuote = true;
                    }
                }
                else field.Append(ch);
            }
            else if (ch == '"')
            {
                if (!atFieldStart || afterQuote)
                    throw new BankReconciliationConflictException("Invalid CSV quoting.");
                insideQuotes = true;
                atFieldStart = false;
            }
            else if (ch == ',')
            {
                FinishField();
            }
            else if (ch == (char)10 || ch == (char)13)
            {
                if (ch == (char)13 && i + 1 < csv.Length &&
                    csv[i + 1] == (char)10) i++;
                FinishRow();
            }
            else
            {
                if (afterQuote)
                    throw new BankReconciliationConflictException("Invalid CSV quoting.");
                field.Append(ch);
                atFieldStart = false;
            }
            if (field.Length > 4096)
                throw new BankReconciliationConflictException("Oversized CSV cell.");
        }

        if (insideQuotes)
            throw new BankReconciliationConflictException("Unclosed quoted CSV cell.");
        if (row.Count > 0 || field.Length > 0 || afterQuote)
            FinishRow();
        return rows;
    }
}
