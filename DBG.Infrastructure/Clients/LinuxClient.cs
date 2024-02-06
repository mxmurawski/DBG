#region

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Clients;

public class LinuxClient : ILinuxClient
{
    private readonly HttpClient _client = new();

    public LinuxClient(string address)
    {
        _client.BaseAddress = new Uri(address);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task CheckConnectivityAsync()
    {
        if (await PingAsync() != true) throw new Exception("Cannot connect to SSH worker.");
    }

    public async Task<string?> ExecuteCommandAsync(Guid guid, string query, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/ExecuteCommand", query);
        var responseMessage = await response.Content.ReadFromJsonAsync<string?>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/AddOrUpdateConnection");
        var responseMessage = await response.Content.ReadFromJsonAsync<bool>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync($"{guid}/DeleteConnection");
        var responseMessage = await response.Content.ReadFromJsonAsync<bool>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> PingAsync()
    {
        var response = await _client.GetAsync("/");
        return response.IsSuccessStatusCode;
    }

    public async Task<OsStaticState?> GetStaticStateAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/GetStaticState");
        var responseMessage = await response.Content.ReadFromJsonAsync<OsStaticState>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<OsDynamicState?> GetDynamicStateAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/GetDynamicState");
        var responseMessage = await response.Content.ReadFromJsonAsync<OsDynamicState>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> TestSystemConnectivityAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/TestSystemConnectivity");
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception(responseMessage);
    }
}