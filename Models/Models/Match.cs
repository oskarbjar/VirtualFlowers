using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string PlayerName { get; set; }
        [NotMapped]
        public int TeamID { get; set; }
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
        public bool     FirstRound1HWinCt      { get; set; }
        public bool     FirstRound1HWinTerr { get; set; }
        public bool FirstRound2HWinCT { get; set; }

        public bool FirstRound2HWinTerr { get; set; }

        public int    Team1Id        { get; set; }
        public double Team1RankValue { get; set; }
        public int    T1Player1Id    { get; set; }
        public int    T1Player2Id    { get; set; }
        public int    T1Player3Id    { get; set; }
        public int    T1Player4Id    { get; set; }
        public int    T1Player5Id    { get; set; }
        [NotMapped]
        public double T1FTR;

        public int    Team2Id        { get; set; }
        public double Team2RankValue { get; set; }
        public int    T2Player1Id    { get; set; }
        public int    T2Player2Id    { get; set; }
        public int    T2Player3Id    { get; set; }
        public int    T2Player4Id    { get; set; }
        public int    T2Player5Id    { get; set; }

        public int BombExplosions { get; set; }
        public int BombDefuses    { get; set; }
        public int TimeOut        { get; set; }
        public int KnifeKill      { get; set; }
        public int MolotovKill    { get; set; }
        public int GrenadeKill    { get; set; }
        public int ZuesKill       { get; set; }

        public bool round1BombExplosion  { get; set; }
        public bool round1Defuse         { get; set; }
        public bool round1Timout         { get; set; }
        public bool round1KillWin        { get; set; }
        public bool round16BombExplosion { get; set; }
        public bool round16Defuse        { get; set; }
        public bool round16Timout        { get; set; }
        public bool round16KillWin       { get; set; }
    }

    [NotMapped]
    public class RoundDetail
    {
        public int Team1ID { get; set; }
        public string Team1Name { get; set; }
        public int Team2ID { get; set; }
        public string Team2Name { get; set; }
        public RoundHistory rounds { get; set; }
    }

    [NotMapped]
    public class RoundHistory
    {
        public int R1WinTeamId { get; set; }
        public bool R1WinCt { get; set; }
        public int R16WinTeamId { get; set; }
        public bool R16WinCt { get; set; }

        public int BombExplosions { get; set; }
        public int BombDefuses { get; set; }
        public int TimeOut { get; set; }
        public int KnifeKill { get; set; }
        public int MolotovKill { get; set; }
        public int GrenadeKill { get; set; }
        public int ZuesKill { get; set; }

        public bool round1BombExplosion { get; set; }
        public bool round1Defuse { get; set; }
        public bool round1Timout { get; set; }
        public bool round1KillWin { get; set; }
        public bool round16BombExplosion { get; set; }
        public bool round16Defuse { get; set; }
        public bool round16Timout { get; set; }
        public bool round16KillWin { get; set; }
    }
}
