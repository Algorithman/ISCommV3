// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EchoMessage.cs" company="">
//   
// </copyright>
// <summary>
//   The Echo message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3.Messages
{
    using ISCommV3.MessageBase;

    using MsgPack.Serialization;

    /// <summary>
    ///     The Echo message.
    /// </summary>
    public class EchoMessage : BaseMessage
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the Echo text.
        /// </summary>
        [MessagePackMember(0)]
        public string EchoText { get; set; }

        #endregion
    }
}