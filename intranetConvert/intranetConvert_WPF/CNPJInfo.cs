using Newtonsoft.Json;
using System.Net.Http;

namespace intranetConvert_WPF
{
    public class CNPJInfo
    {
        public string Nome { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string Uf { get; set; }
        public string Cep { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
    }

    public class CNPJConsulta
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<CNPJInfo> ConsultarCNPJ(string cnpj)
        {
            try
            {
                var _configuracoes = ConfiguracaoManager.CarregarConfiguracoes();

                if (_configuracoes.ConsultarCNPJ == true)
                {
                    //string url = $"https://www.receitaws.com.br/v1/cnpj/{cnpj}";
                    string url = $"http://ws.hubdodesenvolvedor.com.br/v2/cnpj/?cnpj=${cnpj}&token=161080935xNpKnUZqtX290826744";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);

                        if (jsonObject != null)
                        {
                            var CNPJInfo = new CNPJInfo();

                            CNPJInfo.Nome = jsonObject.result.nome;
                            CNPJInfo.Logradouro = jsonObject.result.logradouro;
                            CNPJInfo.Numero = jsonObject.result.numero;
                            CNPJInfo.Complemento = jsonObject.result.complemento;
                            CNPJInfo.Bairro = jsonObject.result.bairro;
                            CNPJInfo.Municipio = jsonObject.result.municipio;
                            CNPJInfo.Uf = jsonObject.result.uf;
                            CNPJInfo.Cep = jsonObject.result.cep;
                            CNPJInfo.Telefone = jsonObject.result.telefone;
                            CNPJInfo.Email = jsonObject.result.email;

                            return CNPJInfo;
                        }

                    }
                    else
                    {
                        Console.WriteLine($"Erro na consulta do CNPJ: {response.StatusCode}");
                        return null;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na consulta do CNPJ: {ex.Message}");
                return null;
            }
        }
    }
}