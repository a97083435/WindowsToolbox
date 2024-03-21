using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsTools.Services.Root;
using WindowsTools.Strings;
using WindowsTools.Views.Windows;
using WindowsTools.WindowsAPI.ComTypes;
using WINSATLib;

namespace WindowsTools.Views.Pages
{
    /// <summary>
    /// 系统评估页面
    /// </summary>
    public sealed partial class WinSATPage : Page, INotifyPropertyChanged
    {
        private CInitiateWinSAT cInitiateWinSAT = new CInitiateWinSAT();
        private _RemotableHandle _RemotableHandle = new _RemotableHandle();
        private Guid progressDialogCLSID = new Guid("F8383852-FCD3-11d1-A6B9-006097DF5BD4");
        private CWinSATCallbacks cWinSATCallbacks;
        private IProgressDialog progressDialog;

        private string _processorSubScore;

        public string ProcessorSubScore
        {
            get { return _processorSubScore; }

            set
            {
                if (!Equals(_processorSubScore, value))
                {
                    _processorSubScore = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessorSubScore)));
                }
            }
        }

        private string _memorySubScore;

        public string MemorySubScore
        {
            get { return _memorySubScore; }

            set
            {
                if (!Equals(_memorySubScore, value))
                {
                    _memorySubScore = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MemorySubScore)));
                }
            }
        }

        private string _graphicsSubScore;

        public string GraphicsSubScore
        {
            get { return _graphicsSubScore; }

            set
            {
                if (!Equals(_graphicsSubScore, value))
                {
                    _graphicsSubScore = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphicsSubScore)));
                }
            }
        }

        private string _gamingGraphicsSubScore;

        public string GamingGraphicsSubScore
        {
            get { return _gamingGraphicsSubScore; }

            set
            {
                if (!Equals(_gamingGraphicsSubScore, value))
                {
                    _gamingGraphicsSubScore = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GamingGraphicsSubScore)));
                }
            }
        }

        private string _primaryDiskSubScore;

        public string PrimaryDiskSubScore
        {
            get { return _primaryDiskSubScore; }

            set
            {
                if (!Equals(_primaryDiskSubScore, value))
                {
                    _primaryDiskSubScore = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrimaryDiskSubScore)));
                }
            }
        }

        private string _basicScore;

        public string BasicScore
        {
            get { return _basicScore; }

            set
            {
                if (!Equals(_basicScore, value))
                {
                    _basicScore = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasicScore)));
                }
            }
        }

        private bool _basicScoreExisted;

        public bool BasicScoreExisted
        {
            get { return _basicScoreExisted; }

            set
            {
                if (!Equals(_basicScoreExisted, value))
                {
                    _basicScoreExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BasicScoreExisted)));
                }
            }
        }

        private bool _isNotRunningAssessment = true;

        public bool IsNotRunningAssessment
        {
            get { return _isNotRunningAssessment; }

            set
            {
                if (!Equals(_isNotRunningAssessment, value))
                {
                    _isNotRunningAssessment = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotRunningAssessment)));
                }
            }
        }

        private InfoBarSeverity _resultSeverity;

        private InfoBarSeverity ResultServerity
        {
            get { return _resultSeverity; }

            set
            {
                if (!Equals(_resultSeverity, value))
                {
                    _resultSeverity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultServerity)));
                }
            }
        }

        private string _resultMessage;

        public string ResultMessage
        {
            get { return _resultMessage; }

            set
            {
                if (!Equals(_resultMessage, value))
                {
                    _resultMessage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultMessage)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WinSATPage()
        {
            InitializeComponent();
        }

        #region 第一部分：系统评估页面——挂载的事件

        /// <summary>
        /// 初始化系统评估信息
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            GetWinSATInfo();
        }

        /// <summary>
        /// 运行评估
        /// </summary>
        private void OnRunAssesssmentClicked(object sender, RoutedEventArgs args)
        {
            IsNotRunningAssessment = false;
            try
            {
                cWinSATCallbacks = new CWinSATCallbacks();

                if (cWinSATCallbacks is not null)
                {
                    cWinSATCallbacks.StatusUpdated += OnStatusUpdated;
                    cWinSATCallbacks.StatusCompleted += OnStatusCompleted;
                }

                cInitiateWinSAT.InitiateFormalAssessment(cWinSATCallbacks, ref _RemotableHandle);

                progressDialog = (IProgressDialog)Activator.CreateInstance(Type.GetTypeFromCLSID(progressDialogCLSID));

                if (progressDialog is not null)
                {
                    progressDialog.SetTitle(WinSAT.WEI);
                    progressDialog.SetLine(2, WinSAT.WEITipContent, false, IntPtr.Zero);
                    progressDialog.StartProgressDialog(IntPtr.Zero, null, PROGDLG.PROGDLG_NOMINIMIZE, IntPtr.Zero);
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(EventLogEntryType.Error, "Run Windows system assessment tool failed", e);
                cWinSATCallbacks = null;
                progressDialog = null;
            }
        }

        /// <summary>
        /// 打开评估日志目录
        /// </summary>
        private void OnOpenAssessmentLogFolderClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"performance\winsat\datastore"));
            });
        }

        /// <summary>
        /// 了解系统评估
        /// </summary>
        private void OnLearnSystemAssessmentClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                Process.Start("https://learn.microsoft.com/zh-cn/windows-hardware/manufacture/desktop/configure-windows-system-assessment-test-scores");
            });
        }

        #endregion 第一部分：系统评估页面——挂载的事件

        #region 第二部分：自定义事件

        /// <summary>
        /// 评估取得进展时触发的事件
        /// </summary>
        private void OnStatusUpdated(object sender, EventArgs args)
        {
            if (cWinSATCallbacks is not null && progressDialog is not null)
            {
                MainWindow.Current.BeginInvoke(() =>
                {
                    try
                    {
                        // 用户主动取消了操作
                        if (progressDialog.HasUserCancelled())
                        {
                            progressDialog.StopProgressDialog();
                            cInitiateWinSAT.CancelAssessment();
                            cWinSATCallbacks.StatusCompleted -= OnStatusUpdated;
                            cWinSATCallbacks.StatusUpdated -= OnStatusUpdated;
                            cWinSATCallbacks = null;
                            progressDialog = null;
                            IsNotRunningAssessment = true;
                            return;
                        }

                        progressDialog.SetLine(1, cWinSATCallbacks.CurrentState, false, IntPtr.Zero);
                        progressDialog.SetProgress(cWinSATCallbacks.CurrentTick, cWinSATCallbacks.TickTotal);
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(EventLogEntryType.Error, "Windows system assessment tool state updated failed", e);
                        cWinSATCallbacks = null;
                        progressDialog = null;
                        IsNotRunningAssessment = true;
                    }
                });
            }
        }

        /// <summary>
        /// 评估完成时触发的事件
        /// </summary>
        private void OnStatusCompleted(object sender, EventArgs args)
        {
            if (cWinSATCallbacks is not null && progressDialog is not null)
            {
                MainWindow.Current.BeginInvoke(() =>
                {
                    try
                    {
                        progressDialog.StopProgressDialog();
                        cWinSATCallbacks.StatusCompleted -= OnStatusUpdated;
                        cWinSATCallbacks.StatusUpdated -= OnStatusUpdated;
                        cWinSATCallbacks = null;
                        progressDialog = null;
                        GetWinSATInfo();
                        IsNotRunningAssessment = true;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(EventLogEntryType.Error, "Windows system assessment tool state completed failed", e);
                        cWinSATCallbacks = null;
                        progressDialog = null;
                        IsNotRunningAssessment = true;
                    }
                });
            }
        }

        #endregion 第二部分：自定义事件

        /// <summary>
        /// 加载系统评估信息
        /// </summary>
        private void GetWinSATInfo()
        {
            Task.Run(() =>
            {
                CQueryWinSAT queryWinSAT = new CQueryWinSAT();
                double basicScore = 0.0;
                double processorSubScore = 0.0;
                double memorySubScore = 0.0;
                double graphicsSubScore = 0.0;
                double gamingGraphicsSubScore = 0.0;
                double primaryDiskSubScore = 0.0;

                try
                {
                    basicScore = queryWinSAT.Info.SystemRating;
                    processorSubScore = queryWinSAT.Info.GetAssessmentInfo(WINSAT_ASSESSMENT_TYPE.WINSAT_ASSESSMENT_CPU).Score;
                    memorySubScore = queryWinSAT.Info.GetAssessmentInfo(WINSAT_ASSESSMENT_TYPE.WINSAT_ASSESSMENT_MEMORY).Score;
                    graphicsSubScore = queryWinSAT.Info.GetAssessmentInfo(WINSAT_ASSESSMENT_TYPE.WINSAT_ASSESSMENT_GRAPHICS).Score;
                    gamingGraphicsSubScore = queryWinSAT.Info.GetAssessmentInfo(WINSAT_ASSESSMENT_TYPE.WINSAT_ASSESSMENT_D3D).Score;
                    primaryDiskSubScore = queryWinSAT.Info.GetAssessmentInfo(WINSAT_ASSESSMENT_TYPE.WINSAT_ASSESSMENT_DISK).Score;
                    dynamic assessmentDate = queryWinSAT.Info.AssessmentDateTime;

                    MainWindow.Current.BeginInvoke(() =>
                    {
                        BasicScoreExisted = basicScore is not 0.0;
                        BasicScore = basicScore is 0.0 ? "N/A" : basicScore.ToString("F1");
                        ProcessorSubScore = processorSubScore is 0.0 ? "N/A" : processorSubScore.ToString("F1");
                        MemorySubScore = memorySubScore is 0.0 ? "N/A" : memorySubScore.ToString("F1");
                        GraphicsSubScore = graphicsSubScore is 0.0 ? "N/A" : graphicsSubScore.ToString("F1");
                        GamingGraphicsSubScore = gamingGraphicsSubScore is 0.0 ? "N/A" : gamingGraphicsSubScore.ToString("F1");
                        PrimaryDiskSubScore = primaryDiskSubScore is 0.0 ? "N/A" : primaryDiskSubScore.ToString("F1");
                        ResultMessage = basicScore is 0.0 ? WinSAT.ErrorMessage : string.Format(WinSAT.SuccessMessage, assessmentDate);
                        ResultServerity = basicScore is 0.0 ? InfoBarSeverity.Warning : InfoBarSeverity.Success;
                    });
                }
                catch (Exception e)
                {
                    LogService.WriteLog(EventLogEntryType.Error, "Query WinSAT score failed", e);
                    MainWindow.Current.BeginInvoke(() =>
                    {
                        BasicScoreExisted = false;
                        BasicScore = "N/A";
                        ProcessorSubScore = "N/A";
                        MemorySubScore = "N/A";
                        GraphicsSubScore = "N/A";
                        GamingGraphicsSubScore = "N/A";
                        PrimaryDiskSubScore = "N/A";
                        ResultMessage = WinSAT.ErrorMessage;
                        ResultServerity = InfoBarSeverity.Warning;
                    });
                }
            });
        }
    }
}
