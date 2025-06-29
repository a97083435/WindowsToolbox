﻿using PowerToolboxShellExtension.Helpers.Root;

namespace PowerToolboxShellExtension.Services.Root
{
    /// <summary>
    /// 应用本地设置服务
    /// </summary>
    public static class LocalSettingsService
    {
        private static readonly string settingsKey = @"Software\PowerToolboxs\Settings";

        /// <summary>
        /// 读取设置选项存储信息
        /// </summary>
        public static T ReadSetting<T>(string key)
        {
            return RegistryHelper.ReadRegistryKey<T>(settingsKey, key);
        }
    }
}