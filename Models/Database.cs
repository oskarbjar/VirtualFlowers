using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models
{
    public class DatabaseContext :DbContext
    {

        public DatabaseContext()
           : base("CsDatabase")
        {

        }

        public DbSet<Teams> Teams {get; set; }
    }
}
