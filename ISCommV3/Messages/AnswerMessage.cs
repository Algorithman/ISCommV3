// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnswerMessage.cs" company="">
//   
// </copyright>
// <summary>
//   The answer message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3.Messages
{
    using ISCommV3.MessageBase;

    using MsgPack.Serialization;

    /// <summary>
    ///     The answer message.
    /// </summary>
    public class AnswerMessage : BaseMessage
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the Echo.
        /// </summary>
        [MessagePackMember(0)]
        public string Echo { get; set; }

        #endregion
    }
}