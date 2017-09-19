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
using Newtonsoft.Json;

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
                    model = await runCompare(model);
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
            //ViewBag.Message = "Abot us.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "See some error, some ideas or just tell us how awesome we are, contact below :)";

            return View();
        }

        // public List<string> overviewurls = new List<string>();
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Overview()
        {

            //var OverViewList = new List<OverViewViewModel>();

            var result = _program.GetMatches();
            //foreach (var item in result.Take(52))
            //{
            //    var overview = new OverViewViewModel
            //    {
            //        Url = "http://www.hltv.org" + item.Url,
            //        //overview.Cached = Cache.Exists(overview.Url);
            //        //overview.BestOf3 = item.BestOf3;
            //        Checked = false
            //    };
            //    OverViewList.Add(overview);
            //   // overviewurls.Add(item.Url);

            //}
            ////TempData["UrlList"] = overviewurls;
            //return View(OverViewList);

            var list = new List<OverViewViewModel>();
            var counter = 0;


            foreach (var item in result.Take(52))
            {
                var matchId = _program.GetTeamIdFromUrl(item.Url); 
                var overViewViewModel = new OverViewViewModel { Id = counter++, Url = "http://www.hltv.org" + item.Url, UrlChecked = false, Name = item.Url, BestOf3 = item.BestOf3, ScrapedMatch = _dataWorker.IsMatchScraped(matchId) };

                list.Add(overViewViewModel);

            }
            return View(list);



        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Overview(List<VirtualFlowersMVC.Models.OverViewViewModel> list)
        {
            foreach (var item in list.Where(p => p.UrlChecked).ToList())
            {
                await SendToCompare(item.Name);
            }

            return RedirectToAction("CsIndex");
        }



        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToCompare(string url)
        {
            var scrapedmatchid = 0;
            var PeriodSelection = new List<string>();
            PeriodSelection.Add("3");
            PeriodSelection.Add("6");
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

            // ******* Get Ids and teamlineup *******
            if (url.Length > 0)
            {
                var result = _program.GetTeamIdsFromUrl(model.MatchUrl);
                model.Team1Id = result.Item1;
                model.Team2Id = result.Item2;
                model.ExpectedLineUp = _program.GetTeamLineup(model.MatchUrl, model.Team1Id, model.Team2Id);
            }

            // ******* Scrape *******
            if (model.Team1Id > 0)
            {
                if (model.Scrape)
                    await _program.GetTeamDetails(model.Team1Id);
            }
            if (model.Team2Id > 0)
            {
                if (model.Scrape)
                    await _program.GetTeamDetails(model.Team2Id);
            }

            if (model.Team1Id > 0 && model.Team2Id > 0)
            {
                var secondaryTeam1Id = _dataWorker.GetSecondaryTeamId(model.Team1Id);
                var secondaryTeam2Id = _dataWorker.GetSecondaryTeamId(model.Team2Id);

                List<int> FTR = new List<int> { 0, 4, 5 };
                List<Tuple<int, string>> jsonlist = new List<Tuple<int, string>>();

                // ******* Create model result for each MinFtr and save as json *******
                foreach (int ftr in FTR)
                {
                    model.Teams = new List<TeamStatisticPeriodModel>();
                    model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team1Id, model.PeriodSelection, model.ExpectedLineUp, secondaryTeam1Id, model.NoCache, ftr, _program.Team1Rank));
                    model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team2Id, model.PeriodSelection, model.ExpectedLineUp, secondaryTeam2Id, model.NoCache, ftr, _program.Team2Rank));
                    if (model.Teams != null && model.Teams.Count > 0)
                    {
                        _dataWorker.GenerateSuggestedMaps(ref model);
                    }
                    if (model != null)
                    {
                        jsonlist.Add(new Tuple<int, string>(ftr, JsonConvert.SerializeObject(model)));
                    }
                }

                // ******* If we have any json result we create ScrapedMatch and fill in info *******
                if (jsonlist.Count > 0)
                {
                    ScrapedMatches scrapedMatch = new ScrapedMatches();
                    scrapedMatch.SportName = "CS:GO";
                    scrapedMatch.Event = model.ExpectedLineUp.EventName;
                    scrapedMatch.MatchId = model.ExpectedLineUp.MatchId;
                    scrapedMatch.Start = model.ExpectedLineUp.Start;
                    scrapedMatch.MatchUrl = model.MatchUrl;
                    scrapedMatch.Name = $"{model.Teams[0].TeamName} - {model.Teams[1].TeamName}";
                    if (jsonlist.Any(p => p.Item1 == 4))
                        scrapedMatch.Json4MinFTR = jsonlist.Single(p => p.Item1 == 4).Item2;
                    if (jsonlist.Any(p => p.Item1 == 5))
                        scrapedMatch.Json5MinFTR = jsonlist.Single(p => p.Item1 == 5).Item2;
                    if (jsonlist.Any(p => p.Item1 == 0))
                        scrapedMatch.Json = jsonlist.Single(p => p.Item1 == 0).Item2;
                    scrapedmatchid = _dataWorker.AddScrapedMatch(scrapedMatch, -1);
                }
            }

            return RedirectToAction("LoadCompare", new { id = scrapedmatchid, MinFTR = 0 });

        }

        public async Task<CompareStatisticModel> runCompare(CompareStatisticModel model)
        {
            try
            {
                if (model != null)
                {
                    if (!string.IsNullOrEmpty(model.MatchUrl) || model.Team1Id > 0 || model.Team2Id > 0)
                    {
                        if (!string.IsNullOrEmpty(model.MatchUrl))
                        {
                            Program.MatchUrl = model.MatchUrl;
                            var result = _program.GetTeamIdsFromUrl(model.MatchUrl);

                            model.Team1Id = result.Item1;
                            model.Team2Id = result.Item2;
                            model.ExpectedLineUp = _program.GetTeamLineup(model.MatchUrl, model.Team1Id, model.Team2Id);
                        }
                        else
                            model.ExpectedLineUp = new ExpectedLineUp();
                        if (model.Team1Id > 0)
                        {
                            var secondaryTeamId = _dataWorker.GetSecondaryTeamId(model.Team1Id);

                            if (model.Scrape)
                                await _program.GetTeamDetails(model.Team1Id);
                            model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team1Id, model.PeriodSelection, model.ExpectedLineUp, secondaryTeamId, model.NoCache, model.MinFullTeamRanking, _program.Team1Rank));
                        }
                        if (model.Team2Id > 0)
                        {
                            var secondaryTeamId = _dataWorker.GetSecondaryTeamId(model.Team2Id);

                            if (model.Scrape)
                                await _program.GetTeamDetails(model.Team2Id);
                            model.Teams.Add(await _dataWorker.GetTeamPeriodStatistics(model.Team2Id, model.PeriodSelection, model.ExpectedLineUp, secondaryTeamId, model.NoCache, model.MinFullTeamRanking, _program.Team2Rank));
                        }

                        if (model.Teams != null && model.Teams.Count > 0)
                        {
                            _dataWorker.GenerateSuggestedMaps(ref model);
                        }


                        if (model != null)
                        {
                            ScrapedMatches scrapedMatch = new ScrapedMatches();
                            scrapedMatch.SportName = "CS:GO";
                            scrapedMatch.Event = model.ExpectedLineUp.EventName;
                            scrapedMatch.MatchId = model.ExpectedLineUp.MatchId;
                            scrapedMatch.Start = model.ExpectedLineUp.Start;
                            scrapedMatch.MatchUrl = model.MatchUrl;
                            scrapedMatch.Name = $"{model.Teams[0].TeamName} - {model.Teams[1].TeamName}";
                            if (model.MinFullTeamRanking == 4)
                                scrapedMatch.Json4MinFTR = JsonConvert.SerializeObject(model);
                            else if (model.MinFullTeamRanking == 5)
                                scrapedMatch.Json5MinFTR = JsonConvert.SerializeObject(model);
                            else
                                scrapedMatch.Json = JsonConvert.SerializeObject(model);
                            _dataWorker.AddScrapedMatch(scrapedMatch, model.MinFullTeamRanking);
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


        // GET: CsIndex
        public ActionResult CsIndex()
        {
            int ShowHourHistory = -6;
            var result = _dataWorker.GetScrapedMatches(ShowHourHistory);

            return View(result);
        }

        // GET: LoadCompare
        public ActionResult LoadCompare(int id, int MinFTR = 0)
        {
            CompareStatisticModel model = new CompareStatisticModel();
            ScrapedMatches match = new ScrapedMatches();
            // Create Cachekey from parameters
            var CACHEKEY = $"cacheKey:MatchId={id}-MinFTR={MinFTR}";

            // If we have object in cache, return it
            if (Cache.Exists(CACHEKEY))
                model = (CompareStatisticModel)Cache.Get(CACHEKEY);
            else
            {
                match = _dataWorker.GetScrapedMatch(id);

                if (match != null)
                {
                    if (MinFTR == 5)
                        model = JsonConvert.DeserializeObject<CompareStatisticModel>(match.Json5MinFTR);
                    else if (MinFTR == 4)
                        model = JsonConvert.DeserializeObject<CompareStatisticModel>(match.Json4MinFTR);
                    else
                        model = JsonConvert.DeserializeObject<CompareStatisticModel>(match.Json);
                }

                // Save in cache
                if (!string.IsNullOrEmpty(CACHEKEY) && !Cache.Exists(CACHEKEY))
                {
                    int storeTime = 1000 * 3600 * 24 * 2; // store 2 days
                    Cache.Store(CACHEKEY, model, storeTime);
                }
            }

            model.ScrapeMatchId = id;
            model.MinFullTeamRanking = MinFTR;
            return View(model);
        }
    }
}



