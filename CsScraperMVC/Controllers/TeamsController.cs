using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VirtualFlowersMVC.Data;

namespace VirtualFlowersMVC.Controllers
{
    public class TeamsController : Controller
    {
        private dataWorker _dataWorker = new dataWorker();
        
        // GET: Teams
        public ActionResult Index()
        {
            var result = _dataWorker.GetTeamsList();

            return View(result);
        }

        // GET: Teams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var team = _dataWorker.GetTeamDetails(id);

            if (team == null)
            {
                return HttpNotFound();
            }
            return View(team);
        }
    }
}
