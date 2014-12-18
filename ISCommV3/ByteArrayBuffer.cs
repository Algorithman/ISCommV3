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
namespace ISCommV3
{
    #region Usings

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    ///     The byte array buffer.
    /// </summary>
    public class ByteArrayBuffer
    {
        #region Fields

        /// <summary>
        ///     The buffer.
        /// </summary>
        private readonly List<byte[]> buffer = new List<byte[]>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The append.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public void Append(byte[] data)
        {
            lock (this.buffer)
            {
                this.buffer.Add(data);
            }
        }

        /// <summary>
        ///     The get buffer size.
        /// </summary>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int GetBufferSize()
        {
            int c = 0;
            int length = 0;
            lock (this.buffer)
            {
                while (c < this.buffer.Count)
                {
                    length += this.buffer[c++].Length;
                }
            }

            return length;
        }

        /// <summary>
        /// The get bytes.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public byte[] GetBytes(int length = 0)
        {
            int maxlen = this.GetBufferSize();
            if (length == 0)
            {
                length = maxlen;
            }
            else if (length > maxlen)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Length({0}) is too high, Buffer size is {1} bytes.", length, maxlen));
            }

            var result = new byte[length];

            lock (this.buffer)
            {
                while (length > 0)
                {
                    if (length >= this.buffer[0].Length)
                    {
                        // Take the whole buffer segment
                        Array.Copy(this.buffer[0], 0, result, result.Length - length, this.buffer[0].Length);
                        length -= this.buffer[0].Length;
                        this.buffer.RemoveAt(0);
                    }
                    else
                    {
                        Array.Copy(this.buffer[0], 0, result, result.Length - length, length);
                        var temp = new byte[this.buffer[0].Length - length];
                        Array.Copy(this.buffer[0], length, temp, 0, this.buffer[0].Length - length);
                        this.buffer[0] = temp;
                        length = 0;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}