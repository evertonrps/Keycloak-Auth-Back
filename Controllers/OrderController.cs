using CustomerApplication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderController : ControllerBase
    {
        // GET: api/<OrderController>
        [HttpGet]
        [Authorize(Policy = "admins")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<OrderController>/5
        [HttpGet("{id}")]
        [Authorize(Policy = "noaccess")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet("carros")]
        [Authorize]
        public IEnumerable<string> GetCars()
        {
            return new string[] { "Gol", "Passat" };
        }


        [HttpGet("cars")]
        //[AllowAnonymous]
        //[Authorize(Policy = "admins")]
        [Authorize(Policy = "users")]
        public IActionResult ListCars()
        {
            List<Car> cars = new List<Car>();
            cars.Add(new Car { Id = 1, Color = "Red", Model = "Jeep", Price = 1000 });
            cars.Add(new Car { Id = 2, Color = "Blue", Model = "Terrytory", Price = 75000 });
            return Ok(cars);
        }

        [HttpGet("games")]        
        public IEnumerable<string> GetGames()
        {
            return new string[] { "GOD", "Naruto" };
        }

        // POST api/<OrderController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
