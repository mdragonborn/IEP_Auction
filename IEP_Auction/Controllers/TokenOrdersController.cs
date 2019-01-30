using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using IEP_Auction.Models;

namespace IEP_Auction.Views
{
    public class TokenOrdersController : Controller
    {
        private IepAuction db = new IepAuction();

        // GET: TokenOrders
        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var tokenOrders = db.TokenOrders.Include(t => t.AspNetUser).Where(t => t.AspNetUser.Id == userId ).OrderByDescending(t => t.Time);
            return View(tokenOrders.ToList());
        }

        // GET: TokenOrders/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "FirstName");
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Silver - 10 tokens", Value = "10" });
            items.Add(new SelectListItem() { Text = "Gold - 50 tokens", Value = "50" });
            items.Add(new SelectListItem() { Text = "Platinum - 100 tokens", Value = "100" });

            ViewBag.PurchaseOptions = items;

            return View();
        }

        // POST: TokenOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(String PurchaseOptions)
        {
            TokenOrder newOrder = db.TokenOrders.Create();
            newOrder.Id = Guid.NewGuid();
            newOrder.UserId = User.Identity.GetUserId();
            newOrder.Amount = int.Parse(PurchaseOptions);
            newOrder.Time = DateTime.Now;
            newOrder.Status = "SUBMITTED";

            db.TokenOrders.Add(newOrder);
            db.SaveChanges();

            return RedirectToAction("Index", "TokenOrders");
        }
        
        public JsonResult ConfirmOrder(Guid orderId)
        {
            var order = db.TokenOrders.Find(orderId);
            if(order == null)
            {
                return new JsonResult() { Data = new { status = "Order doesn't exist." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            if(order.Status != "SUBMITTED")
            {
                return new JsonResult() { Data = new { status = "Order already processed." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            var balance = db.Balances.Find(order.AspNetUser.Id);
            if(balance == null)
            {
                balance = new Models.Balance();
                balance.Id = order.AspNetUser.Id;
                balance.Tokens = 0;
                db.Balances.Add(balance);
            }

            balance.Tokens += order.Amount;
            order.Status = "COMPLETED";
            db.SaveChanges();

            return new JsonResult() { Data = new { status = "Order confirmed." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult CancelOrder(Guid orderId)
        {
            var order = db.TokenOrders.Find(orderId);
            if (order == null)
            {
                return new JsonResult() { Data = new { status = "Order doesn't exist." } , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            if (order.Status != "SUBMITTED")
            {
                return new JsonResult() { Data = new { status = "Order already processed." } , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            order.Status = "CANCELED";
            db.SaveChanges();

            return new JsonResult() { Data = new { status = "Order canceled." } , JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
