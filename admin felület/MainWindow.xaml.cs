using admin_felület;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace YourNamespace
{
    public static class UserSession
    {
        public static string Token { get; set; }
    }

    public partial class LoginWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Felhasználónév és jelszó megadása kötelező!";
                return;
            }

            try
            {
                // Só lekérése az API-ból
                HttpResponseMessage saltResponse = await _httpClient.PostAsync($"https://localhost:7117/api/Login/GetSalt/{username}", null);
                string salt = await saltResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(salt) || salt == "null")
                    throw new Exception("A só nem érvényes vagy üres.");

                // Jelszó hash-elése
                string tmpHash = ComputeSha256Hash(password + salt);

                // Bejelentkezési adatok küldése
                var loginData = new { loginName = username, tmpHash };
                string jsonData = JsonSerializer.Serialize(loginData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage loginResponse = await _httpClient.PostAsync("https://localhost:7117/api/Login", content);
                string loginResponseText = await loginResponse.Content.ReadAsStringAsync();

                if (!loginResponse.IsSuccessStatusCode)
                    throw new Exception($"Hibás bejelentkezési adatok! {loginResponse.StatusCode}: {loginResponseText}");

                var responseData = JsonSerializer.Deserialize<LoginResponse>(loginResponseText);


                // MessageBox.Show("Sikeres bejelentkezés!", "Bejelentkezés", MessageBoxButton.OK, MessageBoxImage.Information);
                //this.Close();

                // Ellenőrizd a jogosultsági szintet
                if (responseData.permissionLevel <= 8)
                {
                    throw new Exception("Nincs megfelelő jogosultság a bejelentkezéshez!");
                }

                // Token mentése UserSession osztályba
                UserSession.Token = responseData.token;

                // Bejelentkezés sikeres -> adminpanel megnyitása
                MessageBox.Show(UserSession.Token);
                adminpanel adminpanel = new adminpanel();
                adminpanel.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte t in bytes)
                    builder.Append(t.ToString("x2"));
                return builder.ToString();
            }
        }

        private class LoginResponse
        {
            public string token { get; set; } = "";
            public int permissionLevel { get; set; }
        }
    }
}