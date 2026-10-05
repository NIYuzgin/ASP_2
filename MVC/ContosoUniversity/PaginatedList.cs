using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; set; }
		public int TotalPages { get; set; }
		public PaginatedList(List<T> items, int count, int pageIndex, int pageSize )
		{ 
			this.PageIndex = pageIndex;
			this.TotalPages = count;

			this.AddRange(items);
		}

		public bool HasPreviousPage=> PageIndex >1;
		public bool HasNextPage=> PageIndex < TotalPages;
		public static async Task<PaginatedList<T>> CreateAsync
			(
			IQueryable<T> sourse,
			int pageIndex,
			int pageSize
			)
		{
			int count=await sourse.CountAsync();
			List<T> items =await sourse.Skip((pageIndex - 1) * pageSize)
										.Take(count)
										.ToListAsync();
			return new PaginatedList<T> (items, count, pageIndex, pageSize);

		}

	}

}
