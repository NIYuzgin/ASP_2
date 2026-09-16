using System.ComponentModel.DataAnnotations;

namespace Academy.Models
{
	public class Human
	{

		[Required]
		[StringLength(50, MinimumLength =2)]
		public string last_name { get; set; }
		[RegularExpression("^[A-ZА-Я][a-zа-я]+$")]
		
		[Required]
		[StringLength(50, MinimumLength = 2)]
		public string first_name { get; set; }

		public string? middle_name { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateOnly birth_date { get; set; }

		[EmailAddress]
		public string? email { get; set; }

		[Phone]
		public string? phone { get; set; }

		public byte[]? photo { get; set; }

	}
}
