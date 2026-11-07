namespace GithubActivity.Models
{
    public class CommandParam
    {
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }
        public bool IsRequired { get; set; } = true;
    }
}