using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YourNamespace;

namespace admin_felület
{
    public partial class adminpanel : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private List<Termek> _termekek;

        public adminpanel()
        {
            InitializeComponent();
        }

        // Termékek betöltése az API-ból
        private async Task LoadProducts()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7117/api/Products/GetProducts");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba a termékek lekérdezésekor: {response.StatusCode}\n{errorMsg}");
                    return;
                }

                string jsonData = await response.Content.ReadAsStringAsync();
                _termekek = JsonSerializer.Deserialize<List<Termek>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                TermekekDataGrid.ItemsSource = _termekek ?? new List<Termek>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }
        }

        // Menü kattintások
        private void LoadProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TermekHozzaadasPanel.Visibility = Visibility.Collapsed;
            TermekekDataGrid.Visibility = Visibility.Visible;
            LoadProducts();
        }
        private void DeleteProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
        private void EditProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void AddProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Megjelenítjük az űrlapot, és elrejtjük a terméklistát
            TermekHozzaadasPanel.Visibility = Visibility.Visible;
            TermekekDataGrid.Visibility = Visibility.Collapsed;
        }

        private async void HozzaadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Token beolvasása valahonnan (pl. bejelentkezés után elmentett UserSession.Token)
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newProduct = new
                {
                    TermekNeve = TermekNeveTextBox.Text,
                    Meret = MeretTextBox.Text,
                    Ar = int.Parse(ArTextBox.Text),
                    Kep = KepTextBox.Text,
                    Kategoria = KategoriaTextBox.Text
                };

                string jsonData = JsonSerializer.Serialize(newProduct);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Token hozzáadása query paraméterként
                string requestUrl = $"https://localhost:7117/api/Products/CreateNewProduct?token={token}";

                HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sikeres hozzáadás!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProducts(); // Frissíti a termékeket
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba történt: {response.StatusCode}\n{errorMsg}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

        // Termék osztály
        public class Termek
    {
        public int Id { get; set; }
        public string TermekNeve { get; set; }
        public string Meret { get; set; }
        public int Ar { get; set; }
        public string Kep { get; set; }
        public string Kategoria { get; set; }
    }
}
