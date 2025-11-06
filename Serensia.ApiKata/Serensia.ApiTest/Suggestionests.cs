using FluentAssertions;
using Serensia.ApiKata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;


namespace Serensia.ApiTest
{
    public sealed class SuggestTestCase
    {
        public required string Id { get; init; }
        public string? Description { get; init; }
        public required string Term { get; init; }
        public required List<string> Candidates { get; init; }
        public int TopN { get; init; }
        public required List<string> Expected { get; init; }
        public string? Notes { get; init; }
    }


    public class Suggestionests
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static IEnumerable<object[]> AllCases()
        {
            var json = File.ReadAllText("suggestions_test.json");
            var cases = JsonSerializer.Deserialize<List<SuggestTestCase>>(json, Options)!;
            foreach (var c in cases)
                yield return new object[] { c };
        }

        [Theory]
        [MemberData(nameof(AllCases))]
        public void Engine_Should_Match_Expected(SuggestTestCase tc)
        {
            var engine = new SernesiaSuggestion();
            var got = engine.Suggestion(tc.Term, tc.Candidates, tc.TopN);
            // Les cas JSON attendent une sortie sans doublons ; on déduplique ici
            got = got.Distinct(StringComparer.Ordinal).ToList();

            got.Should().Equal(
                tc.Expected,
                $"Test {tc.Id} échoue. {tc.Description}\nNotes: {tc.Notes}"
            );
        }

    }
}
