using CBTW_TEST.Domain.Models.Dto;
using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Services.Http
{
    public class OpenLibraryApiClient
    {
        private readonly HttpClient _client;
        private readonly DaprClient _daprClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenLibraryApiClient> _logger;

        public OpenLibraryApiClient(IHttpClientFactory clientFactory, DaprClient daprClient, IConfiguration configuration, ILogger<OpenLibraryApiClient> logger)
        {
            _client = clientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://openlibrary.org/");
            _daprClient = daprClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<OpenLibraryDocDto>> SearchBooksAsync(BookHypothesisDto hypothesis)
        {
            try
            {
                if (string.IsNullOrEmpty(hypothesis.Title) && string.IsNullOrEmpty(hypothesis.Author))
                {
                    throw new ArgumentNullException(nameof(hypothesis));
                }
                string stateStoreName = _configuration["DAPR_STATE_STORE_NAME"] ?? "statestore";
                string cacheKey = $"search_{hypothesis.Title}_{hypothesis.Author}".Replace(" ", "_").ToLower();


                var cached = await _daprClient.GetStateAsync<List<OpenLibraryDocDto>>(stateStoreName, cacheKey);
                if (cached != null) return cached;

                List<OpenLibraryDocDto> docs = new();
                if (!string.IsNullOrEmpty(hypothesis.Title) && !string.IsNullOrEmpty(hypothesis.Author))
                {
                    string url = $"/search.json?title={Uri.EscapeDataString(hypothesis.Title)}&author={Uri.EscapeDataString(hypothesis.Author)}&limit=10";
                    var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                    docs = response?.Docs ?? new();
                }
                if (docs.Count == 0 && !string.IsNullOrEmpty(hypothesis.Title))
                {
                    _logger.LogInformation("Tier 1 failed. Falling back to Title-only search.");
                    string url = $"/search.json?title={Uri.EscapeDataString(hypothesis.Title)}&limit=10";
                    var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                    docs = response?.Docs ?? new();
                }
                if (docs.Count == 0)
                {
                    _logger.LogInformation("Tier 2 failed. Falling back to General query.");
                    string combined = $"{hypothesis.Title} {hypothesis.Author}".Trim();
                    string url = $"/search.json?q={Uri.EscapeDataString(combined)}&limit=10";
                    var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                    docs = response?.Docs ?? new();
                }

                await _daprClient.SaveStateAsync(stateStoreName, cacheKey, docs,
                    metadata: new Dictionary<string, string> { { "ttlInSeconds", "3600" } });

                return docs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Open Library API after tiered attempts");
                return new List<OpenLibraryDocDto>();
            }
        }
    }
}
