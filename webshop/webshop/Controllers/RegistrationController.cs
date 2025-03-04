using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webshop.DTOs;
using webshop.Models;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        [HttpPost("UserRegistration")]
        public async Task<IActionResult> UserRegistration(RegistrationDTO registrationUser)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    if(context.Users.FirstOrDefault(u => u.LoginName == registrationUser.LoginName) is not null)
                    {
                        return BadRequest("Ez a felhasználónév már foglalt!");
                    }
                    if(context.Users.FirstOrDefault(u => u.Email == registrationUser.Email) is not null)
                    {
                        return BadRequest("Ez az E-mail cím már foglalt!");
                    }

                    User user = new User
                    {
                        LoginName = registrationUser.LoginName,
                        Email = registrationUser.Email,
                        FirstName = registrationUser.FirstName,
                        LastName = registrationUser.LastName,
                        Hash = Manager.CreateSHA256(registrationUser.TempHash),
                        Salt = registrationUser.Salt,
                        PermissionLevel = 0,
                        Active = 0,
                        RegistarionDate = DateTime.Now,
                    };

                    await context.AddAsync(user);
                    await context.SaveChangesAsync();

                    //TODO: send Email
                    Manager.SendEmail(registrationUser.Email, "Regisztráció", $"https://localhost:7117/api/Registration/FinishRegistration?loginName={registrationUser.LoginName}&email={registrationUser.Email}");

                    return Ok(user);
                }
                catch (Exception ex)
                {
                    return BadRequest("Nem sikerült a regisztráció! " + ex.Message);
                }
            }
        }

        [HttpPost("FinishRegistration")]
        public async Task<IActionResult> EndOfRegistration(string loginName, string email)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    User user = await context.Users.FirstOrDefaultAsync(u => u.LoginName == loginName && u.Email == email);


                    if(user is null)
                    {
                        return BadRequest("Sikertelen regisztráció! A felhasználó nem regésztrált!");
                    }

                    if(user.Active == 1)
                    {
                        return BadRequest("A felhasználó már regisztrálva van!");
                    }

                    user.Active = 1;
                    context.Users.Update(user);
                    await context.SaveChangesAsync();

                    return Ok("A felhasználó sikeresen regisztrálva!");
                }
                catch (Exception ex)
                {
                    return BadRequest("Sikertelen regisztráció! " + ex.Message);
                }
            }
        }

        [HttpGet("GetNewSalt")]
        public IActionResult GenerateNewSalt()
        {
            return Ok(Manager.GenerateSalt());
        }
    }
}
