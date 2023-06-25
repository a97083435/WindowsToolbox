﻿using FileRenamer.Services.Root;
using GetStoreApp.Services.Root;
using Windows.UI.Xaml.Controls;

namespace FileRenamer.Views.Pages
{
    /// <summary>
    /// 文件名称页面
    /// </summary>
    public sealed partial class FileNamePage : Page
    {
        public FileNamePage()
        {
            InitializeComponent();
        }

        public string LocalizeTotal(int count)
        {
            return string.Format(ResourceService.GetLocalized("FileName/Total"), ViewModel.FileNameDataList.Count);
        }

        public bool IsItemChecked(string selectedInternalName, string internalName)
        {
            return selectedInternalName == internalName;
        }

        private void DropDownButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            LogService.WriteLog(LogType.Info, "Test LogService");
        }
    }
}
