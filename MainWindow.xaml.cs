﻿using log4net;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;


namespace NetRadio
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            var model = new ViewModels.MainWindowViewModel();
            DataContext = model;
            if(!string.IsNullOrEmpty(ViewModels.Settings.LastVisitedUrl))
                model.FindTreeNode(ViewModels.Settings.LastVisitedUrl);
        }

        private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var model = DataContext as ViewModels.MainWindowViewModel;
            ViewModels.Item item = e.NewValue as ViewModels.Item;
            if (item != null)
            {
                item.IsSelected = true; 
                model.EditViewModel.CurrentItem = item;
                foreach (ViewModels.ImageItem i in model.EditViewModel.ImagePathes)
                {
                    if (item.ImagePath.Equals(i.ImagePath))
                    {
                        model.EditViewModel.CurrentImage = i;
                        i.IsSelected = true;
                        break;
                    }
                }
                model.EditViewModel.ResetProperties();
                model.Message = new ViewModels.MessageModel(string.Empty); // ????
               // model.PlayButtonIconPath = ViewModels.Settings.ResourcePath + "play.png"; //???? musi is playing on
               // model.HasPlayed = false;
                if (item is ViewModels.Program)
                    model.BrowserViewModel.Url = item.Parent.Url;
                else
                    model.BrowserViewModel.Url = item.Url;  // hier an Json-Data anpassen!

            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (ViewModels.Settings.IsDirty)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "The tree view has changed. Should a backup be performed?",
                        Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        ViewModels.CountryModel.Instance.WriteBookmarks(ViewModels.Settings.ResourcePath + "bookmarks.xml");
                    }
                }
                ViewModels.Settings.SaveSettings();
                ViewModels.WebRadioControl.Instance.CloseBass();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
