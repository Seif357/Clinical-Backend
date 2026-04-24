using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Application.Dto;

public static class QueryableExtensions
{
    public static async Task<PagedResponse<TResult>> ToPagedAsync<TResult>(
        this IQueryable<TResult> query,
        int page,
        int pageSize)
    {
        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<TResult>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }
    public static async Task<PagedResponse<TResult>> ToPagedDtoAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TResult>> selector,
        int page,
        int pageSize)
    {
        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync();

        return new PagedResponse<TResult>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }
}