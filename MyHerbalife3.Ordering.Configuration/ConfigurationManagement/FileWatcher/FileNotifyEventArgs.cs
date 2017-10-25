using System;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.FileWatcher
{
    /// <summary>
    /// Event arguments for a file change/delete/create event
    /// </summary>
    public class FileNotifyEventArgs : EventArgs
    {
        #region Private members
        private string _fileName;
        private string _filePath;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize private members
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="filePath">The path of the file</param>
        public FileNotifyEventArgs(string fileName, string filePath)
        {
            _fileName = fileName;
            _filePath = filePath;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// The path of the file that is changed/deleted/created
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        /// <summary>
        /// The name of the file that is changed/deleted/created
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        #endregion
    }
}
