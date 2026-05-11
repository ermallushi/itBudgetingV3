using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace itBudgetingV3.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(200)]
    public string? FullName { get; set; }
}
