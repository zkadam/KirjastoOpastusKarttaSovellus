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
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {


                return View("Index","_Layout_Admin",db.Kuvat.ToList());

            }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }



        // GET: Kuvat/Create
        public ActionResult Create()
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {

                return View("Create","_Layout_Admin");


            }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }

        // POST: Kuvat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "KuvaId,AltText,ImagePath,Image,kuva")] Kuvat kuvat)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
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

                return View("Create", "_Layout_Admin",kuvat);
            }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }

        // GET: Kuvat/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
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
            return View("Edit", "_Layout_Admin",kuvat);

            }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }

        // POST: Kuvat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "KuvaId,AltText,ImagePath,Image,kuva")] Kuvat kuvat)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
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
                return View("Edit", "_Layout_Admin", kuvat);
                }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }

        // GET: Kuvat/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
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
                return View("Delete", "_Layout_Admin", kuvat);
                }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }

        // POST: Kuvat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                Kuvat kuvat = db.Kuvat.Find(id);
                db.Kuvat.Remove(kuvat);
                db.SaveChanges();
                return RedirectToAction("Index", "_Layout_Admin");
            }
            else if (Session["PerusUser"] != null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }


        //lista josta voi valita kuvia koordinaatteihin
        // GET: Kuvat
        public ActionResult ValitseKuva(int? id, int? kuvaID, string kategoria)
        {
            //laitetaan viewbagin koordinaatti id ja kuva id että viewissä voimme valita joka on tän hetkeinen kuva ja että koord-idn voimme lehettää seuraavaalle actionille
            if (id!=null)
            {
                ViewBag.KoordId = id;
                ViewBag.Kategoria = kategoria;
            }
            else
            {
                return RedirectToAction("Index","Admin", "_Layout_Admin");
            }
            if (kuvaID!=null)
            {
                ViewBag.kuvaID = kuvaID;
            }
            else
            {//kuvaid ei voi olla nulla, tää vain helpottaa koodin kirjoittaminen
                ViewBag.kuvaID = 0;
            }


            return View("ValitseKuva","_Layout_Admin", db.Kuvat.ToList());
        }



        //actionresult joka asettaa valitun kuvan kordinaattiin - sitten palauttaa indexillä
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsetaKuva(int? id, int? kuvaID)
        {
            if (id!=null&&kuvaID!=null)
            {
                try
                {
                    Koordinaatit koordinaatti = db.Koordinaatit.Find(id);
                    var kat = db.Kategoria.Find(id);
                    koordinaatti.kuvaID = kuvaID;
                    db.Entry(koordinaatti).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index","Admin", new { kategoriaRefer = kat.ReferTo });
                }
                catch (Exception)
                {

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
               
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        //Action result joka poista valitun kuvan linkki Koordinaateista
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PoistaKuva(int? id,  string kategoria)
        {
            if (id != null)
            {
                try
                {
                    Koordinaatit koordinaatti = db.Koordinaatit.Find(id);
                    koordinaatti.kuvaID = null;
                    db.Entry(koordinaatti).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("ValitseKuva", "Kuvat", new { id=koordinaatti.KoordinaattiID, kategoria=kategoria });
                }
                catch (Exception)
                {

                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        //------------------------------------------------------PARTIAL VIEW KUVIEN NÄYTTÄMISEN VARTEN

        // GET
        public ActionResult _NaytaKuva(int? id)
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
            return PartialView(kuvat);
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
