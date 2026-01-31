using CBTW_TEST.Domain.Models.Dto;
using Dapr.Workflow;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CBTW_TEST.Core.Activities.Match
{
    public class ExtractSearchEntitiesActivity : WorkflowActivity<string, BookHypothesisDto>
    {
        private readonly ILogger<ExtractSearchEntitiesActivity> _logger;
        private readonly Client _geminiClient;

        public ExtractSearchEntitiesActivity(ILogger<ExtractSearchEntitiesActivity> logger, Client geminiClient)
        {
            _logger = logger;
            _geminiClient = geminiClient;
        }

        public override async Task<BookHypothesisDto> RunAsync(WorkflowActivityContext context, string messyBlob)
        {
            // Configuramos Gemini para que responda estrictamente en JSON
            var config = new GenerateContentConfig
            {
                Temperature = 0.1f, // Baja temperatura para mayor precisión técnica
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

            try
            {
                var response = await _geminiClient.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash",
                    contents: prompt,
                    config: config
                );
                   
                var rawJson = response.Candidates[0].Content.Parts[0].Text;

                // Usamos System.Text.Json con CaseInsensitive por seguridad
                var result = JsonSerializer.Deserialize<BookHypothesisDto>(rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? throw new Exception("Failed to deserialize Gemini response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini Extraction Failed for blob: {blob}", messyBlob);
                // Estrategia de Fallback: devolvemos el blob como título para no detener el workflow
                return new BookHypothesisDto(messyBlob, string.Empty, Enumerable.Empty<string>().ToArray(), string.Empty, string.Empty);
            }
        }
    }
}
