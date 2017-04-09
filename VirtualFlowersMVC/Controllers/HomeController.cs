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
using MemoryCache;
using System.Threading.Tasks;

namespace VirtualFlowersMVC.Controllers
{
    public class HomeController : Controller
    {
        private dataWorker _dataWorker = new dataWorker();
        private Program _program = new Program();
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexNew()
        {
            return View();
        }


        // GET: Home/Compare
        [Authorize]
        public ActionResult Compare()
        {
            return View();
        }

        //
        // POST: /Home/Compare
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Compare(CompareStatisticModel model)
        {
            try
            {
                if (model != null)
                {
                    model = await runCompare(model, model.NoCache);
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
        
        public List<string> overviewurls = new List<string>();
        [Authorize(Roles = "Admin")]
        public ActionResult Overview()
        {

            var OverViewList = new List<OverViewViewModel>();

            var result = _program.GetMatches();

            foreach (var item in result)
            {
                var overview = new OverViewViewModel();

                overview.Url = "http://www.hltv.org/" + item;
                overview.Cached = Cache.Exists(overview.Url);
                OverViewList.Add(overview);
                overviewurls.Add(item);
            }
            TempData["UrlList"] = overviewurls;
            return View(OverViewList);

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Overview(List<OverViewViewModel> model)
        {
            var result = _program.GetMatches();
            var PeriodSelection = new List<string>();
            PeriodSelection.Add("3");
            PeriodSelection.Add("6");

            foreach (var item in result)
            {
                var statsModel = new CompareStatisticModel();
                statsModel.MatchUrl = "http://www.hltv.org" + item;
                statsModel.Scrape = true;
                statsModel.PeriodSelection = PeriodSelection;
                await runCompare(statsModel);
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToCompare(string url)
        {          

            var PeriodSelection = new List<string>();
            PeriodSelection.Add("3");
            var model = new CompareStatisticModel();

            if (!url.Contains("http://www.hltv.org/"))
            {
                model.MatchUrl = "http://www.hltv.org/" + url;
            }
            else
            {
                model.MatchUrl = url;
            }
            
          
            model.Scrape = true;
            model.PeriodSelection = PeriodSelection;

            if (url.Length > 0)
            {
                var result = _program.GetTeamIdsFromUrl(url);
                model.ExpectedLineUp = _program.GetTeamLineup(model.MatchUrl);
                model.Team1Id = result.Item1;
                model.Team2Id = result.Item2;
            }
            
            #region "Hægt að nota svona til að scrapa allt"
            
            /*
            foreach (var item in str.Take(3))
            {
                List<string> str = TempData["UrlList"] as List<string>;
                var model = new CompareStatisticModel();
                model.MatchUrl = "http://www.hltv.org/" + item;
                model.Scrape = true;
                model.PeriodSelection = PeriodSelection;

                var result = Program.GetTeamIdsFromUrl(url);
                model.ExpectedLineUp = Program.GetTeamLineup(model.MatchUrl);
                model.Team1Id = result.Item1;
                model.Team2Id = result.Item2;
            }*/
            #endregion
            
            if (model.Team1Id > 0)
                {
                    if (model.Scrape)
                        await _program.GetTeamDetails(model.Team1Id);
                    model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team1Id, model.PeriodSelection, model.ExpectedLineUp));
                }
                if (model.Team2Id > 0)
                {
                    if (model.Scrape)
                        await _program.GetTeamDetails(model.Team2Id);
                    model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team2Id, model.PeriodSelection, model.ExpectedLineUp));
                }

            if (model.Teams != null && model.Teams.Count > 0)
            {
                _dataWorker.GenerateSuggestedMaps(ref model);
            }

            return View(model);
  
        }

        public async Task<CompareStatisticModel> runCompare(CompareStatisticModel model, bool BypassCache = false)
        {
            try
            {
                var CACHEKEY = "";
                if (model != null)
                {
                    if (!string.IsNullOrEmpty(model.MatchUrl) || model.Team1Id > 0 || model.Team2Id > 0)
                    {
                        if (!string.IsNullOrEmpty(model.MatchUrl))
                        {
                            // Create Cachekey from parameters
                            CACHEKEY = "cacheKey:MatchUrl=" + model.MatchUrl;
                            
                            // If we have object in cache, return it
                            if (!BypassCache && Cache.Exists(CACHEKEY))
                                return (CompareStatisticModel)Cache.Get(CACHEKEY);

                            // If we dont have in cache, we continue
                            var result = _program.GetTeamIdsFromUrl(model.MatchUrl);
                            model.ExpectedLineUp = _program.GetTeamLineup(model.MatchUrl);
                            model.Team1Id = result.Item1;
                            model.Team2Id = result.Item2;
                        }
                        else
                            model.ExpectedLineUp = new ExpectedLineUp();
                        if (model.Team1Id > 0)
                        {
                            if (model.Scrape)
                                await _program.GetTeamDetails(model.Team1Id);
                            model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team1Id, model.PeriodSelection, model.ExpectedLineUp));
                        }
                        if (model.Team2Id > 0)
                        {
                            if (model.Scrape)
                                await _program.GetTeamDetails(model.Team2Id);
                            model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team2Id, model.PeriodSelection, model.ExpectedLineUp));
                        }

                        if (model.Teams != null && model.Teams.Count > 0)
                        {
                            _dataWorker.GenerateSuggestedMaps(ref model);
                        }


                        if (!string.IsNullOrEmpty(CACHEKEY)) {
                            if (!Cache.Exists(CACHEKEY))
                            {
                                int storeTime = 1000 * 3600 * 24 * 2; // store 2 days
                                Cache.Store(CACHEKEY, model, storeTime); 
                            }
                            else
                                Cache.Update(CACHEKEY, model);
                        }
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                // just redisplay the form if something failed.
                ModelState.AddModelError("", ex.Message);
                return new CompareStatisticModel();
            }
        }
    }
}