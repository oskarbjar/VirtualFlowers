using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualFlowersMVC.Models
{
    public class CompareStatisticModel
    {
        public int Team1Id { get; set; }
        public int Team2Id { get; set; }
        public string MatchUrl { get; set; }
        public List<string> PeriodSelection { get; set; }
        public bool Scrape { get; set; }
        public List<TeamStatisticPeriodModel> Teams { get; set; }
        public ExpectedLineUp ExpectedLineUp { get; set; }

        public CompareStatisticModel()
        {
            this.Teams = new List<TeamStatisticPeriodModel>();
            this.PeriodSelection = new List<string>();
        }
    }

    public class TeamStatisticPeriodModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public double TeamDifficultyRating { get; set; }
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
        public List<SuggestedMapModel> SuggestedMaps { get; set; }

        public TeamStatisticModel()
        {
            this.Maps = new List<MapStatisticModel>();
            this.SuggestedMaps = new List<SuggestedMapModel>();
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
        public double AverageWinRoundsWhenWin { get; set; }
        public double AverageLossRoundsWhenWin { get; set; }
        public double AverageWinRoundsWhenLoss { get; set; }
        public double AverageLossRoundsWhenLoss { get; set; }
        public double DifficultyRating { get; set; }
        public string DiffTitleGroupBy { get; set; }
        public Tuple<double,string> FullTeamRanking { get; set; }
    }


    public class SuggestedMapModel
    {
        public string Map { get; set; }
        public double WinPercent { get; set; }
        public double DifficultyRating { get; set; }
        public int WinLossRecord { get; set; }
        public double SuggestedRank { get; set; }
    }

}