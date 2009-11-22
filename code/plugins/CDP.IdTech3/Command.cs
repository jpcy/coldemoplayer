using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.IdTech3
{
    public abstract class Command
    {
        public abstract CommandIds Id { get; }
        public abstract string Name { get; }
        public abstract bool IsSubCommand { get; }
        public abstract bool ContainsSubCommands { get; }
        public virtual bool HasFooter
        {
            get { return false; }
        }

        protected Demo demo;
        public Demo Demo
        {
            set { demo = value; }
        }

        public abstract void Read(BitReader buffer);
        public abstract void Write(BitWriter buffer);
        public abstract void Log(StreamWriter log);

        public virtual void ReadFooter(BitReader buffer) { }
        public virtual void WriteFooter(BitWriter buffer) { }
        public virtual void LogFooter(StreamWriter log) { }
    }

    public enum CommandIds : byte
    {
        svc_bad,
        svc_nop, // not saved to demo files?
        svc_gamestate,
        svc_configstring, // subcommand of svc_gamestate
        svc_baseline, // subcommand of svc_gamestate
        svc_servercommand,
        svc_download, // not saved to demo files?
        svc_snapshot,
        svc_eof
    }
}
