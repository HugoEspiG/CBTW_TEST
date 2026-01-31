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
                // 2. IA Activity: Parsear el blob desordenado a una hipótesis estructurada
                // Gemini entra aquí para limpiar "mark huckleberry" -> { Title: "Huckleberry Finn", Author: "Mark Twain" }
                var hypothesis = await context.CallActivityAsync<BookHypothesisDto>(
                    nameof(ExtractSearchEntitiesActivity), messyBlob);

                // 3. API Activity: Buscar candidatos en Open Library
                // Aquí normalizamos y consultamos la API REST
                var rawCandidates = await context.CallActivityAsync<List<OpenLibraryBookDto>>(
                    nameof(SearchOpenLibraryActivity), hypothesis);

                // 4. IA Activity: Rankear y Explicar
                // Le pasamos la hipótesis original + los resultados para que la IA decida 
                // quién es el "Primary Author" vs "Contributor" y genere el "Why it matched"
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
                // Logging de error técnico
                result = new WorkflowResultDto { WasSuccess = false, ErrorMessage = ex.Message };
            }
            finally
            {
                await context.CallActivityAsync(nameof(LogWorkflowCompletionActivity), context.InstanceId);
            }

            return result;
        }
    }
}
