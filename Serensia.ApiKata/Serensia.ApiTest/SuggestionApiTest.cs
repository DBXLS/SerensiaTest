using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Serensia.ApiKata.Models;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Threading.Tasks;

namespace Serensia.ApiTest
{
    public  class SuggestionApiTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SuggestionApiTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(_ => { });
        }

        [Fact]
        public async Task Post_Suggestion_Should_Return_Expected_Example()
        {
            var client = _factory.CreateClient();

            var payload = new
            {
                term = "gros",
                candidates = new[] { "gros", "gras", "graisse", "agressif", "go", "ros", "gro" },
                topN = 2
            };

            var resp = await client.PostAsJsonAsync("/suggestion", payload);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<SerensiaResponse>();
            body.Should().NotBeNull();
            body!.Term.Should().Be("gros");
            body.Suggestions.Should().Equal("gros", "gras");
        }
    }

}
