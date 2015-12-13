using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace BoneStock.Data
{
    public class StocksDataSource
    {

        private static string PORTFOLIO_FILENAME = "portfolio.dat";
        
        private static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ObservableCollection<Stock>));

        private static ObservableCollection<Stock> _items = new ObservableCollection<Stock>();

        public async static Task ReadFromFile()
        {
            StorageFile portfolioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(PORTFOLIO_FILENAME, CreationCollisionOption.OpenIfExists);

            using (Stream stream = await portfolioFile.OpenStreamForReadAsync())
            {
                try
                {
                    _items = (ObservableCollection<Stock>)serializer.ReadObject(stream);
                    foreach (Stock item in _items)
                        Debug.WriteLine("Deserialized: " + item.Tick);
                }
                catch(SerializationException e)
                {
                    Debug.WriteLine("Failed deserializing portfolio: " + e.Message);
                }
            }
        }

        public async static Task WriteToFile()
        {
            StorageFile portfolioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(PORTFOLIO_FILENAME, CreationCollisionOption.ReplaceExisting);
            using (Stream stream = await portfolioFile.OpenStreamForWriteAsync())
            {
                try
                {
                    serializer.WriteObject(stream, _items);
                }
                catch (SerializationException)
                {
                    Debug.WriteLine("Failed serializing portfolio");
                }
            }
        }

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
                        try {
                            _items.ElementAt(i).Value = float.Parse(stockLines[i].Split(',').ElementAt(1));
                        }
                        catch (FormatException e)
                        {
                            new MessageDialog("Bad response for " + _items.ElementAt(i).Tick, "Error").ShowAsync();
                        }
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
            foreach (Stock item in _items)
                if (item.Id == id)
                    return item;
            return null;
        }

        public async static Task DeleteItemById(int id)
        {
            _items.Remove(GetItemById(id));
            await WriteToFile();
        }

        public async static Task Add(Stock stock)
        {
            _items.Add(stock);
            await WriteToFile();
        }

        public static int nextId()
        {
            if (_items.Count > 0)
                return _items.Last().Id + 1;
            else
                return 0;
        }

        public async static Task<List<Stock>> getGraph(string Tick)
        {
            HttpClient aClient = new HttpClient();

            string uriStr = "http://ichart.finance.yahoo.com/table.txt?a=0&b=0&c=2015&d=11&e=30&f=2015&g=d&s="+Tick;

            List<Stock> items = new List<Stock>();

            try
            {
                HttpResponseMessage aResponse = await aClient.GetAsync(new Uri(uriStr));

                if (aResponse.IsSuccessStatusCode)
                {
                    String responseDataString = await aResponse.Content.ReadAsStringAsync();
                    String[] stockLines = responseDataString.Split('\n');
                    for (int i = 1; i < stockLines.Length; i++) //skip title
                    {
                        String[] stockLine = stockLines[i].Split(',');
                        string date = stockLine[0];
                        float value = float.Parse(stockLine[4]);
                        try
                        {
                            Stock s = new Stock(Tick, value, date);
                            items.Add(s);
                        }
                        catch (FormatException e)
                        {
                            new MessageDialog("Bad response for " + _items.ElementAt(i).Tick, "Error").ShowAsync();
                        }
                    }
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
            return items;
        }
    }
}
