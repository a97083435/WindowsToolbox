﻿#pragma once

#include "winrt/base.h"
#include "AboutPage.g.h"
#include "ViewModels/Pages/AboutViewModel.h"

using namespace winrt;

namespace winrt::FileRenamer::implementation
{
	struct AboutPage : AboutPageT<AboutPage>
	{
	public:
		AboutPage();

		FileRenamer::AboutViewModel ViewModel();

		hstring Title();
		hstring BriefIntroduction();
		hstring Reference();
		hstring UseInstruction();
		hstring Precaution();
		hstring SettingsHelp();
		hstring Thanks();
		hstring QuickOperation();
		hstring CreateDesktopShortcut();
		hstring CreateDesktopShortcutToolTip();
		hstring PinToStartScreen();
		hstring PinToStartScreenToolTip();
		hstring PinToTaskbar();
		hstring PinToTaskbarToolTip();
		hstring UpdateAndLicensing();
		hstring ShowReleaseNotes();
		hstring ShowReleaseNotesToolTip();
		hstring ShowLicense();
		hstring ShowLicenseToolTip();

	private:
		FileRenamer::AboutViewModel _viewModel;

		hstring _title;
		hstring _briefIntroduction;
		hstring _reference;
		hstring _useInstruction;
		hstring _precaution;
		hstring _settingsHelp;
		hstring _thanks;
		hstring _quickOperation;
		hstring _createDesktopShortcut;
		hstring _createDesktopShortcutToolTip;
		hstring _pinToStartScreen;
		hstring _pinToStartScreenToolTip;
		hstring _pinToTaskbar;
		hstring _pinToTaskbarToolTip;
		hstring _updateAndLicensing;
		hstring _showReleaseNotes;
		hstring _showReleaseNotesToolTip;
		hstring _showLicense;
		hstring _showLicenseToolTip;
	};
}

namespace winrt::FileRenamer::factory_implementation
{
	struct AboutPage : AboutPageT<AboutPage, implementation::AboutPage>
	{
	};
}
