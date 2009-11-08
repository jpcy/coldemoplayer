using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Controls;
using CDP.IdTech3;
using CDP.IdTech3.Commands;

namespace CDP.Quake3Arena
{
    public class Handler : IdTech3.Handler
    {
        public override string FullName
        {
            get { return "Quake III Arena"; }
        }

        public override string Name
        {
            get { return "Q3A"; }
        }

        /// <summary>
        /// Game names that are valid for this plugin - i.e. they are compatiable enough with Q3A that they don't require any special treatment to convert, analyse or play (aside from resources).
        /// </summary>
        private readonly string[] validGameNames =
        {
            // TODO: read these from a config file.
            "baseq3",
            "cpma", // TODO: probably requires its own plugin.
            "excessive",
            "instaunlagged",
            "ost" // TODO: probably requires its own plugin.
        };

        public override bool IsValidDemo(Core.FastFileStreamBase stream, string fileExtension)
        {
            if (!base.IsValidDemo(stream, fileExtension))
            {
                return false;
            }

            string gameName = ReadGameName(stream, fileExtension);

            if (gameName == null)
            {
                return false;
            }

            return validGameNames.Contains(gameName);
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override UserControl CreateAnalysisView()
        {
            return new IdTech3.Analysis.View();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            return new IdTech3.Analysis.ViewModel((IdTech3.Demo)demo);
        }
    }
}
