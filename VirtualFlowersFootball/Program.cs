using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VirtualFlowersFootball
{
    public class Program
    {

        readonly HtmlWeb HWeb = new HtmlAgilityPack.HtmlWeb();
        static void Main(string[] args)
        {
           // GetBettingTips();
        }

        public List<string>  GetBettingTips()
        {
            var contentList = new List<string>();
            string url = "https://www.thepunterspage.com/category/tips/";

            var matchesHtml = HWeb.Load(url);
            var xxx = $"//div[@class='post_content']";
            var newstring = "";
            var selection = matchesHtml.DocumentNode.SelectNodes(xxx);        
            for (int i = 0; i < selection.Count; i++)
            {
                var bla = selection[i].InnerHtml;
                string results = Regex.Replace(bla, @"\n\n?|\n", newstring);
                contentList.Add(results);
            }

            return contentList;
        }
       
    }
}
