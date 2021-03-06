﻿using Db4objects.Db4o;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Library
{
	public partial class AddPublicationWindow : Window
	{
		#region properties
		private IObjectContainer _db;
		public bool isClosedWithSave = false;
		#endregion
		public AddPublicationWindow(ref IObjectContainer db)
		{
			InitializeComponent();
			TypeCombo.ItemsSource = StaticData.publicationTypes;
			_db = db;

			var collection = new ObservableCollection<AuthorEditableGrid>();
			foreach (var item in _db.QueryByExample(new Author()))
			{
				var itemAut = item as Author;
				collection.Add(new AuthorEditableGrid()
				{
					LastName = itemAut.LastName,
					BirthDate = itemAut.BirthDate.ToShortDateString(),
					IsPublication = false
				});
			}
			authorsGrid.ItemsSource = collection.OrderBy(x => x.LastName);
			authorsGrid.SelectedItem = null;
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
		private void toggleTxtBoxesVisibility()
		{
			var isBook = TypeCombo.SelectedIndex == 1;
			PriceTxtBox.IsEnabled = isBook;
			PageFromTxtBox.IsEnabled = !isBook;
			PageToTxtBox.IsEnabled = !isBook;
		}
		private void authorsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = authorsGrid.SelectedItem as AuthorEditableGrid;
			item.IsPublication = !item.IsPublication;
			authorsGrid.Items.Refresh();
		}
		private void saveBtn_Click(object sender, RoutedEventArgs e)
		{
			var isBook = TypeCombo.SelectedIndex == 1;
			if (TypeCombo.SelectedIndex == -1
				|| TitleTxtBox.Text == String.Empty
				|| PublisherTxtBox.Text == String.Empty
				|| YearTxtBox.Text == String.Empty
				|| (isBook && PriceTxtBox.Text == String.Empty)
				|| (!isBook && PageFromTxtBox.Text == String.Empty)
				|| (!isBook && PageToTxtBox.Text == String.Empty)
			)
			{
				MessageBox.Show("Wypełnij wszystkie pola");
				return;
			}
			dynamic publication;
			if (isBook)
			{
				publication = new Book();
				publication.Price = Convert.ToDecimal(PriceTxtBox.Text);
			}
			else
			{
				publication = new Article();
				publication.PageFrom = Convert.ToInt32(PageFromTxtBox.Text);
				publication.PageTo = Convert.ToInt32(PageToTxtBox.Text);
			}
			publication.Title = TitleTxtBox.Text.Replace(";", "").Trim();
			publication.Publisher = PublisherTxtBox.Text.Replace(";", "").Trim();
			publication.Year = Convert.ToInt32(YearTxtBox.Text);
			publication.Authors = new List<Author>();
			_db.Store(publication);
			var all = _db.Query<Author>().ToList();
			foreach (var item in authorsGrid.Items)
			{
				var itemObj = item as AuthorEditableGrid;
				var one = all.Where(x => x.LastName == itemObj.LastName).First();
				if (itemObj.IsPublication)
				{
					publication.Authors.Add(one);
					one.Publications.Add(publication);
				}
			}
			_db.Store(publication.Authors);
			isClosedWithSave = true;
			this.Close();
		}

		private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			toggleTxtBoxesVisibility();
		}
	}
}
