using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CBTW_TEST.Domain.Models.Dto
{
    public record OpenLibrarySearchResponse(
        [property: JsonPropertyName("numFound")] int NumFound,
        [property: JsonPropertyName("docs")] List<OpenLibraryDocDto> Docs
    );
}
