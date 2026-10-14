using Fintrox.Application.Accounting;
using Fintrox.Contracts.Accounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

[TestClass]
public sealed class BankStatementCsvParserTests
{
    private static ImportCsvBankStatementRequest Request(string csv) =>
        new("CSV-2026-03", new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31), 0m, 75.5m, csv);

    [TestMethod]
    public void ParserAcceptsQuotedCommasEscapedQuotesAndUtf8Bom()
    {
        var request = Request(
            ((char)65279) +
            "externalId,valueDate,signedAmount,description\r\n" +
            "ROW-A,2026-03-12,75.5000,\"Payment, \"\"priority\"\"\"\r\n");
        var parsed = BankStatementCsvParser.Parse(request);
        Assert.HasCount(1, parsed.Lines);
        Assert.AreEqual("Payment, \"priority\"", parsed.Lines[0].Description);
        Assert.AreEqual(75.5000m, parsed.Lines[0].SignedAmount);
    }

    [TestMethod]
    public void ParserRejectsEuropeanDecimalSeparatorAndThousandsSyntax()
    {
        var csv = "externalId,valueDate,signedAmount,description\n" +
            "ROW,2026-03-12,\"1,200\",Payment";
        Assert.ThrowsExactly<BankReconciliationConflictException>(
            () => BankStatementCsvParser.Parse(Request(csv)));
    }

    [TestMethod]
    public void ParserRejectsUnexpectedColumnCountAndMalformedQuotes()
    {
        var invalid =
            new[]
            {
                "externalId,valueDate,signedAmount,description\nROW,2026-03-12,5",
                "externalId,valueDate,signedAmount,description\nROW,2026-03-12,5,\"unterminated",
                "externalId,valueDate,signedAmount,description\nROW,2026-03-12,5,un\"quoted",
                "externalId,valueDate,signedAmount,description\nROW,2026-03-12,5,too,many"
            };
        foreach (var csv in invalid)
            Assert.ThrowsExactly<BankReconciliationConflictException>(
                () => BankStatementCsvParser.Parse(Request(csv)));
    }

    [TestMethod]
    public void ParserRejectsOversizedCsvBeforeParsing()
    {
        var csv = "externalId,valueDate,signedAmount,description\n" +
            new string('X', 1_048_576);
        Assert.ThrowsExactly<BankReconciliationConflictException>(
            () => BankStatementCsvParser.Parse(Request(csv)));
    }
}
