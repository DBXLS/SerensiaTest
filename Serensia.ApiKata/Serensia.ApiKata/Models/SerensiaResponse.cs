namespace Serensia.ApiKata.Models
{
    public sealed class SerensiaResponse
    {
        public required string Term { get; init; }
        public required IReadOnlyList<string> Suggestions { get; init; }
    }
}
