﻿using System;
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
    public class HomeController : Controller
    {

        KirjastoProjektiEntities1 db = new KirjastoProjektiEntities1();

        public ActionResult Index()
        {
            Session.Abandon();
            return View();
        }
    }
}