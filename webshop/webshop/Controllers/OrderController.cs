using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using webshop.Models;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpGet("GetOrdersByOrderNumber")]
        public async Task<IActionResult> GetOrdersByOrderNumber(string orderNumber)
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    Order order = await context.Orders.Include(o => o.Orderitems).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                    if (order is null)
                    {
                        return NotFound("Rendelés nem található ilyen rendelés számmal!");
                    }

                    return Ok(order);
                }
                catch (Exception ex)
                {
                    return BadRequest("Nem sikerült lekérni a rendelést! " + ex.Message);
                }
            }
        }

        [HttpGet("GetOrderByUser")]
        public async Task<IActionResult> GetOrderByUser(string token)
        {
            if (Manager.CheckPermission(token, 1))
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
                            return NotFound("Felhasználó nem található!");
                        }

                        List<Order> orders = await context.Orders.Where(o => o.FelhasznaloId == user.Id).Include(o => o.Orderitems).ToListAsync();

                        if(orders.Count == 0)
                        {
                            return NotFound("A felhasználónak nincsenek rendelései!");
                        }

                        return Ok(orders);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült lekérni a rendeléseket! " + ex.Message);
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotExistingMessage);
            }
        }

        [HttpPost("NewOrder")]
        public async Task<IActionResult> NewOrder(OrderDetails orderDetails)
        {
            if (Manager.CheckPermission(orderDetails.token, 1))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        User user = null;

                        string orderNumber = Manager.GenerateOrderNumber();


                        if (Manager.LoggedInUsers.TryGetValue(orderDetails.token, out User tempUser))
                        {
                            user = tempUser;
                        }

                        if (user is null)
                        {
                            return NotFound(Manager.UserNotExistingMessage);
                        }


                        Order order = new Order
                        {
                            FelhasznaloId = user.Id,
                            Status = 0,
                            OrderNumber = orderNumber
                        };

                        await context.Orders.AddAsync(order);
                        await context.SaveChangesAsync();

                        int orderId = context.Orders.FirstOrDefault(o => o.OrderNumber == orderNumber).Id;


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
                                RendelésId = orderId,
                                TermekId = product.Id,
                                Meret = products.size,
                                Darabszam = products.amount,
                            };

                            await context.Orderitems.AddAsync(orderitem);
                        }

                        await context.SaveChangesAsync();

                        return Ok("Sikeres mentés! Rendelés szám: " + orderNumber);
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

        [HttpDelete("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(string token, string orderNumber)
        {
            if (Manager.CheckPermission(token, 1))
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

                        if (user is null)
                        {
                            return NotFound(Manager.UserNotExistingMessage);
                        }

                        Order order = context.Orders.FirstOrDefault(o => o.OrderNumber == orderNumber);

                        if (order is null)
                        {
                            return NotFound("Megrendelés nem található!");
                        }

                        List<Orderitem> orderitems = context.Orderitems.Where(o => o.RendelésId == order.Id).ToList();

                        //if(orderitems.Count == 0)
                        //{
                        //    return NotFound("A megrendelés tárgyai nem találhatóak!");
                        //}

                        foreach (var item in orderitems)
                        {
                            context.Orderitems.Remove(item);
                        }

                        context.Orders.Remove(order);

                        await context.SaveChangesAsync();
                        return Ok("Rendelés sikeresen törölve!");

                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Sikertelen megrendelés törlés! " + ex.Message);
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