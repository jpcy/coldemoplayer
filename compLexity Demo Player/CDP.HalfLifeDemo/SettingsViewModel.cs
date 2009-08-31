using System;

namespace CDP.HalfLifeDemo
{
    public class SettingsViewModel : Core.ViewModelBase
    {
        protected readonly Core.Config config;

        public bool Playdemo
        {
            get 
            {
                return ((Handler.PlaybackMethods)config.DemoSettings["HlPlaybackMethod"] == Handler.PlaybackMethods.Playdemo);
            }
            set
            {
                config.DemoSettings["HlPlaybackMethod"] = Handler.PlaybackMethods.Playdemo;
            }
        }

        public bool Viewdemo
        {
            get
            {
                return ((Handler.PlaybackMethods)config.DemoSettings["HlPlaybackMethod"] == Handler.PlaybackMethods.Viewdemo);
            }
            set
            {
                config.DemoSettings["HlPlaybackMethod"] = Handler.PlaybackMethods.Viewdemo;
            }
        }

        public bool StartListenServer
        {
            get { return (bool)config.DemoSettings["HlStartListenServer"]; }
            set { config.DemoSettings["HlStartListenServer"] = value; }
        }

        public bool RemoveShowscores
        {
            get { return (bool)config.DemoSettings["HlRemoveShowscores"]; }
            set { config.DemoSettings["HlRemoveShowscores"] = value; }
        }

        public SettingsViewModel()
            : this(Core.Config.Instance)
        {
        }

        public SettingsViewModel(Core.Config config)
        {
            this.config = config;
        }

        public override void Initialise()
        {
        }

        public override void Initialise(object parameter)
        {
        }
    }
}
