#pragma once

#include "MainPage.g.h"

namespace winrt::FileRenamer::implementation
{
    struct MainPage : MainPageT<MainPage>
    {
        MainPage();

        int32_t MyProperty();
        void MyProperty(int32_t value);
    };
}

namespace winrt::FileRenamer::factory_implementation
{
    struct MainPage : MainPageT<MainPage, implementation::MainPage>
    {
    };
}
