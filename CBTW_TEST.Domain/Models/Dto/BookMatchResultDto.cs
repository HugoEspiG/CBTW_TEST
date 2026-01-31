using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Domain.Models.Dto
{
    public class BookMatchResultDto
    {
        public BookMatchResultDto(int rank, string title, string author, string explanation, string openLibraryKey, string matchLevel)
        {
            Rank = rank;
            Title = title;
            Author = author;
            Explanation = explanation;
            OpenLibraryKey = openLibraryKey;
            MatchLevel = matchLevel;
        }

        public int Rank { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Explanation { get; set; }
        public string OpenLibraryKey { get; set; }
        public string MatchLevel { get; set; } // "Exact", "ContributorMatch", "Partial", "AuthorFallback"
    }
}
