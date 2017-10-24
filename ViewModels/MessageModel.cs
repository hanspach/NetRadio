using System.Windows;

namespace NetRadio.ViewModels
{
    public enum Status
    {
        None,
        Init,
        Error,
        Paused,
        Playing,
    }

    public class MessageModel 
    {
        public string Text { get; set; }

        public string StatusColorString { get; set; }
       
        public MessageModel(string msg, Status status=Status.None)
        {
            Text = msg;
            switch (status)
            {
                case Status.None: StatusColorString = SystemColors.MenuBrush.Color.ToString(); break;
                case Status.Init: StatusColorString = "Yellow"; break;
                case Status.Error: StatusColorString = "Red"; break;
                case Status.Paused: StatusColorString = "Blue"; break;
                case Status.Playing: StatusColorString = "Green"; break;
            }
        }
    }
}
