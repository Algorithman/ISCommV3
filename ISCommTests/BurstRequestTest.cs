// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BurstRequestTest.cs" company="">
//   
// </copyright>
// <summary>
//   The burst requests test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommTests
{
    using System;
    using System.Threading;

    using ISCommV3;
    using ISCommV3.EventArgs;
    using ISCommV3.Messages;

    using NUnit.Framework;

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
            this.server.Start(3444);

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
            client.ObjectReceived += this.client_ObjectReceived;
            this.countdown.Signal();
            this.mre.WaitOne();
            client.Connect("localhost", this.server.Port);

            try
            {
                client.Send(new WaitMessage());
            }
            catch (Exception exception)
            {
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
        private void client_ObjectReceived(object sender, ReceivedObjectEventArgs e)
        {
            this.countdown2.Signal();
        }

        #endregion
    }
}