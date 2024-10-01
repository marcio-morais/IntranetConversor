using intranetConvert_WPF.Integracao.bling.Models;
using intranetConvert_WPF.Integracao.bling.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

public class BlingApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private PedidoParseXML _pedidoParseXML= new PedidoParseXML();

    public BlingApiService(string apiKey)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;
    }

    public async Task SendOrderAsync(Pedido pedido)
    {
        var xml = _pedidoParseXML.GerarXml(pedido);
        SaveXmlToFile(xml, pedido.cliente.cpf_cnpj);

        var url = $"https://bling.com.br/Api/v3/pedido/json/?apikey={_apiKey}";
        var content = new StringContent($"xml={xml}", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }
    private void SaveXmlToFile(string xmlContent, string clientCNPJ)
    {
        var directoryPath = @"C:\Users\First Class\Desktop\Nova pasta";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = $"pedido_{clientCNPJ}_{DateTime.Now:yyyyMMddHHmmss}.xml";
        var filePath = Path.Combine(directoryPath, fileName);

        File.WriteAllText(filePath, xmlContent);
    }
}