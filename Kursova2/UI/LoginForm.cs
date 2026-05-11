using System;
using System.Drawing;
using System.Windows.Forms;
using Kursova2.Services;

namespace Kursova2.UI
{
    public partial class LoginForm : Form
    {
        private readonly IApiGateway _apiGateway; // Заменено на интерфейс
        public bool IsAdmin { get; private set; }
        private TextBox textBoxPassword;
        private Button buttonLogin, buttonUserLogin;

        public LoginForm(IApiGateway apiGateway) // Заменено на интерфейс
        {
            _apiGateway = apiGateway;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Авторизація";
            this.Size = new Size(250, 150);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lbl = new Label { Location = new Point(10, 10), Text = "Пароль адміна:", AutoSize = true };
            textBoxPassword = new TextBox { Location = new Point(10, 30), Width = 210, PasswordChar = '*' };

            buttonLogin = new Button { Location = new Point(10, 60), Width = 100, Text = "Адмін" };
            buttonLogin.Click += (s, e) => {
                if (textBoxPassword.Text == "admin") { IsAdmin = true; DialogResult = DialogResult.OK; }
                else MessageBox.Show("Невірний пароль!");
            };

            buttonUserLogin = new Button { Location = new Point(120, 60), Width = 100, Text = "Читач" };
            buttonUserLogin.Click += (s, e) => { IsAdmin = false; DialogResult = DialogResult.OK; };

            Controls.AddRange(new Control[] { lbl, textBoxPassword, buttonLogin, buttonUserLogin });
        }
    }
}