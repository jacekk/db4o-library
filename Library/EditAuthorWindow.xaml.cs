using Db4objects.Db4o;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Library
{
	public partial class EditAuthorWindow : Window
	{
		#region properties
		private IObjectContainer _db;
		public Author author;
		#endregion
		public EditAuthorWindow(ref IObjectContainer db, Author _author)
		{
			InitializeComponent();
			_db = db;
			author = _author;
			Title += author.LastName;
		}
	}
}
