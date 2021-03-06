﻿using HomeManagement.App.Common;
using HomeManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HomeManagement.App.Services.Rest
{
    public class StorageRestClient
    {
        private const string AuthorizationHeader = "Authorization";

        public async Task<IEnumerable<StorageFileModel>> Get()
        {
            if (Connectivity.NetworkAccess.Equals(NetworkAccess.None)) throw new AppException($"No internet connection detected.");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Constants.Endpoints.STORAGE_API);
                client.DefaultRequestHeaders.Add(AuthorizationHeader, await GetToken());

                var result = await client.GetAsync("storage");

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var files = JsonConvert.DeserializeObject<IEnumerable<StorageFileModel>>(content);

                return files;
            }
        }

        public async Task<Stream> Get(string tag)
        {
            if (Connectivity.NetworkAccess.Equals(NetworkAccess.None)) throw new AppException($"No internet connection detected.");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Constants.Endpoints.STORAGE_API);
                client.DefaultRequestHeaders.Add(AuthorizationHeader, await GetToken());

                var result = await client.GetAsync($"storage/{tag}");

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStreamAsync();

                return content;
            }
        }

        private async Task<string> GetToken()
        {
            return await SecureStorage.GetAsync("Token");
        }
    }
}