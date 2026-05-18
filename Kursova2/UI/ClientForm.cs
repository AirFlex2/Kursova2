using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Kursova2.Services;

namespace Kursova2.UI
{
    public class ClientForm : BaseLibraryForm
    {
        private TextBox txtUserName;
        private Button btnRegister, btnReserve, btnMyFines, btnMyProfile;
        private Button btnMyReservations, btnCancelReservation;
        private Button btnLibraryChart;

        public ClientForm(IApiGateway apiGateway) : base(apiGateway)
        {
            this.Text = "Бібліотека - Режим Читача";
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;

            InitializeClientComponents();
            LoadTables(isAdmin: false);
        }

        private void InitializeClientComponents()
        {
            Label lblName = new Label { Text = "Ваше ПІБ:", Location = new Point(12, 545), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            txtUserName = new TextBox { Location = new Point(80, 542), Width = 150, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            btnMyProfile = new Button { Location = new Point(240, 540), Size = new Size(100, 28), Text = "Мій профіль", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnMyProfile.Click += (s, e) => ShowUserData(GetUserName(), "Profile");

            btnMyFines = new Button { Location = new Point(350, 540), Size = new Size(100, 28), Text = "Мої штрафи", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnMyFines.Click += (s, e) => ShowUserData(GetUserName(), "Fines");

            btnMyReservations = new Button { Location = new Point(460, 540), Size = new Size(130, 28), Text = "Мої бронювання", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnMyReservations.Click += (s, e) => ShowUserData(GetUserName(), "Reservations");

            btnLibraryChart = new Button
            {
                Location = new Point(600, 540),
                Size = new Size(140, 28),
                Text = "📊 Статистика фонду",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.LightSkyBlue
            };
            btnLibraryChart.Click += (s, e) => {
                DataTable data = _apiGateway.Analytics.GetBooksGroupedByGenre();
                new ChartForm(data, "Фонд бібліотеки за жанрами", "Genre", "Count").Show();
            };

            btnReserve = new Button { Location = new Point(12, 580), Size = new Size(150, 28), Text = "Забронювати книгу", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnReserve.Click += BtnReserve_Click;

            btnCancelReservation = new Button { Location = new Point(170, 580), Size = new Size(160, 28), Text = "Скасувати бронювання", Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnCancelReservation.Click += BtnCancelReservation_Click;

            btnRegister = new Button { Location = new Point(800, 580), Size = new Size(160, 28), Text = "Новий читач? Реєстрація", Anchor = AnchorStyles.Bottom | AnchorStyles.Right, BackColor = Color.LightGreen };
            btnRegister.Click += (s, e) => new RegistrationForm(_apiGateway).ShowDialog();

            Controls.AddRange(new Control[] {
                lblName, txtUserName, btnMyProfile, btnMyFines, btnMyReservations, btnLibraryChart,
                btnReserve, btnCancelReservation, btnRegister
            });
        }

        private string GetUserName()
        {
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Будь ласка, введіть своє ПІБ у поле зліва для цієї дії.");
                return null;
            }
            return txtUserName.Text.Trim();
        }

        private void BtnReserve_Click(object sender, EventArgs e)
        {
            string userName = GetUserName();
            if (userName == null) return;

            if (dataGridView.SelectedRows.Count == 0 || !dataGridView.Columns.Contains("BookID"))
            {
                MessageBox.Show("Оберіть книгу з каталогу (таблиця 'Book') для бронювання!");
                return;
            }

            try
            {
                int bookId = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["BookID"].Value);
                string resultMessage = _apiGateway.Reservation.ReserveBook(bookId, userName);
                MessageBox.Show(resultMessage);

                if (comboBoxTables.SelectedItem != null) LoadData(comboBoxTables.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка бронювання: {ex.Message}");
            }
        }

        private void BtnCancelReservation_Click(object sender, EventArgs e)
        {
            if (!dataGridView.Columns.Contains("ReservationID"))
            {
                MessageBox.Show("Спочатку натисніть 'Мої бронювання' і виберіть запис зі списку!");
                return;
            }

            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Виберіть бронювання, яке бажаєте скасувати.");
                return;
            }

            try
            {
                int reservationId = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["ReservationID"].Value);
                string result = _apiGateway.Reservation.CancelReservation(reservationId);
                MessageBox.Show(result);

                ShowUserData(GetUserName(), "Reservations");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

        private void ShowUserData(string userName, string mode)
        {
            if (userName == null) return;

            try
            {
                DataTable data = null;
                if (mode == "Profile") data = _apiGateway.Auth.GetReaderInfo(userName);
                else if (mode == "Fines") data = _apiGateway.Analytics.GetUserFines(userName);
                else if (mode == "Reservations") data = _apiGateway.Reservation.GetUserReservations(userName);

                if (data != null && data.Rows.Count > 0)
                {
                    comboBoxTables.SelectedIndex = -1;
                    dataGridView.DataSource = data;
                }
                else
                {
                    MessageBox.Show("Даних не знайдено.");
                    dataGridView.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }
    }
}