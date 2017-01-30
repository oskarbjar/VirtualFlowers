using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Models;
using CsScraperMVC.Models;

namespace CsScraperMVC.Controllers
{
    public class MatchesController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        // GET: Matches
        public ActionResult Index(int TeamId)
        {
            var query = db.Match.Where(k => k.Team1Id == TeamId || k.Team2Id == TeamId).ToList();
                
                var matches = (from p in query
                        select new MatchesViewModel
                        {
                            Id = p.Id,
                            MatchId = p.MatchId,
                            Date = p.Date,
                            Map = p.Map,
                            Event = p.Event,
                            ResultT1 = p.ResultT1,
                            ResultT2 = p.ResultT2,
                            Team1Name = db.Team.FirstOrDefault(k => k.TeamId == p.Team1Id).TeamName,
                            Team2Name = db.Team.FirstOrDefault(k => k.TeamId == p.Team2Id).TeamName
                        }).ToList();
            return View(matches);
        }

        // GET: Matches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Match match = db.Match.Find(id);
            if (match == null)
            {
                return HttpNotFound();
            }
            return View(match);
        }
    }
}
