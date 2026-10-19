using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Models;

/// <summary>
/// Helper class untuk pagination
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class PaginatedList<T>
{
    public List<T> Items { get; init; } = new();
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList()
    {
    }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items ?? new List<T>();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
