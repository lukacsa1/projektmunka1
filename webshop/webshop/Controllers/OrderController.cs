using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using webshop.DTOs;
using webshop.Models;

namespace webshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpGet("GetAllOrders")]
        public async Task<IActionResult> GetAll(string token)
        {
            if (Manager.CheckPermission(token, 9))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        var orders = await context.Orders
                        .Include(o => o.Orderitems)
                        .Include(o => o.Rendelesszamlazas) // Kapcsolat a számlázási adatokkal
                        .Select(o => new OrderDTO
                        {
                            Id = o.Id,
                            Datum = o.Datum,
                            Status = o.Status,
                            OrderNumber = o.OrderNumber,
                            Orderitems = o.Orderitems.Select(oi => new OrderItemsDTO
                            {
                                Id = oi.Id,
                                TermekId = oi.TermekId,
                                Meret = oi.Meret,
                                Darabszam = oi.Darabszam
                            }).ToList(),
                            Szamlazas = o.Rendelesszamlazas != null ? new RendelesSzamlazasDTO
                            {
                                Nev = o.Rendelesszamlazas.Nev,
                                Email = o.Rendelesszamlazas.Email,
                                Telefonszam = o.Rendelesszamlazas.Telefonszam,
                                Orszag = o.Rendelesszamlazas.Orszag,
                                Varos = o.Rendelesszamlazas.Varos,
                                Utca = o.Rendelesszamlazas.Utca,
                                Hazszam = o.Rendelesszamlazas.Hazszam,
                                Iranyitoszam = o.Rendelesszamlazas.Iranyitoszam
                            } : null!
                        })
                        .ToListAsync();

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
                return Unauthorized(Manager.UserNotEligableMessage);
            }
        }


        [HttpGet("GetOrdersByOrderNumber")]
        public async Task<IActionResult> GetOrdersByOrderNumber(string token, string orderNumber)
        {
            if (Manager.CheckIfUserLoggedIn(token))
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

                        if (user == null)
                        {
                            return NotFound("A felhasználó nem található!");
                        }

                        Order order = context.Orders.Include(o => o.Orderitems).FirstOrDefault(o => o.OrderNumber == orderNumber);

                        var orderOut = await context.Orders
                        .Where(o => o.OrderNumber == orderNumber)
                        .Include(o => o.Orderitems)
                        .Select(o => new OrderDTO
                        {
                            Id = o.Id,
                            OrderNumber = o.OrderNumber,
                            Datum = o.Datum,
                            Status = o.Status,
                            Orderitems = o.Orderitems.Select(oi => new OrderItemsDTO
                            {
                                Id = oi.Id,
                                TermekId = oi.TermekId,
                                Meret = oi.Meret,
                                Darabszam = oi.Darabszam
                            }).ToList()
                        })
                        .FirstOrDefaultAsync();

                        if (order is null)
                        {
                            return NotFound("Rendelés nem található ilyen rendelés számmal!");
                        }

                        if (order.FelhasznaloId != user.Id)
                        {
                            return Unauthorized("A rendelés nem ehhez a felhasználóhoz tartozik!");
                        }

                        return Ok(orderOut);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Nem sikerült lekérni a rendelést! " + ex.Message);
                    }
                }
            }
            else
            {
                return Unauthorized(Manager.UserNotExistingMessage);
            }
        }

        [HttpGet("GetOrdersByUser")]
        public async Task<IActionResult> GetOrdersByUser(string token)
        {
            if (Manager.CheckIfUserLoggedIn(token))
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

                        if (orders.Count == 0)
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
        public async Task<IActionResult> NewOrder(string token, [FromBody] NewOrderDTO newOrder)
        {
            if (Manager.CheckIfUserLoggedIn(token))
            {
                using (var context = new WebshopContext())
                {
                    try
                    {
                        User user = null;

                        string orderNumber = Manager.GenerateOrderNumber();


                        if (Manager.LoggedInUsers.TryGetValue(token, out User tempUser))
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


                        foreach (var products in newOrder.orderProducts)
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
                            await context.SaveChangesAsync();
                        }


                        Rendelesszamlazas szamlazas = new Rendelesszamlazas
                        {
                            Nev = newOrder.billing.Name,
                            Email = newOrder.billing.Email,
                            Telefonszam = newOrder.billing.PhoneNumber,
                            Iranyitoszam = newOrder.billing.PostalCode,
                            Orszag = newOrder.billing.Country,
                            Varos = newOrder.billing.City,
                            Utca = newOrder.billing.Street,
                            Hazszam = newOrder.billing.HouseNumber,
                            RendelesId = orderId
                        };

                        await context.Rendelesszamlazas.AddAsync(szamlazas);
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
            if (Manager.CheckIfUserLoggedIn(token))
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

                        if (order.FelhasznaloId != user.Id)
                        {
                            return Unauthorized("A rendelés nem ehhez a felhasználóhoz tartozik!");
                        }

                        List<Orderitem> orderitems = context.Orderitems.Where(o => o.RendelésId == order.Id).ToList();

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