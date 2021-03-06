﻿using System;
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
using System.Threading;
using LinqKit;

namespace IEP_Auction.Views
{
    public class AuctionsController : Controller
    {
        private IepAuction db = new IepAuction();
        private Hubs.NotificationHub notificationContext = new Hubs.NotificationHub();

        static DateTime lastUpdate;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var time = DateTime.Now.ToUniversalTime();
            Mutex updateCheck = new Mutex(false, "Global\\Update");
            try
            {
                updateCheck.WaitOne();

                if (time - lastUpdate < TimeSpan.FromSeconds(1))
                    return;
                lastUpdate = time;
            } finally
            {
                updateCheck.ReleaseMutex();
            }
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var finishedAuctions = (from a in db.Auctions
                                            where a.Status == "OPENED" && a.TimeEnd < time
                                            select a).ToList();

                    foreach(Auction auction in finishedAuctions)
                    {
                        auction.Status = "CLOSED";
                        notificationContext.NewBid(auction.Id.ToString(), new { status="CLOSED" }, "statuschange");
                        notificationContext.NotifyAll(new { auction = auction.Id, status = "CLOSED" }, "statuschange");

                        if (auction.Bid != null && auction.Bid.AspNetUser != auction.AspNetUser)
                        {
                            long amount = (long)auction.Bid.Reservations.ElementAt(0).Amount;
                            db.Reservations.Remove(auction.Bid.Reservations.ElementAt(0));
                            auction.AspNetUser.Balance.Tokens += amount;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        // private bool containsAny(string SearchString, string Name)
        // {
        //     var tokens = SearchString.Split(' ');
        //     if (tokens.Count() == 0) return true;
        //     foreach(string token in tokens)
        //     {
        //         if (Name.Contains(token)) return true;
        //     }

        //     return false;
        // }

        // GET: Auctions
        public ActionResult Index(String SearchString, String auctionState, String minPrice, String maxPrice, int? page)
        {
            int pageSize = PortalParametersController.GetPageSize(db);
            PortalParameter currency = PortalParametersController.GetCurrency(db);
            ViewBag.currencyValue = currency.NumValue;
            ViewBag.currencySymbol = currency.StrValue;

            int min = minPrice==null||minPrice==""?0:int.Parse(minPrice);
            int max = maxPrice==null || maxPrice=="" ? 0:int.Parse(maxPrice);
            var predicate = PredicateBuilder.New<Auction>();
            if (SearchString != null && SearchString != "")
            {
                ViewBag.prevString = SearchString;
                var tokens = SearchString.Split(' ');
                predicate.Or(a => SearchString == "");
                foreach (string token in tokens)
                {
                    predicate = predicate.Or(a => a.Name.Contains(token));
                }
            }
            if (auctionState != null && auctionState != "")
            {
                ViewBag.selectedState = auctionState;
                predicate = predicate.And(a => a.Status == auctionState);
            }
            else ViewBag.selectedState = "";
            if (min != 0 || max != 0)
            {
                ViewBag.prevMin = min;
                ViewBag.prevMax = max;
                predicate = predicate.And(a => a.Bid.Amount * currency.NumValue >= min && a.Bid.Amount * currency.NumValue <= max);
            }
            IQueryable<Auction> auction;
            if (predicate.IsStarted)
                auction = db.Auctions.AsExpandable().Where(predicate);
            else
                auction = db.Auctions;
            if (auction.Count() > 0)
            {
                ViewBag.pageCount = ((auction.Count()+pageSize-1) / pageSize)>0? (auction.Count() + pageSize - 1) / pageSize:1;
                if (page == null || page <= 0) page = 1;
                if (page > ViewBag.pageCount) page = ViewBag.pageCount;
                ViewBag.pageNumber = page;
                if (auction.Count() < pageSize)
                    pageSize = auction.Count();
                int limit = auction.Count() - (pageSize * ((int)page - 1));
                if (limit > pageSize) limit = pageSize;
                return View(auction.ToList().GetRange(pageSize * ((int)page - 1), limit));
            }
            else
            {
                ViewBag.pageCount = 0;
                ViewBag.pageNumber = 0;
                return View(auction.ToList());
            }
        }

        // GET: Auctions/Details/5
        public ActionResult Details(Guid? id)
        {
            PortalParameter currency = PortalParametersController.GetCurrency(db);
            ViewBag.currencyValue = currency.NumValue;
            ViewBag.currencySymbol = currency.StrValue;

            var detailsModel = new DetailsModel();
            ViewBag.guid = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }

            detailsModel.Auction = auction;
            var bids = (from a in db.BidAuctions
                        where a.AuctionId == id && a.Bid.UserId != a.Auction.CreatorId
                        select a);
            bids = bids.OrderByDescending(a => a.Bid.Time);
            detailsModel.Bids = bids.ToList();

            return View(detailsModel);
        }

        // GET: Auctions/Create
        [System.Web.Mvc.Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize]
        public ActionResult Create(CreateAuctionModel auctionData)
        {
            if (ModelState.IsValid)
            {
                var file = auctionData.File;
                
                var id = Guid.NewGuid();
                byte[] imageFile = new byte[file.ContentLength];
                file.InputStream.Read(imageFile, 0, file.ContentLength);
                //var imgPath = id + file.FileName;
                //file.SaveAs(Server.MapPath("~" + imgPath));

                var auction = new Auction
                {
                    Id = id,
                    CreatorId = User.Identity.GetUserId(),
                    DurationMinutes = (int)auctionData.AuctionLength.TotalMinutes,
                    LastBidId = null,
                    Status = "READY",
                    Name = auctionData.Name,
                    Description = auctionData.Description,
                    ImagePath = file.FileName,
                    ImageFile = imageFile
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
        [System.Web.Mvc.Authorize]
        public ActionResult Bid(Guid guid)
        {
            Auction a = db.Auctions.Find(guid);
            ViewBag.AuctionGuid = guid;
            if (a == null)
            {
                ViewBag.ErrMsg = "Auction doesn't exist.";
                return View();
            }
            CreateBidModel model = new CreateBidModel()
            {
                AuctionGuid = guid,
                TokenAmount = a.Bid.Amount.Value + 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize]
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
                    if(auction.CreatorId == User.Identity.GetUserId())
                    {
                        ViewBag.ErrMsg = "Cannot bid on your own auction.";
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
                    AspNetUser currentUser = db.AspNetUsers.Find(userId);
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
                        notificationContext.NotifyAll(new { auction = auction.Id, price = bid.Amount, user = currentUser.Email }, "NewBid");
                        notificationContext.NewBid(auction.Id.ToString(), new { price = bid.Amount, user = currentUser.Email, time = bid.Time.ToString() }, "newbid");
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
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public JsonResult Start(string guid) {
            Auction auction = db.Auctions.Find(new Guid(guid));
            if (auction == null)
                return Json(new { status = "WrongGuid" });
            using (var transaction = db.Database.BeginTransaction())
            {
                auction.TimeStart = DateTime.Now.ToUniversalTime();
                auction.TimeEnd = auction.TimeStart + new TimeSpan(0, auction.DurationMinutes, 0);
                auction.Status = "OPENED";

                notificationContext.NewBid(auction.Id.ToString(), new { status = "OPENED", time = auction.TimeEnd.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") }, "statuschange");
                notificationContext.NotifyAll(new { auction = auction.Id, status = "OPENED", time = auction.TimeEnd.Value.ToString("yyyy-MM-dd'T'HH:mm:ss") }, "statuschange");

                db.SaveChanges();

                try
                {
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    ViewBag.ErrMsg = "Error. Please try again.";
                    return Json(new { status = "Error" });
                }
            }
            return Json(new { status = "Success" });
        }

        // GET: Auctions/Edit/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
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
        [System.Web.Mvc.Authorize(Users = "Admin")]
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
            if(auction.Status=="OPENED")
            {
                ViewBag.ErrMsg = "Cannot delete an auction in progress.";
                return View(auction);
            }
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
