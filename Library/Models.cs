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
	public class PublicationEditableGrid
	{
		public string Title { get; set; }
		public int Year { get; set; }
		public bool IsAuthor { get; set; }
	}
	public class AuthorEditableGrid
	{
		public string LastName { get; set; }
		public string BirthDate { get; set; }
		public bool IsPublication { get; set; }
	}
	public class ComboBoxElement
	{
		public char Key { get; set; }
		public string Value { get; set; }
	}
}
