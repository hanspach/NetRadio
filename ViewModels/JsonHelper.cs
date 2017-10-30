using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Reflection;
using log4net;
using System;
using System.Collections.Generic;

namespace NetRadio.ViewModels
{
    class JsonStream
    {
        [JsonProperty("stream")]
        public string Url { get; set; }

        [JsonProperty("bitrate")]
        public int? Bitrate { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("listeners")]
        public int Listeners { get; set; }
    }

    class ProgramProps : ViewModelBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("streams")]
        public ObservableCollection<JsonStream> Streams { get; set; }

        private JsonStream currentStream;
        public JsonStream CurrentStream
        {
            get { return currentStream; }
            set { currentStream = value; OnPropertyChanged("CurrentStream"); }
        }
    }

    class State
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("region")]
        public string Continent { get; set; }
    }

    static class JsonHelper
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);

        public static ObservableCollection<State> GetCountries()
        {
            ObservableCollection<State> res = null;
            string json = ReadJsonFile("countries.json");
            if (!string.IsNullOrEmpty(json))
            {
                res = JsonConvert.DeserializeObject<ObservableCollection<State>>(json);
            }
            return res == null ? new ObservableCollection<State>() : res;
        }

        public static Dictionary<string,string> GetVisitedCountries()
        {
            var res = new Dictionary<string, string>();
            var countries = GetCountries();
            foreach(ProgramProps p in GetStations())
            {
                if (!res.ContainsKey(p.Country))
                {
                    foreach(State state in countries)
                    {
                        if(p.Country == state.CountryCode)
                        {
                            res.Add(p.Country, state.Name);
                            break;
                        }
                    }
                }
            }
            return res;
        }
        public static ObservableCollection<ProgramProps> GetStations()
        {
            ObservableCollection<ProgramProps> res = null;
            string json = ReadJsonFile("data.json");
            if(!string.IsNullOrEmpty(json))
            { 
                res = JsonConvert.DeserializeObject<ObservableCollection<ProgramProps>>(json);
                foreach(ProgramProps pp in res)         
                {
                    pp.CurrentStream = pp.Streams[0];
                }
            }
            return res == null ? new ObservableCollection<ProgramProps>() : res;
        }

        static string ReadJsonFile(string filename)
        {
            try
            {
                using (StreamReader r = new StreamReader(Settings.ResourcePath + filename))
                {
                    return r.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
                return null;
            }
        }
    }
}
