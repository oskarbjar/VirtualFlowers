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
}
