using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Core.Application.Features.Lookups.Queries;

namespace RestaurantManagementSystem.WebApi.Controllers
{
    [Authorize]
    public class LookupsController : ApiControllerBase
    {
        [HttpGet("cuisines")]
        public async Task<IActionResult> GetCuisines()
        {
            var result = await Mediator.Send(new GetCuisineTypesQuery());
            return Ok(result);
        }

        [HttpGet("table-types")]
        public async Task<IActionResult> GetTableTypes()
        {
            var result = await Mediator.Send(new GetTableTypesQuery());
            return Ok(result);
        }

        [HttpGet("restaurants")]
        public async Task<IActionResult> GetRestaurants()
        {
            var result = await Mediator.Send(new GetRestaurantLookupQuery());
            return Ok(result);
        }
    }
}
