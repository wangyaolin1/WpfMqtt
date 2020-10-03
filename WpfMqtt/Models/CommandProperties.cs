using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTT_Commander_WPF;

namespace MQTT_Client.Models
{
    public class CommandProperties

    {
        public MqttManager.Cmd Command { get; set; }

        public string Topic { get; set; }

        public string[] Topics { get; set; }

        public byte[] QosLevels { get; set; }

        public byte[] Message { get; set; }

        public byte QosLevel { get; set; }

        public bool Retain { get; set; }

        public bool CleanSession { get; set; }

        public string Client_Id { get; set; }

        public CommandProperties(MqttManager.Cmd pCommand, bool pCleanSession, bool pRetain, byte pQoSLevel, string pClient_Id = null, string pTopic = null, byte[] pMessage = null, string[] pTopics = null, byte[] pQosLevels = null) 
        {
            Command = pCommand;
            Topic = pTopic;
            Topics = pTopics;
            QosLevels = pQosLevels;
            Message = pMessage;
            QosLevel = pQoSLevel;
            Retain = pRetain;
            CleanSession = pCleanSession;
            Client_Id = pClient_Id;
        }

    }
}
