﻿using FileRenamer.Helpers.Controls;
using FileRenamer.Helpers.Root;
using FileRenamer.Services.Controls.Settings;
using FileRenamer.Services.Root;
using FileRenamer.Services.Window;
using FileRenamer.UI.Dialogs;
using FileRenamer.Views.Pages;
using FileRenamer.WindowsAPI.PInvoke.DwmApi;
using FileRenamer.WindowsAPI.PInvoke.User32;
using FileRenamer.WindowsAPI.PInvoke.Uxtheme;
using Mile.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace FileRenamer.Views.Forms
{
    /// <summary>
    /// 应用主界面
    /// </summary>
    public class MainForm : Form
    {
        private int windowWidth = 1000;
        private int windowHeight = 700;

        private IContainer components = null;
        private WindowsXamlHost MileXamlHost = new WindowsXamlHost();

        public MainPage MainPage { get; } = new MainPage();

        public DisplayInformation DisplayInformation { get; } = DisplayInformation.GetForCurrentView();

        public MainForm()
        {
            InitializeComponent();

            Controls.Add(MileXamlHost);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
            MaximizeBox = false;
            Size = new Size(Convert.ToInt32(windowWidth * DisplayInformation.RawPixelsPerViewPixel), Convert.ToInt32(windowHeight * DisplayInformation.RawPixelsPerViewPixel));
            StartPosition = FormStartPosition.CenterParent;
            Text = ResourceService.GetLocalized("Resources/AppDisplayName");

            DisplayInformation.DpiChanged += OnDpiChanged;

            MileXamlHost.AutoSize = true;
            MileXamlHost.Dock = DockStyle.Fill;
            MileXamlHost.Child = MainPage;
        }

        private void InitializeComponent()
        {
            components = new Container();
            AutoScaleMode = AutoScaleMode.Font;
        }

        /// <summary>
        /// 处置由 MainForm 窗体占用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 关闭窗口时恢复默认状态
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs args)
        {
            base.OnFormClosing(args);
            AllowTransparency = false;
            DisplayInformation.DpiChanged -= OnDpiChanged;
        }

        /// <summary>
        /// 窗体程序加载时初始化应用程序设置
        /// </summary>
        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);

            ThemeService.SetWindowTheme();
            BackdropService.SetAppBackdrop(Handle);
            TopMostService.SetAppTopMost();

            SetAppTheme();
        }

        /// <summary>
        /// 窗体移动时关闭浮出窗口
        /// </summary>
        protected override void OnMove(EventArgs args)
        {
            base.OnMove(args);
            if (MainPage.XamlRoot is not null)
            {
                IReadOnlyList<Popup> PopupRoot = VisualTreeHelper.GetOpenPopupsForXamlRoot(MainPage.XamlRoot);
                foreach (Popup popup in PopupRoot)
                {
                    // 关闭浮出控件
                    if (popup.Child as FlyoutPresenter is not null)
                    {
                        popup.IsOpen = false;
                        break;
                    }

                    // 关闭菜单浮出控件
                    if (popup.Child as MenuFlyoutPresenter is not null)
                    {
                        popup.IsOpen = false;
                        break;
                    }

                    // 关闭组合框弹出控件
                    if (popup.Child as Canvas is not null)
                    {
                        popup.IsOpen = false;
                        break;
                    }

                    // 关闭日期选择器浮出控件
                    if (popup.Child as DatePickerFlyoutPresenter is not null)
                    {
                        popup.IsOpen = false;
                        break;
                    }

                    // 关闭时间选择器浮出控件
                    if (popup.Child as TimePickerFlyoutPresenter is not null)
                    {
                        popup.IsOpen = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 处理 Windows 消息
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // 系统设置发生变化时的消息
                case (int)WindowMessage.WM_SETTINGCHANGE:
                    {
                        SetAppTheme();
                        RefreshWindowState();
                        break;
                    }
                // 当用户按下鼠标左键时，光标位于窗口的非工作区内的消息
                case (int)WindowMessage.WM_NCLBUTTONDOWN:
                    {
                        if (MainPage.TitlebarMenuFlyout.IsOpen)
                        {
                            MainPage.TitlebarMenuFlyout.Hide();
                        }
                        break;
                    }
                // 当用户按下鼠标右键时，光标位于窗口的非工作区内的消息
                case (int)WindowMessage.WM_NCRBUTTONDOWN:
                    {
                        Point ms = MousePosition;
                        FlyoutShowOptions options = new FlyoutShowOptions();
                        options.Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft;
                        options.ShowMode = FlyoutShowMode.Standard;
                        options.Position = InfoHelper.SystemVersion.Build >= 22000 ?
                            new Windows.Foundation.Point(
                                (ms.X - Location.X - 8) / DisplayInformation.RawPixelsPerViewPixel,
                                (ms.Y - Location.Y - 32) / DisplayInformation.RawPixelsPerViewPixel) :
                            new Windows.Foundation.Point(ms.X - Location.X - 8, ms.Y - Location.Y - 32);
                        MainPage.TitlebarMenuFlyout.ShowAt(null, options);
                        return;
                    }
                case (int)WindowMessage.WM_COPYDATA:
                    {
                        COPYDATASTRUCT copyDataStruct = Marshal.PtrToStructure<COPYDATASTRUCT>(m.LParam);

                        if (copyDataStruct.lpData is "AppIsRunning")
                        {
                            BeginInvoke(async () =>
                            {
                                await ContentDialogHelper.ShowAsync(new AppRunningDialog(), MainPage);
                            });
                        }
                        else if (copyDataStruct.lpData is "FileName")
                        {
                            NavigationService.NavigateTo(typeof(FileNamePage));
                        }
                        else if (copyDataStruct.lpData is "ExtensionName")
                        {
                            NavigationService.NavigateTo(typeof(ExtensionNamePage));
                        }
                        else if (copyDataStruct.lpData is "UpperAndLowerCase")
                        {
                            NavigationService.NavigateTo(typeof(UpperAndLowerCasePage));
                        }
                        else if (copyDataStruct.lpData is "FileProperties")
                        {
                            NavigationService.NavigateTo(typeof(FilePropertiesPage));
                        }
                        break;
                    };
                // 选择窗口右键菜单的条目时接收到的消息
                case (int)WindowMessage.WM_SYSCOMMAND:
                    {
                        SystemCommand sysCommand = (SystemCommand)(m.WParam.ToInt32() & 0xFFF0);

                        if (sysCommand is SystemCommand.SC_MOUSEMENU || sysCommand is SystemCommand.SC_KEYMENU)
                        {
                            FlyoutShowOptions options = new FlyoutShowOptions();
                            options.Position = new Windows.Foundation.Point(0, 0);
                            options.ShowMode = FlyoutShowMode.Standard;
                            MainPage.TitlebarMenuFlyout.ShowAt(null, options);
                            return;
                        }
                        break;
                    }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// 设备的每英寸像素 (PPI) 显示更改修改后触发的事件
        /// </summary>
        public void OnDpiChanged(DisplayInformation sender, object args)
        {
            Size = new Size(
            Convert.ToInt32(windowWidth * sender.RawPixelsPerViewPixel),
            Convert.ToInt32(windowHeight * sender.RawPixelsPerViewPixel)
            );
        }

        /// <summary>
        /// 设置应用的主题色
        /// </summary>
        public void SetAppTheme()
        {
            if (ThemeService.AppTheme.SelectedValue == ThemeService.ThemeList[0].SelectedValue)
            {
                if (Windows.UI.Xaml.Application.Current.RequestedTheme is ApplicationTheme.Light)
                {
                    int useLightMode = 0;
                    DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useLightMode, Marshal.SizeOf(typeof(int)));
                    UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceLight);
                    UxthemeLibrary.FlushMenuThemes();

                    RefreshWindowState();
                }
                else
                {
                    int useDarkMode = 1;
                    DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, Marshal.SizeOf(typeof(int)));
                    UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceDark);
                    UxthemeLibrary.FlushMenuThemes();

                    RefreshWindowState();
                }
            }
            if (ThemeService.AppTheme.SelectedValue == ThemeService.ThemeList[1].SelectedValue)
            {
                int useLightMode = 0;
                DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useLightMode, Marshal.SizeOf(typeof(int)));
                UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceLight);
                UxthemeLibrary.FlushMenuThemes();

                RefreshWindowState();
            }
            else if (ThemeService.AppTheme.SelectedValue == ThemeService.ThemeList[2].SelectedValue)
            {
                int useDarkMode = 1;
                DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, Marshal.SizeOf(typeof(int)));
                UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceDark);
                UxthemeLibrary.FlushMenuThemes();

                RefreshWindowState();
            }
        }

        /// <summary>
        /// 添加窗口背景色
        /// </summary>
        public void SetWindowBackdrop()
        {
            if (BackdropService.AppBackdrop.SelectedValue == BackdropService.BackdropList[0].SelectedValue)
            {
                int noBackdrop = 1;
                DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref noBackdrop, Marshal.SizeOf(typeof(int)));

                RefreshWindowState();
            }
            else if (BackdropService.AppBackdrop.SelectedValue == BackdropService.BackdropList[1].SelectedValue)
            {
                int micaBackdrop = 2;
                DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref micaBackdrop, Marshal.SizeOf(typeof(int)));

                RefreshWindowState();
            }
            else if (BackdropService.AppBackdrop.SelectedValue == BackdropService.BackdropList[2].SelectedValue)
            {
                int micaAltBackdrop = 4;
                DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref micaAltBackdrop, Marshal.SizeOf(typeof(int)));

                RefreshWindowState();
            }
            else if (BackdropService.AppBackdrop.SelectedValue == BackdropService.BackdropList[3].SelectedValue)
            {
                int acrylicBackdrop = 3;
                DwmApiLibrary.DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref acrylicBackdrop, Marshal.SizeOf(typeof(int)));

                RefreshWindowState();
            }
        }

        /// <summary>
        /// 当主题色或背景色发生改变时，刷新窗体的状态
        /// </summary>
        public void RefreshWindowState()
        {
            if (BackdropService.AppBackdrop.SelectedValue == BackdropService.BackdropList[0].SelectedValue)
            {
                if (ThemeService.AppTheme.SelectedValue == ThemeService.ThemeList[0].SelectedValue)
                {
                    if (Windows.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Light)
                    {
                        TransparencyKey = BackColor = Color.White;
                        AllowTransparency = false;
                    }
                    else
                    {
                        TransparencyKey = BackColor = Color.Black;
                        AllowTransparency = false;
                    }
                }
                else if (ThemeService.AppTheme.SelectedValue == ThemeService.ThemeList[1].SelectedValue)
                {
                    TransparencyKey = BackColor = Color.White;
                    AllowTransparency = false;
                }
                else if (ThemeService.AppTheme.SelectedValue == ThemeService.ThemeList[2].SelectedValue)
                {
                    TransparencyKey = BackColor = Color.Black;
                    AllowTransparency = false;
                }
            }
            else
            {
                AllowTransparency = true;
                BackColor = Color.FromArgb(255, 255, 254);
                TransparencyKey = Color.FromArgb(255, 255, 254);
            }
        }
    }
}
