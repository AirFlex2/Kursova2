using System;
using System.Windows.Forms;
using Kursova2.UI;
using Kursova2.Network;

namespace Kursova2
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Единый клиент для общения с сервером
            ApiClient apiClient = new ApiClient();
            LoginForm loginForm = new LoginForm();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Form mainForm = loginForm.IsAdmin
                    ? (Form)new AdminForm(apiClient)
                    : (Form)new ClientForm(apiClient);

                Application.Run(mainForm);
            }
        }
    }
}