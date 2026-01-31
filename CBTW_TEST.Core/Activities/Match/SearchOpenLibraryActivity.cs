using CBTW_TEST.Domain.Models.Dto;
using CBTW_TEST.Services.Http;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Core.Activities.Match
{
    public class SearchOpenLibraryActivity : WorkflowActivity<BookHypothesisDto, List<OpenLibraryDocDto>>
    {
        private readonly OpenLibraryApiClient _apiClient;
        private readonly ILogger<SearchOpenLibraryActivity> _logger;

        public SearchOpenLibraryActivity(OpenLibraryApiClient apiClient, ILogger<SearchOpenLibraryActivity> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public override async Task<List<OpenLibraryDocDto>> RunAsync(WorkflowActivityContext context, BookHypothesisDto hypothesis)
        {
            _logger.LogInformation("Searching Open Library for Title: {Title}, Author: {Author}", hypothesis.Title, hypothesis.Author);

            var results = await _apiClient.SearchBooksAsync(hypothesis);

            // Aquí podríamos aplicar la normalización de diacríticos si fuera necesario 
            // antes de devolver los resultados al workflow.

            return results;
        }
    }
}
