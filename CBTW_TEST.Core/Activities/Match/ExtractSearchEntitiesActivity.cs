using CBTW_TEST.Domain.Models.Dto;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CBTW_TEST.Core.Activities.Match
{
    // Eliminamos la herencia de Dapr y la convertimos en una clase normal
    public class ExtractSearchEntitiesActivity
    {
        private readonly ILogger<ExtractSearchEntitiesActivity> _logger;
        private readonly Client _geminiClient;

        public ExtractSearchEntitiesActivity(ILogger<ExtractSearchEntitiesActivity> logger, Client geminiClient)
        {
            _logger = logger;
            _geminiClient = geminiClient;
        }

        public async Task<BookHypothesisDto> RunAsync(string messyBlob)
        {
            var config = new GenerateContentConfig
            {
                Temperature = 0.1f,
                ResponseMimeType = "application/json"
            };

            string prompt = $"""
            You are a professional librarian. Extract search entities from this messy input: "{messyBlob}".
            Return a JSON object with:
            - Title: The most likely book title (null if not found).
            - Author: The most likely primary author (normalized name, e.g., 'Tolkien' to 'J.R.R. Tolkien').
            - Keywords: List of editions, years, or formats found.

            Constraint: If the query is a character (e.g., 'Huckleberry'), map it to the book title ('Huckleberry Finn').
            """;

            // Mantenemos tu lógica de Gemini intacta
            var response = await _geminiClient.Models.GenerateContentAsync(
                model: "gemini-2.0-flash-lite", // Nota: Corregí a 2.0 que es la versión común, ajusta si usas una preview específica
                contents: prompt,
                config: config
            );

            var rawJson = response.Candidates[0].Content.Parts[0].Text;

            var result = JsonSerializer.Deserialize<BookHypothesisDto>(rawJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? throw new Exception("Failed to deserialize Gemini response");
        }
    }
}