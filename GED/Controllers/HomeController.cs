using GED.Models;
using System;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace GED.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        [HttpPost]
        public ActionResult PostFile(HttpPostedFileBase file)
        {
            ViewBag.UploadSucess = false;
            if (file.ContentLength > 0)
            {
                ArquivoController arquivo = new ArquivoController
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new System.Web.Http.HttpConfiguration()
                };
                HttpResponseMessage responseMessage = arquivo.Upload();
                if (responseMessage.IsSuccessStatusCode)
                {
                    ViewBag.UploadSucess = true;
                    return View("Index");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nenhum arquivo enviado.");
            }

            return View("Index");
        }

        public ActionResult EditFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditFile(Guid id, HttpPostedFileBase file)
        {
            ViewBag.EditSucess = false;
            if (file.ContentLength > 0)
            {
                ArquivoController arquivo = new ArquivoController
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new System.Web.Http.HttpConfiguration()
                };
                HttpResponseMessage responseMessage = arquivo.Edit(id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    ViewBag.EditSucess = true;
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nenhum arquivo enviado.");
            }

            return View();
        }

        public ActionResult Stream(Guid id)
        {
            Uri myuri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);
            string pathQuery = myuri.PathAndQuery;
            string hostName = myuri.ToString().Replace(pathQuery, "");
            ViewBag.ApiUri = hostName + "/api/Media/Play/" + id.ToString();

            return View();
        }
    }
}
