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
            GetTeamDetails(4501);

            //GetTeamIdsFromUrl("http://www.hltv.org/match/2307714-g2-flipsid3-iem-katowice-2017-eu-closed-qualifier");
            //Import url from mvc project
            //GetRankingList("http://www.hltv.org/ranking/teams/2016/january/5/");

        }




        private static int GetPlayerID(string outerHtml)
        {
            bool first = false;
            string wordToFind = "/player/";
            int final = -1;

            if (outerHtml.Contains(wordToFind))
            {
                first = true;
            }


            if (first)
            {

                string[] stringSeparators = { "<a href=" };
                string[] secondSplint = { "><span style=" };
                string[] thirdspilt = { "-" };
                string word = "/player//";

                var result = outerHtml.Split(stringSeparators, StringSplitOptions.None);
                var result2 = result[1].Split(secondSplint, StringSplitOptions.None);
                var ID = result2[0].Remove(0, word.Length);
                var ids = ID.Split(thirdspilt, StringSplitOptions.None);
                final = Convert.ToInt32(ids[0]);
            }
            else
            {

                string[] stringSeparators = { "pageid=173&amp;" };
                string[] secondSplint = { "\">" };
                string[] thirdspilt = { "-" };
                string word = "playerid=";
                var result = outerHtml.Split(stringSeparators, StringSplitOptions.None);
                var result2 = result[1].Split(secondSplint, StringSplitOptions.None);
                var ID = result2[0].Remove(0, word.Length);

                final = Convert.ToInt32(ID);


            }



            return final;
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

        public static void GetTeamDetails(int TeamId)
        {


            DateTime lastScraped = new DateTime(1900, 1, 1);
            // Get ScrapeHistoryTeams record for latest scrape info for this team
            var history = db.ScrapeHistoryTeams.Where(p => p.TeamId == TeamId).OrderByDescending(k => k.LastDayScraped).ToList();
            if (history.Any())
                lastScraped = history.FirstOrDefault().LastDayScraped;

            int lCounter = 0;

            string url = $"http://www.hltv.org/?pageid=188&teamid={TeamId}";

            bool bHasCreatedCurrentTeam = false;
            HtmlDocument teamhtml = HWeb.Load(url);

            var playersHtmlString = "//*[@id='back']/div[3]/div[3]/div/div[8]/div[2]";


            for (int i = 5; i < 2074; i++)
            {

                i++;
                var htmlstring = $"//*[@id='back']/div[3]/div[3]/div/div[3]/div/div[{i}]/div";
                var htmlSectionss = teamhtml.DocumentNode.SelectNodes(htmlstring);

                var matchIdHtmlString = htmlstring + "/a[1]";
                var team1IdHtmlString = htmlstring + "/a[2]";
                var team2IdHtmlString = htmlstring + "/a[3]";
                var nodes = htmlSectionss?[0].SelectNodes(".//div");

                if (nodes?.Count() != 8) continue;
                var matchid = GetMatchId(teamhtml, matchIdHtmlString);
                if (db.Match.Any(s => s.MatchId == matchid)) continue;
                // If matchId had been previously saved, we skip

                var team1Id = GetTeamId(teamhtml, team1IdHtmlString);
                var team2Id = GetTeamId(teamhtml, team2IdHtmlString);

                // Get Team Name and result
                var team1Name = GetTeamNameAndResult(nodes[1].InnerText);
                var team2Name = GetTeamNameAndResult(nodes[2].InnerText);

                var gameURL = $"http://www.hltv.org/?pageid=188&matchid={matchid}";
                var Players = GetPlayers(gameURL, team1Id, team2Id);




                // Create team if needed, no need after that.
                if (!bHasCreatedCurrentTeam)
                {
                    CheckIfNeedToCreateTeam(team1Id, team1Name.Item1);
                    bHasCreatedCurrentTeam = true;
                }
                CheckIfNeedToCreateTeam(team2Id, team2Name.Item1);

                var dateString = nodes[0].InnerText; //date
                var mapString = nodes[3].InnerText; //map
                var eventString = nodes[4].InnerText; //event
                var dDate = Convert.ToDateTime(dateString);

                // If we have moved past last scraped date, or year old data
                if (dDate < lastScraped.AddDays(-1) || dDate < DateTime.Now.AddYears(-1))
                {
                    // And we have added some records
                    if (lCounter > 0)
                    {
                        // We save history record when last scraped for this team.
                        db.ScrapeHistoryTeams.Add(new ScrapeHistoryTeams { TeamId = TeamId, LastDayScraped = DateTime.Now });
                        db.SaveChanges();
                    }

                    // And quit scraping
                    return;
                }

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

                var t1Players = Players.Where(x => x.TeamID == team1Id).ToArray();
                if (t1Players.Length > 0)
                    match.T1Player1Id = t1Players[0].PlayerId;
                if (t1Players.Length > 1)
                    match.T1Player2Id = t1Players[1].PlayerId;
                if (t1Players.Length > 2)
                    match.T1Player3Id = t1Players[2].PlayerId;
                if (t1Players.Length > 3)
                    match.T1Player4Id = t1Players[3].PlayerId;
                if (t1Players.Length > 4)
                    match.T1Player5Id = t1Players[4].PlayerId;


                var t2Players = Players.Where(x => x.TeamID == team2Id).ToArray();
                if(t2Players.Length > 0)
                    match.T2Player1Id = t2Players[0].PlayerId;
                if (t2Players.Length > 1)
                    match.T2Player2Id = t2Players[1].PlayerId;
                if (t2Players.Length > 2)
                    match.T2Player3Id = t2Players[2].PlayerId;
                if (t2Players.Length > 3)
                    match.T2Player4Id = t2Players[3].PlayerId;
                if (t2Players.Length > 4)
                    match.T2Player5Id = t2Players[4].PlayerId;





                db.Match.Add(match);
                db.SaveChanges();
                lCounter++;
            }
        }

        private static List<Player> GetPlayers(string gameURl, int team1ID, int team2ID)
        {
            List<Player> players = new List<Player>();

            HtmlDocument GameHtml = HWeb.Load(gameURl);


            for (int i = 6; i <= 24; i += 2)
            {
                var playerNameHtml = $"//*[@id='back']/div[3]/div[3]/div/div[8]/div[2]/div/div[{i}]/div/div[1]";
                var teamNameHtml = $"//*[@id='back']/div[3]/div[3]/div/div[8]/div[2]/div/div[{i}]/div/div[2]/a";
                var teamNameIDhtml = $"//*[@id='back']/div[3]/div[3]/div/div[8]/div[2]/div/div[{i}]/div/div[2]";
                //*[@id="back"]/div[3]/div[3]/div/div[6]/div[2]/div/div[6]/div

                try
                {


                    var html = GameHtml.DocumentNode.SelectNodes(playerNameHtml);
                    var playerID = GetPlayerID(html[0].InnerHtml);
                    var playerName = html[0].InnerText;
                    var teamNameResult = GameHtml.DocumentNode.SelectNodes(teamNameHtml);
                    var Teamname = teamNameResult[0].InnerHtml;

                    var teamIDResult = GameHtml.DocumentNode.SelectNodes(teamNameIDhtml);

                    var teamID = 0;

                    if (teamIDResult[0].InnerHtml.Contains(team1ID.ToString()))
                    {
                        teamID = team1ID;

                    }
                    else
                    {
                        teamID = team2ID;
                    }

                    var pl = new Player()
                    {
                        PlayerId = playerID,
                        PlayerName = playerName,
                        TeamID = teamID
                    };


                    players.Add(pl);

                    if (!db.Player.Any(p => p.PlayerId == pl.PlayerId))
                    {
                        db.Player.Add(pl);
                    }

                }
                catch (Exception ex)
                {
                    var logger = new ErrorLogger
                    {
                        Error = ex.ToString(),
                        url = gameURl
                    };

                    db.ErrorLoggers.Add(logger);


                }

            }
            db.SaveChanges();
            return players;
        }

        public static double GetRankingValueForTeam(int TeamId, DateTime dDate)
        {
            var result = 0.5;
            var dDateFrom = dDate.AddDays(-8);

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
                for (int index = 2; index < 35; index++)
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

            if (teamid1 != null)
            {
                var teamid = teamid1[0].Attributes["href"].Value;
                var result = teamid.Split(stringSeparators, StringSplitOptions.None);
                var gameId = result[1].Substring(8);

                int.TryParse(gameId, out lMatchID);
            }
            else
            {
                lMatchID = 0;
            }
          

            
          
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

        public static Tuple<int, int> GetTeamIdsFromUrl(string matchUrls)
        {
            string url = matchUrls;
            HtmlDocument matchHtml = HWeb.Load(url);

            string[] stringSeperators = { ";teamid=" };
            string[] SecondSpilt = { ">" };

            var team1IdHtmlString = $"  //*[@id='back']/div[3]/div[3]/div/div[1]/div[1]/span/div/div/div[1]/span[1]/a";
            var htmlSectionss = matchHtml.DocumentNode.SelectNodes(team1IdHtmlString);

            var result = htmlSectionss[0].OuterHtml.Split(stringSeperators, StringSplitOptions.None);
            var team1Spilt = result[1].Split(SecondSpilt, StringSplitOptions.None);
            var team11Id = team1Spilt[0].Remove(team1Spilt[0].Length - 1);


            var team2IdHtmlString = $"    //*[@id='back']/div[3]/div[3]/div/div[1]/div[1]/span/div/div/div[3]/span/a";
            //*[@id="back"]/div[3]/div[3]/div/div[1]/div[1]/span/div/div/div[3]/span
            var team2HtmlSections = matchHtml.DocumentNode.SelectNodes(team2IdHtmlString);


            var result2 = team2HtmlSections[0].OuterHtml.Split(stringSeperators, StringSplitOptions.None);
            var team2Spilt = result2[1].Split(SecondSpilt, StringSplitOptions.None);

            var team2Id = team2Spilt[0].Remove(team2Spilt[0].Length - 1);
            var tupleString = new Tuple<int, int>(Convert.ToInt32(team11Id), Convert.ToInt32(team2Id));


            return tupleString;

        }
    }
}









#region "Unused"



// private static void GetTeamLineup(string matchUrls)
//        {

//            var teamIDs = GetTeamIdsFromUrl(matchUrls);

//            //*[@id="myTab"]

//            int firstdivId = 0;
//            int secondDivId = 0;
//            bool finished = false;
//            string url = matchUrls;
//            HtmlDocument matchHtml = HWeb.Load(url);

//            var matchIDHtml = "//*[@id='myTab']/li";

//            var matchID = matchHtml.DocumentNode.SelectNodes(matchIDHtml);
//            var team1string = "//*[@id='back']/div[3]/div[3]/div/div[1]/div[20]/div";
//            var matchOver = "//*[@id='back']/div[3]/div[3]/div/div[1]/div[3]";
//            var matchOverCheck = matchHtml.DocumentNode.SelectNodes(matchOver);
//            if (matchOverCheck[0].InnerText == "Match over")
//            {
//                firstdivId = 19;
//                secondDivId = 22;

//            }
//            else
//            {
//                firstdivId = 16;
//                secondDivId = 19;
//            }

//            for (int i = 1; i < 10; i++)
//            {
//                var span1 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[{firstdivId}]/div[{i}]/div[1]/span/a";
//                var span1Name = matchHtml.DocumentNode.SelectNodes(span1);
//                var name = span1Name[0].InnerText;
//                var ID = GetPlayerID(span1Name[0].OuterHtml);
//                var Team1 = teamIDs.Item1;
//                /*Create new player*/
//var span2 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[{secondDivId}]/div[{i}]/div[1]/span/a";
//var span2Name = matchHtml.DocumentNode.SelectNodes(span2);
//var name2 = span2Name[0].InnerText;
//var ID2 = GetPlayerID(span2Name[0].OuterHtml);
//var team2ID = teamIDs.Item2;

///*Create new player*/



////*[@id='back']/div[3]/div[3]/div/div[1]/div[16]/div[3]/div[1]/span
////*[@id='back']/div[3]/div[3]/div/div[1]/div[16]/div[5]/div[1]/span
////*[@id='back']/div[3]/div[3]/div/div[1]/div[16]/div[7]/div[1]/span
////*[@id='back']/div[3]/div[3]/div/div[1]/div[16]/div[9]/div[1]/span


///*Buin*/
////*[@id="back"]/div[3]/div[3]/div/div[1]/div[{secondDivId}]

///*Buin seinni*/
////*[@id="back"]/div[3]/div[3]/div/div[1]/div[22]

//i++;
//            }



//        }


#endregion