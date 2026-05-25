using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kursova2.Network
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5189/api/library/";

        public ApiClient()
        {
            _httpClient = new HttpClient();
        }

        private async Task<DataTable> GetDataTableAsync(string url)
        {
            var response = await _httpClient.GetAsync(_baseUrl + url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var table = JsonConvert.DeserializeObject<DataTable>(json);

            table?.AcceptChanges();

            return table;
        }

        private async Task<string> PostAsync(string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + url, content);

            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(responseString);

            dynamic result = JsonConvert.DeserializeObject(responseString);
            return result?.Message ?? "";
        }

        // --- Data ---
        public async Task<List<string>> GetAvailableTablesAsync(bool isAdmin)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}tables/{isAdmin}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(json);
        }

        public Task<DataTable> GetTableDataAsync(string tableName) => GetDataTableAsync($"table/{tableName}");

        public Task<DataTable> SearchInTableAsync(string table, string column, string keyword)
            => GetDataTableAsync($"search/{table}/{column}/{keyword}");

        public async Task SaveChangesAsync(string tableName, DataTable changes)
        {
            if (changes == null) return;

            // Используем XML (DiffGram), чтобы сохранить состояния строк (Added, Modified, Deleted)
            using (var sw = new System.IO.StringWriter())
            {
                changes.WriteXml(sw, XmlWriteMode.DiffGram);
                var xmlContent = sw.ToString();
                var content = new StringContent(xmlContent, System.Text.Encoding.UTF8, "application/xml");

                var response = await _httpClient.PostAsync($"{_baseUrl}table/save/{tableName}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception(error);
                }
            }
        }

        // --- Auth ---
        public Task<string> RegisterReaderAsync(string name, string email, string address, string homePhone, string workPhone)
        {
            var payload = new { FullName = name, Email = email, Address = address, HomePhone = homePhone, WorkPhone = workPhone };
            return PostAsync("auth/register", payload);
        }

        public Task<DataTable> GetReaderInfoAsync(string name) => GetDataTableAsync($"auth/reader/{name}");

        // --- Reservation ---
        public Task<string> ReserveBookAsync(int bookId, string readerName)
        {
            var payload = new { BookId = bookId, ReaderName = readerName };
            return PostAsync("reservation/reserve", payload);
        }

        public Task<DataTable> GetUserReservationsAsync(string name) => GetDataTableAsync($"reservation/user/{name}");

        public Task<string> CancelReservationAsync(int id) => PostAsync($"reservation/cancel/{id}", null);

        // --- Loan ---
        public Task<string> ReturnBookAsync(int id) => PostAsync($"loan/return/{id}", null);

        public Task<string> SetAmnestyAsync(bool enable) => PostAsync($"loan/amnesty/{enable}", null);

        // --- Analytics ---
        public Task<DataTable> GetBooksGroupedByGenreAsync() => GetDataTableAsync("analytics/grouped-genre");
        public Task<DataTable> GetBookRankingsAsync() => GetDataTableAsync("analytics/rankings");
        public Task<DataTable> GetBooksByAuthorsAsync() => GetDataTableAsync("analytics/authors");
        public Task<DataTable> GetFinesSummaryAsync() => GetDataTableAsync("analytics/fines");
        public Task<DataTable> GetUserFinesAsync(string name) => GetDataTableAsync($"analytics/user-fines/{name}");
    }
}