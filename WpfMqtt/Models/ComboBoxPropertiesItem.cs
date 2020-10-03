using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Client.Models
{
    public class ComboBoxPropertiesItem
    {
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }

        public ComboBoxPropertiesItem() { }
        public ComboBoxPropertiesItem(DateTime pTimestamp, string pText)
        {
            TimeStamp = pTimestamp;
            Text = pText;
        }
    }
}
