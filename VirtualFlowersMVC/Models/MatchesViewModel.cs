using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualFlowersMVC.Models
{
    public class MatchesViewModel
    {
        public int      Id                      { get; set; }
        public int      MatchId                 { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime Date                    { get; set; }
        public string   Map                     { get; set; }
        public string   Event                   { get; set; }
        public int      ResultT1                { get; set; }
        public int      ResultT2                { get; set; }
        public string   FirstRound1HWinTeamName { get; set; }
        public string   FirstRound2HWinTeamName { get; set; }
        public string   FirstRound1HWinCtTerr   { get; set; }
        public string   FirstRound2HWinCtTerr   { get; set; }

        public int    Team1Id       { get; set; }
        public string Team1Name     { get; set; }
        public string T1Player1Name { get; set; }
        public string T1Player2Name { get; set; }
        public string T1Player3Name { get; set; }
        public string T1Player4Name { get; set; }
        public string T1Player5Name { get; set; }

        public int    Team2Id       { get; set; }
        public string Team2Name     { get; set; }
        public string T2Player1Name { get; set; }
        public string T2Player2Name { get; set; }
        public string T2Player3Name { get; set; }
        public string T2Player4Name { get; set; }
        public string T2Player5Name { get; set; }
    }


    public class OverViewViewModel
    {   [Key]
        public int Id { get; set; } 
        public string Url { get; set; }
        public bool Cached { get; set; }

        public bool BestOf3 { get; set; }

        public bool Checked { get; set; }

        public string Name { get; set; }


    }

    public class UrlViewModel
    {
        public int id { get; set; }
        public string Url { get; set; }
        public bool BestOf3 { get; set; }


    }
}