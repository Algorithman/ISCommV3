// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sessions.cs" company="">
//   
// </copyright>
// <summary>
//   The sessions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3
{
    using System.Collections.Generic;
    using System.Net.Sockets;

    using TinyMessenger;

    /// <summary>
    ///     The sessions.
    /// </summary>
    public class Sessions
    {
        #region Fields

        /// <summary>
        ///     The bus.
        /// </summary>
        private readonly ITinyMessengerHub bus;

        /// <summary>
        ///     The sessions.
        /// </summary>
        private readonly List<Session> sessions = new List<Session>();

        /// <summary>
        ///     The singularity.
        /// </summary>
        private readonly object singularity = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Sessions"/> class.
        /// </summary>
        /// <param name="bus">
        /// The bus.
        /// </param>
        public Sessions(ITinyMessengerHub bus)
        {
            this.bus = bus;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose all sessions.
        /// </summary>
        public void DisposeAllSessions()
        {
            for (int i = this.sessions.Count - 1; i >= 0; i--)
            {
                this.sessions[i].Dispose();
            }
        }

        /// <summary>
        /// The disposed.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        public void Disposed(Session session)
        {
            lock (this.singularity)
            {
                this.sessions.Remove(session);
            }
        }

        /// <summary>
        /// The new session.
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="Session"/>.
        /// </returns>
        public Session NewSession(ISCommServer server, TcpClient client)
        {
            var session = new Session(this, this.bus, client);

            lock (this.singularity)
            {
                this.sessions.Add(session);
            }

            return session;
        }

        #endregion
    }
}