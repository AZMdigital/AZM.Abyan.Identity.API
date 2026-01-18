using System.ComponentModel.DataAnnotations;

namespace AZM.Abyan.Identity.Application.DTOs.Policies;

public class CreatePolicyRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    //[Required]
    //public string Type { get; set; } = "role"; // "role" for Role Policy
    
    //public string Logic { get; set; } = "POSITIVE";
    
    //public string DecisionStrategy { get; set; } = "UNANIMOUS";
    
    //public Dictionary<string, object> Config { get; set; } = new();
    
    public List<string> RoleNames { get; set; } = new(); // For role policy
}
