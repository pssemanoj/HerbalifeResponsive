using System;
using System.Collections.Generic;
using System.IO;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.FileWatcher
{

    #region "Enums"

    /// -----------------------------------------------------------------------------
    /// Project		: FileSystemWatcherManager
    /// Enum		: ObjectTypes
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     The types of objects the program can watch
    /// </summary>
    /// <remarks></remarks>
    /// <history>
    ///     [michaelpi] 	6/4/2004	Created
    ///     [karunk]        6/28/2004   Changed the NotifyFilter in AddWatchedObject
    ///     so that events get fired properly for Creation,
    ///     Deletion and Change.
    /// </history>
    /// -----------------------------------------------------------------------------
    internal enum ObjectType
    {
        File = 0,
        Directory = 1,
        Unknown = 2
    }

    #endregion

    #region "Manager"

    /// -----------------------------------------------------------------------------
    /// Project		: FileSystemWatcherManager
    /// Class		: Manager
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Facade layer to simplify communication with the FileSystemWatcherObjects
    /// </summary>
    /// <remarks></remarks>
    /// <history>
    ///     [michaelpi] 	6/4/2004	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class FileSystemWatcherManager
    {
        #region Constants

        private const string mc_AllFiles = "*.*";

        #endregion

        //TODO: Add a method of appending filters to a currently existing FSW
        //TODO: Need to perform verification of child items for a file AND Directory

        private static readonly Dictionary<string, WatchedObject> _watchedObjects;

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Static constructor
        /// </summary>
        /// <remarks></remarks>
        /// <history>
        ///     [michaelpi] 	6/4/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        static FileSystemWatcherManager()
        {
            _watchedObjects = new Dictionary<string, WatchedObject>();
        }

        private FileSystemWatcherManager()
        {
        }

        public static event EventHandler<FileNotifyEventArgs> FileCreated;
        public static event EventHandler<FileNotifyEventArgs> FileDeleted;
        public static event EventHandler<FileNotifyEventArgs> FileChanged;

        /// -----------------------------------------------------------------------------
        /// <overloads>
        ///     Adds a watched object into the collection
        /// </overloads>
        /// <summary>
        ///     Adds a watched object into the collection
        /// </summary>
        /// <param name="ObjectPath">The path of the item</param>
        /// <param name="Filter"></param>
        /// <param name="IncludeSubdirectories"></param>
        /// <remarks></remarks>
        /// <history>
        ///     [michaelpi] 	6/4/2004	Created
        ///     [karunk]        6/28/2004   Changed the NotifyFilter so that
        ///     events get fired properly for Creation,
        ///     Deletion and Change.
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void AddWatchedObject(string ObjectPath)
        {
            WatchedObject watchedObject = null;
            var watcher = new FileSystemWatcher();

            watcher.Path = new FileInfo(ObjectPath).DirectoryName;
            watcher.Filter = Path.GetFileName(ObjectPath);
            watcher.IncludeSubdirectories = false;

            watcher.NotifyFilter = NotifyFilters.LastWrite;
            //-----------------------------------------------------------------------------
            // Set the Handlers
            //-----------------------------------------------------------------------------
            watcher.Changed += OnFileChanged;
            watcher.Deleted += OnFileDeleted;
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;

            watchedObject = new WatchedObject(ObjectType.File, watcher);

            _watchedObjects.Add(ObjectPath, watchedObject);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Removes a watched file from the collection
        /// </summary>
        /// <param name="ObjectPath">The key to remove from the collection</param>
        /// <remarks></remarks>
        /// <history>
        ///     [michaelpi] 	6/4/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void RemoveWatchedFile(string ObjectPath)
        {
            WatchedObject watchedObject = null;

            if (_watchedObjects.ContainsKey(ObjectPath))
            {
                watchedObject = _watchedObjects[ObjectPath];

                if (watchedObject == null)
                {
                    throw (new Exception(
                        string.Format("Unable to retreive the object from the collection using path {0}", ObjectPath)));
                }

                var watcher = watchedObject.Watcher;
                watcher.Created -= OnFileCreated;
                watcher.Changed -= OnFileChanged;
                watcher.Deleted -= OnFileDeleted;
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();

                _watchedObjects.Remove(ObjectPath);
            }
            else
            {
                throw (new Exception(string.Format("The item {0} does not exist in the collection", ObjectPath)));
            }
        }

        /// <summary>
        ///     Gets the filter for a watched object.
        /// </summary>
        /// <param name="objectPath">The key to obtain the filter for.</param>
        /// <returns>
        ///     If the key does not exist, returns a null;
        ///     otherwise returns the Filter of the watched object.
        /// </returns>
        public static string GetFilter(string objectPath)
        {
            var wo = _watchedObjects[objectPath];

            return (wo == null ? null : wo.Watcher.Filter);
        }

        private static string GetFileName(string ObjectPath)
        {
            return Path.GetFileName(ObjectPath);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Handles the Changed event on a FSW object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        ///     [michaelpi] 	6/4/2004	Created
        ///     [hansvk]        5/22/2007   Adjusted for event conventions
        /// </history>
        /// -----------------------------------------------------------------------------
        protected static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (FileChanged != null)
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    FileChanged(sender, new FileNotifyEventArgs(GetFileName(e.FullPath), e.FullPath));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Handles the Deleted event on a FSW object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        ///     [michaelpi] 	6/4/2004	Created
        ///     [hansvk]        5/22/2007   Adjusted for event conventions
        /// </history>
        /// -----------------------------------------------------------------------------
        protected static void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (FileDeleted != null)
                FileDeleted(sender, new FileNotifyEventArgs(GetFileName(e.FullPath), e.FullPath));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Handles the Created event on a FSW object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        ///     [michaelpi] 	6/4/2004	Created
        ///     [hansvk]        5/22/2007   Adjusted for event conventions
        /// </history>
        /// -----------------------------------------------------------------------------
        protected static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (FileCreated != null)
                FileCreated(sender, new FileNotifyEventArgs(GetFileName(e.FullPath), e.FullPath));
        }
    }

    #endregion
}