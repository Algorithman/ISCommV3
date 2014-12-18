// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Session.cs" company="">
//   
// </copyright>
// <summary>
//   The session.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Reflection;

    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;

    using TinyMessenger;

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
        /// <param name="tcpClient">
        /// The tcp client.
        /// </param>
        public Session(Sessions sessions, ITinyMessengerHub bus, TcpClient tcpClient)
        {
            this.client = tcpClient;
            this.stream = new ISCommStream(tcpClient);
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