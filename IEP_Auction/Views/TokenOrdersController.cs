using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IEP_Auction.Models;

namespace IEP_Auction.Views
{
    public class TokenOrdersController : Controller
    {
        private IepAuction db = new IepAuction();

        // GET: TokenOrders
        public ActionResult Index()
        {
            var tokenOrders = db.TokenOrders.Include(t => t.AspNetUser);
            return View(tokenOrders.ToList());
        }

        // GET: TokenOrders/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TokenOrder tokenOrder = db.TokenOrders.Find(id);
            if (tokenOrder == null)
            {
                return HttpNotFound();
            }
            return View(tokenOrder);
        }

        // GET: TokenOrders/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "FirstName");
            return View();
        }

        // POST: TokenOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Status,UserId,Amount,Time")] TokenOrder tokenOrder)
        {
            if (ModelState.IsValid)
            {
                tokenOrder.Id = Guid.NewGuid();
                db.TokenOrders.Add(tokenOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "FirstName", tokenOrder.UserId);
            return View(tokenOrder);
        }

        // GET: TokenOrders/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TokenOrder tokenOrder = db.TokenOrders.Find(id);
            if (tokenOrder == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "FirstName", tokenOrder.UserId);
            return View(tokenOrder);
        }

        // POST: TokenOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Status,UserId,Amount,Time")] TokenOrder tokenOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tokenOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "FirstName", tokenOrder.UserId);
            return View(tokenOrder);
        }

        // GET: TokenOrders/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TokenOrder tokenOrder = db.TokenOrders.Find(id);
            if (tokenOrder == null)
            {
                return HttpNotFound();
            }
            return View(tokenOrder);
        }

        // POST: TokenOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            TokenOrder tokenOrder = db.TokenOrders.Find(id);
            db.TokenOrders.Remove(tokenOrder);
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
