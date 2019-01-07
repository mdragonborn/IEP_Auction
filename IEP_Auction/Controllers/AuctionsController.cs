using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using IEP_Auction.Models;
using System.Diagnostics;

namespace IEP_Auction.Views
{
    public class AuctionsController : Controller
    {
        private IepAuction db = new IepAuction();

        // GET: Auctions
        public ActionResult Index()
        {
            var auction = db.Auction.Include(a => a.Bid);
            return View(auction.ToList());
        }

        // GET: Auctions/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // GET: Auctions/Create
        public ActionResult Create()
        {
            ViewBag.LastBidId = new SelectList(db.Bid, "Id", "UserId");
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateAuctionModel auctionData)
        {
            if (ModelState.IsValid)
            {
                var file = auctionData.File;
                var id = Guid.NewGuid();
                var imgPath = "/auctionItems/" + id + file.FileName;
                file.SaveAs(Server.MapPath("~" + imgPath));

                var auction = new Auction
                {
                    Id = id,
                    CreatorId = User.Identity.GetUserId(),
                    TimeStart = DateTime.Now,
                    TimeEnd = DateTime.Now + auctionData.AuctionLength,
                    Status = "OPENED",
                    LastBidId = null,
                    Name = auctionData.Name,
                    Description = auctionData.Description,
                    ImagePath = imgPath
                };

                var initBid = new Bid
                {
                    Time = auction.TimeStart,
                    Amount = auctionData.InitialPrice,
                    UserId = User.Identity.GetUserId()
                };
                try
                {
                    db.Auction.Add(auction);
                    db.Bid.Add(initBid);
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    string msg = "";
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        msg += eve.Entry.Entity.GetType().Name + eve.Entry.State + "\n";
                        foreach (var ve in eve.ValidationErrors)
                        {
                            msg += ve.PropertyName + ve.ErrorMessage + "\n";
                        }
                    }
                    ViewBag.errmsg = msg;
                    return RedirectToAction(auction.Id.ToString(), "Auctions/Details");
                }

                var auctionInitBit = new BidAuction
                {
                    BidId = initBid.Id,
                    AuctionId = auction.Id
                };

                db.BidAuction.Add(auctionInitBit);
                db.SaveChanges();
                // redirect to auction page ?
                return RedirectToAction(auction.Id.ToString(), "Auctions/Details");
            }
            ViewBag.errmsg = "model validation fail";
            return View(auctionData);
        }

        // GET: Auctions/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            ViewBag.LastBidId = new SelectList(db.Bid, "Id", "UserId", auction.LastBidId);
            return View(auction);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CreatorId,Status,TimeStart,TimeEnd,LastBidId,Name,Description,ImagePath")] Auction auction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LastBidId = new SelectList(db.Bid, "Id", "UserId", auction.LastBidId);
            return View(auction);
        }

        // GET: Auctions/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // POST: Auctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Auction auction = db.Auction.Find(id);
            db.Auction.Remove(auction);
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
