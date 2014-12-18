﻿#region License
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

    using Ionic.Zlib;

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
        ///     The client.
        /// </summary>
        private readonly TcpClient client;

        /// <summary>
        ///     The stream.
        /// </summary>
        private readonly NetworkStream networkStream;

        /// <summary>
        ///     The use compression.
        /// </summary>
        private readonly bool useCompression;

        /// <summary>
        ///     The receiving.
        /// </summary>
        private bool receiving;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ISCommStream"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="useZlib">
        /// The use Zlib.
        /// </param>
        public ISCommStream(TcpClient client, bool useZlib = true)
        {
            this.client = client;
            this.networkStream = client.GetStream();
            this.useCompression = useZlib;
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
        ///     Gets the network stream.
        /// </summary>
        public NetworkStream NetworkStream
        {
            get
            {
                return this.networkStream;
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

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.receiving = false;
            this.networkStream.Close();
        }

        /// <summary>
        ///     The receive length.
        /// </summary>
        public void ReceiveLength()
        {
            var lengthBytes = new byte[4];

            this.networkStream.BeginRead(lengthBytes, 0, 4, this.ReceiveObject, lengthBytes);
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
        /// The receive object.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void ReceiveObject(IAsyncResult result)
        {
            this.networkStream.EndRead(result);
            var lengthBytes = result.AsyncState as byte[];
            if (lengthBytes != null)
            {
                int length = BitConverter.ToInt32(lengthBytes, 0);
                var tempBuffer = new byte[length];
                this.networkStream.BeginRead(tempBuffer, 0, length, this.ReceiveObjectCallBack, tempBuffer);
            }
        }

        /// <summary>
        /// The received object.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void ReceiveObjectCallBack(IAsyncResult result)
        {
            this.networkStream.EndRead(result);
            var tempBuffer = result.AsyncState as byte[];
            if (tempBuffer != null)
            {
                BaseMessage bm;
                if (this.useCompression)
                {
                    byte[] uncompressed = ZlibStream.UncompressBuffer(tempBuffer);
                    bm = DynamicMessage.Unpack(uncompressed);
                }
                else
                {
                    bm = DynamicMessage.Unpack(tempBuffer);
                }

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
            if (!this.useCompression)
            {
                // Write length prefix, then the data
                this.SendLength(data.Length);

                this.networkStream.Write(data, 0, data.Length);
            }
            else
            {
                byte[] compressed = ZlibStream.CompressBuffer(data);
                this.SendLength(compressed.Length);
                this.networkStream.Write(compressed, 0, compressed.Length);
            }
        }

        /// <summary>
        /// The send length.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        private void SendLength(int length)
        {
            this.networkStream.Write(BitConverter.GetBytes(length), 0, 4);
        }

        #endregion
    }
}