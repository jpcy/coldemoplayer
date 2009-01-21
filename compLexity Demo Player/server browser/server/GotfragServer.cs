using System;
using System.Collections.Generic;
using System.Text;

namespace Server_Browser
{
    public class GotfragServer : Server
    {
        #region Properties
        public String Event
        {
            get
            {
                return eventName;
            }

            set
            {
                eventName = value;
                OnPropertyChanged("Event");
            }
        }

        public String Teams
        {
            get
            {
                return teams;
            }

            set
            {
                teams = value;
                OnPropertyChanged("Teams");
            }
        }

        public String StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                startTime = value;
                OnPropertyChanged("StartTime");
            }
        }
        #endregion

        private String eventName;
        private String teams;
        private String startTime;

        public GotfragServer(IMainWindow mainWindowInterface)
        {
            this.mainWindowInterface = mainWindowInterface;
        }
    }
}
