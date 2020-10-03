using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using MQTT_Client.Models;


namespace MQTT_Commander_WPF
{
    public class MqttManager

    {
        private bool _workThreadLoopShallRun = true;
        private bool _workThreadLoopIsRunning = false;
        private bool _hasClient = false;

        private readonly object theLock = new object();

        #region fields and methods belonging to Queue Buffer

        #region Fields belonging to Queue Buffer
        // This is the Queue to hold the commands
        private const int _defaultCapacity = 10;
        private const int _defaultPreFillLevel = 1;

        private bool _preFillLevelReached = true;

        private CommandProperties[] _buffer = new CommandProperties[_defaultCapacity];

        private int _head;
        private int _tail;
        private int _count;
        private int _capacity;
        private int _preFillLevel;
        #endregion

        #region Methods belonging to Queue Buffer
        public void InitializeQueue()
        {
            _capacity = _defaultCapacity;
            _head = 0;
            _tail = 0;
            _count = 0;
            _preFillLevel = _defaultPreFillLevel;
            _preFillLevelReached = true;
        }

        public int Count
        {
            get { return _count; }
        }

        public int preFillLevel
        {
            get { return _preFillLevel; }
            set { _preFillLevel = value; }
        }

        public int capacity
        {
            get { return _capacity; }
        }

        public bool preFillLevelReached
        {
            get { return _preFillLevelReached; }
            set { _preFillLevelReached = value; }
        }

        public void Clear()
        {
            _count = 0;
            _tail = _head;
        }


        public bool hasFreePlaces(int value = 1)
        {
            lock (theLock)
            {
                if (_count + value <= _capacity)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void EnqueueCommandProperties(CommandProperties CommandProperties)
        {
            lock (theLock)
            {
                if (_count == _capacity)
                {
                    Grow();
                    //Debug.Print("New Capacity: " + _buffer.Length);
                }
                _buffer[_head] = CommandProperties;
                _head = (_head + 1) % _capacity;
                _count++;
            }
        }


        public CommandProperties PreViewNextCommandProperties()
        {
            lock (theLock)
            {
                CommandProperties Value = DequeueCommandProperties(true);
                return Value;
            }
        }

        public CommandProperties DequeueNextCommandProperties()
        {
            lock (theLock)
            {
                CommandProperties Value = DequeueCommandProperties(false);
                return Value;
            }
        }

        private CommandProperties DequeueCommandProperties(bool PreView)
        {
            lock (theLock)
            {
                if (_count > 0)
                {
                    CommandProperties value = _buffer[_tail];

                    if (!PreView)
                    {
                        _tail = (_tail + 1) % _capacity;
                        _count--;
                    }
                    return value;
                }
                else
                {
                    return null;
                }
            }
        }

        private void Grow()
        {
            int newCapacity = _capacity << 1;
            CommandProperties[] newBuffer = new CommandProperties[newCapacity];

            if (_tail < _head)
            {
                Array.Copy(_buffer, _tail, newBuffer, 0, _count);
            }
            else
            {
                Array.Copy(_buffer, _tail, newBuffer, 0, _capacity - _tail);
                Array.Copy(_buffer, 0, newBuffer, _capacity - _tail, _head);
            }
            _buffer = newBuffer;
            _head = _count;
            _tail = 0;
            _capacity = newCapacity;
        }
        #endregion

        #endregion

        public bool WorkThreadLoopShallRun 
        { 
            get 
            {
                return _workThreadLoopShallRun;
            } 
            set
            {
                _workThreadLoopShallRun = value;
            }
        }


        public bool WorkThreadLoopIsRunning
        {
            get
            {
                return _workThreadLoopIsRunning;
            }
            set
            {
                _workThreadLoopIsRunning = value;
            }
        }


        public bool HasClient
        {
            get
            {
                return _hasClient;
            }
            set
            {
                _hasClient = value;
            }
        }

        public event EventHandler<ConnectEventArgs> ConnectResultReceived;

        public event EventHandler<MqttManagerMessageEventArgs> MqttManagerMessageReceived;

        public event EventHandler<SubscribedEventArgs> MqttManagerSubscribedReceived;

        public event EventHandler<UnsubscribedEventArgs> MqttManagerUnsubscribedReceived;

        public event EventHandler<PublishedEventArgs> MqttManagerPublishedAckReceived;

        public event EventHandler<PublishReceivedEventArg> MqttManagerPublishReceived;

        public bool HasEventhandler { get; set; }

        public enum Cmd
        {
            Connect,
            Disconnect,
            Subscribe,
            Unsubscribe,
            Publish,
            PublishReceived
        }

        public class PublishReceivedEventArg : EventArgs
        {
            public string Sender_Id { get; set; }

            public bool DupFlag { get; set; }

            public byte[] Message { get; set; }

            public bool Retain { get; set; }

            public string Topic { get; set; }

            public byte QoSLevel { get; set; }

            public PublishReceivedEventArg(string pSender_Id, string pTopic, byte[] pMessage, byte pQosLevel,  bool pDupFlag, bool pRetain)
            {
                Sender_Id = pSender_Id;
                DupFlag = pDupFlag;
                Message = pMessage;
                Retain = pRetain;
                Topic = pTopic;
                QoSLevel = pQosLevel;
            }
        }
        #region Class ConnectEventArgs

        public class ConnectEventArgs : EventArgs
        {
            public bool ConnectionState { get; set; }
            public string ExceptionMessage { get; set; }

            public string Sender_Id { get; set; }

            public byte ResultCode { get; set; }

            public ConnectEventArgs(string pSender_Id, bool pConnectionState, byte pResultCode, string pMessage)
            {
                ExceptionMessage = pMessage;
                ConnectionState = pConnectionState;
                Sender_Id = pSender_Id;
                ResultCode = pResultCode;
            }
        }
        #endregion 

        #region Class  PublishedEventArgs
        public class PublishedEventArgs : EventArgs
        {
            public string Sender_Id { get; set; }
            public ushort MessageId { get; set; }

            public bool IsPublished { get; set; }

            public PublishedEventArgs(string pSender_Id, ushort pMessageId, bool pIsPublished )
            {
                Sender_Id = pSender_Id;
                MessageId = pMessageId;
                IsPublished = pIsPublished;
            }
        }
        #endregion

        #region Class  UnsubscribedEventArgs
        public class UnsubscribedEventArgs : EventArgs
        {
            public string Sender_Id { get; set; }
            public ushort MessageId { get; set; }

            public UnsubscribedEventArgs(string pSender_Id, ushort pMessageId)
            {
                Sender_Id = pSender_Id;
                MessageId = pMessageId;
            }

        }
        #endregion

        #region Class SubscribedEventArgs
        public class SubscribedEventArgs : EventArgs
        {
            public string Sender_Id { get; set; }

            public ushort MessageId { get; set; }

            public byte[] GrantedQoSLevels { get; set; }

            public SubscribedEventArgs(string pSender_Id, ushort pMessageId, byte[] pGrantedQoSLevels)  
            {
                MessageId = pMessageId;
                GrantedQoSLevels = pGrantedQoSLevels;               
                Sender_Id = pSender_Id;               
            }
        }
        #endregion

        #region Class MqttManagerMessageEventArgs
        public class MqttManagerMessageEventArgs : EventArgs
        {
            public Cmd Command { get; set; }
            public string Message { get; set; }
            public string Sender_Id { get; set; }
            public bool State { get; set; }

            public byte ResultCode { get; set; }
            public string ExceptionMessage { get; set; }

            public MqttManagerMessageEventArgs(string pMessage, string pSender_Id, Cmd pCommand, bool pState, byte pResultCode, string pExceptMessage)
            {
                ExceptionMessage = pExceptMessage;
                State = pState;
                Message = pMessage;
                Sender_Id = pSender_Id;
                ResultCode = pResultCode;
                Command = pCommand;
            }
        }
        #endregion


        string _ioTEndPoint;
        string _name;
        int _brokerPort;
        bool _security;
        X509Certificate _caCert;
        X509Certificate2 _clientCert;
        MqttSslProtocols _sslProtocol;
        
        MqttClient client;

        // Constructor
        public MqttManager(string IoTEndpoint, string Name, bool Security, int BrokerPort, X509Certificate pCaCert, X509Certificate2 pClientCert, MqttSslProtocols SslProtocol)
        {
            InitializeQueue();
            HasEventhandler = false;

            _ioTEndPoint = IoTEndpoint;
            _brokerPort = BrokerPort;
            _name = Name;
            _security = Security;
            _caCert = pCaCert;
            _clientCert = pClientCert;
            _sslProtocol = SslProtocol;


            if (_ioTEndPoint != null)
            {
                if (Security)
                {
                    client = new MqttClient(_ioTEndPoint, _brokerPort, true, _caCert, _clientCert, MqttSslProtocols.TLSv1_2 /*this is what AWS IoT uses*/);
                }
                else
                {
                    client = new MqttClient(_ioTEndPoint, _brokerPort);
                }

                _hasClient = true;

            client.ConnectionClosed += Client_ConnectionClosed;
            client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
            client.MqttMsgUnsubscribed += Client_MqttMsgUnsubscribed;
            client.MqttMsgPublished += Client_MqttMsgPublished;
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

            }
        }


        #region Region Command Connect
        public void Connect(string pClient_Id, bool pCleanSession)
        {           
            if (hasFreePlaces())
            {
                EnqueueCommandProperties(new CommandProperties(Cmd.Connect, pCleanSession, false, 0, pClient_Id));
            }
            Thread workerThread = new Thread(new ThreadStart(runConnectThread));
            //workerThread.IsBackground = true;
            workerThread.Start();
        }

        // In this thread we run a loop which processes command that were stored in the queue
        private void runConnectThread()
        {
            CommandProperties _commandProperties;           
            byte connectResult = 0xff;
            string exceptionMessage = "";

            while (_workThreadLoopShallRun)
            {
                WorkThreadLoopIsRunning = true;
                if (Count > 0)
                {

                    _commandProperties = DequeueCommandProperties(false);

                    if (_commandProperties != null)
                    {
                        switch (_commandProperties.Command)
                        {
                            case Cmd.Connect:
                                {
                                    try
                                    {
                                        connectResult = client.Connect(_commandProperties.Client_Id, _commandProperties.CleanSession);
                                    }
                                    catch (Exception ex)
                                    {
                                        //string mess = ex.Message;
                                    }

                                    if (MqttManagerMessageReceived != null)
                                    {
                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Tried to connect ", _name, Cmd.Connect, (connectResult == 0), connectResult, "ResultCode :" + connectResult.ToString()));
                                    }

                                    if (ConnectResultReceived != null)
                                    {
                                        ConnectResultReceived(this, new ConnectEventArgs(_name, (connectResult == 0), connectResult, exceptionMessage));
                                    }
                                    break;
                                }
                            case Cmd.Disconnect:
                                {
                                    if (client.IsConnected)
                                    {
                                        client.Disconnect();
                                        if (MqttManagerMessageReceived != null)
                                        {
                                            MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Tried to disconnect ", _name, Cmd.Disconnect, false, 0, ""));
                                        }
                                    }
                                
                                    else
                                    {
                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Could not disconnect, Connection was closed!", _name, Cmd.Disconnect, false, 0x01, ""));
                                    }

                                break;
                                }


                            case Cmd.Subscribe:
                                {
                                    
                                    if (client.IsConnected)
                                    {                                       
                                        client.Subscribe(_commandProperties.Topics, _commandProperties.QosLevels);

                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Tried to subscribe", _name, Cmd.Subscribe, false, 0x01, ""));

                                    }
                                    else
                                    {
                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Could not subscribe, Connection was closed!", _name, Cmd.Subscribe, false, 0x01, ""));
                                    }
                                    break;
                                }

                            case Cmd.Unsubscribe:
                                {
                                    if (client.IsConnected)
                                    {
                                        client.Unsubscribe(_commandProperties.Topics);

                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Tried to unsubscribe", _name, Cmd.Unsubscribe, false, 0, ""));

                                    }
                                    else
                                    {
                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Could not unsubscribe, Connection was closed!", _name, Cmd.Unsubscribe, false, 0x01, ""));
                                    }
                                    break;
                                }
                            case Cmd.Publish:
                                {
                                    if (client.IsConnected)
                                    {
                                        client.Publish(_commandProperties.Topic, _commandProperties.Message, _commandProperties.QosLevel, _commandProperties.Retain);
                                       
                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Tried to publish", _name, Cmd.Publish, false, 0x00, ""));
                                    }
                                    else
                                    {
                                        MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Could not publish, Connection was closed", _name, Cmd.Publish, false, 0x00, ""));
                                    }
                                    break;
                                }
                        }
                    }
                }
                Thread.Sleep(100);
            }
            // Signal that thread is ending
            WorkThreadLoopIsRunning = false;
        }

        
        #endregion

        #region Region Subscribe
        public void Subscribe(string[] pTopics, byte[] pQosLevels)
        {          
            if (hasFreePlaces())
            {
                EnqueueCommandProperties(new CommandProperties(Cmd.Subscribe, false, false, 0, null, null, null, pTopics, pQosLevels));
            }          
        }
        #endregion

        #region Region Unsubscribe
        public void Unsubscribe(string[] pTopics)
        {
            if (hasFreePlaces())
            {
                EnqueueCommandProperties(new CommandProperties(Cmd.Unsubscribe, false, false, 0, null, null, null, pTopics, null));
            }         
        }
        #endregion

        #region Region Publish
        public void Publish(string pTopicPublish, byte[] pMessagePublish, byte pQosLevelPublish, bool pRetainPublish)
        {

            if (hasFreePlaces())
            {
                EnqueueCommandProperties(new CommandProperties(Cmd.Publish, false, pRetainPublish, pQosLevelPublish, null, pTopicPublish, pMessagePublish));
            }           
        }
        #endregion

        #region Region Disconnect
        public void Disconnect()
        {

            if (hasFreePlaces())
            {
                EnqueueCommandProperties(new CommandProperties(Cmd.Disconnect, false, false, 0));
            }           
        }
        #endregion



        #region Region Event Client_ConnectionClosed

        private void Client_ConnectionClosed(object sender, EventArgs e)
        {
            if (MqttManagerMessageReceived != null)
            {
                MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Connection Closed", _name, Cmd.Disconnect, false, 0x00, ""));
            }
            if (ConnectResultReceived != null)
            {
                ConnectResultReceived(this, new ConnectEventArgs(_name, false, 0x00, ""));
            }
        }
        #endregion

        #region Region Event Client_MqttMsgSubscribed
        private void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            if (MqttManagerMessageReceived != null)
            {
                MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Subscribed ", _name, Cmd.Subscribe, true, 0, ""));
            }

            if (MqttManagerSubscribedReceived != null)
            {
                MqttManagerSubscribedReceived(this, new SubscribedEventArgs(_name, e.MessageId, e.GrantedQoSLevels));
            }
           
        }
        #endregion

        #region Region Client_MqttMsgUnsubscribed
        private void Client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            if (MqttManagerMessageReceived != null)
            {
                
                    MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Unsubscribed ", _name, Cmd.Unsubscribe, true, 0, ""));
            }

            if (MqttManagerUnsubscribedReceived != null)
            {
                MqttManagerUnsubscribedReceived(this, new UnsubscribedEventArgs(_name, e.MessageId));
            }
        }
        #endregion

        #region Region Event Client_MqttMsgPublished
        private void Client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            if (MqttManagerMessageReceived != null)
            {
                MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Published ", _name, Cmd.Publish, true, 0, ""));
            }

            if (MqttManagerPublishedAckReceived != null)
            {
                MqttManagerPublishedAckReceived(this, new PublishedEventArgs(_name, e.MessageId, e.IsPublished));
            }
        }
        #endregion

        #region Region Event Client_MqttMsgPublishReceived
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (MqttManagerMessageReceived != null)
            {
                string facultMessage = "No String Message";
                try
                {
                    facultMessage = Encoding.UTF8.GetString(e.Message);
                }
                catch
                {}
                MqttManagerMessageReceived(this, new MqttManagerMessageEventArgs("Received from ", _name, Cmd.PublishReceived, true, 0, " Topic: " + e.Topic + " Message: " + facultMessage)); ;
            }

            if (MqttManagerPublishReceived != null)
            {
                MqttManagerPublishReceived(this, new PublishReceivedEventArg(_name, e.Topic, e.Message, e.QosLevel, e.DupFlag, e.Retain));
            }
            // Console.WriteLine("Received published Message");
        }
        #endregion
    }
}
