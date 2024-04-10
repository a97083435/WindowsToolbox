﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using WindowsToolsShellExtension.Commands;
using WindowsToolsShellExtension.Services.Controls.Settings;
using WindowsToolsShellExtension.Strings;

namespace WindowsToolsShellExtension
{
    /// <summary>
    /// Windows 工具箱 右键菜单扩展
    /// </summary>
    public class Program
    {
        private static long g_cRefModule = 0;

        public static StrategyBasedComWrappers StrategyBasedComWrappers { get; } = new StrategyBasedComWrappers();

        private static IReadOnlyDictionary<Guid, Func<object>> createFunctions = new Dictionary<Guid, Func<object>>()
        {
            [typeof(RootItemCommand).GUID] = () => new RootItemCommand()
        };

        static Program()
        {
            string language = LanguageService.GetLanguage();

            if (!string.IsNullOrEmpty(language))
            {
                ShellMenu.Culture = new CultureInfo(LanguageService.GetLanguage());
            }
        }

        /// <summary>
        /// 确定是否正在使用实现此函数的 DLL。 否则，调用方可以从内存中卸载 DLL。
        /// </summary>
        /// <returns>OLE 不提供此函数。 支持 OLE 组件对象模型 (COM) 的 DLL 应实现并导出 DllCanUnloadNow。</returns>
        [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
        public static int DllCanUnloadNow()
        {
            return g_cRefModule >= 1 ? 1 : 0;
        }

        public static void DllAddRef()
        {
            Interlocked.Increment(ref g_cRefModule);
        }

        public static void DllRelease()
        {
            Interlocked.Decrement(ref g_cRefModule);
        }

        /// <summary>
        /// 从 DLL 对象处理程序或对象应用程序中检索类对象。
        /// </summary>
        /// <param name="clsid">将关联正确数据和代码的 CLSID。</param>
        /// <param name="riid">对调用方用于与类对象通信的接口标识符的引用。 通常，IID_IClassFactory (OLE 标头中将其定义为 IClassFactory) 的接口标识符。</param>
        /// <param name="ppv">接收 riid 中请求的接口指针的指针变量的地址。 成功返回后，*ppv 包含请求的接口指针。 如果发生错误，接口指针为 NULL。</param>
        /// <returns>此函数可以返回标准返回值E_INVALIDARG、E_OUTOFMEMORY和E_UNEXPECTED，以及以下值。</returns>
        [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
        public static unsafe int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv)
        {
            foreach ((Guid guid, Func<object> func) in createFunctions)
            {
                if (clsid->Equals(guid))
                {
                    ClassFactory classFactory = new ClassFactory(func);
                    IntPtr pClassFactory = StrategyBasedComWrappers.GetOrCreateComInterfaceForObject(classFactory, CreateComInterfaceFlags.None);

                    ((IUnknown*)pClassFactory)->QueryInterface(riid, ppv);
                    ((IUnknown*)pClassFactory)->Release();

                    return 0;
                }
            }

            return unchecked((int)0x80040111);
        }
    }
}
