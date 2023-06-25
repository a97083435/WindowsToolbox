﻿using FileRenamer.Extensions.DataType.Enums;
using FileRenamer.Helpers.Root;
using FileRenamer.Services.Root;
using System;
using System.Text;
using System.Windows.Forms;
using Windows.UI.Xaml.Hosting;

namespace FileRenamer
{
    public sealed partial class App : Windows.UI.Xaml.Application
    {
        private WindowsXamlManager WindowsXamlManager { get; set; }

        public App()
        {
            WindowsXamlManager = WindowsXamlManager.InitializeForCurrentThread();
            InitializeComponent();
            UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// 处理应用程序未知异常处理
        /// </summary>
        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs args)
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            stringBuilder1.AppendLine("HelpLink:" + args.Exception.HelpLink);
            stringBuilder1.AppendLine("HResult:" + args.Exception.HResult);
            stringBuilder1.AppendLine("Message:" + args.Exception.Message);
            stringBuilder1.AppendLine("Source:" + args.Exception.Source);
            stringBuilder1.AppendLine("StackTrace:" + args.Exception.StackTrace);

            DialogResult Result = MessageBox.Show(
                ResourceService.GetLocalized("MessageInfo/Title") + Environment.NewLine +
                ResourceService.GetLocalized("MessageInfo/Content1") + Environment.NewLine +
                ResourceService.GetLocalized("MessageInfo/Content2"),
                ResourceService.GetLocalized("Resources/AppDisplayName"),
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error
                );

            // 复制异常信息到剪贴板
            if (Result == DialogResult.OK)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("HelpLink:" + args.Exception.HelpLink);
                stringBuilder.AppendLine("HResult:" + args.Exception.HResult);
                stringBuilder.AppendLine("Message:" + args.Exception.Message);
                stringBuilder.AppendLine("Source:" + args.Exception.Source);
                stringBuilder.AppendLine("StackTrace:" + args.Exception.StackTrace);

                CopyPasteHelper.CopyToClipBoard(stringBuilder.ToString());
            }

            return;
        }

        /// <summary>
        /// 关闭应用并释放所有资源
        /// </summary>
        public void CloseApp()
        {
            WindowsXamlManager?.Dispose();
            WindowsXamlManager = null;
            Environment.Exit(Convert.ToInt32(AppExitCode.Successfully));
        }
    }
}
