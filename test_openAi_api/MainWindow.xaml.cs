using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace test_openAi_api
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IConfiguration Configuration;
        public MainWindow()
        {
            InitializeComponent();
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        private async void apiAction()
        {
            string inputText = txt_inputbox.Text;
            string passkey = Configuration["ApiSettings:Passkey"];
            string endpoint = "https://api.openai.com/v1/chat/completions";
            
            if(null == passkey)
            {
                Console.WriteLine("Check passkey");
                throw new Exception("stop");
            }

            try
            {
                string result = await GetApiDataAsync(endpoint, inputText, passkey);
                rtx_output.Document.Blocks.Clear();
                rtx_output.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(result)));
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"HTTP Request error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async Task<string> GetApiDataAsync(string endpoint, string inputText, string passkey)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", passkey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo", // Specify a valid model
                messages = new[]
                {
                    new { role = "user", content = inputText }
                }
            };
            string jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            HttpContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase})");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody);

            return result.choices[0].message.content;
        }

        private void btn_submit_Click(object sender, RoutedEventArgs e)
        {
            apiAction();
        }


    }
}

