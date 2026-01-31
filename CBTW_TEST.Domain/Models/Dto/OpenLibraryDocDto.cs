using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CBTW_TEST.Domain.Models.Dto
{
    public record OpenLibraryDocDto(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("author_name")] List<string>? AuthorNames,
        [property: JsonPropertyName("first_publish_year")] int? FirstPublishYear,
        [property: JsonPropertyName("author_key")] List<string>? AuthorKeys,
        [property: JsonPropertyName("seed")] List<string>? Seed
    );
}
