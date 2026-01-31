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

            string systemPrompt = $"""
            You are a Senior Library Data Analyst. Your goal is to rank book candidates based on a user's search hypothesis.
            
            USER HYPOTHESIS:
            Title: {input.Hypothesis.Title}
            Author: {input.Hypothesis.Author}
            Keywords: {string.Join(", ", input.Hypothesis.Keywords)}

            RANKING HIERARCHY (Priority 1 is highest):
            1. Exact Title + Primary Author Match.
            2. Exact Title + Contributor Match (author is an illustrator, editor, etc.).
            3. Near-match/Fuzzy Title + Author Match.
            4. Author-only match (Return top works by this author).

            INSTRUCTIONS:
            - Compare the Hypothesis against the provided list of Open Library candidates.
            - For each candidate, explain WHY it matched in 1-2 ffactual sentences.
            - Mention if the author is the primary one or just a contributor.
            - Return a JSON array of the top 5 BookMatchResultDto objects.
            """;

            var userMessage = $"Candidates from Open Library: {JsonSerializer.Serialize(input.RawCandidates)}";

            try
            {
                var response = await _geminiClient.Models.GenerateContentAsync(
                    model: "gemini-1.5-flash",
                    contents: new List<Content> {
                    new Content { Role = "user", Parts = new List<Part> { new Part { Text = systemPrompt + "\n" + userMessage } } }
                    },
                    config: config
                );

                var rawJson = response.Candidates[0].Content.Parts[0].Text;
                var results = JsonSerializer.Deserialize<List<BookMatchResultDto>>(rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return results ?? new List<BookMatchResultDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Ranking and Explanation phase.");
                return new List<BookMatchResultDto>();
            }
        }
    }
}
