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
        public async Task<IActionResult> NewOrder(OrderDetails orderDetails)
        {
            if(Manager.CheckPermission(orderDetails.token, 1))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        Termekek product = context.Termekeks.FirstOrDefault(p => p.Id == orderDetails.productId)!;
                        User user = null;
                        

                        if (Manager.LoggedInUsers.TryGetValue(orderDetails.token, out User tempUser))
                        {
                            user = tempUser;
                        }

                        if(user is null)
                        {
                            return NotFound(Manager.UserNotExistingMessage);
                        }

                        if(product is null)
                        {
                            return NotFound("A termék nem található!");
                        }

                        if(!product.Meret.Contains(orderDetails.size))
                        {
                            return NotFound("A termék nem található ebben a méretben!");
                        }

                        if(orderDetails.amount > 1000)
                        {
                            return BadRequest("Túl sok megrendelt termék!");
                        }

                        Order order = new Order
                        {
                            FelhasznaloId = user.Id,
                            Status = 0
                        };

                        Orderitem orderItem = new Orderitem
                        {

                        };

                        await context.Orders.AddAsync(order);
                        await context.SaveChangesAsync();

                        return Ok("Sikeres mentés!");
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
