# ��� IntranetConversor

Aplicação WPF desenvolvida para **ECO BRAZIL** para conversão e processamento de arquivos de remessa com integração ao sistema Bling.

## ��� Sobre o Projeto

O IntranetConversor é uma ferramenta desktop robusta desenvolvida em **C# .NET 8 com WPF** que automatiza o processo de conversão de arquivos de remessa, facilitando a integração entre sistemas internos e a plataforma Bling ERP.

## ✨ Funcionalidades Principais

- ��� **Conversão de Arquivos de Remessa**: Processamento automático de arquivos de dados
- �� **Consulta CNPJ**: Integração com APIs para validação e enriquecimento de dados
- ��� **Integração Bling**: Conexão direta com API do Bling ERP
- ��� **Exportação Excel**: Geração de planilhas usando EPPlus
- ⚙️ **Configurações Avançadas**: Interface para personalização de parâmetros
- ��� **System Tray**: Execução em segundo plano com notificações
- ��� **Monitoramento**: Sistema de timer para processamento automático

## ���️ Tecnologias Utilizadas

- **Framework**: .NET 8.0 (Windows)
- **Interface**: WPF (Windows Presentation Foundation)
- **Linguagem**: C# com Implicit Usings
- **Bibliotecas Principais**:
  - `EPPlus` - Manipulação de arquivos Excel
  - `NPOI` - Processamento de documentos Office
  - `Newtonsoft.Json` - Serialização JSON
  - `Ookii.Dialogs.Wpf` - Diálogos avançados
  - `System.Windows.Forms` - Integração com Windows Forms

## ���️ Estrutura do Projeto

```
intranetConvert_WPF/
├── ��� Integracao/
│   └── bling/                  # Integração com API Bling
├── ��� UserControls/            # Controles customizados
│   ├── InputBox.xaml          # Caixa de entrada personalizada
│   └── SplashScreenControl.xaml # Tela de carregamento
├── ��� Resources/               # Recursos e ícones
├── MainWindow.xaml             # Interface principal
├── CNPJInfo.cs                 # Processamento de dados CNPJ
├── RemessaParser.cs            # Parser de arquivos de remessa
├── Configuracoes.cs            # Gerenciamento de configurações
└── Config.xml                  # Arquivo de configuração
```

## ��� Como Executar

### Pré-requisitos
- Windows 10/11
- .NET 8.0 Runtime
- Visual Studio 2022 (para desenvolvimento)

### Instalação
1. Clone o repositório:
```bash
git clone https://github.com/marcio-morais/IntranetConversor.git
```

2. Navegue até o diretório:
```bash
cd IntranetConversor/intranetConvert
```

3. Compile e execute:
```bash
dotnet build
dotnet run --project intranetConvert_WPF
```

## ⚙️ Configuração

1. **Configurações Bling**: Configure as credenciais da API do Bling
2. **Caminhos de Arquivo**: Defina diretórios de entrada e saída
3. **Parâmetros de Conversão**: Ajuste regras de processamento
4. **Timer de Monitoramento**: Configure intervalos de execução automática

## ��� Funcionalidades Detalhadas

### ��� Processamento de Remessas
- Leitura de arquivos em diversos formatos
- Validação de dados estruturais
- Conversão para formato padronizado
- Geração de relatórios de erro

### ��� Integração CNPJ
- Consulta automática de dados empresariais
- Validação de documentos
- Enriquecimento de informações
- Cache local para otimização

### ��� Exportação e Relatórios
- Geração de planilhas Excel detalhadas
- Relatórios de processamento
- Logs de operações
- Estatísticas de conversão

## ��� Contribuição

Este projeto foi desenvolvido especificamente para **ECO BRAZIL**. Para contribuições ou suporte técnico, entre em contato com a equipe de desenvolvimento.

## ���‍��� Desenvolvedor

**Márcio Morais**
- ��� Developer @ firstclassHome
- ��� Especialista em C#/.NET/WPF e React Native
- ��� [Contato via GitHub](https://github.com/marcio-morais)

## ��� Licença

Este projeto é licenciado sob os termos especificados no arquivo [LICENSE](LICENSE).

---

**⚡ Desenvolvido com C# e WPF para automação de processos empresariais**
