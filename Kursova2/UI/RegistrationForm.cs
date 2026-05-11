using System;
using System.Drawing;
using System.Windows.Forms;
using Kursova2.Services;

namespace Kursova2.UI
{
    public class RegistrationForm : Form
    {
        private readonly IApiGateway _apiGateway; // Заменено на интерфейс
        private TextBox txtName, txtEmail, txtAddress, txtHomePhone, txtWorkPhone;
        private Button btnRegister;

        public RegistrationForm(IApiGateway apiGateway) // Заменено на интерфейс
        {
            _apiGateway = apiGateway;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Реєстрація нового читача";
            this.Size = new Size(300, 380);
            this.StartPosition = FormStartPosition.CenterParent;

            Controls.Add(new Label { Text = "ПІБ Читача:", Location = new Point(10, 10), AutoSize = true });
            txtName = new TextBox { Location = new Point(10, 30), Width = 260 };
            Controls.Add(txtName);

            Controls.Add(new Label { Text = "Email (Контактні дані):", Location = new Point(10, 60), AutoSize = true });
            txtEmail = new TextBox { Location = new Point(10, 80), Width = 260 };
            Controls.Add(txtEmail);

            Controls.Add(new Label { Text = "Домашня адреса:", Location = new Point(10, 110), AutoSize = true });
            txtAddress = new TextBox { Location = new Point(10, 130), Width = 260 };
            Controls.Add(txtAddress);

            Controls.Add(new Label { Text = "Домашній телефон:", Location = new Point(10, 160), AutoSize = true });
            txtHomePhone = new TextBox { Location = new Point(10, 180), Width = 260 };
            Controls.Add(txtHomePhone);

            Controls.Add(new Label { Text = "Робочий телефон:", Location = new Point(10, 210), AutoSize = true });
            txtWorkPhone = new TextBox { Location = new Point(10, 230), Width = 260 };
            Controls.Add(txtWorkPhone);

            Label lblInfo = new Label
            {
                Text = "*Дата реєстрації та статус 'Активний' \nвстановлюються автоматично.",
                Location = new Point(10, 260),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            Controls.Add(lblInfo);

            btnRegister = new Button { Text = "Зареєструватися", Location = new Point(10, 300), Width = 260, Height = 30 };
            btnRegister.Click += BtnRegister_Click;
            Controls.Add(btnRegister);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Поля ПІБ та Email є обов'язковими для заповнення!");
                return;
            }

            try
            {
                if (_apiGateway.Auth.RegisterReader(txtName.Text, txtEmail.Text, txtAddress.Text, txtHomePhone.Text, txtWorkPhone.Text))
                {
                    MessageBox.Show("Вас успішно зареєстровано в системі!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка реєстрації");
            }
        }
    }
}