using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Core.Application.Features.Restaurants.Commands;
using RestaurantManagementSystem.Core.Application.Features.Restaurants.Queries;

namespace RestaurantManagementSystem.WebApi.Controllers
{
    [Authorize]
    public class RestaurantsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int? cuisineTypeId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetRestaurantListQuery(
                CuisineTypeId: cuisineTypeId,
                Status: status,
                SearchTerm: searchTerm,
                SortBy: sortBy,
                IsAscending: isAscending,
                PageNumber: pageNumber,
                PageSize: pageSize
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var result = await Mediator.Send(new GetRestaurantDetailsQuery(id));
            if (result == null)
            {
                return NotFound(new { Message = "Restaurant not found." });
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> Create([FromBody] AddRestaurantCommand command)
        {
            var commandWithUser = command with { CurrentUserId = CurrentUserId };
            var result = await Mediator.Send(commandWithUser);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRestaurantCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { Message = "Restaurant ID mismatch." });
            }

            var commandWithUser = command with { CurrentUserId = CurrentUserId };
            var result = await Mediator.Send(commandWithUser);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await Mediator.Send(new DeleteRestaurantCommand(id, CurrentUserId));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
