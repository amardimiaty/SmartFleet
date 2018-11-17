using System.Threading.Tasks;
using System.Web.Mvc;
using SmartFleet.Service.Authentication;
using SmartFLEET.Web.Models.Account;

namespace SmartFLEET.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home", new { area = "" });
            return View(new LoginModel());
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home", new { area = "" });
            if (!ModelState.IsValid) return View(model);
            var userExists =await _authenticationService.Authentication(model.UserName, model.Password, model.RememberMe);
            if (userExists!=null)
            {
                foreach (var identityUserRole in _authenticationService.GetRoleByUserId(userExists.Id))
                {

                    if (identityUserRole.Equals("customer") || identityUserRole.Equals("user"))
                        return RedirectToAction("Index", "Home");

                }
                return RedirectToAction("Index", "Admin", new {area = "Administrator"});
            }

            return View();

        }
        [HttpGet]
        public ActionResult Logout()
        {
            _authenticationService.Logout();
            return RedirectToAction("Login", "Account", new { area = "" });
        }
    }
}