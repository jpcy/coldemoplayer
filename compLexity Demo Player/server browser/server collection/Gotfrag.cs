// Comment me out to test .NET framework 3.0 compilation.
#define USE_TIMEZONEINFO

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Collections;
using System.Diagnostics;
using HtmlAgilityPack;
using compLexity_Demo_Player;

namespace Server_Browser
{
    public class Gotfrag : ServerCollection<GotfragServer>
    {
        private Thread updateThread;
        private GotfragServer tempServer;
        private Int32 teamCount = 0; // hack
        private Hashtable dayFullNames;

        public Gotfrag(IMainWindow mainWindowInterface)
        {
            this.mainWindowInterface = mainWindowInterface;

            // gotfrag seems to use annoying abbreviations...
            dayFullNames = new Hashtable(7);
            dayFullNames.Add("Mon", DayOfWeek.Monday.ToString());
            dayFullNames.Add("Tue", DayOfWeek.Tuesday.ToString()); // verified
            dayFullNames.Add("Wed", DayOfWeek.Wednesday.ToString()); // verified
            dayFullNames.Add("Thu", DayOfWeek.Thursday.ToString()); // verified
            dayFullNames.Add("Fri", DayOfWeek.Friday.ToString()); // verified
            dayFullNames.Add("Sat", DayOfWeek.Saturday.ToString());
            dayFullNames.Add("Sun", DayOfWeek.Sunday.ToString());
        }

        public override void RefreshAll()
        {
            if (updateThread != null && !updateThread.IsAlive)
            {
                base.RefreshAll();
            }
        }

        public void Update(String scoreboardUrl)
        {
            // kill update and refresh threads
            AbortUpdateThread();

            foreach (GotfragServer gs in servers)
            {
                if (gs.Thread != null)
                {
                    Common.AbortThread(gs.Thread);
                }
            }

            // start update thread
            servers.Clear();
            tempServer = new GotfragServer(mainWindowInterface);
            updateThread = new Thread(new ParameterizedThreadStart(UpdateThread));
            updateThread.Start(scoreboardUrl);
        }

        private void UpdateThread(Object scoreboardUrl)
        {
            try
            {
                UpdateThreadWorker((String)scoreboardUrl);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                mainWindowInterface.SetGotfragStatus(String.Format("Error updating server list. {0}", ex.Message));
            }
        }

        private void UpdateThreadWorker(String scoreboardUrl)
        {
            // download the gotfrag scoreboard page
            HtmlDocument doc = new HtmlDocument();

            using (WebClient client = new WebClient())
            {
                mainWindowInterface.SetGotfragStatus("Downloading server list...");
                using (Stream stream = client.OpenRead(scoreboardUrl))
                {
                    doc.Load(stream);
                }
            }

            mainWindowInterface.SetGotfragStatus("Parsing server list...");
            ParseGotfragHtmlNode(doc.DocumentNode);

            if (servers.Count > 0)
            {
                mainWindowInterface.SetGotfragStatus(null);
            }
            else
            {
                mainWindowInterface.SetGotfragStatus("No servers found.");
            }
        }

        public void AbortUpdateThread()
        {
            if (updateThread != null)
            {
                Common.AbortThread(updateThread);
            }

            //setStatus(null);
        }

        public override void AbortAllThreads()
        {
            AbortUpdateThread();
            base.AbortAllThreads();
        }

        #region HTML scraping
        private String HtmlNodeFindAttribute(HtmlNode node, String name)
        {
            foreach (HtmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name == name)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        private String HtmlNodeFindText(HtmlNode node)
        {
            // 2nd condition: <td class='first'> problem (first team)
            if (node.Name == "#text" && node.InnerText.Trim().Length > 0)
            {
                return node.InnerText;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                String result = HtmlNodeFindText(child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void ParseGotfragHtmlNode(HtmlNode node)
        {
            if (node.Name == "div")
            {
                String divClass = HtmlNodeFindAttribute(node, "class");

                // get event name
                if (divClass == "scoreboard-header")
                {
                    // add current server to list if we have an event name and an address
                    if (tempServer != null && tempServer.Address != null)
                    {
                        //UpdateServerInformation(new GotfragServerInformation(currentGotfragServerInformation));
                    }

                    // <h1><a>Event</a></h1>
                    tempServer.Event = HtmlNodeFindText(node);
                }
            }

            if (node.Name == "td")
            {
                String divClass = HtmlNodeFindAttribute(node, "class");

                if (divClass == "first")
                {
                    String team = HtmlNodeFindText(node).Trim();

                    if (tempServer.Teams == null || teamCount > 1)
                    {
                        tempServer.Teams = team;
                        teamCount = 1;
                    }
                    else
                    {
                        tempServer.Teams += " vs. " + team;
                        teamCount++;
                    }
                }
            }
            else if (node.Name == "a")
            {
                String address = HtmlNodeFindAttribute(node, "href");

                if (address.StartsWith("hlsw://"))
                {
                    tempServer.Address = address.Substring(7); // trim hlsw://

                    // add server to gotfrag list
                    mainWindowInterface.AddGotfragServer(tempServer);

                    String eventName = tempServer.Event; // preserve the current event name
                    tempServer = new GotfragServer(mainWindowInterface);
                    tempServer.Event = eventName;
                }
            }
            else if (node.Name == "tr")
            {
                if (HtmlNodeFindAttribute(node, "class") == "footnote")
                {
                    String text = HtmlNodeFindText(node);

                    if (text == "Start Time:" || text == "Status:")
                    {
                        String startTime = node.FirstChild.ChildNodes[1].InnerText.Trim
();

                        if (startTime != "Live Now!")
                        {
                            String newTime = (Config.Settings.ServerBrowserConvertTimeZone ? GotfragConvertTimeZone(startTime) : null);

                            if (newTime != null)
                            {
                                startTime = newTime;
                            }
                        }

                        tempServer.StartTime = startTime;
                    }
                }
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                ParseGotfragHtmlNode(child);
            }
        }

        private String GotfragConvertTimeZone(String start)
        {
#if USE_TIMEZONEINFO
            // look for EST/EDT TimeZoneInfo object
            // kinda dumb initialising this everytime a timezone needs to be converted, but this is necessary to be backwards compatible with the .NET framework 3.0
            TimeZoneInfo est = Common.FirstOrDefault<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones(), tzi => tzi.Id == "Eastern Standard Time");

            if (est == null)
            {
                return null;
            }

            // get current date/time in EST/EDT
            DateTime nowEdt = TimeZoneInfo.ConvertTime(DateTime.Now, est);

            // convert start time string into a DateTime object
            Regex r = new Regex(@"(^\w+)\sat\s(\d+):(\d+)\s(\w+)\s(\w+)");
            Match m = r.Match(start);

            if (!m.Success)
            {
                return null;
            }

            String day = m.Groups[1].ToString();
            String hour = m.Groups[2].ToString();
            String minute = m.Groups[3].ToString();
            String meridiem = m.Groups[4].ToString();
            String timeZone = m.Groups[5].ToString();

            if (timeZone != "EST" && timeZone != "EDT")
            {
                return null;
            }

            Int32 startDayDelta = 0;

            if (day != "Today")
            {
                // convert abbreviated day names to full names
                String dayFull = (String)dayFullNames[day];

                if (dayFull != null)
                {
                    day = dayFull;
                }

                // find out how many days away the parsed day is from the current day
                startDayDelta = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, true) - nowEdt.DayOfWeek;

                if (startDayDelta < 0) // wrap
                {
                    startDayDelta += 7;
                }
            }

            // FIXME: If PM and hour is 12, add an extra day???
            DateTime startTimeEdt = new DateTime(nowEdt.Year, nowEdt.Month, nowEdt.Day, Int32.Parse(hour) + (meridiem == "PM" && Int32.Parse(hour) != 12 ? 12 : 0), Int32.Parse(minute), 0);
            startTimeEdt.AddDays((Double)startDayDelta);

            // convert EDT start time to local timezone
            DateTime startTimeLocal = TimeZoneInfo.ConvertTime(startTimeEdt, est, TimeZoneInfo.Local);

            // format local start time string
            return String.Format("{0} at {1}", (startTimeLocal.DayOfWeek == DateTime.Now.DayOfWeek ? "Today" : Enum.GetName(typeof(DayOfWeek), startTimeLocal.DayOfWeek)), startTimeLocal.ToShortTimeString());
#else
            return start;
#endif
        }
        #endregion
    }
}
