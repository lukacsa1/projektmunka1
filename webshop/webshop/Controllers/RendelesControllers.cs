using Microsoft.AspNetCore.Mvc;
using webshop.Models;

namespace webshop.Controllers
{

    [Route("Rendeles")]
    [ApiController]
    public class RendelesControllers : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            using (var context = new WebshopContext())
            {
                try
                {
                    List<Order> response = context.Orders.ToList();
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    List<Order> hiba = new List<Order>();
                    hiba.Add(new Order()
                    {
                        Id = -1,
                    });
                    return BadRequest(hiba);
                }

            }
        }
    }
}
