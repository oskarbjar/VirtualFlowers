using System;
using System.Linq;
using HtmlAgilityPack;
using Models;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Match = Models.Match;
using System.Threading.Tasks;

namespace VirtualFlowers
{
    public class Program
    {
        static readonly HtmlWeb HWeb = new HtmlAgilityPack.HtmlWeb();
        private readonly DatabaseContext db = new DatabaseContext();
        private static bool quitTeamDetails;

        static void Main(string[] args)
        {
            //GetTeamDetails(4501);

            //GetTeamIdsFromUrl("http://www.hltv.org/match/2307714-g2-flipsid3-iem-katowice-2017-eu-closed-qualifier");
            //Import url from mvc project
            //GetRankingList("http://www.hltv.org/ranking/teams/2016/january/5/");
            // GetFirstAndSixtenthRound()

            //GetTeamLineup("http://www.hltv.org/match/2308074-godsent-endpoint-esea-premier-season-24-europe");

            // var stringList = GetMatches("http://www.hltv.org/matches");


        }

        public List<UrlViewModel> GetMatches()
        {
            var Models = new List<UrlViewModel>();

            string url = "http://www.hltv.org/matches";
            List<string> urlsList = new List<string>();

            for (int i = 1; i < 50; i++)
            {
                var objectToAdd = new UrlViewModel();
                var htmlString = $"//*[@id='back']/div[3]/div[3]//div/div[{i}]/div[1]";
                var matchesHtml = HWeb.Load(url);
                var selection = matchesHtml.DocumentNode.SelectNodes(htmlString);


                //var team1 = $"//*[@id='back']/div[3]/div[3]/div/div[{i}]/div[1]/div[4]"/*Team2*/;

                //var team1Selections = matchesHtml.DocumentNode.SelectNodes(team1);
                //var team2 = $"//*[@id='back']/div[3]/div[3]/div/div[{i}]/div[1]/div[2]"/*Team1*/;
                //var team2Selections = matchesHtml.DocumentNode.SelectNodes(team2);
                var detaild = $"//*[@id='back']/div[3]/div[3]/div/div[{i}]/div[1]/div[5]" /*Url*/;

                var BestOf3 = $"//*[@id='back']/div[3]/div[3]/div/div[{i}]/div[1]/div[3]/div/div[1]";


                var urls = matchesHtml.DocumentNode.SelectNodes(detaild);
                var BestOf3Result = matchesHtml.DocumentNode.SelectNodes(BestOf3);




                if (urls != null)
                {
                    var urlString = GetMatchHref(urls[0].InnerHtml);
                    var IsBestOf3 = false;
                    if (BestOf3Result[0].InnerText.Contains("Best of 3"))
                    {
                        IsBestOf3 = true;
                    };



                    var urlstring = urlString.Remove(0, 1);
                    urlstring = urlstring.Remove(urlstring.Length - 1);

                    urlsList.Add(urlstring);

                    objectToAdd.Url = urlstring;
                    objectToAdd.BestOf3 = IsBestOf3;
                    Models.Add(objectToAdd);
                }

            }

            return Models;


        }

        private static string GetMatchHref(string innerHtml)
        {
            string[] stringSeperator = { "<a href=" };
            string[] secondSplit = { ">" };

            var result1 = innerHtml.Split(stringSeperator, StringSplitOptions.None);
            var result2 = result1[1].Split(secondSplit, StringSplitOptions.None);

            return result2[0];

        }

        public ExpectedLineUp GetTeamLineup(string matchUrls)
        {

            try
            {

                var expectedLineUp = new ExpectedLineUp();
                //var lineUps = new ExpectedLineUp().Players;
                //var players = new List<Player>();

                if (expectedLineUp.Players == null)
                {
                    expectedLineUp.Players = new List<Player>();
                }

                var teamIDs = GetTeamIdsFromUrl();

                var url = matchUrls;
                var matchHtml = HWeb.Load(MatchUrl);
                var firstDiv = true;
                var secondDiv = false;
                var thirdDiv = false;
                var fourthDiv = false;

                var span1Name = new HtmlNodeCollection(HtmlNode.CreateNode(""));
                var span2Name = new HtmlNodeCollection(HtmlNode.CreateNode(""));
                var count = 0;


                try
                {
                    string span1;
                    string span2;
                    if (firstDiv)
                    {
                        //var team1IdHtmlString = $"//*[@class='team1-gradient']";
                        span1 = $"//*[@class='lineup standard-box']";
                        span1Name = matchHtml.DocumentNode.SelectNodes(span1);

                        /*Get players from lineup*/
                        var counter = 0;
                        foreach (var item in span1Name)
                        {
                            //td[@class="plaintext"]
                            var players = item.SelectNodes("//*[@class='players']/*[@class='table']//*[@class='player']//@href");

                            foreach (var player in players)
                            {
                                var names = player.InnerText.Trim();

                                if (names != "")
                                {


                                    var ids = GetPlayerID(player.Attributes[0].Value);
                                    var pl = new Player();

                                    pl.PlayerId = ids;
                                    pl.PlayerName = names;

                                    if (counter <= 4)
                                    {


                                        pl.TeamID = teamIDs.Item1;
                                    }
                                    else
                                    {
                                        pl.TeamID = teamIDs.Item2;
                                    }



                                    var plexists = expectedLineUp.Players.Any(x => x.PlayerId == ids);
                                    if (!plexists)
                                    {
                                        expectedLineUp.Players.Add(pl);

                                    }
                                    counter++;
                                }
                            }











                            //span1 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[16]/div[{i}]/div[1]/span/a";
                            //span1Name = matchHtml.DocumentNode.SelectNodes(span1);
                            //span2 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[19]/div[{i}]/div[1]/span/a";
                            //span2Name = matchHtml.DocumentNode.SelectNodes(span2);

                            if (span1Name == null)
                            {
                                secondDiv = true;
                                firstDiv = false;
                            }



                        }
                    }

                    //if (secondDiv)
                    //{
                    //    span1 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[19]/div[{i}]/div[1]/span/a";
                    //    span1Name = matchHtml.DocumentNode.SelectNodes(span1);
                    //    span2 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[22]/div[{i}]/div[1]/span/a";
                    //    span2Name = matchHtml.DocumentNode.SelectNodes(span2);

                    //    if (span2Name == null)
                    //    {
                    //        secondDiv = false;
                    //        thirdDiv = true;
                    //    }
                    //}

                    //if (thirdDiv)
                    //{
                    //    span1 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[21]/div[{i}]/div[1]/span/a";
                    //    span1Name = matchHtml.DocumentNode.SelectNodes(span1);
                    //    span2 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[24]/div[{i}]/div[1]/span/a";
                    //    span2Name = matchHtml.DocumentNode.SelectNodes(span2);

                    //    if (span1Name == null)
                    //    {
                    //        thirdDiv = false;
                    //        fourthDiv = true;

                    //    }
                    //}

                    //if (fourthDiv)
                    //{
                    //    span1 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[24]/div[{i}]/div[1]/span/a";
                    //    span1Name = matchHtml.DocumentNode.SelectNodes(span1);
                    //    span2 = $"//*[@id='back']/div[3]/div[3]/div/div[1]/div[27]/div[{i}]/div[1]/span/a";

                    //    span2Name = matchHtml.DocumentNode.SelectNodes(span2);

                    //}




                }
                catch (Exception ex)
                {
                    var nothing = ex;
                }


                return expectedLineUp;

            }
            catch (Exception)
            {
                /*Logga niður villur*/
                //throw;
                // return empty list, to avoid crash
                return new ExpectedLineUp();
            }


        }


        private int GetPlayerID(string outerHtml)
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

                //    string[] stringSeparators = { "<a href=" };
                string[] secondSplint = { "/" };
                string[] thirdspilt = { "/" };
                string word = "/player//";

                //    var result = outerHtml.Split(stringSeparators, StringSplitOptions.None);
                //    var result2 = result[1].Split(secondSplint, StringSplitOptions.None);
                //    var ID = result2[0].Remove(0, word.Length);
                var ids = outerHtml.Split(secondSplint, StringSplitOptions.None);
                final = Convert.ToInt32(ids[2]);
            }
            //else
            //{

            //    string[] stringSeparators = { "pageid=173&amp;" };
            //    string[] secondSplint = { "\">" };
            //    string[] thirdspilt = { "-" };
            //    string word = "playerid=";
            //    var result = outerHtml.Split(stringSeparators, StringSplitOptions.None);
            //    var result2 = result[1].Split(secondSplint, StringSplitOptions.None);
            //    var ID = result2[0].Remove(0, word.Length);

            //    final = Convert.ToInt32(ID);


            //}



            return final;
        }
        private void GetTeamDetails()
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

        public Task GetTeamDetails(int TeamId)
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
            var StatsTableHtml = $"//*[@class='stats-table']";
            var StatsTable1 = teamhtml.DocumentNode.SelectNodes(StatsTableHtml);
            var StatsTable = teamhtml.DocumentNode.SelectNodes($"/ html / body / div[2] / div / div[2] / div[1] / div / table / tbody / tr[25] / td[1]");

            var StatsTable2 = teamhtml.DocumentNode.SelectNodes($"/ html / body / div[2] / div / div[2] / div[1] / div / table / tbody / tr[position()>0]");

            var test = teamhtml.DocumentNode.SelectNodes($"/html/body/div[2]/div/div[2]/div[1]/div/table/tbody/tr");




            foreach (var item in StatsTable2)
            {
                var bla = item;

                var dateString = bla.ChildNodes[1].InnerText;
                var dDate = NewDate(dateString);
                var matchID = GetMatchIDS(bla.ChildNodes[1].InnerHtml);
                var mathcurl = GetMatchUrl(bla.ChildNodes[1].InnerHtml);

                var rounds = GetRoundsV2(mathcurl);

                var players = GetTeamLineup(mathcurl);

                var Event = bla.ChildNodes[3].InnerText;
                var opponent = bla.ChildNodes[7].InnerText;


                var map = bla.ChildNodes[9].InnerText;
                var result = getResult(bla.ChildNodes[11].InnerText);
                var winOrLoss = bla.ChildNodes[13].InnerText;


                // If we have moved past last scraped date, or year old data
                if (dDate < lastScraped.AddDays(-1) || dDate < DateTime.Now.AddYears(-1))
                {
                    // And we have added some records
                    if (lCounter > 0 || !history.Any())
                    {
                        // We save history record when last scraped for this team.
                        db.ScrapeHistoryTeams.Add(new ScrapeHistoryTeams { TeamId = TeamId, LastDayScraped = DateTime.Now });
                        db.SaveChanges();
                    }

                    // And quit scraping
                    return Task.FromResult(0);
                }

                if (!bHasCreatedCurrentTeam)
                {
                    CheckIfNeedToCreateTeam(Team1ID, Team1Name);
                    bHasCreatedCurrentTeam = true;
                }
                CheckIfNeedToCreateTeam(Team2ID, Team2Name);


                var match = new Match
                {
                    MatchId = Convert.ToInt32(matchID),
                    Date = dDate,
                    Map = map,
                    Event = Event,
                    ResultT1 = result.Item1, //Sækja value úr rounds
                    ResultT2 = result.Item2,
                    Team1Id = Team1ID,
                    Team1RankValue = GetRankingValueForTeam(Team1ID, dDate),
                    Team2Id = Team2ID,
                    Team2RankValue = GetRankingValueForTeam(Team2ID, dDate),



                    FirstRound1HWinTeamId = rounds.Where(x => x.Round1 == true).FirstOrDefault().TeamId,
                    FirstRound1HWinTerr = rounds.Where(x => x.Round1).FirstOrDefault().Terrorist,
                    FirstRound1HWinCt = rounds.Where(x => x.Round1).FirstOrDefault().CounterTerrorist,
                    FirstRound2HWinTeamId = rounds.Where(x => x.Round16).FirstOrDefault().TeamId,
                    FirstRound2HWinTerr = rounds.Where(x => x.Round16).FirstOrDefault().Terrorist,
                    FirstRound2HWinCT = rounds.Where(x => x.Round16).FirstOrDefault().CounterTerrorist

                };

                var t1Players = players.Players.Where(x => x.TeamID == Team1ID).ToArray();
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

                var t2Players = players.Players.Where(x => x.TeamID == Team2ID).ToArray();
                if (t2Players.Length > 0)
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




            }



            //table[@id='ctl00_MasterPlaceHolder_GrdHistory']/tbody/tr[position()>1]










            //for (int i = 5; i < 2074; i++)
            //{

            //    i++;
            //    var htmlstring = $"//*[@id='back']/div[3]/div[3]/div/div[3]/div/div[{i}]/div";
            //    var htmlSectionss = teamhtml.DocumentNode.SelectNodes(htmlstring);
            //    if (htmlSectionss == null)
            //        break;

            //    var matchIdHtmlString = htmlstring + "/a[1]";
            //    var team1IdHtmlString = htmlstring + "/a[2]";
            //    var team2IdHtmlString = htmlstring + "/a[3]";
            //    var nodes = htmlSectionss?[0].SelectNodes(".//div");

            //    var dateString = nodes[0].InnerText; //date
            //    var dDate = NewDate(dateString);

            //    // If we have moved past last scraped date, or year old data
            //    if (dDate < lastScraped.AddDays(-1) || dDate < DateTime.Now.AddYears(-1))
            //    {
            //        // And we have added some records
            //        if (lCounter > 0 || !history.Any())
            //        {
            //            // We save history record when last scraped for this team.
            //            db.ScrapeHistoryTeams.Add(new ScrapeHistoryTeams { TeamId = TeamId, LastDayScraped = DateTime.Now });
            //            db.SaveChanges();
            //        }

            //        // And quit scraping
            //        return Task.FromResult(0);
            //    }

            //    if (nodes?.Count() != 8)
            //    {
            //        string TeamNameUrl = $"http://www.hltv.org/?pageid=179&teamid={TeamId}";
            //        HtmlDocument teamNameHtml = HWeb.Load(TeamNameUrl);
            //        var nameHtml = "//*[@id='back']/div[3]/div[3]/div/div[2]/div[2]/div[1]/div[2]";
            //        var name = teamNameHtml.DocumentNode.SelectNodes(nameHtml);
            //        CheckIfNeedToCreateTeam(TeamId, name[0].InnerText);
            //        continue;
            //    }
            //    var matchid = GetMatchId(teamhtml, matchIdHtmlString);
            //    if (db.Match.Any(s => s.MatchId == matchid)) continue;
            //    // If matchId had been previously saved, we skip
            //    var team1Id = GetTeamId(teamhtml, team1IdHtmlString);
            //    var team2Id = GetTeamId(teamhtml, team2IdHtmlString);


            //    // Get Team Name and result
            //    var team1Name = GetTeamNameAndResult(nodes[1].InnerText);
            //    var team2Name = GetTeamNameAndResult(nodes[2].InnerText);

            //    var gameURL = $"http://www.hltv.org/?pageid=188&matchid={matchid}";
            //    var Players = GetPlayers(gameURL, team1Id, team2Id);

            //    var rounds = GetRounds(gameURL, team1Id, team2Id);


            //    // Create team if needed, no need after that.
            //    if (!bHasCreatedCurrentTeam)
            //    {
            //        CheckIfNeedToCreateTeam(team1Id, team1Name.Item1);
            //        bHasCreatedCurrentTeam = true;
            //    }
            //    CheckIfNeedToCreateTeam(team2Id, team2Name.Item1);

            //    var mapString = nodes[3].InnerText; //map
            //    var eventString = nodes[4].InnerText; //event



            //    var firstRoundWin = rounds.FirstOrDefault(y => y.Round1);
            //    bool firstRoundTerr = firstRoundWin.Terrorist;
            //    bool firstRoundCt = !firstRoundTerr;
            //    var secondRoundWin = rounds.FirstOrDefault(y => y.Round16);
            //    bool secondRoundTerr = false;

            //    if (secondRoundWin != null)
            //    {
            //        secondRoundTerr = secondRoundWin.Terrorist;
            //    }

            //    var secondRoundCt = !secondRoundTerr;
            //    var firstRound2HWinTeamId = 0;
            //    var firstRound1HWinTeamId = 0;
            //    var firstOrDefault = rounds.FirstOrDefault(x => x.Round16);
            //    if (firstOrDefault != null)
            //    {
            //        firstRound2HWinTeamId = firstOrDefault.TeamId;
            //    }


            //    var roundHistory = rounds.FirstOrDefault(x => x.Round1);
            //    if (roundHistory != null)
            //    {
            //        firstRound1HWinTeamId = roundHistory.TeamId;
            //    }



            //    var match = new Match
            //    {
            //        MatchId = matchid,
            //        Date = dDate,
            //        Map = mapString,
            //        Event = eventString,
            //        ResultT1 = team1Name.Item2,
            //        ResultT2 = team2Name.Item2,
            //        Team1Id = team1Id,
            //        Team1RankValue = GetRankingValueForTeam(team1Id, dDate),
            //        Team2Id = team2Id,
            //        Team2RankValue = GetRankingValueForTeam(team2Id, dDate),
            //        FirstRound1HWinTeamId = firstRound1HWinTeamId,
            //        FirstRound1HWinTerr = firstRoundTerr,
            //        FirstRound1HWinCt = firstRoundCt,
            //        FirstRound2HWinTeamId = firstRound2HWinTeamId,
            //        FirstRound2HWinTerr = secondRoundTerr,
            //        FirstRound2HWinCT = secondRoundCt

            //    };

            //    var t1Players = Players.Where(x => x.TeamID == team1Id).ToArray();
            //    if (t1Players.Length > 0)
            //        match.T1Player1Id = t1Players[0].PlayerId;
            //    if (t1Players.Length > 1)
            //        match.T1Player2Id = t1Players[1].PlayerId;
            //    if (t1Players.Length > 2)
            //        match.T1Player3Id = t1Players[2].PlayerId;
            //    if (t1Players.Length > 3)
            //        match.T1Player4Id = t1Players[3].PlayerId;
            //    if (t1Players.Length > 4)
            //        match.T1Player5Id = t1Players[4].PlayerId;

            //    var t2Players = Players.Where(x => x.TeamID == team2Id).ToArray();
            //    if (t2Players.Length > 0)
            //        match.T2Player1Id = t2Players[0].PlayerId;
            //    if (t2Players.Length > 1)
            //        match.T2Player2Id = t2Players[1].PlayerId;
            //    if (t2Players.Length > 2)
            //        match.T2Player3Id = t2Players[2].PlayerId;
            //    if (t2Players.Length > 3)
            //        match.T2Player4Id = t2Players[3].PlayerId;
            //    if (t2Players.Length > 4)
            //        match.T2Player5Id = t2Players[4].PlayerId;

            //    db.Match.Add(match);

            //    db.SaveChanges();
            //    lCounter++;
            //}

            // If we have added some records or have no record for this team
            if (lCounter > 0 || !history.Any())
            {
                // We save history record when last scraped for this team.
                db.ScrapeHistoryTeams.Add(new ScrapeHistoryTeams { TeamId = TeamId, LastDayScraped = DateTime.Now });
                db.SaveChanges();
            }
            return Task.FromResult(0);
        }
        /// <summary>
        /// Returns Result from matches
        /// </summary>
        /// <param name="innerText"></param>
        /// <returns>tuple with 2 integers, home and away team.</returns>
        private Tuple<int, int> getResult(string innerText)
        {
            var returnvalue = new Tuple<int, int>(int.MinValue, int.MinValue);
            string[] seperator = new string[] { "-" };
            var resultArray = innerText.Split(seperator, StringSplitOptions.None);

            returnvalue = new Tuple<int, int>(Convert.ToInt32(resultArray[0]), Convert.ToInt32(resultArray[1]));

            return returnvalue;
        }

        private object GetMatchIDS(string innerHtml)
        {
            int id = 0;
            string[] stringSeperators = { "/" };
            var IdArray = innerHtml.Split(stringSeperators, StringSplitOptions.None);

            return IdArray[4];
        }

        private string GetMatchUrl(string innerHtml)
        {
            string[] stringSeparators = new string[] { "&amp;" };
            string[] stringSeprators2 = new string[] { "<a href=" };
            var urlsArray = innerHtml.Split(stringSeparators, StringSplitOptions.None);
            var FinalUrl = urlsArray[0].Split(stringSeprators2, StringSplitOptions.None);


            return FinalUrl[1];
        }

        private DateTime NewDate(string dateString)
        {
            string[] stringSeperators = { "/" };
            string[] stringSeperators1 = { " " };

            var result = dateString.Split(stringSeperators, StringSplitOptions.None);
            var result1 = result[1].Split(stringSeperators1, StringSplitOptions.None);
            var date = Convert.ToInt32(result[0]);
            var month = Convert.ToInt32(result[1]);
            var year = 2000 + Convert.ToInt32(result[2]);
            var newdatetime = new DateTime(year, month, date);


            return newdatetime;
        }

        private List<RoundHistory> GetRoundsV2(string gameUrl)
        {
            var roundWinner = new List<RoundHistory>();
            var xx = gameUrl.Substring(1);
            var prefix = "https://www.hltv.org";

            var fullUrl = prefix + xx;
            bool team1Round1Win = false;
            bool team1Round16Win = false;



            HtmlDocument gameHtml = HWeb.Load(fullUrl);
            var strings = "//*[@class='round-history-team-row']";
            var resultHtml = "//*[@class='round-history-team-row']/*[@class='round-history-half']";
            var results = gameHtml.DocumentNode.SelectNodes(resultHtml);
            var teamnames = gameHtml.DocumentNode.SelectNodes(strings);

            Team1Name = teamnames[0].FirstChild.Attributes["Title"].Value;
            Team2Name = teamnames[1].FirstChild.Attributes["Title"].Value;


            var Team1Firsthalf = results[0].ChildNodes;
            var Team1FirstChildNodes = Team1Firsthalf[0].Attributes["title"].Value;

            var team1Secondhalf = results[1].ChildNodes;
            var team1SecondhalfChildNodes = team1Secondhalf[0].Attributes["title"].Value;

            var team2Firsthalf = results[2].ChildNodes;
            var Team2FirstChildNodes = team2Firsthalf[0].Attributes["title"].Value;

            var team2Secondhalf = results[3].ChildNodes;
            var Team2SecondChildNodes = team2Secondhalf[0].Attributes["title"].Value;

            if (Team1FirstChildNodes.Length > 0)
            {
                team1Round1Win = true;
            }
            if (team1SecondhalfChildNodes.Length > 0)
            {
                team1Round16Win = true;
            }
            var FirstRoundScore = Team1FirstChildNodes.Length > 0 ? Team1FirstChildNodes : Team2FirstChildNodes;
            var SixteenRoundScore = team1SecondhalfChildNodes.Length > 0 ? team1SecondhalfChildNodes : Team2SecondChildNodes;





            var bla = gameHtml.DocumentNode.SelectNodes(strings);

            Team1ID = GetTeamID(teamnames[0].FirstChild.Attributes["src"].Value);
            Team2ID = GetTeamID(teamnames[1].FirstChild.Attributes["src"].Value);


            var rounds = new RoundHistory();
            var round1Teamid = team1Round1Win ? Team1ID : Team2ID;
            var round16Teamid = team1Round16Win ? Team1ID : Team2ID;

            //var rounds = getScore2()


            rounds.TeamId = round1Teamid;
            if (team1Round1Win)
            {
                rounds.CounterTerrorist = true;

            }
            else
            {
                rounds.Terrorist = true;
            }
            rounds.Round1 = true;
            rounds.round = 0;
            roundWinner.Add(rounds);

            var round16 = new RoundHistory();

            round16.TeamId = round16Teamid;
            if (team1Round16Win)
            {
                round16.CounterTerrorist = true;

            }
            else
            {
                round16.Terrorist = true;
            }
            round16.Round16 = true;
            round16.round = 0;
            roundWinner.Add(round16);




            //if (score.Item1 > score.Item2)
            //    {
            //        rounds.TeamId = round1Teamid;
            //        rounds.CounterTerrorist = true;
            //        rounds.Round1 = true;
            //        rounds.round = score.Item1 + score.Item2;
            //    }
            //    roundWinner.Add(rounds);


            //}




            return roundWinner;
        }

        private int GetTeamID(string innerHtml)
        {
            string[] stringSeperators = { "logo/" };
            var htls = innerHtml.Split(stringSeperators, StringSplitOptions.None);
            return Convert.ToInt32(htls[1]);
        }

        private List<RoundHistory> GetRounds(string gameUrl, int team1Id, int team2Id)
        {
            var roundWinner = new List<RoundHistory>();
            HtmlDocument gameHtml = HWeb.Load(gameUrl);

            /*Þetta er fyrsta línan í rounds boxinu, rounds 1-15 (byrjar sem Counter terr )*/
            var upperRow = $"//*[@id='back']/div[3]/div[3]/div/div[5]/div[2]/div/div[6]/div/div[1]/div/div[1]/div[1]/img[1]";
            var upperRowResult = gameHtml.DocumentNode.SelectNodes(upperRow);
            if (upperRowResult != null)
            {
                var rounds = new RoundHistory();
                var score = getScore(upperRowResult[0].OuterHtml);
                if (score.Item1 != int.MinValue)
                {

                    if (score.Item1 > score.Item2)
                    {
                        rounds.TeamId = team1Id;
                        rounds.CounterTerrorist = true;
                        rounds.Round1 = true;
                        rounds.round = score.Item1 + score.Item2;
                    }
                    roundWinner.Add(rounds);


                }
            }
            /*Þetta er fyrsta línan í rounds boxinu, rounds 16-31 (terr ) - Sama á við um lower línuna*/
            upperRow = $"//*[@id='back']/div[3]/div[3]/div/div[5]/div[2]/div/div[6]/div/div[1]/div/div[3]/div[1]/img[1]";



            upperRowResult = gameHtml.DocumentNode.SelectNodes(upperRow);
            if (upperRowResult != null)
            {
                var rounds = new RoundHistory();
                var score = getScore(upperRowResult[0].OuterHtml);
                if (score.Item1 != int.MinValue)
                {

                    rounds.TeamId = team1Id;
                    rounds.Terrorist = true;
                    rounds.Round16 = true;
                    roundWinner.Add(rounds);
                }


            }

            var lowerRow =
                $"//*[@id='back']/div[3]/div[3]/div/div[5]/div[2]/div/div[6]/div/div[1]/div/div[1]/div[2]/img[1]";
            var lowerRowResult = gameHtml.DocumentNode.SelectNodes(lowerRow);
            if (lowerRowResult != null)
            {
                var rounds = new RoundHistory();
                var score = getScore(lowerRowResult[0].OuterHtml);

                if (score.Item1 != int.MinValue)
                {
                    rounds.TeamId = team2Id;
                    rounds.Terrorist = true;
                    rounds.Round1 = true;
                    rounds.round = score.Item1 + score.Item2;
                    roundWinner.Add(rounds);
                }

            }

            lowerRow =
               $"//*[@id='back']/div[3]/div[3]/div/div[5]/div[2]/div/div[6]/div/div[1]/div/div[3]/div[2]/img[1]";
            lowerRowResult = gameHtml.DocumentNode.SelectNodes(lowerRow);
            if (lowerRowResult != null)
            {
                var score = getScore(lowerRowResult[0].OuterHtml);
                if (score.Item1 != int.MinValue)
                {
                    var rounds = new RoundHistory
                    {
                        TeamId = team2Id,
                        CounterTerrorist = true,
                        Round16 = true,
                        round = score.Item1 + score.Item2
                    };
                    roundWinner.Add(rounds);
                }
            }

            return roundWinner;
        }
        private Tuple<int, int> getScore(string outerHtml)
        {
            string[] stringSeperators = { "style=" };
            var returnvalue = new Tuple<int, int>(int.MinValue, int.MinValue);
            var result = outerHtml.Split(stringSeperators, StringSplitOptions.None);
            var result2 = result[0].Substring(12, result[0].Length - 12);
            var res3 = result2.Remove(result2.Length - 1);
            var rounds = res3.Remove(res3.Length - 1, 1);

            if (rounds == "") return returnvalue;
            string[] roundsSeperator = { ":" };
            var splitrounds = rounds.Split(roundsSeperator, StringSplitOptions.None);
            returnvalue = new Tuple<int, int>(Convert.ToInt32(splitrounds[0]), Convert.ToInt32(splitrounds[1]));


            return returnvalue;
        }

        private Tuple<int, int> getScore2(string outerHtml)
        {
            string[] stringSeperators = { "style=" };
            var returnvalue = new Tuple<int, int>(int.MinValue, int.MinValue);
            var result = outerHtml.Split(stringSeperators, StringSplitOptions.None);
            var result2 = result[0].Substring(12, result[0].Length - 12);
            var res3 = result2.Remove(result2.Length - 1);
            var rounds = res3.Remove(res3.Length - 1, 1);

            if (rounds == "") return returnvalue;
            string[] roundsSeperator = { ":" };
            var splitrounds = rounds.Split(roundsSeperator, StringSplitOptions.None);
            returnvalue = new Tuple<int, int>(Convert.ToInt32(splitrounds[0]), Convert.ToInt32(splitrounds[1]));


            return returnvalue;
        }

        private List<Player> GetPlayers(string gameURl, int team1ID, int team2ID)
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

        public double GetRankingValueForTeam(int TeamId, DateTime dDate)
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

        public void GetRankingList(string rankingUrl)
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
        private DateTime GetDateTime(string rankingUrl)
        {
            string[] stringSeperators = { "/teams/" };
            var result = rankingUrl.Split(stringSeperators, StringSplitOptions.None);

            string[] stringsep = { "/" };
            var results = result[1].Split(stringsep, StringSplitOptions.None);

            var dateTimeValue = Convert.ToDateTime(results[1] + "," + results[2] + "," + results[0]);
            var datetime = new DateTime(dateTimeValue.Year, dateTimeValue.Month, dateTimeValue.Day);
            return datetime;

        }

        private void CheckIfNeedToCreateTeam(int TeamId, string TeamName)
        {
            if (!db.Team.Any(s => s.TeamId == TeamId))
            {
                var newTeam = new Team { TeamId = TeamId, TeamName = TeamName };
                db.Team.Add(newTeam);
                db.SaveChanges();
            }
        }

        private Tuple<string, int> GetTeamNameAndResult(string innerText)
        {
            string[] stringSeperators = { "(" };

            var result = innerText.Split(stringSeperators, StringSplitOptions.None);
            var rounds = result[1].Remove(result[1].Length - 1);
            var tupleString = new Tuple<string, int>(result[0], Convert.ToInt32(rounds));
            return tupleString;

        }

        private int GetTeamId(HtmlDocument teamhtml, string htmlstring)
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

        private int GetMatchId(HtmlDocument teamhtml, string htmlstring)
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

        private Tuple<string, int> GetTeamRanking(string innerText)
        {
            string[] stringSeperators = { "(" };
            var newstring = "";
            string results = Regex.Replace(innerText, @"\n\n?|\n", newstring);

            var result = results.Split(stringSeperators, StringSplitOptions.None);
            var rounds = result[1].Remove(result[1].Length - 9);
            var tupleString = new Tuple<string, int>(result[0].TrimStart(), Convert.ToInt32(rounds));
            return tupleString;

        }

        public Tuple<int, int> GetTeamIdsFromUrl(string matchUrls = "")
        {
            string url = "";
            HtmlDocument matchHtml = new HtmlDocument();

            if (matchUrls.Length > 0)
            {
                matchHtml = HWeb.Load(matchUrls);
            }
            else
            {
                matchHtml = HWeb.Load(MatchUrl);
            }



            string[] stringSeperators = { "src=\"https://static.hltv.org/images/team/logo/" };
            string[] SecondSpilt = { "<img alt=\" " };
            string[] thirdSplilt = { "\" class=\"" };
            #region Team1

            var team1IdHtmlString = $"//*[@class='team1-gradient']";
            var urls = $"//*[@class='team1-gradient']/a";

            var team1GradientSection = matchHtml.DocumentNode.SelectNodes(urls);
            var htmlSectionss = matchHtml.DocumentNode.SelectNodes(team1IdHtmlString);
            var Team1Name = htmlSectionss[0].InnerText.TrimStart().TrimEnd();


            string team11Id = GenerateTeamID(stringSeperators, SecondSpilt, thirdSplilt, team1GradientSection);
            #endregion
            #region Team2

            var team2IdHtmlString = $"//*[@class='team2-gradient']";
            var urlsTeam2 = $"//*[@class='team2-gradient']/a";

            var htmlSectionsss = matchHtml.DocumentNode.SelectNodes(urlsTeam2);
            var htmlSectionss2 = matchHtml.DocumentNode.SelectNodes(team2IdHtmlString);
            var Team2Name = htmlSectionss2[0].InnerText.TrimStart().TrimEnd();


            string team2Id = GenerateTeamID(stringSeperators, SecondSpilt, thirdSplilt, htmlSectionsss);
            #endregion

            var tupleString = new Tuple<int, int>(Convert.ToInt32(team11Id), Convert.ToInt32(team2Id));
            return tupleString;
        }

        private static string GenerateTeamID(string[] stringSeperators, string[] SecondSpilt, string[] thirdSplilt, HtmlNodeCollection htmlSectionsss)
        {
            var result = htmlSectionsss[0].OuterHtml.Split(stringSeperators, StringSplitOptions.None);
            var team1Spilt = result[1].Split(SecondSpilt, StringSplitOptions.None);
            var thirdSpilts = team1Spilt[0].Split(thirdSplilt, StringSplitOptions.None);
            var team11Id = thirdSpilts[0];
            return team11Id;
        }

        public class UrlViewModel
        {
            public int id { get; set; }
            public string Url { get; set; }
            public bool BestOf3 { get; set; }

        }

        public int Team1ID { get; set; }
        public int Team2ID { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public static string MatchUrl { get; set; }
    }
}

