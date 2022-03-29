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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace PasswordKeeperApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection sqlConnection = null;

        // List for all db data
        private List<string[]> dbRows = null;
        // List of filtered db data
        private List<string[]> filteredDbRows = new List<string[]>();

        SqlDataReader dataReader = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        //////////////////////////////////////////////////////
        /// Main form load
        private void MainPageLoaded_ev(object sender, RoutedEventArgs e)
        {
            // SQL connection value
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["PasswordKeeperDB"].ConnectionString);

            // Connect to DataBase
            sqlConnection.Open();

            // Load data from db to dbRows list
            LoadDbData();
        }
        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Filter for searchListView data

        // Search textbox event
        private void SearchTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            filterData();
        }

        // Data Filtering Method
        private void filterData()
        {
            filteredDbRows = dbRows.Where((x) =>
                x[1].ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();

            RefreshList(filteredDbRows);
        }

        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Load and refresh db data from list to ListView
        private void RefreshList(List<string[]> list)
        {
            SearchListView.Items.Clear();

            foreach (string[] item in list)
            {
                SearchListView.Items.Add(new ListViewClass() { PassId = item[0], SiteName = item[1], Password = item[2] });
            }
        }
        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Load DB data method
        private void LoadDbData()
        {
            string[] dbRow = null;
            dbRows = new List<string[]>();

            try
            {
                SqlCommand sqlCommand = new SqlCommand(
                    "SELECT * FROM Passwords",
                    sqlConnection);

                dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    dbRow = new string[]
                    {
                        Convert.ToString(dataReader["Id"]),
                        Convert.ToString(dataReader["Name"]),
                        Convert.ToString(dataReader["Password"]),
                    };

                    dbRows.Add(dbRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }

            // Load list data to ListView
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                RefreshList(dbRows);
            }
            else
            {
                filterData();
            }

        }


        //////////////////////////////////////////////////////
        /// Adding placeholder text to "Find password" textbox

        // Remove placeholder event
        private void SearchTextBox_Enter(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Background != null)
            {
                SearchTextBox.Background = null;
            }
        }

        // Add placeholder event
        private void SearchTextBox_Leave(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                if (SearchTextBox.Text != "")
                {
                    SearchTextBox.Text = "";
                }

                // Create an ImageBrush.
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri(@"D:\PasswordKeeper\PasswordKeeper\Image\lofiGirl.gif", UriKind.Relative)
                    );
                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.Stretch = Stretch.None;

                SearchTextBox.Background = textImageBrush;
            }
        }
        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Add new password method
        private void InsertBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand command = new SqlCommand(
                    $"INSERT INTO [Passwords] (Name, Password) VALUES (@Name, @Password)",
                    sqlConnection);

            command.Parameters.AddWithValue("Name", newSiteName.Text);
            command.Parameters.AddWithValue("Password", newPassword.Text);
            command.ExecuteNonQuery();

            LoadDbData();
        }
        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Delete password

        // Open delete window
        private void DeleteBtnClick(object sender, RoutedEventArgs e)
        {
            string msgtext = "Are you sure?";
            string boxTitle = "Removing password";
            MessageBoxButton buttonList = MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show(msgtext, boxTitle, buttonList);

            switch (result)
            {
                case MessageBoxResult.OK:
                    RemovePassword(sender);
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        private void RemovePassword(object sender)
        {
            // Fetch item id
            Button b = sender as Button;
            ListViewClass listViewClass = b.CommandParameter as ListViewClass;

            // Delete an item from the database
            SqlCommand command = new SqlCommand(
                $"DELETE FROM [Passwords] WHERE Id = {int.Parse(listViewClass.PassId)}",
                sqlConnection);

            command.ExecuteNonQuery();

            // Reload data in the ListView
            LoadDbData();
        }
        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Edit password

        // Show edit password menu
        private void showEditMenu(object sender, RoutedEventArgs e)
        {
            // Fetch item data
            Button b = sender as Button;
            ListViewClass listViewClass = b.CommandParameter as ListViewClass;

            // Insert item data to edit menu rows
            editPasswordId.Text = listViewClass.PassId;
            editSiteName.Text = listViewClass.SiteName;
            editPassword.Text = listViewClass.Password;

            // Show edit menu
            EditPasswordStackPanel.Visibility = Visibility.Visible;
        }

        // Edit password data
        private void EditBtnClick(object sender, RoutedEventArgs e)
        {
            // Edit an item in the database
            SqlCommand command = new SqlCommand(
                $"UPDATE [Passwords] SET Name = N'{editSiteName.Text}', Password = N'{editPassword.Text}' WHERE Id = {int.Parse(editPasswordId.Text)}",
                sqlConnection);

            command.ExecuteNonQuery();

            CloseEditBtnClick(sender, e);

            // Reload data in the ListView
            LoadDbData();
        }

        // Close edit menu
        private void CloseEditBtnClick(object sender, RoutedEventArgs e)
        {
            // Close edit menu
            EditPasswordStackPanel.Visibility = Visibility.Collapsed;

            // Delete data from edit menu
            editPasswordId.Text = "";
            editSiteName.Text = "";
            editPassword.Text = "";
        }
    }

    // List view item create class
    class ListViewClass
    {
        public string PassId { get; set; }
        public string SiteName { get; set; }
        public string Password { get; set; }
    }
}
