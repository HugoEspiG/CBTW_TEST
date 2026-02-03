using CBTW_TEST.Core.Activities.Match;
using CBTW_TEST.Domain.Models.Dto;
using Dapr.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Core.Workflows.Match
{
    public class LibraryDiscoveryWorkflow : Workflow<string, WorkflowResultDto>
    {
        public override async Task<WorkflowResultDto> RunAsync(WorkflowContext context, string messyBlob)
        {
            var result = new WorkflowResultDto();
            try
            {
                var hypothesis = await context.CallActivityAsync<BookHypothesisDto>(
                    nameof(ExtractSearchEntitiesActivity), messyBlob);

                var rawCandidates = await context.CallActivityAsync<List<OpenLibraryDocDto>>(
                    nameof(SearchOpenLibraryActivity), hypothesis);

                if (!rawCandidates.Any())
                {
                    return new WorkflowResultDto
                    {
                        WasSuccess = true,
                        Result = new List<BookMatchResultDto>()
                    };
                }
                var rankedResults = await context.CallActivityAsync<List<BookMatchResultDto>>(
                    nameof(RankAndExplainActivity),
                    new RankingInputDto(hypothesis, rawCandidates));

                result = new WorkflowResultDto
                {
                    WasSuccess = true,
                    Result = rankedResults.OrderBy(r => r.Rank).Take(5).ToList()
                };
            }
            catch (Exception ex)
            {
                result = new WorkflowResultDto { WasSuccess = false, Result = ex.Message };
            }
            return result;
        }
    }
}
