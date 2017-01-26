using System;
using System.Linq;
using HtmlAgilityPack;
using Models;

namespace CsScraper
{
    class Program
    {
        static HtmlWeb hWeb = new HtmlAgilityPack.HtmlWeb();
        private static readonly DatabaseContext db = new DatabaseContext();
        static void Main(string[] args)
        {
            string url = "http://www.hltv.org/?pageid=188&statsfilter=5&teamid=6615";
            HtmlDocument teamhtml = hWeb.Load(url);
            var htmlSections = teamhtml.DocumentNode.SelectNodes("//*[@id='back']/div[3]/div[3]/div/div[3]/div");
            var bla = htmlSections.Nodes();
            //*[@id="back"]/div[3]/div[3]/div/div[3]/div/div[6]
            for (int i = 1; i < 2074; i++)
            {
                  i++;
                var htmlstring = $"//*[@id='back']/div[3]/div[3]/div/div[3]/div/div[{i}]/div";
                var htmlSectionss =
               teamhtml.DocumentNode.SelectNodes(htmlstring);
                var blala =  htmlSectionss?[0].SelectNodes(".//div");
                
                if (blala?.Count() == 8)
                {

                    var testString = blala[0].InnerText;//date
                    var teststring1 = blala[1].InnerText;//Team1
                    var teststring2 = blala[2].InnerText;//team2
                    var teststring3 = blala[3].InnerText;//map
                    var teststring4 = blala[4].InnerText;//event
                    var teststring5 = blala[5].InnerText;//event - No need to save
                    var teststring6 = blala[6].InnerText;//Result
                    var teststring7 = blala[7].InnerText;

                    var teams = new Teams
                    {
                        Date = Convert.ToDateTime(testString),
                        Team1 = teststring1,
                        Team2 = teststring2,
                        Map = teststring3,
                        Event = teststring4,
                        Result = teststring6,
                        TeamId = 6615

                    };

                    db.Teams.Add(teams);
                    db.SaveChanges();

                }
            }
          
            string RankingUrl = "http://www.hltv.org/ranking/teams/2017/january/2/";
            HtmlDocument rankingHtmlDocument = hWeb.Load(RankingUrl);

            for (int i = 2; i < 31; i++)
            {
                var rankingUrlStringNo = $"//*[@id='back']/div[3]/div[3]/div/div[{i}]";
                var team = rankingHtmlDocument.DocumentNode.SelectNodes(rankingUrlStringNo);
            }

        }
    }
}
