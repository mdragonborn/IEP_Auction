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
using Microsoft.AspNet.SignalR;

namespace IEP_Auction.Views
{
    public class AuctionsController : Controller
    {
        private IepAuction db = new IepAuction();
        private Hubs.NotificationHub notificationContext = new Hubs.NotificationHub();

        // GET: Auctions
        public ActionResult Index()
        {
            var auction = db.Auctions.Include(a => a.Bid);
            return View(auction.ToList());
        }

        // GET: Auctions/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }

            return View(auction);
        }

        // GET: Auctions/Create
        public ActionResult Create()
        {
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
                    DurationMinutes = (int)auctionData.AuctionLength.TotalMinutes,
                    LastBidId = null,
                    Status = "READY",
                    Name = auctionData.Name,
                    Description = auctionData.Description,
                    ImagePath = imgPath
                };

                var initBid = new Bid
                {
                    Time = DateTime.Now,
                    Amount = auctionData.InitialPrice,
                    UserId = User.Identity.GetUserId()
                };
                try
                {
                    db.Auctions.Add(auction);
                    db.Bids.Add(initBid);
                    db.SaveChanges();

                    auction.LastBidId = initBid.Id;

                    var auctionInitBit = new BidAuction
                    {
                        BidId = initBid.Id,
                        AuctionId = auction.Id
                    };

                    db.BidAuctions.Add(auctionInitBit);
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

                // redirect to auction page ?
                return RedirectToAction(auction.Id.ToString(), "Auctions/Details");
            }
            ViewBag.errmsg = "model validation fail";
            return View(auctionData);
        }


        // GET: Auctions/Bid
        public ActionResult Bid(Guid guid)
        {
            ViewBag.AuctionGuid = guid;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Bid(CreateBidModel auctionData)
        {
            ViewBag.AuctionGuid = auctionData.AuctionGuid;
            if (ModelState.IsValid)
            {

                using (var transaction = db.Database.BeginTransaction())
                {
                    Auction auction = db.Auctions.Find(auctionData.AuctionGuid);
                    if (auction == null)
                    {
                        ViewBag.ErrMsg = "Auction doesn't exist";
                        transaction.Dispose();
                        return View(auctionData);
                    }
                    if (auction.Bid.Amount >= auctionData.TokenAmount)
                    {
                        ViewBag.ErrMsg = "Bid must be larger than last bid. Current bid: " + auction.Bid.Amount + " tokens.";
                        transaction.Dispose();
                        return View(auctionData);
                    }

                    var previousBid = auction.Bid;

                    var prevReservation = db.Reservations.Find(previousBid.Id, previousBid.UserId);

                    if (prevReservation != null)
                    {
                        prevReservation.AspNetUser.Balance.Tokens += prevReservation.Amount;
                        db.Reservations.Remove(prevReservation);
                    }

                    var userId = User.Identity.GetUserId();
                    Balance balance = db.Balances.Find(userId);

                    if (balance == null || balance.Tokens <= auctionData.TokenAmount)
                    {
                        ViewBag.ErrMsg = "Insufficient funds.";
                        transaction.Dispose();
                        return View(auctionData);
                    }


                    Bid bid = new Models.Bid {
                        Amount = auctionData.TokenAmount,
                        Time = DateTime.Now,
                        UserId = userId,
                    };

                    balance.Tokens -= auctionData.TokenAmount;
                    db.Bids.Add(bid);
                    db.SaveChanges();

                    BidAuction bidAuction = new BidAuction
                    {
                        BidId = bid.Id,
                        AuctionId = auction.Id
                    };
                    db.BidAuctions.Add(bidAuction);

                    Reservation reservation = new Reservation
                    {
                        UserId = userId,
                        Amount = auctionData.TokenAmount,
                        BidId = bid.Id
                    };

                    db.Reservations.Add(reservation);

                    auction.LastBidId = bid.Id;

                    db.SaveChanges();

                    try
                    {
                        transaction.Commit();
                        notificationContext.NotifyAll(new { auction = auction.Id, price = bid.Amount, user = userId }, "NewBid");                        
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ViewBag.ErrMsg = "Error. Please try again.";
                        return View(auctionData);
                    }
                    return RedirectToAction("Details", "Auctions", new { Id = auctionData.AuctionGuid });
                }
            }
            ViewBag.ErrMsg = "Amount can't be empty.";
            return View(auctionData);
        }

        [HttpPost]
        public JsonResult Start(string guid) {
            if (User.IsInRole("Admin"))
            {
                Auction auction = db.Auctions.Find(new Guid(guid));
                if (auction == null)
                    return Json(new { status = "WrongGuid" });
                auction.TimeStart = DateTime.Now;
                auction.TimeEnd = auction.TimeStart + new TimeSpan(0, auction.DurationMinutes, 0);
                auction.Status = "OPENED";
                db.SaveChanges();
                return Json(new { status = "Success" });
            }
            return Json(new { status = "NotAuthorized" });
        }

        // GET: Auctions/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            ViewBag.LastBidId = new SelectList(db.Bids, "Id", "UserId", auction.LastBidId);
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
            ViewBag.LastBidId = new SelectList(db.Bids, "Id", "UserId", auction.LastBidId);
            return View(auction);
        }

        // GET: Auctions/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
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
            Auction auction = db.Auctions.Find(id);
            db.Auctions.Remove(auction);
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
