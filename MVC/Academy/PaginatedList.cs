using Microsoft.EntityFrameworkCore;

namespace Academy
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; set; }
		public int TotalPages { get; set; }

		public PaginatedList(List<T> items, int count, int pageindex, int pagesize)
		{
			this.PageIndex = pageindex;
			this.TotalPages = (int)Math.Ceiling((double)count / pagesize);

			this.AddRange(items);
		}

		public bool HasPreviousPage => PageIndex > 1;
		public bool HasNextPage => PageIndex < TotalPages;

		public static async Task<PaginatedList<T>> CreateAsync
			(
				IQueryable<T> sourse,
				int pageIndex,
				int pageSize
			)
			{
				int count = await sourse.CountAsync();
				List<T> items = await sourse.Skip((pageIndex - 1) * pageSize)
											.Take(pageSize)
											.ToListAsync();
				return new PaginatedList<T>(items, count, pageIndex, pageSize);
			}

	}
}


