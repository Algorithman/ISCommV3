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
namespace ISCommTests
{
    #region Usings

    using System;
    using System.Threading;

    using ISCommV3;
    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;
    using ISCommV3.Messages;

    using NUnit.Framework;

    #endregion

    /// <summary>
    ///     The is comm v 3 test 1.
    /// </summary>
    [TestFixture]
    public class ISCommV3Test1
    {
        #region Static Fields

        /// <summary>
        ///     The mrep.
        /// </summary>
        public static ManualResetEvent mrep = new ManualResetEvent(false);

        #endregion

        #region Fields

        /// <summary>
        ///     The reply.
        /// </summary>
        public BaseMessage reply;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The test 1.
        /// </summary>
        [Test]
        public void Test1()
        {
            var server = new ISCommServer();
            var client = new ISCommClient(-1, -1, false);

            server.Start(4545);

            client.Connect("localhost", 4545);
            client.ObjectReceived += this.client_ObjectReceived;

            var em = new EchoMessage();
            em.EchoText = "Hallo";

            mrep.Reset();

            client.Send(em);
            mrep.WaitOne();

            Assert.IsNotNull(this.reply);
            Assert.IsInstanceOf<AnswerMessage>(this.reply);
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
        private void client_ObjectReceived(object sender, ReceivedObjectEventArgs e)
        {
            Console.WriteLine("Data received also through event handler");
            this.reply = e.MessageObject;
            mrep.Set();
        }

        #endregion
    }
}