using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using CDP.Core.Extensions;

namespace CDP.Source
{
    public class Demo : Core.Demo
    {
        public class MessageException : Exception
        {
            private readonly string message;

            public override string Message
            {
                get { return message; }
            }

            public MessageException(IEnumerable<Message> messageHistory, Exception innerException)
                : base(null, innerException)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Last frame messages:\n");

                foreach (Message m in messageHistory)
                {
                    // FIXME: output correct ID based on network protocol.
                    sb.AppendFormat("({0}) {1} [{2}]\n", m.Offset, m.Name, (byte)m.Id);
                }

                message = sb.ToString();
            }
        }

        private class FrameCallback
        {
            public FrameIds Id { get; set; }
            public object Delegate { get; set; }

            public void Fire(Frame frame)
            {
                MethodInfo methodInfo = Delegate.GetType().GetMethod("Invoke");
                methodInfo.Invoke(Delegate, new object[] { frame });
            }
        }

        protected Handler handler;

        public override Core.DemoHandler Handler
        {
            get { return handler; }
            set { handler = (Handler)value; }
        }

        public override string GameName
        {
            get { throw new NotImplementedException(); }
        }

        public override string MapName { get; protected set; }
        public override string Perspective { get; protected set; }
        public override TimeSpan Duration { get; protected set; }
        public override IList<Core.Demo.Detail> Details { get; protected set; }
        public override ArrayList Players { get; protected set; }

        public override string[] IconFileNames
        {
            get { throw new NotImplementedException(); }
        }

        public override string MapImagesRelativePath
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanPlay
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanAnalyse
        {
            get { throw new NotImplementedException(); }
        }

        public int DemoProtocol { get; private set; }
        public int NetworkProtocol { get; private set; }

        private readonly List<FrameCallback> frameCallbacks;
        private readonly Core.CyclicQueue<Message> messageHistory;

        /// <summary>The offset of the last message block that was read.</summary>
        /// <remarks>Used to calculate a message's offset, which in turn is used for diagnostics and error messages.</remarks>
        private long currentMessageBlockOffset;

        public Demo()
        {
            frameCallbacks = new List<FrameCallback>();
            messageHistory = new Core.CyclicQueue<Message>(16);
        }

        #region Operations
        public override void Load()
        {
            throw new NotImplementedException();
        }

        public override void Read()
        {
            throw new NotImplementedException();
        }

        public override void Write(string destinationFileName)
        {
            throw new NotImplementedException();
        }

        public void RunDiagnostic(string logFileName, IEnumerable<byte> framesToLog, IEnumerable<byte> messagesToLog)
        {
            try
            {
                using (Core.FastFileStream stream = new Core.FastFileStream(FileName, Core.FastFileAccess.Read))
                using (StreamWriter log = new StreamWriter(logFileName))
                {
                    try
                    {
                        Header header = new Header();
                        header.Read(stream.ReadBytes(Header.SizeInBytes));
                        LoadHeader(header);
                        header.Log(log);
                        log.WriteLine();

                        while (true)
                        {
                            Frame frame = ReadFrame(stream, framesToLog, log);

                            if (frame.HasMessages)
                            {
                                Core.BitReader messageReader = null;

                                try
                                {
                                    byte[] messageBlock = ReadMessageBlock(stream);

                                    if (messageBlock != null)
                                    {
                                        log.WriteLine("Message block length: {0}", messageBlock.Length);
                                        messageReader = new Core.BitReader(messageBlock);
                                        ReadMessages(messageReader, messagesToLog, false, log);
                                    }
                                }
                                catch (MessageException ex)
                                {
                                    log.WriteLine("\n*** Error processing message block ***\n");
                                    log.WriteLine(ex.ToString());
                                    log.WriteLine();
                                    log.WriteLine("\n*** Re-processing message block without skipping ***\n");
                                    messageHistory.Clear();
                                    messageReader.SeekBytes(0, SeekOrigin.Begin);
                                    ReadMessages(messageReader, null, true, log);
                                    break;
                                }
                            }

                            if (frame.Id == FrameIds.Stop || stream.Position == stream.Length)
                            {
                                break;
                            }

                            if (IsOperationCancelled())
                            {
                                OnOperationCancelled();
                                return;
                            }

                            UpdateProgress(stream.Position, stream.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine(ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                OnOperationError(FileName, ex);
                return;
            }
            finally
            {
                messageHistory.Clear();
                frameCallbacks.Clear();
            }

            OnOperationComplete();
        }
        #endregion

        private void LoadHeader(Header header)
        {
            DemoProtocol = header.DemoProtocol;
            NetworkProtocol = header.NetworkProtocol;
            MapName = header.MapName;
            Perspective = (header.RecorderName == "SourceTV Demo" ? "SourceTV" : "POV");
            Duration = new TimeSpan(0, 0, (int)header.DurationInSeconds);
        }

        private Frame ReadFrameHeader(Core.FastFileStream stream)
        {
            // New frame, clear the message history.
            messageHistory.Clear();

            byte id = stream.ReadByte();
            Frame frame = handler.CreateFrame(id, NetworkProtocol);

            if (frame == null)
            {
                throw new ApplicationException(string.Format("Unknown frame type \"{0}\" at offset \"{1}\"", id, stream.Position - 1));
            }

            frame.Demo = this;
            frame.ReadHeader(stream);
            return frame;
        }

        private Frame ReadFrame(Core.FastFileStream stream, IEnumerable<byte> framesToNotSkip, StreamWriter log)
        {
            long frameOffset = stream.Position;
            Frame frame = ReadFrameHeader(stream);

            if (log != null)
            {
                log.Write("\nFrame ");
                
                if (NetworkProtocol >= 36)
                {
                    log.Write("{0} [{1}]", frame.Id_Protocol36, (byte)frame.Id_Protocol36);
                }
                else
                {
                    log.Write("{0} [{1}]", frame.Id, (byte)frame.Id);
                }

                log.WriteLine(". Uk1: {0}. Tick: {1}, Offset: {2}", frame.Unknown1, frame.Tick, frameOffset);
            }

            List<FrameCallback> frameCallbacks = FindFrameCallbacks(frame);
            bool logFrame = framesToNotSkip.Contains(NetworkProtocol >= 36 ? (byte)frame.Id_Protocol36 : (byte)frame.Id);

            if (frameCallbacks.Count == 0 && frame.CanSkip && !logFrame)
            {
                frame.Skip(stream);
            }
            else
            {
                frame.Read(stream);

                if (logFrame && log != null)
                {
                    log.WriteLine();
                    frame.Log(log);
                    log.WriteLine();
                }
            }

            foreach (FrameCallback frameCallback in frameCallbacks)
            {
                frameCallback.Fire(frame);
            }

            return frame;
        }

        private byte[] ReadMessageBlock(Core.FastFileStream stream)
        {
            uint length = stream.ReadUInt();
            currentMessageBlockOffset = stream.Position;
            byte[] data = null;

            if (length > 0)
            {
                data = stream.ReadBytes((int)length);
            }

            return data;
        }

        private byte ReadMessageId(Core.BitReader buffer)
        {
            // 37 and earlier are 5 bits.
            // 1015 is 6 bits.
            return (byte)buffer.ReadUBits(NetworkProtocol >= 1015 ? 6 : 5);
        }

        /// <summary>
        /// Iterate over a BitReader and read the messages it contains.
        /// </summary>
        /// <param name="buffer">The BitReader that contains messages to be read.</param>
        /// <param name="messagesToNotSkip">Message IDs of messages that should not be skipped.</param>
        /// <param name="doNotSkipAnyMessages">If true, no messages are skipped.</param>
        /// <param name="log">If this StreamWriter is not null, any message that isn't skipped is logged to it.</param>
        private void ReadMessages(Core.BitReader buffer, IEnumerable<byte> messagesToNotSkip, bool doNotSkipAnyMessages, StreamWriter log)
        {
            while (buffer.BitsLeft > 7)
            {
                try
                {
                    byte id = ReadMessageId(buffer);
                    Message message = ReadMessageHeader(buffer, id);

                    if (log != null)
                    {
                        log.WriteLine("{0} [{1}] Offset: {2}", message.Name, id, message.Offset);
                    }

                    if (doNotSkipAnyMessages || (messagesToNotSkip != null && messagesToNotSkip.Contains(id)))
                    {
                        try
                        {
                            message.Read(buffer);
                        }
                        catch (Exception)
                        {
                            if (log != null)
                            {
                                message.Log(log);
                                log.WriteLine();
                            }

                            throw;
                        }

                        if (log != null)
                        {
                            message.Log(log);
                            log.WriteLine();
                        }
                    }
                    else
                    {
                        message.Skip(buffer);
                    }
                }
                catch (Exception ex)
                {
                    throw new MessageException(messageHistory, ex);
                }
            }
        }

        private Message ReadMessageHeader(Core.BitReader buffer, byte? id)
        {
            long messageOffset = currentMessageBlockOffset + buffer.CurrentByte;

            if (id == null)
            {
                id = ReadMessageId(buffer);
            }
            else
            {
                // Assume the buffer position is one byte after where the ID was read.
                messageOffset--;
            }

            Message message = handler.CreateMessage(id.Value, NetworkProtocol);

            if (message == null)
            {
                throw new ApplicationException(string.Format("Unknown message type \"{0}\".", id));
            }

            message.Demo = this;
            message.Offset = messageOffset;
            messageHistory.Enqueue(message);
            return message;
        }

        #region Frame callbacks
        private List<FrameCallback> FindFrameCallbacks(Frame frame)
        {
            return frameCallbacks.FindAll(fc => fc.Id == frame.Id);
        }

        public void AddFrameCallback<T>(Action<T> method) where T : Frame
        {
            FrameCallback callback = new FrameCallback
            {
                Delegate = method
            };

            // Instantiate the frame type to get the ID.
            Frame frame = (Frame)Activator.CreateInstance(typeof(T));
            callback.Id = frame.Id;
            frameCallbacks.Add(callback);
        }
        #endregion
    }
}
