﻿using FileRenamer.Contracts;
using FileRenamer.Extensions.Command;
using FileRenamer.UI.Dialogs.Settings;
using System;
using Windows.System;

namespace FileRenamer.ViewModels.Pages
{
    /// <summary>
    /// 设置页面数据模型
    /// </summary>
    public sealed class SettingsViewModel
    {
        // 打开重启应用确认的窗口对话框
        public IRelayCommand RestartCommand = new RelayCommand(async () =>
        {
            await new RestartAppsDialog().ShowAsync();
        });

        // 打开系统主题设置
        public IRelayCommand SettingsColorCommand => new RelayCommand(async () =>
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
        });
    }
}