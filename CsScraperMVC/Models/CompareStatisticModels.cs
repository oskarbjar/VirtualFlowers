using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CsScraperMVC.Models
{
    public class CompareStatisticModel
    {
        public int Team1Id { get; set; }
        public int Team2Id { get; set; }
        public List<TeamStatisticPeriodModel> Teams { get; set; }

        public CompareStatisticModel()
        {
            this.Teams = new List<TeamStatisticPeriodModel>();
        }
    }

    public class TeamStatisticPeriodModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public List<TeamStatisticModel> TeamStatistics { get; set; }

        public TeamStatisticPeriodModel()
        {
            this.TeamStatistics = new List<TeamStatisticModel>();
        }
    }

    public class TeamStatisticModel
    {
        public string Period { get; set; }
        public List<MapStatisticModel> Maps { get; set; }

        public TeamStatisticModel()
        {
            this.Maps = new List<MapStatisticModel>();
        }
    }

    public class MapStatisticModel
    {
        public string Map { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public double WinPercent { get; set; }
        public double AverageWinRounds { get; set; }
        public double AverageLossRounds { get; set; }
        public double DifficultyRating { get; set; }
        public double FullTeamPercent { get; set; }
    }
}