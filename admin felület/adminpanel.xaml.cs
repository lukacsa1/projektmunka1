using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YourNamespace; // Az UserSession és Termek osztály helye
using admin_felület.classok;

namespace admin_felület
{
    public partial class adminpanel : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private List<Termek> _termekek;
        private Termek _selectedTermek;
        public int kivalasztva=0;
        private ProductService _productService; // Az új szolgáltatás példánya

        public adminpanel()
        {
            InitializeComponent();
            _productService = new ProductService(); // Szolgáltatás példányosítása
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
            TermekekMegjelenitese.Visibility = Visibility.Visible;
            LoadProducts();
        }

        private void AddProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Termék hozzáadás panel megjelenítése, és a másik panel elrejtése
            TermekHozzaadasPanel.Visibility = Visibility.Visible;
            TermekekMegjelenitese.Visibility = Visibility.Collapsed;

            // Törlés és szerkesztés gombok elrejtése
            HideEditAndDeleteButtons();
        }

        // Törlés és szerkesztés gombok elrejtése
        private void HideEditAndDeleteButtons()
        {
            EditProductButton.Visibility = Visibility.Collapsed;
            DeleteProductButton.Visibility = Visibility.Collapsed;
        }

        // Kiválasztott termék kezelése
        private void TermekekDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTermek = TermekekDataGrid.SelectedItem as Termek;

            // Ha nincs kiválasztott termék, akkor elrejtjük a szerkesztés és törlés gombokat
            if (_selectedTermek != null)
            {
                EditProductButton.Visibility = Visibility.Visible;
                DeleteProductButton.Visibility = Visibility.Visible;
            }
            else
            {
                HideEditAndDeleteButtons();
            }
        }

        // Termék hozzáadása
        private async void HozzaadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(ArTextBox.Text, out int ar))
                {
                    MessageBox.Show("Kérem, adjon meg érvényes számot az ár mezőbe!");
                    return;
                }

                var newProduct = new
                {
                    TermekNeve = TermekNeveTextBox.Text,
                    Meret = MeretTextBox.Text,
                    Ar = ar,
                    Kep = KepTextBox.Text,
                    Kategoria = KategoriaTextBox.Text
                };

                string jsonData = JsonSerializer.Serialize(newProduct);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                string requestUrl = $"https://localhost:7117/api/Products/CreateNewProduct?token={token}";

                HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sikeres hozzáadás!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProducts();
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

        // Szerkesztés
        // Szerkesztés (a "Mentés" gomb láthatóvá tétele)
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTermek == null)
            {
                MessageBox.Show("Kérjük, válasszon ki egy terméket a szerkesztéshez!");
                return;
            }

            TermekHozzaadasPanel.Visibility = Visibility.Visible;
            TermekekDataGrid.Visibility = Visibility.Collapsed;

            // A kiválasztott termék adatainak kitöltése
            TermekNeveTextBox.Text = _selectedTermek.TermekNeve;
            MeretTextBox.Text = _selectedTermek.Meret;
            ArTextBox.Text = _selectedTermek.Ar.ToString();
            KepTextBox.Text = _selectedTermek.Kep;
            KategoriaTextBox.Text = _selectedTermek.Kategoria;

            // A mentés gombot láthatóvá tesszük, a hozzáadás gombot elrejtjük
            HozzaadButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
            EditProductButton.Visibility = Visibility.Collapsed;
            DeleteProductButton.Visibility = Visibility.Collapsed;
        }

        // Mentés gomb eseménykezelője
        private async void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedTermek == null)
                {
                    MessageBox.Show("Kérjük, válasszon ki egy terméket a szerkesztéshez!");
                    return;
                }

                // Az ár validálása
                if (!int.TryParse(ArTextBox.Text, out int ar))
                {
                    MessageBox.Show("Kérem, adjon meg érvényes számot az ár mezőbe!");
                    return;
                }

                // A frissített termék adatainak előkészítése
                var updatedProduct = new
                {
                    id = _selectedTermek.Id, // A termék id-je
                    termekNeve = TermekNeveTextBox.Text,
                    meret = MeretTextBox.Text,
                    ar = ar,
                    kep = KepTextBox.Text,
                    kategoria = KategoriaTextBox.Text
                };

                // A JSON adat formázása
                string jsonData = JsonSerializer.Serialize(updatedProduct);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Token megszerzése a session-ből
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // API végpont URL-je
                string requestUrl = $"https://localhost:7117/api/Products/UpdateProduct?token={token}"; // Token paraméterként

                // A PUT kérés elküldése
                HttpResponseMessage response = await _httpClient.PutAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sikeres módosítás!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProducts(); // A termékek újratöltése
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba történt: {response.StatusCode}\n{errorMsg}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // A nézet visszaállítása a termékek listájára
                TermekekDataGrid.Visibility = Visibility.Visible;
                TermekHozzaadasPanel.Visibility = Visibility.Collapsed;

                // A gombok láthatóságának kezelése
                HozzaadButton.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Törlés
        private async void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTermek == null)
            {
                MessageBox.Show("Kérjük, válasszon ki egy terméket a törléshez!");
                return;
            }
            string token=UserSession.Token;
            await _productService.DeleteProduct(_selectedTermek.Id);
            LoadProducts();
        }

        // Termék törlése
       
        private void LoadUsersMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void AddUsersMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            kivalasztva = 1;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            kivalasztva = 0;
        }
    }
}
