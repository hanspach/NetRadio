using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetRadio.ViewModels
{
    class SettingViewModel : ViewModelBase
    {
        public ICommand SettingsChanedCommand { get; private set; }
        private BackgroundWorker countryworker = new BackgroundWorker();
        private BackgroundWorker categoryworker = new BackgroundWorker();

        private string apiKey;
        public string ApiKey
        {
            get { return apiKey; }
            set { SetProperty<string>(ref apiKey, value);}
        }

        private string urlStatesRequest;
        public string UrlStatesRequest
        {
            get { return urlStatesRequest; }
            set { SetProperty<string>(ref urlStatesRequest, value); }
        }

        private string urlCategoriesRequest;
        public string UrlCategoriesRequest
        {
            get { return urlCategoriesRequest; }
            set { SetProperty<string>(ref urlCategoriesRequest, value); }
        }

        private ObservableCollection<string> countries;
        public ObservableCollection<string> Countries
        {
            get { return countries; }
            set { SetProperty(ref countries, value); }
        }
        private string currentCountry;
        public string CurrentCountry
        {
            get { return currentCountry; }
            set { SetProperty(ref currentCountry, value); }
        }

        private ObservableCollection<string> categories;
        public ObservableCollection<string> Categories
        {
            get { return categories; }
            set { SetProperty(ref categories, value); }
        }
        private string currentCategory;
        public string CurrentCategory
        {
            get { return currentCategory; }
            set { SetProperty(ref currentCategory, value); }
        }

        public SettingViewModel() : base()
        {
            SettingsChanedCommand = new ActionCommand(s => { SettingsChanged(s); }, s => { return !string.IsNullOrEmpty(apiKey); });

            if (!string.IsNullOrEmpty((string)Settings.Properties["ApiKey"])) 
                ApiKey = (string)Settings.Properties["ApiKey"];
            if (string.IsNullOrEmpty((string)Settings.Properties["UrlStatesRequest"]))
                urlStatesRequest = "http://api.dirble.com/v2/countries?";
            else
                urlStatesRequest = (string)Settings.Properties["UrlStatesRequest"];
            if (string.IsNullOrEmpty((string)Settings.Properties["UrlCategoriesRequest"]))
                urlCategoriesRequest = "http://api.dirble.com/v2/categories?";
            else
                urlCategoriesRequest = (string)Settings.Properties["UrlCategoriesRequest"];
            if (!string.IsNullOrEmpty(ApiKey))
            {
                UrlStatesRequest += "token=" + ApiKey;
                UrlCategoriesRequest += "token" + ApiKey;
            }
            countries = new ObservableCollection<string>();
           
                foreach (State state in JsonHelper.States)
                    countries.Add(state.Name);
            categories = new ObservableCollection<string>();
           
                foreach (Category cat in JsonHelper.Categories)
                    categories.Add(cat.Title);
        }

        private void SettingsChanged(object o)
        {
            Console.WriteLine("SettingsChanged");
        }
    }
}
