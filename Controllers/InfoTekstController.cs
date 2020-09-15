using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KirjastoAppScrum.Models;

namespace KirjastoAppScrum.Controllers
{
    public class InfoTekstController : Controller
    {
        private KirjastoProjektiEntities1 db = new KirjastoProjektiEntities1();

        // GET: InfoTekst
        public ActionResult Index()
        {
            var infoTekstit = db.InfoTekstit.Include(i => i.Tekstit);
            return View(infoTekstit.ToList());
        }

        // GET: InfoTekst/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InfoTekstit infoTekstit = db.InfoTekstit.Find(id);
            if (infoTekstit == null)
            {
                return HttpNotFound();
            }
            return View(infoTekstit);
        }

        // GET: InfoTekst/Create
        public ActionResult Create(int? id)
        {


            ViewBag.Infotext_ID = new SelectList(db.Tekstit.Where(t=>t.Kategoria.Class<3), "TekstiID", "Teksti");
            return View();
        }

        // POST: InfoTekst/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Infotext_ID,InfotextContent")] InfoTekstit infoTekstit)
        {
            if (ModelState.IsValid)
            {
                db.InfoTekstit.Add(infoTekstit);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Infotext_ID = new SelectList(db.Tekstit, "TekstiID", "KieliID", infoTekstit.Infotext_ID);
            return View(infoTekstit);
        }

        // GET: InfoTekst/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InfoTekstit infoTekstit = db.InfoTekstit.Find(id);
            if (infoTekstit == null)
            {
                return HttpNotFound();
            }
            ViewBag.Infotext_ID = new SelectList(db.Tekstit, "TekstiID", "KieliID", infoTekstit.Infotext_ID);
            return View(infoTekstit);
        }

        // POST: InfoTekst/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Infotext_ID,InfotextContent")] InfoTekstit infoTekstit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(infoTekstit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Infotext_ID = new SelectList(db.Tekstit, "TekstiID", "KieliID", infoTekstit.Infotext_ID);
            return View(infoTekstit);
        }

        // GET: InfoTekst/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InfoTekstit infoTekstit = db.InfoTekstit.Find(id);
            if (infoTekstit == null)
            {
                return HttpNotFound();
            }
            return View(infoTekstit);
        }

        // POST: InfoTekst/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InfoTekstit infoTekstit = db.InfoTekstit.Find(id);
            db.InfoTekstit.Remove(infoTekstit);
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
