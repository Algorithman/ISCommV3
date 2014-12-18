// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitTest1.cs" company="">
//   
// </copyright>
// <summary>
//   The unit test 1.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommTests
{
    using System.Threading;

    using ISCommV3;
    using ISCommV3.EventArgs;
    using ISCommV3.MessageBase;
    using ISCommV3.MessageHandlers.Server;
    using ISCommV3.Messages;

    using MsgPack.Serialization;

    using NUnit.Framework;

    using TinyMessenger;

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
            server.Start(4343);
            this.mrep.Reset();
            client.ObjectReceived += this.client_ObjectReceived;
            client.Connect("localhost", server.Port);

            client.Send(new DerivedTest());

            this.mrep.WaitOne();
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

            server.Start(4545);

            BaseMessage dm = new EchoMessage();

            client.Connect("localhost", 4545);

            var em = new EchoMessage();
            em.EchoText = "Hallo";

            this.mrep.Reset();
            client.ObjectReceived += this.ClientObjectReceived;
            client.Send(em);

            this.mrep.WaitOne();

            Assert.IsNotNull(this.reply);
            Assert.IsAssignableFrom<AnswerMessage>(this.reply);
            Assert.AreEqual(((AnswerMessage)this.reply).Echo, em.EchoText);
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