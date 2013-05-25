using Db4objects.Db4o;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

			LastNameTxtBox.Text = author.LastName;
			BirthDatepicker.SelectedDate = author.BirthDate;

			var collection = new ObservableCollection<PublicationEdit>();
			foreach (var item in _db.QueryByExample(new Publication()))
			{
				var itemPub = item as Publication;
				collection.Add(new PublicationEdit()
				{
					Title = itemPub.Title,
					Year = itemPub.Year,
					IsAuthor = author.Publications.Contains(itemPub)
				});
			}
			publicationsGrid.ItemsSource = collection;
			publicationsGrid.SelectedItem = null;
		}

		private void publicationsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = publicationsGrid.SelectedItem as PublicationEdit;
			item.IsAuthor = !item.IsAuthor;
			publicationsGrid.Items.Refresh();
		}

		private void saveBtn_Click(object sender, RoutedEventArgs e)
		{
			if (LastNameTxtBox.Text.Trim()==String.Empty){
				MessageBox.Show("Pole Nazwisko niemoże być puste");
				return;
			}
			author.LastName = LastNameTxtBox.Text.Trim();
			author.BirthDate = BirthDatepicker.SelectedDate.HasValue ? BirthDatepicker.SelectedDate.Value : DateTime.Now;
			_db.Store(author);

			// START HERE

			MessageBox.Show("Zmiany zapisane");
			this.Close();
		}
	}
}
