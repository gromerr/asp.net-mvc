using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GuestListManagerWeb.Models;
using System.Xml;
using System.IO;
using System.Text;

namespace GuestListManagerWeb.Controllers
{
    public class GuestListsController : Controller
    {
        private GuestListDatabaseEntities db = new GuestListDatabaseEntities();

        // GET: GuestLists
        public ActionResult Index(string searchString)
        {
            CheckEqualGuestNumber();
            var guestLists = db.GuestLists.Include(g => g.Sex);

            if (!String.IsNullOrEmpty(searchString))
            {
                guestLists = guestLists.Where(guest => guest.FirstName.Contains(searchString) || guest.LastName.Contains(searchString));
            }


            return View(guestLists.ToList());
        }

        // GET: GuestLists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GuestList guestList = db.GuestLists.Find(id);
            if (guestList == null)
            {
                return HttpNotFound();
            }
            return View(guestList);
        }

        // GET: GuestLists/Create
        public ActionResult Create()
        {
            ViewBag.ExistsInfo = false;
            ViewBag.SexId = new SelectList(db.Sexes, "Id", "Name");
            return View();
        }

        // POST: GuestLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,SexId,Responded")] GuestList guestList)
        {
            int exists = CountFoundGuests(guestList);

            if (ModelState.IsValid && exists == 0)
            {
                db.GuestLists.Add(guestList);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            SetExistsInfo(exists);

            ViewBag.SexId = new SelectList(db.Sexes, "Id", "Name", guestList.SexId);
            return View(guestList);
        }

        // GET: GuestLists/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GuestList guestList = db.GuestLists.Find(id);
            if (guestList == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExistsInfo = false;
            ViewBag.SexId = new SelectList(db.Sexes, "Id", "Name", guestList.SexId);
            return View(guestList);
        }

        // POST: GuestLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,SexId,Responded")] GuestList guestList)
        {
            int exists = CountFoundGuests(guestList);

            if (ModelState.IsValid && exists <= 1)
            {
                GuestList record = db.GuestLists.Find(guestList.Id);
                record.LastName = guestList.LastName;
                record.FirstName = guestList.FirstName;
                record.SexId = guestList.SexId;
                record.Responded = guestList.Responded;
                
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            SetExistsInfo(exists);
            ViewBag.SexId = new SelectList(db.Sexes, "Id", "Name", guestList.SexId);
            return View(guestList);
        }

        // GET: GuestLists/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GuestList guestList = db.GuestLists.Find(id);
            if (guestList == null)
            {
                return HttpNotFound();
            }
            return View(guestList);
        }

        // POST: GuestLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GuestList guestList = db.GuestLists.Find(id);
            db.GuestLists.Remove(guestList);
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

        /// <summary>
        /// Count number of found guests
        /// </summary>
        /// <param name="guestList">guest list element</param>
        /// <returns>Number of existing guests</returns>
        private int CountFoundGuests(GuestList guestList)
        {
            var existsGuest = db.GuestLists.Where(guest => guest.LastName.Contains(guestList.LastName) && guest.FirstName.Contains(guestList.FirstName));
            int exists = existsGuest.ToList().Count;

            return exists;
        }

        /// <summary>
        /// Set exists info in message box at site
        /// </summary>
        /// <param name="exists"></param>
        private void SetExistsInfo(int exists)
        {
            if (exists > 0)
            {
                ViewBag.ExistsInfo = true;
            }
            else
            {
                ViewBag.ExistsInfo = false;
            }
        }

        /// <summary>
        /// Check equal numbers of men and women invited to party.
        /// </summary>
        private void CheckEqualGuestNumber()
        {
            List<GuestList> maleList = db.GuestLists.Where(guest => guest.SexId.Equals(1)).ToList();
            List<GuestList> femaleList = db.GuestLists.Where(guest => guest.SexId.Equals(2)).ToList();

            if (maleList.Count > femaleList.Count)
            {
                ViewBag.info = "List contains " + (maleList.Count-femaleList.Count) + " man more";
            }
            else if (maleList.Count < femaleList.Count)
            {
                ViewBag.info = "List contains " + (femaleList.Count - maleList.Count) + " woman more";
            }
            else
            {
                ViewBag.info = "The list contains the same number of men and women";
            }
        }

        /// <summary>
        /// Create and download GuestList as xml file
        /// </summary>
        /// <returns>xml file</returns>
        public FileContentResult  DownloadXMLFile()
        {
            byte[] file = null;

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlNode rootNode = xmlDoc.CreateElement("GuestList");
            xmlDoc.AppendChild(rootNode);
            xmlDoc.InsertBefore(declaration, rootNode);

            List<GuestList> guestList = db.GuestLists.Include(g => g.Sex).ToList();

            foreach (GuestList person in guestList)
            {
                XmlNode personNode = xmlDoc.CreateElement("Guest");
                XmlAttribute attribute = xmlDoc.CreateAttribute("Id");
                attribute.Value = person.Id.ToString();
                personNode.Attributes.Append(attribute);

                XmlNode firestNameNode = xmlDoc.CreateElement("FirstName");
                firestNameNode.InnerText = person.FirstName;
                personNode.AppendChild(firestNameNode);
                XmlNode lastNameNode = xmlDoc.CreateElement("LastName");
                lastNameNode.InnerText = person.LastName;
                personNode.AppendChild(lastNameNode);
                XmlNode sexNode = xmlDoc.CreateElement("Sex");
                sexNode.InnerText = person.Sex.Name;
                personNode.AppendChild(sexNode);
                XmlNode respondedNode = xmlDoc.CreateElement("Responded");
                respondedNode.InnerText = person.Responded.ToString();
                personNode.AppendChild(respondedNode);

                rootNode.AppendChild(personNode);
            }

            file = Encoding.Default.GetBytes(xmlDoc.OuterXml);

            return File(file, ".xml","GuestList.xml");
        }
    }
}
