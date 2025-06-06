using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using WindowsTools.Extensions.DataType.Enums;
using WindowsTools.Models;
using WindowsTools.Services.Root;
using WindowsTools.UI.TeachingTips;
using WindowsTools.Views.Windows;
using WindowsTools.WindowsAPI.PInvoke.Rstrtmgr;
using WindowsTools.WindowsAPI.PInvoke.Shell32;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace WindowsTools.Views.Pages
{
    /// <summary>
    /// 文件解锁页面
    /// </summary>
    public sealed partial class FileUnlockPage : Page, INotifyPropertyChanged
    {
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        private string FileNotUse { get; } = ResourceService.FileUnlockResource.GetString("FileNotUse");

        private InfoBarSeverity _resultSeverity = InfoBarSeverity.Informational;

        public InfoBarSeverity ResultSeverity
        {
            get { return _resultSeverity; }

            set
            {
                if (!Equals(_resultSeverity, value))
                {
                    _resultSeverity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultSeverity)));
                }
            }
        }

        private string _stateInfoText = ResourceService.FileUnlockResource.GetString("Welcome");

        public string StateInfoText
        {
            get { return _stateInfoText; }

            set
            {
                if (!Equals(_stateInfoText, value))
                {
                    _stateInfoText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateInfoText)));
                }
            }
        }

        private bool _isRingActive = false;

        public bool IsRingActive
        {
            get { return _isRingActive; }

            set
            {
                if (!Equals(_isRingActive, value))
                {
                    _isRingActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRingActive)));
                }
            }
        }

        private bool _resultCotnrolVisable = false;

        public bool ResultControlVisable
        {
            get { return _resultCotnrolVisable; }

            set
            {
                if (!Equals(_resultCotnrolVisable, value))
                {
                    _resultCotnrolVisable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultControlVisable)));
                }
            }
        }

        private string _fileName = string.Empty;

        public string FileName
        {
            get { return _fileName; }

            set
            {
                if (!Equals(_fileName, value))
                {
                    _fileName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
                }
            }
        }

        private ObservableCollection<ProcessInfoModel> ProcessInfoCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public FileUnlockPage()
        {
            InitializeComponent();
        }

        #region 第一部分：第一部分：重写父类事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        protected override async void OnDragOver(global::Windows.UI.Xaml.DragEventArgs args)
        {
            base.OnDragOver(args);
            DragOperationDeferral deferral = args.GetDeferral();

            IReadOnlyList<IStorageItem> dragItemsList = await args.DataView.GetStorageItemsAsync();

            if (dragItemsList.Count is 1)
            {
                args.AcceptedOperation = DataPackageOperation.Copy;
                args.DragUIOverride.IsCaptionVisible = true;
                args.DragUIOverride.IsContentVisible = false;
                args.DragUIOverride.IsGlyphVisible = true;
                args.DragUIOverride.Caption = ResourceService.FileUnlockResource.GetString("DragOverContent");
            }
            else
            {
                args.AcceptedOperation = DataPackageOperation.None;
                args.DragUIOverride.IsCaptionVisible = true;
                args.DragUIOverride.IsContentVisible = false;
                args.DragUIOverride.IsGlyphVisible = true;
                args.DragUIOverride.Caption = ResourceService.FileUnlockResource.GetString("NoMultiFile");
            }

            args.Handled = true;
            deferral.Complete();
        }

        /// <summary>
        /// 拖动文件完成后获取文件信息
        /// </summary>
        protected override async void OnDrop(global::Windows.UI.Xaml.DragEventArgs args)
        {
            base.OnDrop(args);
            DragOperationDeferral deferral = args.GetDeferral();
            DataPackageView view = args.DataView;

            IReadOnlyList<IStorageItem> filesList = await Task.Run(async () =>
            {
                try
                {
                    if (view.Contains(StandardDataFormats.StorageItems))
                    {
                        return await view.GetStorageItemsAsync();
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(EventLevel.Warning, "Drop file in icon extract page failed", e);
                }

                return null;
            });

            if (filesList is not null && filesList.Count is 1)
            {
                ParseFile(filesList[0].Path);
            }
            deferral.Complete();
        }

        #endregion 第一部分：第一部分：重写父类事件

        #region 第二部分：XamlUICommand 命令调用时挂载的事件

        /// <summary>
        /// 终止进程
        /// </summary>
        private async void OnTerminateProcessExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            int processid = Convert.ToInt32(args.Parameter);

            if (processid is not 0)
            {
                bool result = await Task.Run(() =>
                {
                    try
                    {
                        Process process = Process.GetProcessById(processid);
                        process?.Kill();
                        return true;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(EventLevel.Error, string.Format("Terminate process id {0} failed", processid), e);
                        return false;
                    }
                });

                if (result)
                {
                    foreach (ProcessInfoModel processItem in ProcessInfoCollection)
                    {
                        if (processItem.ProcessId.Equals(processid))
                        {
                            ProcessInfoCollection.Remove(processItem);
                            break;
                        }
                    }
                }

                await MainWindow.Current.ShowNotificationAsync(new OperationResultTip(OperationKind.TerminateProcess, result));
            }
        }

        /// <summary>
        /// 打开本地目录
        /// </summary>

        private void OnOpenProcessPathExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is string filePath && !string.IsNullOrEmpty(filePath))
            {
                Task.Run(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            if (File.Exists(filePath))
                            {
                                IntPtr pidlList = Shell32Library.ILCreateFromPath(filePath);
                                if (pidlList != IntPtr.Zero)
                                {
                                    Shell32Library.SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0);
                                    Shell32Library.ILFree(pidlList);
                                }
                            }
                            else
                            {
                                string directoryPath = Path.GetDirectoryName(filePath);

                                if (Directory.Exists(directoryPath))
                                {
                                    Process.Start(directoryPath);
                                }
                                else
                                {
                                    Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(EventLevel.Error, "Open terminate process path failed", e);
                    }
                });
            }
        }

        #endregion 第二部分：XamlUICommand 命令调用时挂载的事件

        #region 第三部分：文件解锁页面——挂载的事件

        /// <summary>
        /// 打开任务管理器
        /// </summary>
        private void OnOpenTaskManagerClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("taskmgr.exe");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(EventLevel.Error, "Open task manager failed", e);
                }
            });
        }

        /// <summary>
        /// 打开本地文件
        /// </summary>
        private void OnOpenLocalFileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dialog = new()
            {
                Multiselect = false,
                Title = ResourceService.FileUnlockResource.GetString("SelectFile")
            };
            if (dialog.ShowDialog() is DialogResult.OK && !string.IsNullOrEmpty(dialog.FileName))
            {
                ParseFile(dialog.FileName);
            }
            dialog.Dispose();
        }

        #endregion 第三部分：文件解锁页面——挂载的事件

        /// <summary>
        /// 解析文件
        /// </summary>
        public void ParseFile(string filePath)
        {
            FileName = Path.GetFileName(filePath);
            ResultSeverity = InfoBarSeverity.Informational;
            IsRingActive = true;
            ResultControlVisable = false;
            StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParsingFile"), FileName);
            ProcessInfoCollection.Clear();

            Task.Run(async () =>
            {
                Guid keyGuid = Guid.NewGuid();
                List<Process> processList = [];

                int result = RstrtmgrLibrary.RmStartSession(out uint handle, 0, keyGuid.ToString());

                // 解析失败
                if (result is not 0)
                {
                    await Task.Delay(500);
                    synchronizationContext.Post(_ =>
                    {
                        ResultSeverity = InfoBarSeverity.Error;
                        IsRingActive = false;
                        StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParseFileFailed"), FileName);
                    }, null);
                }

                try
                {
                    uint pnProcInfo = 0;
                    string[] resources = [filePath];

                    result = RstrtmgrLibrary.RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                    // 解析失败
                    if (result is not 0)
                    {
                        await Task.Delay(500);
                        synchronizationContext.Post(_ =>
                        {
                            ResultSeverity = InfoBarSeverity.Error;
                            IsRingActive = false;
                            StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParseFileFailed"), FileName);
                        }, null);
                    }

                    result = RstrtmgrLibrary.RmGetList(handle, out uint pnProcInfoNeeded, ref pnProcInfo, null, out uint lpdwRebootReasons);

                    if (result is 234)
                    {
                        RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                        pnProcInfo = pnProcInfoNeeded;

                        result = RstrtmgrLibrary.RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, out lpdwRebootReasons);

                        if (result is 0)
                        {
                            for (int index = 0; index < (uint)0; index++)
                            {
                                try
                                {
                                    processList.Add(Process.GetProcessById(processInfo[index].Process.dwProcessId));
                                }
                                catch (Exception e)
                                {
                                    LogService.WriteLog(EventLevel.Error, string.Format("Parse process id {0} failed", processInfo[index].Process.dwProcessId), e);
                                    continue;
                                }
                            }
                        }

                        // 解析成功
                        if (processList.Count is 0)
                        {
                            await Task.Delay(500);
                            synchronizationContext.Post(_ =>
                            {
                                ResultControlVisable = true;
                                ResultSeverity = InfoBarSeverity.Success;
                                IsRingActive = false;
                                StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParseFileSuccessfully"), FileName);
                            }, null);
                        }
                        else
                        {
                            List<string> processUserList = [];

                            foreach (Process processItem in processList)
                            {
                                processUserList.Add(GetProcessUserName(processItem.Id));
                            }

                            await Task.Delay(500);
                            synchronizationContext.Post(_ =>
                            {
                                ResultControlVisable = true;
                                ResultSeverity = InfoBarSeverity.Success;
                                IsRingActive = false;
                                StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParseFileSuccessfully"), FileName);

                                for (int index = 0; index < processList.Count; index++)
                                {
                                    Process processItem = processList[index];
                                    try
                                    {
                                        Icon icon = Icon.ExtractAssociatedIcon(Path.Combine(processItem.MainModule.FileName));
                                        MemoryStream memoryStream = new();
                                        icon.ToBitmap().Save(memoryStream, ImageFormat.Png);
                                        memoryStream.Seek(0, SeekOrigin.Begin);
                                        BitmapImage bitmapImage = new();
                                        bitmapImage.SetSource(memoryStream.AsRandomAccessStream());

                                        ProcessInfoCollection.Add(new ProcessInfoModel()
                                        {
                                            ProcessName = Path.GetFileName(processItem.MainModule.FileVersionInfo.FileName),
                                            ProcessId = processItem.Id,
                                            ProcessUser = processUserList[index],
                                            ProcessPath = processItem.MainModule.FileName,
                                            ProcessIcon = bitmapImage
                                        });
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }
                            }, null);
                        }
                    }
                    // 解析成功，文件没有被占用
                    else
                    {
                        await Task.Delay(500);
                        synchronizationContext.Post(_ =>
                        {
                            ResultControlVisable = true;
                            ResultSeverity = InfoBarSeverity.Success;
                            IsRingActive = false;
                            StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParseFileSuccessfully"), FileName);
                        }, null);
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(500);
                    synchronizationContext.Post(_ =>
                    {
                        ResultControlVisable = false;
                        ResultSeverity = InfoBarSeverity.Error;
                        IsRingActive = false;
                        StateInfoText = string.Format(ResourceService.FileUnlockResource.GetString("ParseFileFailed"), FileName);
                    }, null);
                }
                finally
                {
                    RstrtmgrLibrary.RmEndSession(handle);
                }
            });
        }

        /// <summary>
        /// 通过进程Id来获取进程的用户名
        /// </summary>
        private static string GetProcessUserName(int processId)
        {
            string name = string.Empty;

            SelectQuery query = new("select * from Win32_Process where processID=" + processId);
            ManagementObjectSearcher searcher = new(query);
            try
            {
                foreach (ManagementObject manageObject in searcher.Get().Cast<ManagementObject>())
                {
                    ManagementBaseObject inPar = null;
                    ManagementBaseObject outPar = null;

                    inPar = manageObject.GetMethodParameters("GetOwner");
                    outPar = manageObject.InvokeMethod("GetOwner", inPar, null);

                    name = outPar["User"].ToString();
                    break;
                }
            }
            catch
            {
                name = "SYSTEM";
            }

            return name;
        }
    }
}
