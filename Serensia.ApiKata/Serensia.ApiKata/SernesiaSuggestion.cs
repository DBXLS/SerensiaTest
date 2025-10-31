using System.Text.RegularExpressions;

namespace Serensia.ApiKata
{
    public sealed class SernesiaSuggestion
    {
        public IReadOnlyList<string> Suggestion(string term, IEnumerable<string> candidates, int topN)
        {
            if (term is null) throw new ArgumentNullException(nameof(term));
            if (candidates is null) throw new ArgumentNullException(nameof(candidates));
            if (topN <= 0) return Array.Empty<string>();

            var query = Normalisation(term);
            int qlen = query.Length;

            var scored = new List<(string word, int distance, int lengthDelta)>();

            foreach (var raw in candidates)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var word = Normalisation(raw);
                if (word.Length < qlen) continue; // pas assez de lettres

                int best = Min_Fenetre(word, query);
                scored.Add((raw, best, Math.Abs(word.Length - qlen)));
            }

            return scored
                .OrderBy(s => s.distance)
                .ThenBy(s => s.lengthDelta)
                .ThenBy(s => s.word, StringComparer.Ordinal)
                .Take(topN)
                .Select(s => s.word)
                .ToList();
        }
        private static int Min_Fenetre(string word, string query)
        {
            int qlen = query.Length;
            int wlen = word.Length;
            int best = int.MaxValue;

            for (int i = 0; i + qlen <= wlen; i++)
            {
                int d = GetDiff_Score(word.AsSpan(i, qlen), query.AsSpan());
                if (d < best) best = d;
                if (best == 0) break; 
            }
            return best;
        }

        public static int GetDiff_Score(ReadOnlySpan<char> dest, ReadOnlySpan<char> src)
        {
            if (dest.Length != src.Length)
                throw new ArgumentException("Les longueurs doivent correspondre pour la distance de Hamming.");

            int diff = 0;
            for (int i = 0; i < dest.Length; i++)
                if (dest[i] != src[i]) diff++;

            return diff;
        }
        private static string Normalisation(string s)
        {
            var lower = s.ToLowerInvariant();
            return Regex.Replace(lower, "[^a-z0-9]", "");
        }
    }
}
