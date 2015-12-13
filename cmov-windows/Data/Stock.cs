using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BoneStock.Data
{
    [DataContract]
    public class Stock
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Tick { get; set; }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public float Value { get; set; }

        [IgnoreDataMember]
        public string Date { get; set; } //only used for chart

        public Stock(string tick, float value, string date)
        {
            this.Id = -1;
            this.Tick = tick;
            this.Name = "";
            this.Value = value;
            this.Date = date;
        }

        public Stock(int id, string tick, string name)
        {
            this.Id = id;
            this.Tick = tick;
            this.Name = name;
            this.Value = 0;
        }
    }
}
