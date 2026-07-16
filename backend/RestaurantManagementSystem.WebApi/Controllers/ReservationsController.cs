using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Core.Application.Features.Reservations.Commands;
using RestaurantManagementSystem.Core.Application.Features.Reservations.Queries;

namespace RestaurantManagementSystem.WebApi.Controllers
{
    [Authorize]
    public class ReservationsController : ApiControllerBase
    {
        // 1. Manager Queue
        [HttpGet]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> GetList(
            [FromQuery] Guid? restaurantId,
            [FromQuery] string? status,
            [FromQuery] DateOnly? reservationDate,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetReservationListQuery(
                RestaurantId: restaurantId,
                Status: status,
                ReservationDate: reservationDate,
                SearchTerm: searchTerm,
                SortBy: sortBy,
                IsAscending: isAscending,
                PageNumber: pageNumber,
                PageSize: pageSize
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }

        // 2. Customer Dashboard
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyReservations(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMyReservationsQuery(
                CustomerId: CurrentUserId,
                SearchTerm: searchTerm,
                SortBy: sortBy,
                IsAscending: isAscending,
                PageNumber: pageNumber,
                PageSize: pageSize
            );

            var result = await Mediator.Send(query);
            return Ok(result);
        }

        // 3. Customer: Place Request
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestReservation([FromBody] RequestReservationCommand command)
        {
            var commandWithUser = command with { CustomerId = CurrentUserId };
            var result = await Mediator.Send(commandWithUser);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // 4. Manager Actions
        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await Mediator.Send(new ConfirmReservationCommand(id, CurrentUserId));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/checkin")]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> CheckIn(Guid id)
        {
            var result = await Mediator.Send(new CheckInReservationCommand(id, CurrentUserId));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "RestaurantManager")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await Mediator.Send(new CompleteReservationCommand(id, CurrentUserId));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // 5. Shared Action (Customer can cancel pending, Manager can cancel active)
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await Mediator.Send(new CancelReservationCommand(id, CurrentUserId, IsManager));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
