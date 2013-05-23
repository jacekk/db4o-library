using System;
using System.Collections.Generic;

namespace Library
{
	public class Author
	{
		public string LastName { get; set; }
		public DateTime BirthDate { get; set; }
		public List<Publication> Publications { get; set; }
	}
	public class Publication
	{
		public string Title { get; set; }
		public string Publisher { get; set; }
		public int Year { get; set; }
		public decimal Price { get; set; }
		public int PageFrom { get; set; }
		public int PageTo { get; set; }
		public char Type { get; set; }
		public List<Author> Authors { get; set; }
	}
}
