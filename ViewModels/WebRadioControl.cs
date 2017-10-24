using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using ManagedBass;
using System.Collections.Generic;
using System.Windows.Interop;
using WPFSoundVisualizationLib;
using System.ComponentModel;

namespace NetRadio.ViewModels
{
    class MessageEventArgs : EventArgs
    {
        public MessageModel Message { get; private set; }

        public MessageEventArgs(string title, Status status = Status.Playing)
        {
            Message = new MessageModel(title, status);
        }
    }

    class MetadataEventArgs : EventArgs
    {
        public IList<MetadataModel> Metadata { get; set; }

        public MetadataEventArgs()
        {
            Metadata = new List<MetadataModel>();
        }
    }

    sealed class WebRadioControl : ViewModelBase,ISpectrumPlayer, IDisposable
    {
        #region Fields
        static readonly object Lock = new object();
        static WebRadioControl control;

        readonly Timer timer;
        int req;                                             // request number/counter
        int chan;                                            // stream handle
        #endregion

        bool _directConnect;
        public bool DirectConnection
        {
            get { return _directConnect; }
            set { SetProperty<bool>(ref _directConnect, value); }
        }
       
        string _proxy;
        public string Proxy
        {
            get { return _proxy; }
            set { SetProperty<string>(ref _proxy, value); }
        }
 
        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty<bool>(ref isPlaying, value); }
        }

        public event EventHandler<MessageEventArgs> OnMessageChanged;
        public event EventHandler<MetadataEventArgs> OnMetadataChanged;

        public static WebRadioControl Instance
        {
            get
            {
                if (control == null)
                    control = new WebRadioControl(new WindowInteropHelper(
                        Application.Current.MainWindow).Handle);
                return control;
            }
        }

        private WebRadioControl(IntPtr handle)
        {
            if (!Bass.Init(-1, 44100, 0, handle))
            {
                MessageBox.Show("Can't initialize device");
                Application.Current.Shutdown();
            }
            // enable playlist processing
            Bass.NetPlaylist = 1;

            // minimize automatic pre-buffering, so we can do it (and display it) instead
            Bass.NetPreBuffer = 0;
          //  Bass.Volume = 0.25;         
            timer = new Timer(50);
            timer.Elapsed += elapsedTime;
        }

        public void OpenRadioProgram(string url)
        {
            Bass.NetProxy = DirectConnection ? null : Proxy;
            Task.Factory.StartNew(() =>
            {
                int r;

                lock (Lock)     // make sure only 1 thread at a time can do the following
                {
                    r = ++req; // increment the request counter for this request
                }
                timer.Stop();          // stop prebuffer monitoring
                Bass.StreamFree(chan); // close old stream
                OnMessageChanged(this, new MessageEventArgs("Connecting...", Status.Init));

                var c = Bass.CreateStream(url, 0, BassFlags.StreamDownloadBlocks | BassFlags.StreamStatus
                    | BassFlags.AutoFree, StatusProc, new IntPtr(r));

                lock (Lock)
                {
                    if (r != req)
                    {
                        if (c != 0)     // there is a newer request, discard this stream
                            Bass.StreamFree(c);
                        return;
                    }
                    chan = c;                                   // this is now the current stream
                }

                if (chan == 0)
                {  
                    OnMessageChanged(this, new MessageEventArgs(            // failed to open
                        "Can't play the stream. - Please check the URL.", Status.Error));
                }
                else
                    timer.Start();                              // start prebuffer monitoring
            });
        }

        public void Pause()
        {
            IsPlaying = false; 
            Bass.ChannelPause(chan);
        }
        
        public void SetVolume(float val)
        {
            Bass.Volume = val;
        }

        public void CloseBass()
        {
            timer.Stop(); // stop prebuffer monitoring
            Bass.StreamFree(chan); // close old stream
            Bass.Free();
        }

        void elapsedTime(object sender, EventArgs e)
        {
            var progress = Bass.StreamGetFilePosition(chan, FileStreamPosition.Buffer)
                * 100 / Bass.StreamGetFilePosition(chan, FileStreamPosition.End);   // percentage of buffer filled

            if (progress > 75 || Bass.StreamGetFilePosition(chan, FileStreamPosition.Connected) == 0)
            {
                IsPlaying = true;
                timer.Stop(); // finished prebuffering, stop monitoring
                var icy = Bass.ChannelGetTags(chan, TagType.ICY);// get the broadcast name and URL
                if (icy == IntPtr.Zero)
                    icy = Bass.ChannelGetTags(chan, TagType.HTTP); // no ICY tags, try HTTP

                if (icy != IntPtr.Zero)
                {
                    var tags = Extensions.ExtractMultiStringAnsi(icy);
                    MetadataEventArgs eventArgs = new MetadataEventArgs();
                    foreach (var tag in tags)
                    {
                        string[] items = tag.Split(new char[] { ':', '=' });
                        if (items.Length > 1)
                        {
                            eventArgs.Metadata.Add(new MetadataModel { Name = items[0], Value = items[1] });
                        }
                    }
                    OnMetadataChanged?.Invoke(this, eventArgs);
                }

                // get the stream title and set sync for subsequent titles
                DoMeta();

                Bass.ChannelSetSync(chan, SyncFlags.MetadataReceived, 0, MetaSync); // Shoutcast
                Bass.ChannelSetSync(chan, SyncFlags.OggChange, 0, MetaSync); // Icecast/OGG

                // set sync for end of stream
                Bass.ChannelSetSync(chan, SyncFlags.End, 0, EndSync);

                // play it!
                Bass.ChannelPlay(chan);
            }
            else
            {
               OnMessageChanged(this, new MessageEventArgs(string.Format("Buffering... {0}%",progress), Status.Init));
            }
        }

        void StatusProc(IntPtr buffer, int length, IntPtr user)
        {
            if (buffer != IntPtr.Zero
                && length == 0
                && user.ToInt32() == req) // got HTTP/ICY tags, and this is still the current request

                OnMessageChanged(this, new MessageEventArgs(Marshal.PtrToStringAnsi(buffer))); // display status
        }

        void EndSync(int Handle, int Channel, int Data, IntPtr User)
        {
            /*Status = "Not Playing"*/;
        }

        void MetaSync(int Handle, int Channel, int Data, IntPtr User) => DoMeta();

        void DoMeta()
        {
            var meta = Bass.ChannelGetTags(chan, TagType.META);
            string data = string.Empty;
            int a, e;

            if (meta != IntPtr.Zero)
            {
                data = Marshal.PtrToStringAnsi(meta);   // got Shoutcast metadata
                a = data.IndexOf('\'');
                e = data.IndexOf("';");
                if (a != -1 && e != -1)
                {
                    data = data.Substring(a + 1, (e - a - 1));
                }
            }
            else
            {
                meta = Bass.ChannelGetTags(chan, TagType.OGG);
                if (meta != IntPtr.Zero)
                {
                    foreach (var tag in Extensions.ExtractMultiStringUtf8(meta)) // got Icecast/OGG tags
                    {
                        if (tag.StartsWith("TITLE="))
                            data += tag.Substring(6);
                        else if (tag.StartsWith("ARTIST="))
                            if(data.Length > 0 && tag.Length > 7)
                                data += " - " + tag.Substring(7);
                    }
                }
            }
            OnMessageChanged(this, new MessageEventArgs(data));
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            return (int)(frequency * (double)2048 / 44100)+1;    //Utils.FFTFrequency2Index(frequency, fftDataSize, sampleFrequency);
        }

        public bool GetFFTData(float[] fftDataBuffer)
        {
            return (Bass.ChannelGetData(chan, fftDataBuffer, -2147483645)) > 0;
        }

        public void Dispose() => timer.Dispose();
    }
}
