using Ybt.Core.Entities;

namespace Ybt.Web.Models;

public class AboutViewModel
{
    public List<TeamMember> TeamMembers { get; set; } = new();
    public List<AboutFeature> AboutFeatures { get; set; } = new();
}
