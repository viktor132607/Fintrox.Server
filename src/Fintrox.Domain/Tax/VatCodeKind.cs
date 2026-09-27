namespace Fintrox.Domain.Tax;

public enum VatCodeKind
{
    Standard = 0,
    Reduced = 10,
    ZeroRated = 20,
    Exempt = 30,
    OutOfScope = 40,
    ReverseCharge = 50
}
