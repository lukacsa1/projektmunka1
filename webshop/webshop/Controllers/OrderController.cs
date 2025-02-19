using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webshop.Models;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public IActionResult NewOrder(string token, int productId, int amount, string size)
        {
            if(Manager.CheckPermission(token, 1))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        Termekek product = context.Termekeks.FirstOrDefault(p => p.Id == productId)!;
                        User user = null;
                        if(Manager.LoggedInUsers.TryGetValue(token, out User tempUser))
                        {
                            user = tempUser;
                        }

                        if(product is null)
                        {
                            return NotFound("A termék nem található!");
                        }

                        if(!product.Meret.Contains(size))
                        {
                            return NotFound("A termék nem található ebben a méretben!");
                        }

                        if(amount > 1000)
                        {
                            return BadRequest("Túl sok megrendelt termék!");
                        }

                        return Ok(user.LoginName + "\n" + product.TermekNeve);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Sikertelen megrendelés! " + ex.Message);
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
