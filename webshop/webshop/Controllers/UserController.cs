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
        public async Task<IActionResult> DeleteUser(string token, int DeleteUserId)
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

                        context.Remove(user);
                        await context.SaveChangesAsync();

                        return Ok("Felhasználó sikeresen törölve!");
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

        [HttpPost("RequestChangePassword")]
        public IActionResult UserRequestChangePassword(string email, string username)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    User user = context.Users.FirstOrDefault(u => u.Email == email && u.LoginName == username);

                    if(user is null)
                    {
                        return NotFound("Felhasználó nem található ilyen e-mail címmel vagy felhasználónévvel!");
                    }

                    Manager.SendEmail(email, "Jelszó visszaállítás", $"https://localhost:7117/api/User/RecoverPassword?loginName={user.LoginName}&email={user.Email}&authCode={Manager.GenerateAuthCode()}");

                    return Ok("Jelszó váltási kérelem feldolgozva, ellenőrizze az emailjeit!");
                }
                catch (Exception ex)
                {
                    return BadRequest("Nem sikerült elküldeni a kérelmet! " + ex.Message);
                }
            }
        }

        [HttpPost("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword(string loginName, string email, string authCode, string tmpHash)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    User user = context.Users.FirstOrDefault(u => u.LoginName == loginName && u.Email == email);

                    if(user is null)
                    {
                        return NotFound("Nincs ilyen felhasználó!");
                    }

                    if(Manager.CheckAuthCode(user, authCode))
                    {
                        Manager.PasswordRecoveryCodes.Remove(user);

                        user.Salt = Manager.GenerateSalt();
                        user.Hash = Manager.CreateSHA256(tmpHash);

                        context.Users.Update(user);
                        await context.SaveChangesAsync();

                        Manager.SendEmail(email, "Jelszó visszaállítás", "Jelszavad sikeresen visszaállítva!");

                        return Ok("Jelszó sikeresen módosítva!");
                    }
                    return BadRequest("A felhasználó nem kért jelszó visszaállítást!");
                }
                catch (Exception ex)
                {
                    return BadRequest("Nem sikerült visszaállítani a jelszót! " + ex.Message);
                }
            }
        }
    }
}
