﻿using System.ComponentModel;

namespace WindowsTools.Models
{
    /// <summary>
    /// 文件属性模型
    /// </summary>
    public class OldAndNewPropertiesModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        private string _fileName;

        public string FileName
        {
            get { return _fileName; }

            set
            {
                if (!Equals(_fileName, value))
                {
                    _fileName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
                }
            }
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }

            set
            {
                if (!Equals(_filePath, value))
                {
                    _filePath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
                }
            }
        }

        /// <summary>
        /// 文件属性
        /// </summary>
        private string _fileProperties;

        public string FileProperties
        {
            get { return _fileProperties; }

            set
            {
                if (!Equals(_fileProperties, value))
                {
                    _fileProperties = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileProperties)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
