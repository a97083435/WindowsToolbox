﻿using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace FileRenamer.Helpers.Root
{
    /// <summary>
    /// 应用资源字典
    /// </summary>
    public static class ResourceDictionaryHelper
    {
        public static ResourceDictionary HyperlinkButtonResourceDict { get; private set; }

        public static ResourceDictionary InAppNotificationResourceDict { get; private set; }

        public static ResourceDictionary ListViewResourceDict { get; private set; }

        public static ResourceDictionary MenuFlyoutResourceDict { get; private set; }

        public static ResourceDictionary ScrollBarResourceDict { get; private set; }

        /// <summary>
        /// 初始化资源字典信息
        /// </summary>
        public static void InitializeResourceDictionary()
        {
            XamlControlsResources xamlControlsResources = Application.Current.Resources as XamlControlsResources;

            HyperlinkButtonResourceDict = xamlControlsResources.MergedDictionaries[0];
            InAppNotificationResourceDict = xamlControlsResources.MergedDictionaries[1];
            ListViewResourceDict = xamlControlsResources.MergedDictionaries[2];
            MenuFlyoutResourceDict = xamlControlsResources.MergedDictionaries[3];
            ScrollBarResourceDict = xamlControlsResources.MergedDictionaries[4];
        }
    }
}
