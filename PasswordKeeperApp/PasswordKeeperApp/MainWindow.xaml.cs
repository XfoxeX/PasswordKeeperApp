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
using System.Timers;
using System.Windows.Threading;

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

        // Timer for an alert status text
        Timer alertTimer = new Timer();

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
        private void TextBox_Enter(object sender, RoutedEventArgs e)
        {
            // Fetch textbox
            TextBox textBox = sender as TextBox;

            textBox.Background = new SolidColorBrush(Colors.White);
        }

        // Add placeholder event
        private void TextBox_Leave(object sender, RoutedEventArgs e)
        {
            // Fetch textbox
            TextBox textBox = sender as TextBox;

            showWatermark(textBox);
        }

        // Show watermark method
        private void showWatermark(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Text != "")
                {
                    textBox.Text = "";
                }

                // Set watermark content
                string textBoxWatermark = null;

                if (textBox.Name == "SearchTextBox")
                {
                    textBoxWatermark = "Search Site";
                }
                else if (textBox.Name == "editSiteName" || textBox.Name == "newSiteName")
                {
                    textBoxWatermark = "Site name";
                }
                else
                {
                    textBoxWatermark = "Password";
                }

                // Create an VisualBrush.
                VisualBrush textVisualBrush = new VisualBrush();
                textVisualBrush.Stretch = Stretch.None;

                // Create the watermark
                Label searchBoxWatermark = new Label();

                searchBoxWatermark.HorizontalContentAlignment = HorizontalAlignment.Center;
                searchBoxWatermark.Width = 10000;
                searchBoxWatermark.Content = textBoxWatermark;
                searchBoxWatermark.HorizontalAlignment = HorizontalAlignment.Center;
                searchBoxWatermark.Foreground = new SolidColorBrush(Colors.Gray);
                searchBoxWatermark.Background = new SolidColorBrush(Colors.White);
                searchBoxWatermark.FontSize = 12;
                searchBoxWatermark.FontStyle = FontStyles.Italic;

                // Insert watermark to textbox
                textVisualBrush.Visual = searchBoxWatermark;
                textBox.Background = textVisualBrush;
            }
        }

        //////////////////////////////////////////////////////

        //////////////////////////////////////////////////////
        /// Add new password method
        private void InsertBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCommand command = new SqlCommand(
                    $"INSERT INTO [Passwords] (Name, Password) VALUES (@Name, @Password)",
                    sqlConnection);

                command.Parameters.AddWithValue("Name", newSiteName.Text);
                command.Parameters.AddWithValue("Password", newPassword.Text);
                command.ExecuteNonQuery();

                LoadDbData();

                ShowStatusAlert("Success");

                // Delete data from textboxes
                newSiteName.Text = "";
                newPassword.Text = "";

                // Add watermark to textboxes
                showWatermark(newSiteName);
                showWatermark(newPassword);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ShowStatusAlert("Error");
            }
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

            // Set textbox backgroud
            editSiteName.Background = new SolidColorBrush(Colors.White);
            editPassword.Background = new SolidColorBrush(Colors.White);
        }

        //////////////////////////////////////////////////////
        /// Status alert

        // Show message box
        private void ShowStatusAlert(String status)
        {
            // Message settings
            if (status == "Success")
            {
                AlertBox.Background = new SolidColorBrush(Colors.LightGreen);
                AlertLabel.Content = "Success";

            }else if(status == "Error")
            {
                AlertBox.Background = new SolidColorBrush(Colors.LightPink);
                AlertLabel.Content = "Error";
            }

            // Show message
            AlertBox.Visibility = Visibility.Visible;

            // Wait 3 sec -> Close message box
            DelayAction(2000, new Action(() => { this.CloseStatusAlert(); }));


        }

        // Close message box method
        private void CloseStatusAlert()
        {
            AlertBox.Visibility = Visibility.Collapsed;
        }

        // Timer implementations
        private static void DelayAction(int millisecond, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
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
