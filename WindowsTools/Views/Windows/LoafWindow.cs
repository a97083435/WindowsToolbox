﻿using Mile.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsTools.Extensions.DataType.Enums;
using WindowsTools.Helpers.Root;
using WindowsTools.Services.Controls.Settings;
using WindowsTools.Services.Root;
using WindowsTools.Views.Pages;
using WindowsTools.WindowsAPI.PInvoke.User32;

namespace WindowsTools.Views.Windows
{
    /// <summary>
    /// 摸鱼窗口
    /// </summary>
    public class LoafWindow : Form
    {
        private readonly bool _blockAllKeys = false;
        private readonly bool _lockScreenAutomaticly = false;
        private readonly IContainer components = new Container();
        private readonly WindowsXamlHost windowsXamlHost = new();

        private IntPtr hHook = IntPtr.Zero;
        private HOOKPROC KeyBoardHookProc;

        public static LoafWindow Current { get; private set; }

        public LoafWindow(UpdatingKind updatingKind, TimeSpan duration, bool blockAllKeys, bool lockScreenAutomaticly)
        {
            AllowDrop = false;
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            Current = this;
            Controls.Add(windowsXamlHost);
            FormBorderStyle = FormBorderStyle.None;
            RightToLeft = LanguageService.RightToLeft;
            RightToLeftLayout = LanguageService.RightToLeft is RightToLeft.Yes;
            WindowState = FormWindowState.Maximized;
            ShowInTaskbar = false;
            TopMost = true;
            windowsXamlHost.AutoSize = true;
            windowsXamlHost.Dock = DockStyle.Fill;
            _blockAllKeys = blockAllKeys;
            _lockScreenAutomaticly = lockScreenAutomaticly;
            Cursor.Hide();
            windowsXamlHost.Child = new SimulateUpdatePage(updatingKind, duration);
            SystemSleepHelper.PreventForCurrentThread();
            StartHook();
        }

        /// <summary>
        /// 关闭窗体后发生的事件
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs args)
        {
            base.OnFormClosed(args);
            Current = null;
        }

        /// <summary>
        /// 处置由主窗体占用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && components is not null)
            {
                components.Dispose();
            }
        }

        /// <summary>
        /// 添加钩子
        /// </summary>
        private void StartHook()
        {
            try
            {
                // 安装键盘钩子
                if (hHook == IntPtr.Zero)
                {
                    KeyBoardHookProc = new HOOKPROC(OnKeyboardHookProc);

                    hHook = User32Library.SetWindowsHookEx(HOOKTYPE.WH_KEYBOARD_LL, KeyBoardHookProc, Process.GetCurrentProcess().MainModule.BaseAddress, 0);

                    //如果设置钩子失败.
                    if (hHook == IntPtr.Zero)
                    {
                        StopHook();
                    }
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(EventLevel.Error, "Add keyboard hook failed", e);
            }
        }

        /// <summary>
        /// 取消钩子
        /// </summary>
        private void StopHook()
        {
            try
            {
                bool unHookResult = true;
                if (hHook != IntPtr.Zero)
                {
                    unHookResult = User32Library.UnhookWindowsHookEx(hHook);
                }

                if (!unHookResult)
                {
                    throw new Win32Exception();
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(EventLevel.Error, "Remove keyboard hook failed", e);
            }
        }

        /// <summary>
        /// 自定义钩子消息处理
        /// </summary>
        public IntPtr OnKeyboardHookProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            // 处理键盘钩子消息
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kbdllHookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                // Esc 键，退出摸鱼
                if (kbdllHookStruct.vkCode is Keys.Escape)
                {
                    StopLoaf();
                    return new IntPtr(1);
                }

                // 屏蔽所有键盘按键
                if (_blockAllKeys)
                {                // 左 Windows 徽标键
                    if (kbdllHookStruct.vkCode is Keys.LWin)
                    {
                        return new IntPtr(1);
                    }

                    // 右 Windows 徽标键
                    if (kbdllHookStruct.vkCode is Keys.LWin)
                    {
                        return new IntPtr(1);
                    }

                    // Ctrl 和 Esc 组合
                    if (kbdllHookStruct.vkCode is Keys.Escape && ModifierKeys is Keys.Control)
                    {
                        return new IntPtr(1);
                    }

                    // Alt 和 F4 组合
                    if (kbdllHookStruct.vkCode is Keys.F4 && ModifierKeys is Keys.Alt)
                    {
                        return new IntPtr(1);
                    }

                    // Alt 和 Tab 组合
                    if (kbdllHookStruct.vkCode is Keys.Tab && ModifierKeys is Keys.Alt)
                    {
                        return new IntPtr(1);
                    }

                    // Ctrl Shift Esc 组合
                    if (kbdllHookStruct.vkCode is Keys.Escape && ModifierKeys is (Keys.Control | Keys.Shift))
                    {
                        return new IntPtr(1);
                    }

                    // Alt 和 Space 组合
                    if (kbdllHookStruct.vkCode is Keys.Space && ModifierKeys is Keys.Alt)
                    {
                        return new IntPtr(1);
                    }
                }
            }
            return User32Library.CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// 停止摸鱼
        /// </summary>
        public void StopLoaf()
        {
            Cursor.Show();
            StopHook();
            (windowsXamlHost.Child as SimulateUpdatePage).StopSimulateUpdate();
            if (_lockScreenAutomaticly)
            {
                User32Library.LockWorkStation();
            }
            // 恢复此线程曾经阻止的系统休眠和屏幕关闭。
            SystemSleepHelper.RestoreForCurrentThread();
            Close();
            User32Library.keybd_event(Keys.LWin, 0, KEYEVENTFLAGS.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            User32Library.keybd_event(Keys.D, 0, KEYEVENTFLAGS.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            User32Library.keybd_event(Keys.D, 0, KEYEVENTFLAGS.KEYEVENTF_KEYUP, UIntPtr.Zero);
            User32Library.keybd_event(Keys.LWin, 0, KEYEVENTFLAGS.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
