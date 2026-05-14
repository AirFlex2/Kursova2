using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Kursova2.Services;
using Kursova2.Services.Strategies; // Додано для роботи з патерном Strategy

namespace Kursova2.UI
{
    public class AdminForm : BaseLibraryForm
    {
        private Button btnSave, btnDelete;
        private Button btnGroup, btnRank, btnAuthors, btnFines;
        private Button btnChartGroup;

        // Нові кнопки для логіки штрафів та повернення
        private Button btnAmnesty, btnNoAmnesty, btnReturnBook;

        public AdminForm(IApiGateway apiGateway) : base(apiGateway)
        {
            this.Text = "Бібліотека - Панель Адміністратора";
            dataGridView.ReadOnly = false;
            InitializeAdminComponents();
            LoadTables(isAdmin: true);
        }

        private void InitializeAdminComponents()
        {
            // Стандартні кнопки керування даними
            btnSave = new Button { Location = new Point(550, 10), Size = new Size(100, 28), Text = "Зберегти" };
            btnSave.Click += (s, e) => {
                if (_currentTable != null)
                {
                    _apiGateway.Data.SaveChanges(_currentTableName, _currentTable.GetChanges());
                    _currentTable.AcceptChanges();
                    MessageBox.Show("Зміни збережено!");
                }
            };

            btnDelete = new Button { Location = new Point(660, 10), Size = new Size(100, 28), Text = "Видалити" };
            btnDelete.Click += (s, e) => {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                        if (!row.IsNewRow) dataGridView.Rows.Remove(row);
                }
            };

            // Кнопки аналітики
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

            // НОВІ КНОПКИ (Логіка штрафів)

            // 1. Увімкнення амністії (Strategy)
            btnAmnesty = new Button
            {
                Location = new Point(170, 575),
                Size = new Size(160, 30),
                Text = "🎁 Увімкнути Амністію",
                BackColor = Color.LightYellow,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAmnesty.Click += (s, e) => {
                _apiGateway.Loan.SetFineStrategy(new AmnestyFineStrategy());
                MessageBox.Show("Режим амністії активовано! Штрафи тепер будуть 0 грн.");
            };

            // 2. Вимкнення амністії (Strategy)
            btnNoAmnesty = new Button
            {
                Location = new Point(340, 575),
                Size = new Size(160, 30),
                Text = "❌ Вимкнути Амністію",
                BackColor = Color.LightCoral,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnNoAmnesty.Click += (s, e) => {
                _apiGateway.Loan.SetFineStrategy(new StandardFineStrategy());
                MessageBox.Show("Амністію вимкнено. Повернення до стандартних штрафів.");
            };

            // 3. Повернення книги (Microservice Logic)
            btnReturnBook = new Button
            {
                Location = new Point(510, 575),
                Size = new Size(150, 30),
                Text = "🔙 Повернути книгу",
                BackColor = Color.LightBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnReturnBook.Click += (s, e) => {
                if (_currentTableName != "Loan" || dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Оберіть запис у таблиці 'Loan' (Позики) для повернення.");
                    return;
                }
                int loanId = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["LoanID"].Value);
                string result = _apiGateway.Loan.ReturnBook(loanId);
                MessageBox.Show(result);
                LoadData("Loan"); // Оновлюємо список позик
            };

            Controls.AddRange(new Control[] {
                btnSave, btnDelete, btnGroup, btnRank, btnAuthors, btnFines,
                btnChartGroup, btnAmnesty, btnNoAmnesty, btnReturnBook
            });
        }

        private void ShowAnalytics(DataTable data)
        {
            comboBoxTables.SelectedIndex = -1;
            dataGridView.DataSource = data;
        }
    }
}