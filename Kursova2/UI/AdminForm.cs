using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Kursova2.Services;

namespace Kursova2.UI
{
    public class AdminForm : BaseLibraryForm
    {
        private Button btnSave, btnDelete;
        private Button btnGroup, btnRank, btnAuthors, btnFines;
        private Button btnChartGroup;

        // Заменено на интерфейс
        public AdminForm(IApiGateway apiGateway) : base(apiGateway)
        {
            this.Text = "Библиотека - Панель Администратора";
            dataGridView.ReadOnly = false;
            InitializeAdminComponents();
            LoadTables(isAdmin: true);
        }

        private void InitializeAdminComponents()
        {
            btnSave = new Button { Location = new Point(550, 10), Size = new Size(100, 28), Text = "Сохранить" };
            btnSave.Click += (s, e) => {
                if (_currentTable != null)
                {
                    _apiGateway.Data.SaveChanges(_currentTableName, _currentTable.GetChanges());
                    _currentTable.AcceptChanges();
                    MessageBox.Show("Изменения сохранены!");
                }
            };

            btnDelete = new Button { Location = new Point(660, 10), Size = new Size(100, 28), Text = "Удалить" };
            btnDelete.Click += (s, e) => {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                        if (!row.IsNewRow) dataGridView.Rows.Remove(row);
                }
            };

            btnGroup = new Button { Location = new Point(12, 540), Size = new Size(150, 30), Text = "Групування книг", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnGroup.Click += (s, e) => ShowAnalytics(_apiGateway.Analytics.GetBooksGroupedByGenre());

            btnRank = new Button { Location = new Point(170, 540), Size = new Size(180, 30), Text = "Ранг книг за кількістю", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnRank.Click += (s, e) => ShowAnalytics(_apiGateway.Analytics.GetBookRankings());

            btnAuthors = new Button { Location = new Point(360, 540), Size = new Size(150, 30), Text = "Книги по авторам", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnAuthors.Click += (s, e) => ShowAnalytics(_apiGateway.Analytics.GetBooksByAuthors());

            btnFines = new Button { Location = new Point(520, 540), Size = new Size(150, 30), Text = "Підсумок штрафів", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnFines.Click += (s, e) => ShowAnalytics(_apiGateway.Analytics.GetFinesSummary());

            btnChartGroup = new Button
            {
                Location = new Point(12, 575),
                Size = new Size(150, 30),
                Text = "📊 Діаграма жанрів",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.LightSkyBlue
            };
            btnChartGroup.Click += (s, e) => {
                DataTable data = _apiGateway.Analytics.GetBooksGroupedByGenre();
                new ChartForm(data, "Розподіл книг за жанрами", "Genre", "Count").Show();
            };

            Controls.AddRange(new Control[] { btnSave, btnDelete, btnGroup, btnRank, btnAuthors, btnFines, btnChartGroup });
        }

        private void ShowAnalytics(DataTable data)
        {
            comboBoxTables.SelectedIndex = -1;
            dataGridView.DataSource = data;
        }
    }
}