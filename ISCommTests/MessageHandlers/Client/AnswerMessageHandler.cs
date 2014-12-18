// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnswerMessageHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The answer message handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommTests.MessageHandlers.Client
{
    using System;

    using ISCommV3.MessageHandlers.Client;
    using ISCommV3.Messages;

    using TinyMessenger;

    /// <summary>
    ///     The answer message handler.
    /// </summary>
    internal class AnswerMessageHandler : BaseClientHandler<AnswerMessageHandler>, IClientHandler<AnswerMessage>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Execute(AnswerMessage message)
        {
            Console.WriteLine("Answermessage received");
            Console.WriteLine("Echo Data: " + message.Echo);
        }

        /// <summary>
        /// The subscriber.
        /// </summary>
        /// <param name="hub">
        /// The hub.
        /// </param>
        public void Subscriber(ITinyMessengerHub hub)
        {
            hub.Subscribe<AnswerMessage>(Instance.Execute);
        }

        #endregion
    }
}