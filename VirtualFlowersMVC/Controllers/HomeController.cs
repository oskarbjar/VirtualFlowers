using VirtualFlowersMVC.Data;
using VirtualFlowersMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualFlowers;
using VirtualFlowersMVC.Utility;
using Models;

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
        
        //
        // POST: /Home/Compare
        [HttpPost]
        public ActionResult Compare(CompareStatisticModel model)
        {
            try
            {
                if (model != null)
                {
                    if (!string.IsNullOrEmpty(model.MatchUrl))
                    {
                        var result = Program.GetTeamIdsFromUrl(model.MatchUrl);
                        model.ExpectedLineUp = Program.GetTeamLineup(model.MatchUrl);
                        model.Team1Id = result.Item1;
                        model.Team2Id = result.Item2;
                    }
                    else
                        model.ExpectedLineUp = new ExpectedLineUp();
                    if (model.Team1Id > 0)
                    {
                        if(model.Scrape)
                            Program.GetTeamDetails(model.Team1Id);
                        model.Teams.Add(_dataWorker.GetTeamPeriodStatistics(model.Team1Id, model.PeriodSelection, model.ExpectedLineUp));
                    }
                    if (model.Team2Id > 0)
                    {
                        if (model.Scrape)
                            Program.GetTeamDetails(model.Team2Id);
                        model.Teams.Add(_dataWorker.GetTeamPeriodStatistics(model.Team2Id, model.PeriodSelection, model.ExpectedLineUp));
                    }

                    if (model.Teams != null && model.Teams.Count > 0)
                    {
                        _dataWorker.GenerateSuggestedMaps(ref model);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // just redisplay the form if something failed.
                ModelState.AddModelError("", ex.Message);
                return View();
            }
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