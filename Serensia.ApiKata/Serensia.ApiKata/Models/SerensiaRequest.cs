namespace Serensia.ApiKata.Models
{
    public sealed class SerensiaRequest
    {
        public string Term { get; set; } = string.Empty;
        public List<string> Candidates { get; set; } = new();
        public int TopN { get; set; } = 5;
    }
}
