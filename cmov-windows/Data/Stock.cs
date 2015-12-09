using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailApp.Data
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

        public Stock(int id, string tick, string name)
        {
            this.Id = id;
            this.Tick = tick;
            this.Name = name;
            this.Value = 0;
        }
    }
}
