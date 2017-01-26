using System;
using System.Linq;
using HtmlAgilityPack;
using Models;

namespace CsScraper
{
    class Program
    {
        static readonly HtmlWeb HWeb = new HtmlAgilityPack.HtmlWeb();
        private static readonly DatabaseContext db = new DatabaseContext();
        static void Main(string[] args)
        {
            Console.WriteLine("Enter TeamID");
            var input = Console.ReadLine();
            
            string url = $"http://www.hltv.org/?pageid=188&statsfilter=5&teamid={input}";
            HtmlDocument teamhtml = HWeb.Load(url);
            for (int i = 5; i < 2074; i++)
            {

                i++;
                var htmlstring = $"//*[@id='back']/div[3]/div[3]/div/div[3]/div/div[{i}]/div";
                var htmlSectionss =
               teamhtml.DocumentNode.SelectNodes(htmlstring);

                var team1IdHtmlString = htmlstring + "/a[2]";
                var team2IdHtmlString = htmlstring + "/a[3]";
                var nodes = htmlSectionss?[0].SelectNodes(".//div");

                if (nodes?.Count() == 8)
                {

                    var team1Id = GetTeamID(teamhtml, team1IdHtmlString);
                    var team2Id = GetTeamID(teamhtml, team2IdHtmlString);
                    var team1Name = GetTeamNameAndResult(nodes[1].InnerText);
                    var team2Name = GetTeamNameAndResult(nodes[2].InnerText);

                    var testString = nodes[0].InnerText;//date
                    var teststring3 = nodes[3].InnerText;//map
                    var teststring4 = nodes[4].InnerText;//event

                    var teams = new Teams
                    {
                        Date = Convert.ToDateTime(testString),
                        Team1 = team1Name.Item1,
                        Team2 = team2Name.Item1,
                        Map = teststring3,
                        Event = teststring4,
                        ResultT1 = team1Name.Item2,
                        ResultT2 = team2Name.Item2,
                        Team1Id = team1Id,
                        Team2Id =  team2Id
                    };
                    db.Teams.Add(teams);
                    db.SaveChanges();
                }
            }

            string RankingUrl = "http://www.hltv.org/ranking/teams/2017/january/2/";
            HtmlDocument rankingHtmlDocument = HWeb.Load(RankingUrl);

            for (int i = 2; i < 31; i++)
            {
                var rankingUrlStringNo = $"//*[@id='back']/div[3]/div[3]/div/div[{i}]";
                var team = rankingHtmlDocument.DocumentNode.SelectNodes(rankingUrlStringNo);
            }

        }

        private static Tuple<string,int> GetTeamNameAndResult(string innerText)
        {
            string[] stringSeperators = {"("};

            var result = innerText.Split(stringSeperators, StringSplitOptions.None);
            var rounds = result[1].Remove(result[1].Length - 1);
            var tupleString = new Tuple<string, int>(result[0],Convert.ToInt32(rounds));
            return tupleString;

        }

        private static int GetTeamID(HtmlDocument teamhtml, string htmlstring)
        {
            string[] stringSeparators = new string[] { "&amp;" };
          
            var teamid1 = teamhtml.DocumentNode.SelectNodes(htmlstring);
            var teamid = teamid1[0].Attributes["href"].Value;
            var result = teamid.Split(stringSeparators, StringSplitOptions.None);
            var teamids = result[1].Substring(7);
            return Convert.ToInt32(teamids);
        }
    }
}
