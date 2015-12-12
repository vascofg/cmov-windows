using BoneStock.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BoneStock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddStock : Page
    {
        public AddStock()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Register for hardware and software back request from the system
            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += DetailPage_BackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested -= DetailPage_BackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender==AppBarAddButton)
            {
                await StocksDataSource.Add(new Stock(StocksDataSource.nextId(), Tick.Text, Name.Text));
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MasterDetailPage));
                Subscribe();
            }
        }

        private async Task Subscribe()
        {
            HttpClient aClient = new HttpClient();

            string uriStr = "http://cmov-trains.herokuapp.com/addSub";

            try
            {
                var json = new JsonObject();
                json.Add("tick", JsonValue.CreateStringValue(Tick.Text));
                if (checkBox.IsChecked.GetValueOrDefault())
                {
                    json.Add("max", JsonValue.CreateNumberValue(alertMax.Value));
                    json.Add("min", JsonValue.CreateNumberValue(alertMin.Value));
                }
                else
                {
                    json.Add("max", JsonValue.CreateNumberValue(-1));
                    json.Add("min", JsonValue.CreateNumberValue(-1));
                }
                json.Add("wns", JsonValue.CreateStringValue((Application.Current as App).channel.Uri));

                HttpResponseMessage aResponse = await aClient.PostAsync(new Uri(uriStr), new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
                if (aResponse.IsSuccessStatusCode)
                {
                }
                else
                {
                    // show the response status code 
                    String failureMsg = "HTTP Status: " + aResponse.StatusCode.ToString() + " - Reason: " + aResponse.ReasonPhrase;
                    new MessageDialog(failureMsg, "Error").ShowAsync();
                }
            }
            catch (COMException e)
            {
                new MessageDialog("Connection error", "Error").ShowAsync();
            }
        }

        private void OnBackRequested()
        {
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.

            Frame.GoBack(new DrillInNavigationTransitionInfo());
        }

        private void DetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Mark event as handled so we don't get bounced out of the app.
            e.Handled = true;

            OnBackRequested();
        }

        private void checkBox_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox bah = sender as CheckBox;
            bool? naksod = bah.IsChecked;
            alertMin.IsEnabled = bah.IsChecked.GetValueOrDefault();
            alertMax.IsEnabled = bah.IsChecked.GetValueOrDefault();
        }
    }
}
