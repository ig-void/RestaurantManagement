using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Core.Application.Features.Tables.Commands;
using RestaurantManagementSystem.Core.Application.Features.Tables.Queries;

namespace RestaurantManagementSystem.WebApi.Controllers
{
    [Authorize(Roles = "RestaurantManager")]
    public class TablesController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] Guid? restaurantId,
            [FromQuery] int? tableTypeId,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetTableListQuery(
                RestaurantId: restaurantId,
                TableTypeId: tableTypeId,
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddTableCommand command)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTableCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { Message = "Table ID mismatch." });
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
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await Mediator.Send(new DeleteTableCommand(id, CurrentUserId));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
