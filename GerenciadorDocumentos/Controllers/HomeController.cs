using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace GerenciadorDocumentos.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public ActionResult PostFile(HttpPostedFileBase file)
        {
            ViewBag.UploadSucess = false;
            //Montar API Upload URL
            string strPathAndQuery = HttpContext.Request.Url.PathAndQuery;
            string apiUrl = HttpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/") + "api/Upload";

            if (file.ContentLength > 0)
            {
                UploadController upload = new UploadController
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new System.Web.Http.HttpConfiguration()
                };
                HttpResponseMessage responseMessage = upload.Post();
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
    }
}
