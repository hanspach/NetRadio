using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Reflection;

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

    static class JsonHelper
    {
        public static ObservableCollection<ProgramProps> ReadJson()
        {
            using (StreamReader r = new StreamReader(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location) + @"\Resources\data.json"))
            {
                string json = r.ReadToEnd();
                ObservableCollection <ProgramProps> res = JsonConvert.DeserializeObject<ObservableCollection<ProgramProps>>(json);
                foreach(ProgramProps pp in res)         
                {
                    pp.CurrentStream = pp.Streams[0];
                }
                return res;
            }
        }
    }
}
