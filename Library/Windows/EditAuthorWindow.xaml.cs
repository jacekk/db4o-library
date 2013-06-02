using Db4objects.Db4o;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Library
{
	public partial class EditAuthorWindow : Window
	{
		#region properties
		private IObjectContainer _db;
		private Author _author;
		public bool isClosedWithSave = false;
		#endregion
		public EditAuthorWindow(ref IObjectContainer db, Author author)
		{
			InitializeComponent();
			_db = db;
			_author = author;
			Title += _author.LastName;

			// fill form
			LastNameTxtBox.Text = _author.LastName;
			BirthDatepicker.SelectedDate = _author.BirthDate;

			var collection = new ObservableCollection<PublicationEditableGrid>();
			foreach (var item in _db.QueryByExample(new Publication()))
			{
				var itemPub = item as Publication;
				collection.Add(new PublicationEditableGrid()
				{
					Title = itemPub.Title,
					Year = itemPub.Year,
					IsAuthor = _author.Publications.Contains(itemPub)
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
			_author.LastName = LastNameTxtBox.Text.Replace(";","").Trim();
			_author.BirthDate = BirthDatepicker.SelectedDate.HasValue ? BirthDatepicker.SelectedDate.Value : DateTime.Now;
			_author.Publications = new List<Publication>();
			_db.Store(_author);
			var all = _db.Query<Publication>().ToList();
			foreach (var item in publicationsGrid.Items)
			{
				var itemObj = item as PublicationEditableGrid;
				var one = all.Where(x => x.Year == itemObj.Year && x.Title == itemObj.Title).First();
				if (itemObj.IsAuthor)
				{
					_author.Publications.Add(one);
					if (!one.Authors.Contains(_author))
						one.Authors.Add(_author);
				}
				else
				{
					if (one.Authors.Contains(_author))
						one.Authors.Remove(_author);
				}
				_db.Store(one.Authors);
			}
            _db.Store(_author.Publications);
            _db.Commit();
			isClosedWithSave = true;
			this.Close();
		}
	}
}
