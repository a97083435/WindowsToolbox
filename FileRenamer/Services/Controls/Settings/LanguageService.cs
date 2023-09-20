﻿using FileRenamer.Extensions.DataType.Constant;
using FileRenamer.Models;
using FileRenamer.Services.Root;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace FileRenamer.Services.Controls.Settings
{
    /// <summary>
    /// 应用语言设置服务
    /// </summary>
    public static class LanguageService
    {
        private static string resourceFileName = string.Format("{0}.resources.dll", Assembly.GetExecutingAssembly().GetName().Name);

        private static string SettingsKey { get; } = ConfigKey.LanguageKey;

        public static GroupOptionsModel DefaultAppLanguage { get; set; }

        public static GroupOptionsModel AppLanguage { get; set; }

        private static List<string> AppLanguagesList { get; } = new List<string>();

        public static List<GroupOptionsModel> LanguageList { get; set; } = new List<GroupOptionsModel>();

        /// <summary>
        /// 初始化应用语言信息列表
        /// </summary>
        private static void InitializeLanguageList()
        {
            try
            {
                string[] resourceFolder = Directory.GetFiles(AppContext.BaseDirectory, resourceFileName, SearchOption.AllDirectories);

                foreach (string file in resourceFolder)
                {
                    AppLanguagesList.Add(Path.GetFileName(Path.GetDirectoryName(file)));
                }
            }
            catch
            {
                AppLanguagesList.Clear();
                AppLanguagesList.Add("en-us");
            }

            foreach (string applanguage in AppLanguagesList)
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(applanguage);

                LanguageList.Add(new GroupOptionsModel()
                {
                    DisplayMember = culture.NativeName,
                    SelectedValue = culture.Name,
                });
            }
        }

        /// <summary>
        /// 当设置中的键值为空时，判断当前系统语言是否存在于语言列表中
        /// </summary>
        private static bool IsExistsInLanguageList(string currentSystemLanguage)
        {
            foreach (GroupOptionsModel languageItem in LanguageList)
            {
                if (languageItem.SelectedValue == currentSystemLanguage)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 应用在初始化前获取设置存储的语言值，如果设置值为空，设定默认的应用语言值
        /// </summary>
        public static void InitializeLanguage()
        {
            InitializeLanguageList();

            DefaultAppLanguage = LanguageList.Find(item => item.SelectedValue.Equals("en-US", StringComparison.OrdinalIgnoreCase));

            AppLanguage = GetLanguage();
        }

        /// <summary>
        /// 获取设置存储的语言值，如果设置没有存储，使用默认值
        /// </summary>
        private static GroupOptionsModel GetLanguage()
        {
            string language = ConfigService.ReadSetting<string>(SettingsKey);

            // 当前系统的语言值
            string CurrentSystemLanguage = CultureInfo.CurrentCulture.Parent.Parent.Name;

            if (string.IsNullOrEmpty(language))
            {
                // 判断当前系统语言是否存在应用默认添加的语言列表中
                bool result = IsExistsInLanguageList(CurrentSystemLanguage);

                // 如果存在，设置存储值和应用初次设置的语言为当前系统的语言
                if (result)
                {
                    GroupOptionsModel currentSystemLanguage = LanguageList.Find(item => item.SelectedValue.Equals(CurrentSystemLanguage, StringComparison.OrdinalIgnoreCase));
                    SetLanguage(currentSystemLanguage);
                    return currentSystemLanguage;
                }

                // 不存在，设置存储值和应用初次设置的语言为默认语言：English(United States)
                else
                {
                    SetLanguage(DefaultAppLanguage);
                    return LanguageList.Find(item => item.SelectedValue.Equals(DefaultAppLanguage.SelectedValue, StringComparison.OrdinalIgnoreCase));
                }
            }

            return LanguageList.Find(item => item.SelectedValue.Equals(language, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 语言发生修改时修改设置存储的语言值
        /// </summary>
        public static void SetLanguage(GroupOptionsModel language)
        {
            ConfigService.SaveSetting(SettingsKey, language.SelectedValue);
        }
    }
}
