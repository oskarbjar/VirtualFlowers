﻿using VirtualFlowersMVC.Data;
using VirtualFlowersMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualFlowers;
using VirtualFlowersMVC.Utility;

namespace VirtualFlowersMVC.Controllers
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

        // GET: Home/CompareTeams
        [HttpPost]
        public ActionResult CompareTeams(string matchURl)
        {
            var compareStatisticModel = new CompareStatisticModel();
            var result = Program.GetTeamIdsFromUrl(matchURl);


            if (result != null)
            {
                var model = new CompareStatisticModel
                {
                    Team1Id = result.Item1,
                    Team2Id = result.Item2

                };

                compareStatisticModel = model;
            }


            return RedirectToAction("compare", new { CompareStatisticModel = compareStatisticModel});
        }

        //
        // POST: /Home/Compare
        [HttpPost]
        public ActionResult Compare(CompareStatisticModel model)
        {
            if (model != null)
            {
                if (model.Team1Id > 0)
                {
                    Program.GetTeamDetails(model.Team1Id);
                    model.Teams.Add(_dataWorker.GetTeamPeriodStatistics(model.Team1Id));
                }
                if (model.Team2Id > 0)
                {
                    Program.GetTeamDetails(model.Team2Id);
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