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
    public class KuvatController : Controller
    {
        private KirjastoProjektiEntities1 db = new KirjastoProjektiEntities1();

        // GET: Kuvat
        public ActionResult Index()
        {
            return View(db.Kuvat.ToList());
        }



        // GET: Kuvat/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Kuvat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "KuvaId,AltText,ImagePath,Image,kuva")] Kuvat kuvat)
        {
            if (ModelState.IsValid)
            {
                int count = Request.Files.Count;
                var file = Request.Files[0];
                string filename = file.FileName;
                byte[] buffer = new byte[file.InputStream.Length];
                file.InputStream.Read(buffer, 0, (int)file.InputStream.Length);

                db.Entry(kuvat).State = EntityState.Added;
                kuvat.Image = buffer;
                kuvat.ImagePath = filename;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(kuvat);
        }

        // GET: Kuvat/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Kuvat kuvat = db.Kuvat.Find(id);
            if (kuvat == null)
            {
                return HttpNotFound();
            }
            return View(kuvat);
        }

        // POST: Kuvat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "KuvaId,AltText,ImagePath,Image,kuva")] Kuvat kuvat)
        {
            if (ModelState.IsValid)
            {
                int count = Request.Files.Count;
                var file = Request.Files[0];
                string filename = file.FileName;
                byte[] buffer = new byte[file.InputStream.Length];
                file.InputStream.Read(buffer, 0, (int)file.InputStream.Length);



                db.Entry(kuvat).State = EntityState.Modified;
                kuvat.Image = buffer;
                kuvat.ImagePath = filename;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(kuvat);
        }

        // GET: Kuvat/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Kuvat kuvat = db.Kuvat.Find(id);
            if (kuvat == null)
            {
                return HttpNotFound();
            }
            return View(kuvat);
        }

        // POST: Kuvat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Kuvat kuvat = db.Kuvat.Find(id);
            db.Kuvat.Remove(kuvat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        //lista josta voi valita kuvia koordinaatteihin
        // GET: Kuvat
        public ActionResult ValitseKuva(int? id, int? kuvaID)
        {
            return View(db.Kuvat.ToList());
        }

        //actionresult joka asettaa valitun kuvan kordinaattiin
        public ActionResult AsetaKuva()
        {
            return View(db.Kuvat.ToList());
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
