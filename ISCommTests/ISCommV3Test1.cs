
namespace ISCommTests
{
    using System;
    using System.Threading;

    using ISCommV3;
    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;
    using ISCommV3.Messages;

    using NUnit.Framework;

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