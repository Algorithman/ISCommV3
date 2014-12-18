// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReceivedObjectEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   The received object event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3.EventArgs
{
    using System;

    using ISCommV3.MessageBase;

    /// <summary>
    ///     The received object event args.
    /// </summary>
    public class ReceivedObjectEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedObjectEventArgs"/> class.
        /// </summary>
        /// <param name="messageObject">
        /// The message object.
        /// </param>
        public ReceivedObjectEventArgs(BaseMessage messageObject)
        {
            this.MessageObject = messageObject;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the message object.
        /// </summary>
        public BaseMessage MessageObject { get; set; }

        #endregion
    }
}