using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;

namespace NetRadio.ViewModels
{
    class VisualViewModel : ViewModelBase, IDisposable
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);
        BackgroundWorker worker = new BackgroundWorker();

        private IList<MetadataModel> metadata;
        public IList<MetadataModel> Metadata
        {
            get { return metadata; }
            set
            {
                SetProperty<IList<MetadataModel>>(ref metadata, value);
                if (metadata != null)
                {
                    int i;
                    for (i=0; i < Metadata.Count; ++i)
                    {
                        if (Metadata[i].Name == "icy-name")
                        {
                            SelectedMeta = Metadata[i];
                            break;
                        }
                    }
                    if (i == Metadata.Count)            // nothings found
                        SelectedMeta = Metadata[0];     // take the first entry
                }
            }
        }

        private MetadataModel selectedMeta;
        public MetadataModel SelectedMeta
        {
            get { return selectedMeta; }
            set { SetProperty<MetadataModel>(ref selectedMeta, value); }
        }

        private string albumImagePath;
        public string AlbumImagePath
        {
            get { return albumImagePath; }
            set { SetProperty<string>(ref albumImagePath, value); }
        }

        public VisualViewModel()
        {
            WebRadioControl.Instance.OnMessageChanged += ((s, args) => {
                if(!worker.IsBusy)
                    worker.RunWorkerAsync(args.Message.Text);
            });
            WebRadioControl.Instance.OnMetadataChanged += ((s, args) =>{ Metadata = args.Metadata;});
            worker.DoWork += new DoWorkEventHandler(LoadAlbumImage);
            worker.RunWorkerCompleted += ((s, args) => { AlbumImagePath = args.Result as string; });
            AlbumImagePath = Settings.ResourcePath + "album.png";
        }

        private void LoadAlbumImage(object sender, DoWorkEventArgs args)
        {
            string msg = args.Argument as string;
            string title = string.Empty;
            string actor = string.Empty;
            string[] separators = new string[] { " - ", ": "," -- "};
            string[] res = msg.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if(res.Length == 2)
            {
                title = res[1];
                actor = res[0];
            }
            else if(res.Length == 3)
            {
                title = res[1];
                actor = res[2];
            }
            Console.WriteLine("Title:{0}, Sänger:{1}",title,actor);
            if (title.Length > 0 && actor.Length > 0)
            {
                try
                {
                    string url = GetAlbumImageUrl(title, actor);
                    if (url.Length != 0)
                        args.Result = url;
                    else
                        args.Result = Settings.ResourcePath + "album.png";
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
                    args.Result = Settings.ResourcePath + "album.png";
                }
            }
            else
            {
                args.Result = Settings.ResourcePath + "album.png";
            }
        }

        private MatchCollection GetWebServiceData(string url, string pattern)
        {
            string s = string.Empty;
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.UserAgent = "MusicBrainze.API/2.0";
                request.Proxy = WebRequest.DefaultWebProxy;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                var response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    s = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("{0}\n{1}", e.Message, e.StackTrace));
                return null;
            }
            return Regex.Matches(s, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private string GetAlbumImageUrl(string title, string artist, string header = "http://musicbrainz.org/")
        {
            string url = string.Format
                 ("{0}ws/2/recording?query=\"{1}\" AND artist:\"{2}\"", header, HttpUtility.UrlEncode(title), HttpUtility.UrlEncode(artist));

             MatchCollection matches = GetWebServiceData(url, "<release id=\"(?<id>.+?)\"");
            if (matches != null)
            {
                foreach (Match match in matches)
                {
                    url = Uri.EscapeUriString(string.Format("{0}release/{1}", header, match.Groups["id"]));
                    MatchCollection mc = GetWebServiceData(url, "data-small-thumbnail=\"(?<id>.+?)\"");
                    foreach (Match m in mc)
                    {
                        string smallurl = "http:" + m.Groups["id"].Value;
                        if (Uri.IsWellFormedUriString(smallurl, UriKind.Absolute))
                        {
                            return smallurl;
                        }
                    }
                }
            }
            return string.Empty;
        }

        public void Dispose() => worker.Dispose(); 
    }
}
