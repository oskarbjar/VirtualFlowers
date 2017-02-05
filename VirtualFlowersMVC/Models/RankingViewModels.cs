using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualFlowersMVC.Models
{
    public class RankViewModel
    {
        public int      Id           { get; set; }
        public int      RankPosition { get; set; }
        public int      TeamId       { get; set; }
        public string   TeamName     { get; set; }
        public int      Points       { get; set; }
    }
}