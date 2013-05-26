using Db4objects.Db4o;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Library
{
    public partial class MainWindow : Window
	{
		#region properties
		private IObjectContainer _db;
		private readonly string _dbPath = "library.db";
		#endregion
		public MainWindow()
        {
            InitializeComponent();
			_db = Db4oEmbedded.OpenFile(_dbPath);
			showAuthorsList();
        }
		#region files operations
		private string[] loadFile()
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "CSV|*.csv";
			if (dlg.ShowDialog() == true)
			{
				var content = String.Empty;
				var sr = new StreamReader(dlg.FileName);
				try
				{
					content = sr.ReadToEnd();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
				finally
				{
					sr.Close();
				}
				if (content == String.Empty)
				{
					MessageBox.Show("Brak danych do parsowania");
					return new string[0];
				}
				var lines = content.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
				if (lines.Count() <= 1)
					MessageBox.Show("Brak linii z danymi");
				return lines;
			}
			return new string[0];
		}
		private void saveFile(string defaultTitle,StringBuilder content)
		{
			var dlg = new SaveFileDialog();
			dlg.Filter = "CSV|*.csv";
			dlg.FileName = defaultTitle + ".csv";
			if(dlg.ShowDialog() == true){
				var path = dlg.FileName;
				if (path == String.Empty)
					MessageBox.Show("Ścieżka nie może buć pusta");
				else
				{
					try
					{
						File.WriteAllText(path, content.ToString());
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.ToString());
					}
					if (File.Exists(path))
						MessageBox.Show("plik zapisano pomyślnie", "sukces", MessageBoxButton.OK, MessageBoxImage.None);
					else
						MessageBox.Show("niestety", "błąd", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
		#endregion
		#region btns
		private void clearDatabaseBtn_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Czy na pewno chcesz wyczyścić bazę?", "Potwierdzenie",
				MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				_db.Close();
				try
				{
					File.Delete(_dbPath);
				}
				catch (Exception) { }
				_db = Db4oEmbedded.OpenFile(_dbPath);
				MessageBox.Show("Baza została wyczyszczona");
				authorsGrid.Visibility = Visibility.Collapsed;
				publicationsGrid.Visibility = Visibility.Collapsed;
			}
		}
		private void addAuthorsBtn_Click(object sender, RoutedEventArgs e)
        {
			var lines = loadFile();
			if (lines.Count() == 0)
				return;
			var counter = 0;
			for (int i = lines.Count() - 1; i > 0; i--)
			{
				if (lines[i].IndexOf(';') != -1)
				{
					string[] elements = lines[i].Split(new string[] { ";" }, StringSplitOptions.None);
					if (elements.Count() == 2)
					{
						var birthDate = DateTime.Parse(elements[1].Trim());
						var check = _db.QueryByExample(new Author() { LastName = elements[0], BirthDate = birthDate });
						if (check.Count == 0)
						{
							_db.Store(new Author()
							{
								BirthDate = birthDate,
								LastName = elements[0].Trim(),
								Publications = new List<Publication>()
							});
							counter++;
						}
					}
				}
			}
			var message = String.Format("Ilość dodanych autorów: {0}",counter);
			MessageBox.Show(message);
			showAuthorsList();
        }
		private void addPublicationBtn_Click(object sender, RoutedEventArgs e)
		{
			var lines = loadFile();
			if (lines.Count() == 0)
				return;
			var counter = 0;
			for (int i = lines.Count() - 1; i > 0; i--)
			{
				if (lines[i].IndexOf(';') != -1)
				{
					string[] elements = lines[i].Split(new string[] { ";" }, StringSplitOptions.None);
					if (elements.Count() == 7)
					{
						var check = _db.QueryByExample(new Publication() {
							Title = elements[0].Trim(),
							Publisher = elements[1].Trim(),
							Type = elements[6].Trim().ToCharArray().First()
						});
						if (check.Count == 0)
						{
							_db.Store(new Publication()
							{
								Title = elements[0].Trim(),
								Publisher = elements[1].Trim(),
								Year = elements[2] != "" ? Int32.Parse(elements[2].Trim()) : 0,
								Price = elements[3] != "" ? Convert.ToDecimal(elements[3].Trim()) : 0,
								PageFrom = elements[4] != "" ? Int32.Parse(elements[4].Trim()) : 0,
								PageTo = elements[5] != "" ? Int32.Parse(elements[5].Trim()) : 0,
								Type = elements[6].Trim().ToCharArray().First(),
								Authors = new List<Author>()
							});
							counter++;
						}
					}
				}
			}
			var message = String.Format("Ilość dodanych publikacji: {0}", counter);
			MessageBox.Show(message);
			showPublicationList();
		}
		private void addRelationsBtn_Click(object sender, RoutedEventArgs e)
		{
			var lines = loadFile();
			if (lines.Count() == 0)
				return;
			var counterA = 0;
			var counterP = 0;
			for (int i = lines.Count() - 1; i > 0; i--)
			{
				if (lines[i].IndexOf(';') != -1)
				{
					string[] elements = lines[i].Split(new string[] { ";" }, StringSplitOptions.None);
					if (elements.Count() == 2)
					{
						var pubCheck = _db.QueryByExample(new Publication() { Title = elements[0].Trim() });
						var autCheck = _db.QueryByExample(new Author() { LastName = elements[1].Trim() });
						if (pubCheck.Count == 1 && autCheck.Count == 1)
						{
							var publication = pubCheck.Next() as Publication;
							var author = autCheck.Next() as Author;
							if (!publication.Authors.Contains(author))
							{
								publication.Authors.Add(author);
								_db.Store(publication.Authors);
								counterA++;
							}
							if (!author.Publications.Contains(publication))
							{
								author.Publications.Add(publication);
								_db.Store(author.Publications);
								counterP++;
							}
						}
						else
							MessageBox.Show(String.Format(
								"Relacja nie pasująca do niczego:\n{0}\n{1}",
								elements[0].Trim(),
								elements[1].Trim()
							));
					}
				}
			}
			_db.Commit();
			MessageBox.Show(String.Format("Dodano relacje:\nautorów: {0}\npubliacji: {1}", counterP, counterA));
			showAuthorsList();
		}
		private void showAuthorsBtn_Click(object sender, RoutedEventArgs e)
		{
			showAuthorsList();
		}
		private void showAuthorsList()
		{
			var authors = _db.QueryByExample(new Author());
			var collection = new ObservableCollection<Author>();
			foreach (var item in authors)
				collection.Add(item as Author);
			authorsListGrid.ItemsSource = collection.OrderBy(x => x.LastName);
			authorsListGrid.SelectedItem = null;
			authorsCount.Content = authors.Count.ToString();

			authorsGrid.Visibility = Visibility.Visible;
			publicationsGrid.Visibility = Visibility.Hidden;
			authorPublicationsListGrid.Visibility = Visibility.Collapsed;
			authorsPublicationsNone.Visibility = Visibility.Collapsed;
			editAuthorBtn.Visibility = Visibility.Collapsed;
			authorSearchBox.Text = String.Empty; // TODO do not override filter
		}
		private void showPublicationBtn_Click(object sender, RoutedEventArgs e)
		{
			showPublicationList();
		}
		private void showPublicationList()
		{
			var publications = _db.QueryByExample(new Publication());
			var collection = new ObservableCollection<Publication>();
			foreach (var item in publications)
				collection.Add(item as Publication);
			publicationsListGrid.ItemsSource = collection.OrderBy(x => x.Title);
			publicationsListGrid.SelectedItem = null;
			publicationsCount.Content = publications.Count.ToString();

			authorsGrid.Visibility = Visibility.Collapsed;
			publicationsGrid.Visibility = Visibility.Visible;
			publicationAuthorsListGrid.Visibility = Visibility.Collapsed;
			publicationAuthorsNone.Visibility = Visibility.Collapsed;
			editPublicationBtn.Visibility = Visibility.Collapsed;
			publicationSearchBox.Text = String.Empty; // TODO do not override filter
		}
		private void exportAuthorsBtn_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendLine("Nazwisko;DataUr");
			_db.Query<Author>().OrderBy(x => x.LastName).ToList().ForEach(x => {
				sb.AppendLine(String.Format("{0};{1}",
					x.LastName,
					x.BirthDate.ToShortDateString()
				));
			});
			saveFile("autorzy",sb);
		}
		private void exportPublicationsBtn_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendLine("Tytul;Wydawnictwo;Rok;Cena;OdStrony;Do Strony;Rodzaj");
			_db.Query<Publication>().OrderBy(x => x.Title).ToList().ForEach(x =>
			{
				sb.AppendLine(String.Format("{0};{1};{2};{3};{4};{5};{6}",
					x.Title,
					x.Publisher,
					x.Year,
					x.Price,
					x.PageFrom,
					x.PageTo,
					x.Type
				));
			});
			saveFile("publikacje", sb);
		}
		private void exportRelationsBtn_Click(object sender, RoutedEventArgs e)
		{
			var sb = new StringBuilder();
			sb.AppendLine("Tytul;Nazwisko");
			_db.Query<Publication>().OrderBy(x => x.Title).ToList().ForEach(publication =>
			{
				publication.Authors.ForEach(author => {
					sb.AppendLine(String.Format("{0};{1}",
						publication.Title,
						author.LastName
					));
				});
			});
			saveFile("relacje", sb);
		}
		#endregion
		#region grids
		private void authorsListGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			showAuthorPublications();
		}
		private void authorsListGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			showAuthorPublications();
		}
		private void showAuthorPublications()
		{
			if (authorsListGrid.SelectedItem == null)
				return;
			var selected = authorsListGrid.SelectedItem as Author;
			var publications = _db.QueryByExample(new Publication());
			var collection = new ObservableCollection<Publication>();
			foreach(var item in publications){
				var pubItem = item as Publication;
				if (pubItem.Authors != null && pubItem.Authors.Contains(selected))
					collection.Add(pubItem);
			}
			authorPublicationsListGrid.ItemsSource = collection.OrderBy(x => x.Title);
			authorPublicationsListGrid.SelectedItem = null;
			if (collection.Count() != 0)
			{
				authorPublicationsListGrid.Visibility = Visibility.Visible;
				authorsPublicationsNone.Visibility = Visibility.Collapsed;
				authorsPublicationsCount.Content = collection.Count().ToString();
			}
			else
			{
				authorPublicationsListGrid.Visibility = Visibility.Collapsed;
				authorsPublicationsNone.Visibility = Visibility.Visible;
				authorsPublicationsCount.Content = String.Empty;
			}
			editAuthorBtn.Visibility = Visibility.Visible;
		}
		private void publicationsListGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			showPublicationAuthors();
		}
		private void publicationsListGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			showPublicationAuthors();
		}
		private void showPublicationAuthors()
		{
			if (publicationsListGrid.SelectedItem == null)
				return;

			var selected = publicationsListGrid.SelectedItem as Publication;
			var authors = _db.QueryByExample(new Author());
			var collection = new ObservableCollection<Author>();
			foreach (var item in authors)
			{
				var pubItem = item as Author;
				if (pubItem.Publications != null && pubItem.Publications.Contains(selected))
					collection.Add(pubItem);
			}
			publicationAuthorsListGrid.ItemsSource = collection.OrderBy(x => x.LastName);
			publicationAuthorsListGrid.SelectedItem = null;
			if (collection.Count() != 0)
			{
				publicationAuthorsListGrid.Visibility = Visibility.Visible;
				publicationAuthorsNone.Visibility = Visibility.Collapsed;
				publicationsAuthorsCount.Content = collection.Count().ToString();
			}
			else
			{
				publicationAuthorsListGrid.Visibility = Visibility.Collapsed;
				publicationAuthorsNone.Visibility = Visibility.Visible;
				publicationsAuthorsCount.Content = String.Empty;
			}
			editPublicationBtn.Visibility = Visibility.Visible;
		}
		#endregion
		#region search
		private void authorSearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var phrase = (sender as TextBox).Text.ToLower();
			if (phrase.Length == 0)
			{
				showAuthorsList();
			}
			else
			{
				authorsListGrid.ItemsSource = _db.Query<Author>(delegate(Author item) {
					return item.LastName.ToLower().IndexOf(phrase) != -1;
				});
			}
			authorPublicationsListGrid.Visibility = Visibility.Collapsed;
			authorsPublicationsNone.Visibility = Visibility.Collapsed;
		}
		private void publicationSearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var phrase = (sender as TextBox).Text.ToLower();
			if (phrase.Length == 0)
			{
				showPublicationList();
			}
			else
			{
				publicationsListGrid.ItemsSource = _db.Query<Publication>(delegate(Publication item) {
					return item.Title.ToLower().IndexOf(phrase) != -1
						|| item.Publisher.ToLower().IndexOf(phrase) != -1;
				});
			}
			publicationAuthorsListGrid.Visibility = Visibility.Collapsed;
			publicationAuthorsNone.Visibility = Visibility.Collapsed;
		}
		#endregion
		#region edit
		private void authorsListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			editAuthor();
		}
		private void editAuthorBtn_Click(object sender, RoutedEventArgs e)
		{
			editAuthor();
		}
		private void editAuthor()
		{
			var author = authorsListGrid.SelectedItem as Author;
			var dlg = new EditAuthorWindow(ref _db, author);
			dlg.ShowDialog();
			if (dlg.isClosedWithSave)
			{
				showAuthorsList();
				authorsListGrid.SelectedItem = author;
			}
		}
		private void editPublicationBtn_Click(object sender, RoutedEventArgs e)
		{
			editPublication();
		}
		private void publicationAuthorsListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			editPublication();
		}
		private void editPublication()
		{
			var publication = publicationsListGrid.SelectedItem as Publication;
			var dlg = new EditPublicationWindow(ref _db, publication);
			dlg.ShowDialog();
			if (dlg.isClosedWithSave){
				showPublicationList();
				publicationsListGrid.SelectedItem = publication;
			}
		}
		#endregion
	}
}
