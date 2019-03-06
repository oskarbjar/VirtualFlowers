using System;
using System.Linq;
using HtmlAgilityPack;
using Models;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Match = Models.Match;
using System.Threading.Tasks;
using System.Net;

namespace VirtualFlowers
{
    public class Program
    {
        static readonly HtmlWeb HWeb = new HtmlAgilityPack.HtmlWeb();
        private readonly DatabaseContext db = new DatabaseContext();

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
      
        /// <summary>
        /// This function goes through mathces page and fetches next 50 matches
        /// </summary>
        /// <returns>List of urls of matches</returns>
        public List<UrlViewModel> GetMatches()
        {

            var Models = new List<UrlViewModel>();
            string url = "https://www.hltv.org/matches";

            var page = new HtmlWeb()
            {
                PreRequest = request =>
                {
                    // Make any changes to the request object that will be used.
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    return true;
                }
            };



            var matchesHtml = page.Load(url);          
            var selection2 = "//div[@class='upcoming-matches']";
            var MatchesCollectionNodes = matchesHtml.DocumentNode.SelectNodes(selection2);
            var MatchesCollection = MatchesCollectionNodes[0].ChildNodes[1].SelectNodes(".//div[@class='match-day']//@href");
            
            var counter = 1;
            foreach (var item in MatchesCollection.Take(52))
            {
                counter++;
                var BestOf3 = "";
                var objectToAdd = new UrlViewModel();
                var bo3 = item.SelectNodes(".//div[@class='map-text']");

                if (bo3 == null)
                {
                    var Mop3Value = item.SelectNodes(".//div[@class='map map-text']");
                    //If this is null, then the match is undecided ( As in winner of match x, faces the winner of match y)
                    if (Mop3Value == null)
                    {
                        BestOf3 = "";
                    }
                    else
                    {
                        BestOf3 = Mop3Value[0].InnerHtml;
                    }


                }
                else
                {
                    var bo3Value = bo3[0].InnerHtml;
                    BestOf3 = bo3Value;
                }
                
                var urls = item.Attributes[0].Value;
                objectToAdd.Url = urls;
                objectToAdd.BestOf3 = (BestOf3 == "bo3") ? true : false; 
                Models.Add(objectToAdd);               
            }           

            return Models;

        }

        public ExpectedLineUp GetTeamLineup(string matchUrls, int team1Id = 0, int team2ID=0)
        {
            try
            {
                var expectedLineUp = new ExpectedLineUp();
                if (expectedLineUp.Players == null)
                {
                    expectedLineUp.Players = new List<Player>();
                }

                //var teamIDs = GetTeamIdsFromUrl();
                var urlHtml = $"[@class='//match-page-link button']";

               

                var url = matchUrls;
                var matchHtml = HWeb.Load(matchUrls);
                var span1Name = new HtmlNodeCollection(HtmlNode.CreateNode(""));
                var span2Name = new HtmlNodeCollection(HtmlNode.CreateNode(""));

                Team1Rank = GetTeamRank(team1Id, Team1Name);

                try
                {
                    // Get Event name
                    string eventspan = $"//*[@class='event text-ellipsis']//@href";
                    var eventnode = matchHtml.DocumentNode.SelectNodes(eventspan);
                    expectedLineUp.EventName = eventnode.Count > 0 ? eventnode[0].InnerHtml : "";

                    // Get MatchId
                    expectedLineUp.MatchId = GetTeamIdFromUrl(url);

                    // Get Start
                    var startUnix = matchHtml.DocumentNode.Descendants("div") // All <a links
                                          .Where(d => d.Attributes.Contains("class") // contains href attribute
                                        && d.Attributes["class"].Value.Contains("date") // with this value
                                        && d.Attributes.Contains("data-unix")) // contains href attribute
                                          .Select(p => p.Attributes["data-unix"].Value).FirstOrDefault(); // return InnerHtml
                    
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    double dUnix;
                    if (!string.IsNullOrEmpty(startUnix) && double.TryParse(startUnix, out dUnix))
                        expectedLineUp.Start = dtDateTime.AddMilliseconds(dUnix).ToLocalTime();

                    string span1;
                    //var team1IdHtmlString = $"//*[@class='team1-gradient']";
                    span1 = $"//*[@class='lineup standard-box']";
                    span1Name = matchHtml.DocumentNode.SelectNodes(span1);
    
                    /*Get players from lineup*/
                    var counter = 0;
                    foreach (var item in span1Name)
                    {
                       /*Players line up box on match overview site*/
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
                                    if (team1Id>0)
                                    {
                                        //teamID that comes from runcompare in home controller
                                        pl.TeamID = team1Id;
                                    }
                                    else
                                    {
                                        pl.TeamID = Team1ID;
                                    }
                                    
                                }
                                else
                                {
                                    if (team1Id > 0)
                                    {
                                        //teamID that comes from runcompare in home controller
                                        pl.TeamID = team2ID;
                                    }
                                    else
                                    {
                                        pl.TeamID = Team2ID;
                                    }

                                }
                               Team2Rank = GetTeamRank(team2ID, Team2Name);

                                var plexists = expectedLineUp.Players.Any(x => x.PlayerId == ids);
                                if (!plexists)
                                {
                                    expectedLineUp.Players.Add(pl);

                                }
                                counter++;
                            }
                        }

                    }
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

        public ExpectedLineUp GetTeamLineupFromDetails(string matchUrls, int team1Id = 0, int team2ID = 0)
        {
            try
            {
                var expectedLineUp = new ExpectedLineUp();
                if (expectedLineUp.Players == null)
                {
                    expectedLineUp.Players = new List<Player>();
                }

                //var teamIDs = GetTeamIdsFromUrl();
                var urlHtml = $"[@class='//match-page-link button']";
                var url = matchUrls;
                var MoreInfo = HWeb.Load(url);
                var span1Name = new HtmlNodeCollection(HtmlNode.CreateNode(""));
                var span2Name = new HtmlNodeCollection(HtmlNode.CreateNode(""));
               
                try
                {
                    int leftTeamID = 0;
                    int rightTeamID = 0;
                    string sLeftTeamId = "";
                    string sRightTeamId = "";
                    
                    string span1;
                    //var team1IdHtmlString = $"//*[@class='team1-gradient']";
                    span1 = $"//*[@class='lineup standard-box']";
                    
                    var spantest = "//*[@class='match-info-box-con']";

                    //span1Name = matchHtml.DocumentNode.SelectNodes(span1);
                    var lineup = MoreInfo.DocumentNode.SelectNodes(spantest);

                    var href = lineup[0].ChildNodes[13].OuterHtml;

                    var externalLink = GetMatchUrlForDetails(href);

                    var externalUrl = "http://www.hltv.org" + externalLink;

                     MoreInfo = HWeb.Load(externalUrl);
                    
                    //*********** GET IDs FROM PAGE ***********
                    var LeftNode = MoreInfo.DocumentNode.SelectSingleNode("//div[@class='team1-gradient']/a"); // divs of class="team-left"
                    if (LeftNode.Attributes.Contains("href"))
                        sLeftTeamId = LeftNode.Attributes["href"].Value;

                    var RightNode = MoreInfo.DocumentNode.SelectSingleNode("//div[@class='team2-gradient']/a"); // divs of class="team-left"
                    if (RightNode.Attributes.Contains("href"))
                        sRightTeamId = RightNode.Attributes["href"].Value;

                    if (!string.IsNullOrEmpty(sLeftTeamId))
                        leftTeamID = GetTeamIDFromUrl(sLeftTeamId);
                    if (!string.IsNullOrEmpty(sRightTeamId))
                        rightTeamID = GetTeamIDFromUrl(sRightTeamId);
                    //*****************************************

                    span1Name = MoreInfo.DocumentNode.SelectNodes(span1);
                    
                    /*Get players from lineup*/
                    var counter = 0;
                    foreach (var item in span1Name)
                    {
                        //td[@class="plaintext"]
                        var players = item.SelectNodes("//*[@class='players']/*[@class='table']//*[@class='player']//@href");

                        foreach (var player in players)
                        {
                            var names = player.InnerText.Trim();
                            //if(string.IsNullOrEmpty(names))
                            //{
                            //    var title = player.Descendants("img").Where(p => p.Attributes["title"].Value.Length > 0).FirstOrDefault();
                            //    names = title.Attributes["title"].Value;
                            //}

                            if (names != "")
                            {

                                var ids = GetPlayerID(player.Attributes[0].Value);
                                var pl = new Player();

                                pl.PlayerId = ids;
                                pl.PlayerName = names;

                                if (counter <= 4)
                                {
                                    if (leftTeamID > 0)
                                        pl.TeamID = leftTeamID;
                                    else
                                        pl.TeamID = Team1ID;
                                }
                                else
                                {
                                    if (rightTeamID > 0)
                                        pl.TeamID = rightTeamID;
                                    else
                                        pl.TeamID = Team2ID;
                                }

                                var plexists = expectedLineUp.Players.Any(x => x.PlayerId == ids);
                                if (!plexists)
                                {
                                    expectedLineUp.Players.Add(pl);

                                }
                                counter++;
                            }
                        }

                    }



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
                string[] secondSplint = { "/" };
                string[] thirdspilt = { "/" };
               
                var ids = outerHtml.Split(secondSplint, StringSplitOptions.None);
                final = Convert.ToInt32(ids[2]);
            }
           
            return final;
        }

        public Task GetTeamDetails(int TeamId)
        {
            DateTime lastScraped = new DateTime(1900, 1, 1);
            // Get ScrapeHistoryTeams record for latest scrape info for this team
            var history = db.ScrapeHistoryTeams.Where(p => p.TeamId == TeamId).OrderByDescending(k => k.LastDayScraped).ToList();
            if (history.Any())
                lastScraped = history.FirstOrDefault().LastDayScraped;

            int lCounter = 0;

            string url = $"" +
                $"http://www.hltv.org/?pageid=188&teamid={TeamId}";

            bool bHasCreatedCurrentTeam = false;
            
              
            HtmlDocument teamhtml = HWeb.Load(url);

            var StatsTable2 = teamhtml.DocumentNode.SelectNodes($"/ html / body / div[2] / div / div[2] / div[1] / div / table / tbody / tr[position()>0]");           
            if (StatsTable2 != null)
            {
                foreach (var item in StatsTable2)
                {
                    var statstableRow = item;

                    var dateString = statstableRow.ChildNodes[1].InnerText;
                    var dDate = NewDate(dateString);
                    if (dDate < lastScraped.AddDays(-1) || dDate < DateTime.Now.AddMonths(-6))
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
                   
                    int MatchID = GetMatchIDS(statstableRow.ChildNodes[1].InnerHtml);
                    if (MatchID == 0 || db.Match.Any(s => s.MatchId == MatchID))
                        continue;

                    var matchUrl = GetMatchUrl(statstableRow.ChildNodes[1].InnerHtml);

                    var xx = matchUrl.Substring(1);
                    var prefix = "https://www.hltv.org";

                    var fullUrl = prefix + xx;

                    var rounds = GetRoundsV2(matchUrl, TeamId);

                    var players = GetTeamLineupFromDetails(fullUrl);
                    if (!players.Players.Any())
                    {
                        db.ErrorLoggers.Add(new ErrorLogger { url = fullUrl, Error = $"{DateTime.Now} - No players, matchid: {MatchID}, date: {dDate}" });
                        db.SaveChanges();
                        // If we don't find any players, we quit.
                        if (lCounter > 0 || !history.Any())
                        {
                            // We save history record when last scraped for this team.
                            db.ScrapeHistoryTeams.Add(new ScrapeHistoryTeams { TeamId = TeamId, LastDayScraped = DateTime.Now });
                            db.SaveChanges();
                        }
                        // And quit scraping
                        return Task.FromResult(0);
                    }

                    var Event = statstableRow.ChildNodes[3].InnerText;
                    var opponent = statstableRow.ChildNodes[7].InnerText;

                    var map = statstableRow.ChildNodes[9].InnerText;
                    var result = getResult(statstableRow.ChildNodes[11].InnerText);
                    var winOrLoss = statstableRow.ChildNodes[13].InnerText;

                    // If we have moved past last scraped date, or year old data                   
                    if (!bHasCreatedCurrentTeam)
                    {
                        CheckIfNeedToCreateTeam(Team1ID, Team1Name);
                        bHasCreatedCurrentTeam = true;
                    }
                    CheckIfNeedToCreateTeam(Team2ID, Team2Name);

                    var bSwitchTeams = Team1ID != TeamId;
                    if (bSwitchTeams)
                    {
                        Team2ID = Team1ID;
                        Team1ID = TeamId;
                    }
                    var match = new Match
                    {
                        MatchId = MatchID,
                        Date = dDate,
                        Map = map,
                        Event = Event,
                        ResultT1 = result.Item1, //Sækja value úr rounds
                        ResultT2 = result.Item2,
                        Team1Id = Team1ID,
                        Team1RankValue = GetRankingValueForTeam(Team1ID, dDate),
                        Team2Id = Team2ID,
                        Team2RankValue = GetRankingValueForTeam(Team2ID, dDate),

                        FirstRound1HWinTeamId = rounds.R1WinTeamId,
                        FirstRound1HWinTerr = rounds.R1WinCt ? false : true,
                        FirstRound1HWinCt = rounds.R1WinCt ? true : false,
                        FirstRound2HWinTeamId = rounds.R16WinTeamId,
                        FirstRound2HWinTerr = rounds.R16WinCt ? false : true,
                        FirstRound2HWinCT = rounds.R16WinCt ? true : false,

                        BombExplosions = rounds.BombExplosions,
                        BombDefuses = rounds.BombDefuses,
                        TimeOut = rounds.TimeOut,
                        GrenadeKill = rounds.GrenadeKill,
                        MolotovKill = rounds.MolotovKill,
                        ZuesKill = rounds.ZuesKill
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

                    if (db.Match.Any(s => s.MatchId == MatchID)) continue;
                    db.Match.Add(match);

                    db.SaveChanges();
                    lCounter++;
                    
                }
            }
            
            // If we have added some records or have no record for this team
            if (lCounter > 0 || !history.Any())
            {
                // We save history record when last scraped for this team.
                db.ScrapeHistoryTeams.Add(new ScrapeHistoryTeams { TeamId = TeamId, LastDayScraped = DateTime.Now });
                db.SaveChanges();
            }
            return Task.FromResult(0);
        }

        private string GetTeamRank(int teamID, string teamName)
        {
            string result = "No rank";
            try
            {
                var rankUrl = $"https://www.hltv.org/team/{teamID}/{teamName.Replace("#", "")}/";

                HtmlDocument rankHtml = HWeb.Load(rankUrl);

                // Get ranked
                var rank = rankHtml.DocumentNode.Descendants("a") // All <a links
                                      .Where(d => d.Attributes.Contains("href") // contains href attribute
                                        && d.Attributes["href"].Value.Contains("/ranking/teams") // with this value
                                        && d.InnerHtml.Contains("#")) // and this text in innerHtml
                                      .Select(p => p.InnerHtml).FirstOrDefault(); // return InnerHtml


                if (!string.IsNullOrEmpty(rank))
                {
                    result = rank;
                }
            }
            catch (Exception ex)
            {
                // do nothing
            }
            return result;
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

        private int GetMatchIDS(string innerHtml)
        {
            int id = 0;
            string[] stringSeperators = { "/" };
            var IdArray = innerHtml.Split(stringSeperators, StringSplitOptions.None);

            if (int.TryParse(IdArray[4], out id))
                return id;
            else
                return 0;
        }

        private string GetMatchUrl(string innerHtml)
        {
            string[] stringSeparators = new string[] { "&amp;" };
            string[] stringSeprators2 = new string[] { "<a href=" };
            var urlsArray = innerHtml.Split(stringSeparators, StringSplitOptions.None);
            var FinalUrl = urlsArray[0].Split(stringSeprators2, StringSplitOptions.None);


            return FinalUrl[1];
        }

        private string GetMatchUrlForDetails(string innerHtml)
        {
            string[] stringSeparators = new string[] { "class=" };
            string[] stringSeprators2 = new string[] { "<a href=" };
            var urlsArray = innerHtml.Split(stringSeparators, StringSplitOptions.None);
            var FinalUrl = urlsArray[0].Split(stringSeprators2, StringSplitOptions.None);

            var finalString = FinalUrl[1].Substring(1);

            return finalString;
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

        private RoundHistory GetRoundsV2(string gameUrl, int TeamId)
        {
            RoundHistory rounds = default(RoundHistory);
            var xx = gameUrl.Substring(1);
            var prefix = "https://www.hltv.org";

            var fullUrl = prefix + xx;
            bool team1Round1Win = false;
            bool team1CtWin = false;
            bool team1Round16Win = false;
            bool team16CtWin = false;
            int NrBombExplosion = 0, NrDefuses = 0, NrTimeout = 0, NrGrenadeKill= 0, NrMolotovKill = 0, NrZeusKill = 0, NrKnifeKill = 0;

            HtmlDocument gameHtml = HWeb.Load(fullUrl);
            var teamNameHtml = "//*[@class='round-history-team-row']";
            var resultHtml = "//*[@class='round-history-team-row']/*[@class='round-history-half']";
            var results = gameHtml.DocumentNode.SelectNodes(resultHtml);
            var teamnames = gameHtml.DocumentNode.SelectNodes(teamNameHtml);

            if (teamnames != null)
            {
                // Get team names and ids
                var Team1Name = teamnames[0].FirstChild.Attributes["Title"].Value;
                var Team2Name = teamnames[1].FirstChild.Attributes["Title"].Value;
                Team1ID = GetTeamID(teamnames[0].FirstChild.Attributes["src"].Value);
                Team2ID = GetTeamID(teamnames[1].FirstChild.Attributes["src"].Value);

                // Count explosion, defuses and timout wins in all history halfs
                foreach (var half in results)
                {
                    NrBombExplosion += half.Descendants("img").Count(p => p.Attributes["src"].Value.Contains("bomb_exploded"));
                    NrDefuses += half.Descendants("img").Count(p => p.Attributes["src"].Value.Contains("bomb_defused"));
                    NrTimeout += half.Descendants("img").Count(p => p.Attributes["src"].Value.Contains("stopwatch"));
                }

                // Get which team won 1st and 16th round
                if (results[0].ChildNodes[0].Attributes["title"].Value.Length > 0) // ef ekki length!
                {
                    team1Round1Win = true;
                }

                if (results[0].ChildNodes[0].Attributes["src"].Value.Contains("bomb_exploded") || results[0].ChildNodes[0].Attributes["src"].Value.Contains("t_win") ||
                    results[2].ChildNodes[0].Attributes["src"].Value.Contains("bomb_exploded") || results[2].ChildNodes[0].Attributes["src"].Value.Contains("t_win"))
                    team1CtWin = false;
                else
                    team1CtWin = true;

                if (results[1].ChildNodes[0].Attributes["title"].Value.Length > 0)
                {
                    team1Round16Win = true;
                }

                if (results[1].ChildNodes[0].Attributes["src"].Value.Contains("bomb_exploded") || results[1].ChildNodes[0].Attributes["src"].Value.Contains("t_win") ||
                    results[3].ChildNodes[0].Attributes["src"].Value.Contains("bomb_exploded") || results[3].ChildNodes[0].Attributes["src"].Value.Contains("t_win"))
                    team16CtWin = false;
                else
                    team16CtWin = true;

                // ************* Get Kills *************
                var sHeatMapUrl = "?showKills=true&showDeaths=false&firstKillsOnly=false&allowEmpty=false&showKillDataset=true&showDeathDataset=false";
                var heatmapUrl = fullUrl.Remove(fullUrl.IndexOf('?')).Replace("/mapstatsid", "/heatmap/mapstatsid") + sHeatMapUrl;
                gameHtml = HWeb.Load(heatmapUrl);
                var playersHtml = "//*[@class='player']";
                var playerResult = gameHtml.DocumentNode.SelectNodes(playersHtml);
                foreach (var player in playerResult)
                {
                    if (player.Descendants("select").Any(p => p.InnerText.Contains("hegrenade")))
                    {
                        var hegrenade = player.Descendants("select").Where(p => p.InnerText.Contains("hegrenade")).FirstOrDefault().InnerText;
                        int.TryParse(hegrenade.Substring(hegrenade.IndexOf("hegrenade (") + "hegrenade (".Length, 1), out int nrHegrenade);
                        NrGrenadeKill += nrHegrenade;
                    }
                    if (player.Descendants("select").Any(p => p.InnerText.Contains("inferno")))
                    {
                        var inferno = player.Descendants("select").Where(p => p.InnerText.Contains("inferno")).FirstOrDefault().InnerText;
                        int.TryParse(inferno.Substring(inferno.IndexOf("inferno (")+ "inferno (".Length, 1), out int nrInferno);
                        NrMolotovKill += nrInferno;
                    }
                    if (player.Descendants("select").Any(p => p.InnerText.Contains("taser")))
                    {
                        var taser = player.Descendants("select").Where(p => p.InnerText.Contains("taser")).FirstOrDefault().InnerText;
                        int.TryParse(taser.Substring(taser.IndexOf("taser (") + "taser (".Length, 1), out int nrTaser);
                        NrZeusKill += nrTaser;
                    }
                    if (player.Descendants("select").Any(p => p.InnerText.Contains("knife")))
                    {
                        // Different kind of knifes, we just increase by 1
                        NrKnifeKill += 1;
                    }
                }

                // Set 
                rounds = new RoundHistory()
                {
                    R1WinTeamId = team1Round1Win ? Team1ID : Team2ID,
                    R1WinCt = team1CtWin,
                    R16WinTeamId = team1Round16Win ? Team1ID : Team2ID,
                    R16WinCt = team16CtWin,
                    BombExplosions = NrBombExplosion,
                    BombDefuses = NrDefuses,
                    TimeOut = NrTimeout,
                    GrenadeKill = NrGrenadeKill,
                    MolotovKill = NrMolotovKill,
                    ZuesKill = NrZeusKill
                };
            }
            return rounds;
        }

        private int GetTeamID(string innerHtml)
        {
            string[] stringSeperators = { "logo/" };
            var htls = innerHtml.Split(stringSeperators, StringSplitOptions.None);
            return Convert.ToInt32(htls[1]);
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


                var tablesHtml = "//*[@class='ranked-team standard-box']";
                var results = rankingHtmlDocument.DocumentNode.SelectNodes(tablesHtml);
               
                for (int i = 0; i < results.Count; i++)
                {
                    var position = results[i].Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("position")).Select(p => p.InnerHtml).FirstOrDefault();
                    var name = results[i].Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("name")).Select(p => p.InnerHtml).FirstOrDefault();
                    var teamlogourl = results[i].Descendants("img").Where(d => d.Attributes.Contains("title") && d.Attributes["title"].Value.Contains(name)).Select(p => p.Attributes["src"].Value).FirstOrDefault();
                    var points = results[i].Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("points")).Select(p => p.InnerHtml).FirstOrDefault();

                    var teamID = int.Parse(teamlogourl.Replace("https://static.hltv.org/images/team/logo/", ""));
                    //var sTeamId = results[i].SelectNodes(".//*[@class='name js-link']");
                    //var teamID = GetTeamIDs(sTeamId[0].OuterHtml);
                    //var points = GetPoints(results[i].SelectNodes(".//*[@class='points']")[0].InnerHtml);
                    var ranking = new Rank
                    {
                        RankPosition = int.Parse(position.Replace("#", "")),
                        Points = int.Parse(points.Replace("(", "").Replace(" points)", "")),
                        TeamId = teamID,
                        RankingListId = rankingListId
                    };
                    db.Rank.Add(ranking);

                }
                    db.SaveChanges();

                

            }
        }

        public void ScrapeRankingListIfNeeded()
        {
            var url = "https://www.hltv.org/ranking/teams";
            var redirectUrl = GrtUrl(url);

            if(!string.IsNullOrEmpty(redirectUrl))
            {
                // And scrape it
                GetRankingList("http://www.hltv.org" + redirectUrl);
            }
        }

        // Temp to get logos
        public void GetImages()
        {
            using (WebClient wc = new WebClient())
            {
                string emptyurl = "https://static.hltv.org/images/team/logo/7002";
                byte[] emptyFileBytes = wc.DownloadData(emptyurl);
                var teamids = db.Team.Where(p => p.TeamId > 7000).OrderBy(s => s.TeamId).Select(k => k.TeamId).ToList();
                foreach (var item in teamids)
                {
                    string remoteFileUrl = "https://static.hltv.org/images/team/logo/" + item;
                    byte[] fileBytes = wc.DownloadData(remoteFileUrl);
                    if (!fileBytes.SequenceEqual(emptyFileBytes))
                    {
                        var extension = "";
                        string fileType = wc.ResponseHeaders[HttpResponseHeader.ContentType];
                        switch (fileType)
                        {
                            case "image/jpeg":
                                extension += ".jpg";
                                break;
                            case "image/svg+xml":
                                extension += ".svg";
                                break;
                            case "image/png":
                                extension += ".png";
                                break;
                            default:
                                break;
                        }

                        string localFileName = @"C:\Users\eysteinne\Source\Repos\VirtualFlowers\VirtualFlowersMVC\Content\Image\teamlogo\" + item + extension;
                        System.IO.File.WriteAllBytes(localFileName, fileBytes);
                    }
                }
            }
        }

        public string GrtUrl(string url)
        {
            string result = "";
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.AllowAutoRedirect = false;  // IMPORTANT

            webRequest.Timeout = 10000;           // timeout 10s
            webRequest.Method = "HEAD";
            // Get the response ...
            HttpWebResponse webResponse;
            using (webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                // Now look to see if it's a redirect
                if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
                {
                    result = webResponse.Headers["Location"];
                    webResponse.Close(); // don't forget to close it - or bad things happen!
                }
            }
            return result;
        }


        /// <summary>
        /// Gets the teamID from the html string 
        /// </summary>
        /// <param name="outerHtml"></param>
        /// <returns></returns>
        private int GetTeamIDs(string outerHtml)
        {
            string[] stringseperator = { "data-url=" };
            string[] secondSpilt = { "/team/" };
            string[] thirdspilt = { "'/'" };

           
            var result = outerHtml.Split(stringseperator, StringSplitOptions.None);
            var result2 = result[1].Split(secondSpilt, StringSplitOptions.None);
            int index = result2[1].IndexOf("/");

            var input = result2[1].Substring(0, index);
            //var result3 = result2[1].Split(thirdspilt, StringSplitOptions.None);
            return Convert.ToInt32( input);
        }

        /// <summary>
        /// Gets the points from ranking list - Removes points text from string and returns the integer number
        /// </summary>
        /// <param name="innerHtml"></param>
        /// <returns></returns>
        private int GetPoints(string innerHtml)
        {
            string[] stringseperator = { "points)" };
            var result = innerHtml.Split(stringseperator, StringSplitOptions.None);
            var finalResult = result[0].Replace("(", string.Empty);
            return Convert.ToInt32(finalResult);
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

        private int GetTeamIDFromUrl(string url)
        {
            string[] stringSeperators = { "/team/" };
            var result = url.Split(stringSeperators, StringSplitOptions.None);

            string[] stringsep = { "/" };
            var results = result[1].Split(stringsep, StringSplitOptions.None);

            int teamId = 0;
            Int32.TryParse(results[0], out teamId);

            return teamId;
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
            Team1Name = htmlSectionss[0].InnerText.TrimStart().TrimEnd();


            string team11Id = GenerateTeamID(stringSeperators, SecondSpilt, thirdSplilt, team1GradientSection);
            int Team1Id = Convert.ToInt32(team11Id);
            CheckIfNeedToCreateTeam(Team1Id, Team1Name);
            #endregion
            #region Team2

            var team2IdHtmlString = $"//*[@class='team2-gradient']";
            var urlsTeam2 = $"//*[@class='team2-gradient']/a";

            var htmlSectionsss = matchHtml.DocumentNode.SelectNodes(urlsTeam2);
            var htmlSectionss2 = matchHtml.DocumentNode.SelectNodes(team2IdHtmlString);
            Team2Name = htmlSectionss2[0].InnerText.TrimStart().TrimEnd();


            string team2Id = GenerateTeamID(stringSeperators, SecondSpilt, thirdSplilt, htmlSectionsss);
            int Team2Id = Convert.ToInt32(team2Id);
            CheckIfNeedToCreateTeam(Team2Id, Team2Name);
            #endregion

            var tupleString = new Tuple<int, int>(Team1Id, Team2Id);
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

        public int GetTeamIdFromUrl(string url)
        {
            // Get MatchId
            var urlSplit = url.Split('/');
            bool next = false;
            int matchid = 0;
            foreach (var str in urlSplit)
            {
                if (next && int.TryParse(str, out matchid))
                    return matchid;
                if (str == "matches")
                    next = true;
            }
            return matchid;
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

        public string Team1Rank { get; set; }
        public string Team2Rank { get; set; }
    }
}

