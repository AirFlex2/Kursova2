using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kursova2.Network;

namespace Kursova2.UI
{
    public partial class BaseLibraryForm : Form
    {
        protected readonly ApiClient _apiClient;

        protected DataTable _currentTable;
        protected string _currentTableName;

        protected ComboBox comboBoxTables;
        protected DataGridView dataGridView;
        protected TextBox textBoxSearch;
        protected Button buttonSearch;

        public BaseLibraryForm(ApiClient apiClient)
        {
            _apiClient = apiClient;
            InitializeBaseComponents();
        }

        private void InitializeBaseComponents()
        {
            this.Size = new System.Drawing.Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            comboBoxTables = new ComboBox { Location = new System.Drawing.Point(12, 12), Size = new System.Drawing.Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxTables.SelectedIndexChanged += async (s, e) =>
            {
                if (comboBoxTables.SelectedItem != null)
                    await LoadDataAsync(comboBoxTables.SelectedItem.ToString());
            };

            textBoxSearch = new TextBox { Location = new System.Drawing.Point(230, 12), Size = new System.Drawing.Size(200, 24) };

            buttonSearch = new Button { Location = new System.Drawing.Point(440, 10), Size = new System.Drawing.Size(100, 28), Text = "Пошук" };
            buttonSearch.Click += SearchData_Click;

            dataGridView = new DataGridView
            {
                Location = new System.Drawing.Point(12, 50),
                Size = new System.Drawing.Size(960, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { comboBoxTables, textBoxSearch, buttonSearch, dataGridView });
        }

        protected async Task LoadTablesAsync(bool isAdmin)
        {
            try
            {
                var tables = await _apiClient.GetAvailableTablesAsync(isAdmin);
                comboBoxTables.Items.Clear();
                foreach (var table in tables) comboBoxTables.Items.Add(table);
                if (comboBoxTables.Items.Count > 0) comboBoxTables.SelectedIndex = 0;
            }
            catch (Exception ex) { MessageBox.Show("Помилка завантаження списку таблиць: " + ex.Message); }
        }

        protected async Task LoadDataAsync(string tableName)
        {
            try
            {
                _currentTableName = tableName;
                _currentTable = await _apiClient.GetTableDataAsync(tableName);
                dataGridView.DataSource = _currentTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}");
            }
        }

        private async void SearchData_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentTableName) || string.IsNullOrWhiteSpace(textBoxSearch.Text)) return;
            try
            {
                string colName = "BookName";
                if (_currentTableName == "Author") colName = "AuthorName";
                if (_currentTableName == "Reader") colName = "ReaderName";

                dataGridView.DataSource = await _apiClient.SearchInTableAsync(_currentTableName, colName, textBoxSearch.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка пошуку: {ex.Message}");
            }
        }
    }
}