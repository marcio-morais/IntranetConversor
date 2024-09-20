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
                string url = $"https://www.receitaws.com.br/v1/cnpj/{cnpj}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);

                    return new CNPJInfo
                    {
                        Nome = jsonObject.nome,
                        Logradouro = jsonObject.logradouro,
                        Numero = jsonObject.numero,
                        Complemento = jsonObject.complemento,
                        Bairro = jsonObject.bairro,
                        Municipio = jsonObject.municipio,
                        Uf = jsonObject.uf,
                        Cep = jsonObject.cep,
                        Telefone = jsonObject.telefone,
                        Email = jsonObject.email
                    };
                }
                else
                {
                    Console.WriteLine($"Erro na consulta do CNPJ: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na consulta do CNPJ: {ex.Message}");
                return null;
            }
        }
    }
}