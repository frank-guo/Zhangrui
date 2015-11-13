using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContactManager.Models;
using System.Data.Entity.Infrastructure;

namespace ContactManager.Controllers
{
    public class AllContactsController : Controller
    {
        private NewContactManagerContext contactsDb = new NewContactManagerContext();

        // GET: Contacts
        public ViewResult Index(Models.Filter filter)
        {
            var contacts = from c in contactsDb.Contacts
                           select c;
            if (!String.IsNullOrEmpty(filter.FirstName))
            {
                contacts = contacts.Where(contact => contact.FirstName.Contains(filter.FirstName));
            }
            if (!String.IsNullOrEmpty(filter.LastName))
            {
                contacts = contacts.Where(contact => contact.LastName.Contains(filter.LastName));
            }
            if (!String.IsNullOrEmpty(filter.EmailAddress))
            {
                contacts = contacts.Where(contact => contact.EmailAddress.Contains(filter.EmailAddress));
            }

            if (filter.MyLatitude!=null && filter.MyLongitude!=null)
            {
                contacts = contacts.Where(contact => contact.EmailAddress.Contains(filter.EmailAddress));
            }

            var contactsList = contacts.ToList<Contact>();
            return View(contactsList);
        }

        // GET: AllContacts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var contact = contactsDb.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // GET: AllContacts/Create
        public ViewResult Create()
        {
            return View();
        }

        //POST: AllContacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Contact contact)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!String.IsNullOrEmpty(contact.Address))
                    {
                        var coordinate = GetCoordinate(contact.Address);
                        contact.Latitude = coordinate.Item1;
                        contact.Longitude = coordinate.Item2;
                    }
                    
                    contactsDb.Contacts.Add(contact);
                    contactsDb.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException)
            {
                //Log the error 
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(contact);
        }

        private Tuple<string, string> GetCoordinate(String address)
        {
            var url = "http://maps.googleapis.com/maps/api/geocode/xml?address=";
            var stringArray = address.Split(' ');
            var formatedAddr = "";
            for (int i = 0; i < stringArray.Length; i++ )
            {
                formatedAddr += stringArray[i];
                if (i < stringArray.Length -1)
                {
                    formatedAddr += '+';
                }
            }
            url += formatedAddr;

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                var response = httpWebRequest.GetResponse() as HttpWebResponse;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());

                var geometry = xmlDoc.GetElementsByTagName("geometry")[0];
                var latitude = geometry.FirstChild.FirstChild.InnerText;
                var longitude = geometry.FirstChild.ChildNodes[1].InnerText;

                return new Tuple<string, string>(latitude, longitude);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        // GET: AllContacts/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            var contact = contactsDb.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: AllContacts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var contact = contactsDb.Contacts.Find(id);
                contactsDb.Contacts.Remove(contact);
                contactsDb.SaveChanges();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                //Log the error 
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        // GET: AllContacts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var student = contactsDb.Contacts.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: AllContacts/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var contactToUpdate = contactsDb.Contacts.Find(id);
            if (TryUpdateModel(contactToUpdate, "",
               new string[] { "LastName", "FirstName", "Address", "EmailAddress", "PhoneNumber" }))
            {
                try
                {
                    contactsDb.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error 
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(contactToUpdate);
        }
    }
}



