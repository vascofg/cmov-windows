using BoneStock.Data;
using BoneStock.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace BoneStock
{
    public sealed partial class MasterDetailPage : Page
    {
        private StockViewModel _lastSelectedItem;

        private DataPointSeries series;
        private Chart chart;

        private CalendarDatePicker GraphStartDate;

        private ComboBox GraphGroup;

        public ObservableCollection<Stock> items;

        public MasterDetailPage()
        {
            this.InitializeComponent();
        }

        public async Task Refresh()
        {
            var items = MasterListView.ItemsSource as ObservableCollection<StockViewModel>;
            
            items = new ObservableCollection<StockViewModel>();     

            var itemsList = await StocksDataSource.GetAllItems();

            for(int i=0;i<itemsList.Count;i++)
            {
                 items.Add(StockViewModel.FromStock(itemsList[i]));
            }
            
            MasterListView.ItemsSource = items;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            var items = MasterListView.ItemsSource as ObservableCollection<StockViewModel>;

            //refresh on all navigations
            //if (items == null)
            //{
            await Refresh();
            //}

            if (e.Parameter != null)
            {
                // Parameter is item ID
                var id = (int)e.Parameter;
                _lastSelectedItem =
                    items.Where((item) => item.ItemId == id).FirstOrDefault();
            }
            
            UpdateForVisualState(AdaptiveStates.CurrentState);

            // Don't play a content transition for first item load.
            // Sometimes, this content will be animated as part of the page transition.
            DisableContentTransitions();
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null)
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(DetailPage), _lastSelectedItem.ItemId, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(MasterListView, isNarrow);
            if (DetailContentPresenter != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(DetailContentPresenter, !isNarrow);
            }
        }

        private async Task LoadGraph(StockViewModel item)
        {
            chart.Visibility = Visibility.Collapsed;

            items = new ObservableCollection<Stock>(await StocksDataSource.getGraph(
                item.Tick, GraphStartDate.Date.GetValueOrDefault().LocalDateTime,
                (GraphGroup.SelectedItem as ComboBoxItem).Tag.ToString()[0]));

            series.ItemsSource = items;

            chart.Visibility = Visibility.Visible;
        }

        private async void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (StockViewModel)e.ClickedItem;
            if(_lastSelectedItem != clickedItem || _lastSelectedItem == null)
                LoadGraph(clickedItem);
            _lastSelectedItem = clickedItem;
        
            if (AdaptiveStates.CurrentState == NarrowState)
            {
                // Use "drill in" transition for navigating from master list to detail view
                Frame.Navigate(typeof(DetailPage), clickedItem.ItemId, new DrillInNavigationTransitionInfo());
            }
            else
            {
                // Play a refresh animation when the user switches detail items.
                EnableContentTransitions();
            }
        }

        T FindFirstChild<T>(FrameworkElement element) where T : FrameworkElement
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            var children = new FrameworkElement[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                children[i] = child;
                if (child is T)
                    return (T)child;
            }

            for (int i = 0; i < childrenCount; i++)
                if (children[i] != null)
                {
                    var subChild = FindFirstChild<T>(children[i]);
                    if (subChild != null)
                        return subChild;
                }

            return null;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            // Assure we are displaying the correct item. This is necessary in certain adaptive cases.
            MasterListView.SelectedItem = _lastSelectedItem;

            series = FindFirstChild<LineSeries>(DetailContentPresenter);
            series.ItemsSource = items;

            chart = FindFirstChild<Chart>(DetailContentPresenter);

            GraphStartDate = FindFirstChild<CalendarDatePicker>(DetailContentPresenter);
            GraphGroup = FindFirstChild<ComboBox>(DetailContentPresenter);

            GraphStartDate.MaxDate = DateTime.Now.AddDays(-1);
            GraphStartDate.Date = new DateTime(DateTime.Now.Year, 01, 01);

            GraphGroup.SelectionChanged += GraphGroup_SelectionChanged;
            GraphStartDate.DateChanged += GraphStartDate_DateChanged;
        }

        private void EnableContentTransitions()
        {
            DetailContentPresenter.ContentTransitions.Clear();
            DetailContentPresenter.ContentTransitions.Add(new EntranceThemeTransition());
        }

        private void DisableContentTransitions()
        {
            if (DetailContentPresenter != null)
            {
                DetailContentPresenter.ContentTransitions.Clear();
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == AppBarAddButton)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(AddStock));
            }
            else if(sender == AppBarRefreshButton)
            {
                await Refresh();
            }
        }

        private void MasterListView_Flyout(object sender, RoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var items = MasterListView.ItemsSource as ObservableCollection<StockViewModel>;
            var item = (e.OriginalSource as FrameworkElement).DataContext as StockViewModel;
            await StocksDataSource.DeleteItemById(item.ItemId);

            HttpClient aClient = new HttpClient();

            string uriStr = "http://cmov-trains.herokuapp.com/delSub";

            try
            {
                var json = new JsonObject();
                json.Add("tick", JsonValue.CreateStringValue(item.Tick));
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
            catch (COMException)
            {
                new MessageDialog("Connection error", "Error").ShowAsync();
            }
            items.Remove(item);
            _lastSelectedItem = null;
        }

        private async void GraphStartDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            await LoadGraph(_lastSelectedItem);
        }

        private async void GraphGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadGraph(_lastSelectedItem);
        }
    }
}
