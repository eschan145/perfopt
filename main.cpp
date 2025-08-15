#include "pch.h"

#include <iostream>
#include <windows.h>
#include <microsoft.ui.xaml.window.h>

#include <winrt/Microsoft.UI.Xaml.h>

#include <winrt/Microsoft.UI.Windowing.h>
#include <winrt/Windows.Graphics.h>
#include <winrt/Microsoft.UI.Xaml.Hosting.h>

using namespace winrt;
using namespace Microsoft::UI::Xaml;
using namespace Microsoft::UI::Xaml::Controls;
using namespace Microsoft::UI::Xaml::XamlTypeInfo;
using namespace Microsoft::UI::Xaml::Markup;
using namespace Windows::UI::Xaml::Interop;
using namespace Windows::Foundation;


class MainWindow : public WindowT<MainWindow>
{
public:
	MainWindow()
	{
this->ExtendsContentIntoTitleBar(true);
auto windowNative{ this->m_inner.as<::IWindowNative>() };
    HWND hwnd{ 0 };
    windowNative->get_WindowHandle(&hwnd);
winrt::Microsoft::UI::WindowId windowId = winrt::Microsoft::UI::GetWindowIdFromWindow(hwnd);
winrt::Microsoft::UI::Windowing::AppWindow appWindow = winrt::Microsoft::UI::Windowing::AppWindow::GetFromWindowId(windowId);
        // Access the title bar
        auto titleBar = appWindow.TitleBar();
		StackPanel stackPanel;
		stackPanel.HorizontalAlignment(HorizontalAlignment::Center);
		stackPanel.VerticalAlignment(VerticalAlignment::Center);


		TextBlock title;
		title.Style(Application::Current().Resources().Lookup(box_value(L"TitleTextBlockStyle")).as<Style>());
		title.Text(L"WinUI 3 in C++ Without XAML!");
		title.HorizontalAlignment(HorizontalAlignment::Center);

		HyperlinkButton project;
		project.Content(box_value(L"Github Project Repository"));
		project.NavigateUri(Uri(L"https://github.com/sotanakamura/winui3-without-xaml"));
		project.HorizontalAlignment(HorizontalAlignment::Center);

		Button button;
		button.Content(box_value(L"Click"));
		button.Click([&](IInspectable const& sender, RoutedEventArgs) { sender.as<Button>().Content(box_value(L"Thank You!")); });
		button.HorizontalAlignment(HorizontalAlignment::Center);
		button.Margin(ThicknessHelper::FromUniformLength(20));

		Content(stackPanel);
		stackPanel.Children().Append(title);
		stackPanel.Children().Append(project);
		stackPanel.Children().Append(button);
	}
};



class App : public ApplicationT<App, IXamlMetadataProvider> {
public:
    void OnLaunched(LaunchActivatedEventArgs const&) {
        this->Resources().MergedDictionaries().Append(XamlControlsResources());

        window = make<MainWindow>();
        window.Activate();
    }
    IXamlType GetXamlType(TypeName const& type) {
        return provider.GetXamlType(type);
    }
    IXamlType GetXamlType(hstring const& fullname) {
        return provider.GetXamlType(fullname);
    }
    winrt::com_array<XmlnsDefinition> GetXmlnsDefinitions() {
        return provider.GetXmlnsDefinitions();
    }
private:
        Window window{ nullptr };
        XamlControlsXamlMetaDataProvider provider;
};

int WINAPI wWinMain(HINSTANCE, HINSTANCE, LPWSTR, int) {
        init_apartment();
        Application::Start([](auto&&) {make<App>();});
    return 0;
}
