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
    [Authorize(Roles = "Admin")]
    public class PortalParametersController : Controller
    {
        private IepAuction db = new IepAuction();
        private static IQueryable<PortalParameter>  parameters;

        public static IQueryable<PortalParameter> LoadParameters(IepAuction db)
        {
            parameters = (from p in db.PortalParameters
                          select p);
            return parameters;
        }

        public static List<PortalParameter> GetCurrencies(IepAuction db=null)
        {
            if(parameters == null)
            {
                if(db == null)
                {
                    throw new InvalidExpressionException();
                }
                LoadParameters(db);
            }

            return parameters.Where(p => p.Type == "Currency").ToList();
        }

        public static List<PortalParameter> GetTokenPacks(IepAuction db = null)
        {
            if (parameters == null)
            {
                if (db == null)
                {
                    throw new InvalidExpressionException();
                }
                LoadParameters(db);
            }

            return parameters.Where(p => p.Type == "TokenPack").ToList();
        }

        public static string GetBaseCurrency(IepAuction db = null)
        {
            if (parameters == null)
            {
                if (db == null)
                {
                    throw new InvalidExpressionException();
                }
                LoadParameters(db);
            }

            return parameters.Where(p => p.Type == "BasePortalCurrency").First().StrValue;
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
