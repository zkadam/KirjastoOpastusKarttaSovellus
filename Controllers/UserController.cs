using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using KirjastoAppScrum.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;

namespace KirjastoAppScrum.Controllers
{
    public class UserController : Controller
    {
        // Luodaan DB olio
        KirjastoProjektiEntities1 db = new KirjastoProjektiEntities1();
        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var cultureInfo = CultureInfo.GetCultureInfo("fi-FI");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
        public ActionResult Kategoriat(string kieli, int? referi, int? id, int? koordinaatit, int? luokka, int? dublikaatti)
        {
            var lista = from t in db.Tekstit.Include(t => t.Kategoria).Include(t => t.Kategoria.Koordinaatit).Include(t=>t.InfoTekstit).OrderBy(t => t.Teksti)
                        select t;



            string setLang = "";

            //setting apumuuttuja if it is null (eg you start the page from somewhere else then index) 
            //----------------------------------- - this was just necessary coz the code only had options for0 and 1ZK

            if (luokka == 1 && referi == null)
            {
                luokka = 2;
            }

            Session["koordinaatiPolku"] = koordinaatit;
            Session["linkkiPolkku"] = referi;
            Session["luokkaPolku"] = luokka;
            //Session["koordinaattiPolku"] = referi;
            //setiing the language for session and filtering results ZK
            //if there is no session nor kieli parameter, we set them both to FI -zk
            if ((Session["setLangTemp"] == null) && string.IsNullOrEmpty(kieli))
            {
                setLang = "FI";
                Session["setLangTemp"] = "FI";
            }
            //if came kieli parameter, we set them both to kieli parameter ZK
            else if (!string.IsNullOrEmpty(kieli))
            {
                setLang = kieli;
                Session["setLangTemp"] = kieli;
            }
            //if there in no kieli parameter but there is session, we set filtering to session value ZK
            else
            {
                setLang = Session["setLangTemp"].ToString();
            }

            //Tulostaa pääkategoriat kielellä FI jos kieli = empty OR null (R.J)

            if (String.IsNullOrEmpty(setLang))
            {
                lista = lista.Where(t => t.Kategoria.ReferTo == null && t.KieliID == "FI");
            }
            // jos kieli löytyy ja apuMuuttuja on 0 (apuMuuttuja on arvolta 0 kun painaa buttonia)  (R.J)
            if (!String.IsNullOrEmpty(setLang) && luokka == 2)
            {

                //Dictionary<int?, string> naviList = new Dictionary<int?, string>(); // Tehdään dictionary johon haetaan linkkipolkujen itemit RJ
                List<int?> EndLista = new List<int?>();
                List<LinkkiLista> naviList = new List<LinkkiLista>();

                // Haetaan painetun buttonin tiedot linkkipolku listan ensimmäistä lisäystä varten.
                var listaNimi = from a in db.Tekstit.Where(a => a.KieliID == setLang && a.ReferTo == koordinaatit) select a.Teksti;

                // Tekee listan kaikista kategorian itemeistä ja ottaa talteen ReferTo arvon (R.J)
                foreach (Kategoria loppu in db.Kategoria.Include(t => t.Tekstit))
                    EndLista.Add(loppu.ReferTo);

                // checkReferi muuttujaksi asetetaan painetun buttonin referi, tämä muuttuu foreach loopissa kun poraudutaan syvemmälle RJ
                var checkRefer = referi;
                naviList.Add(new LinkkiLista { Refer = referi, Koordinaatti = koordinaatit, Id = id, Luokka = luokka, Teksti = listaNimi.SingleOrDefault()}); // lisätään painetun buttonin linkkitiedot naviList - listaan RJ
                while (checkRefer != null || checkRefer == 0) // while looppi kunnes referi on null niin linkkipolku on valmis dictionaryssa RJ
                {
                    foreach (var listaItem in db.Kategoria.Include(c => c.Tekstit))
                    {
                        if (listaItem.KategoriaID == checkRefer)
                        {
                            naviList.Add(new LinkkiLista { Refer = listaItem.ReferTo, Koordinaatti = listaItem.KategoriaID, Id = listaItem.KategoriaID,Luokka = listaItem.Class, kieliID = kieli, Teksti = listaItem.Tekstit.Where(t => t.KieliID == setLang).SingleOrDefault().Teksti }); // lisätään painetun buttonin master linkkipolkuun RJ
                            checkRefer = listaItem.ReferTo; //jos masterilla on oma master niin tehdään sille lisäys myös, looppii loppuu jos checkReferiksi tulee null (eli pääkategoria) RJ
                        }
                    }
                }

                // tekee foreach tsekkauksen löytyykö painetun buttonin KategoriaID arvo Kategorian.ReferTo arvolistasta (R.J)
                if (EndLista.Contains(id) && dublikaatti == 1)
                {
                    ViewBag.naviListToData = naviList.ToArray().Reverse(); // listaData navigaatio polkuun Reverse metodilla niin tulostaa polun oikein RJ
                    // Jos löytyy niin tulostaa kaikki jotka referöivät siihen (R.J)
                    return RedirectToAction("NaytaKartta", new { referi, koordinaatit, luokka, id, dublikaatti });
                }
                else if (EndLista.Contains(id))
                {
                    ViewBag.naviListToData = naviList.ToArray().Reverse(); // listaData navigaatio polkuun Reverse metodilla niin tulostaa polun oikein RJ
                    // Jos löytyy niin tulostaa kaikki jotka referöivät siihen (R.J)
                    lista = lista.Where(t => t.KieliID.Contains(setLang) && t.Kategoria.ReferTo == id && t.Kategoria.KategoriaID != id);
                }
                else
                {               
                    return RedirectToAction("NaytaKartta", new {referi, koordinaatit,luokka, id });               
                    // refer välittää parametrit NaytaKartta Actioniin RJ
                }
            }

            // Tulostaa kielen mukaan pääkategoriat. apuMuuttuja on arvoltaan 1 kun painaa pääsivun Tervetuloa,Välkommen,Welcome painikkeita. (R.J)

            if (!String.IsNullOrEmpty(setLang) && luokka == 1)
            {
                lista = lista.Where(t => t.KieliID.Contains(setLang) && t.Kategoria.Class == 1 && t.Kategoria.ReferTo == null);
            }
            return View("Kategoriat", "_Layout_Kategoriat", lista);
        } 
        //-------------------------------------------------ABC järjestyksessä kategoriat
        public ActionResult ABC_Kategoriat(string kieli, int? referi, int? id, int? koordinaatit, int? luokka, int? dublikaatti)
        {
            //CultureInfo culture = new CultureInfo("fi-FI");
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fi-FI");
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(("fi-FI"));


            var lista = from t in db.Tekstit.Include(t => t.Kategoria).Include(t => t.Kategoria.Koordinaatit).OrderBy(t => t.Teksti)
                        select t;


            string setLang = "";

            //setting apumuuttuja if it is null (eg you start the page from somewhere else then index) 
            //----------------------------------- - this was just necessary coz the code only had options for0 and 1ZK

            if (luokka == 1 && referi == null)
            {
                luokka = 2;
            }

            Session["koordinaatiPolku"] = koordinaatit;
            Session["linkkiPolkku"] = referi;
            Session["luokkaPolku"] = luokka;
            //Session["koordinaattiPolku"] = referi;
            //setiing the language for session and filtering results ZK
            //if there is no session nor kieli parameter, we set them both to FI -zk
            if ((Session["setLangTemp"] == null) && string.IsNullOrEmpty(kieli))
            {
                setLang = "FI";
                Session["setLangTemp"] = "FI";
            }
            //if came kieli parameter, we set them both to kieli parameter ZK
            else if (!string.IsNullOrEmpty(kieli))
            {
                setLang = kieli;
                Session["setLangTemp"] = kieli;
            }
            //if there in no kieli parameter but there is session, we set filtering to session value ZK
            else
            {
                setLang = Session["setLangTemp"].ToString();
            }

            //Tulostaa pääkategoriat kielellä FI jos kieli = empty OR null (R.J)

            if (String.IsNullOrEmpty(setLang))
            {
                lista = lista.Where(t => t.Kategoria.Class<3 && t.KieliID == "FI");
            }
            // jos kieli löytyy ja apuMuuttuja on 0 (apuMuuttuja on arvolta 0 kun painaa buttonia)  (R.J)
                List<int?> EndLista = new List<int?>();
           

                //Dictionary<int?, string> naviList = new Dictionary<int?, string>(); // Tehdään dictionary johon haetaan linkkipolkujen itemit RJ
                List<LinkkiLista> naviList = new List<LinkkiLista>();

                // Haetaan painetun buttonin tiedot linkkipolku listan ensimmäistä lisäystä varten.
                var listaNimi = from a in db.Tekstit.Where(a => a.KieliID == setLang && a.ReferTo == koordinaatit) select a.Teksti;

                // Tekee listan kaikista kategorian itemeistä ja ottaa talteen ReferTo arvon (R.J)
                foreach (Kategoria loppu in db.Kategoria.Include(t => t.Tekstit))
                    EndLista.Add(loppu.ReferTo);

                // checkReferi muuttujaksi asetetaan painetun buttonin referi, tämä muuttuu foreach loopissa kun poraudutaan syvemmälle RJ
                var checkRefer = referi;
                naviList.Add(new LinkkiLista { Refer = referi, Koordinaatti = koordinaatit, Id = id, Luokka = luokka, Teksti = listaNimi.SingleOrDefault() }); // lisätään painetun buttonin linkkitiedot naviList - listaan RJ
                while (checkRefer != null || checkRefer == 0) // while looppi kunnes referi on null niin linkkipolku on valmis dictionaryssa RJ
                {
                    foreach (var listaItem in db.Kategoria.Include(c => c.Tekstit))
                    {
                        if (listaItem.KategoriaID == checkRefer)
                        {
                            naviList.Add(new LinkkiLista { Refer = listaItem.ReferTo, Koordinaatti = listaItem.KategoriaID, Id = listaItem.KategoriaID, Luokka = listaItem.Class, kieliID = kieli, Teksti = listaItem.Tekstit.Where(t => t.KieliID == setLang).SingleOrDefault().Teksti }); // lisätään painetun buttonin master linkkipolkuun RJ
                            checkRefer = listaItem.ReferTo; //jos masterilla on oma master niin tehdään sille lisäys myös, looppii loppuu jos checkReferiksi tulee null (eli pääkategoria) RJ
                        }
                    }
                }

                // tekee foreach tsekkauksen löytyykö painetun buttonin KategoriaID arvo Kategorian.ReferTo arvolistasta (R.J)
                if (EndLista.Contains(id) && dublikaatti == 1)
                {
                    ViewBag.naviListToData = naviList.ToArray().Reverse(); // listaData navigaatio polkuun Reverse metodilla niin tulostaa polun oikein RJ
                    // Jos löytyy niin tulostaa kaikki jotka referöivät siihen (R.J)
                    return RedirectToAction("NaytaKartta", new { referi, koordinaatit, luokka, id, dublikaatti });
                }
                else if (EndLista.Contains(id))
                {
                    ViewBag.naviListToData = naviList.ToArray().Reverse(); // listaData navigaatio polkuun Reverse metodilla niin tulostaa polun oikein RJ
                    // Jos löytyy niin tulostaa kaikki jotka referöivät siihen (R.J)
                    //lista = lista.Where(t => t.KieliID.Contains(setLang) && t.Kategoria.ReferTo == id && t.Kategoria.KategoriaID != id);
                }
                else
                {
                    return RedirectToAction("NaytaKartta", new { referi, koordinaatit, luokka, id });
                    // refer välittää parametrit NaytaKartta Actioniin RJ
                }
           

            // Tulostaa kielen mukaan pääkategoriat. apuMuuttuja on arvoltaan 1 kun painaa pääsivun Tervetuloa,Välkommen,Welcome painikkeita. (R.J)
            var RefList = from r in db.Kategoria
                          select
                          r;



            //listasta otetaan pois pelkestään koordinaatit(monitulostus) ja kategoriat jotka ei viittaa mihinkään
            lista = lista.Where(t => EndLista.Contains(t.Kategoria.KategoriaID) || t.Kategoria.Koordinaatit.KoordinaattiID > 0 || t.Kategoria.Class == 1);
            lista = lista.Where(t => t.KieliID.Contains(setLang) && t.Kategoria.Class < 3);


            return View("ABC_Kategoriat", "_Layout_Kategoriat", lista);
        }

        //-----------------------------------------------action result karttan näyttämisen porautumisen jälkeen-ZK----------------------------------------------------------------
        public ActionResult NaytaKartta(int? referi, int? koordinaatit, string kieli, int? id,int? dublikaatti, int? luokka) 
        {
            // Muokattu karttanäkymää sillä tavalla että tuo koordinaatit näkyviin kartalle. Jos esim monta itemiä referöi samaan ja on porautumisen lopetus niin tulostaa kaikki kohteet kartalle,
            // tätä varten on määritelty luokka määritykseksi kategorian 'class' muuttuja. Esim WC on class 1, mutta wc1,wc2,wc3,wc4 ovat class 2.

            //asennetaan sessionit parametriä varten joita sijoitetaan mm. navbaar buttoneihin parametriks ZK 
            Session["koordinaatiPolku"] = koordinaatit;
            Session["linkkiPolkku"] = referi;
            Session["dublikaattiPolkku"] = dublikaatti;
            Session["luokkaPolku"] = luokka;
            string noCoords =  "";
            string name = "";
           

            // Tehdään lista josta etsitään tarvittava nimitieto lennosta vaihtoon.
            var coordName = from c in db.Tekstit.Include(c => c.Kategoria).Include(c => c.Kategoria.Koordinaatit)
                            select c;

            //kieli asetukset voimaan, riippuen , saimmeko kieli parametri tai käytetään session arvo -ZK
            string setLang = "";
            if ((Session["setLangTemp"] == null) && string.IsNullOrEmpty(kieli))
            {
                setLang = "FI";
                Session["setLangTemp"] = "FI";
            }
            //if came kieli parameter, we set them both to kieli parameter ZK
            else if (!string.IsNullOrEmpty(kieli))
            {
                setLang = kieli;
                Session["setLangTemp"] = kieli;
            }
            //if there in no kieli parameter but there is session, we set filtering to session value ZK
            else
            {
                setLang = Session["setLangTemp"].ToString();
            }

            //ViewBagin sijoitetaan kerrosvaihto button tekstit ja muita kieli riipuvia textejä-ZK
            //TÄHÄN VOI LAITTAA KAIKKI MUUT KIELI RIIPUVIA ASETUKSIA ZK
           
            if (Session["setLangTemp"].ToString() == "SE")
            {
                ViewBag.Kieliviesti = "Namn";
                ViewBag.Kerros = "våning";
                ViewBag.KoordsToisessa = "Det du söker finns på andra våningen. Du kan byta våning genom att trycka på knappen nedanför kartan.";
                ViewBag.notFound = "Det du söker finns inte eller har inte sparats in till applikationen. Be om hjälp av personalen.";
                ViewBag.OletTassa = "Du är här";
                // tehdään tarkastus onko referiä ja haetaan sen oikealla kielellä oleva nimi RJ
                if (referi != null)
                {
                    coordName = coordName.Where(c => c.ReferTo == koordinaatit && c.KieliID == "SE");
                    name = coordName.SingleOrDefault().Teksti;
                } else
                {
                    coordName = coordName.Where(c => c.ReferTo == koordinaatit && c.KieliID == "SE");
                    name = coordName.SingleOrDefault().Teksti;
                    // jos ei löydy referiä niin noCoords stringiin tulee kielen mukainen EiKoordinaatteja-teksti RJ
                    noCoords = "Inget läge.";
                }
            }
            else if (Session["setLangTemp"].ToString() == "EN")
            {
                ViewBag.Kieliviesti = "Item name";
                ViewBag.Kerros = "floor";
                ViewBag.KoordsToisessa = "Your target is on the 2nd floor. You can change the floor by pressing the buttons below the map.";
                ViewBag.notFound = "No location for the item that you are searching for. Please contact library staff for assistance.";
                ViewBag.OletTassa = "You are here";

                // tehdään tarkastus onko referiä ja haetaan sen oikealla kielellä oleva nimi RJ
                if (referi != null)
                {
                    coordName = coordName.Where(c => c.ReferTo == koordinaatit && c.KieliID == "EN");
                    name = coordName.SingleOrDefault().Teksti;
                }
                else
                {
                    coordName = coordName.Where(c => c.ReferTo == koordinaatit && c.KieliID == "EN");
                    name = coordName.SingleOrDefault().Teksti;
                    // jos ei löydy referiä niin noCoords stringiin tulee kielen mukainen EiKoordinaatteja-teksti RJ
                    noCoords = "No location information.";
                }
            }
            else
            {
                ViewBag.Kerros = "kerros";
                ViewBag.KoordsToisessa = "Etsimäsi kohde löytyy toisesta kerroksesta. Voit vaihtaa karttanäkymän kerrosta kartan alpuolella olevilla painikkeilla.";
                ViewBag.notFound = "Etsityn kohteen sijaintia ei löydy. Pyytäkää henkilökunnalta apua.";
                ViewBag.OletTassa = "Olet tässä";

                // tehdään tarkastus onko referiä ja haetaan sen oikealla kielellä oleva nimi RJ
                if (referi != null)
                {
                    coordName = coordName.Where(c => c.ReferTo == koordinaatit && c.KieliID == "FI");
                    name = coordName.SingleOrDefault().Teksti;
                }
                else
                {

                    coordName = coordName.Where(c => c.ReferTo == koordinaatit && c.KieliID == "FI");
                    name = coordName.SingleOrDefault().Teksti;
                    // jos ei löydy referiä niin noCoords stringiin tulee kielen mukainen EiKoordinaatteja-teksti RJ
                    noCoords = "Ei sijaintitietoja";
                }
            }

            List<LinkkiLista> naviList = new List<LinkkiLista>();
            var listaNimi = from a in db.Tekstit.Where(a => a.KieliID == setLang && a.ReferTo == koordinaatit) select a.Teksti;

            var checkMaster = referi;
            naviList.Add(new LinkkiLista { Refer = referi, Koordinaatti = koordinaatit, Id = id, Luokka = luokka, Teksti = listaNimi.SingleOrDefault() }); // lisätään painetun buttonin linkkitiedot naviList - listaan RJ
            while (checkMaster != null || checkMaster == 0) // while looppi kunnes referi on null niin linkkipolku on valmis dictionaryssa RJ
            {
                foreach (var listaItem in db.Kategoria.Include(c => c.Tekstit))
                {
                    if (listaItem.KategoriaID == checkMaster)
                    {
                        naviList.Add(new LinkkiLista { Refer = listaItem.ReferTo, Koordinaatti = listaItem.KategoriaID, Id = listaItem.KategoriaID, Luokka = listaItem.Class, Teksti = listaItem.Tekstit.Where(t => t.KieliID == setLang).SingleOrDefault().Teksti }); // lisätään painetun buttonin master linkkipolkuun RJ
                        checkMaster = listaItem.ReferTo; //jos masterilla on oma master niin tehdään sille lisäys myös, looppii loppuu jos checkReferiksi tulee null (eli pääkategoria) RJ
                    }
                }
            }

            ViewBag.naviListToData = naviList.ToArray().Reverse();

            // Haetaan koordinaattinäkymä R.J
            var koords = from t in db.Koordinaatit.Include(t => t.Kategoria).Include(t => t.Kategoria.Tekstit)
                         select t;

            // Haetaan koordinaatit, jos on dublikaatti on 1 niin tulostaa kaikki saman nimiset johon referöi niin kartalle, muuten tulostaa valitun itemin koordinaatit. RJ
            if (dublikaatti == 1)
            {
                koords = koords.Where(k => k.Kategoria.KategoriaID == k.KoordinaattiID && k.Kategoria.ReferTo == koordinaatit);
                // jos ei löytynyt koordinaatteja niin tulostaa nimen sekä "noCoords" RJ
                if (!string.IsNullOrEmpty(noCoords))
                {
                    ViewBag.Name = name;
                    ViewBag.NoCoords = noCoords;
                }
                // jos löytyi koordinaatteja niin tulostaa nimen RJ
                else
                {
                    ViewBag.Name = name;
                }
            }
            else
            {
                koords = koords.Where(k => k.Kategoria.KategoriaID == koordinaatit);
                if (!string.IsNullOrEmpty(noCoords))
                {
                    ViewBag.Name = name;
                    ViewBag.NoCoords = noCoords;
                }
                else
                {
                    ViewBag.Name = name;
                }
            }
            return View("NaytaKartta", "_Layout_Kategoriat", koords);
        }

        public ActionResult Map(string kieli)
        {
            string setLang = "";
            if ((Session["setLangTemp"] == null) && string.IsNullOrEmpty(kieli))
            {
                setLang = "FI";
                ViewBag.Kerros = "kerros";
                Session["setLangTemp"] = "FI";
            }
            else if (!string.IsNullOrEmpty(kieli))
            {
                setLang = kieli;
                Session["setLangTemp"] = kieli;
            }
            else
            {
                setLang = Session["setLangTemp"].ToString();
            }

            if (Session["setLangTemp"].ToString() == "FI")
            {
                ViewBag.Kerros = "kerros";
            }
            else if (Session["setLangTemp"].ToString() == "EN")
            {
                ViewBag.Kerros = "floor";
            }
            else if (Session["setLangTemp"].ToString() == "SE")
            {
                ViewBag.Kerros = "våning";
            }
            else
            {
                ViewBag.Kerros = "kerros";
            }

            return View("Map", "_Layout_Kategoriat");
        }

        public ActionResult Info(string kieli)
        {
            string setLang = "";
            if ((Session["setLangTemp"] == null) && string.IsNullOrEmpty(kieli))
            {
                setLang = "FI";
                Session["setLangTemp"] = "FI";
            }
            else if (!string.IsNullOrEmpty(kieli))
            {
                setLang = kieli;
                Session["setLangTemp"] = kieli;
            }
            else
            {
                setLang = Session["setLangTemp"].ToString();
            }

            return View("Info", "_Layout_Kategoriat");
        }
    }
}