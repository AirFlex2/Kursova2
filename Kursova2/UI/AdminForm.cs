using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Kursova2.Network;

namespace Kursova2.UI
{
    public class AdminForm : BaseLibraryForm
    {
        private Button btnSave, btnDelete;
        private Button btnGroup, btnRank, btnAuthors, btnFines;
        private Button btnChartGroup;
        private Button btnAmnesty, btnNoAmnesty, btnReturnBook;

        public AdminForm(ApiClient apiClient) : base(apiClient)
        {
            this.Text = "Бібліотека - Панель Адміністратора";
            dataGridView.ReadOnly = false;
            InitializeAdminComponents();

            this.Load += async (s, e) => await LoadTablesAsync(isAdmin: true);
        }

        private void InitializeAdminComponents()
        {
            btnSave = new Button { Location = new Point(550, 10), Size = new Size(100, 28), Text = "Зберегти" };
            btnSave.Click += async (s, e) => {
                if (_currentTable != null)
                {
                    try
                    {
                        // ЯВНО УКАЗЫВАЕМ ИМЯ ТАБЛИЦЫ
                        _currentTable.TableName = _currentTableName;

                        await _apiClient.SaveChangesAsync(_currentTableName, _currentTable.GetChanges());
                        _currentTable.AcceptChanges();
                        MessageBox.Show("Успішно збережено!");
                    }
                    catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
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

            btnGroup = new Button { Location = new Point(12, 540), Size = new Size(150, 30), Text = "Групування книг", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnGroup.Click += async (s, e) => ShowAnalytics(await _apiClient.GetBooksGroupedByGenreAsync());

            btnRank = new Button { Location = new Point(170, 540), Size = new Size(180, 30), Text = "Ранг книг за кількістю", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnRank.Click += async (s, e) => ShowAnalytics(await _apiClient.GetBookRankingsAsync());

            btnAuthors = new Button { Location = new Point(360, 540), Size = new Size(150, 30), Text = "Книги по авторам", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnAuthors.Click += async (s, e) => ShowAnalytics(await _apiClient.GetBooksByAuthorsAsync());

            btnFines = new Button { Location = new Point(520, 540), Size = new Size(150, 30), Text = "Підсумок штрафів", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnFines.Click += async (s, e) => ShowAnalytics(await _apiClient.GetFinesSummaryAsync());

            btnChartGroup = new Button
            {
                Location = new Point(12, 575),
                Size = new Size(150, 30),
                Text = "📊 Діаграма жанрів",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.LightSkyBlue
            };
            btnChartGroup.Click += async (s, e) => {
                try
                {
                    DataTable data = await _apiClient.GetBooksGroupedByGenreAsync();
                    new ChartForm(data, "Розподіл книг за жанрами", "Genre", "Count").Show();
                }
                catch (Exception ex) { MessageBox.Show("Помилка діаграми: " + ex.Message); }
            };

            btnAmnesty = new Button
            {
                Location = new Point(170, 575),
                Size = new Size(160, 30),
                Text = "🎁 Увімкнути Амністію",
                BackColor = Color.LightYellow,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAmnesty.Click += async (s, e) => {
                await _apiClient.SetAmnestyAsync(true);
                MessageBox.Show("Режим амністії активовано на сервері!");
            };

            btnNoAmnesty = new Button
            {
                Location = new Point(340, 575),
                Size = new Size(160, 30),
                Text = "❌ Вимкнути Амністію",
                BackColor = Color.LightCoral,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnNoAmnesty.Click += async (s, e) => {
                await _apiClient.SetAmnestyAsync(false);
                MessageBox.Show("Амністію вимкнено на сервері.");
            };

            btnReturnBook = new Button
            {
                Location = new Point(510, 575),
                Size = new Size(150, 30),
                Text = "🔙 Повернути книгу",
                BackColor = Color.LightBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnReturnBook.Click += async (s, e) => {
                if (_currentTableName != "Loan" || dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Оберіть запис у таблиці 'Loan' (Позики) для повернення.");
                    return;
                }
                try
                {
                    int loanId = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["LoanID"].Value);
                    string result = await _apiClient.ReturnBookAsync(loanId);
                    MessageBox.Show(result);
                    await LoadDataAsync("Loan");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
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