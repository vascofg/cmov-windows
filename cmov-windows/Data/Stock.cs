using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailApp.Data
{
    public class Stock
    {
        public int Id { get; set; }

        public string Tick { get; set; }

        public string Name { get; set; }

        public float Value { get; set; }

        public Stock(int id, string tick, string name)
        {
            this.Id = id;
            this.Tick = tick;
            this.Name = name;
            this.Value = 0;
        }
    }
}
