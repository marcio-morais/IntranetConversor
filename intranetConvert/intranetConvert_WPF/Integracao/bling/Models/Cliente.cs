// Models/Client.cs
using intranetConvert_WPF;
using System.CodeDom;

namespace intranetConvert_WPF.Integracao.bling.Models
{
    public class Cliente
    {
        public string nome { get; set; }
        public string tipoPessoa { get; set; }
        public string endereco { get; set; }
        public string cpf_cnpj { get; set; }
        public string ie { get; set; }
        public string Number { get; set; }
        public string complemento { get; set; }
        public string bairro { get; set; }
        public string cep { get; set; }
        public string cidade { get; set; }
        public string uf { get; set; }
        public string fone { get; set; }
        public string email { get; set; }

        public Cliente()
        {

        }

        public Cliente(string cnpj)
        {
            _ = BuscarDadosCliente(cnpj);
        }

        private async Task<CNPJInfo> BuscarDadosCliente(string cnpj)
        {
            CNPJInfo cnpjInfo = await CNPJConsulta.ConsultarCNPJ(cnpj);
            if (cnpjInfo != null)
            {
                nome = cnpjInfo.Nome;
                tipoPessoa = "J";
                endereco = cnpjInfo.Logradouro;
                Number = cnpjInfo.Numero;
                complemento = cnpjInfo.Complemento;
                bairro = cnpjInfo.Bairro;
                cep = cnpjInfo.Cep;
                cidade = cnpjInfo.Municipio;
                uf = cnpjInfo.Uf;
                fone = cnpjInfo.Telefone;
                email = cnpjInfo.Email;
            }

            return cnpjInfo;
        }
    }

}