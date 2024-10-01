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

    public string Error => null;

    private bool _forApi;

    public bool ForApi
    {
        get => _forApi;
        set
        {
            if (_forApi != value)
            {
                _forApi = value;
                OnPropertyChanged(nameof(ForApi));
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