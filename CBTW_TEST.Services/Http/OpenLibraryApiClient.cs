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
            string stateStoreName = _configuration["DAPR_STATE_STORE_NAME"] ?? "statestore";
            // Generamos una key única basada en la hipótesis (Title + Author)
            string cacheKey = $"search_{hypothesis.Title}_{hypothesis.Author}".Replace(" ", "_").ToLower();

            try
            {
                // 1. Intento recuperar de caché
                var cached = await _daprClient.GetStateAsync<List<OpenLibraryDocDto>>(stateStoreName, cacheKey);
                if (cached != null) return cached;

                // 2. Construcción de Query Params
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(hypothesis.Title)) queryParams.Add($"title={Uri.EscapeDataString(hypothesis.Title)}");
                if (!string.IsNullOrEmpty(hypothesis.Author)) queryParams.Add($"author={Uri.EscapeDataString(hypothesis.Author)}");

                // Si es muy ambiguo, usamos q= (search general)
                string url = $"/search.json?{string.Join("&", queryParams)}&limit=15";
                if (queryParams.Count == 0) url = $"/search.json?q={Uri.EscapeDataString(hypothesis.Title ?? "library")}";

                var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                var docs = response?.Docs ?? new List<OpenLibraryDocDto>();

                // 3. Guardar en caché (Time-to-live de 1 hora por ejemplo)
                if (docs.Any())
                {
                    await _daprClient.SaveStateAsync(stateStoreName, cacheKey, docs,
                        metadata: new Dictionary<string, string> { { "ttlInSeconds", "3600" } });
                }

                return docs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Open Library API");
                return new List<OpenLibraryDocDto>();
            }
        }
    }
}
