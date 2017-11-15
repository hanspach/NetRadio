using System.Threading.Tasks;

namespace NetRadio.ViewModels
{
    class BrowserViewModel : ViewModelBase
    {
        private string url;
        public string Url
        {
            get { return url; }
            set { SetProperty<string>(ref url, value); }
        }
    }
}
