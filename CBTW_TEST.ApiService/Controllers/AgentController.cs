using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CBTW_TEST.ApiService.Controllers
{
    public class AgentController : ControllerBase
    {
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }
}
