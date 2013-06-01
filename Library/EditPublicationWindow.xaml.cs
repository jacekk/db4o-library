using Db4objects.Db4o;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Library
{
	public partial class EditPublicationWindow : Window
	{
		#region properties
		private IObjectContainer _db;
		private dynamic _publication;
		private bool _isBook;
		public bool isClosedWithSave = false;
		#endregion
		public EditPublicationWindow(ref IObjectContainer db, dynamic publication)
		{
			InitializeComponent();
			TypeCombo.ItemsSource = StaticData.publicationTypes;
			_db = db;
			_publication = publication;
			_isBook = _publication is Book;
			Title += _publication.Title;

			// fill form
			TitleTxtBox.Text = _publication.Title;
			PublisherTxtBox.Text = _publication.Publisher;
			YearTxtBox.Text = _publication.Year.ToString();
            if (_isBook)
            {
				TypeCombo.SelectedIndex = 1;
                PriceTxtBox.Text = _publication.Price.ToString();
				PageFromTxtBox.Text = String.Empty;
				PageToTxtBox.Text = String.Empty;
            } else {
				TypeCombo.SelectedIndex = 0;
				PriceTxtBox.Text = String.Empty;
                PageFromTxtBox.Text = _publication.PageFrom.ToString();
                PageToTxtBox.Text = _publication.PageTo.ToString();
            }
			toggleTxtBoxesVisibility();

			var collection = new ObservableCollection<AuthorEditableGrid>();
			foreach (var item in _db.QueryByExample(new Author()))
			{
				var itemAut = item as Author;
				collection.Add(new AuthorEditableGrid()
				{
					LastName = itemAut.LastName,
					BirthDate = itemAut.BirthDate.ToShortDateString(),
					IsPublication = _publication.Authors.Contains(itemAut)
				});
			}
			authorsGrid.ItemsSource = collection.OrderBy(x => x.LastName);
			authorsGrid.SelectedItem = null;
		}
		private void toggleTxtBoxesVisibility()
		{
			PriceTxtBox.IsEnabled = _isBook;
			PageFromTxtBox.IsEnabled = !_isBook;
			PageToTxtBox.IsEnabled = !_isBook;
		}
		#region validation
		private void DigitsOnly_TextChanged(object sender, TextChangedEventArgs e)
		{
			var input = sender as TextBox;
			var regex = new System.Text.RegularExpressions.Regex("[0-9]");
			var value = String.Empty;
			foreach (var item in regex.Matches(input.Text))
				if (value.Length < 4)
					value += item.ToString();
			input.Text = value;
			input.CaretIndex = 99;
		}
		private void PriceTxtBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var caret = PriceTxtBox.CaretIndex;
			var regex = new System.Text.RegularExpressions.Regex(@"[0-9,\.]");
			var value = String.Empty;
			bool separatorIncluded = false;
			foreach (var item in regex.Matches(PriceTxtBox.Text))
			{
				var ch = item.ToString().Replace(".",",");
				if (ch == ",")
				{
					if (separatorIncluded == false)
						value += ch;
					separatorIncluded = true;
				}
				else
				{
					value += ch;
				}
			}
			PriceTxtBox.Text = value;
			PriceTxtBox.CaretIndex = caret;
		}
		#endregion
		private void authorsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = authorsGrid.SelectedItem as AuthorEditableGrid;
			item.IsPublication = !item.IsPublication;
			authorsGrid.Items.Refresh();
		}
		private void saveBtn_Click(object sender, RoutedEventArgs e)
		{
			if (TitleTxtBox.Text == String.Empty
				|| PublisherTxtBox.Text == String.Empty
				|| YearTxtBox.Text == String.Empty
				|| (_isBook && PriceTxtBox.Text == String.Empty)
				|| (!_isBook && PageFromTxtBox.Text == String.Empty)
				|| (!_isBook && PageToTxtBox.Text == String.Empty)
			)
			{
				MessageBox.Show("Wypełnij wszystkie pola");
				return;
			}
			_publication.Title = TitleTxtBox.Text.Replace(";", "").Trim();
			_publication.Publisher = PublisherTxtBox.Text.Replace(";", "").Trim();
			_publication.Year = Convert.ToInt32(YearTxtBox.Text);
			if (_isBook)
			{
				_publication.Price = Convert.ToDecimal(PriceTxtBox.Text);
			}
			else
			{
				_publication.PageFrom = Convert.ToInt32(PageFromTxtBox.Text);
				_publication.PageTo = Convert.ToInt32(PageToTxtBox.Text);
			}
			_publication.Authors = new List<Author>();
            _db.Store(_publication);

			var all = _db.Query<Author>().ToList();
			foreach (var item in authorsGrid.Items)
			{
				var itemObj = item as AuthorEditableGrid;
				var one = all.Where(x => x.LastName == itemObj.LastName).First();
				if (itemObj.IsPublication)
				{
					_publication.Authors.Add(one);
                    if (!one.Publications.Contains(_publication))
                        one.Publications.Add(_publication);
				}
				else
				{
					if (one.Publications.Contains(_publication))
						one.Publications.Remove(_publication);
				}
                _db.Store(one.Publications);
			}
			_db.Store(_publication.Authors);
            _db.Commit();
			isClosedWithSave = true;
			this.Close();
		}

		private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// TODO: finish publication type change
			toggleTxtBoxesVisibility();
		}
	}
}
