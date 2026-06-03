namespace ItBudgetingV3.Domain.Entities;

public sealed class PersonProfile
{
    public Guid PersonId { get; init; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Dictionary<string, bool> ConsentFlags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
