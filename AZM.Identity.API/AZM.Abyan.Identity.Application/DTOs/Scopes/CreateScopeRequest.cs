using System.ComponentModel.DataAnnotations;

namespace AZM.Abyan.Identity.Application.DTOs.Scopes;

public class CreateScopeRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
