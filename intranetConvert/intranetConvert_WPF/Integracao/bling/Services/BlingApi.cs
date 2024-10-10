using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using intranetConvert_WPF.Integracao.bling.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Controls;
using intranetConvert_WPF.Integracao.bling.Models.PedidoJson;

public class BlingApi
{
    private readonly ApiBlingConfig _config;

    public BlingApi(ApiBlingConfig config)
    {
        _config = config;
    }

    public string GenerateState()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] randomBytes = new byte[16];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }

    public string GetAuthorizationUrl()
    {
        return $"{_config.Url}/oauth/authorize?response_type=code&client_id={_config.ClientId}&state={_config.State}";
    }

    public async Task<Token> GetAccessTokenAsync(string authorizationCode)
    {
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new StringContent(
                $"grant_type=authorization_code&code={authorizationCode}&redirect_uri=http://gestoque.com.br",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            HttpResponseMessage response = await client.PostAsync($"https://api.bling.com.br/Api/v3/oauth/token", requestBody);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            // Parse o JSON de resposta para extrair o access_token
            var tokenData = JsonConvert.DeserializeObject<Token>(responseBody);

            return tokenData;
        }
    }
   
    public async Task ExportToApiAsync(List<object> todosPedidosApi)
    {
        foreach (Pedido pedido in todosPedidosApi)
        {            
            await EnviarPedidoAsync(pedido, _config.Token.AccessToken);
        }
    }

    private async Task EnviarPedidoAsync(Pedido pedido, string accessToken)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            string pedidoJson = JsonConvert.SerializeObject(pedido);
            
            string server = "https://developer.bling.com.br/api/bling";
            string serverProducao = "https://api.bling.com.br/Api/v3";

            var content = new StringContent(pedidoJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync($"{server}/pedidos/vendas", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Pedido {pedido.Contato.NumeroDocumento} enviado com sucesso.");
            }
            else
            {
                Console.WriteLine($"Erro ao enviar o pedido {pedido.Contato.NumeroDocumento}: {response.ReasonPhrase}");
            }
        }
    }
}

public class Token
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}

