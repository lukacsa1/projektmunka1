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
using System.Threading;
using System.Runtime.Remoting.Contexts;

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

        //Rendelések
        private List<Rendeles> _rendelesek;
        private Rendeles _selectedRendeles;
        private OrderService _orderService;
        public bool rendeltTermekek = false;
        public adminpanel()
        {
            InitializeComponent();
            _productService = new ProductService(); // Szolgáltatás példányosítása
            _userService = new UserService();
            _orderService = new OrderService();
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
            TermekNeveTextBox.Text = "";
            MeretTextBox.Text = "";
            ArTextBox.Text = "";
            KepTextBox.Text = "";
            KategoriaTextBox.Text = "";
            TermekHozzaadasPanel.Visibility = Visibility.Visible;
            HozzaadButton.Visibility = Visibility.Visible;
            
        }

        // Minden elrejtése
        private void Hide()
        {
            SaveUserButton.Visibility = Visibility.Collapsed;
            DeleteUserButton.Visibility = Visibility.Collapsed;
            EditUserButton.Visibility = Visibility.Collapsed;
            FelhasznaloHozzaadasPanel.Visibility = Visibility.Collapsed;
            FelhasznalokMegjelenitese.Visibility = Visibility.Collapsed;

            TermekHozzaadasPanel.Visibility = Visibility.Collapsed;
            TermekekMegjelenitese.Visibility = Visibility.Collapsed;
            EditProductButton.Visibility = Visibility.Collapsed;
            DeleteProductButton.Visibility = Visibility.Collapsed;
            HozzaadButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;

            RendelesHozzaadasPanel.Visibility = Visibility.Collapsed;
            RendelesekMegjelenitese.Visibility = Visibility.Collapsed;
            EditOrderButton.Visibility = Visibility.Collapsed;
            DeleteOrderButton.Visibility = Visibility.Collapsed;
            SaveOrderButton.Visibility = Visibility.Collapsed;
            ShowOrderItemsButton.Visibility = Visibility.Collapsed;
            VisszaButton.Visibility = Visibility.Collapsed;
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

                if (TermekNeveTextBox.Text == "" || MeretTextBox.Text == "" || ArTextBox.Text == "" || KepTextBox.Text == "" || KategoriaTextBox.Text == "")
                {
                    MessageBox.Show("Minden mezőt töltsön ki!");
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
                    MessageBox.Show($"Hiba a felhasználók lekérdezésekor: {response.StatusCode}\n{errorMsg}");
                    return;
                }

                string jsonData = await response.Content.ReadAsStringAsync();
                _felhasznalok = JsonSerializer.Deserialize<List<Felhasznalo>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                FelhasznalokDataGrid.ItemsSource = _felhasznalok ?? new List<Felhasznalo>();

                // Itt regisztráljuk az oszlopok generálása közbeni eseményt
                FelhasznalokDataGrid.AutoGeneratingColumn += FelhasznalokDataGrid_AutoGeneratingColumn;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }
        }

        private void FelhasznalokDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Ne jelenjen meg a Hash és a Salt oszlop
            if (e.PropertyName == "Hash" || e.PropertyName == "Salt")
            {
                e.Cancel = true;  // Az oszlop generálása megszakad
            }
        }

        // Felhasználó kiválasztás
        private void FelhasznalokDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _selectedFelhasznalo = FelhasznalokDataGrid.SelectedItem as Felhasznalo;

            // Ha nincs kiválasztott felhasználó, akkor elrejtjük a szerkesztés és törlés gombokat
            if (_selectedFelhasznalo != null)
            {
                EditUserButton.Visibility = Visibility.Visible;
                DeleteUserButton.Visibility = Visibility.Visible;
            }
            else
            {
                Hide();
                FelhasznalokMegjelenitese.Visibility = Visibility.Visible;
            }
        }

        // Felhasználó módosítása
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFelhasznalo == null)
            {
                MessageBox.Show("Kérjük, válasszon ki egy terméket a szerkesztéshez!");
                return;
            }
            Hide();
            FelhasznaloHozzaadasPanel.Visibility = Visibility.Visible;

            // A kiválasztott felhasználó adatainak kitöltése
            LastNameTextBox.Text = _selectedFelhasznalo.LastName;
            FirstNameTextBox.Text = _selectedFelhasznalo.FirstName;
            PhoneNumberTextBox.Text = _selectedFelhasznalo.PhoneNumber.ToString();
            LoginNameTextBox.Text = _selectedFelhasznalo.LoginName;
            EmailTextBox.Text = _selectedFelhasznalo.Email;
            ActiveTextBox.Text = _selectedFelhasznalo.Active.ToString();
            PermissionLevelTextBox.Text = _selectedFelhasznalo.PermissionLevel.ToString();

            // A mentés gombot láthatóvá tesszük
            SaveUserButton.Visibility = Visibility.Visible;
        }

        // Felhasználó törlése
        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFelhasznalo == null)
            {
                MessageBox.Show("Kérjük, válasszon ki egy felhasználót a törléshez!");
                return;
            }
            string token = UserSession.Token;
            await _userService.DeleteUser(_selectedFelhasznalo.Id);
            
            LoadUsers();
        }
        private async void SaveUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedFelhasznalo == null)
                {
                    MessageBox.Show("Kérjük, válasszon ki egy felhasználót a szerkesztéshez!");
                    return;
                }

                // Frissítsük a _selectedFelhasznalo objektumot a TextBox-értékek alapján
                _selectedFelhasznalo.LastName = LastNameTextBox.Text;
                _selectedFelhasznalo.FirstName = FirstNameTextBox.Text;
                _selectedFelhasznalo.PhoneNumber = PhoneNumberTextBox.Text;
                _selectedFelhasznalo.LoginName = LoginNameTextBox.Text;
                _selectedFelhasznalo.Email = EmailTextBox.Text;
                _selectedFelhasznalo.Active = int.TryParse(ActiveTextBox.Text, out int activeStatus) ? (int?)activeStatus : null;
                _selectedFelhasznalo.PermissionLevel = int.TryParse(PermissionLevelTextBox.Text, out int permission) ? permission : _selectedFelhasznalo.PermissionLevel;

                // JSON-adatok serializálása
                string jsonData = JsonSerializer.Serialize(_selectedFelhasznalo);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Token megszerzése a session-ből és Authorization fejléc beállítása
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // API végpont és token hozzáadása az Authorization fejlécbe
                string id = Convert.ToString(_selectedFelhasznalo.Id);  // Id lekérdezése
                string requestUrl = $"https://localhost:7117/api/User/Admin/UpdateUser?token={token}&userId={id}";  // String interpolációval helyes URL
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // PUT kérés elküldése
                HttpResponseMessage response = await _httpClient.PutAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sikeres módosítás!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUsers();  // Felhasználók újratöltése
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba történt: {response.StatusCode}\n{errorMsg}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Nézet visszaállítása
                Hide();

                // TextBox-ok kiürítése
                LastNameTextBox.Text = "";
                FirstNameTextBox.Text = "";
                PhoneNumberTextBox.Text = "";
                LoginNameTextBox.Text = "";
                EmailTextBox.Text = "";
                ActiveTextBox.Text = "";
                PermissionLevelTextBox.Text = "";

                FelhasznalokMegjelenitese.Visibility = Visibility.Visible;
                FelhasznalokDataGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Menüpontok
        private void LoadUsersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Nézet visszaállítása
            Hide();
            // Felhasználók betöltése
            FelhasznalokMegjelenitese.Visibility = Visibility.Visible;
            LoadUsers();
        }

        // Rendelések betöltése az API-ból
        private async Task LoadOrders()
        {
            
            try
            {
                string token = UserSession.Token;
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:7117/api/Order/GetAllOrders?token={token}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba a rendelések lekérdezésekor: {response.StatusCode}\n{errorMsg}");
                    return;
                }

                string jsonData = await response.Content.ReadAsStringAsync();
                _rendelesek = JsonSerializer.Deserialize<List<Rendeles>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                RendelesekDataGrid.ItemsSource = _rendelesek ?? new List<Rendeles>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }
        }

        private async void ShowOrderItemsButton_Click(object sender, RoutedEventArgs e)
        {
            RendelesekDataGrid.ItemsSource = _selectedRendeles.OrderItems ?? new List<OrderItem>();
            ShowOrderItemsButton.Visibility = Visibility.Collapsed;
            VisszaButton.Visibility = Visibility.Visible;
            rendeltTermekek = true;
        }

        private async void VisszaButton_Click(object sender, RoutedEventArgs e)
        {
            RendelesekDataGrid.ItemsSource = _rendelesek ?? new List<Rendeles>();
            VisszaButton.Visibility = Visibility.Collapsed;
            rendeltTermekek = false;
        }

        private async void SaveOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedRendeles == null)
                {
                    MessageBox.Show("Kérjük, válasszon ki egy rendelést a szerkesztéshez!");
                    return;
                }

                // Frissítsük a _selectedRendelés objektumot a TextBox-értékek alapján
                _selectedRendeles.Status = int.TryParse(StatusTextBox.Text, out int status) ? status : _selectedRendeles.Status;

                // JSON-adatok serializálása
                string jsonData = JsonSerializer.Serialize(_selectedRendeles);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Token megszerzése a session-ből és Authorization fejléc beállítása
                string token = UserSession.Token;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Hiba: Token hiányzik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // API végpont és token hozzáadása az Authorization fejlécbe
                string id = Convert.ToString(_selectedRendeles.Id);  // Id lekérdezése
                string requestUrl = $"https://localhost:7117/api/Order/Admin/UpdateOrder?token={token}&id={id}";  // String interpolációval helyes URL
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // PUT kérés elküldése
                HttpResponseMessage response = await _httpClient.PutAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sikeres módosítás!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUsers();  // Felhasználók újratöltése
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Hiba történt: {response.StatusCode}\n{errorMsg}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Nézet visszaállítása
                Hide();

                // TextBox-ok kiürítése
                StatusTextBox.Text = "";

                RendelesekMegjelenitese.Visibility = Visibility.Visible;
                RendelesekDataGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRendeles == null)
            {
                MessageBox.Show("Kérjük, válasszon ki egy rendelést a törléshez!");
                return;
            }
            string token = UserSession.Token;
            await _orderService.DeleteOrder(_selectedRendeles.Id);

            LoadOrders();
        }

        private void EditOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRendeles == null)
            {
                MessageBox.Show("Kérjük, válasszon ki egy rendelést a szerkesztéshez!");
                return;
            }
            Hide();
            RendelesHozzaadasPanel.Visibility = Visibility.Visible;

            // A kiválasztott rendelés adatainak kitöltése
            StatusTextBox.Text = _selectedRendeles.Status.ToString();

            // A mentés gombot láthatóvá tesszük
            SaveOrderButton.Visibility = Visibility.Visible;
        }

        private void RendelesekDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (rendeltTermekek == false)
            {
                _selectedRendeles = RendelesekDataGrid.SelectedItem as Rendeles;

                // Ha nincs kiválasztott termék, akkor elrejtjük a szerkesztés és törlés gombokat
                if (_selectedRendeles != null)
                {
                    EditOrderButton.Visibility = Visibility.Visible;
                    DeleteOrderButton.Visibility = Visibility.Visible;
                    ShowOrderItemsButton.Visibility = Visibility.Visible;
                }
                else
                {
                    Hide();
                    RendelesekMegjelenitese.Visibility = Visibility.Visible;
                }
            }
        }
        private void LoadOrdersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            RendelesekMegjelenitese.Visibility = Visibility.Visible;
            LoadOrders();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

        }
    }
}
