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
    public class RankAndExplainActivity : WorkflowActivity<RankingInputDto, List<BookMatchResultDto>>
    {
        private readonly Client _geminiClient;
        private readonly ILogger<RankAndExplainActivity> _logger;

        public RankAndExplainActivity(Client geminiClient, ILogger<RankAndExplainActivity> logger)
        {
            _geminiClient = geminiClient;
            _logger = logger;
        }

        public override async Task<List<BookMatchResultDto>> RunAsync(WorkflowActivityContext context, RankingInputDto input)
        {
            var config = new GenerateContentConfig
            {
                Temperature = 0.1f,
                ResponseMimeType = "application/json"
            };

            string jsonSchema = """
                    {
                        "Rank": 1,
                        "Title": "Example Title",
                        "Author": "Example Author",
                        "Explanation": "Example Explanation",
                        "OpenLibraryKey": "/works/OL12345W",
                        "MatchLevel": "Exact"
                    }
                    """;

            string systemPrompt = $"""
                You are a Senior Library Data Analyst. Your absolute priority is ACCURACY.
    
                USER HYPOTHESIS:
                - Title: {input.Hypothesis.Title}
                - Author: {input.Hypothesis.Author}

                STRICT RANKING RULES:
                1. Exact: Title matches AND Author matches (Primary).
                2. ContributorMatch: Title matches AND Author is a contributor/editor.
                3. Partial: 
                   - Use this if Title matches but Author is COMPLETELY DIFFERENT. 
                   - Use this for fuzzy title matches with correct author.
                4. AuthorFallback: Title does not match, but it is a work by the requested Author.

                CRITICAL LOGIC FOR DISCREPANCIES:
                - If Title matches but the Candidate's Author is NOT the one requested, you MUST use 'Partial'. 
                - In the 'Explanation', explicitly state: "Title match found, but author mismatch (Requested: {input.Hypothesis.Author}, Found: Candidate Author)".
    
                OUTPUT: JSON ARRAY of {jsonSchema}
                """;

            var userMessage = $"Candidates from Open Library: {JsonSerializer.Serialize(input.RawCandidates)}";

            try
            {
                var response = await _geminiClient.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash-lite",
                    contents: new List<Content> {
                new Content {
                    Role = "user",
                    Parts = new List<Part> { new Part { Text = systemPrompt + "\n" + userMessage } }
                }
                    },
                    config: config
                );

                var rawJson = response.Candidates[0].Content.Parts[0].Text;
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<BookMatchResultDto>>(rawJson, options);

                return results ?? new List<BookMatchResultDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Ranking and Explanation phase.");
                throw new Exception(ex.Message);
            }
        }
    }
}
