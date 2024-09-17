using System.ComponentModel;

public class Configuracoes : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}