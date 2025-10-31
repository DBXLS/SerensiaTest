using Microsoft.AspNetCore.Http.HttpResults;
using Serensia.ApiKata;
using Serensia.ApiKata.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SernesiaSuggestion>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapPost("/suggestion", Results<Ok<SerensiaResponse>, BadRequest<string>> (
    SerensiaRequest req, SernesiaSuggestion result) =>
{
    if (string.IsNullOrWhiteSpace(req.Term))
        return TypedResults.BadRequest("'term' est requis.");

    if (req.Candidates is null || req.Candidates.Count == 0)
        return TypedResults.BadRequest("'candidates' ne doit pas �tre vide.");

    var topN = req.TopN <= 0 ? 5 : req.TopN;
    var suggestions = result.Suggestion(req.Term, req.Candidates, topN);

    return TypedResults.Ok(new SerensiaResponse
    {
        Term = req.Term,
        Suggestions = suggestions
    });
})
.WithName("Suggestion")
.WithOpenApi(operation =>
{
    operation.Summary = "Retourne les N meilleures suggestions pour un terme donn�.";
    operation.Description = "Classe les candidats par distance minimale de remplacement, puis �cart de longueur, puis ordre alphab�tique.";
    return operation;
});

app.Run();

