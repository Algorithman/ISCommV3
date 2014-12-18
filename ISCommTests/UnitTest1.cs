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
namespace ISCommTests
{
    #region Usings

    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading;

    using ISCommV3;
    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;
    using ISCommV3.MessageHandlers.Server;
    using ISCommV3.Messages;

    using MsgPack.Serialization;

    using NUnit.Framework;

    using TinyMessenger;

    #endregion

    /// <summary>
    ///     The unit test 1.
    /// </summary>
    [TestFixture]
    public class UnitTest1
    {
        #region Fields

        /// <summary>
        ///     The mrep.
        /// </summary>
        private readonly ManualResetEvent mrep = new ManualResetEvent(false);

        /// <summary>
        ///     The reply.
        /// </summary>
        private BaseMessage reply;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The out side message class test.
        /// </summary>
        [Test]
        public void OutSideMessageClassTest()
        {
            var server = new ISCommServer();
            var client = new ISCommClient();
            server.Start();
            this.mrep.Reset();
            client.ObjectReceived += this.client_ObjectReceived;
            bool connected = client.Connect("127.0.0.1", server.Port);
            if (!connected)
            {
                server.Stop();
            }

            Assert.True(connected);

            client.Send(new DerivedTest());

            this.mrep.WaitOne();
            server.Stop();
            Assert.IsTrue((this.reply is AnswerMessage) && (((AnswerMessage)this.reply).Echo == "Derived"));
        }

        /// <summary>
        ///     The test method 1.
        /// </summary>
        [Test]
        public void TestMethod1()
        {
            var server = new ISCommServer();
            var client = new ISCommClient();

            server.Start();

            BaseMessage dm = new EchoMessage();

            bool connected = client.Connect("127.0.0.1", server.Port);
            if (!connected)
            {
                server.Stop();
            }

            Assert.True(connected);

            var em = new EchoMessage();
            em.EchoText = "Hallo";

            this.mrep.Reset();
            client.ObjectReceived += this.ClientObjectReceived;
            client.Send(em);

            this.mrep.WaitOne();
            server.Stop();

            Assert.IsNotNull(this.reply);
            Assert.IsAssignableFrom<AnswerMessage>(this.reply);
            Assert.AreEqual(((AnswerMessage)this.reply).Echo, em.EchoText);
        }

        /// <summary>
        /// The show i ps.
        /// </summary>
        [Test]
        public void ShowIPs()
        {
            var server = new ISCommServer();
            server.Start();
            IPEndPoint ipep = server.listener.Server.LocalEndPoint as IPEndPoint;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                foreach (IPAddressInformation unicast in properties.UnicastAddresses)
                {
                    if (ipep.AddressFamily == unicast.Address.AddressFamily) Console.WriteLine("Listening: {0}:{1}", unicast.Address, ipep.Port);
                }
            }

            server.Stop();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The client_ object received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ClientObjectReceived(object sender, ReceivedObjectEventArgs e)
        {
            this.reply = e.MessageObject;
            this.mrep.Set();
        }

        /// <summary>
        /// The client_ object received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void client_ObjectReceived(object sender, ReceivedObjectEventArgs e)
        {
            this.reply = e.MessageObject;
            this.mrep.Set();
        }

        #endregion
    }

    /// <summary>
    ///     The derived test.
    /// </summary>
    public class DerivedTest : BaseMessage
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the dummy 1.
        /// </summary>
        [MessagePackMember(0)]
        public int dummy1 { get; set; }

        #endregion
    }

    /// <summary>
    ///     The derived handler.
    /// </summary>
    public class DerivedHandler : BaseServerHandler<DerivedHandler>, IServerHandler<DerivedTest>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Execute(DerivedTest message)
        {
            var am = new AnswerMessage();
            am.Echo = "Derived";
            (message.Sender as Session).Reply(am);
        }

        /// <summary>
        /// The subscriber.
        /// </summary>
        /// <param name="hub">
        /// The hub.
        /// </param>
        public void Subscriber(ITinyMessengerHub hub)
        {
            hub.Subscribe<DerivedTest>(Instance.Execute);
        }

        #endregion
    }
}