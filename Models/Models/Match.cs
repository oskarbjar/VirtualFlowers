using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Team
    {
        public int    Id       { get; set; }
        public int    TeamId   { get; set; }
        public string TeamName { get; set; }
    }

    public class Player
    {
        public int Id         { get; set; }
        public int PlayerId   { get; set; }
        public int PlayerName { get; set; }
    }

    public class Match
    {
        public int      Id                    { get; set; }
        public int      MatchId               { get; set; }
        public DateTime Date                  { get; set; }
        public string   Map                   { get; set; }
        public string   Event                 { get; set; }
        public int      ResultT1              { get; set; }
        public int      ResultT2              { get; set; }
        public int      FirstRound1HWinTeamId { get; set; }
        public int      FirstRound2HWinTeamId { get; set; }
        public string   FirstRound1HWinCtTerr { get; set; }
        public string   FirstRound2HWinCtTerr { get; set; }

        public int    Team1Id        { get; set; }
        public double Team1RankValue { get; set; }
        public int    T1Player1Id    { get; set; }
        public int    T1Player2Id    { get; set; }
        public int    T1Player3Id    { get; set; }
        public int    T1Player4Id    { get; set; }
        public int    T1Player5Id    { get; set; }

        public int    Team2Id        { get; set; }
        public double Team2RankValue { get; set; }
        public int    T2Player1Id    { get; set; }
        public int    T2Player2Id    { get; set; }
        public int    T2Player3Id    { get; set; }
        public int    T2Player4Id    { get; set; }
        public int    T2Player5Id    { get; set; }
    }
}
