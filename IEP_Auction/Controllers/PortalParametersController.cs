﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IEP_Auction.Models;
using System.Threading;

namespace IEP_Auction.Views
{
    [Authorize(Roles = "Admin")]
    public class PortalParametersController : Controller
    {
        private IepAuction db = new IepAuction();
        private static IQueryable<PortalParameter> parameters;
        private static PortalParameter currency;
        private static List<PortalParameter> tokenPacks;
        private static int pageSize;

        public static IQueryable<PortalParameter> LoadParameters(IepAuction db)
        {
            parameters = (from p in db.PortalParameters
                          select p);

            currency = parameters.Where(p => p.Type == "Currency").First();
            tokenPacks = parameters.Where(p => p.Type == "TokenPack").OrderBy(p => p.NumValue).ToList();
            pageSize = (int)parameters.Where(p => p.Type == "PageSize").First().NumValue;

            if (pageSize < 1)
                pageSize = 9;
            if (currency.NumValue <= 0)
                currency.NumValue = 1;
            if (tokenPacks.Count() == 0)
            {
                tokenPacks.Add(new PortalParameter() { Type = "TokenPack", NumValue = 10, Name = "Silver", StrValue = "10" });
                tokenPacks.Add(new PortalParameter() { Type = "TokenPack", NumValue = 50, Name = "Gold", StrValue = "50" });
                tokenPacks.Add(new PortalParameter() { Type = "TokenPack", NumValue = 100, Name = "Platinum", StrValue = "100" });
            }
            else {
                foreach(var pack in tokenPacks)
                {
                    if (pack.NumValue <= 0)
                    {
                        pack.NumValue = 1;
                        pack.StrValue = "1";
                    }
                }
            }


            return parameters;
        }

        public static List<PortalParameter> GetType(String type=null, IepAuction db = null)
        {
            if (db == null)
            {
                db = new IepAuction();
            }
            LoadParameters(db);
            
            if(type!=null)
            {
                return parameters.Where(p => p.Type == type).ToList();
            }
            return null;

        }

        public static int GetPageSize(IepAuction db = null)
        {
            Mutex updateCheck = new Mutex(false, "Global/Params");
            if (pageSize == 0) try
            {
                updateCheck.WaitOne();
                if (pageSize == 0)
                    GetType(null, db);
            } finally { updateCheck.ReleaseMutex(); }
            
            return pageSize;
        }

        public static List<PortalParameter> GetTokenPacks(IepAuction db = null)
        {
            Mutex updateCheck = new Mutex(false, "Global/Params");
            if (tokenPacks == null)  try
            {
                updateCheck.WaitOne();
                if (tokenPacks==null)
                    GetType(null, db);
            }
            finally { updateCheck.ReleaseMutex(); }

            return tokenPacks;
        }

        public static PortalParameter GetCurrency(IepAuction db = null)
        {
            Mutex updateCheck = new Mutex(false, "Global/Params");
            if (currency == null)  try
            {
                updateCheck.WaitOne();
                if (currency==null)
                    GetType(null, db);
            }
            finally { updateCheck.ReleaseMutex(); }

            return currency;
        }

        // GET: PortalParameters
        public ActionResult Index()
        {
            return View(db.PortalParameters.OrderBy(p => p.Type).ToList());
        }

        // GET: PortalParameters/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PortalParameter portalParameter = db.PortalParameters.Find(id);
            if (portalParameter == null)
            {
                return HttpNotFound();
            }
            return View(portalParameter);
        }

        // GET: PortalParameters/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PortalParameters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Type,NumValue,StrValue")] PortalParameter portalParameter)
        {
            if (ModelState.IsValid)
            {
                db.PortalParameters.Add(portalParameter);
                db.SaveChanges();
                LoadParameters(db);
                return RedirectToAction("Index");
            }

            return View(portalParameter);
        }

        // GET: PortalParameters/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PortalParameter portalParameter = db.PortalParameters.Find(id);
            if (portalParameter == null)
            {
                return HttpNotFound();
            }
            return View(portalParameter);
        }

        // POST: PortalParameters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Name,Type,NumValue,StrValue")] PortalParameter portalParameter)
        {
            if (ModelState.IsValid)
            {
                db.Entry(portalParameter).State = EntityState.Modified;
                db.SaveChanges();
                LoadParameters(db);
                return RedirectToAction("Index");
            }
            return View(portalParameter);
        }

        // GET: PortalParameters/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PortalParameter portalParameter = db.PortalParameters.Find(id);
            if (portalParameter == null)
            {
                return HttpNotFound();
            }
            return View(portalParameter);
        }

        // POST: PortalParameters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            PortalParameter portalParameter = db.PortalParameters.Find(id);
            db.PortalParameters.Remove(portalParameter);
            db.SaveChanges();
            LoadParameters(db);
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
