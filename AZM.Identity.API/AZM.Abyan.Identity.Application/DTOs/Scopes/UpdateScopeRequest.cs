using System.ComponentModel.DataAnnotations;

namespace AZM.Abyan.Identity.Application.DTOs.Scopes;

public class UpdateScopeRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
