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
    public class SearchOpenLibraryActivity
    {
        private readonly OpenLibraryApiClient _apiClient;
        private readonly ILogger<SearchOpenLibraryActivity> _logger;

        public SearchOpenLibraryActivity(OpenLibraryApiClient apiClient, ILogger<SearchOpenLibraryActivity> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<OpenLibraryDocDto>> RunAsync(BookHypothesisDto hypothesis)
        {
            _logger.LogInformation("Searching Open Library for Title: {Title}, Author: {Author}", hypothesis.Title, hypothesis.Author);

            var results = await _apiClient.SearchBooksAsync(hypothesis);

            return results;
        }
    }
}
