using BlazorApp.Helpers;
using BlazorApp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp.Services
{
    public interface IHttpService
    {
        Task<T> Get<T>(string uri);
        Task<T> Post<T>(string uri, object value);
        Task Login(string username, string password);
    }

    public class HttpService : IHttpService
    {
        private HttpClient _httpClient;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private IConfiguration _configuration;

        public HttpService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService,
            IConfiguration configuration
        ) {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
            _configuration = configuration;
        }
        public async Task Login(string username, string password)
        {
            try
            {
                /*
                Uri uri = new Uri(_httpClient.BaseAddress + "login");
                LoginModel userlogin = new LoginModel() { UserName = username, Password = password };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(userlogin);
                StringContent stringContent = new StringContent(json, Encoding.UTF8, StaticValues.APP_JSON);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(StaticValues.APP_JSON));
                _httpClient.DefaultRequestHeaders.Add(StaticValues.APP_CORS, "*");
                HttpResponseMessage response = await _httpClient.PostAsync(uri, stringContent);
                */

                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(_httpClient.BaseAddress + "login"),
                    Content =
                    JsonContent.Create(new LoginModel
                    {
                        UserName = username,
                        Password = password
                    })
                };
                requestMessage.Headers.Add(StaticValues.APP_CORS, "*");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                bool success = response.IsSuccessStatusCode;
                if (!success) return;

                string bearer = await response.Content.ReadAsStringAsync();
                await _localStorageService.SetItem(StaticValues.BEARER, bearer);
                Console.WriteLine(bearer);
                /*
                requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(_httpClient.BaseAddress + "api/systemusers/" + username)
                };
                requestMessage.Headers.Add(StaticValues.APP_CORS, "*");
                requestMessage.Headers.Add(StaticValues.Authorization, StaticValues.BEARER + " " + bearer);
                //requestMessage.Headers.Add(StaticValues.COOKIE, cookie); // Blazor WASM do not allowed cookie
                response = await _httpClient.SendAsync(requestMessage);
                if (!success) return;

                string content = await response.Content.ReadAsStringAsync();
                UserLogin usr = Newtonsoft.Json.JsonConvert.DeserializeObject<UserLogin>(content);
                usr.Bearer = bearer;
                */
                UserLogin usr = await Get<UserLogin>("api/systemusers/" + username);
                await _localStorageService.SetItem("user", usr);
                Console.WriteLine(usr.UserName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<T> Get<T>(string uri)
        {
            string bearer = await _localStorageService.GetItem<string>(StaticValues.BEARER);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add(StaticValues.APP_CORS, "*");
            request.Headers.Add(StaticValues.Authorization, StaticValues.BEARER + " " + bearer);
            return await sendRequest<T>(request);
        }

        public async Task<T> Post<T>(string uri, object value)
        {
            string bearer = await _localStorageService.GetItem<string>(StaticValues.BEARER);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add(StaticValues.APP_CORS, "*");
            request.Headers.Add(StaticValues.Authorization, StaticValues.BEARER + " " + bearer);
            request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            return await sendRequest<T>(request);
        }

        // helper methods

        private async Task<T> sendRequest<T>(HttpRequestMessage request)
        {
            // add basic auth header if user is logged in and request is to the api url
            var user = await _localStorageService.GetItem<User>("user");
            var isApiUrl = !request.RequestUri.IsAbsoluteUri;
            if (user != null && isApiUrl)
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", user.AuthData);

            using var response = await _httpClient.SendAsync(request);

            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("logout");
                return default;
            }

            // throw exception on error response
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new Exception(error["message"]);
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}