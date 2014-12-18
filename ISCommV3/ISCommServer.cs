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
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;

    using ISCommV3.MessageHandlers.Server;

    using TinyMessenger;

    #endregion

    /// <summary>
    ///     The is comm server.
    /// </summary>
    public class ISCommServer
    {
        #region Fields

        /// <summary>
        ///     The bus.
        /// </summary>
        private readonly ITinyMessengerHub bus;

        /// <summary>
        ///     The sessions.
        /// </summary>
        private readonly Sessions sessions;

        /// <summary>
        ///     The listener.
        /// </summary>
        private TcpListener listener;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ISCommServer"/> class.
        /// </summary>
        /// <param name="autoSubscribe">
        /// The auto Subscribe.
        /// </param>
        /// <param name="compression">
        /// </param>
        public ISCommServer(bool autoSubscribe = true, bool compression = true)
        {
            this.bus = new TinyMessengerHub();
            this.sessions = new Sessions(this.bus, compression);
            this.Subscribe(autoSubscribe);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the bus.
        /// </summary>
        public ITinyMessengerHub Bus
        {
            get
            {
                return this.bus;
            }
        }

        /// <summary>
        ///     Gets the port.
        /// </summary>
        public int Port
        {
            get
            {
                return ((IPEndPoint)this.listener.LocalEndpoint).Port;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The start method of the Server.
        /// </summary>
        /// <param name="ipaddressRange">
        /// The IP address range the Server listens on.
        /// </param>
        /// <param name="port">
        /// The port the Server listens on.
        /// </param>
        /// <param name="compression">
        /// The use Compression.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Start(string ipaddressRange = "0.0.0.0", int port = 6670)
        {
            try
            {
                IPAddress address;
                if (!IPAddress.TryParse(ipaddressRange, out address))
                {
                    // No valid IP Address given, lets check hostnames over dns
                    if (!this.GetHostIPV4(ipaddressRange, out address))
                    {
                        // Also no valid hostname (or cannot be resolved)
                        throw new ArgumentException("ipaddressRange no valid IP Address or hostname");
                    }
                }

                this.listener = new TcpListener(address, port);
                this.listener.Start();
                this.sessions.DisposeAllSessions();
                this.AcceptClients();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     The stop.
        /// </summary>
        public void Stop()
        {
            this.listener.Stop();
            this.sessions.DisposeAllSessions();
        }

        #endregion

        #region Methods

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
        ///     The accept clients.
        /// </summary>
        private void AcceptClients()
        {
            this.listener.BeginAcceptTcpClient(
                ar =>
                {
                    try
                    {
                        TcpClient client = this.listener.EndAcceptTcpClient(ar);
                        this.AcceptClients();
                        this.sessions.NewSession(this, client);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }, 
                null);
        }

        /// <summary>
        /// The get host ip v 4.
        /// </summary>
        /// <param name="ipaddressRange">
        /// The ipaddress range.
        /// </param>
        /// <param name="ipAddress">
        /// The ip address.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool GetHostIPV4(string ipaddressRange, out IPAddress ipAddress)
        {
            IPAddress[] list = Dns.GetHostAddresses(ipaddressRange);
            foreach (IPAddress address in list)
            {
                // Lets focus on IPV4 first
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = address;
                    return true;
                }
            }

            ipAddress = IPAddress.Any;
            return false;
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
                foreach (Type type in GetAllTypesImplementingOpenGenericType(typeof(IServerHandler<>)))
                {
                    PropertyInfo prop = type.GetProperty(
                        "Instance", 
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    object instance = prop.GetValue(null, null);
                    var inst = (IServerHandler)instance;
                    inst.Subscriber(this.bus);
#if DEBUG
                    Console.WriteLine(instance.GetType().FullName);
#endif
                }
            }
        }

        #endregion
    }
}