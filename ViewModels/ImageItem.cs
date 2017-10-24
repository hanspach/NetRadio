
namespace NetRadio.ViewModels
{
    class ImageItem : ViewModelBase
    {
        private string imgpath;
        private bool isSelected;

        public string ImagePath
        {
            get { return imgpath; }
            set { SetProperty<string>(ref imgpath, value); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty<bool>(ref isSelected, value); }
        }
    }
}
