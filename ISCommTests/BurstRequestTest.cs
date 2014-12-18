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
    using System.Diagnostics;
    using System.Threading;

    using ISCommV3;
    using ISCommV3.EventArgs;
    using ISCommV3.Messages;

    using NUnit.Framework;

    #endregion

    /// <summary>
    ///     The burst requests test.
    /// </summary>
    [TestFixture]
    public class BurstRequestsTest
    {
        #region Constants

        /// <summary>
        ///     The num threads.
        /// </summary>
        private const int NumThreads = 100;

        #endregion

        #region Static Fields

        /// <summary>
        ///     The singularity.
        /// </summary>
        private static object Singularity = new object();

        #endregion

        #region Fields

        /// <summary>
        ///     The server.
        /// </summary>
        protected ISCommServer server;

        /// <summary>
        ///     The mre.
        /// </summary>
        private readonly ManualResetEvent mre = new ManualResetEvent(false);

        /// <summary>
        ///     The countdown.
        /// </summary>
        private CountdownEvent countdown;

        /// <summary>
        ///     The countdown 2.
        /// </summary>
        private CountdownEvent countdown2;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     This test fires 100 request to server to command wait.
        ///     Command wait will sleep two seconds and response with 1.
        ///     The server has to open 100 threads at same time, and sleep two seconds
        ///     for each thread and send the response.
        ///     So this test doesn't has to take more than 4 seconds
        /// </summary>
        [Test]
        public void BurstTestMethodAsync()
        {
            this.server = new ISCommServer();
            this.server.Start("localhost", 3444);

            ThreadPool.SetMinThreads(NumThreads + 20, NumThreads + 20);
            Console.WriteLine("Burst test started");
            this.mre.Reset();
            this.countdown = new CountdownEvent(NumThreads);
            this.countdown2 = new CountdownEvent(NumThreads);
            for (int i = 0; i < NumThreads; i++)
            {
                new Thread(this.OneThreadExecution) { Name = "Thread " + i }.Start();
            }

            this.countdown.Wait();
            DateTime dateTime = DateTime.Now;
            this.countdown = new CountdownEvent(NumThreads);
            this.mre.Set();
            this.countdown.Wait();
            DateTime dt2 = DateTime.Now;
            Console.WriteLine("Send time: {0}", dt2 - dateTime);
            this.countdown2.Wait();
            TimeSpan timeSpan = DateTime.Now - dateTime;
            Console.WriteLine("Async Test finished ({0} Messages sent and received)", NumThreads);
            Console.WriteLine("Executed at {0}.{1:0}s.", timeSpan.Seconds, timeSpan.Milliseconds / 100);
            Console.WriteLine("Countdown 1: {0}", this.countdown.CurrentCount);
            Console.WriteLine("Countdown 2: {0}", this.countdown2.CurrentCount);

            Assert.IsTrue(timeSpan.Seconds < 10);
            this.server.Stop();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The one thread execution.
        /// </summary>
        private void OneThreadExecution()
        {
            var client = new ISCommClient();
            client.ObjectReceived += this.ClientObjectReceived;
            this.countdown.Signal();
            this.mre.WaitOne();
            bool connected = client.Connect("localhost", server.Port);
            if (!connected)
            {
                this.countdown.Signal();
            }

            Assert.True(connected);

            try
            {
                client.Send(new WaitMessage());
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message + "\r\n" + exception.StackTrace);
            }

            this.countdown.Signal();
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
        private void ClientObjectReceived(object sender, ReceivedObjectEventArgs e)
        {
            this.countdown2.Signal();
        }

        #endregion
    }
}