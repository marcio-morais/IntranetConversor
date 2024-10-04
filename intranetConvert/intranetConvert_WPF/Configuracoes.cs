using intranetConvert_WPF.Integracao.bling.Models;
using System.ComponentModel;

public class Configuracoes : INotifyPropertyChanged, IDataErrorInfo
{
    private string _pastaRemessa;
    public string PastaRemessa
    {
        get => _pastaRemessa;
        set
        {
            if (_pastaRemessa != value)
            {
                _pastaRemessa = value;
                OnPropertyChanged(nameof(PastaRemessa));
            }
        }
    }

    private string _pastaCSV;

    public string PastaCSV
    {
        get => _pastaCSV;
        set
        {
            if (_pastaCSV != value)
            {
                _pastaCSV = value;
                OnPropertyChanged(nameof(PastaCSV));
            }
        }
    }

    private int _tempoDeEspera;

    public int TempoDeEspera
    {
        get => _tempoDeEspera;
        set
        {
            if (_tempoDeEspera != value)
            {
                _tempoDeEspera = value;
                OnPropertyChanged(nameof(TempoDeEspera));
            }
        }
    }

    private string _tipoIntegracao;

    public string TipoIntegracao
    {
        get => _tipoIntegracao;
        set
        {
            if (_tipoIntegracao != value)
            {
                _tipoIntegracao = value;
                OnPropertyChanged(nameof(TipoIntegracao));
            }
        }
    }

    private bool _consultarCNPJ;
    public bool ConsultarCNPJ {
        get => _consultarCNPJ;
        set
        {
            if (_consultarCNPJ != value)
            {
                _consultarCNPJ = value;
                OnPropertyChanged(nameof(_consultarCNPJ));
            }
        }
    }

    private string _apiCNPJ;
    public string ApiCNPJ
    {
        get => _apiCNPJ;
        set
        {
            if (_apiCNPJ != value)
            {
                _apiCNPJ = value;
                OnPropertyChanged(nameof(_apiCNPJ));
            }
        }
    }

    private ApiBlingConfig _apiBlingConfig;
    public ApiBlingConfig ApiBlingConfig
    {
        get => _apiBlingConfig;
        set
        {
            if (_apiBlingConfig != value)
            {
                _apiBlingConfig = value;
                OnPropertyChanged(nameof(_apiBlingConfig));
            }
        }
    }

    public string Error => null;
    public string this[string columnName]
    {
        get
        {
            if (columnName == nameof(TempoDeEspera))
            {
                if (TempoDeEspera < 0)
                {
                    return "O tempo de espera não pode ser negativo.";
                }
            }
            return null;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}