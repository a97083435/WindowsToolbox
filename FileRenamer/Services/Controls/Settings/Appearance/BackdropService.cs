﻿using FileRenamer.Extensions.DataType.Constant;
using FileRenamer.Models.Controls.Settings.Appearance;
using FileRenamer.Services.Root;
using FileRenamer.WindowsAPI.PInvoke.DwmApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileRenamer.Services.Controls.Settings.Appearance
{
    /// <summary>
    /// 应用背景色设置服务
    /// </summary>
    public static class BackdropService
    {
        private static string SettingsKey { get; } = ConfigKey.BackdropKey;

        private static BackdropModel DefaultAppBackdrop { get; set; }

        public static BackdropModel AppBackdrop { get; set; }

        public static List<BackdropModel> BackdropList { get; set; }

        /// <summary>
        /// 应用在初始化前获取设置存储的背景色值
        /// </summary>
        public static async Task InitializeBackdropAsync()
        {
            BackdropList = ResourceService.BackdropList;

            DefaultAppBackdrop = BackdropList.Find(item => item.InternalName.Equals("Default", StringComparison.OrdinalIgnoreCase));

            AppBackdrop = await GetBackdropAsync();
        }

        /// <summary>
        /// 获取设置存储的背景色值，如果设置没有存储，使用默认值
        /// </summary>
        private static async Task<BackdropModel> GetBackdropAsync()
        {
            string backdrop = await ConfigService.ReadSettingAsync<string>(SettingsKey);

            if (string.IsNullOrEmpty(backdrop))
            {
                return BackdropList.Find(item => item.InternalName.Equals(DefaultAppBackdrop.InternalName, StringComparison.OrdinalIgnoreCase));
            }

            return BackdropList.Find(item => item.InternalName.Equals(backdrop, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 应用背景色发生修改时修改设置存储的背景色值
        /// </summary>
        public static async Task SetBackdropAsync(BackdropModel backdrop)
        {
            AppBackdrop = backdrop;

            await ConfigService.SaveSettingAsync(SettingsKey, backdrop.InternalName);
        }

        /// <summary>
        /// 设置应用显示的背景色
        /// </summary>
        public static void SetAppBackdrop()
        {
            if(AppBackdrop.InternalName == BackdropList[0].InternalName)
            {
                int noBackdrop = 1;
                DwmApiLibrary.DwmSetWindowAttribute(Program.MainWindow.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE,ref noBackdrop, Marshal.SizeOf(typeof(int)));
            }
            else if (AppBackdrop.InternalName == BackdropList[1].InternalName)
            {
                int micaBackdrop = 2;
                DwmApiLibrary.DwmSetWindowAttribute(Program.MainWindow.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref micaBackdrop, Marshal.SizeOf(typeof(int)));
            }
            else if (AppBackdrop.InternalName == BackdropList[2].InternalName)
            {
                int micaAltBackdrop = 4;
                DwmApiLibrary.DwmSetWindowAttribute(Program.MainWindow.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref micaAltBackdrop, Marshal.SizeOf(typeof(int)));
            }
            else if (AppBackdrop.InternalName == BackdropList[3].InternalName)
            {
                int acrylicBackdrop = 3;
                DwmApiLibrary.DwmSetWindowAttribute(Program.MainWindow.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref acrylicBackdrop, Marshal.SizeOf(typeof(int)));
            }
        }
    }
}
