using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace compLexity_Demo_Player
{
    public interface IUpdateCheck
    {
        void UpdateCheckComplete();
    }

    public class UpdateCheck
    {
        public class Version
        {
            public Int32 Major { get; set; }
            public Int32 Minor { get; set; }
            public Int32 Update { get; set; }
        }

        private readonly String versionFileName = "version.xml";

        private IUpdateCheck updateCheckInterface;
        private Thread thread = null;

        // results
        private Boolean available = false;
        private Version version = null;
        private String errorMessage = null;

        public Boolean Available
        {
            get
            {
                return available;
            }
        }

        public String AvailableVersion
        {
            get
            {
                return String.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Update);
            }
        }

        public String ErrorMessage
        {
            get
            {
                return errorMessage;
            }
        }

        public UpdateCheck(IUpdateCheck updateCheckInterface)
        {
            this.updateCheckInterface = updateCheckInterface;

            thread = new Thread(new ThreadStart(CheckForUpdateThreadWorker));
            thread.Start();
        }

        public void Cancel()
        {
            if (thread != null)
            {
                Common.AbortThread(thread);
            }
        }

        private void CheckForUpdateThreadWorker()
        {
            try
            {
                WebRequest request = WebRequest.Create(Config.Settings.UpdateUrl + versionFileName);

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Version));
                        version = (Version)serializer.Deserialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                errorMessage = ex.Message;
                updateCheckInterface.UpdateCheckComplete();
                return;
            }

            // see if found version number is greater than current version
            Int32 deltaMajor = version.Major - Config.Settings.ProgramVersionMajor;
            Int32 deltaMinor = version.Minor - Config.Settings.ProgramVersionMinor;
            Int32 deltaUpdate = version.Update - Config.Settings.ProgramVersionUpdate;

            if (deltaMajor > 0 || (deltaMajor == 0 && deltaMinor > 0) || (deltaMajor == 0 && deltaMinor == 0 && deltaUpdate > 0))
            {
                available = true;
            }

            updateCheckInterface.UpdateCheckComplete();
        }
    }
}
