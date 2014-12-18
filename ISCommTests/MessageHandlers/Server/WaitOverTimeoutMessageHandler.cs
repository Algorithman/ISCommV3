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
namespace ISCommTests.MessageHandlers.Server
{
    #region Usings

    using System.Threading;

    using ISCommV3;
    using ISCommV3.MessageHandlers.Server;
    using ISCommV3.Messages;

    using TinyMessenger;

    #endregion

    /// <summary>
    ///     The wait over timeout message handler.
    /// </summary>
    internal class WaitOverTimeoutMessageHandler : BaseServerHandler<WaitOverTimeoutMessageHandler>, 
                                                   IServerHandler<WaitOverTimeoutMessage>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="em">
        /// The em.
        /// </param>
        public void Execute(WaitOverTimeoutMessage em)
        {
            var am = new AnswerMessage();
            am.Echo = "Waited for 1 second";
            Thread.Sleep(10000);
            if (em.Sender is Session)
            {
                (em.Sender as Session).Reply(am);
            }
        }

        /// <summary>
        /// The subscriber.
        /// </summary>
        /// <param name="hub">
        /// The hub.
        /// </param>
        public void Subscriber(ITinyMessengerHub hub)
        {
            hub.Subscribe<WaitOverTimeoutMessage>(Instance.Execute);
        }

        #endregion
    }
}