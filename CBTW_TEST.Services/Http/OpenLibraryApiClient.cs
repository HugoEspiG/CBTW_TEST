using CBTW_TEST.Domain.Models.Dto;
using Microsoft.Extensions.Caching.Memory; // Sustituye a Dapr
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CBTW_TEST.Services.Http
{
    public class OpenLibraryApiClient
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OpenLibraryApiClient> _logger;

        // Cambiamos IHttpClientFactory por HttpClient directamente
        public OpenLibraryApiClient(
            HttpClient client,
            IMemoryCache cache,
            ILogger<OpenLibraryApiClient> logger)
        {
            _client = client;
            _cache = cache;
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

                // Generamos la misma lógica de key que tenías
                string cacheKey = $"search_{hypothesis.Title}_{hypothesis.Author}".Replace(" ", "_").ToLower();

                // 1. Intentar obtener del caché de memoria (Sustituye a GetStateAsync)
                if (_cache.TryGetValue(cacheKey, out List<OpenLibraryDocDto>? cachedDocs))
                {
                    _logger.LogInformation("Cache hit for key: {cacheKey}", cacheKey);
                    return cachedDocs ?? new();
                }

                List<OpenLibraryDocDto> docs = new();

                // Tier 1: Title + Author
                if (!string.IsNullOrEmpty(hypothesis.Title) && !string.IsNullOrEmpty(hypothesis.Author))
                {
                    string url = $"/search.json?title={Uri.EscapeDataString(hypothesis.Title)}&author={Uri.EscapeDataString(hypothesis.Author)}&limit=10";
                    var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                    docs = response?.Docs ?? new();
                }

                // Tier 2: Title Only
                if (docs.Count == 0 && !string.IsNullOrEmpty(hypothesis.Title))
                {
                    _logger.LogInformation("Tier 1 failed. Falling back to Title-only search.");
                    string url = $"/search.json?title={Uri.EscapeDataString(hypothesis.Title)}&limit=10";
                    var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                    docs = response?.Docs ?? new();
                }

                // Tier 3: General Query
                if (docs.Count == 0)
                {
                    _logger.LogInformation("Tier 2 failed. Falling back to General query.");
                    string combined = $"{hypothesis.Title} {hypothesis.Author}".Trim();
                    string url = $"/search.json?q={Uri.EscapeDataString(combined)}&limit=10";
                    var response = await _client.GetFromJsonAsync<OpenLibrarySearchResponse>(url);
                    docs = response?.Docs ?? new();
                }

                // 2. Guardar en caché de memoria (Sustituye a SaveStateAsync)
                // Usamos un TTL de 1 hora como tenías en Dapr
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, docs, cacheEntryOptions);

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