using CsScraperMVC.Data;
using CsScraperMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CsScraperMVC.Controllers
{
    public class HomeController : Controller
    {
        private dataWorker _dataWorker = new dataWorker();

        public ActionResult Index()
        {
            return View();
        }


        // GET: Home/Compare
        public ActionResult Compare()
        {            
            return View();
        }

        //
        // POST: /Home/Compare
        [HttpPost]
        public ActionResult Compare(CompareStatisticModel model)
        {
            if (model != null)
            {
                if(model.Team1Id > 0)
                {
                    model.Teams.Add(_dataWorker.GetTeamPeriodStatistics(model.Team1Id));
                }
                if (model.Team2Id > 0)
                {
                    model.Teams.Add(_dataWorker.GetTeamPeriodStatistics(model.Team2Id));
                }
            }

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}