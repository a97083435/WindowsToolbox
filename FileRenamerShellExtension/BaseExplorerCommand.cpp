﻿#include "pch.h"
#include "BaseExplorerCommand.h"
#include "BaseExplorerCommand.g.cpp"

winrt::Windows::ApplicationModel::Resources::Core::ResourceContext resourceContext{ nullptr };
winrt::Windows::ApplicationModel::Resources::Core::ResourceMap resourceMap{ nullptr };

bool isAllowShowShellMenu = false;

namespace winrt::FileRenamerShellExtension::implementation
{
	/// <summary>
	/// SubMenu 部分
	/// </summary>
	IFACEMETHODIMP SubMenu::Next(ULONG celt, __out_ecount_part(celt, *pceltFetched) IExplorerCommand** apUICommand, __out_opt ULONG* pceltFetched)
	{
		const uint32_t oldIndex = mIndex;
		const uint32_t endIndex = mIndex + celt;
		const uint32_t commandCount = mCommands.Size();
		for (; mIndex < endIndex && mIndex < commandCount; mIndex++)
		{
			mCommands.GetAt(mIndex).try_as<IExplorerCommand>().copy_to(apUICommand);
		}

		const uint32_t fetched = mIndex - oldIndex;
		ULONG outParam = static_cast<ULONG>(fetched);
		if (pceltFetched != nullptr)
		{
			*pceltFetched = outParam;
		}
		return (fetched == celt) ? S_OK : S_FALSE;
	}

	IFACEMETHODIMP SubMenu::Skip(ULONG /*celt*/)
	{
		return E_NOTIMPL;
	}

	IFACEMETHODIMP SubMenu::Reset()
	{
		mIndex = 0;
		return S_OK;
	}

	IFACEMETHODIMP SubMenu::Clone(__deref_out IEnumExplorerCommand** ppenum)
	{
		*ppenum = nullptr; return E_NOTIMPL;
	}

	/// <summary>
	/// BaseExplorerCommand 部分
	/// </summary>
	BaseExplorerCommand::BaseExplorerCommand() {};

	IFACEMETHODIMP BaseExplorerCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = L"";
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP BaseExplorerCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP BaseExplorerCommand::GetToolTip(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* infoTip)
	{
		*infoTip = nullptr;
		return E_NOTIMPL;
	}

	IFACEMETHODIMP BaseExplorerCommand::GetCanonicalName(_Out_ GUID* guidCommandName)
	{
		*guidCommandName = GUID_NULL;
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Disabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::GetFlags(_Out_ EXPCMDFLAGS* flags)
	{
		auto subCommands = SubCommands();
		bool hasSubCommands = subCommands != nullptr && subCommands.Size() > 0;
		*flags = !hasSubCommands ? ECF_DEFAULT : ECF_HASSUBCOMMANDS;
		return S_OK;
	}

	IFACEMETHODIMP BaseExplorerCommand::EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands)
	{
		*enumCommands = nullptr;
		auto subCommands = SubCommands();
		bool hasSubCommands = subCommands != nullptr && subCommands.Size() > 0;
		if (hasSubCommands)
		{
			auto subMenu = winrt::make<SubMenu>(SubCommands());
			winrt::hresult result = subMenu.as(IID_PPV_ARGS(enumCommands));
			return result;
		}
		return E_NOTIMPL;
	}

	winrt::Windows::Foundation::Collections::IVectorView<FileRenamerShellExtension::BaseExplorerCommand> BaseExplorerCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// FileRenamerCommand 部分
	/// </summary>
	FileRenamerCommand::FileRenamerCommand() {};

	IFACEMETHODIMP FileRenamerCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = GetLocalized(L"FileRenamerShellMenu/AppName");
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP FileRenamerCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = winrt::Windows::ApplicationModel::Package::Current().InstalledLocation().Path() + L"\\Assets\\FileRenamer.ico";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP FileRenamerCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		if (isAllowShowShellMenu)
		{
			ExplorerCommandState state = ExplorerCommandState::Enabled;
			*cmdState = static_cast<EXPCMDSTATE>(state);
		}
		else
		{
			ExplorerCommandState state = ExplorerCommandState::Hidden;
			*cmdState = static_cast<EXPCMDSTATE>(state);
		}

		return S_OK;
	}

	IFACEMETHODIMP FileRenamerCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<FileRenamerCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::FileRenamerShellExtension::BaseExplorerCommand> FileRenamerCommand::SubCommands()
	{
		return winrt::single_threaded_vector<winrt::FileRenamerShellExtension::BaseExplorerCommand>(
			{
				winrt::make<FileNameCommand>(),
				winrt::make<ExtensionNameCommand>(),
				winrt::make<UpperAndLowerCaseCommand>(),
				winrt::make<FilePropertiesCommand>()
			}).GetView();
	}

	/// <summary>
	/// FileNameCommand 部分
	/// </summary>
	FileNameCommand::FileNameCommand() {};

	IFACEMETHODIMP FileNameCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = GetLocalized(L"FileRenamerShellMenu/AddToFileNamePage");
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP FileNameCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP FileNameCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP FileNameCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<FileNameCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::FileRenamerShellExtension::BaseExplorerCommand> FileNameCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// ExtensionNameCommand 部分
	/// </summary>
	ExtensionNameCommand::ExtensionNameCommand() {};

	IFACEMETHODIMP ExtensionNameCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = GetLocalized(L"FileRenamerShellMenu/AddToExtensionNamePage");
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP ExtensionNameCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP ExtensionNameCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP ExtensionNameCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<ExtensionNameCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::FileRenamerShellExtension::BaseExplorerCommand> ExtensionNameCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// UpperAndLowerCaseCommand 部分
	/// </summary>
	UpperAndLowerCaseCommand::UpperAndLowerCaseCommand() {};

	IFACEMETHODIMP UpperAndLowerCaseCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = GetLocalized(L"FileRenamerShellMenu/AddToUpperAndLowerCasePage");
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP UpperAndLowerCaseCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP UpperAndLowerCaseCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP UpperAndLowerCaseCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<ExtensionNameCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::FileRenamerShellExtension::BaseExplorerCommand> UpperAndLowerCaseCommand::SubCommands()
	{
		return nullptr;
	}

	/// <summary>
	/// FilePropertiesCommand 部分
	/// </summary>
	FilePropertiesCommand::FilePropertiesCommand() {};

	IFACEMETHODIMP FilePropertiesCommand::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
	{
		winrt::hstring title = GetLocalized(L"FileRenamerShellMenu/AddToFilePropertiesPage");
		return SHStrDup(title.c_str(), name);
	}

	IFACEMETHODIMP FilePropertiesCommand::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
	{
		winrt::hstring iconPath = L"";
		return SHStrDup(iconPath.c_str(), icon);
	}

	IFACEMETHODIMP FilePropertiesCommand::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
	{
		ExplorerCommandState state = ExplorerCommandState::Enabled;
		*cmdState = static_cast<EXPCMDSTATE>(state);
		return S_OK;
	}

	IFACEMETHODIMP FilePropertiesCommand::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*)
	{
		constexpr winrt::guid uuid = winrt::guid_of<ExtensionNameCommand>();
		return S_OK;
	}

	winrt::Windows::Foundation::Collections::IVectorView<winrt::FileRenamerShellExtension::BaseExplorerCommand> FilePropertiesCommand::SubCommands()
	{
		return nullptr;
	}
}

/// <summary>
/// 初始化应用本地化资源
/// </summary>
void InitializeResource()
{
	winrt::Windows::Foundation::IInspectable fileShellMenu = ReadSettings(L"FileShellMenu");
	if (fileShellMenu != nullptr)
	{
		isAllowShowShellMenu = winrt::unbox_value<bool>(fileShellMenu);
	}

	resourceContext = winrt::Windows::ApplicationModel::Resources::Core::ResourceContext::GetForViewIndependentUse();
	winrt::Windows::Foundation::IInspectable language = ReadSettings(L"AppLanguage");

	if (language == nullptr)
	{
		resourceContext.QualifierValues().Insert(L"language", L"en-us");
	}
	else
	{
		resourceContext.QualifierValues().Insert(L"language", winrt::unbox_value<winrt::hstring>(language));
	}

	resourceMap = winrt::Windows::ApplicationModel::Resources::Core::ResourceManager::Current().MainResourceMap();
}

/// <summary>
/// 字符串本地化
/// </summary>
winrt::hstring GetLocalized(winrt::hstring resource)
{
	return resourceMap.GetValue(resource, resourceContext).ValueAsString();
}

/// <summary>
/// 读取设置选项存储信息
/// </summary>
winrt::Windows::Foundation::IInspectable ReadSettings(winrt::hstring key)
{
	if (winrt::Windows::Storage::ApplicationData::Current().LocalSettings().Values().TryLookup(key) == nullptr)
	{
		return nullptr;
	}

	return winrt::Windows::Storage::ApplicationData::Current().LocalSettings().Values().Lookup(key);
}