using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [NotMapped]
    public class ExpectedLineUp
    {
        public DateTime Start { get; set; }
        public int MatchId { get; set; }
        public string EventName { get; set; }
        public List<Player> Players { get; set; }

        public ExpectedLineUp()
        {
            this.Players = new List<Player>();
        }
    }
}
