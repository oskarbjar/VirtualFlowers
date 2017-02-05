using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Models;
using VirtualFlowersMVC.Data;

namespace VirtualFlowersMVC.Controllers
{
    public class MatchesController : Controller
    {
        private dataWorker _dataWorker = new dataWorker();

        // GET: Matches
        public ActionResult Index(int TeamId)
        {
            var result = _dataWorker.GetMatches(TeamId);

            return View(result);
        }

        // GET: Matches/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var match = _dataWorker.GetMatchDetails(id);

            if (match == null)
            {
                return HttpNotFound();
            }
            return View(match);
        }

    }
}
