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
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;

    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;
    using ISCommV3.MessageHandlers.Client;

    using TinyMessenger;

    #endregion

    /// <summary>
    ///     The is comm client.
    /// </summary>
    public class ISCommClient : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// The publishers.
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> publishers = new Dictionary<Type, MethodInfo>();

        #endregion

        #region Fields

        /// <summary>
        ///     The bus.
        /// </summary>
        private readonly ITinyMessengerHub bus;

        /// <summary>
        ///     The receive timeout.
        /// </summary>
        private int receiveTimeout = -1;

        /// <summary>
        ///     The send timeout.
        /// </summary>
        private int sendTimeout = -1;

        /// <summary>
        ///     The stream.
        /// </summary>
        private ISCommStream stream;

        /// <summary>
        ///     The tcp client.
        /// </summary>
        private TcpClient tcpClient = new TcpClient();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ISCommClient"/> class.
        /// </summary>
        /// <param name="receiveTimeout">
        /// The receive timeout.
        /// </param>
        /// <param name="sendTimeout">
        /// The send timeout.
        /// </param>
        /// <param name="autoSubscribe">
        /// The auto Subscribe.
        /// </param>
        public ISCommClient(int receiveTimeout = -1, int sendTimeout = 1, bool autoSubscribe = true)
        {
            this.receiveTimeout = receiveTimeout;
            this.sendTimeout = sendTimeout;
            this.bus = new TinyMessengerHub();
            this.Subscribe(autoSubscribe);
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The object received.
        /// </summary>
        public event EventHandler<ReceivedObjectEventArgs> ObjectReceived;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the receive timeout.
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return this.receiveTimeout;
            }

            set
            {
                this.receiveTimeout = value;
                if (this.stream.TcpClient.Connected)
                {
                    this.stream.NetworkStream.ReadTimeout = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the send timeout.
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return this.sendTimeout;
            }

            set
            {
                this.sendTimeout = value;
                if (this.tcpClient.Connected)
                {
                    this.stream.NetworkStream.WriteTimeout = value;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The connect.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <returns>
        /// The <see cref="ISCommClient"/>.
        /// </returns>
        public bool Connect(string host, int port)
        {
            if (this.tcpClient.Connected == false)
            {
                try
                {
                    this.tcpClient.Connect(host, port);
                    this.stream = new ISCommStream(this.tcpClient);
                    this.stream.NetworkStream.WriteTimeout = this.sendTimeout;
                    this.stream.NetworkStream.ReadTimeout = this.receiveTimeout;
                    this.stream.ObjectReceived += this.StreamObjectReceived;
                    this.stream.BeginReceive();
                    return true;
                }
                catch (SocketException socketException)
                {
                    if (socketException.ErrorCode == 10056)
                    {
                        // One immediate retry only
                        this.tcpClient.Close();
                        this.tcpClient = new TcpClient();
                        this.tcpClient.Connect(host, port);
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="messageObject">
        /// The message object.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        public void Send(BaseMessage messageObject, Action<BaseMessage> callback)
        {
            this.stream.Send(messageObject);
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="messageObject">
        /// The message object.
        /// </param>
        public void Send(BaseMessage messageObject)
        {
            this.stream.Send(messageObject);
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
            this.tcpClient.Close();
            this.stream.Dispose();
        }

        /// <summary>
        /// The get all types implementing open generic type.
        /// </summary>
        /// <param name="openGenericType">
        /// The open generic type.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType)
        {
            return from h in AppDomain.CurrentDomain.GetAssemblies()
                   from x in h.GetTypes()
                   from z in x.GetInterfaces()
                   let y = x.BaseType
                   where
                       (y != null && y.IsGenericType && openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition()))
                       || (z.IsGenericType && openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                   select x;
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
            e.MessageObject.Sender = this;
            this.bus.Publish(e.MessageObject);

            EventHandler<ReceivedObjectEventArgs> handler = this.ObjectReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// The subscribe.
        /// </summary>
        /// <param name="autoSubscribe">
        /// The auto subscribe.
        /// </param>
        private void Subscribe(bool autoSubscribe)
        {
            if (autoSubscribe)
            {
                foreach (Type type in GetAllTypesImplementingOpenGenericType(typeof(IClientHandler<>)))
                {
                    PropertyInfo prop = type.GetProperty(
                        "Instance", 
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    object instance = prop.GetValue(null, null);
                    var inst = (IClientHandler)instance;
                    inst.Subscriber(this.bus);
                    Console.WriteLine(instance.GetType().FullName + " subscribed");
                }
            }
        }

        #endregion
    }
}