using System.Data.Entity;
using System.Web.Mvc;
using Models;
using Models.Models;

namespace VirtualFlowersMVC.Controllers
{
    public class FootballTipsController : Controller
    {
        private DatabaseContext db = new DatabaseContext();
        private VirtualFlowersFootball.Program _program = new VirtualFlowersFootball.Program();

        // GET: FootballTips
        public ActionResult Index()
        {

            var content = _program.GetBettingTips();
            return View(content);
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
