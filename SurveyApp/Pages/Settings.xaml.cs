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
using SurveyApp.Utils;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SurveyApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            App currentApp = (App)Application.Current;
            sharePointSingleton = currentApp.sp;
            this.InitializeComponent();
            this.DataContext = sharePointSingleton;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RestoreCredentials();
            RestoreLists();
        }

        // odkaz na jedináčka udržující nastavení apliakce
        private SurveySingleton sharePointSingleton;

        /// <summary>
        /// Pokud má aplikace v úložisti hedel uložená hesla, tak se obnoví naposledy použité
        /// </summary>
        private void RestoreCredentials()
        {
            var creds = sharePointSingleton.GetSavedUser();
            if (creds != null)
            {
                creds.RetrievePassword();
                URL.Text = creds.Resource;
                userName.Text = creds.UserName;
                var pass = creds.Password;
                if (String.IsNullOrWhiteSpace(pass))
                {
                    password.Password = "";
                }
                else
                {
                    password.Password = pass;
                }
            }
            else
            {
                URL.Text = "";
                userName.Text = "";
                password.Password = "";
            }
        }
        /// <summary>
        /// Přo přihlášeného uživatele se načte naposled používaný seznam
        /// </summary>
        private async void RestoreLists()
        {
            if (sharePointSingleton.IsLoggedIn)
            {
                loadingRing.Visibility = Visibility.Visible;
                var lists = await sharePointSingleton.GetListsTitles();
                listSelector.ItemsSource = lists;
                loadingRing.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Obsluha přihlašovacího tlačítka. Údaje jsou předány jedináčkovi a ten se postará o zbyek práce
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void login_Click(object sender, RoutedEventArgs e)
        {
            var baseURL = URL.Text;
            var user = userName.Text;
            var pass = password.Password;
            var savePass = (bool)savePassword.IsChecked;

            loadingRing.Visibility = Visibility.Visible;

            await sharePointSingleton.Login(baseURL, user, pass, savePass);

            if (sharePointSingleton.IsLoggedIn)
            {
                var lists = await sharePointSingleton.GetListsTitles();
                listSelector.ItemsSource = lists;
            }

            loadingRing.Visibility = Visibility.Collapsed;

            this.DataContext = null;
            this.DataContext = sharePointSingleton;
        }

        /// <summary>
        /// Obsluha jednoduchého odhlašování 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoff_Click(object sender, RoutedEventArgs e)
        {
            sharePointSingleton.Logoff();
            RestoreCredentials();
            this.DataContext = null;
            this.DataContext = sharePointSingleton;
        }

        /// <summary>
        /// Odhlášení, které po sobě vymaže uložené údaje
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteCredentials_Click(object sender, RoutedEventArgs e)
        {
            sharePointSingleton.DeleteCredentials();
            RestoreCredentials();
            this.DataContext = null;
            this.DataContext = sharePointSingleton;
        }

        /// <summary>
        /// Obsluha výběru dotazníku pro vyplnění. Při výběru se ve vedlejším panelu zobrazují položky z daného seznamu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listSelector_SelectionChanged(object sender, RoutedEventArgs e)
        {
            loadingRing.Visibility = Visibility.Visible;
            var item = (ListInfo)listSelector.SelectedItem;
            if (item != null)
            {
                sharePointSingleton.CurrentList = item.ID;
                sharePointSingleton.CurrentFields = await sharePointSingleton.GetListFields(sharePointSingleton.CurrentList);
                listFields.ItemsSource = sharePointSingleton.CurrentFields;
            }
            loadingRing.Visibility = Visibility.Collapsed;
        }

        private void publicMode_Click(object sender, RoutedEventArgs e)
        {

        }

    }


}
