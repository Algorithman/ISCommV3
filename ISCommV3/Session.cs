#region License
// Copyright (c) 2005-2014, CellAO Team
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion
namespace ISCommV3
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Reflection;

    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;

    using TinyMessenger;

    #endregion

    /// <summary>
    ///     The session.
    /// </summary>
    public class Session : IDisposable
    {
        #region Fields

        /// <summary>
        ///     The bus.
        /// </summary>
        private readonly ITinyMessengerHub bus;

        /// <summary>
        ///     The client.
        /// </summary>
        private readonly TcpClient client;

        /// <summary>
        ///     The publishers.
        /// </summary>
        private readonly Dictionary<Type, MethodInfo> publishers = new Dictionary<Type, MethodInfo>();

        /// <summary>
        ///     The sessions.
        /// </summary>
        private readonly Sessions sessions;

        /// <summary>
        ///     The stream.
        /// </summary>
        private readonly ISCommStream stream;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="sessions">
        /// The sessions.
        /// </param>
        /// <param name="bus">
        /// The bus.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="useCompression">
        /// The use compression.
        /// </param>
        public Session(Sessions sessions, ITinyMessengerHub bus, TcpClient client, bool useCompression = true)
        {
            this.client = client;
            this.stream = new ISCommStream(client, useCompression);
            this.stream.ObjectReceived += this.StreamObjectReceived;
            this.bus = bus;
            this.sessions = sessions;
            this.stream.BeginReceive();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// The reply.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Reply(BaseMessage message)
        {
            this.stream.Send(message);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            this.stream.Dispose();
            this.client.Close();
            this.sessions.Disposed(this);
        }

        /// <summary>
        /// The publish.
        /// </summary>
        /// <param name="messageObject">
        /// The message object.
        /// </param>
        private void Publish(BaseMessage messageObject)
        {
            messageObject.Sender = this;

            MethodInfo mig;
            if (!this.publishers.ContainsKey(messageObject.GetType()))
            {
                MethodInfo mi = typeof(ITinyMessengerHub).GetMethod("Publish");
                mig = mi.MakeGenericMethod(messageObject.GetType());
                this.publishers.Add(messageObject.GetType(), mig);
            }
            else
            {
                mig = this.publishers[messageObject.GetType()];
            }

            mig.Invoke(this.bus, new object[] { messageObject });
        }

        /// <summary>
        /// The stream object received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void StreamObjectReceived(object sender, ReceivedObjectEventArgs e)
        {
            this.Publish(e.MessageObject);
        }

        #endregion
    }
}