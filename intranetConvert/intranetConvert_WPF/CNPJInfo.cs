using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.Export.HtmlExport.StyleCollectors.StyleContracts;
using System.IO;
using System.Net.Http;


namespace intranetConvert_WPF
{
    public class CNPJInfo
    {

        public string Cnpj { get; set; }
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
                    string caminho = _configuracoes.CaminhoConsultaCnpj;
                    bool porApi = false;

                    if (caminho.StartsWith("http://"))
                        porApi = true;

                    if (porApi)
                    {
                        caminho = $"http://ws.hubdodesenvolvedor.com.br/v2/cnpj/?cnpj=${cnpj}&token=161080935xNpKnUZqtX290826744";
                        HttpResponseMessage response = await client.GetAsync(caminho);

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
                    else
                    {
                        return BuscarCNPJNaPlanilha(caminho, cnpj);
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

        public static CNPJInfo BuscarCNPJNaPlanilha(string caminhoPlanilha, string cnpj)
        {
            // Remove caracteres especiais do CNPJ para comparação
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            // Lê todos os dados da planilha
            var listaCNPJInfo = LerPlanilhaExcel(caminhoPlanilha);

            // Busca o CNPJ na lista
            var cnpjEncontrado = listaCNPJInfo.FirstOrDefault(c =>
                c.Cnpj.Replace(".", "").Replace("-", "").Replace("/", "") == cnpj);

            // Retorna o objeto CNPJInfo encontrado ou null se não encontrar
            return cnpjEncontrado;
        }

        public static List<CNPJInfo> LerPlanilhaExcel(string caminhoPlanilha)
        {
            var listaCNPJInfo = new List<CNPJInfo>();

            // Verifica se o caminho existe
            if (!Directory.Exists(caminhoPlanilha))
            {
                Console.WriteLine("Caminho não encontrado.");
                return listaCNPJInfo;
            }

            // Obtém todos os arquivos Excel compatíveis no diretório
            var arquivosExcel = Directory.EnumerateFiles(caminhoPlanilha, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                .Where(s => Path.GetFileName(s).StartsWith("contatos", StringComparison.OrdinalIgnoreCase));

            // Percorre cada arquivo Excel encontrado
            foreach (var arquivo in arquivosExcel)
            {
                IWorkbook workbook;

                // Abre o arquivo Excel de acordo com a extensão
                using (var fileStream = new FileStream(arquivo, FileMode.Open, FileAccess.Read))
                {
                    if (Path.GetExtension(arquivo).Equals(".xls", StringComparison.OrdinalIgnoreCase))
                    {
                        workbook = new HSSFWorkbook(fileStream);
                    }
                    else if (Path.GetExtension(arquivo).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        workbook = new XSSFWorkbook(fileStream);
                    }
                    else // .csv - assumindo que é um CSV separado por vírgula
                    {
                        // Lógica para ler arquivos CSV usando um leitor de CSV
                        // (Você pode usar uma biblioteca dedicada para CSV ou implementar sua própria lógica)
                        continue; // Pula para o próximo arquivo
                    }
                }

                // Seleciona a primeira planilha
                var planilha = workbook.GetSheetAt(0);

                // Lê os dados da planilha a partir da segunda linha (ignorando o cabeçalho)
                for (int linha = 1; linha <= planilha.LastRowNum; linha++)
                {
                    var row = planilha.GetRow(linha);
                    if (row == null) continue; // Pula linhas vazias

                    var cnpjInfo = new CNPJInfo
                    {
                        Nome = ObterValorCelula(row.GetCell(2)),  // Coluna 'nome' (índice 2)
                        Logradouro = ObterValorCelula(row.GetCell(4)),  // Coluna 'endereco' (índice 4)
                        Numero = ObterValorCelula(row.GetCell(5)),  // Coluna 'numero' (índice 5)
                        Complemento = ObterValorCelula(row.GetCell(6)),  // Coluna 'complemento' (índice 6)
                        Bairro = ObterValorCelula(row.GetCell(7)),  // Coluna 'bairro' (índice 7)
                        Municipio = ObterValorCelula(row.GetCell(9)), // Coluna 'cidade' (índice 9)
                        Uf = ObterValorCelula(row.GetCell(10)), // Coluna 'uf' (índice 10)
                        Cep = ObterValorCelula(row.GetCell(8)),  // Coluna 'cep' (índice 8)
                        Telefone = ObterValorCelula(row.GetCell(12)), // Coluna 'fone' (índice 12)
                        Email = ObterValorCelula(row.GetCell(15)),  // Coluna 'e_mail' (índice 15)
                        Cnpj = ObterValorCelula(row.GetCell(18)).Replace(".", "").Replace("-", "").Replace("/", "")  // Coluna 'e_mail' (índice 16)
                    };

                    listaCNPJInfo.Add(cnpjInfo);
                }
            }
            return listaCNPJInfo;
        }

        // Método auxiliar para obter o valor da célula como string
        private static string ObterValorCelula(ICell cell)
        {
            if (cell == null) return "";

            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Numeric:
                    // Formata como número inteiro se não tiver casas decimais
                    return cell.NumericCellValue == Math.Floor(cell.NumericCellValue)
                        ? cell.NumericCellValue.ToString("0")
                        : cell.NumericCellValue.ToString();
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                default:
                    return cell.ToString() ?? "";
            }
        }

    }
}