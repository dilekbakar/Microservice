using Common.Library.Models.Page;
using Microsoft.EntityFrameworkCore;

namespace Common.Library.Infrastructure.Extensions
{
    public static class PagingExtensions
    {
        public static PagedViewModel<T> GetPaged<T>(this IQueryable<T> query, int currentPage, int pageSize) where T : class
        {
            var count = query.Count();

            Page paging = new(currentPage, pageSize, count);

            var data = query.Skip(paging.Skip).Take(paging.PageSize).AsNoTracking().ToList();

            var result = new PagedViewModel<T>(data, paging);

            return result;
        }
    }
}
