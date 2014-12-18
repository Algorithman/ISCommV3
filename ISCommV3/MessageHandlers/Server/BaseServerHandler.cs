// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseServerHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The base server handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ISCommV3.MessageHandlers.Server
{
    /// <summary>
    /// The base server handler.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class BaseServerHandler<T>
        where T : BaseServerHandler<T>, new()
    {
        #region Static Fields

        /// <summary>
        ///     The _instance.
        /// </summary>
        private static readonly T _instance = new T();

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion
    }
}