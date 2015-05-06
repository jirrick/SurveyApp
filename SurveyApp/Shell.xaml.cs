using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SurveyApp.Pages;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SurveyApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();

            // Na telefonu skryje tlačítko zpět a přehodí SplitView napravo
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                this.BackRadioButton.Visibility = Visibility.Collapsed;
                this.SplitView.FlowDirection = FlowDirection.RightToLeft;
            }
        }

        private void BackRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            if (frame?.CanGoBack == true)
            {
                frame.GoBack();
            }
        }

        private void HamburgerRadioButton_Click(object sender, RoutedEventArgs e)
        {
            this.SplitView.IsPaneOpen = !this.SplitView.IsPaneOpen;
        }

        private void HomeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(Home))
            {
                frame.Navigate(typeof(Home));
                frame.BackStack.Clear(); //vymazat historii po návratu na domovskou obrazovku
            }
            this.SplitView.IsPaneOpen = false;
        }

        private void SettingsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(Settings))
            {
                frame.Navigate(typeof(Settings));
            }
            this.SplitView.IsPaneOpen = false;
        }

        private void AboutRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(About))
            {
                frame.Navigate(typeof(About));
            }
            this.SplitView.IsPaneOpen = false;
        }

        private void SurveyRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            if (page?.GetType() != typeof(Survey))
            {
                frame.Navigate(typeof(Survey));
            }
            this.SplitView.IsPaneOpen = false;
        }
    }
}
