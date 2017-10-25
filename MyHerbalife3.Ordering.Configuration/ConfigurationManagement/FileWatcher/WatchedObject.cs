using System.IO;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.FileWatcher
{
    internal class WatchedObject
    {
        #region Private members

        private readonly ObjectType _objectType = ObjectType.Unknown;
        private readonly FileSystemWatcher _watcher;

        #endregion

        #region Constructors

        public WatchedObject(ObjectType objectType, FileSystemWatcher fileSystemWatcher)
        {
            _objectType = objectType;
            _watcher = fileSystemWatcher;
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     The type of object, file or directory
        /// </summary>
        public ObjectType ObjectType
        {
            get { return _objectType; }
        }

        /// <summary>
        ///     The underlying system FileSystemWatcher
        /// </summary>
        public FileSystemWatcher Watcher
        {
            get { return _watcher; }
        }

        #endregion
    }
}