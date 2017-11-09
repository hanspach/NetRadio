using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using System.ComponentModel;
using System.Collections.Generic;

namespace NetRadio.ViewModels
{
    class StationArgs
    {
        public string Filename { get; private set; }
        public string Country { get; private set; }
        public string Category { get; private set; }

        public StationArgs(string filename, string country, string category)
        {
            if (country == "All countries")
                Country = string.Empty;
            else
                Country = country;
            if (category == "All categories")
                Category = string.Empty;
            else
                Category = category;
        }

        public StationArgs(string filename) : this(filename, "", "")
        {
        }
    }

    class EditViewModel : ViewModelBase, IDropTarget, IDisposable
    {
        private MainWindowViewModel mainViewModel;
        private bool isNewEntryPerformed;
        private bool isAddEntryPerformed;
        private BackgroundWorker worker = new BackgroundWorker();
        private BackgroundWorker countryworker = new BackgroundWorker();
        private BackgroundWorker categoryworker = new BackgroundWorker();

        public ObservableCollection<ImageItem> ImagePathes { get; private set; }
        
        public ICommand NewEntryCommand { get; private set; }
        public ICommand AddEntryCommand { get; private set; }
        public ICommand DeleteEntryCommand { get; private set; }

        private ObservableCollection<ProgramProps> jsonProgramList;
        public ObservableCollection<ProgramProps> JsonProgramList
        {
            get
            {
                if (jsonProgramList == null)
                {
                    if(!worker.IsBusy)
                        worker.RunWorkerAsync(new StationArgs("data.json"));
                    
                }
                return jsonProgramList;
            }
            set { SetProperty<ObservableCollection<ProgramProps>>(ref jsonProgramList, value); }
        }

        private Item currentItem;
        public Item CurrentItem
        {
            get { return currentItem; }
            set
            {
                if (value != null && currentItem != value)
                {
                    currentItem = value;
                    ProgramName = currentItem.Name;
                    ProgramUrl = currentItem.Url;
                    currentItem.IsSelected = true; 
                    OnPropertyChanged("CurrentItem");
                }
            }
        }

        private ProgramProps currentProgramProps;
        public ProgramProps CurrentProgramProps {
            get { return currentProgramProps; }
            set {
                SetProperty<ProgramProps>(ref currentProgramProps, value);
                //CurrentItem = new Program(currentProgramProps.Name,currentProgramProps.CurrentStream.Url,"");  // ????????
            }
        }

        private ImageItem currentImage;
        public ImageItem CurrentImage
        {
            get { return currentImage; }
            set
            {
                if (value != null && currentImage != value)
                {
                    currentImage = value;
                    if (!isNewEntryPerformed)
                    {
                        if (CurrentItem != null)
                        {
                            CurrentItem.ImagePath = currentImage.ImagePath;
                            if (isAddEntryPerformed)
                            {
                                Settings.IsDirty = true;
                                isAddEntryPerformed = false;
                            }
                        }
                    }
                    OnPropertyChanged("CurrentImage");
                }
            }
        }

        private string programName;
        public string ProgramName
        {
            get { return programName; }
            set
            {
                programName = value;
                if (CurrentItem != null && !isNewEntryPerformed)
                {
                    CurrentItem.Name = programName;
                    if (isAddEntryPerformed)
                    {
                        Settings.IsDirty = true;
                        isAddEntryPerformed = false;
                    }
                }
                OnPropertyChanged("ProgramName");
            }
        }

        private bool isNameIsEmpty;
        public bool IsNameIsEmpty {
            get { return isNameIsEmpty; }
            set { isNameIsEmpty = value; OnPropertyChanged("IsNameIsEmpty"); }
        }

        private double nameFieldThickness;
        public double NameFieldThickness
        {
            get { return nameFieldThickness; }
            set { SetProperty<double>(ref nameFieldThickness, value); }
        }

        private string programUrl;
        public string ProgramUrl
        {
            get { return programUrl; }
            set
            {
                programUrl = value;
                if (CurrentItem != null && !isNewEntryPerformed)
                {
                    CurrentItem.Url = programUrl;
                    if (isAddEntryPerformed)
                    {
                        Settings.IsDirty = true;
                        isAddEntryPerformed = false;
                    }
                }
                OnPropertyChanged("ProgramUrl");
            }
        }

        private double urlFieldThickness;
        public double UrlFieldThickness
        {
            get { return urlFieldThickness; }
            set
            {
                if (urlFieldThickness != value)
                {
                    urlFieldThickness = value;
                    OnPropertyChanged("UrlFieldThickness");
                }
            }
        }

        private ObservableCollection<string> countries;
        public ObservableCollection<string> Countries
        {
            get {
                if(countries == null)
                {
                    countryworker.DoWork += new DoWorkEventHandler(delegate (object sender, DoWorkEventArgs args)
                    {
                        string filename = args.Argument as string;
                        args.Result = JsonHelper.GetExistingStates(filename);
                    });
                    
                    countryworker.RunWorkerAsync("data.json");
                    countryworker.RunWorkerCompleted += ((s, args) => { Countries = new 
                        ObservableCollection<string>(args.Result as List<string>);
                    });
                }
                return countries; }
            set {
                countries = value;
                OnPropertyChanged("Countries");
                if (countryworker != null)
                {
                    countryworker.Dispose();
                    countryworker = null;
                    SelectedCountry = countries[0];
                }
            }
        }
        private string selectedCountry;
        public string SelectedCountry
        {
            get { return selectedCountry; }
            set
            {
                if (selectedCountry != value)
                {
                    SetProperty<string>(ref selectedCountry, value);
                    if (!worker.IsBusy)
                        worker.RunWorkerAsync(new StationArgs("data.json", SelectedCountry, SelectedCategory));
                }
            }
        }

        private ObservableCollection<string> categories;
        public ObservableCollection<string> Categories
        {
            get
            {
                if(categories == null)
                {
                    categoryworker.DoWork += new DoWorkEventHandler(delegate (object sender, DoWorkEventArgs args)
                    {
                        string filename = args.Argument as string;
                        args.Result = JsonHelper.GetExistingCategories(filename);
                    });

                    categoryworker.RunWorkerAsync("data.json");
                    categoryworker.RunWorkerCompleted += ((s, args) => {
                        Categories = new ObservableCollection<string>(args.Result as List<string>);
                    });
                }
                return categories;
            }
            set
            {
                categories = value;
                OnPropertyChanged("Categories");
                if (categoryworker != null)
                {
                    categoryworker.Dispose();
                    categoryworker = null;
                    SelectedCategory = categories[0];
                }
            }
        }
        private string selectedCategory;
        public string SelectedCategory
        {
            get { return selectedCategory; }
            set
            { 
                if(selectedCountry != value)
                {
                    SetProperty<string>(ref selectedCategory, value);
                    if (!worker.IsBusy)
                        worker.RunWorkerAsync(new StationArgs("data.json", SelectedCountry, SelectedCategory));
                }
            }
        }

        public EditViewModel(MainWindowViewModel mvm) : base()
        {
            mainViewModel = mvm;
            ImagePathes = new ObservableCollection<ImageItem>();
            DirectoryInfo di = new DirectoryInfo(Settings.ResourcePath);
            FileInfo[] fis = di.GetFiles("*.png");
            foreach (FileInfo fi in fis)
            {
                ImagePathes.Add(new ImageItem { ImagePath = fi.FullName });
            }
            NewEntryCommand = new ActionCommand(s => { NewItem(s); }, s => { return CurrentItem != null; });
            AddEntryCommand = new ActionCommand(s => { AddItem(s); }, s => { return isNewEntryPerformed; });
            DeleteEntryCommand = new ActionCommand(s => { DeleteItem(s); }, s => {return CurrentItem != null; });
            worker.DoWork += new DoWorkEventHandler(delegate (object sender, DoWorkEventArgs args)
            {
                var a = args.Argument as StationArgs;
                args.Result = JsonHelper.GetDownloadedStations("data.json",a.Country,a.Category);
            });
            worker.RunWorkerCompleted += ((s, args) =>
            {
                JsonProgramList = new
                    ObservableCollection<ProgramProps>(args.Result as List<ProgramProps>);
            });
            ResetProperties();
        }

        public void DragOver(IDropInfo info)
        {
            var properties = info.Data as ProgramProps;
            if (properties != null && isNewEntryPerformed)
            {
                info.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo info)
        {
            var properties = info.Data as ProgramProps;
            if (properties != null)
            {
                ProgramName = properties.Name;
                ProgramUrl =  CurrentProgramProps.CurrentStream.Url;
            }
        }

        public void ResetProperties()
        {
            isNewEntryPerformed = false;
            IsNameIsEmpty = false;
            NameFieldThickness = 1;
            UrlFieldThickness = 1;
        }

        private void NewItem(object o)
        {
            string path = Settings.ResourcePath;

            if (CurrentItem is Program)
                path += "audio.png";
            else if (CurrentItem is Station)
                if (((Station)CurrentItem).Programs.Count == 0)
                    path += "audio.png";
                else
                    path += "station.png";
            else
                path += "home.png";
            isNewEntryPerformed = true;
            if (currentImage == null)
                currentImage = new ImageItem();
            CurrentImage.ImagePath = path;
            CurrentImage.IsSelected = true;
            ProgramName = string.Empty;
            ProgramUrl  = string.Empty;
        }

        private void AddItem(object o)
        {
            Item newItem = null;
            IsNameIsEmpty = ProgramName.Length == 0;
            if (IsNameIsEmpty)
            {
                mainViewModel.Message = new MessageModel("Field 'Name' musn't be empty!", Status.Error);
                NameFieldThickness = 3;
                return;
            }
            else
            {
                NameFieldThickness = 1;
                mainViewModel.Message = new MessageModel(string.Empty);
            }
            if (CurrentItem is Program && !ProgramUrl.Trim().StartsWith("http://")) // hier URL-Prüfung!!!
            {
                UrlFieldThickness = 3;
                mainViewModel.Message = new MessageModel("Invalid URL", Status.Error);
                return;
            }
            else
            {
                UrlFieldThickness = 1;
                mainViewModel.Message = new MessageModel(string.Empty);
            }
            Item parent = CurrentItem.Parent; 
            if (CurrentItem is Program)
            {
                Station station = (Station)parent;
                newItem = new Program(ProgramName, ProgramUrl, CurrentImage.ImagePath); 
                newItem.Parent = station;
                int idx = station.Programs.IndexOf((Program)CurrentItem);
                if (idx != -1 && idx < station.Programs.Count - 1)
                    station.Programs.Insert(idx + 1, (Program)newItem);
                else
                    station.Programs.Add((Program)newItem);
            }
            else if (CurrentItem is Station)
            {  
                Station station = CurrentItem as Station;
                if (station.Programs.Count > 0)
                {
                    Country country = (Country)parent;
                    newItem = new Station(ProgramName, ProgramUrl, CurrentImage.ImagePath);
                    newItem.Parent = country;
                    int idx = country.Stations.IndexOf((Station)CurrentItem);
                    if (idx != -1 && idx < country.Stations.Count - 1)
                        country.Stations.Insert(idx + 1, (Station)newItem);
                    else
                        country.Stations.Add((Station)newItem);
                }
                else
                {
                    station = CurrentItem as Station;
                    newItem = new Program(ProgramName, ProgramUrl, CurrentImage.ImagePath);
                    newItem.Parent = station;
                    station.Programs.Add((Program)newItem);
                }
            }
            else
            {
                Country country = CurrentItem as Country;
                if (country.Stations.Count > 0)
                {
                    RootItem ri = (RootItem)parent;
                    Country c = new Country(ProgramName, ProgramUrl, CurrentImage.ImagePath);
                    c.Parent = ri;
                    int idx = ri.Countries.IndexOf((Country)CurrentItem);
                    if (idx != -1 && idx < ri.Countries.Count - 1)
                        ri.Countries.Insert(idx + 1, (Country)newItem);
                    else
                        ri.Countries.Add(c);
                }
                else
                {
                    Station station = new Station(ProgramName, ProgramUrl, CurrentImage.ImagePath);
                    station.Parent = CurrentItem;
                    country.Stations.Add(station);
                }
            }
            Settings.IsDirty = true;
            isNewEntryPerformed = false;
            isAddEntryPerformed = true;
            CurrentItem = newItem;
        }

        private void DeleteItem(object o)
        {
            Item parent = CurrentItem.Parent;
            int idx = -1;
            if (CurrentItem is Program)
            {
                Station station = (Station)parent;
                idx = station.Programs.IndexOf((Program)CurrentItem);
                station.Programs.Remove((Program)CurrentItem);
                if (station.Programs.Count > 0)
                {
                    idx = Math.Min(idx, station.Programs.Count - 1);
                    CurrentItem = station.Programs[idx];
                }
                else
                    CurrentItem = parent;
            }
            else if(CurrentItem is Station)
            {
                Country country = (Country)parent;
                idx = country.Stations.IndexOf((Station)CurrentItem);
                country.Stations.Remove((Station)CurrentItem);
                if (country.Stations.Count > 0)
                {
                    idx = Math.Min(idx, country.Stations.Count - 1);
                    CurrentItem = country.Stations[idx];
                }
                else
                    CurrentItem = parent;
            }
            else
            {
                RootItem ri = (RootItem)parent;
                idx = ri.Countries.IndexOf((Country)CurrentItem);
                ri.Countries.Remove((Country)CurrentItem);
                if (ri.Countries.Count > 0)
                {
                    idx = Math.Min(idx, ri.Countries.Count - 1);
                    CurrentItem = ri.Countries[idx];
                }
                else
                    CurrentItem = parent;
            }
            Settings.IsDirty = true;
        }

        public void Dispose() => worker.Dispose();
    }
}
