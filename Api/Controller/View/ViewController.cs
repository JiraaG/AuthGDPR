using Microsoft.AspNetCore.Mvc;

namespace AuthGDPR.Api.Controller.View
{
    [Route("[controller]/[action]")]
    public class ViewController : Microsoft.AspNetCore.Mvc.Controller // Fully qualify 'Controller' to resolve ambiguity
    {
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("Login");
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string returnUrl)
        {
            // TODO: autenticazione reale
            // Esempio di autenticazione fittizia:
            // if (username != "admin" || password != "password") {
            //     ViewBag.Error = "Credenziali non valide";
            //     ViewBag.ReturnUrl = returnUrl;
            //     return View("~/Api/Controller/View/View.cshtml");
            // }

            // Se autenticazione ok:
            return Redirect(returnUrl ?? "/");
        }
    }
}
