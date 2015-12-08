using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterDetailApp.Data;
using Windows.Globalization.DateTimeFormatting;

namespace MasterDetailApp.ViewModels
{
    public class StockViewModel
    {

        private int _itemId;

        public int ItemId
        {
            get
            {
                return _itemId;
            }
        }

        public string Tick { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }

        public StockViewModel()
        {
        }

        public static StockViewModel FromStock(Stock stock)
        {
            var viewModel = new StockViewModel();

            viewModel._itemId = stock.Id;
            viewModel.Tick = stock.Tick;
            viewModel.Name = stock.Name;
            viewModel.Value = stock.Value;

            return viewModel;
        }
    }
}
