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
        [HttpGet("Admin/GetAll")]
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

        [HttpGet("Admin/GetById")]
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

        [HttpGet("Admin/GetByPermission")]
        public IActionResult GetUsersByPermission(string token, int permission)
        {
            if(Manager.CheckPermission(token, 9))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        List<User> users = context.Users.Where(u => u.PermissionLevel == permission).ToList();

                        if(users.Count == 0)
                        {
                            return NotFound("Nem található felhasználó ezzel a jogosultsági szinttel!");
                        }

                        return Ok(users);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült lekérni a felhasználókat!");
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotEligableMessage);
            }
        }

        [HttpDelete("Admin/Delete")]
        public IActionResult DeleteUser(string token, int DeleteUserId)
        {
            if(Manager.CheckPermission(token, 9))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        User user = context.Users.FirstOrDefault(u => u.Id == DeleteUserId);

                        if(user is null)
                        {
                            return NotFound("Nem található felhasználó ilyen azonosítóval");
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült törölni a felhasználót! " + ex.Message);
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
