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
            List<PortalParameter> options = PortalParametersController.GetTokenPacks(db);
            options.Sort((a, b) => a.NumValue > b.NumValue?1:-1);
            foreach (var option in options)
                items.Add(new SelectListItem() {
                    Text = option.Name + " - " + option.StrValue + " tokens",
                    Value = option.StrValue
                });

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
            TokenOrder newOrder = new TokenOrder();
            newOrder.Id = Guid.NewGuid();
            newOrder.UserId = User.Identity.GetUserId();
            newOrder.Amount = int.Parse(PurchaseOptions);
            newOrder.Value = newOrder.Amount;
            newOrder.Currency = "EUR";
            newOrder.Time = DateTime.Now;
            newOrder.Status = "SUBMITTED";

            db.TokenOrders.Add(newOrder);
            db.SaveChanges();


            return Redirect("https://stage.centili.com/payment/widget?apikey=91063299dc643a1994d0becac101275b&country=rs&userid=" + newOrder.Id);
        }

        public ActionResult ConfirmOrder(string userid, string status)
        {
            //return new JsonResult() { Data = new { id = Request.QueryString.ToString()}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.TokenOrders.Find(Guid.Parse(userid));
                    if (order == null)
                    {
                        transaction.Dispose();
                        ViewBag.ErrMsg = "Order doesn't exist";
                        return RedirectToAction("Index", "TokenOrders");
                    }

                    if (order.Status != "SUBMITTED")
                    {
                        transaction.Dispose();
                        ViewBag.ErrMsg = "Order already processed";
                        return RedirectToAction("Index", "TokenOrders");
                    }

                    if (status.Equals("success"))
                    {
                        var balance = db.Balances.Find(order.AspNetUser.Id);
                        if (balance == null)
                        {
                            balance = new Models.Balance();
                            balance.Id = order.AspNetUser.Id;
                            balance.Tokens = 0;
                            db.Balances.Add(balance);
                        }

                        balance.Tokens += order.Amount;
                        ViewBag.ErrMsg = "Order confirmed.";
                        order.Status = "COMPLETED";
                    }
                    else
                    {
                        ViewBag.ErrMsg = "Order failed.";
                        order.Status = "CANCELED";
                    }
                    db.SaveChanges();
                    transaction.Commit();
                }catch
                {
                    transaction.Rollback();
                    ViewBag.ErrMsg = "Confirmation failed.";
                }
            }

            return RedirectToAction("Index", "TokenOrders");
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
