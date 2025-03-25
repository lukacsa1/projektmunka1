using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using webshop.DTOs;
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
                        List<User> users = context.Users.Include(u => u.SzamlazasiCim).ToList();
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
        [HttpPut("Admin/UpdateUser")]
        public async Task<IActionResult> UpdateUser(string token, int userId, UpdateUserDTO updateUser)
        {
            if(Manager.CheckPermission(token, 9))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        User user = context.Users.FirstOrDefault(u => u.Id == userId);
                        if(user is null)
                        {
                            return NotFound("Nem található felhasználó ilyen azonosítóval!");
                        }

                        User tempUpdateUser = new User
                        {
                            Active = (int)(updateUser.Active == null ? user.Active : updateUser.Active),
                            Email = updateUser.Email == null ? user.Email : updateUser.Email,
                            FirstName = updateUser.FirstName == null ? user.FirstName : updateUser.FirstName,
                            LastName = updateUser.LastName == null ? user.LastName : updateUser.LastName,
                            LoginName = updateUser.LoginName == null ? user.LoginName : updateUser.LoginName,
                            PermissionLevel = (int)(updateUser.PermissionLevel == null ? user.PermissionLevel : updateUser.PermissionLevel),
                            PhoneNumber = updateUser.PhoneNumber == null ? user.PhoneNumber : updateUser.PhoneNumber,
                            RegistarionDate = (DateTime)(updateUser.RegistrationDate == null ? user.RegistarionDate : updateUser.RegistrationDate)
                        };

                        user.Active = tempUpdateUser.Active;
                        user.Email = tempUpdateUser.Email;
                        user.FirstName = tempUpdateUser.FirstName;
                        user.LastName = tempUpdateUser.LastName;
                        user.LoginName = tempUpdateUser.LoginName;
                        user.PhoneNumber = tempUpdateUser.PhoneNumber;
                        user.RegistarionDate = tempUpdateUser.RegistarionDate;
                        
                        context.Users.Update(user);
                        await context.SaveChangesAsync();

                        return Ok("Felhasználó sikeresen módosítva!");
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült módosítani a felhasználó adatait! " + ex.Message);
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotExistingMessage);
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

        [HttpPost("RequestRecoverPassword")]
        public IActionResult UserRequestRecoverPassword(string email, string username)
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

        [HttpPut("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordDTO param)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    User user = context.Users.FirstOrDefault(u => u.LoginName == param.loginName && u.Email == param.email);

                    if(user is null)
                    {
                        return NotFound("Nincs ilyen felhasználó!");
                    }

                    if(Manager.CheckAuthCode(user, param.authCode))
                    {
                        Manager.PasswordRecoveryCodes.Remove(user);

                        user.Salt = Manager.GenerateSalt();
                        user.Hash = Manager.CreateSHA256(param.tmpHash);

                        context.Users.Update(user);
                        await context.SaveChangesAsync();

                        Manager.SendEmail(param.email, "Jelszó visszaállítás", "Jelszavad sikeresen visszaállítva!");

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

        [HttpPut("ChangePassword")]
        
        public async Task<IActionResult> ChangePassword(string token, [FromBody] ChangePasswordDTO changepass)
        {
            if(Manager.CheckIfUserLoggedIn(token))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        User user = null;
                        if(Manager.LoggedInUsers.TryGetValue(token, out User tempUser))
                        {
                            user = tempUser;
                        }

                        if(Manager.CreateSHA256(changepass.OldPasswordHash) == user.Hash)
                        {
                            user.Hash = Manager.CreateSHA256(changepass.NewPasswordHash);
                            user.Salt = changepass.NewSalt;

                            context.Users.Update(user);
                            await context.SaveChangesAsync();
                            return Ok("Jelszó sikeresen módosítva!");
                        }
                        else
                        {
                            return BadRequest("A jelszó nem egyezik!");
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült módosítani a jelszót! " + ex.Message);
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotExistingMessage);
            }
        }

        [HttpPut("UpdateUserDetails")]
        public async Task<IActionResult> UpdateUserDetails(string token, ChangeUserDetailsDTO updateUser)
        {
            if(Manager.CheckIfUserLoggedIn(token))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        User user = null;
                        if(Manager.LoggedInUsers.TryGetValue(token, out User tempUser))
                        {
                            user = tempUser;
                        }
                        else
                        {
                            return NotFound("Felhasználó nem található!");
                        }

                        user.LoginName = updateUser.loginName;
                        user.LastName = updateUser.lastName;
                        user.FirstName = updateUser.firstName;
                        user.PhoneNumber = updateUser.phoneNumber;

                        context.Users.Update(user);
                        await context.SaveChangesAsync();
                        return Ok("Felhasználó adatok sikeresen módosítva!");
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült módosítani a felhasználó adatait! " + ex.Message);
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotExistingMessage);
            }
        }
    }
}
