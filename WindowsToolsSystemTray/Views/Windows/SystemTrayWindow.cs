using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using WindowsToolsSystemTray.Helpers.Root;
using WindowsToolsSystemTray.Services.Controls.Settings;
using WindowsToolsSystemTray.Services.Root;
using WindowsToolsSystemTray.Views.Pages;
using WindowsToolsSystemTray.WindowsAPI.ComTypes;
using WindowsToolsSystemTray.WindowsAPI.PInvoke.User32;

// 抑制 CA1806，CA1822 警告
#pragma warning disable CA1806,CA1822

namespace WindowsToolsSystemTray.Views.Windows
{
    /// <summary>
    /// 托盘程序辅助窗口
    /// </summary>
    public class SystemTrayWindow : Form
    {
        private readonly string title = ResourceService.SystemTrayResource.GetString("Title");
        private readonly string currentSystemTheme = ResourceService.SystemTrayResource.GetString("SystemTheme");
        private readonly string currentAppTheme = ResourceService.SystemTrayResource.GetString("AppTheme");
        private readonly string light = ResourceService.SystemTrayResource.GetString("Light");
        private readonly string dark = ResourceService.SystemTrayResource.GetString("Dark");
        private readonly Container container = new();
        private readonly DesktopWindowXamlSource desktopWindowXamlSource = new();

        private readonly System.Timers.Timer timer = new()
        {
            Interval = 1000,
            Enabled = true
        };

        public UIElement Content { get; set; }

        public static SystemTrayWindow Current { get; private set; }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle = (int)(WindowExStyle.WS_EX_TRANSPARENT | WindowExStyle.WS_EX_LAYERED | WindowExStyle.WS_EX_TOOLWINDOW);
                createParams.Style = unchecked((int)WindowStyle.WS_POPUPWINDOW);
                return createParams;
            }
        }

        public SystemTrayWindow()
        {
            AllowDrop = false;
            AutoScaleMode = AutoScaleMode.Font;
            Current = this;
            Content = new SystemTrayPage();

            desktopWindowXamlSource.Content = Content;
            IDesktopWindowXamlSourceNative2 desktopWindowXamlSourceNative = desktopWindowXamlSource as IDesktopWindowXamlSourceNative2;
            desktopWindowXamlSourceNative.AttachToWindow(Handle);
            desktopWindowXamlSource.TakeFocusRequested += OnTakeFocusRequested;

            Icon = Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            StartPosition = FormStartPosition.CenterScreen;
            Text = ResourceService.SystemTrayResource.GetString("WindowTitle");

            RightToLeft = LanguageService.RightToLeft;
            RightToLeftLayout = LanguageService.RightToLeft is RightToLeft.Yes;

            if (RuntimeHelper.IsElevated)
            {
                User32Library.ChangeWindowMessageFilter(WindowMessage.WM_COPYDATA, ChangeFilterFlags.MSGFLT_ADD);
                User32Library.ChangeWindowMessageFilter(WindowMessage.WM_COPYGLOBALDATA, ChangeFilterFlags.MSGFLT_ADD);
            }

            SystemTrayService.RightClick += OnRightClick;
            timer.Elapsed += OnElapsed;

            Task.Run(() =>
            {
                ElementTheme systemTheme = GetSystemTheme();
                ElementTheme appTheme = GetAppTheme();

                string notifyIconTitle = string.Join(Environment.NewLine, title, string.Format(currentSystemTheme, systemTheme is ElementTheme.Light ? light : dark), string.Format(currentAppTheme, appTheme is ElementTheme.Light ? light : dark));
                SystemTrayService.UpdateTitle(notifyIconTitle);
            });
        }

        #region 第一部分：窗口类内置需要重载的事件

        /// <summary>
        /// 处置由主窗体占用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && container is not null)
            {
                container.Dispose();
            }
        }

        /// <summary>
        /// 窗口不处于焦点状态时触发的事件
        /// </summary>
        protected override void OnDeactivate(EventArgs args)
        {
            base.OnDeactivate(args);

            if ((Content as SystemTrayPage).SystemTrayFlyout.IsOpen)
            {
                (Content as SystemTrayPage).SystemTrayFlyout.Hide();
            }
        }

        /// <summary>
        /// 关闭窗口后释放资源
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs args)
        {
            base.OnFormClosed(args);
            desktopWindowXamlSource.TakeFocusRequested -= OnTakeFocusRequested;
            timer.Elapsed -= OnElapsed;
            SystemTrayService.RightClick -= OnRightClick;
            desktopWindowXamlSource.Dispose();
            timer.Dispose();

            if (RuntimeHelper.IsElevated)
            {
                User32Library.ChangeWindowMessageFilter(WindowMessage.WM_COPYDATA, ChangeFilterFlags.MSGFLT_REMOVE);
                User32Library.ChangeWindowMessageFilter(WindowMessage.WM_COPYGLOBALDATA, ChangeFilterFlags.MSGFLT_REMOVE);
            }

            Current = null;
            (global::Windows.UI.Xaml.Application.Current as SystemTrayApp).Dispose();
        }

        #endregion 第一部分：窗口类内置需要重载的事件

        #region 第二部分：自定义事件

        /// <summary>
        /// 当主机桌面应用程序收到从 DesktopWindowXamlSource 对象 (获取焦点的请求时发生，用户位于 DesktopWindowXamlSource 中的最后一个可聚焦元素上，然后按 Tab) 。
        /// </summary>
        private void OnTakeFocusRequested(DesktopWindowXamlSource sender, DesktopWindowXamlSourceTakeFocusRequestedEventArgs args)
        {
            XamlSourceFocusNavigationReason reason = args.Request.Reason;

            if (reason < XamlSourceFocusNavigationReason.Left)
            {
                sender.NavigateFocus(args.Request);
            }
        }

        /// <summary>
        /// 处理托盘鼠标右键单击事件
        /// </summary>
        private void OnRightClick(object sender, MouseEventArgs args)
        {
            FlyoutShowOptions options = new()
            {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft,
                ShowMode = FlyoutShowMode.Standard,
                Position = InfoHelper.SystemVersion.Build >= 22000
                        ? new global::Windows.Foundation.Point(MousePosition.X / ((double)DeviceDpi / 96), MousePosition.Y / ((double)DeviceDpi / 96))
                        : new global::Windows.Foundation.Point(MousePosition.X, MousePosition.Y)
            };

            Activate();
            (Content as SystemTrayPage).SystemTrayFlyout.ShowAt(Content, options);
        }

        /// <summary>
        /// 时间流逝触发的事件
        /// </summary>
        private void OnElapsed(object sender, ElapsedEventArgs args)
        {
            TimeSpan currentTime = new(DateTime.Now.Hour, DateTime.Now.Minute, 0);

            // 已启用自动切换主题
            if (AutoSwitchThemeService.AutoSwitchThemeEnableValue)
            {
                // 自动切换系统主题
                if (AutoSwitchThemeService.AutoSwitchSystemThemeValue)
                {
                    // 白天时间小于夜间时间
                    if (AutoSwitchThemeService.SystemThemeLightTime < AutoSwitchThemeService.SystemThemeDarkTime)
                    {
                        // 介于白天时间和夜间时间，切换浅色主题
                        if (currentTime > AutoSwitchThemeService.SystemThemeLightTime && currentTime < AutoSwitchThemeService.SystemThemeDarkTime)
                        {
                            bool isModified = false;

                            if (GetSystemTheme() is ElementTheme.Dark)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1);
                                isModified = true;
                            }

                            if (RegistryHelper.ReadRegistryKey<int>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence") is 1)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence", 0);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                        // 切换深色主题
                        else
                        {
                            bool isModified = false;

                            if (GetSystemTheme() is ElementTheme.Light)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0);
                                isModified = true;
                            }

                            if (AutoSwitchThemeService.IsShowColorInDarkThemeValue && RegistryHelper.ReadRegistryKey<int>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence") is 0)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence", 1);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                    }
                    // 白天时间大于夜间时间
                    else
                    {
                        // 介于白天时间和夜间时间，切换深色主题
                        if (currentTime > AutoSwitchThemeService.SystemThemeDarkTime && currentTime < AutoSwitchThemeService.SystemThemeLightTime)
                        {
                            bool isModified = false;

                            if (GetAppTheme() is ElementTheme.Light)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0);
                                isModified = true;
                            }

                            if (AutoSwitchThemeService.IsShowColorInDarkThemeValue && RegistryHelper.ReadRegistryKey<int>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence") is 0)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence", 1);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                        // 切换浅色主题
                        else
                        {
                            bool isModified = false;

                            if (GetSystemTheme() is ElementTheme.Dark)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1);
                                isModified = true;
                            }

                            if (RegistryHelper.ReadRegistryKey<int>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence") is 1)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence", 0);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                    }
                }

                // 自动切换应用主题
                if (AutoSwitchThemeService.AutoSwitchAppThemeValue)
                {
                    // 白天时间小于夜间时间
                    if (AutoSwitchThemeService.AppThemeLightTime < AutoSwitchThemeService.AppThemeDarkTime)
                    {
                        // 介于白天时间和夜间时间，切换浅色主题
                        if (currentTime > AutoSwitchThemeService.AppThemeLightTime && currentTime < AutoSwitchThemeService.AppThemeDarkTime)
                        {
                            bool isModified = false;

                            if (GetAppTheme() is ElementTheme.Dark)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                        // 切换深色主题
                        else
                        {
                            bool isModified = false;

                            if (GetAppTheme() is ElementTheme.Light)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                    }
                    // 白天时间大于夜间时间
                    else
                    {
                        // 介于白天时间和夜间时间，切换深色主题
                        if (currentTime > AutoSwitchThemeService.AppThemeDarkTime && currentTime < AutoSwitchThemeService.AppThemeLightTime)
                        {
                            bool isModified = false;

                            if (GetAppTheme() is ElementTheme.Light)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                        // 切换浅色主题
                        else
                        {
                            bool isModified = false;

                            if (GetAppTheme() is ElementTheme.Dark)
                            {
                                RegistryHelper.SaveRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
                                isModified = true;
                            }

                            if (isModified)
                            {
                                User32Library.SendMessageTimeout(new IntPtr(0xffff), WindowMessage.WM_SETTINGCHANGE, UIntPtr.Zero, Marshal.StringToHGlobalUni("ImmersiveColorSet"), SMTO.SMTO_ABORTIFHUNG, 50, out _);
                            }
                        }
                    }
                }
            }
            else
            {
                Close();
            }

            ElementTheme systemTheme = GetSystemTheme();
            ElementTheme appTheme = GetAppTheme();

            string notifyIconTitle = string.Join(Environment.NewLine, title, string.Format(currentSystemTheme, systemTheme is ElementTheme.Light ? light : dark), string.Format(currentAppTheme, appTheme is ElementTheme.Light ? light : dark));
            SystemTrayService.UpdateTitle(notifyIconTitle);
        }

        #endregion 第二部分：自定义事件

        /// <summary>
        /// 应用主窗口消息处理
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                // 应用主题设置跟随系统发生变化时，当系统主题设置发生变化时修改修改应用背景色
                case (int)WindowMessage.WM_SETTINGCHANGE:
                    {
                        BeginInvoke(async () =>
                        {
                            await (Content as SystemTrayPage).UpdateSystemTrayThemeAsync();
                        });
                        break;
                    }
                // 从其他应用程序接收到的消息
                case (int)WindowMessage.WM_COPYDATA:
                    {
                        COPYDATASTRUCT copyDataStruct = Marshal.PtrToStructure<COPYDATASTRUCT>(m.LParam);

                        if (copyDataStruct.lpData.Equals("Auto switch theme settings changed"))
                        {
                            Task.Run(AutoSwitchThemeService.InitializeAutoSwitchTheme);
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// 获取系统主题样式
        /// </summary>
        private ElementTheme GetSystemTheme()
        {
            return RegistryHelper.ReadRegistryKey<bool>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme") ? ElementTheme.Light : ElementTheme.Dark;
        }

        /// <summary>
        /// 获取应用主题样式
        /// </summary>
        private ElementTheme GetAppTheme()
        {
            return RegistryHelper.ReadRegistryKey<bool>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme") ? ElementTheme.Light : ElementTheme.Dark;
        }
    }
}
