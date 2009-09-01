using System;

namespace CDP.HalfLifeDemo
{
    public class SettingsViewModel : Core.ViewModelBase
    {
        protected readonly Core.Settings config;

        public bool Playdemo
        {
            get 
            {
                return ((Handler.PlaybackMethods)config.Demo["HlPlaybackMethod"] == Handler.PlaybackMethods.Playdemo);
            }
            set
            {
                config.Demo["HlPlaybackMethod"] = Handler.PlaybackMethods.Playdemo;
            }
        }

        public bool Viewdemo
        {
            get
            {
                return ((Handler.PlaybackMethods)config.Demo["HlPlaybackMethod"] == Handler.PlaybackMethods.Viewdemo);
            }
            set
            {
                config.Demo["HlPlaybackMethod"] = Handler.PlaybackMethods.Viewdemo;
            }
        }

        public bool StartListenServer
        {
            get { return (bool)config.Demo["HlStartListenServer"]; }
            set { config.Demo["HlStartListenServer"] = value; }
        }

        public bool RemoveShowscores
        {
            get { return (bool)config.Demo["HlRemoveShowscores"]; }
            set { config.Demo["HlRemoveShowscores"] = value; }
        }

        public SettingsViewModel()
            : this(Core.Settings.Instance)
        {
        }

        public SettingsViewModel(Core.Settings config)
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
