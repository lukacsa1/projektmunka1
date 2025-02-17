using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        [HttpPost("{token}")]
        public IActionResult Logout(string token)
        {
            if (Manager.LoggedInUsers.ContainsKey(token))
            {
                Manager.LoggedInUsers.Remove(token);
                return Ok("Sikeres kijelentkeztetés!");
            }
            else return NotFound("Felhasználó nem található!");
        }
    }
}
