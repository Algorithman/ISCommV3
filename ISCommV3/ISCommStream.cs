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
    using System.Net.Sockets;
    using System.Threading;

    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;

    #endregion

    /// <summary>
    ///     The is comm stream.
    /// </summary>
    internal class ISCommStream : IDisposable
    {
        #region Fields

        /// <summary>
        ///     The stream.
        /// </summary>
        protected readonly NetworkStream stream;

        /// <summary>
        ///     The client.
        /// </summary>
        private readonly TcpClient client;

        /// <summary>
        ///     The receiving.
        /// </summary>
        private bool receiving;

        /// <summary>
        ///     The stop receive.
        /// </summary>
        private bool stopReceive;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ISCommStream"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        public ISCommStream(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();
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
        ///     Gets the buffer size.
        /// </summary>
        public int BufferSize
        {
            get
            {
                return this.buffer.GetBufferSize();
            }
        }

        /// <summary>
        ///     Gets the network stream.
        /// </summary>
        public NetworkStream NetworkStream
        {
            get
            {
                return this.stream;
            }
        }

        /// <summary>
        ///     Gets the tcp client.
        /// </summary>
        public TcpClient TcpClient
        {
            get
            {
                return this.client;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether buffer has data.
        /// </summary>
        protected bool BufferHasData
        {
            get
            {
                return this.BufferSize > 0;
            }
        }

        /// <summary>
        ///     Gets or sets the buffer.
        /// </summary>
        private ByteArrayBuffer buffer { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.receiving = false;
            Thread.Sleep(50);
            this.stream.Close();
        }

        /// <summary>
        ///     The receive length.
        /// </summary>
        public void ReceiveLength()
        {
            var lengthBytes = new byte[4];

            this.stream.BeginRead(lengthBytes, 0, 4, this.ReceiveObject, lengthBytes);
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="messageData">
        /// The message data.
        /// </param>
        public void Send(BaseMessage messageData)
        {
            this.Send(DynamicMessage.Pack(messageData));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The begin receive.
        /// </summary>
        internal void BeginReceive()
        {
            this.receiving = true;
            this.Receive();
        }

        /// <summary>
        ///     The receive.
        /// </summary>
        private void Receive()
        {
            if (this.receiving)
            {
                this.ReceiveLength();
            }
        }

        /// <summary>
        /// The receive message.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private byte[] ReceiveMessage(int length)
        {
            var bytes = new byte[length];
            int currentIndex = 0;
            int bytesRead = -1;

            while (bytesRead != 0 && currentIndex < bytes.Length)
            {
                bytesRead = this.stream.Read(bytes, currentIndex, bytes.Length - currentIndex);
                currentIndex += bytesRead;
            }

            return bytes;
        }

        /// <summary>
        /// The receive object.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void ReceiveObject(IAsyncResult result)
        {
            this.stream.EndRead(result);
            var lengthBytes = result.AsyncState as byte[];
            if (lengthBytes != null)
            {
                int length = BitConverter.ToInt32(lengthBytes, 0);
                var buffer = new byte[length];
                this.stream.BeginRead(buffer, 0, length, this.ReceivedObject, buffer);
            }
        }

        /// <summary>
        /// The received object.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void ReceivedObject(IAsyncResult result)
        {
            this.stream.EndRead(result);
            var buffer = result.AsyncState as byte[];
            if (buffer != null)
            {
                BaseMessage bm = DynamicMessage.Unpack(buffer);
                EventHandler<ReceivedObjectEventArgs> handler = this.ObjectReceived;
                if (handler != null)
                {
                    handler(this, new ReceivedObjectEventArgs(bm));
                }
            }
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void Send(byte[] data)
        {
            // Write length prefix, then the data
            this.SendLength(data.Length);

            this.stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// The send length.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        private void SendLength(int length)
        {
            this.stream.Write(BitConverter.GetBytes(length), 0, 4);
        }

        #endregion
    }
}