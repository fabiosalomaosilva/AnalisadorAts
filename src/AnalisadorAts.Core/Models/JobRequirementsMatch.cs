namespace AnalisadorAts.Core.Models;

public class JobRequirementsMatch
{
    public int RequiredSkillsMet { get; set; }
    public int RequiredSkillsTotal { get; set; }
    public int DesiredSkillsMet { get; set; }
    public int DesiredSkillsTotal { get; set; }
    public bool ExperienceMatch { get; set; }
}
