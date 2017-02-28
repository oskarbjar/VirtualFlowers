using VirtualFlowersMVC.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualFlowers;

namespace VirtualFlowersMVC.Data
{
    public class dataWorker
    {
        private DatabaseContext _db;

        public dataWorker()
        {
            _db = new DatabaseContext();
        }


        #region MATCHES

        public List<MatchesViewModel> GetMatches(int TeamId)
        {
            var query = _db.Match.Where(k => k.Team1Id == TeamId || k.Team2Id == TeamId).ToList();

            var matches = (from p in query
                           select new MatchesViewModel
                           {
                               Id = p.Id,
                               MatchId = p.MatchId,
                               Date = p.Date,
                               Map = p.Map,
                               Event = p.Event,
                               ResultT1 = p.ResultT1,
                               ResultT2 = p.ResultT2,
                               Team1Id = p.Team1Id,
                               Team1Name = _db.Team.FirstOrDefault(k => k.TeamId == p.Team1Id).TeamName,
                               Team2Id = p.Team2Id,
                               Team2Name = _db.Team.FirstOrDefault(k => k.TeamId == p.Team2Id).TeamName
                           }).ToList();

            return matches;
        }

        public Match GetMatchDetails(int? id)
        {
            var result = _db.Match.Find(id);

            return result;
        }

        #endregion

        #region TEAMS

        public List<Team> GetTeamsList()
        {
            var result = _db.Team.ToList();

            return result;
        }

        public Team GetTeamDetails(int? id)
        {
            var result = _db.Team.Find(id);

            return result;
        }

        public string GetTeamName(int id)
        {
            var result = "";

            result = _db.Team.SingleOrDefault(p => p.TeamId == id)?.TeamName;

            return result ?? "";
        }

        #endregion

        #region COMPARE

        public TeamStatisticPeriodModel GetTeamPeriodStatistics(int TeamId, List<string> PeriodSelection, ExpectedLineUp expectedLinup)
        {
            var result = new TeamStatisticPeriodModel();
            result.TeamId = TeamId;
            result.TeamName = GetTeamName(TeamId);
            result.TeamDifficultyRating = Program.GetRankingValueForTeam(TeamId, DateTime.Now);
            foreach (var period in PeriodSelection)
            {
                result.TeamStatistics.Add(GetTeamPeriodStatistics(TeamId, (PeriodEnum)int.Parse(period), expectedLinup));
            }

            return result;
        }

        public TeamStatisticModel GetTeamPeriodStatistics(int TeamId, PeriodEnum period, ExpectedLineUp expectedLinup)
        {
            var result = new TeamStatisticModel();
            var dTo = DateTime.Now;
            var dFrom = new DateTime();

            switch (period)
            {
                case PeriodEnum.ThreeMonths:
                    dFrom = dTo.AddMonths(-3);
                    result.Period = "3 Months";
                    break;
                case PeriodEnum.SixMonths:
                    dFrom = dTo.AddMonths(-6);
                    result.Period = "6 Months";
                    break;
                case PeriodEnum.Year:
                    dFrom = dTo.AddYears(-1);
                    result.Period = "Year";
                    break;
            }

            result.Maps = GetMapStatistics(TeamId, dFrom, dTo, expectedLinup);
            
            return result;
        }

        public List<MapStatisticModel> GetMapStatistics(int TeamId, DateTime dFrom, DateTime dTo, ExpectedLineUp expectedLinup)
        {
            var result = new List<MapStatisticModel>();
            var secondaryTeam = _db.TransferHistory.Where(p => p.NewTeamId == TeamId).OrderByDescending(k => k.TransferDate).FirstOrDefault();
            var secondaryTeamId = 0;
            if (secondaryTeam != null)
                secondaryTeamId = secondaryTeam.OldTeamId;

            var matches = _db.Match
                .Where(p => (p.Team1Id == TeamId || p.Team2Id == TeamId)
                && (p.Date >= dFrom.Date && p.Date <= dTo.Date)).ToList();

            // Fix matches so that "our" team is always Team1, to simplify later query
            var fixedMatches = FixTeamMatches(matches, TeamId);

            if (secondaryTeamId > 0)
            {
                // Get matches for secondaryTeamId
                var secondaryMatches = _db.Match
                .Where(p => (p.Team1Id == secondaryTeamId || p.Team2Id == secondaryTeamId)
                && (p.Date >= dFrom.Date && p.Date <= dTo.Date)).ToList();

                // Fix matches for secondaryMatches
                var fixedsecondaryMatches = FixTeamMatches(secondaryMatches, secondaryTeamId);

                // Add matches from secondaryTeamId to fixedMatches
                fixedMatches.AddRange(fixedsecondaryMatches);
            }


            // Group list by maps.
            var groupedbymaps = fixedMatches.GroupBy(k => k.Map).ToList();

            result = groupedbymaps.Select(n => new MapStatisticModel
            {
                Map = n.Key,
                TotalWins = n.Count(p => p.ResultT1 > p.ResultT2),
                TotalLosses = n.Count(p => p.ResultT2 > p.ResultT1),
                WinPercent = Math.Round(n.Count(p => p.ResultT1 > p.ResultT2) / (double)n.Count() * 100, 1),
                // *** Get Average Win rounds ***
                AverageWinRounds = Math.Round(n.Sum(p => p.ResultT1) / (double)n.Count(), 1),
                // *** Get Average Loss rounds ***
                AverageLossRounds = Math.Round(n.Sum(p => p.ResultT2) / (double)n.Count(), 1),
                // *** Get Average Win rounds when team has won ***
                AverageWinRoundsWhenWin = Math.Round(n.Where(p => p.ResultT1 > p.ResultT2) // Where team won
                    .Sum(p => p.ResultT1) / // Sum team rounds
                    (double)n.Count(p => p.ResultT1 > p.ResultT2), 1), // Divided total games we won
                // *** Get Average Loss rounds when team has won ***
                AverageLossRoundsWhenWin = Math.Round(n.Where(p => p.ResultT1 > p.ResultT2) // Where team won
                    .Sum(p => p.ResultT2) / // Sum opponents rounds
                    (double)n.Count(p => p.ResultT1 > p.ResultT2), 1), // Divided total games we won
                // *** Get Average Win rounds when team has lost ***
                AverageWinRoundsWhenLoss = Math.Round(n.Where(p => p.ResultT1 < p.ResultT2) // Where team lost
                    .Sum(p => p.ResultT1) / // Sum team rounds
                    (double)n.Count(p => p.ResultT1 < p.ResultT2), 1), // Divided total games we lost
                // *** Get Average Loss rounds when team has lost ***
                AverageLossRoundsWhenLoss = Math.Round(n.Where(p => p.ResultT1 < p.ResultT2) // Where team lost
                    .Sum(p => p.ResultT2) / // Sum opponents rounds
                    (double)n.Count(p => p.ResultT1 < p.ResultT2), 1), // Divided total games we lost
                DifficultyRating = Math.Round(n.Sum(p => p.Team2RankValue) / (double)n.Count(),2),
                DiffTitleGroupBy = GetDiffTitleGroupBy(n.ToList()),
                FullTeamRanking = GetFullTeamPercent(TeamId, n.ToList(), expectedLinup)
            }).OrderByDescending(n => n.WinPercent).ToList();

            return result;
        }

        private Tuple<double,string> GetFullTeamPercent(int TeamId, List<Match> Map, ExpectedLineUp expectedLinup)
        {
            Tuple<double, string> result = new Tuple<double, string>(0, "");
            double Accumulator = 0;
            double AvFTR = 0;

            // Calculate how many of expectedLinup played each match.
            foreach (var match in Map)
            {
                int FTRank = 0;
                if (match.Team1Id == TeamId && expectedLinup != null)
                {
                    if (expectedLinup.Players.Where(p => p.TeamID == TeamId && p.PlayerId == match.T1Player1Id).Any())
                        FTRank += 1;
                    if (expectedLinup.Players.Where(p => p.TeamID == TeamId && p.PlayerId == match.T1Player2Id).Any())
                        FTRank += 1;
                    if (expectedLinup.Players.Where(p => p.TeamID == TeamId && p.PlayerId == match.T1Player3Id).Any())
                        FTRank += 1;
                    if (expectedLinup.Players.Where(p => p.TeamID == TeamId && p.PlayerId == match.T1Player4Id).Any())
                        FTRank += 1;
                    if (expectedLinup.Players.Where(p => p.TeamID == TeamId && p.PlayerId == match.T1Player5Id).Any())
                        FTRank += 1;

                    // How many played this map
                    match.T1FTR = FTRank;

                    // Add to Accumulator for total average.
                    Accumulator += FTRank;
                }
            }

            var rankList = new List<Tuple<double, string>>();
            var FTRankList = Map.GroupBy(p => p.T1FTR).ToList();
            var returnSymbol = "&#010;";
            var Title = "";

            // Group by FTRating.
            foreach (var n in FTRankList)
            {
                var TotalWins = n.Count(p => p.ResultT1 > p.ResultT2);
                var TotalLosses = n.Count(p => p.ResultT2 > p.ResultT1);
                var WinPercent = Math.Round(n.Count(p => p.ResultT1 > p.ResultT2) / (double)n.Count() * 100, 1);
                var AverageWinRounds = Math.Round(n.Sum(p => p.ResultT1) / (double)n.Count(), 1);
                var AverageLossRounds = Math.Round(n.Sum(p => p.ResultT2) / (double)n.Count(), 1);
                rankList.Add(new Tuple<double, string>(n.Key, $"{TotalWins} / {TotalLosses} {WinPercent}% av.r: {AverageWinRounds} / {AverageLossRounds}"));
            }

            // Order the list by FTRating
            var orderedList = rankList.OrderByDescending(p => p.Item1).ToList();

            // Create the title "hover" list
            foreach (Tuple<double, string> tup in orderedList)
            {
                Title += string.IsNullOrEmpty(Title) ? " " : " " + returnSymbol + " ";
                Title += tup.Item1 + " - " + tup.Item2;
            }

            // Av. played
            if (Accumulator > 0)
                AvFTR = Accumulator / Map.Count;

            result = new Tuple<double, string>(AvFTR, Title);
            return result;
        }

        private string GetDiffTitleGroupBy(List<Match> Map)
        {
            var result = "";
            var diffList = new List<Tuple<double, string>>();
            var returnSymbol = "&#010;";

            // Group by Difficulty rating.
            var diffRankList = Map.GroupBy(p => p.Team2RankValue).ToList();
            foreach (var n in diffRankList)
            {
                var TotalWins = n.Count(p => p.ResultT1 > p.ResultT2);
                var TotalLosses = n.Count(p => p.ResultT2 > p.ResultT1);
                var WinPercent = Math.Round(n.Count(p => p.ResultT1 > p.ResultT2) / (double)n.Count() * 100, 1);
                var AverageWinRounds = Math.Round(n.Sum(p => p.ResultT1) / (double)n.Count(), 1);
                var AverageLossRounds = Math.Round(n.Sum(p => p.ResultT2) / (double)n.Count(), 1);
                diffList.Add(new Tuple<double, string>(n.Key, $"{TotalWins} / {TotalLosses} {WinPercent}% av.r: {AverageWinRounds} / {AverageLossRounds}"));
            }

            // Order the list by difficulty
            var orderedList = diffList.OrderByDescending(p => p.Item1).ToList();

            // Create the title "hover" list
            foreach (Tuple<double,string> tup in orderedList)
            {
                result += string.IsNullOrEmpty(result) ? " " : " " + returnSymbol + " ";
                result += tup.Item1 == 1? tup.Item1 + ".0" + " - " + tup.Item2 : tup.Item1 + " - " + tup.Item2;
            }

            return result;
        }

        /// <summary>
        /// Take matches query and set "our" (TeamId) team always as Team1
        /// </summary>
        /// <param name="matches"></param>
        /// <param name="TeamId"></param>
        /// <returns></returns>
        public List<Match> FixTeamMatches(List<Match> matches, int TeamId)
        {
            var result = matches.Select(n => new Match
            {
                Id = n.Id,
                MatchId = n.MatchId,
                Date = n.Date,
                Map = n.Map,
                Event = n.Event,
                ResultT1 = n.Team1Id == TeamId ? n.ResultT1 : n.ResultT2, // 
                ResultT2 = n.Team1Id == TeamId ? n.ResultT2 : n.ResultT1,
                FirstRound1HWinTeamId = n.FirstRound1HWinTeamId,
                FirstRound1HWinCtTerr = n.FirstRound1HWinCtTerr,
                FirstRound2HWinTeamId = n.FirstRound2HWinTeamId,
                FirstRound2HWinCtTerr = n.FirstRound2HWinCtTerr,
                Team1Id = n.Team1Id == TeamId ? n.Team1Id : n.Team2Id,
                Team1RankValue = n.Team1Id == TeamId ? n.Team1RankValue : n.Team2RankValue,
                T1Player1Id = n.Team1Id == TeamId ? n.T1Player1Id : n.T2Player1Id,
                T1Player2Id = n.Team1Id == TeamId ? n.T1Player2Id : n.T2Player2Id,
                T1Player3Id = n.Team1Id == TeamId ? n.T1Player3Id : n.T2Player3Id,
                T1Player4Id = n.Team1Id == TeamId ? n.T1Player4Id : n.T2Player4Id,
                T1Player5Id = n.Team1Id == TeamId ? n.T1Player5Id : n.T2Player5Id,
                Team2Id = n.Team1Id == TeamId ? n.Team1Id : n.Team2Id,
                Team2RankValue = n.Team1Id == TeamId ? n.Team2RankValue : n.Team1RankValue,
                T2Player1Id = n.Team1Id == TeamId ? n.T2Player1Id : n.T1Player1Id,
                T2Player2Id = n.Team1Id == TeamId ? n.T2Player2Id : n.T1Player2Id,
                T2Player3Id = n.Team1Id == TeamId ? n.T2Player3Id : n.T1Player3Id,
                T2Player4Id = n.Team1Id == TeamId ? n.T2Player4Id : n.T1Player4Id,
                T2Player5Id = n.Team1Id == TeamId ? n.T2Player5Id : n.T1Player5Id,
            }).ToList();

            return result;
        }

        public void GenerateSuggestedMaps(ref CompareStatisticModel model)
        {
            if (model.Teams.Count == 2)
            {
                foreach (var period in model.Teams[0].TeamStatistics)
                {
                    var PeriodName = period.Period;
                    foreach (var map in period.Maps)
                    {
                        var resultMap = FindMatchingMaps(map, PeriodName, model.Teams[1]);
                        if (resultMap != null)
                            period.SuggestedMaps.Add(resultMap);
                    }

                    // Order by highest rank
                    if (period.SuggestedMaps != null && period.SuggestedMaps.Count > 1)
                        period.SuggestedMaps = period.SuggestedMaps.OrderByDescending(p => p.SuggestedRank).ToList();
                }


                foreach (var period in model.Teams[1].TeamStatistics)
                {
                    var PeriodName = period.Period;
                    foreach (var map in period.Maps)
                    {
                        var resultMap = FindMatchingMaps(map, PeriodName, model.Teams[0]);
                        if (resultMap != null)
                            period.SuggestedMaps.Add(resultMap);
                    }

                    // Order by highest rank
                    if (period.SuggestedMaps != null && period.SuggestedMaps.Count > 1)
                        period.SuggestedMaps = period.SuggestedMaps.OrderByDescending(p => p.SuggestedRank).ToList();
                }
            }
        }

        public MapStatisticModel FindMatchingMaps(MapStatisticModel MapA, string PeriodName, TeamStatisticPeriodModel TeamB)
        {
            bool MapFound = false;

            foreach (var period in TeamB.TeamStatistics)
            {
                // Find matching period
                if (PeriodName == period.Period)
                {
                    // Try to find matching map
                    foreach (var MapB in period.Maps)
                    {
                        if(MapA.Map == MapB.Map)
                        {
                            MapFound = true;
                            // If found compare maps
                            return CompareStats(MapA, MapB);
                        }
                    }
                    if (MapFound == false)
                    {
                        // Compare with empty map
                        return CompareStats(MapA, new MapStatisticModel());
                    }
                }
            }

            return null;
        }

        private MapStatisticModel CompareStats(MapStatisticModel MapA, MapStatisticModel MapB)
        {
            MapStatisticModel result = null;
            if (MapB.DifficultyRating == 0)
                MapB.DifficultyRating = 0.5;
            var valuePoint = (int)Math.Floor((MapA.WinPercent - MapB.WinPercent) / 20.0);

            // if winPercent difference is less than 40%, we need MapA winpercent atleast 50%
            if (valuePoint < 2 && MapA.WinPercent <= 50)
                valuePoint = 0;
            else if (valuePoint >= 5 && (MapA.TotalWins + MapA.TotalLosses) <= 2) // If extra few games, lower by 2
                valuePoint -= 2;

            var diffPoint = (int)Math.Floor(MapA.DifficultyRating - MapB.DifficultyRating);

            if (valuePoint > 0)
            {
                result = new MapStatisticModel();

                result.Map = MapA.Map;
                result.SuggestedRank = valuePoint + diffPoint;
                result.WinPercent = Math.Round(MapA.WinPercent - MapB.WinPercent, 1);
                result.DifficultyRating = Math.Round(MapA.DifficultyRating - MapB.DifficultyRating, 1);
            }

            return result;
        }

        #endregion

        #region RANKING

        public List<RankViewModel> GetRankingList(Guid listID)
        {
            var result = (from rank in _db.Rank
                where rank.RankingListId == listID
                select new RankViewModel
                {
                    RankPosition = rank.RankPosition,
                    TeamId = rank.TeamId,
                    TeamName = _db.Team.FirstOrDefault(k => k.TeamId == rank.TeamId).TeamName,
                    Points = rank.Points
                }).ToList();

            return result;
        }

        #endregion
    }

    public enum PeriodEnum
    {
        ThreeMonths = 3,
        SixMonths = 6,
        Year = 12
    }
}