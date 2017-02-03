using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    class ScrapeHistoryTeams
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int LatestMatchId { get; set; }
    }

    class ScrapeHistoryRankingList
    {
        public int Id { get; set; }
        public int LatestRankingListId { get; set; }
    }
}
