using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webshop.Models;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        [HttpPost("UserRegistration")]
        public async Task<IActionResult> UserRegistration(User user)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    if(context.Users.FirstOrDefault(u => u.LoginName == user.LoginName) is not null)
                    {
                        return BadRequest("Ez a felhasználónév már foglalt!");
                    }
                    if(context.Users.FirstOrDefault(u => u.Email == user.Email) is not null)
                    {
                        return BadRequest("Ez az E-mail cím már foglalt!");
                    }

                    user.PermissionLevel = 0;
                    user.Active = 0;
                    user.Hash = Manager.CreateSHA256(user.Hash);
                    await context.AddAsync(user);
                    await context.SaveChangesAsync();

                    //TODO: send Email
                    Manager.SendEmail(user.Email, "Regisztráció", $"https://localhost:7117/api/Registration/FinishRegistration?loginName={user.LoginName}&email={user.Email}");

                    return Ok("A regisztráció véglegesítéséhez ellenőrizd az Emailjeid!");
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
