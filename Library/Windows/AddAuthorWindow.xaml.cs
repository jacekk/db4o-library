using Db4objects.Db4o;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Library
{
	public partial class AddAuthorWindow : Window
	{
		#region properties
		private IObjectContainer _db;
		public bool isClosedWithSave = false;
		#endregion
		public AddAuthorWindow(ref IObjectContainer db)
		{
			InitializeComponent();
			_db = db;

			var collection = new ObservableCollection<PublicationEditableGrid>();
			foreach (var item in _db.QueryByExample(new Publication()))
			{
				var itemPub = item as Publication;
				collection.Add(new PublicationEditableGrid()
				{
					Title = itemPub.Title,
					Year = itemPub.Year,
					IsAuthor = false
				});
			}
			publicationsGrid.ItemsSource = collection.OrderBy(x => x.Title);
			publicationsGrid.SelectedItem = null;
		}
		private void publicationsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = publicationsGrid.SelectedItem as PublicationEditableGrid;
			item.IsAuthor = !item.IsAuthor;
			publicationsGrid.Items.Refresh();
		}
		private void saveBtn_Click(object sender, RoutedEventArgs e)
		{
			if (LastNameTxtBox.Text.Trim()==String.Empty){
				MessageBox.Show("Pole Nazwisko nie może być puste");
				return;
			}
			var author = new Author();
			author.LastName = LastNameTxtBox.Text.Replace(";","").Trim();
			author.BirthDate = BirthDatepicker.SelectedDate.HasValue ? BirthDatepicker.SelectedDate.Value : DateTime.Now;
			author.Publications = new List<Publication>();
			_db.Store(author);
			var all = _db.Query<Publication>().ToList();
			foreach (var item in publicationsGrid.Items)
			{
				var itemObj = item as PublicationEditableGrid;
				var one = all.Where(x => x.Year == itemObj.Year && x.Title == itemObj.Title).First();
				if (itemObj.IsAuthor)
				{
					author.Publications.Add(one);
					one.Authors.Add(author);
				}
				_db.Store(one.Authors);
			}
			_db.Store(author.Publications);
			isClosedWithSave = true;
			this.Close();
		}
	}
}
