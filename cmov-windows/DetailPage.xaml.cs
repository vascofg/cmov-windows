using BoneStock.Data;
using BoneStock.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace BoneStock
{

    public sealed partial class DetailPage : Page
    {
        private static DependencyProperty s_stockProperty
            = DependencyProperty.Register("Stock", typeof(StockViewModel), typeof(DetailPage), new PropertyMetadata(null));

        public static DependencyProperty StockProperty
        {
            get { return s_stockProperty; }
        }

        public StockViewModel Item
        {
            get { return (StockViewModel)GetValue(s_stockProperty); }
            set { SetValue(s_stockProperty, value); }
        }

        public DetailPage()
        {
            this.InitializeComponent();
            this.Loaded += DetailPage_Loaded;
        }

        private async void LineSeries_Loaded(object sender, RoutedEventArgs e)
        {
            await Reload_Graph();

            GraphGroup.SelectionChanged += GraphGroup_SelectionChanged;
            GraphStartDate.DateChanged += GraphStartDate_DateChanged;
        }

        private void DetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            GraphStartDate.MaxDate = DateTime.Now.AddDays(-1);
        }

        public ObservableCollection<Stock> items;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Parameter is item ID
            Item = StockViewModel.FromStock(StocksDataSource.GetItemById((int)e.Parameter));

            var backStack = Frame.BackStack;
            var backStackCount = backStack.Count;

            if (backStackCount > 0)
            {
                var masterPageEntry = backStack[backStackCount - 1];
                backStack.RemoveAt(backStackCount - 1);

                // Doctor the navigation parameter for the master page so it
                // will show the correct item in the side-by-side view.
                var modifiedEntry = new PageStackEntry(
                    masterPageEntry.SourcePageType,
                    Item.ItemId,
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }

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

        private void OnBackRequested()
        {
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.
            
            Frame.GoBack(new DrillInNavigationTransitionInfo());
        }

        void NavigateBackForWideState(bool useTransition)
        {
            // Evict this page from the cache as we may not need it again.
            NavigationCacheMode = NavigationCacheMode.Disabled;

            if (useTransition)
            {
                Frame.GoBack(new EntranceNavigationTransitionInfo());
            }
            else
            {
                Frame.GoBack(new SuppressNavigationTransitionInfo());
            }
        }

        private bool ShouldGoToWideState()
        {
            return Window.Current.Bounds.Width >= 720;
        }

        private void PageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // We shouldn't see this page since we are in "wide master-detail" mode.
                // Play a transition as we are navigating from a separate page.
                NavigateBackForWideState(useTransition:true);
            }
            else
            {
                // Realize the main page content.
                FindName("RootPanel");
            }

            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void PageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // Make sure we are no longer listening to window change events.
                Window.Current.SizeChanged -= Window_SizeChanged;

                // We shouldn't see this page since we are in "wide master-detail" mode.
                NavigateBackForWideState(useTransition:false);
            }
        }

        private void DetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Mark event as handled so we don't get bounced out of the app.
            e.Handled = true;

            OnBackRequested();
        }

        private async Task Reload_Graph()
        {
            if (GraphStartDate.Date == null)
            {
                items = new ObservableCollection<Stock>(await StocksDataSource.getGraph(
                    Item.Tick, new DateTime(DateTime.Now.Year, 01, 01),
                    (GraphGroup.SelectedItem as ComboBoxItem).Tag.ToString()[0]));
            }
            else
            {
                items = new ObservableCollection<Stock>(await StocksDataSource.getGraph(
                    Item.Tick, GraphStartDate.Date.GetValueOrDefault().LocalDateTime,
                    (GraphGroup.SelectedItem as ComboBoxItem).Tag.ToString()[0]));
            }
            
            (LineChart.Series[0] as DataPointSeries).ItemsSource = items;
        }

        private async void GraphStartDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            await Reload_Graph();
        }

        private async void GraphGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Reload_Graph();
        }
    }
}
