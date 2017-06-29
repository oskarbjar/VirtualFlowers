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
           : base("DefaultConnection")
        {

        }

        public DbSet<Player> Player { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<Match> Match { get; set; }
        public DbSet<RankingList> RankingList { get; set; }
        public DbSet<Rank> Rank { get; set; }
        public DbSet<ScrapeHistoryTeams> ScrapeHistoryTeams { get; set; }
        public DbSet<ScrapeHistoryRankingList> ScrapeHistoryRankingList { get; set; }
        public DbSet<TransferHistory> TransferHistory { get; set; }
        public DbSet<ErrorLogger> ErrorLoggers { get; set; }

        public DbSet<RoundHistory> RoundHistories { get; set; }

        //public System.Data.Entity.DbSet<Models.Models.FootballTips> FootballTips { get; set; }
    }
}
