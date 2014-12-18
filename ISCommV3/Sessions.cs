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

    using System.Collections.Generic;
    using System.Net.Sockets;

    using TinyMessenger;

    #endregion

    /// <summary>
    ///     The sessions.
    /// </summary>
    public class Sessions
    {
        #region Fields

        /// <summary>
        /// The use compression.
        /// </summary>
        private readonly bool useCompression;

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
        public Sessions(ITinyMessengerHub bus, bool useCompression = true)
        {
            this.bus = bus;
            this.useCompression = useCompression;
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
            var session = new Session(this, this.bus, client, this.useCompression);

            lock (this.singularity)
            {
                this.sessions.Add(session);
            }

            return session;
        }

        #endregion
    }
}