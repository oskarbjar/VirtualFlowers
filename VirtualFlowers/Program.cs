using System;
using System.Linq;
using HtmlAgilityPack;
using Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Match = Models.Match;

namespace VirtualFlowers
{
    public class Program
    {
        static readonly HtmlWeb HWeb = new HtmlAgilityPack.HtmlWeb();
        private static readonly DatabaseContext db = new DatabaseContext();
        private static bool quitTeamDetails;
        private static bool quit;

        static void Main(string[] args)
        {
            GetTeamDetails();

            //Import url from mvc project
            //GetRankingList("http://www.hltv.org/ranking/teams/2016/january/5/");
        }

        private static void GetTeamDetails()
        {

            while (!quitTeamDetails)
            {

                Console.WriteLine("Write quit, or enter TeamID");
                var input = Console.ReadLine();
                if (input.ToLower() == "quit")
                    quitTeamDetails = true;

                for (int m = 0; m < 2; m++)
                {
                    var filter = m == 0 ? 5 : 9;
                    var matches = new List<Match>();

                    string url = $"http://www.hltv.org/?pageid=188&statsfilter={filter}&teamid={input}";
                    HtmlDocument teamhtml = HWeb.Load(url);
                    for (int i = 5; i < 2074; i++)
                    {

                        i++;
                        var htmlstring = $"//*[@id='back']/div[3]/div[3]/div/div[3]/div/div[{i}]/div";
                        var htmlSectionss = teamhtml.DocumentNode.SelectNodes(htmlstring);

                        var matchIdHtmlString = htmlstring + "/a[1]";
                        var team1IdHtmlString = htmlstring + "/a[2]";
                        var team2IdHtmlString = htmlstring + "/a[3]";
                        var nodes = htmlSectionss?[0].SelectNodes(".//div");

                        if (nodes?.Count() == 8)
                        {
                            var matchid = GetMatchId(teamhtml, matchIdHtmlString);

                            var team1Id = GetTeamId(teamhtml, team1IdHtmlString);
                            var team2Id = GetTeamId(teamhtml, team2IdHtmlString);

                            // Get Team Name and result
                            var team1Name = GetTeamNameAndResult(nodes[1].InnerText);
                            var team2Name = GetTeamNameAndResult(nodes[2].InnerText);

                            // Create team if needed
                            CheckIfNeedToCreateTeam(team1Id, team1Name.Item1);
                            CheckIfNeedToCreateTeam(team2Id, team2Name.Item1);

                            var testString = nodes[0].InnerText; //date
                            var teststring3 = nodes[3].InnerText; //map
                            var teststring4 = nodes[4].InnerText; //event

                            /*****GET MORE INFO******
                             * All players
                             * Create player if he does not exist
                             * 1st round winner - and if ct or terr
                             * 16th round winner - and if ct or terr 
                             */

                            var match = new Match
                            {
                                MatchId = matchid,
                                Date = Convert.ToDateTime(testString),
                                //Team1 = team1Name.Item1,
                                //Team2 = team2Name.Item1,
                                Map = teststring3,
                                Event = teststring4,
                                ResultT1 = team1Name.Item2,
                                ResultT2 = team2Name.Item2,
                                Team1Id = team1Id,
                                //Team1RankValue = 1,
                                Team2Id = team2Id//,
                                //Team2RankValue = 1
                            };

                            matches.Add(match);
                            if (i == 6)
                                Console.WriteLine(team1Name.Item1);

                            // If matchId had been previously saved, we skip
                            if (db.Match.Any(s => s.MatchId == matchid)) continue;
                            db.Match.Add(match);
                            db.SaveChanges();
                        }
                    }

                    var result = matches.GroupBy(p => p.Map).Select(n => new
                    {
                        Map = n.Key,
                        Wins = n.Count(p => p.ResultT1 > p.ResultT2),
                        Losses = n.Count(p => p.ResultT2 > p.ResultT1),
                        WinPercent = Math.Round(n.Count(s => s.ResultT1 > s.ResultT2) / (double)n.Count() * 100, 1),
                        AverageRoundsWin =
                            Math.Round(
                                n.Where(e => e.ResultT1 > e.ResultT2).Sum(k => k.ResultT1) /
                                (double)n.Count(e => e.ResultT1 > e.ResultT2), 1),
                        AverageRoundsLost =
                            Math.Round(
                                n.Where(e => e.ResultT1 > e.ResultT2).Sum(k => k.ResultT2) /
                                (double)n.Count(e => e.ResultT1 > e.ResultT2), 1)
                    }
                        )
                        .OrderByDescending(n => n.WinPercent);

                    foreach (var maps in result)
                    {
                        Console.WriteLine(maps.Map + ": " + maps.Wins + " / " + maps.Losses + "  " + maps.WinPercent +
                                          "% - Av. w/l: " + maps.AverageRoundsWin + " / " + maps.AverageRoundsLost);
                    }
                    Console.WriteLine("");
                }

            }
        }

        public static void GetTeamDetails(int TeamId, List<string> Filters)
        {
            foreach (var filter in Filters)
            {
                var matches = new List<Match>();
                string url = $"http://www.hltv.org/?pageid=188&statsfilter={filter}&teamid={TeamId}";
                HtmlDocument teamhtml = HWeb.Load(url);
                for (int i = 5; i < 2074; i++)
                {

                    i++;
                    var htmlstring = $"//*[@id='back']/div[3]/div[3]/div/div[3]/div/div[{i}]/div";
                    var htmlSectionss = teamhtml.DocumentNode.SelectNodes(htmlstring);

                    var matchIdHtmlString = htmlstring + "/a[1]";
                    var team1IdHtmlString = htmlstring + "/a[2]";
                    var team2IdHtmlString = htmlstring + "/a[3]";
                    var nodes = htmlSectionss?[0].SelectNodes(".//div");

                    if (nodes?.Count() == 8)
                    {
                        var matchid = GetMatchId(teamhtml, matchIdHtmlString);
                        // If matchId had been previously saved, we skip
                        if (db.Match.Any(s => s.MatchId == matchid)) continue;

                        var team1Id = GetTeamId(teamhtml, team1IdHtmlString);
                        var team2Id = GetTeamId(teamhtml, team2IdHtmlString);

                        // Get Team Name and result
                        var team1Name = GetTeamNameAndResult(nodes[1].InnerText);
                        var team2Name = GetTeamNameAndResult(nodes[2].InnerText);

                        // Create team if needed
                        CheckIfNeedToCreateTeam(team1Id, team1Name.Item1);
                        CheckIfNeedToCreateTeam(team2Id, team2Name.Item1);

                        var dateString = nodes[0].InnerText; //date
                        var mapString = nodes[3].InnerText; //map
                        var eventString = nodes[4].InnerText; //event
                        var dDate = Convert.ToDateTime(dateString);

                        /*****GET MORE INFO******
                         * All players
                         * Create player if he does not exist
                         * 1st round winner - and if ct or terr
                         * 16th round winner - and if ct or terr 
                         */



                        var match = new Match
                        {
                            MatchId = matchid,
                            Date = dDate,
                            Map = mapString,
                            Event = eventString,
                            ResultT1 = team1Name.Item2,
                            ResultT2 = team2Name.Item2,
                            Team1Id = team1Id,
                            Team1RankValue = GetRankingValueForTeam(team1Id, dDate),
                            Team2Id = team2Id,
                            Team2RankValue = GetRankingValueForTeam(team2Id, dDate),
                        };

                        matches.Add(match);
                        if (i == 6)
                            Console.WriteLine(team1Name.Item1);

                        db.Match.Add(match);
                        db.SaveChanges();
                    }
                }
            }
        }

        private static double GetRankingValueForTeam(int TeamId, DateTime dDate)
        {
            var result = 0.5;
            var dDateFrom = dDate.AddDays(-7);

            // Check if we find RankingList within the last 7 days
            var RankingList = db.RankingList.Where(p => p.DateOfRank > dDateFrom && p.DateOfRank <= dDate).OrderByDescending(n => n.DateOfRank).FirstOrDefault();
            if (RankingList != null)
            {
                // Is this teamid on that list
                var Rank = db.Rank.Where(p => p.TeamId == TeamId && p.RankingListId == RankingList.RankingListId).FirstOrDefault();
                if (Rank != null && Rank.RankPosition != 0)
                {
                    if (Rank.RankPosition <= 6) // Tier 1
                        result = 1.0;
                    else if (Rank.RankPosition <= 12) // Tier 2
                        result = 0.9;
                    else if (Rank.RankPosition <= 18) // Tier 3
                        result = 0.8;
                    else if (Rank.RankPosition <= 24) // Tier 4
                        result = 0.7; // Tier 5
                    else if (Rank.RankPosition <= 30) // Tier 5
                        result = 0.6;
                    else // No on the list for this period, ergo: Tier 6
                        result = 0.5;
                }
            }
            else // Set special value when no ranking list was found for this period, so we may update it later if we want.
                result = 0.51;

            return result;
        }

        public static void GetRankingList(string rankingUrl)
        {
            //string RankingUrl = "http://www.hltv.org/ranking/teams/2017/january/2/";
            //string RankingUrl = "http://www.hltv.org/ranking/teams/2017/january/9/";
            //string RankingUrl = "http://www.hltv.org/ranking/teams/2017/january/16/";
            //string RankingUrl = "http://www.hltv.org/ranking/teams/2017/january/23/";


            var dateTime = GetDateTime(rankingUrl);
            HtmlDocument rankingHtmlDocument = HWeb.Load(rankingUrl);
            if (!db.RankingList.Any(s => s.DateOfRank == dateTime))
            {
                var rankingListId = Guid.NewGuid();
                var rankingList = new RankingList
                {
                    RankingListId = rankingListId,
                    DateOfRank = dateTime

                };
                db.RankingList.Add(rankingList);
                for (int index = 2; index < 31; index++)
                {
                    //*[1] - Selects the first div in within div[{index 1-31}] 
                    //*[2] - Selects the second div in *[1]
                    var IdString = $"//*[@id='back']/div[3]/div[3]/div/div[{index}]/*[1]";
                    var rankingUrlStringNo = $"//*[@id='back']/div[3]/div[3]/div/div[{index}]/*[1]/*[2]";
                    var id = rankingHtmlDocument.DocumentNode.SelectNodes(IdString);

                    if (id != null)
                    {
                        var teamId = id[0].Id.Remove(0, 5);
                        var team = rankingHtmlDocument.DocumentNode.SelectNodes(rankingUrlStringNo);
                        var teamAndRanking = GetTeamRanking(team[0].InnerText);

                        var ranking = new Rank
                        {
                            RankPosition = index - 1,
                            Points = teamAndRanking.Item2,
                            TeamId = Convert.ToInt32(teamId),
                            RankingListId = rankingListId
                        };
                        db.Rank.Add(ranking);
                      
                    }
                    db.SaveChanges();

                }

            }
        }
        private static DateTime GetDateTime(string rankingUrl)
        {
            string[] stringSeperators = { "/teams/" };
            var result = rankingUrl.Split(stringSeperators, StringSplitOptions.None);

            string[] stringsep = { "/" };
            var results = result[1].Split(stringsep, StringSplitOptions.None);

            var dateTimeValue = Convert.ToDateTime(results[1] + "," + results[2] + "," + results[0]);
            var datetime = new DateTime(dateTimeValue.Year, dateTimeValue.Month, dateTimeValue.Day);
            return datetime;

        }

        private static void CheckIfNeedToCreateTeam(int TeamId, string TeamName)
        {
            if (!db.Team.Any(s => s.TeamId == TeamId))
            {
                var newTeam = new Team { TeamId = TeamId, TeamName = TeamName };
                db.Team.Add(newTeam);
                db.SaveChanges();
            }
        }

        private static Tuple<string, int> GetTeamNameAndResult(string innerText)
        {
            string[] stringSeperators = { "(" };

            var result = innerText.Split(stringSeperators, StringSplitOptions.None);
            var rounds = result[1].Remove(result[1].Length - 1);
            var tupleString = new Tuple<string, int>(result[0], Convert.ToInt32(rounds));
            return tupleString;

        }

        private static int GetTeamId(HtmlDocument teamhtml, string htmlstring)
        {
            int lTeamID = 0;
            string[] stringSeparators = new string[] { "&amp;" };

            var teamid1 = teamhtml.DocumentNode.SelectNodes(htmlstring);
            var teamid = teamid1[0].Attributes["href"].Value;
            var result = teamid.Split(stringSeparators, StringSplitOptions.None);
            var teamID = result[1].Substring(7);

            int.TryParse(teamID, out lTeamID);
            return lTeamID;
        }

        private static int GetMatchId(HtmlDocument teamhtml, string htmlstring)
        {
            int lMatchID = 0;
            string[] stringSeparators = new string[] { "&amp;" };

            var teamid1 = teamhtml.DocumentNode.SelectNodes(htmlstring);
            var teamid = teamid1[0].Attributes["href"].Value;
            var result = teamid.Split(stringSeparators, StringSplitOptions.None);
            var gameId = result[1].Substring(8);

            int.TryParse(gameId, out lMatchID);
            return lMatchID;
        }

        private static Tuple<string, int> GetTeamRanking(string innerText)
        {
            string[] stringSeperators = { "(" };
            var newstring = "";
            string results = Regex.Replace(innerText, @"\n\n?|\n", newstring);

            var result = results.Split(stringSeperators, StringSplitOptions.None);
            var rounds = result[1].Remove(result[1].Length - 9);
            var tupleString = new Tuple<string, int>(result[0].TrimStart(), Convert.ToInt32(rounds));
            return tupleString;

        }
    }
}
