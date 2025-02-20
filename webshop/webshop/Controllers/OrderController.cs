using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
                        User user = null;
                        

                        if (Manager.LoggedInUsers.TryGetValue(orderDetails.token, out User tempUser))
                        {
                            user = tempUser;
                        }

                        if(user is null)
                        {
                            return NotFound(Manager.UserNotExistingMessage);
                        }

                        
                        Order order = new Order
                        {
                            FelhasznaloId = user.Id,
                            Status = 0
                        };

                        await context.Orders.AddAsync(order);
                        await context.SaveChangesAsync();


                        foreach (var products in orderDetails.product)
                        {

                            Termekek product = context.Termekeks.FirstOrDefault(p => p.Id == products.Id)!;


                            if (product is null)
                            {
                                return NotFound($"A {product.TermekNeve} termék nem található!");
                            }

                            if (!product.Meret.Contains(product.Meret))
                            {
                                return NotFound($"A {product.TermekNeve} termék nem található ebben a méretben!");
                            }

                            if (products.amount > 1000)
                            {
                                return BadRequest("Túl sok megrendelt termék!");
                            }

                            Orderitem orderitem = new Orderitem
                            {
                                RendelésId = 0,
                                TermekId = product.Id,
                                Meret = product.Meret,
                                Darabszam = products.amount,
                            };
                        }

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
