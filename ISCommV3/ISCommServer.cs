// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISCommServer.cs" company="">
//   
// </copyright>
// <summary>
//   The is comm server.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;

    using ISCommV3.MessageHandlers.Server;
    using ISCommV3.Messages;

    using TinyMessenger;

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
        ///     Initializes a new instance of the <see cref="ISCommServer" /> class.
        /// </summary>
        public ISCommServer(bool autoSubscribe = true)
        {
            this.bus = new TinyMessengerHub();
            this.sessions = new Sessions(this.bus);
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
        /// The start.
        /// </summary>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="autoSubscribe">
        /// The auto Subscribe.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Start(int port)
        {
            try
            {
                this.listener = new TcpListener(IPAddress.Any, port);
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