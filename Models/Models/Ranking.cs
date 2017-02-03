using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RankingList
    {
        public int      Id            { get; set; }
        public Guid      RankingListId { get; set; }
        public DateTime DateOfRank    { get; set; }
    }

    public class Rank
    {
        public int Id            { get; set; }
        public int RankId        { get; set; }
        public Guid RankingListId { get; set; }
        public int RankPosition  { get; set; }
        public int TeamId        { get; set; }
        public int Points        { get; set; }
        [NotMapped]
        public string Url { get; set; }
    }
}
