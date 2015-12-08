using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace MasterDetailApp.Data
{
    public class StocksDataSource
    {
        private static ObservableCollection<Stock> _items = new ObservableCollection<Stock>()
        {
            new Stock(0, "AAPL", "Apple computers!"),
            new Stock(1, "GOOG", "Google inc.")
        };

        public async static Task<IList<Stock>> GetAllItems()
        {
            HttpClient aClient = new HttpClient();

            string uriStr = "http://finance.yahoo.com/d/quotes?f=sl1d1t1v&s=";

            foreach (Stock s in _items)
                uriStr += s.Tick + ',';

            try
            {
                HttpResponseMessage aResponse = await aClient.GetAsync(new Uri(uriStr));

                if (aResponse.IsSuccessStatusCode)
                {
                    String responseDataString = await aResponse.Content.ReadAsStringAsync();
                    String[] stockLines = responseDataString.Split('\n');
                    for(int i=0;i<_items.Count;i++)
                    {
                        _items.ElementAt(i).Value = float.Parse(stockLines[i].Split(',').ElementAt(1));
                    }
                }
               else
                {
                    // show the response status code 
                    String failureMsg = "HTTP Status: " + aResponse.StatusCode.ToString() + " - Reason: " + aResponse.ReasonPhrase;
                    new MessageDialog(failureMsg, "Error").ShowAsync();
                }
            }
            catch(COMException e) {
                new MessageDialog("Connection error", "Error").ShowAsync();
            }
            return _items;
        }

        public static Stock GetItemById(int id)
        {
            return _items[id];
        }
    }
}
