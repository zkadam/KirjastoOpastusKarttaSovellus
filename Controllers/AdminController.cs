using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KirjastoAppScrum.Models;
using Microsoft.Ajax.Utilities;

namespace KirjastoAppScrum.Controllers
{
    public class AdminController : Controller
    {
        KirjastoProjektiEntities1 db = new KirjastoProjektiEntities1();

        public static bool indexAll; // tehdään staattinen muuttuja jolla kerrotaan kummalle indexille palautetaan RJ
        public ActionResult Index(int? kategoriaRefer, string itemName)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null || Session["PerusUser"] != null)
            {
                indexAll = false; // Palautetaan muutokset Index.cshtml sivulle RJ

                var lista = from t in db.Tekstit.Include(t => t.Kategoria).Include(t => t.Kategoria.Koordinaatit)
                            select t;
                //kovakoodattu nyt admin kategorioiden muokkauskieleksi suomi RJ           
                lista = lista.Where(t => t.KieliID == "FI" && t.Kategoria.ReferTo == kategoriaRefer);

                //tehdään lista jolla tarkastetaan onko kategorialla vielä porautumista ja viedään se ViewBagina Index-sivulle RJ
                List<int?> kategLista = new List<int?>();
                foreach (var listaItem in db.Kategoria)
                    kategLista.Add(listaItem.ReferTo);

                // otetaan kategorioiden Masterin nimi talteen linq kyselyllä ja viedään Index sivulle. RJ
                var kategoriaNimi = from n in db.Tekstit.Include(n => n.Kategoria)
                                    .Where(n => n.KieliID == "FI" && n.Kategoria.KategoriaID == kategoriaRefer)
                                    select n.Teksti;

                var kategoriaTulostus = from n in db.Tekstit.Include(n => n.Kategoria)
                                     .Where(n => n.KieliID == "FI" && n.Kategoria.KategoriaID == kategoriaRefer)
                                        select n.Kategoria.SN.ToString();

                ViewBag.Title = itemName; // Itemin title
                ViewBag.kategoriaRefer = kategoriaRefer; // kategorianReferi
                ViewBag.kategoriaReferiNimi = kategoriaNimi.FirstOrDefault(); // MasterinNimi
                ViewBag.listaData = kategLista; // listaData porautumisen tarkastukseen RJ
                ViewBag.kategoriaTulostus = kategoriaTulostus.FirstOrDefault(); ; // otetaan kategorian Luokka välitykseen RJ

                return View(lista);
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }

        public ActionResult IndexAll(string itemName)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null || Session["PerusUser"] != null)
            {
                indexAll = true;  // Palautetaan muutokset IndexAll.cshtml sivulle RJ

                //tehdään lista jolla saadaan kategoriat ja niiden ID tiedot referTo nimen tulostusta varten IndexAll sivulle
                Dictionary<int?, string> referToList = new Dictionary<int?, string>();
                foreach (var listaItem in db.Kategoria.Include(c => c.Tekstit))
                    referToList.Add(listaItem.KategoriaID, listaItem.Tekstit.Where(t => t.KieliID == "FI").SingleOrDefault().Teksti);
                ViewBag.referToData = referToList; // listaData porautumisen tarkastukseen RJ

                var lista = from t in db.Tekstit.Include(t => t.Kategoria).Include(t => t.Kategoria.Koordinaatit)
                            select t;
                //kovakoodattu nyt admin kategorioiden muokkauskieleksi suomi RJ           
                lista = lista.Where(t => t.KieliID == "FI");

                ViewBag.Title = itemName; // Itemin title
                return View(lista);
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Sisaankirjautuminen", "Admin");
            }
        }
        //--------------------- KOORDINAATIT ------------------------
        // mvc autoomaatti koordinaattien lisääminen toteutus ZK
        public ActionResult AddCoords(string itemName, int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                ViewBag.KoordinaattiID = id;
                //välitetään itemin nimi viewbagin kautta että admin näkee mitä hän on lisäämässä ZK
                ViewBag.itemName = itemName;
                return View();
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

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCoords([Bind(Include = "KoordinaattiID,startX,startY,width,height,radius,kerros")] Koordinaatit koordinaatit, int? referi)
        {
            if (ModelState.IsValid)
            {
                db.Koordinaatit.Add(koordinaatit);
                db.SaveChanges();

                // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
                if (indexAll == false)
                {
                    return RedirectToAction("Index", new { kategoriaRefer = referi });
                }
                else
                {
                    return RedirectToAction("IndexAll");
                }
            }

            ViewBag.KoordinaattiID = new SelectList(db.Kategoria, "KategoriaID", "SN", koordinaatit.KoordinaattiID);
            return View(koordinaatit);
        }

        // Koordinaattien muokkausta RJ
        public ActionResult EditCoords(string itemName, int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Koordinaatit koords = db.Koordinaatit.Find(id);
                if (koords == null)
                {
                    return HttpNotFound();
                }
                // välitetään itemin nimi viewbagin kautta että admin näkee mitä hän on lisäämässä/muokkamassa/poistamassa ZK
                ViewBag.itemName = itemName;

                return View(koords);
            }
            else
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCoords([Bind(Include = "KoordinaattiID,startX,startY,width,height,radius,kerros")] Koordinaatit koords, int? referi)
        {
            if (ModelState.IsValid)
            {
                db.Entry(koords).State = EntityState.Modified;
                db.SaveChanges();

                // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
                if (indexAll == false)
                {
                    return RedirectToAction("Index", new { kategoriaRefer = referi });
                }
                else
                {
                    return RedirectToAction("IndexAll");
                }
            }
            return View(koords);
        }
        // Koordinaattien Poisto
        public ActionResult RemoveCoords(string itemName, int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Koordinaatit koords = db.Koordinaatit.Find(id);
                if (koords == null)
                {
                    return HttpNotFound();
                }
                // välitetään itemin nimi viewbagin kautta että admin näkee mitä hän on lisäämässä/muokkamassa/poistamassa ZK
                ViewBag.itemName = itemName;
                return View(koords);
            }
            else
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
        }

        [HttpPost, ActionName("RemoveCoords")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCoordsConfirmed(int id, int? referi)
        {
            Koordinaatit koords = db.Koordinaatit.Find(id);
            db.Koordinaatit.Remove(koords);
            db.SaveChanges();

            // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
            if (indexAll == false)
            {
                return RedirectToAction("Index", new { kategoriaRefer = referi });
            }
            else
            {
                return RedirectToAction("IndexAll");
            }
        }

        //--------------------- KATEGORIAT JA ITEMIT ------------------------

        // Kategorian lisäys by RJ

        // Viedään Kategorian lisäys sivulle
        public ActionResult AddKategoria(int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                var kieliKoodi = "FI";

                List<SelectListItem> luokat = new List<SelectListItem>();
                luokat.Add(new SelectListItem() { Text = "Pääkategoria", Value = 1.ToString() });
                luokat.Add(new SelectListItem() { Text = "Kategoria", Value = 2.ToString() });
                luokat.Add(new SelectListItem() { Text = "Duplikaatti", Value = 3.ToString() });

                List<SelectListItem> kategoriat = new List<SelectListItem>();
                kategoriat.Add(new SelectListItem
                {
                    Value = null,
                    Text = "Pääryhmä"
                });
                foreach (Kategoria kategoria in db.Kategoria)
                {
                    if (kategoria.Class != 3)
                    {
                        var lista = from k in db.Tekstit.Include(k => k.Kategoria).Include(k=>k.InfoTekstit)
                                    select k;

                        lista = lista.Where(k => k.KieliID == kieliKoodi && k.ReferTo == kategoria.KategoriaID);

                        kategoriat.Add(new SelectListItem
                        {
                            Value = kategoria.KategoriaID.ToString(),
                            Text = lista.SingleOrDefault().Teksti
                        });
                    }
                }

                string selectedvalue = id.ToString();

                //kategorialistaan esivalitaan kategorian, jossa painettiin lisää kategoria buttonin ZK 
                if (id != null)
                {
                    ViewBag.LuokkaLista = new SelectList(luokat, "Value", "Text");
                    ViewBag.Title = "Lisää uusi kategoria";
                    ViewBag.Luokka = 2.ToString();
                }
                else
                {
                    ViewBag.LuokkaLista = new SelectList(luokat, "Value", "Text");
                    ViewBag.Title = "Lisää uusi pääryhmä";
                    ViewBag.Luokka = 1.ToString();
                }

                ViewBag.KategoriaLista = new SelectList(kategoriat, "Value", "Text");

                ViewBag.id = id;

                return View();
            }
            else

            {
                return RedirectToAction("AccessDenied", "Admin");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddKategoria(AddKategoriaViewModel model)
        {
            var kategoria = new Kategoria // Lisätään ensiksi uuden kategorian tiedot ja luodaan tietokantaan tämä
            {
                SN = model.SN,
                ReferTo = model.ReferTo,
                Class = model.Class
            };

            db.Kategoria.Add(kategoria);    // Lisätään tietokantaan uusi kategoria
            db.SaveChanges();

            var kategoriaID = kategoria.KategoriaID;   // otetaan lisätyn kategorian kategoriaID tietoon

            var kielet = from k in db.Kieli select k;   // Muutetaan jokaisen uuden tekstin teksti kenttään uudet kirjoitetut tekstit
            foreach (var kieli in kielet)
            {
                var tekstit = (from t in db.Tekstit
                               where (t.KieliID == kieli.KieliID) &&
                               (t.Kategoria.KategoriaID == kategoriaID)
                               select t).FirstOrDefault();

                if (kieli.KieliID == "FI")
                {
                    tekstit.Teksti = model.TekstiFI;
                    db.Entry(tekstit).State = EntityState.Modified;
                }

                else if (kieli.KieliID == "SE")
                {
                    if (String.IsNullOrEmpty(model.TekstiSE))
                    {
                        tekstit.Teksti = model.TekstiFI;
                        db.Entry(tekstit).State = EntityState.Modified;
                    }
                    else
                    {
                        tekstit.Teksti = model.TekstiSE;
                        db.Entry(tekstit).State = EntityState.Modified;
                    }
                }

                else if (kieli.KieliID == "EN")
                {
                    if (String.IsNullOrEmpty(model.TekstiEN))
                    {
                        tekstit.Teksti = model.TekstiFI;
                        db.Entry(tekstit).State = EntityState.Modified;
                    }
                    else
                    {
                        tekstit.Teksti = model.TekstiEN;
                        db.Entry(tekstit).State = EntityState.Modified;
                    }
                }
            }
            db.SaveChanges(); // tallennetaan tekstimuutokset tietokantaan

            // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
            if (indexAll == false)
            {
                return RedirectToAction("Index", new { kategoriaRefer = model.ReferTo });
            }
            else
            {
                return RedirectToAction("IndexAll");
            }
        }

        // Kategorian Muokkaus by RJ111

        // Viedään Kategorian muokkaus sivulle - edit 27.5 Toimii RJ
        public ActionResult EditKategoria(string itemName, int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                var kieliKoodi = "FI"; // kovakoodattu suomenkieli nyt admin paneelin. Muutetaan tämä myöhemmässä vaiheessa.

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Kategoria kateg = db.Kategoria.Find(id);  // Haetaan kategoria taulusta id:lle tämä itemi

                if (kateg == null)
                {
                    return HttpNotFound();
                }

                // tehdään muutosViewModel itemistä joka viedään edit sivulle
                var kategId = new AddKategoriaViewModel
                {
                    KategoriaID = kateg.KategoriaID, // Syötetään Kategoriataulun tiedot
                    SN = kateg.SN,
                    ReferTo = kateg.ReferTo,
                    Class = kateg.Class.GetValueOrDefault()
                };

                // Muutetaan jokaisen kategorian tekstin teksti kenttään uudet muokatut tekstit
                var kielet = from k in db.Kieli select k;
                foreach (var kieli in kielet)
                {
                    var tekstit = (from t in db.Tekstit.Include(t=>t.InfoTekstit).DefaultIfEmpty()
                                   where (t.KieliID == kieli.KieliID) &&
                                   (t.Kategoria.KategoriaID == kateg.KategoriaID)
                                   select t).FirstOrDefault();


                    var infot = db.InfoTekstit.Find(tekstit.TekstiID);
                    if (infot != null)
                    {

                        if (kieli.KieliID == "FI")
                        {
                            kategId.TekstiFI = tekstit.Teksti;
                            kategId.InfoTekstiFI = tekstit.InfoTekstit.InfotextContent;
                        }

                        else if (kieli.KieliID == "SE")
                        {
                            kategId.TekstiSE = tekstit.Teksti;
                            kategId.InfoTekstiSE = tekstit.InfoTekstit.InfotextContent;

                        }

                        else if (kieli.KieliID == "EN")
                        {
                            kategId.TekstiEN = tekstit.Teksti;
                            kategId.InfoTekstiEN = tekstit.InfoTekstit.InfotextContent;
                        }
                    }
                    else
                    {
                        if (kieli.KieliID == "FI")
                        {
                            kategId.TekstiFI = tekstit.Teksti;
                            kategId.InfoTekstiFI = "";
                        }

                        else if (kieli.KieliID == "SE")
                        {
                            kategId.TekstiSE = tekstit.Teksti;
                            kategId.InfoTekstiSE = "";

                        }

                        else if (kieli.KieliID == "EN")
                        {
                            kategId.TekstiEN = tekstit.Teksti;
                            kategId.InfoTekstiEN = "";
                        }
                    }


                }
                IList<SelectListItem> luokat = new List<SelectListItem>();
                luokat.Add(new SelectListItem() { Text = "Pääkategoria", Value = 1.ToString() });
                luokat.Add(new SelectListItem() { Text = "Kategoria", Value = 2.ToString() });
                luokat.Add(new SelectListItem() { Text = "Duplikaatti", Value = 3.ToString() });

                IList<SelectListItem> kategoriat = new List<SelectListItem>();
                // Lisätään ekana Pääryhmä arvolla null
                kategoriat.Add(new SelectListItem
                {
                    Value = null,
                    Text = "Pääkategoria"
                });

                foreach (Kategoria kategoria in db.Kategoria)
                {
                    var lista = from k in db.Tekstit.Include(k => k.Kategoria)
                                select k;

                    lista = lista.Where(k => k.KieliID == kieliKoodi && k.ReferTo == kategoria.KategoriaID);

                    kategoriat.Add(new SelectListItem
                    {
                        Value = kategoria.KategoriaID.ToString(),
                        Text = lista.SingleOrDefault().Teksti
                    });
                }
                ViewBag.LuokkaLista = new SelectList(luokat, "Value", "Text", kateg.Class);
                ViewBag.KategoriaLista = new SelectList(kategoriat, "Value", "Text", kateg.Tekstit.Where(t => t.KieliID == kieliKoodi));
                // välitetään itemin nimi viewbagin kautta että admin näkee mitä hän on lisäämässä/muokkamassa/poistamassa ZK
                ViewBag.itemName = itemName;
                return View(kategId); // palautetaan AddKategoriaViewModeli muuttoja varten
            }
            else
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKategoria(AddKategoriaViewModel model)
        {
            var kategoria = new Kategoria // lisätään Kategoria taulun muutetut tiedot
            {
                KategoriaID = model.KategoriaID,
                SN = model.SN,
                ReferTo = model.ReferTo,
                Class = model.Class
            };

            db.Entry(kategoria).State = EntityState.Modified;    // Muokataan kategorian tiedot tietokantaan
            db.SaveChanges();

            var kategoriaID = kategoria.KategoriaID;   // otetaan kategorian kategoriaID tietoon

            var kielet = from k in db.Kieli select k;   // Muutetaan jokaisen kategorian tekstin teksti kenttiinn uudet muokatut tekstit
            foreach (var kieli in kielet)
            {
                var tekstit = (from t in db.Tekstit
                               where (t.KieliID == kieli.KieliID) &&
                               (t.Kategoria.KategoriaID == kategoriaID)
                               select t).FirstOrDefault();

                var infot = db.InfoTekstit.Find(tekstit.TekstiID);
                if (kieli.KieliID == "FI")
                {
                    //db.Entry(tekstit).State = EntityState.Modified;
                    tekstit.Teksti = model.TekstiFI;
                    
                    db.Entry(tekstit).State = EntityState.Modified;
                    
                    //jos on  olemassa infoteksti ja edit textfield ei ole tyhjä, tallennetaan muutokset
                    if (infot !=null && !String.IsNullOrEmpty(model.InfoTekstiFI))
                    {
                        infot.InfotextContent = model.InfoTekstiFI;
                        db.Entry(infot).State = EntityState.Modified;
                    }
                    //jos ei ole olemassa infoteksti mutta kirjoitettiin infotexti edit kenttään, tallennetaan uusi infotekstin
                    else if (infot ==null && !String.IsNullOrEmpty(model.InfoTekstiFI))
                    {
                        var infoTekst = new InfoTekstit
                        {
                            Infotext_ID=tekstit.TekstiID,
                            InfotextContent = model.InfoTekstiFI
                        };
                        db.InfoTekstit.Add(infoTekst);
                    }
                }

                else if (kieli.KieliID == "SE")
                {
                    tekstit.Teksti = model.TekstiSE;
                    db.Entry(tekstit).State = EntityState.Modified;
                    if (infot != null && !String.IsNullOrEmpty(model.InfoTekstiSE))
                    {
                        infot.InfotextContent = model.InfoTekstiSE;
                        db.Entry(infot).State = EntityState.Modified;
                    }
                    //jos ei ole olemassa infoteksti mutta kirjoitettiin infotexti edit kenttään, tallennetaan uusi infotekstin
                    else if (infot == null && !String.IsNullOrEmpty(model.InfoTekstiSE))
                    {
                        var infoTekst = new InfoTekstit
                        {
                            Infotext_ID = tekstit.TekstiID,
                            InfotextContent = model.InfoTekstiSE
                        };
                        db.InfoTekstit.Add(infoTekst);
                    }
                }

                else if (kieli.KieliID == "EN")
                {
                    tekstit.Teksti = model.TekstiEN;
                    db.Entry(tekstit).State = EntityState.Modified;
                    if (infot != null && !String.IsNullOrEmpty(model.InfoTekstiEN))
                    {
                        infot.InfotextContent = model.InfoTekstiEN;
                        db.Entry(infot).State = EntityState.Modified;
                    }
                    //jos ei ole olemassa infoteksti mutta kirjoitettiin infotexti edit kenttään, tallennetaan uusi infotekstin
                    else if (infot == null && !String.IsNullOrEmpty(model.InfoTekstiEN))
                    {
                        var infoTekst = new InfoTekstit
                        {
                            Infotext_ID = tekstit.TekstiID,
                            InfotextContent = model.InfoTekstiEN
                        };
                        db.InfoTekstit.Add(infoTekst);
                    }
                }
            }
            db.SaveChanges(); // tallennetaan tekstimuutokset tietokantaan

            // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
            if (indexAll == false)
            {
                return RedirectToAction("Index", new { kategoriaRefer = model.ReferTo });
            }
            else
            {
                return RedirectToAction("IndexAll");
            }
        }

        // Kategorian Poisto RJ
        public ActionResult RemoveKategoria(string itemName, int? id)
        {
            if (Session["PaaAdmin"] != null || Session["PerusAdmin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Kategoria kategoriat = db.Kategoria.Find(id);

                if (kategoriat.Class == 1 && Session["PaaAdmin"] == null)
                {
                    return RedirectToAction("AccessDenied", "Admin");
                }


                if (kategoriat == null)
                {
                    return HttpNotFound();
                }
                // välitetään itemin nimi viewbagin kautta että admin näkee mitä hän on lisäämässä/muokkamassa/poistamassa ZK
                ViewBag.itemName = itemName;
                return View(kategoriat);
            }
            else
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
        }

        [HttpPost, ActionName("RemoveKategoria")]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveKategoriaConfirmed(int id)
        {
            Kategoria kategoriat = db.Kategoria.Find(id);
            db.Kategoria.Remove(kategoriat);
            db.SaveChanges();

            // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
            if (indexAll == false)
            {
                return RedirectToAction("Index", new { kategoriaRefer = kategoriat.ReferTo });
            }
            else
            {
                return RedirectToAction("IndexAll");
            }
        }

        // --------------------- LOGIN TOIMINNOT --------------------------//

        public ActionResult Sisaankirjautuminen()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Auktorisointi(Logins LoginModel)
        {
            KirjastoProjektiEntities1 db = new KirjastoProjektiEntities1();
            //Haetaan käyttäjän/Loginin tiedot annetuilla tunnustiedoilla tietokannasta LINQ -kyselyllä -FW
            var SisaanKayttaja = db.Logins.SingleOrDefault(x => x.Nimi == LoginModel.Nimi && x.Hash == LoginModel.Hash);
            if (SisaanKayttaja != null)
            {
                if (SisaanKayttaja.Rooli.Equals("PaaAdmin"))
                {
                    ViewBag.LoginError = 0;
                    Session["PaaAdmin"] = SisaanKayttaja.Nimi + " (pääkäyttäjäoikeudet)";
                    return RedirectToAction("Index", "Admin");
                }
                else if (SisaanKayttaja.Rooli.Equals("PerusAdmin"))
                {
                    ViewBag.LoginError = 0;
                    Session["PerusAdmin"] = SisaanKayttaja.Nimi + " (hallintaoikeudet)";
                    return RedirectToAction("Index", "Admin");
                }
                else if (SisaanKayttaja.Rooli.Equals("PerusUser"))
                {
                    ViewBag.LoginError = 0;
                    Session["PerusUser"] = SisaanKayttaja.Nimi + " (katseluoikeudet)";
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.LoginError = 1;
                    LoginModel.LoginErrorMessage = "Tuntematon käyttäjätunnus tai salasana.";
                    return RedirectToAction("Sisaankirjautuminen", "Admin");
                }
            }
            else
            {
                ViewBag.LoginError = 1;
                LoginModel.LoginErrorMessage = "Tuntematon käyttäjätunnus tai salasana.";
                return View("Sisaankirjautuminen", LoginModel);
            }
        }
        public ActionResult Uloskirjautuminen()
        {
            Session.Abandon(); //:"Hylkää" sessionin
            return RedirectToAction("Sisaankirjautuminen", "Admin"); //Uloskirjautumisen jälkeen palauttaa tällä hetkellä takaisin Index/Home -FW
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult AddKategCoords(string sn, int? referTo, int? luokka, string teksti)
        {
            var kategoria = new Kategoria // Lisätään ensiksi uuden kategorian tiedot ja luodaan tietokantaan tämä
            {
                SN = sn,
                ReferTo = referTo,
                Class = luokka
            };

            db.Kategoria.Add(kategoria);    // Lisätään tietokantaan uusi kategoria
            db.SaveChanges();

            var kategoriaID = kategoria.KategoriaID;   // otetaan lisätyn kategorian kategoriaID tietoon

            var kielet = from k in db.Kieli select k;   // Muutetaan jokaisen uuden tekstin teksti kenttään uudet kirjoitetut tekstit
            foreach (var kieli in kielet)
            {
                var tekstit = (from t in db.Tekstit
                               where (t.KieliID == kieli.KieliID) &&
                               (t.Kategoria.KategoriaID == kategoriaID)
                               select t).FirstOrDefault();

                if (kieli.KieliID == "FI")
                {
                    tekstit.Teksti = teksti + " - Koordinaatti";
                    db.Entry(tekstit).State = EntityState.Modified;
                }

                else if (kieli.KieliID == "SE")
                {
                    tekstit.Teksti = teksti + " - Koordinaatti";
                    db.Entry(tekstit).State = EntityState.Modified;
                }

                else if (kieli.KieliID == "EN")
                {
                    tekstit.Teksti = teksti + " - Koordinaatti";
                    db.Entry(tekstit).State = EntityState.Modified;
                }
            }
            db.SaveChanges(); // tallennetaan tekstimuutokset tietokantaan

            if (luokka != 3)
            {
                // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
                if (indexAll == false)
                {
                    return RedirectToAction("Index", new { id = kategoriaID, itemName = teksti, kategoriaRefer = referTo });
                }
                else
                {
                    return RedirectToAction("IndexAll");
                }

            }
            else
            {
                // Tehdään tarkastus palautetaanko Index vai IndexAll sivu
                if (indexAll == false)
                {
                    return RedirectToAction("Index", new { id = kategoriaID, itemName = teksti, kategoriaRefer = referTo });
                }
                else
                {
                    return RedirectToAction("IndexAll");
                }
            }
        }
        //------- Kayttäjätilien hallinta -------//

        //GET: Logins/Lista
        public ActionResult IndexUsers()
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                return View(db.Logins.ToList());
            }
        }

        // GET: Logins/Lisää
        public ActionResult LisaaKayttaja()
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                List<SelectListItem> userrolelista = new List<SelectListItem>();
                foreach (UserRoles userrole in db.UserRoles)
                {
                    userrolelista.Add(new SelectListItem
                    {
                        Value = userrole.RooliID.ToString(),
                        Text = userrole.RooliID.ToString()
                    });
                }

                ViewBag.Rooli = new SelectList(userrolelista, "Value", "Text", "PerusAdmin");
                return View();
            }
        }

        // POST: Logins/Lisää
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LisaaKayttaja([Bind(Include = "Nimi,Hash,Rooli")] Logins logins)
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {

                if (ModelState.IsValid)
                {
                    if (db.Logins.Where(u => u.Nimi == logins.Nimi).Any())
                    {
                        List<SelectListItem> userrolelista = new List<SelectListItem>();
                        foreach (UserRoles userrole in db.UserRoles)
                        {
                            userrolelista.Add(new SelectListItem
                            {
                                Value = userrole.RooliID.ToString(),
                                Text = userrole.RooliID.ToString()
                            });
                        }

                        logins.LoginErrorMessage = "Käyttäjä nimi on otettu!";
                        ViewBag.Rooli = new SelectList(userrolelista, "Value", "Text", "PerusAdmin");
                        return View("LisaaKayttaja", logins);
                    }
                    else
                    {
                        db.Logins.Add(logins);
                        db.SaveChanges();
                        return RedirectToAction("IndexUsers");
                    }
                }
                return View(logins);
            }
        }

        // GET: Logins/Muokkaa
        public ActionResult MuokkaaKayttaja(string id)
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Logins logins = db.Logins.Find(id);

                if (logins == null)
                {
                    return HttpNotFound();
                }

                List<SelectListItem> userrolelista = new List<SelectListItem>();
                foreach (UserRoles userrole in db.UserRoles)
                {
                    userrolelista.Add(new SelectListItem
                    {
                        Value = userrole.RooliID,
                        Text = userrole.RooliID
                    });
                }

                ViewBag.Rooli = new SelectList(userrolelista, "Value", "Text", logins.Rooli);


                return View(logins);
            }
        }
        // POST: Logins/Muokkaa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MuokkaaKayttaja([Bind(Include = "Nimi,Hash,Rooli")] Logins logins)
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    db.Entry(logins).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("IndexUsers");
                }
                return View(logins);
            }
        }

        // GET: Logins/Poista
        public ActionResult PoistaKayttaja(string id)
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Logins logins = db.Logins.Find(id);
                if (logins == null)
                {
                    return HttpNotFound();
                }
                return View(logins);
            }
        }

        // POST: Logins/Poista
        [HttpPost, ActionName("PoistaKayttaja")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (Session["PaaAdmin"] == null)
            {
                return RedirectToAction("AccessDenied", "Admin");
            }
            else
            {
                Logins logins = db.Logins.Find(id);
                db.Logins.Remove(logins);
                db.SaveChanges();
                return RedirectToAction("IndexUsers");
            }
        }

    }
}