using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace VirtualFlowersMVC.Utility
{
    public static class Utility
    {
        public static RankingList GetRankingListsFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RankingList));
            var dezerializedList = new RankingList();

            using (FileStream stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/RankingLists.xml"))
            {
                dezerializedList = (RankingList)serializer.Deserialize(stream);
            }

            return dezerializedList;
        }

        public static bool AddToRankingListsXml(string RankingListUrl)
        {
            var result = false;
            XmlSerializer serializer = new XmlSerializer(typeof(RankingList));
            var dezerializedRankingList = new RankingList();

            using (FileStream stream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/RankingLists.xml"))
            {
                dezerializedRankingList = (RankingList)serializer.Deserialize(stream);
            }

            dezerializedRankingList.Url.Add(RankingListUrl);

            using (FileStream stream = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory + "/RankingLists.xml"))
            {
                serializer.Serialize(stream, dezerializedRankingList);
                result = true;
            }

            return result;
        }
    }

    [XmlRoot(ElementName = "RankingList")]
    public class RankingList
    {
        [XmlElement(ElementName = "Url")]
        public List<string> Url { get; set; }
    }
}