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