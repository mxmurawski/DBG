#region

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Clients;

public class MssqlClient : IMssqlClient
{
    private readonly HttpClient _client = new();

    public MssqlClient(string address)
    {
        _client.BaseAddress = new Uri(address);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task CheckConnectivityAsync()
    {
        if (await PingAsync() != true) throw new Exception("Cannot connect to MSSQL worker.");
    }

    public async Task<DbResult?> ExecuteQueryAsync(Guid guid, string query, StringValues token,
        string database = "master")
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/ExecuteQuery/{database}", query);
        var responseMessage = await response.Content.ReadFromJsonAsync<DbResult?>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<int> ExecuteNonQueryAsync(Guid guid, string query, StringValues token, string database = "master")
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/ExecuteNonQuery/{database}", query);
        var responseMessage = await response.Content.ReadFromJsonAsync<int>();
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

    public async Task<DbStaticState?> GetStaticStateAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/GetStaticState");
        var responseMessage = await response.Content.ReadFromJsonAsync<DbStaticState>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<DbDynamicState?> GetDynamicStateAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/GetDynamicState");
        var responseMessage = await response.Content.ReadFromJsonAsync<DbDynamicState>();
        return response.IsSuccessStatusCode
            ? responseMessage
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<string?> GetPermissionAuditResultAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/GetPermissionAuditResult");
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage : throw new Exception(responseMessage);
    }

    public async Task<bool> TestSystemConnectivityAsync(Guid guid, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{guid}/TestSystemConnectivity");
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception(responseMessage);
    }

    public async Task<bool> CreateUserAsync(Guid guid, CreateRequest request, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/CreateUser", request);
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode
            ? responseMessage == "true"
            : throw new Exception(await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> CreateLoginAsync(Guid guid, CreateRequest request, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/CreateLogin", request);
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception(responseMessage);
    }

    public async Task<bool> DropUserAsync(Guid guid, DropRequest request, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/DropUser", request);
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception(responseMessage);
    }

    public async Task<bool> DropLoginAsync(Guid guid, DropRequest request, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/DropLogin", request);
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception(responseMessage);
    }

    public async Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/Grant", request);
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception(responseMessage);
    }

    public async Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request, StringValues token)
    {
        await CheckConnectivityAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync($"{guid}/Revoke", request);
        var responseMessage = await response.Content.ReadAsStringAsync();
        return response.IsSuccessStatusCode ? responseMessage == "true" : throw new Exception();
    }
}