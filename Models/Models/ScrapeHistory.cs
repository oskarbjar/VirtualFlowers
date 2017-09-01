using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ScrapeHistoryTeams
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime LastDayScraped { get; set; }
    }

    public class ScrapeHistoryRankingList
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime LastDayScraped { get; set; }
    }

    public class TransferHistory
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime TransferDate { get; set; }
        public int OldTeamId { get; set; }
        public string OldTeamName { get; set; }
        public int NewTeamId { get; set; }
        public string NewTeamName { get; set; }
    }

    public class ScrapedMatches
    {
        public int MatchId { get; set; }
        public string MatchUrl { get; set; }
        public string Name { get; set; }
        public string Event { get; set; }
        public DateTime Start { get; set; }
        public DateTime CachedStart { get; set; }
        public DateTime CachedEnd { get; set; }
        public string SportName { get; set; }
        public string Json { get; set; }
    }
}
