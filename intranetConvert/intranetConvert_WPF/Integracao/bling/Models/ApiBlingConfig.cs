
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace intranetConvert_WPF.Integracao.bling.Models
{
    public class ApiBlingConfig : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _clientId;
        public string ClientId
        {
            get => _clientId;
            set
            {
                if (_clientId != value)
                {
                    _clientId = value;
                    OnPropertyChanged(nameof(_clientId));
                }
            }
        }

        private string _clientSecret;
        public string ClientSecret
        {
            get => _clientSecret;
            set
            {
                if (_clientSecret != value)
                {
                    _clientSecret = value;
                    OnPropertyChanged(nameof(_clientSecret));
                }
            }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged(nameof(_url));
                }
            }
        }

        private string _state;
        public string State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(_state));
                }
            }
        }

        private string _code;
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged(nameof(_code));
                }
            }
        }

        //Token

        private Token _token;

        public Token Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    _token = value;
                    OnPropertyChanged(nameof(_token));
                }
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(ClientId))
                {
                    if (ClientId.Equals(""))
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
}
