using System;
using System.Data;
using System.Windows.Forms;
using Kursova2.Services;

namespace Kursova2.UI
{
    public partial class BaseLibraryForm : Form
    {
        // Заменено на ИНТЕРФЕЙС
        protected readonly IApiGateway _apiGateway;

        protected DataTable _currentTable;
        protected string _currentTableName;

        protected ComboBox comboBoxTables;
        protected DataGridView dataGridView;
        protected TextBox textBoxSearch;
        protected Button buttonSearch;

        // Конструктор теперь принимает абстракцию IApiGateway
        public BaseLibraryForm(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            InitializeBaseComponents();
        }

        private void InitializeBaseComponents()
        {
            this.Size = new System.Drawing.Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            comboBoxTables = new ComboBox { Location = new System.Drawing.Point(12, 12), Size = new System.Drawing.Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxTables.SelectedIndexChanged += (s, e) =>
            {
                if (comboBoxTables.SelectedItem != null)
                    LoadData(comboBoxTables.SelectedItem.ToString());
            };

            textBoxSearch = new TextBox { Location = new System.Drawing.Point(230, 12), Size = new System.Drawing.Size(200, 24) };

            buttonSearch = new Button { Location = new System.Drawing.Point(440, 10), Size = new System.Drawing.Size(100, 28), Text = "Поиск" };
            buttonSearch.Click += (s, e) => SearchData();

            dataGridView = new DataGridView
            {
                Location = new System.Drawing.Point(12, 50),
                Size = new System.Drawing.Size(960, 480),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { comboBoxTables, textBoxSearch, buttonSearch, dataGridView });
        }

        protected void LoadTables(bool isAdmin)
        {
            var tables = _apiGateway.Data.GetAvailableTables(isAdmin);
            comboBoxTables.Items.Clear();
            foreach (var table in tables) comboBoxTables.Items.Add(table);
            if (comboBoxTables.Items.Count > 0) comboBoxTables.SelectedIndex = 0;
        }

        protected void LoadData(string tableName)
        {
            try
            {
                _currentTableName = tableName;
                _currentTable = _apiGateway.Data.GetTableData(tableName);
                dataGridView.DataSource = _currentTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void SearchData()
        {
            if (string.IsNullOrWhiteSpace(_currentTableName) || string.IsNullOrWhiteSpace(textBoxSearch.Text)) return;
            try
            {
                string colName = "BookName";
                if (_currentTableName == "Author") colName = "AuthorName";
                if (_currentTableName == "Reader") colName = "ReaderName";

                dataGridView.DataSource = _apiGateway.Data.SearchInTable(_currentTableName, colName, textBoxSearch.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }
    }
}