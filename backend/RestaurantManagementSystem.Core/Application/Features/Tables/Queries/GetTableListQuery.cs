using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Tables.Queries
{
    public record GetTableListQuery(
        Guid? RestaurantId,
        int? TableTypeId,
        string? Status,
        string? SearchTerm,
        string? SortBy,
        bool IsAscending = true,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedList<TableListDto>>;

    public class TableListDto
    {
        public Guid Id { get; set; }
        public required string RestaurantName { get; set; }
        public required string TableNumber { get; set; }
        public required string TableTypeName { get; set; }
        public int SeatingCapacity { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class GetTableListQueryHandler : IRequestHandler<GetTableListQuery, PaginatedList<TableListDto>>
    {
        private readonly ITableRepository _tableRepository;

        public GetTableListQueryHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<PaginatedList<TableListDto>> Handle(GetTableListQuery request, CancellationToken cancellationToken)
        {
            var pagedTables = await _tableRepository.GetPagedAsync(
                restaurantId: request.RestaurantId,
                tableTypeId: request.TableTypeId,
                status: request.Status,
                searchTerm: request.SearchTerm,
                sortBy: request.SortBy,
                isAscending: request.IsAscending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize
            );

            var dtos = pagedTables.Items.Select(t => new TableListDto
            {
                Id = t.Id,
                RestaurantName = t.Restaurant?.Name ?? "Unknown",
                TableNumber = t.TableNumber,
                TableTypeName = t.TableType?.Name ?? "Unknown",
                SeatingCapacity = t.SeatingCapacity,
                Status = t.Status,
                CreatedDate = t.CreatedDate,
                UpdatedDate = t.UpdatedDate
            }).ToList();

            return new PaginatedList<TableListDto>(
                dtos,
                pagedTables.TotalCount,
                pagedTables.PageNumber,
                request.PageSize
            );
        }
    }
}
