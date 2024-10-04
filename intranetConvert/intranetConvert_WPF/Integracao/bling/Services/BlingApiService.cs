using intranetConvert_WPF;
using intranetConvert_WPF.Integracao.bling.Models;
using intranetConvert_WPF.Integracao.bling.Models.PedidoXml;
using intranetConvert_WPF.Integracao.bling.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

public class BlingApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private PedidoParseXML _pedidoParseXML = new PedidoParseXML();
    private Configuracoes _configuracoes = new Configuracoes();

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
    public BlingApiService(Configuracoes configuracoes)
    {
        _httpClient = new HttpClient();
        _configuracoes = configuracoes;
    }

    public async Task<string> GetAccessTokenAsync(string code)
    {
        var clientId = "ea9cf37975bef2b4ce42d3818531ab3ad2708a05";
        var clientSecret = "41d26b5610e9ab925076b1846e8a3f73ed516a36f0087ca9fe9756074488";
        var redirectUri = "http://www.gestoque.com.br";

        var tokenUrl = "https://www.bling.com.br/Api/v3/oauth/token";
        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
    });

        var response = await _httpClient.PostAsync(tokenUrl, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

        return tokenResponse.AccessToken;
    }

    public async Task SendOrderAsync(Pedido pedido, string accessToken)
    {
        var xml = _pedidoParseXML.GerarXml(pedido);
        SaveXmlToFile(xml, pedido.cliente.cpf_cnpj);

        var url = "https://bling.com.br/Api/v3/pedido/json/";
        var content = new StringContent($"xml={xml}", Encoding.UTF8, "application/x-www-form-urlencoded");
        content.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> ObterTokenAsync()
    {
        //https://www.bling.com.br/Api/v3/oauth/authorize?response_type=code&client_id=[clienteId]&state=[state]
        using (HttpClient client = new HttpClient())
        {
            var response = await client.PostAsync(_configuracoes.ApiBlingConfig.Url, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuracoes.ApiBlingConfig.ClientId),
                new KeyValuePair<string, string>("client_secret", _configuracoes.ApiBlingConfig.ClientSecret),
                new KeyValuePair<string, string>("state", _configuracoes.ApiBlingConfig.State),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            }));

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            // Parse the token from the response content
            // Assuming the response is JSON and contains an access_token field
            var token = Newtonsoft.Json.Linq.JObject.Parse(content)["access_token"].ToString();
            return token;
        }
    }

    public async Task SendOrderAsync(Pedido pedido)
    {
        var xml = _pedidoParseXML.GerarXml(pedido);
        SaveXmlToFile(xml, pedido.cliente.cpf_cnpj);

        var url = $"https://bling.com.br/Api/v3/pedido/json/?apikey={_configuracoes.ApiBlingConfig.ClientId}";
        var content = new StringContent($"xml={xml}", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }
    
    private void SaveXmlToFile(string xmlContent, string clientCNPJ)
    {
        //var directoryPath = @"C:\Users\First Class\Desktop\Nova pasta";
        if (!Directory.Exists(_configuracoes.PastaCSV))
        {
            Directory.CreateDirectory(_configuracoes.PastaCSV);
        }

        var fileName = $"pedido_{clientCNPJ}_{DateTime.Now:yyyyMMddHHmmss}.xml";
        var filePath = Path.Combine(_configuracoes.PastaCSV, fileName);

        File.WriteAllText(filePath, xmlContent);
    }
}