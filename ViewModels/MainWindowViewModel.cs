using log4net;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using System;
using System.IO;

namespace NetRadio.ViewModels
{
    class MainWindowViewModel : ViewModelBase, IDropTarget
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);

        private bool isSilence;
        public bool TreeChanged { get; set; }

        public ICommand EditViewCommand { get; private set; }
        public ICommand BrowserViewCommand { get; private set; }
        public ICommand VisualViewCommand { get; private set; }
        public ICommand SettingViewCommand { get; private set; }
        public ICommand PlayProgramCommand { get; private set; }
        public ICommand MuteCommand { get; private set; }
        public ICommand RecordCommand { get; private set; }
        public ICommand Favorite1Command { get; private set; }
        public ICommand Favorite2Command { get; private set; }
        public ICommand Favorite3Command { get; private set; }
        public ICommand AddFavoriteCommand { get; private set; }

        public CountryModel CountryModel { get; private set; }
        public EditViewModel EditViewModel { get; private set; }
        public BrowserViewModel BrowserViewModel { get; private set; }
        public VisualViewModel VisualViewModel { get; private set; }
        public SettingViewModel SettingViewModel { get; private set; }

        private ViewModelBase currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get { return currentViewModel; }
            set { SetProperty<ViewModelBase>(ref currentViewModel, value); }
        }

        private MessageModel message;
        public MessageModel Message
        {
            get { return message; }
            set { SetProperty<MessageModel>(ref message, value); }
        }

        private bool hasPlayed;
        public bool HasPlayed
        {
            get { return hasPlayed; }
            set { SetProperty<bool>(ref hasPlayed, value); }
        }

        private string playButtonIconPath;
        public string PlayButtonIconPath
        {
            get { return playButtonIconPath; }
            set {
                SetProperty<string>(ref playButtonIconPath, value);
                IsPauseImage = playButtonIconPath.EndsWith("pause.png");
            }
        }

        private bool isPauseImage;
        public bool IsPauseImage
        {
            get { return isPauseImage; }
            set { SetProperty<bool>(ref isPauseImage, value); }
        }

        private string favoriteButtonIconPath;
        public string FavoriteButtonIconPath
        {
            get { return favoriteButtonIconPath; }
            set { SetProperty<string>(ref favoriteButtonIconPath, value); }
        }

        private float volume;
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                Settings.Properties["Volume"] = volume.ToString();
                WebRadioControl.Instance.SetVolume(volume / 100);
            }
        }

        private string favorite1Name;
        public string Favorite1Name
        {
            get { return favorite1Name; }
            set { SetProperty<string>(ref favorite1Name, value); }
        }
        private string favorite2Name;
        public string Favorite2Name
        {
            get { return favorite2Name; }
            set { SetProperty<string>(ref favorite2Name, value); }
        }
        private string favorite3Name;
        public string Favorite3Name
        {
            get { return favorite3Name; }
            set { SetProperty<string>(ref favorite3Name, value); }
        }
        
        public bool IsAddFavorite { get; set; }
        
        public MainWindowViewModel()
        {
            EditViewModel = new EditViewModel(this);
            BrowserViewModel = new BrowserViewModel();
            VisualViewModel = new VisualViewModel();
            SettingViewModel = new SettingViewModel();
            CountryModel = CountryModel.Instance;
            EditViewCommand = new ActionCommand(delegate { CurrentViewModel = EditViewModel; });
            BrowserViewCommand = new ActionCommand(delegate { CurrentViewModel = BrowserViewModel; });
            VisualViewCommand = new ActionCommand(delegate { CurrentViewModel = VisualViewModel; });
            SettingViewCommand = new ActionCommand(delegate { CurrentViewModel = SettingViewModel; });
            if (!File.Exists(Settings.settingsPath))
                CurrentViewModel = SettingViewModel;
            else
                CurrentViewModel = VisualViewModel; // call last saved viewModel!!!!
            PlayProgramCommand = new ActionCommand(Play, CanPlay);
            RecordCommand = new ActionCommand(Record, CanRecord);
            MuteCommand = new ActionCommand(Silence, (o) => { return true; });
            WebRadioControl.Instance.OnMessageChanged += ((s, args) => {
                if (args.Message != null)
                {
                    Message = args.Message;
                    if (Message.StatusColorString == "Green")    //program started
                        Settings.Properties["LastVisitedUrl"] = EditViewModel.CurrentItem.Url;
                }
            });
            PlayButtonIconPath = Settings.ResourcePath + "play.png";
            FavoriteButtonIconPath = Settings.ResourcePath + "white.png";
            if (Settings.Properties.Contains("Volume"))
                Volume = float.Parse((string)Settings.Properties["Volume"]);
            else
                Volume = 10;
            if (Settings.Properties.Contains("LastVisitedUrl"))
                FindTreeNode((string)ViewModels.Settings.Properties["LastVisitedUrl"]);
            Favorite1Command = new ActionCommand((o) => { Favorite("Favorite1");}, (o) =>  { return IsFavorite("Favorite1"); });
            Favorite2Command = new ActionCommand((o) => { Favorite("Favorite2"); }, (o) => { return IsFavorite("Favorite2"); });
            Favorite3Command = new ActionCommand((o) => { Favorite("Favorite3"); }, (o) => { return IsFavorite("Favorite3");});
            AddFavoriteCommand = new ActionCommand((o) => {
                IsAddFavorite = true;
                FavoriteButtonIconPath = Settings.ResourcePath + "Redmark.png";
            }, (o) => { return EditViewModel.CurrentItem != null; });

            if (Settings.Properties.Contains("Favorite1"))
                favorite1Name = ((FavoriteItem)Settings.Properties["Favorite1"]).Name;
            if (Settings.Properties.Contains("Favorite2"))
                favorite2Name = ((FavoriteItem)Settings.Properties["Favorite2"]).Name;
            if (Settings.Properties.Contains("Favorite3"))
                favorite3Name = ((FavoriteItem)Settings.Properties["Favorite3"]).Name;
           
            WebBrowserBehavior.FixBrowserVersion();       // Eintrag in registry
        }
        
        public void DragOver(IDropInfo info)
        {
            if (info.Data != null && info.Data.GetType() == info.TargetItem.GetType()
                && info.Data != info.TargetItem)
            { 
                info.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo info)
        {
            // Indexes of IDropInfo aren't always correct, so calculate them
            if (info.Data is Program && info.TargetItem is Program)
            {
                Station parentSrc = (Station)(info.Data as Program).Parent;
                Station parentDest = (Station)(info.TargetItem as Program).Parent;
                int idxSrc = parentSrc.Programs.IndexOf(info.Data as Program);
                int idxDest = parentDest.Programs.IndexOf(info.TargetItem as Program);
                if (idxSrc != -1 && idxDest != -1)
                {
                    parentSrc.Programs.RemoveAt(idxSrc);
                    parentDest.Programs.Insert(idxDest, info.Data as Program);
                    //TreeChanged = true;
                }
            }
            else if (info.Data is Station && info.TargetItem is Station)
            {
                var stationSrc = info.Data as Station;
                var stationDest = info.TargetItem as Station;
                var parentSrc = stationSrc.Parent as Country;
                var parentDest = stationDest.Parent as Country;
                int idxSrc = parentSrc.Stations.IndexOf(stationSrc);
                int idxDest = parentDest.Stations.IndexOf(stationDest);
                if (idxSrc != -1 && idxDest != -1)
                {
                    parentSrc.Stations.RemoveAt(idxSrc);
                    parentDest.Stations.Insert(idxDest, stationSrc);
                    //TreeChanged = true;
                }
            }
            else if (info.Data is Country && info.TargetItem is Country)
            {
                var countrySrc = info.Data as Country;
                var countryDest = info.TargetItem as Country;
                var parentItem = countrySrc.Parent as RootItem;
                int idxSrc = parentItem.Countries.IndexOf(countrySrc);
                int idxDest = parentItem.Countries.IndexOf(countryDest);
                if (idxSrc != -1 && idxDest != -1)
                {
                    parentItem.Countries.RemoveAt(idxSrc);
                    parentItem.Countries.Insert(idxDest, countrySrc);
                    //TreeChanged = true;
                }
            }
        }

        public bool FindTreeNode(string url)
        {
            foreach (Country c in CountryModel)
                foreach (Station s in c.Stations)
                    foreach (ViewModels.Program p in s.Programs)
                        if (p.Url == url)
                        {
                            p.IsSelected = true;
                            EditViewModel.CurrentItem = p;
                            ViewModels.Station station = (Station)p.Parent;
                            station.IsNodeExpanded = true;
                            ViewModels.Country country = (Country)station.Parent;
                            country.IsNodeExpanded = true;
                            return true;
                        }
            return false;
        }

        private void Play(object o)
        {
            try
            {
                if (!HasPlayed)
                {
                    WebRadioControl.Instance.OpenRadioProgram(EditViewModel.CurrentItem.Url);
                    PlayButtonIconPath = Settings.ResourcePath + "pause.png";
                }
                else
                {
                    WebRadioControl.Instance.Pause();
                    PlayButtonIconPath = Settings.ResourcePath + "play.png";
                }
                HasPlayed = !HasPlayed;
            }
            catch(Exception e)
            {
                log.ErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
            }
        }

        private bool CanPlay(object o)
        {
            if (EditViewModel.CurrentItem != null)
                return EditViewModel.CurrentItem is Program;
            return false;
        }

        private void Record(object o)
        {
            log.ErrorFormat("Record-Button pressed");
            WebRadioControl.Instance.RecordProgram();
        }

        private bool CanRecord(object o)
        {
            return true; // WebRadioControl.Instance.IsPlaying; 
        }

        private void Silence(object o)
        {
            if (isSilence)
                WebRadioControl.Instance.SetVolume(volume / 100);
            else
                WebRadioControl.Instance.SetVolume(0);
            isSilence = !isSilence;
        }

        private void Favorite(string key)
        {
            if (EditViewModel.CurrentItem != null)
            {
                var item = new FavoriteItem();
                item.Url = EditViewModel.CurrentItem.Url;
                item.Name = EditViewModel.CurrentItem.Name;
                if (IsAddFavorite)
                {
                    if (Settings.Properties.Contains(key))
                        Settings.Properties[key] = item;
                    else
                        Settings.Properties.Add(key, item);
                    FavoriteButtonIconPath = Settings.ResourcePath + "white.png";
                    IsAddFavorite = false;
                }
                else
                {
                    if (Settings.Properties.Contains(key))
                    {
                        item = (FavoriteItem)Settings.Properties[key];
                        EditViewModel.CurrentItem.Name = item.Name;
                        EditViewModel.CurrentItem.Url = item.Url;
                    }
                }
                if (FindTreeNode(item.Url))
                {
                    HasPlayed = false;
                    Play(null);
                }
            }
        }

        private bool IsFavorite(string key)
        {
            return Settings.Properties.Contains(key) && !IsAddFavorite || 
                EditViewModel.CurrentItem != null && IsAddFavorite;
        }
    }
}
