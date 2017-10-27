using log4net;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;

namespace NetRadio.ViewModels
{
    class MainWindowViewModel : ViewModelBase, IDropTarget
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);

        private bool isSilence;
        
        public ICommand EditViewCommand { get; private set; }
        public ICommand BrowserViewCommand { get; private set; }
        public ICommand VisualViewCommand { get; private set; }
        public ICommand PlayProgramCommand { get; private set; }
        public ICommand MuteCommand { get; private set; }
        public ICommand Favorite1Command { get; private set; }
        public ICommand Favorite2Command { get; private set; }
        public ICommand Favorite3Command { get; private set; }
        public ICommand AddFavoriteCommand { get; private set; }

        public CountryModel CountryModel { get; private set; }
        public EditViewModel EditViewModel { get; private set; }
        public BrowserViewModel BrowserViewModel { get; private set; }
        public VisualViewModel VisualViewModel { get; private set; }

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

        public bool HasPlayed { get; set; }

        private string playButtonIconPath;
        public string PlayButtonIconPath
        {
            get { return playButtonIconPath; }
            set { SetProperty<string>(ref playButtonIconPath, value); }
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
                Settings.Volume = volume.ToString();
                WebRadioControl.Instance.SetVolume(volume / 100);
            }
        }
        
        public bool IsAddFavorite { get; set; }
        
        public MainWindowViewModel()
        {
            EditViewModel = new EditViewModel(this);
            BrowserViewModel = new BrowserViewModel();
            VisualViewModel = new VisualViewModel();
            CurrentViewModel = EditViewModel;
            CountryModel = CountryModel.Instance;
            EditViewCommand = new ActionCommand(delegate { CurrentViewModel = EditViewModel; });
            BrowserViewCommand = new ActionCommand(delegate { CurrentViewModel = BrowserViewModel; });
            VisualViewCommand = new ActionCommand(delegate { CurrentViewModel = VisualViewModel; });
            PlayProgramCommand = new ActionCommand(Play, CanPlay);
            MuteCommand = new ActionCommand(Silence, (o) => { return true; });
            WebRadioControl.Instance.OnMessageChanged += ((s, args) => {
                if (args.Message != null)
                {
                    Message = args.Message;
                    if (Message.StatusColorString == "Green")    //program started
                        Settings.LastVisitedUrl = EditViewModel.CurrentItem.Url;
                }
            });
            PlayButtonIconPath = Settings.ResourcePath + "play.png";
            FavoriteButtonIconPath = Settings.ResourcePath + "white.png";
            if (string.IsNullOrEmpty(Settings.Volume))
                Volume = 10;
            else
                Volume = float.Parse(Settings.Volume);

            Favorite1Command = new ActionCommand((o) => { Settings.Favorite1 = Favorite(Settings.Favorite1);}, (o) =>  { return IsFavorite(Settings.Favorite1); });
            Favorite2Command = new ActionCommand((o) => { Settings.Favorite2 = Favorite(Settings.Favorite2); }, (o) => { return IsFavorite(Settings.Favorite2); });
            Favorite3Command = new ActionCommand((o) => { Settings.Favorite3 = Favorite(Settings.Favorite3); }, (o) => { return IsFavorite(Settings.Favorite3);});
            AddFavoriteCommand = new ActionCommand((o) => {
                IsAddFavorite = true;
                FavoriteButtonIconPath = Settings.ResourcePath + "Redmark.png";
            }, (o) => { return EditViewModel.CurrentItem != null; });
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
                }
            }
            Settings.IsDirty = true;
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

        private bool CanPlay(object o)
        {
            if (EditViewModel.CurrentItem != null)
                return EditViewModel.CurrentItem is Program;
            return false;
        }

        private void Silence(object o)
        {
            if (isSilence)
                WebRadioControl.Instance.SetVolume(volume / 100);
            else
                WebRadioControl.Instance.SetVolume(0);
            isSilence = !isSilence;
        }

        private string Favorite(string member)
        {
            string url = member;
            if (IsAddFavorite)
            {
                url = EditViewModel.CurrentItem.Url;
                FavoriteButtonIconPath = Settings.ResourcePath + "white.png";
                IsAddFavorite = false;
            }
            if (FindTreeNode(url))
                Play(null);
            return url;
        }

        private bool IsFavorite(string member)
        {
            return !string.IsNullOrEmpty(member)
               && !IsAddFavorite || EditViewModel.CurrentItem != null && IsAddFavorite;
        }
    }
}
