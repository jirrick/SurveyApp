using SurveyApp.Utils;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SurveyApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Survey : Page
    {
        public Survey()
        {
            this.InitializeComponent();
            App currentApp = (App)Application.Current;
            sharePointSingleton = currentApp.sp;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Refresh();
        }

        private SurveySingleton sharePointSingleton;

        /// <summary>
        /// Obnovení vykreslených položek dotazníku po přechodu nezi stránkami
        /// </summary>
        private void Refresh()
        {
            surveyFields.ItemsSource = null;
            if (sharePointSingleton.CurrentFields != null)
            {
                submit.Visibility = Visibility.Visible;
                foreach (FieldInfo f in sharePointSingleton.CurrentFields)
                {
                    f.FieldValue = null;
                } 
                surveyFields.ItemsSource = sharePointSingleton.CurrentFields;
            }
            else { submit.Visibility = Visibility.Collapsed; }
        }

        /// <summary>
        /// Obsluha odeslání dotazníku
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void odeslatDotaznik_Click(object sender, RoutedEventArgs e)
        {
            submitRing.Visibility = Visibility.Visible;
            await sharePointSingleton.AddItem(sharePointSingleton.CurrentFields);
            Refresh();
            submitRing.Visibility = Visibility.Collapsed;
        }
    }
}
