using VirtualFlowersMVC.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualFlowers;
using System.Threading.Tasks;
using MemoryCache;
using Newtonsoft.Json;

namespace VirtualFlowersMVC.Data
{
    public class dataWorker
    {
        private DatabaseContext _db;
        private Program _program = new Program();

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
                           }).OrderByDescending(p => p.MatchId).ToList();

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
            var result = _db.Team.OrderBy(p => p.TeamName).ToList();

            return result;
        }

        public Team GetTeamDetails(int? id)
        {
            var result = _db.Team.Where(p => p.TeamId == id).FirstOrDefault();

            return result;
        }

        public string GetTeamName(int id)
        {
            var result = "";

            result = _db.Team.FirstOrDefault(p => p.TeamId == id)?.TeamName;

            return result ?? "";
        }

        public int GetSecondaryTeamId(int TeamId)
        {
            int secondaryTeamId = 0;
            var secondaryTeam = _db.TransferHistory.Where(p => p.NewTeamId == TeamId).OrderByDescending(k => k.TransferDate).FirstOrDefault();
            if (secondaryTeam != null)
                secondaryTeamId = secondaryTeam.OldTeamId;
            return secondaryTeamId;
        }

        #endregion

        #region COMPARE

        public async Task<TeamStatisticPeriodModel> GetTeamPeriodStatistics(int TeamId, List<string> PeriodSelection, ExpectedLineUp expectedLinup, int secondaryTeamId, bool NoCache, int MinFullTeamRanking, string teamRank, string logo)
        {
            var result = new TeamStatisticPeriodModel();
            result.TeamId = TeamId;
            result.TeamName = GetTeamName(TeamId);
            result.TeamDifficultyRating = _program.GetRankingValueForTeam(TeamId, DateTime.Now);
            result.TeamRank = teamRank;
            result.Logo = logo;
            foreach (var period in PeriodSelection)
            {
                await Task.Run(() => result.TeamStatistics.Add(GetTeamPeriodStatistics(TeamId, (PeriodEnum)int.Parse(period), expectedLinup, secondaryTeamId, NoCache, MinFullTeamRanking))).ConfigureAwait(false);
            }

            return result;
        }

        public TeamStatisticModel GetTeamPeriodStatistics(int TeamId, PeriodEnum period, ExpectedLineUp expectedLinup, int secondaryTeamId, bool NoCache, int MinFullTeamRanking)
        {
            var result = new TeamStatisticModel();
            var dTo = DateTime.Now;
            var dFrom = new DateTime();
            var mapList = new List<string> { "nuke", "overpass", "cobblestone", "train", "inferno", "mirage", "cache" };

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

            result.Maps = GetMapStatistics(TeamId, dFrom, dTo, expectedLinup, secondaryTeamId, NoCache, MinFullTeamRanking);

            foreach (var map in mapList)
            {
                if (!result.Maps.Where(p => p.Map.ToLower() == map).Any())
                    result.Maps.Add(new MapStatisticModel
                    {
                        Map = map,
                        FirstRound1HWinPercent = new Tuple<double, string>(0, ""),
                        FirstRound2HWinPercent = new Tuple<double, string>(0, "")
                    });
            }

            return result;
        }

        public List<MapStatisticModel> GetMapStatistics(int TeamId, DateTime dFrom, DateTime dTo, ExpectedLineUp expectedLinup, int secondaryTeamId, bool NoCache, int MinFullTeamRanking)
        {
            var result = new List<MapStatisticModel>();
            List<Match> fixedMatches = null;

            // Create Cachekey from parameters
            var CACHEKEY = "cacheKey:TeamId=" + TeamId + "-DateFrom=" + dFrom.Date.ToString() + "-dTo=" + dTo.Date.ToString();

            // If we have object in cache, return it
            if (NoCache && Cache.Exists(CACHEKEY))
                fixedMatches = (List<Match>)Cache.Get(CACHEKEY);
            else
            {
                var matches = _db.Match
                    .Where(p => (p.Team1Id == TeamId || p.Team2Id == TeamId)
                    && (p.Date >= dFrom.Date && p.Date <= dTo.Date)).ToList();

                // Fix matches so that "our" team is always Team1, to simplify later query
                fixedMatches = FixTeamMatches(matches, TeamId);

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
            }
            
            if (!string.IsNullOrEmpty(CACHEKEY))
            {
                if (!Cache.Exists(CACHEKEY))
                {
                    int storeTime = 1000 * 3600 * 24 * 2; // store 2 days
                    Cache.Store(CACHEKEY, fixedMatches, storeTime);
                }
                else
                    Cache.Update(CACHEKEY, fixedMatches);
            }

            result = CalculateResults(TeamId, fixedMatches, expectedLinup, secondaryTeamId, MinFullTeamRanking);
            
            return result;
        }

        public List<MapStatisticModel> CalculateResults(int TeamId, List<Match> fixedMatches, ExpectedLineUp expectedLinup, int secondaryTeamId, int MinFullTeamRanking)
        {
            if (MinFullTeamRanking > 0)
            {
                if (MinFullTeamRanking == 5)
                {
                    fixedMatches = fixedMatches.Where(p => expectedLinup.Players.Where(x => p.T1Player1Id == x.PlayerId).Any()
                    && expectedLinup.Players.Where(x => p.T1Player2Id == x.PlayerId).Any()
                    && expectedLinup.Players.Where(x => p.T1Player3Id == x.PlayerId).Any()
                    && expectedLinup.Players.Where(x => p.T1Player4Id == x.PlayerId).Any()
                    && expectedLinup.Players.Where(x => p.T1Player5Id == x.PlayerId).Any()).ToList();
                }
                else if(MinFullTeamRanking == 4)
                {
                        fixedMatches = fixedMatches.Where(p => (expectedLinup.Players.Where(x => p.T1Player1Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player2Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player3Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player4Id == x.PlayerId).Any())
                        ||
                        (expectedLinup.Players.Where(x => p.T1Player1Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player2Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player3Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player5Id == x.PlayerId).Any())
                        ||
                        (expectedLinup.Players.Where(x => p.T1Player1Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player2Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player4Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player5Id == x.PlayerId).Any())
                        ||
                        (expectedLinup.Players.Where(x => p.T1Player1Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player3Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player4Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player5Id == x.PlayerId).Any())
                        ||
                        (expectedLinup.Players.Where(x => p.T1Player2Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player3Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player4Id == x.PlayerId).Any()
                        && expectedLinup.Players.Where(x => p.T1Player5Id == x.PlayerId).Any())).ToList();
                }
            }

            // Group list by maps.
            var groupedbymaps = fixedMatches.GroupBy(k => k.Map, StringComparer.InvariantCultureIgnoreCase).ToList();

            var result = groupedbymaps.Select(n => new MapStatisticModel
            {
                Map = n.Key,
                TitleMapMatches = GetTitleMapMatches(fixedMatches.Where(p => p.Map == n.Key).OrderByDescending(p => p.Date).ToList()),
                TotalMatches = n.Count(),
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
                DifficultyRating = Math.Round(n.Sum(p => p.Team2RankValue) / (double)n.Count(), 2),
                DiffTitleGroupBy = GetDiffTitleGroupBy(n.ToList()),
                FullTeamRanking = GetFullTeamPercent(TeamId, n.ToList(), expectedLinup, secondaryTeamId),
                FullTeamGroupBy = GetFullTeamTitle(TeamId, n.ToList(), expectedLinup, secondaryTeamId),
                FirstRound1HWinPercent = GetFirstRoundStats(TeamId, n.ToList(), true),
                FirstRound2HWinPercent = GetFirstRoundStats(TeamId, n.ToList(), false),
                BombExplosions = n.Sum(p => p.BombExplosions), //Math.Round(n.Sum(p => p.BombExplosions) / (double)n.Count(), 1),
                BombExplosionsWin = n.Count(p => p.BombExplosions > 4.5), //Math.Round((n.Count(p => p.BombExplosions > 4.5) / (double)n.Count()) * 100, 1),
                BombDefuses = n.Sum(p => p.BombDefuses), //Math.Round(n.Sum(p => p.BombDefuses) / (double)n.Count(), 1),
                BombDefusesWin = n.Count(p => p.BombDefuses > 3.5), //Math.Round((n.Count(p => p.BombDefuses > 3.5) / (double)n.Count()) * 100, 1),
                TimeOut = n.Sum(p => p.TimeOut), //Math.Round(n.Sum(p => p.TimeOut) / (double)n.Count(), 1),
                TimeOutWin = n.Count(p => p.TimeOut > 0.5), //Math.Round((n.Count(p => p.TimeOut > 0.5) / (double)n.Count()) * 100, 1),
                GrenadeKill = n.Sum(p => p.GrenadeKill), //Math.Round(n.Sum(p => p.GrenadeKill) / (double)n.Count(), 1),
                GrenadeKillWin = n.Count(p => p.GrenadeKill > 0.5), //Math.Round((n.Count(p => p.GrenadeKill > 0.5) / (double)n.Count()) * 100, 1),
                MolotovKill = n.Sum(p => p.MolotovKill), //Math.Round(n.Sum(p => p.MolotovKill) / (double)n.Count(), 1),
                MolotovKillWin = n.Count(p => p.MolotovKill > 0.5), //Math.Round((n.Count(p => p.MolotovKill > 0.5) / (double)n.Count()) * 100, 1),
                ZuesKill = n.Sum(p => p.ZuesKill), //Math.Round(n.Sum(p => p.ZuesKill) / (double)n.Count(), 1),
                ZuesKillWin = n.Count(p => p.ZuesKill > 0.5), //Math.Round((n.Count(p => p.ZuesKill > 0.5) / (double)n.Count()) * 100, 1),
                KnifeKill = n.Sum(p => p.KnifeKill), //Math.Round(n.Sum(p => p.KnifeKill) / (double)n.Count(), 1),
                KnifeKillWin = n.Count(p => p.KnifeKill > 0.5) //Math.Round((n.Count(p => p.KnifeKill > 0.5) / (double)n.Count()) * 100, 1)
            }).OrderByDescending(n => n.WinPercent).ToList();

            return result;
        }

        private Tuple<double, string> GetFirstRoundStats(int TeamId, List<Match> Map, bool bFirstHalf)
        {
            Tuple<double, string> result = new Tuple<double, string>(0, "");

            double WinPercent = 0.0;
            int WinCt = 0;
            int LossCt = 0;
            int WinTerr = 0;
            int LossTerr = 0;
            if (bFirstHalf)
            {
                WinPercent = Math.Round(Map.Count(p => p.FirstRound1HWinTeamId == TeamId) / (double)Map.Count() * 100, 0);
                WinCt = Map.Count(p => p.FirstRound1HWinCt == true && p.FirstRound1HWinTeamId == TeamId);
                LossCt = Map.Count(p => p.FirstRound1HWinCt == false && p.FirstRound1HWinTeamId != TeamId);
                WinTerr = Map.Count(p => p.FirstRound1HWinTerr == true && p.FirstRound1HWinTeamId == TeamId);
                LossTerr = Map.Count(p => p.FirstRound1HWinTerr == false && p.FirstRound1HWinTeamId != TeamId);
            }
            else
            {
                WinPercent = Math.Round(Map.Count(p => p.FirstRound2HWinTeamId == TeamId) / (double)Map.Count() * 100, 0);
                WinCt = Map.Count(p => p.FirstRound2HWinCT == true && p.FirstRound2HWinTeamId == TeamId);
                LossCt = Map.Count(p => p.FirstRound2HWinCT == false && p.FirstRound2HWinTeamId != TeamId);
                WinTerr = Map.Count(p => p.FirstRound2HWinTerr == true && p.FirstRound2HWinTeamId == TeamId);
                LossTerr = Map.Count(p => p.FirstRound2HWinTerr == false && p.FirstRound2HWinTeamId != TeamId);
            }

            var returnSymbol = "<br>"; // "&#010;";
            var Title = $"<b>CT:</b> {WinCt} / {LossCt}{returnSymbol}<b>Terr:</b> {WinTerr} / {LossTerr}";
            
            result = new Tuple<double, string>(WinPercent, Title);
            return result;
        }


        private double GetFullTeamPercent(int TeamId, List<Match> Map, ExpectedLineUp expectedLinup, int secondaryTeamId)
        {
            double Accumulator = 0;
            double AvFTR = 0;

            // Calculate how many of expectedLinup played each match.
            foreach (var match in Map)
            {
                int FTRank = 0;
                if ((match.Team1Id == TeamId || match.Team1Id == secondaryTeamId) && expectedLinup != null)
                {
                    if (secondaryTeamId > 0)
                    {
                        if (expectedLinup.Players.Where(p => (p.TeamID == TeamId || p.TeamID == secondaryTeamId) && p.PlayerId == match.T1Player1Id).Any())
                            FTRank += 1;
                        if (expectedLinup.Players.Where(p => (p.TeamID == TeamId || p.TeamID == secondaryTeamId) && p.PlayerId == match.T1Player2Id).Any())
                            FTRank += 1;
                        if (expectedLinup.Players.Where(p => (p.TeamID == TeamId || p.TeamID == secondaryTeamId) && p.PlayerId == match.T1Player3Id).Any())
                            FTRank += 1;
                        if (expectedLinup.Players.Where(p => (p.TeamID == TeamId || p.TeamID == secondaryTeamId) && p.PlayerId == match.T1Player4Id).Any())
                            FTRank += 1;
                        if (expectedLinup.Players.Where(p => (p.TeamID == TeamId || p.TeamID == secondaryTeamId) && p.PlayerId == match.T1Player5Id).Any())
                            FTRank += 1;
                    }
                    else
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
                    }

                    // How many played this map
                    match.T1FTR = FTRank;

                    // Add to Accumulator for total average.
                    Accumulator += FTRank;
                }
            }

            // Av. played
            if (Accumulator > 0)
                AvFTR = Accumulator / Map.Count;

            return AvFTR;
        }

        private string GetFullTeamTitle(int TeamId, List<Match> Map, ExpectedLineUp expectedLinup, int secondaryTeamId)
        { 
            var rankList = new List<Tuple<double, string>>();
            var FTRankList = Map.GroupBy(p => p.T1FTR).ToList();
            var returnSymbol = "<br>";//"&#010;";
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
                Title += string.IsNullOrEmpty(Title) ? "<b>Full Team Rating:</b><br> " : " " + returnSymbol + " ";
                Title += tup.Item1 + " - " + tup.Item2;
            }
            
            return Title;
        }

        private string GetDiffTitleGroupBy(List<Match> Map)
        {
            var result = "";
            var diffList = new List<Tuple<double, string>>();
            var returnSymbol = "<br>";//"&#010;";

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
                result += string.IsNullOrEmpty(result) ? "<b>Difficulty Rating:</b><br> " : " " + returnSymbol + " ";
                result += tup.Item1 == 1? tup.Item1 + ".0" + " - " + tup.Item2 : tup.Item1 + " - " + tup.Item2;
            }

            return result;
        }

        private string GetTitleMapMatches(List<Match> MapMatches)
        {
            var result = "";
            var returnSymbol = "<br>";//"&#010;";
            var allTeams = _db.Team.Where(p => p.TeamId > 0).ToList();
            
            foreach (var match in MapMatches)
            {
                var opponentName = "";
                if(allTeams.Any(p => p.TeamId == match.Team2Id))
                    opponentName = allTeams.FirstOrDefault(p => p.TeamId == match.Team2Id).TeamName;
                result += string.IsNullOrEmpty(result) ? "<b>" + match.Map + " games:</b><br> " : " " + returnSymbol + " ";
                var colorspan = match.ResultT1 > match.ResultT2 ? "<span style='color:green'>" : "<span style='color:red'>";
                result += match.Date.ToString("dd.MM.yyyy") + " - " + colorspan + match.ResultT1 + " - " + match.ResultT2 + "</span> " + opponentName + " (" + match.Team2RankValue + ")";
            }
            
            return result;
        }

        public HeadToHeadStatisticModel GetHeadToHeadMatches(int Team1ID, int Team2ID)
        {
            HeadToHeadStatisticModel result = new HeadToHeadStatisticModel();
            try
            {
                var returnSymbol = "<br>";//"&#010;";
                string Team1Name = "", Team2Name = "";
                DateTime dDate = DateTime.Now.AddMonths(-6).Date;
                // Get matches
                var matches = _db.Match.Where(p => (p.Team1Id == Team1ID || p.Team1Id == Team2ID) && (p.Team2Id == Team1ID || p.Team2Id == Team2ID) && p.Date > dDate).OrderByDescending(p => p.Date).ToList();
                var fixedMatches = FixTeamMatches(matches, Team1ID);
                result.Team1Win = fixedMatches.Count(p => p.ResultT1 > p.ResultT2);
                result.Team2Win = fixedMatches.Count(p => p.ResultT1 < p.ResultT2);

                // Get names
                var allTeams = _db.Team.Where(p => p.TeamId > 0).ToList();
                if (allTeams.Any(p => p.TeamId == Team1ID))
                    Team1Name = allTeams.FirstOrDefault(p => p.TeamId == Team1ID).TeamName;
                if (allTeams.Any(p => p.TeamId == Team2ID))
                    Team2Name = allTeams.FirstOrDefault(p => p.TeamId == Team2ID).TeamName;

                foreach (var match in fixedMatches)
                {
                    var T1colorspan = match.ResultT1 > match.ResultT2 ? "<span style='color:green'>" : "<span style='color:red'>";
                    var T2colorspan = match.ResultT1 < match.ResultT2 ? "<span style='color:green'>" : "<span style='color:red'>";
                    result.Title += string.IsNullOrEmpty(result.Title) ? "<b>H2H games (last 6 months) " + returnSymbol + Team1Name + " - " + Team2Name + ":</b><br> " : " " + returnSymbol + " ";
                    result.Title += match.Date.ToString("dd.MM") + " <b>" + T1colorspan + match.ResultT1 + "</span> - " + T2colorspan + match.ResultT2 + "</span></b> (" + match.Map + ")";
                }
            }
            catch(Exception ex)
            {
                // do nothing
            }

            return result;
        }

        public TeamFormModel GetTeamForm(int TeamId)
        {
            TeamFormModel result = new TeamFormModel();
            result.FormUnits = new List<FormUnitModel>();
            try
            {
                var returnSymbol = "<br>";//"&#010;";
                string opponentName = "";
                DateTime dDate = DateTime.Now.AddMonths(-6).Date;
                var matches = _db.Match.Where(p => (p.Team1Id == TeamId || p.Team2Id == TeamId) && p.Date > dDate).OrderByDescending(p => p.Date).Take(10).ToList();
                var fixedMatches = FixTeamMatches(matches, TeamId);
                var ids = fixedMatches.Select(k => k.Team2Id).ToList();
                // Get names
                var allTeams = _db.Team.Where(p => ids.Contains(p.TeamId)).ToList();

                foreach (var match in fixedMatches)
                {
                    // Find name of team
                    if (allTeams.Any(p => p.TeamId == match.Team2Id))
                        opponentName = allTeams.FirstOrDefault(p => p.TeamId == match.Team2Id).TeamName;
                    bool bWin = match.ResultT1 > match.ResultT2;
                    result.FormUnits.Add(new FormUnitModel() { Letter = bWin ? "W" : "L", Color = bWin ? "#5cb85c" : "#d9534f" });
                    result.TitleHtml += string.IsNullOrEmpty(result.TitleHtml) ? "<b>Most recent games:</b><br> " : " " + returnSymbol + " ";
                    var colorspan = bWin ? "<span style='color:green'>" : "<span style='color:red'>";
                    result.TitleHtml += match.Date.ToString("dd.MM") + " - " + colorspan + match.ResultT1 + " - " + match.ResultT2 + "</span> <b>" + opponentName + "</b>" + " (" + match.Team2RankValue + ") <i>" + GetShortMap(match.Map) + "</i>";
                }
            }
            catch(Exception ex)
            {
                // do nothing
            }
            
            return result;
        }

        private string GetShortMap(string Map)
        {
            switch (Map.ToLower())
            {
                case "cobblestone":
                    return "cbl";
                case "nuke":
                    return "nuke";
                case "inferno":
                    return "inf";
                case "overpass":
                    return "ovp";
                case "train":
                    return "trn";
                case "mirage":
                    return "mrg";
                case "cache":
                    return "cch";
                default:
                    return "";
            }
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
                FirstRound1HWinCt = n.FirstRound1HWinCt,
                FirstRound1HWinTerr =  n.FirstRound1HWinTerr,
                FirstRound2HWinTeamId = n.FirstRound2HWinTeamId,
                FirstRound2HWinTerr = n.FirstRound2HWinTerr,
                FirstRound2HWinCT = n.FirstRound2HWinCT,
                Team1Id = n.Team1Id == TeamId ? n.Team1Id : n.Team2Id,
                Team1RankValue = n.Team1Id == TeamId ? n.Team1RankValue : n.Team2RankValue,
                T1Player1Id = n.Team1Id == TeamId ? n.T1Player1Id : n.T2Player1Id,
                T1Player2Id = n.Team1Id == TeamId ? n.T1Player2Id : n.T2Player2Id,
                T1Player3Id = n.Team1Id == TeamId ? n.T1Player3Id : n.T2Player3Id,
                T1Player4Id = n.Team1Id == TeamId ? n.T1Player4Id : n.T2Player4Id,
                T1Player5Id = n.Team1Id == TeamId ? n.T1Player5Id : n.T2Player5Id,
                Team2Id = n.Team1Id == TeamId ? n.Team2Id : n.Team1Id,
                Team2RankValue = n.Team1Id == TeamId ? n.Team2RankValue : n.Team1RankValue,
                T2Player1Id = n.Team1Id == TeamId ? n.T2Player1Id : n.T1Player1Id,
                T2Player2Id = n.Team1Id == TeamId ? n.T2Player2Id : n.T1Player2Id,
                T2Player3Id = n.Team1Id == TeamId ? n.T2Player3Id : n.T1Player3Id,
                T2Player4Id = n.Team1Id == TeamId ? n.T2Player4Id : n.T1Player4Id,
                T2Player5Id = n.Team1Id == TeamId ? n.T2Player5Id : n.T1Player5Id,
                BombExplosions = n.BombExplosions,
                BombDefuses = n.BombDefuses,
                TimeOut = n.TimeOut,
                GrenadeKill = n.GrenadeKill,
                MolotovKill = n.MolotovKill,
                ZuesKill = n.ZuesKill,
                KnifeKill = n.KnifeKill
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
                        FindMatchingMaps(map, PeriodName, model.Teams[1]);
                    }
                }


                foreach (var period in model.Teams[1].TeamStatistics)
                {
                    var PeriodName = period.Period;
                    foreach (var map in period.Maps)
                    {
                        FindMatchingMaps(map, PeriodName, model.Teams[0]);
                    }
                }
            }
        }

        public bool FindMatchingMaps(MapStatisticModel MapA, string PeriodName, TeamStatisticPeriodModel TeamB)
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
                        if(MapA.Map.ToLower() == MapB.Map.ToLower())
                        {
                            MapFound = true;
                            // If found compare maps
                            MapA.SuggestedMap = CompareStats(MapA, MapB);
                        }
                    }
                    if (MapFound == false)
                    {
                        // Compare with empty map
                        MapA.SuggestedMap = CompareStats(MapA, new MapStatisticModel());
                    }
                }
            }

            return true;
        }

        private SuggestedMapModel CompareStats(MapStatisticModel MapA, MapStatisticModel MapB)
        {
            SuggestedMapModel result = null;
            double TFRPoints = 0.0;

            #region SPECIAL CASES
            // Empty record, default 0.5 
            if (MapB.DifficultyRating == 0)
                MapB.DifficultyRating = 0.5;
            var WinPercentA = SetWinPercentSpecialCases(MapA);
            var WinPercentB = SetWinPercentSpecialCases(MapB);
            #endregion

            int WinLossRecord = (MapA.TotalWins - MapA.TotalLosses) - (MapB.TotalWins - MapB.TotalLosses);
            var valuePoint = Math.Round((WinPercentA - WinPercentB) / 10.0);
            
            var diffPoint = (int)Math.Floor((MapA.DifficultyRating - MapB.DifficultyRating) * 10);
            TFRPoints = MapA.FullTeamRanking -  MapB.FullTeamRanking;
            
            if (WinLossRecord > 0 && valuePoint > 0 && MapA.WinPercent >= 50)
            {
                result = new SuggestedMapModel();

                result.Map = MapA.Map;
                result.SuggestedRank = Math.Ceiling((((WinLossRecord + valuePoint) / 2.0) + diffPoint ) * (MapA.FullTeamRanking * 0.2));
                result.WinLossRecord = WinLossRecord;
                result.WinPercent = Math.Round(WinPercentA - WinPercentB, 1);
                result.DifficultyRating = Math.Round((MapA.DifficultyRating - MapB.DifficultyRating) * 10, 1);
                result.TFRating = Math.Round(TFRPoints, 1);
                result.SuggestiveMapClass = GetClassBySuggestedRank(result.SuggestedRank);
            }

            return result;
        }

        private string GetClassBySuggestedRank(double suggestedRank)
        {
            string result = "";

            if (suggestedRank < 2)
            {
                result = "fi-die-one";
            }
            else if (suggestedRank == 2)
            {
                result = "fi-die-two";
            }
            else if (suggestedRank == 3)
            {
                result = "fi-die-three";
            }
            else if (suggestedRank == 4)
            {
                result = "fi-die-four";
            }
            else if (suggestedRank == 5)
            {
                result = "fi-die-five";
            }
            else
            {
                result = "fi-die-six";
            }

            return result;
        }

        private double SetWinPercentSpecialCases(MapStatisticModel Map)
        {
            // If extra few matches, set winpercent manually
            if (Map.TotalWins + Map.TotalLosses < 2)
            {
                if (Map.TotalWins + Map.TotalLosses == 0) // 50% if no games
                    return 50;
                else if (Map.TotalWins == 1) // 1-0 record 67%
                    return 67;
                else // 0-1 record 33%
                    return 33;
            }
            else
                return Map.WinPercent;
        }

        public int AddScrapedMatch(ScrapedMatches scrapedMatch, int MinFTR)
        {
            var model = new ScrapedMatches();
            bool bOldRecord = false;
            if (_db.ScrapedMatches.Any(p => p.MatchId == scrapedMatch.MatchId))
            {
                bOldRecord = true;
                model = _db.ScrapedMatches.Single(p => p.MatchId == scrapedMatch.MatchId);
                scrapedMatch.Id = model.Id;
                model.MatchId = scrapedMatch.MatchId;
                model.MatchUrl = scrapedMatch.MatchUrl;
                model.Name = scrapedMatch.Name;
                model.SportName = scrapedMatch.SportName;
                model.Start = scrapedMatch.Start;
                model.Team1Id = scrapedMatch.Team1Id;
                model.Team1Name = scrapedMatch.Team1Name;
                model.Team1Logo = scrapedMatch.Team1Logo;
                model.Team2Id = scrapedMatch.Team2Id;
                model.Team2Name = scrapedMatch.Team2Name;
                model.Team2Logo = scrapedMatch.Team2Logo;
                model.Json = MinFTR==0 || MinFTR ==-1? scrapedMatch.Json : model.Json;
                model.Json4MinFTR = MinFTR == 4 || MinFTR == -1 ? scrapedMatch.Json4MinFTR : model.Json4MinFTR;
                model.Json5MinFTR = MinFTR == 5 || MinFTR == -1 ? scrapedMatch.Json5MinFTR : model.Json5MinFTR;
            }
            else
                _db.ScrapedMatches.Add(scrapedMatch);
            _db.SaveChanges();


            if(bOldRecord && MinFTR == -1 && scrapedMatch.Id > 0)
            {
                int storeTime = 1000 * 3600 * 24 * 2; // store 2 days
                // Remove all cachekeys, since we got new data
                var CACHEKEY = $"cacheKey:MatchId={scrapedMatch.Id}-MinFTR=0";
                if (Cache.Exists(CACHEKEY))
                {
                    Cache.Update(CACHEKEY, JsonConvert.DeserializeObject<CompareStatisticModel>(model.Json));
                }
                else
                    Cache.Store(CACHEKEY, JsonConvert.DeserializeObject<CompareStatisticModel>(model.Json), storeTime);
                CACHEKEY = $"cacheKey:MatchId={scrapedMatch.Id}-MinFTR=4";
                if (Cache.Exists(CACHEKEY))
                {
                    Cache.Update(CACHEKEY, JsonConvert.DeserializeObject<CompareStatisticModel>(model.Json4MinFTR));
                }
                else
                    Cache.Store(CACHEKEY, JsonConvert.DeserializeObject<CompareStatisticModel>(model.Json4MinFTR), storeTime);
                CACHEKEY = $"cacheKey:MatchId={scrapedMatch.Id}-MinFTR=5";
                if (Cache.Exists(CACHEKEY))
                {
                    Cache.Update(CACHEKEY, JsonConvert.DeserializeObject<CompareStatisticModel>(model.Json5MinFTR));
                }
                else
                    Cache.Store(CACHEKEY, JsonConvert.DeserializeObject<CompareStatisticModel>(model.Json5MinFTR), storeTime);
            }

            return scrapedMatch.Id;
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

        #region SCRAPED MATCHES

        public List<ScrapedMatches> GetScrapedMatches(int hours)
        {
            List<ScrapedMatches> result = new List<ScrapedMatches>();
            DateTime dDate = DateTime.Now.AddHours(hours);
            result = _db.ScrapedMatches.Where(p => p.Start > dDate).OrderBy(p => p.Start).ToList();

            return result;
        }

        public ScrapedMatches GetScrapedMatch(int id)
        {
            ScrapedMatches result = new ScrapedMatches();
            result = _db.ScrapedMatches.Single(p => p.Id == id);

            return result;
        }

        public bool IsMatchScraped(ref List<ScrapedMatches> allScrapedMatches, int matchid)
        {
            return allScrapedMatches.Any(p => p.MatchId == matchid);
        }

        public List<ScrapedMatches> GetAllScrapedMatches()
        {
            DateTime dDate = DateTime.Now.Date;
            if (_db.ScrapedMatches.Where(p => p.Start > dDate).Any())
                return _db.ScrapedMatches?.Where(p => p.Start > dDate).ToList();
            else
                return new List<ScrapedMatches>();
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