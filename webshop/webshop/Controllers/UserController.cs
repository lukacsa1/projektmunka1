using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using webshop.Models;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("GetAll")]
        public IActionResult GetAllUsers(string token)
        {
            if (Manager.CheckPermission(token, 9))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        List<User> users = context.Users.ToList();
                        return Ok(users);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült lekérni a felhasználókat! " + ex.Message);
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotEligableMessage);
            }
        }

        [HttpGet("GetByToken")]
        public IActionResult GetUserByToken(string token)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    User user = null;
                    if (Manager.LoggedInUsers.TryGetValue(token, out User tempUser))
                    {
                        user = tempUser;
                    }
                    else
                    {
                        return NotFound("A felhasznaló nem található!");
                    }

                    return Ok(user);
                }
                catch (Exception ex)
                {
                    return BadRequest("Nem sikerült lekérni a felhasználót! " + ex.Message);
                }
            }
        }

        [HttpGet("GetById")]
        public IActionResult GetUserById(string token, int id)
        {
            if (Manager.CheckPermission(token, 9))
            {
                using (var context = new WebshopContext())
                {

                    try
                    {
                        User user = context.Users.FirstOrDefault(u => u.Id == id);
                        if(user is null)
                        {
                            return NotFound("Nem található felhasználó ilyen azonosítóval!");
                        }

                        return Ok(user);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült lekérni a felhaszanálót! " + ex.Message);
                    }
                }

            }
            else
            {
                return Unauthorized(Manager.UserNotEligableMessage);
            }
        }
    }
}
