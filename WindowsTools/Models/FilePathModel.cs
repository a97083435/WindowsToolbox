﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsTools.Models
{
    /// <summary>
    /// 包文件索引文件路径数据模型
    /// </summary>
    public class FilePathModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 是否已选择
        /// </summary>
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }

            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 文件路径对应的键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 文件路径对应的索引
        /// </summary>
        public uint FilePathIndex { get; set; }

        /// <summary>
        /// 文件路径对应的绝对路径
        /// </summary>
        public string AbsolutePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性值发生变化时通知更改
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
