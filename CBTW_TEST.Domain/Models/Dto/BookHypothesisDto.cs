using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Domain.Models.Dto
{
    public class BookHypothesisDto
    {
        public BookHypothesisDto(string? title, string? author, string[] keywords, string confidenceScore, string reason)
        {
            Title = title;
            Author = author;
            Keywords = keywords;
            ConfidenceScore = confidenceScore;
            Reason = reason;
        }

        public string? Title { get; set; }
        public string? Author { get; set; }
        public string[] Keywords { get; set; }
        public string ConfidenceScore { get; set; } // "Specific", "AuthorOnly", "Ambiguous"
        public string Reason { get; set; } // "Specific", "AuthorOnly", "Ambiguous"
    }
}
