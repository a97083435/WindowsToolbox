﻿using FileRenamer.Models;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace FileRenamer.Services.Window
{
    /// <summary>
    /// 应用导航服务
    /// </summary>
    public static class NavigationService
    {
        private static SlideNavigationTransitionInfo NavigationTransition = new SlideNavigationTransitionInfo()
        {
            Effect = SlideNavigationTransitionEffect.FromRight
        };

        public static Frame NavigationFrame { get; set; }

        public static List<NavigationModel> NavigationItemList { get; private set; } = new List<NavigationModel>();

        /// <summary>
        /// 页面向前导航
        /// </summary>
        public static void NavigateTo(Type navigationPageType, object parameter = null)
        {
            if (NavigationItemList.Exists(item => item.NavigationPage == navigationPageType))
            {
                NavigationFrame.Navigate(NavigationItemList.Find(item => item.NavigationPage == navigationPageType).NavigationPage, parameter, NavigationTransition);
            }
        }

        /// <summary>
        /// 页面向后导航
        /// </summary>
        public static void NavigationFrom()
        {
            if (NavigationFrame.CanGoBack)
            {
                NavigationFrame.GoBack();
            }
        }

        /// <summary>
        /// 获取当前导航到的页
        /// </summary>
        public static Type GetCurrentPageType()
        {
            return NavigationFrame.CurrentSourcePageType;
        }

        /// <summary>
        /// 检查当前页面是否能向后导航
        /// </summary>
        public static bool CanGoBack()
        {
            return NavigationFrame.CanGoBack;
        }
    }
}
