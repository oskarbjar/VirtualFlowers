using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Models;
using VirtualFlowersMVC.Data;
using VirtualFlowers;

namespace VirtualFlowersMVC.Controllers
{
    public class RanksController : Controller
    {
        private DatabaseContext db = new DatabaseContext();
        private dataWorker _dataWorker = new dataWorker();

        // GET: Ranks
        public ActionResult Index()
        {
            return View(db.RankingList.ToList());
        }
        
        public ActionResult NewRank()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewRank(string url)
        {
            // Add to Xml file
            Utility.Utility.AddToRankingListsXml(url);
            
            // And scrape it
            Program.GetRankingList(url);

            return RedirectToAction("Index");
        }

        public ActionResult ScrapeFromXml()
        {
            var RankingList = Utility.Utility.GetRankingListsFromXml();
            foreach (var url in RankingList.Url)
            {
                Program.GetRankingList(url);
            }
            
            return RedirectToAction("Index");
        }

        // GET: Ranks/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var result = _dataWorker.GetRankingList(id);
            return View(result);
        }

        // GET: Ranks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ranks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RankId,RankingListId,RankPosition,TeamId,Points")] Rank rank)
        {
            if (ModelState.IsValid)
            {
                db.Rank.Add(rank);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rank);
        }

        // GET: Ranks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rank rank = db.Rank.Find(id);
            if (rank == null)
            {
                return HttpNotFound();
            }
            return View(rank);
        }

        // POST: Ranks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,RankId,RankingListId,RankPosition,TeamId,Points")] Rank rank)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rank).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rank);
        }

        // GET: Ranks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Rank rank = db.Rank.Find(id);
            if (rank == null)
            {
                return HttpNotFound();
            }
            return View(rank);
        }

        // POST: Ranks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Rank rank = db.Rank.Find(id);
            db.Rank.Remove(rank);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
