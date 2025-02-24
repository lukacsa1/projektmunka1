using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

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

        private async void LoadProductsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            try
            {
                // Try to get the response from the API
                HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7117/api/Products/GetProducts");

                // Handle unsuccessful status codes
                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    ErrorTextBox.Text = $"Hiba a termékek lekérdezésekor: {response.StatusCode}\n{errorMsg}";
                    return;
                }

                string jsonData = await response.Content.ReadAsStringAsync();

                // Debug message to show the raw JSON response
                ErrorTextBox.Text = $"Válasz JSON: {jsonData}";

                // Deserialize JSON response into the list of products
                _termekek = JsonSerializer.Deserialize<List<Termek>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_termekek != null)
                {
                    // Bind the deserialized products to the DataGrid
                    TermekekDataGrid.ItemsSource = _termekek;
                }
                else
                {
                    ErrorTextBox.Text = "Nincsenek termékek.";
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                ErrorTextBox.Text = $"Hiba történt: {ex.Message}";
            }
        }
    }

    // Modified Termek class for Meret field handling
    public class Termek
    {
        public int Id { get; set; }
        public string TermekNeve { get; set; }
        public string Meret { get; set; } // Store Meret as a JSON string
        public int Ar { get; set; }
        public string Kep { get; set; }
        public string Kategoria { get; set; }
      
    }
}