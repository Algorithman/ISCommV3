// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicMessage.cs" company="">
//   
// </copyright>
// <summary>
//   The dynamic message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3.MessageBase
{
    using System;
    using System.Reflection;

    using MsgPack;
    using MsgPack.Serialization;

    /// <summary>
    ///     The dynamic message.
    ///     Acts as a transport wrapper for network transfer
    /// </summary>
    public class DynamicMessage : IPackable, IUnpackable
    {
        #region Static Fields

        /// <summary>
        ///     The serializer.
        /// </summary>
        private static MessagePackSerializer<DynamicMessage> serializer = MessagePackSerializer.Get<DynamicMessage>();

        #endregion

        #region Fields

        /// <summary>
        ///     The data object.
        /// </summary>
        private BaseMessage dataObject;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicMessage" /> class.
        /// </summary>
        public DynamicMessage()
        {
            this.TypeName = string.Empty;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the data object.
        /// </summary>
        public BaseMessage DataObject
        {
            get
            {
                return this.dataObject;
            }

            set
            {
                this.TypeName = value.GetType().AssemblyQualifiedName;
                this.dataObject = value;
            }
        }

        /// <summary>
        ///     Gets or sets the type name.
        /// </summary>
        public string TypeName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The pack.
        /// </summary>
        /// <param name="mb">
        /// The mb.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] Pack(BaseMessage mb)
        {
            var dm = new DynamicMessage();
            dm.DataObject = mb;
            if (serializer == null)
            {
                serializer = MessagePackSerializer.Get<DynamicMessage>();
            }

            return serializer.PackSingleObject(dm);
        }

        /// <summary>
        /// The unpack.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="BaseMessage"/>.
        /// </returns>
        public static BaseMessage Unpack(byte[] data)
        {
            if (serializer == null)
            {
                serializer = MessagePackSerializer.Get<DynamicMessage>();
            }

            DynamicMessage dm = serializer.UnpackSingleObject(data);
            return dm.DataObject;
        }

        /// <summary>
        /// The pack to message.
        /// </summary>
        /// <param name="packer">
        /// The packer.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void PackToMessage(Packer packer, PackingOptions options)
        {
            // Serialize type name of the data object
            packer.Pack(this.TypeName);

            // Serialize the data object itself as a byte array
            packer.Pack(this.dataObject.GetData());
        }

        /// <summary>
        /// The unpack from message.
        /// </summary>
        /// <param name="unpacker">
        /// The unpacker.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void UnpackFromMessage(Unpacker unpacker)
        {
            // Read the type name
            this.TypeName = unpacker.LastReadData.AsString();

            // Read the data object as byte array
            byte[] temp;
            unpacker.ReadBinary(out temp);

            // Create a message serializer object
            Type type = Type.GetType(this.TypeName);
            if (type == null)
            {
                type = Assembly.GetCallingAssembly().GetType(this.TypeName);
                if (type == null)
                {
                    throw new ArgumentException(string.Format("Type '{0}' not found.", this.TypeName));
                }
            }

            IMessagePackSingleObjectSerializer ser = BaseMessage.serializers[type];

            // Unpack the message's data object
            this.dataObject = (BaseMessage)ser.UnpackSingleObject(temp);
        }

        #endregion
    }
}