using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using YourNamespace;

namespace admin_felület.classok
{
    internal class OrderService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public async Task DeleteOrder(int rendelesId)
        {
            try
            {
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string requestUrl = $"https://localhost:7117/api/Order/Admin/DeleteOrder?token={token}&id={rendelesId}";

                HttpResponseMessage response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Rendelés sikeresen törölve!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
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
