using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using CDP.Core;

namespace CDP.Gui
{
    internal interface IIpcChannel
    {
        /// <summary>
        /// Open a server IPC channel.
        /// </summary>
        void Open();

        /// <summary>
        /// Create a client IPC channel and transport a filename to the open server IPC channel.
        /// </summary>
        /// <param name="fileName">The filename of a demo to open.</param>
        void Transport(string fileName);
    }

    internal class IpcChannel : IIpcChannel
    {
        private readonly string portName = "coldemoplayerchannel";
        private readonly string uri = "coldemoplayerserver";

        public void Open()
        {
            IpcServerChannel channel = new IpcServerChannel(portName);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemotingInterface), uri, WellKnownObjectMode.SingleCall);
        }

        public void Transport(string fileName)
        {
            IpcClientChannel channel = new IpcClientChannel();
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownClientType(typeof(RemotingInterface), "ipc://" + portName + "/" + uri);
            RemotingInterface ri = new RemotingInterface();
            ri.OpenFile(fileName);
        }
    }

    internal class RemotingInterface : MarshalByRefObject
    {
        public void OpenFile(string fileName)
        {
            INavigationService navigationService = ObjectCreator.Get<INavigationService>();

            if (navigationService.CurrentPageTitle != "Main")
            {
                // Don't open a demo with IPC if not on the main page.
                return;
            }

            IMediator mediator = ObjectCreator.Get<IMediator>();
            IFileSystem fileSystem = ObjectCreator.Get<IFileSystem>();
            mediator.Notify<SetFolderMessageParameters>(Messages.SetFolder, new SetFolderMessageParameters(fileSystem.GetDirectoryName(fileName), fileSystem.GetFileName(fileName)));
        }
    }
}
