using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YourNamespace;

namespace admin_felület.classok
{
    internal class UserService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public async Task DeleteUser(int felhasznaloId)
        {
            try
            {
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string requestUrl = $"https://localhost:7117/api/User/Admin/Delete/?token={token}&DeleteUserId={felhasznaloId}";

                HttpResponseMessage response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Felhasználó sikeresen törölve!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba történt: {response.StatusCode}\n{errorMsg}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
