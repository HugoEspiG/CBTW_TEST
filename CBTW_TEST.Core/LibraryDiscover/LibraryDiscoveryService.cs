using CBTW_TEST.Core.Activities.Match;
using CBTW_TEST.Domain.Interfaces;
using CBTW_TEST.Domain.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Core.LibraryDiscover
{
    public class LibraryDiscoveryService(
            ExtractSearchEntitiesActivity extractActivity,
            SearchOpenLibraryActivity searchActivity,
            RankAndExplainActivity rankActivity) : ILibraryDiscoveryService
    {
        public async Task<WorkflowResultDto> ExecuteDiscoveryAsync(string messyBlob)
        {
            var result = new WorkflowResultDto();
            try
            {
                var hypothesis = await extractActivity.RunAsync(messyBlob);

                var rawCandidates = await searchActivity.RunAsync(hypothesis);

                if (rawCandidates == null || !rawCandidates.Any())
                {
                    return new WorkflowResultDto
                    {
                        WasSuccess = true,
                        Result = new List<BookMatchResultDto>()
                    };
                }

                // 3. Ranking y Explicación (Gemini)
                var rankedResults = await rankActivity.RunAsync(new RankingInputDto(hypothesis, rawCandidates));

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
