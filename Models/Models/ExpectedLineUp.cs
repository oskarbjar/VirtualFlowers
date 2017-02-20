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
        public List<Player> Players { get; set; }


    }
}
