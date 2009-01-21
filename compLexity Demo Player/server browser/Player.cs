using System;
using System.Collections.Generic;
using System.Text;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public class Player : NotifyPropertyChangedItem
    {
        #region Properties
        public String Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public Int32 Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
                OnPropertyChanged("Score");
            }
        }

        public String Time
        {
            get
            {
                return time;
            }

            set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }
        #endregion

        private String name;
        private Int32 score;
        private String time;
    }
}
