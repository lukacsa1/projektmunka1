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
using System.ComponentModel;
using System.Windows.Input;

namespace admin_felület
{
    public partial class adminpanel : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        //Termékek
        private List<Termek> _termekek;
        private Termek _selectedTermek;
        private ProductService _productService; // Az új szolgáltatás példánya

        //Felhasználók
        private List<Felhasznalo> _felhasznalok;
        private Felhasznalo _selectedFelhasznalo;
        private UserService _userService; // Az új szolgáltatás példánya
        public adminpanel()
        {
            InitializeComponent();
            _productService = new ProductService(); // Szolgáltatás példányosítása
            _userService = new UserService();
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
            Hide();
            TermekekMegjelenitese.Visibility = Visibility.Visible;
            LoadProducts();
        }

        private void AddProductsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            TermekHozzaadasPanel.Visibility = Visibility.Visible;
            HozzaadButton.Visibility = Visibility.Visible;
        }

        // Törlés és szerkesztés gombok elrejtése
        private void Hide()
        {
            FelhasznaloHozzaadasPanel.Visibility = Visibility.Collapsed;
            FelhasznalokMegjelenitese.Visibility = Visibility.Collapsed;
            TermekHozzaadasPanel.Visibility = Visibility.Collapsed;
            TermekekMegjelenitese.Visibility = Visibility.Collapsed;
            EditProductButton.Visibility = Visibility.Collapsed;
            DeleteProductButton.Visibility = Visibility.Collapsed;
            HozzaadButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;
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
                Hide();
                TermekekMegjelenitese.Visibility = Visibility.Visible;
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
            TermekNeveTextBox.Text = "";
            MeretTextBox.Text = "";
            ArTextBox.Text= "";
            KepTextBox.Text = "";
            KategoriaTextBox.Text = "";
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
            Hide();
            TermekHozzaadasPanel.Visibility = Visibility.Visible;

            // A kiválasztott termék adatainak kitöltése
            TermekNeveTextBox.Text = _selectedTermek.TermekNeve;
            MeretTextBox.Text = _selectedTermek.Meret;
            ArTextBox.Text = _selectedTermek.Ar.ToString();
            KepTextBox.Text = _selectedTermek.Kep;
            KategoriaTextBox.Text = _selectedTermek.Kategoria;

            // A mentés gombot láthatóvá tesszük
            SaveButton.Visibility = Visibility.Visible;
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
                Hide();
               
                TermekNeveTextBox.Text = "";
                MeretTextBox.Text = "";
                ArTextBox.Text = "";
                KepTextBox.Text = "";
                KategoriaTextBox.Text = "";

                TermekekMegjelenitese.Visibility = Visibility.Visible;
                HozzaadButton.Visibility = Visibility.Visible;
                TermekekDataGrid.Visibility = Visibility.Visible;
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

        // Felhasználók
        private async Task LoadUsers()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:7117/api/User/Admin/GetAll?token={UserSession.Token}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba a termékek lekérdezésekor: {response.StatusCode}\n{errorMsg}");
                    return;
                }

                string jsonData = await response.Content.ReadAsStringAsync();
                _felhasznalok = JsonSerializer.Deserialize<List<Felhasznalo>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                FelhasznalokDataGrid.ItemsSource = _felhasznalok ?? new List<Felhasznalo>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }
        }
        private void FelhasznalokDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void HozzaadUserButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SaveUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadUsersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FelhasznaloHozzaadasPanel.Visibility = Visibility.Collapsed;
            FelhasznalokMegjelenitese.Visibility = Visibility.Visible;
            TermekHozzaadasPanel.Visibility = Visibility.Collapsed;
            TermekekMegjelenitese.Visibility = Visibility.Collapsed;
            LoadUsers();
        }
        private void AddUsersMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
