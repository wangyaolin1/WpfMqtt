// MQTT_Commander_WPF: Copyright RoSchmi  May 2020, License Apache V 2.0
//
// M2Mqtt.Net Library is Copyright (c) 2013, 2014 Paolo Patierno
// Eclipse Public License v1.0, Eclipse Distribution License v1.0

// Some interesting links:

// https://github.com/eclipse/paho.mqtt.m2mqtt

// https://gist.github.com/cwschroeder/7b5117dca561c01def041e7d4c6d2771

// https://m2mqtt.wordpress.com/m2mqtt-and-amazon-aws-iot/


// https://www.elektormagazine.de/news/mein-weg-ins-iot-4-mqtt

// Tutorials for MVVM:
// https://www.codeproject.com/Articles/165368/WPF-MVVM-Quick-Start-Tutorial
// https://www.codeproject.com/Tips/813345/Basic-MVVM-and-ICommand-Usage-Example

//How to create PFX file
// https://gist.github.com/adrenalinehit/b33994a4d430b26747ac#file-converting-to-pfx-using-openssl
// openssl pkcs12 -export -out YOURPFXFILE.pfx -inkey *****-private.pem.key -in *****-certificate.pem.crt

// https://www.wpf-tutorial.com/basic-controls/the-passwordbox-control/
// blog.functionalfun.net/2008/06/wpf-passwordbox-and-data-binding.html
// https://gigi.nullneuron.net/gigilabs/security-risk-in-binding-wpf-passwordbox-password/
//
// https://www.codeproject.com/Tips/549109/Working-with-SecureString



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
//using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using MvvmHelpers;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using MQTT_Commander_WPF;
using MQTT_Client.Models;
using System.Windows.Controls;
using System.Windows.Data;
using System.Security;
//using System.Runtime.InteropServices;

namespace MQTT_Client.ViewModels
{
    
    public class UI_ViewModel : BaseViewModel
    {
        #region constants and fields


        const string developerDirName = "RoSchmi";
        const string applicationDirName = "MqttCommander";
        const string fileAccountsName = "ClientsDictionaryStore.bin";
        const string fileComboBoxesName = "ComboBoxesContent.bin";
        static string filePath;
        static string fileAccounts;
        static string fileComboBoxes;
        static string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        const string QosLevel_0_Once = "0 -       Once";
        const string QosLevel_1_LeastOnce = "1 - Least Once";
        const string QosLevel_2_OnlyOnce = "2 -  Only Once";

        private Brush greyBrush  = (SolidColorBrush) (new BrushConverter().ConvertFrom("#ffdddddd"));
        private Brush greenBrush = System.Windows.Media.Brushes.LightGreen;

        X509Certificate _caCert;
        X509Certificate2 _clientCert;

        private ObservableCollection<string> url_Collection { get; set; }

        private ObservableCollection<string> _portsCollection { get; set; }

        private ObservableCollection<string> _qoSLevelsCollection { get; set; }

        private ObservableCollection<string> _qoSLevelsCollectionSubscribe { get; set; }

        private ObservableCollection<ComboBoxPropertiesItem> _topicsToSubscribeCollection { get; set; }

        private ObservableCollection<ComboBoxPropertiesItem> _topicsToPublishCollection { get; set; }

        private ObservableCollection<ComboBoxPropertiesItem> _messagesCollection { get; set; }



        public ICommand button_Takeover_clicked_Command { get; private set; }

        public ICommand button_Update_IpAddress_clicked_Command { get; private set; }

        public ICommand button_Connect_Clicked_Command { get; private set; }

        public ICommand button_Disconnect_Clicked_Command { get; private set; }

        public ICommand button_OpenCaCertDialog_Clicked_Command { get; private set; }

        public ICommand button_OpenClientCertDialog_Clicked_Command { get; private set; }

        public ICommand button_SaveAccountSettings_Clicked_Command { get; private set; }

        public ICommand button_NewAccount_Clicked_Command { get; private set; }

        public ICommand button_CreateAccount_Clicked_Command { get; private set; }

        public ICommand button_DeleteAccount_Clicked_Command { get; private set; }

        public ICommand button_CancelAccount_Clicked_Command { get; private set; }

        public ICommand button_Subscribe_Clicked_Command { get; private set; }
        public ICommand button_UnSubscribe_Clicked_Command { get; private set; }

        public ICommand button_Publish_Clicked_Command { get; private set; }

        public ICommand button_ChangeTopicSubscribe_Clicked_Command { get; private set; }

        public ICommand button_ChangeTopicPublish_Clicked_Command { get; private set; }

        public ICommand button_ChangeMessages_Clicked_Command { get; private set; }

        public ICommand button_Help_Clicked_Command { get; private set; }


        static Dictionary<string, AccountMembers> accountsDictionary = new Dictionary<string, AccountMembers>();

        static Dictionary<string, ObservableCollection<ComboBoxPropertiesItem>> comboBoxesContentDictionary = new Dictionary<string, ObservableCollection<ComboBoxPropertiesItem>>();

        private bool canExecute = true;      

        private string _actualBrokerUrl;

        private string _lastBrokerUrl;

        private string _actualBrokerUrlClient;

        private string _lastBrokerUrlClient;

        private string _caCertPath;

        private string _clientCertPath;

        private bool _tls_Security;

        private string _password;

        public static SecureString the_p_w_d;

        private string _actualBrokerIpAddress;

        private string _selectedPort;

        private string _clientName;

        private string _user;

        private string _tbDataText;

        private string _topicSubscribe;

        private ComboBoxPropertiesItem _topicSubscribeShadow;

        private string _topicPublish;

        private string _messagePublish;

        private bool _publish_Retain;

        private string _selectedPuplishQoSLevel;

        private string _selectedSubscribeQoSLevel;

        private bool _connect_CleanSession;

        private bool _showBrokerMessages;

        private bool _showClientMessages;

        private string _receivedMessage;

        private string _connectText;

        private System.Windows.Media.Brush _connectColor;

        private ComboBoxPropertiesItem _topicPublishShadow;

        private ComboBoxPropertiesItem _messageShadow;

        private Visibility _comboBox_Url_Visibility;

        private Visibility _textBox_Url_Visibility;

        private Visibility _btnCreate_Visibility;

        private Visibility _btnCancelAccount_Visibility;

        private Visibility _btnDeleteAccount_Visibility;

        private Visibility _btnUpdateAccount_Visibility;

        private Visibility _btnNewAccount_Visibility;

        private Visibility _comboBox_TopicsToSubsribe_Visibility;

        private Visibility _textBox_TopicsToSubsribe_Visibility;

        private Visibility _comboBox_TopicsToPublish_Visibility;

        private Visibility _textBox_TopicsToPublish_Visibility;

        private Visibility _textBox_Messages_Visibility;

        private Visibility _comboBox_Messages_Visibility;

        
            

        MqttManager myMqttManager;
        #endregion



        #region UI_ViewModel Constructor ******************************
        public UI_ViewModel()
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            filePath = Path.Combine(new string[3] { basePath, developerDirName, applicationDirName });
            fileAccounts = Path.Combine(new string[2] { filePath, fileAccountsName });
            fileComboBoxes = Path.Combine(new string[2] { filePath, fileComboBoxesName });

            #region Region Read back (or initialize) stored settings of created Mqtt-Clients and contents of ComboBoxes

            // Read Mqtt-Clients
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (!File.Exists(fileAccounts))
            {                
                accountsDictionary.Add("test.mosquitto.org -&- myPcClient", new AccountMembers(null, "myPcClient", false, "", "", "", "", null, "1883"));
                accountsDictionary.Add("a4-example_9mj-ats.iot.eu-central-1.amazonaws.com  -&- myAwsClient_No_1", new AccountMembers(null, "myAwsClient_No_1", true, "", "", "", "", null, "8883"));
                WriteAccountsDictionaryToFile(accountsDictionary, fileAccounts);
            }
            else
            {
                try
                {
                    accountsDictionary = ReadAcccountsDictionaryFromFile(fileAccounts);
                }
                catch
                {
                    throw new Exception("Accountsfile could not be read");
                }
            }

            url_Collection = new ObservableCollection<string>();

            foreach(var pair in accountsDictionary)
            {
                url_Collection.Add(pair.Key);
            }
            

            // Read contents of Comboboxes
            if (!File.Exists(fileComboBoxes))
            {
                
                comboBoxesContentDictionary.Add("SubscribeItems", new ObservableCollection<ComboBoxPropertiesItem>() { new ComboBoxPropertiesItem(DateTime.UtcNow, "myTopic") });
                comboBoxesContentDictionary.Add("PublishItems", new ObservableCollection<ComboBoxPropertiesItem>() { new ComboBoxPropertiesItem(DateTime.UtcNow, "myTopic") });
                comboBoxesContentDictionary.Add("MessageItems", new ObservableCollection<ComboBoxPropertiesItem>() { new ComboBoxPropertiesItem(DateTime.UtcNow, "Hello World from MQTT") });

                WriteComboBoxesContentDictionaryToFile(comboBoxesContentDictionary, fileComboBoxes);
            }
            else
            {
                try
                {
                    comboBoxesContentDictionary = ReadComboBoxesContentDictionaryFromFile(fileComboBoxes);
                }
                catch
                {
                    throw new Exception("ComboBoxfiles could not be read");
                }
            }

            _topicsToPublishCollection = new ObservableCollection<ComboBoxPropertiesItem>();
            _topicsToSubscribeCollection = new ObservableCollection<ComboBoxPropertiesItem>();
            _messagesCollection = new ObservableCollection<ComboBoxPropertiesItem>();

            foreach (var pair in comboBoxesContentDictionary)
            {
                switch (pair.Key)
                {
                    case "SubscribeItems":
                        {

                            foreach (var item in pair.Value)
                            {
                                _topicsToSubscribeCollection.Add(item);
                            }
                           break;
                        }
                    case "PublishItems":
                        {
                            foreach (var item in pair.Value)
                            {
                                _topicsToPublishCollection.Add(item);
                            }
                            break;
                        }
                    case "MessageItems":
                        {
                            foreach (var item in pair.Value)
                            {
                                _messagesCollection.Add(item);
                            }
                            break;
                        }
                }
            }
            #endregion

            #region Region Presets
            TextBox_Url_Visibility = Visibility.Hidden;
            BtnCreate_Visibility = Visibility.Hidden;
            BtnCancelAccount_Visibility = Visibility.Hidden;
            TextBox_TopicsToSubsribe_Visibility = Visibility.Hidden;
            TextBox_TopicsToPublish_Visibility = Visibility.Hidden;
            TextBox_Messages_Visibility = Visibility.Hidden;

            ConnectColor = greyBrush;
            ConnectText = "Not Connected";

            _connect_CleanSession = false;
            _showClientMessages = true;
            _showBrokerMessages = true;

            _portsCollection = new ObservableCollection<string>();
            _portsCollection.Add("8883");
            _portsCollection.Add("443");
            _portsCollection.Add("1883");


            _qoSLevelsCollection = new ObservableCollection<string>();
            _qoSLevelsCollection.Add(QosLevel_0_Once);
            _qoSLevelsCollection.Add(QosLevel_1_LeastOnce);
            _qoSLevelsCollection.Add(QosLevel_2_OnlyOnce);

            _selectedPuplishQoSLevel = QosLevel_0_Once;

            _qoSLevelsCollectionSubscribe = new ObservableCollection<string>();
            _qoSLevelsCollectionSubscribe.Add(QosLevel_0_Once);
            _qoSLevelsCollectionSubscribe.Add(QosLevel_1_LeastOnce);
            _qoSLevelsCollectionSubscribe.Add(QosLevel_2_OnlyOnce);

            _selectedSubscribeQoSLevel = QosLevel_0_Once;

            SelectedPort = "18831";

            TLS_Security = false;
            #endregion

            #region Region Defining eventhandlers for Commands
            Button_Update_IpAddress_clicked_Command = new RelayCommand(Button_Update_IpAddress_clicked_Action, param => this.canExecute);
            Button_Takeover_clicked_Command = new RelayCommand(Button_Takeover_Clicked_Action, param => this.canExecute);
            Button_Connect_Clicked_Command = new RelayCommand(Button_Connect_Clicked_Action, param => this.canExecute);
            Button_Disconnect_Clicked_Command = new RelayCommand(Button_Disconnect_Clicked_Action, param => this.canExecute);
            Button_OpenCaCertDialog_Clicked_Command = new RelayCommand(Button_OpenCaCertDialog_Clicked_Action, param => this.canExecute);
            Button_OpenClientCertDialog_Clicked_Command = new RelayCommand(Button_OpenClientCertDialog_Clicked_Action, param => this.canExecute);
            Button_SaveAccountSettings_Clicked_Command = new RelayCommand(Button_SaveAccountSettings_Clicked_Action, param => this.canExecute);
            Button_NewAccount_Clicked_Command = new RelayCommand(Button_NewAccount_Clicked_Action, param => this.canExecute);
            Button_CreateAccount_Clicked_Command = new RelayCommand(Button_CreateAccount_Clicked_Action, param => this.canExecute);
            Button_DeleteAccount_Clicked_Command = new RelayCommand(Button_DeleteAccount_Clicked_Action, param => this.canExecute);
            Button_CancelAccount_Clicked_Command = new RelayCommand(Button_CancelAccount_Clicked_Action, param => this.canExecute);
            Button_Subscribe_Clicked_Command = new RelayCommand(Button_Subscribe_Clicked_Action, param => this.canExecute);
            Button_UnSubscribe_Clicked_Command = new RelayCommand(Button_UnSubscribe_Clicked_Action, param => this.canExecute);
            Button_Publish_Clicked_Command = new RelayCommand(Button_Publish_Clicked_Action, param => this.canExecute);
            Button_ChangeTopicSubscribe_Clicked_Command = new RelayCommand(Button_ChangeTopicSubscribe_Clicked_Action, param => this.canExecute);
            Button_ChangeTopicPublish_Clicked_Command = new RelayCommand(Button_ChangeTopicPublish_Clicked_Action, param => this.canExecute);
            Button_ChangeMessages_Clicked_Command = new RelayCommand(Button_ChangeMessages_Clicked_Action, param => this.canExecute);
            Button_Help_Clicked_Command = new RelayCommand(Button_Help_Clicked_Action, param => this.canExecute);
            #endregion

        }
        #endregion



        #region Region Method GetIPAddress
        IPAddress GetIPAddress(string pBrokerAddress)
        {
            try
            {
                IPAddress[] IpAddresses = System.Net.Dns.GetHostEntry(pBrokerAddress).AddressList;
                return IpAddresses[0];
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Region public bool CanExecute
        public bool CanExecute
        {
            get
            {
                return this.canExecute;
            }

            set
            {
                if (this.canExecute == value)
                {
                    return;
                }

                this.canExecute = value;
            }
        }
        #endregion

        #region Region public void ShowMessage
        public void ShowMessage(object obj)
        {

            MessageBox.Show(obj.ToString());
        }
        #endregion

        #region Region public void ChangeCanExecute
        public void ChangeCanExecute(object obj)
        {
            canExecute = !canExecute;
        }
        #endregion


        #region Region Button_Takeover_clicked_Command
        public ICommand Button_Takeover_clicked_Command
        {
            get { return button_Takeover_clicked_Command; }
            set { button_Takeover_clicked_Command = value; }
        }
        #endregion

        #region Region Button_Takeover_Clicked_Action
        public void Button_Takeover_Clicked_Action(object obj)
        {
            TopicPublish = TopicSubscribe;
        }
        #endregion


        #region Button_Update_IpAddress_clicked_Command
        public ICommand Button_Update_IpAddress_clicked_Command
        {
            get { return button_Update_IpAddress_clicked_Command; }
            set { button_Update_IpAddress_clicked_Command = value; }
        }
        #endregion

        #region Button_Update_IpAddress_clicked_Action
        public void Button_Update_IpAddress_clicked_Action(object obj)
        {
            IPAddress iPAddress = GetIPAddress(ActualBrokerUrl);
            ActualBrokerIpAddress = iPAddress == null ? "No Ip-Addr. found" : iPAddress.ToString();
        }
        #endregion


        #region Region Button_OpenClientCertDialog_Clicked_Command
        public ICommand Button_OpenClientCertDialog_Clicked_Command
        {
            get { return button_OpenClientCertDialog_Clicked_Command; }
            set { button_OpenClientCertDialog_Clicked_Command = value; }
        }
        #endregion

        #region Button_OpenClientCertDialog_Clicked_Action
        public void Button_OpenClientCertDialog_Clicked_Action(object obj)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".PFX";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";            
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                ClientCertPath = dlg.FileName;
            }
        }
        #endregion


        #region RegionButton_OpenCaCertDialog_Clicked_Command
        public ICommand Button_OpenCaCertDialog_Clicked_Command
        {
            get { return button_OpenCaCertDialog_Clicked_Command; }
            set { button_OpenCaCertDialog_Clicked_Command = value; }
        }
        #endregion

        #region Region Button_OpenCaCertDialog_Clicked_Action
        public void Button_OpenCaCertDialog_Clicked_Action(object obj)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.DefaultExt = ".png";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";           
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                CaCertPath = dlg.FileName;
            }
        }
        #endregion


        #region Button_SaveAccountSettings_Clicked_Command
        public ICommand Button_SaveAccountSettings_Clicked_Command
        {
            get { return button_SaveAccountSettings_Clicked_Command; }
            set { button_SaveAccountSettings_Clicked_Command = value; }
        }
        #endregion

        #region Button_SaveAccountSettings_Clicked_Action
        public void Button_SaveAccountSettings_Clicked_Action(object obj)
        {
            var messageBoxResult = MessageBox.Show(obj.ToString(), "Alert", MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                IPAddress iPAddress = GetIPAddress(ActualBrokerUrl);
                ActualBrokerIpAddress = iPAddress == null ? "No Ip-Addr. found" : iPAddress.ToString();

                Update_AccountMembers_From_Gui(ActualBrokerUrl, (iPAddress != null));

                if (!url_Collection.Contains(ActualBrokerUrlClient))
                {
                    url_Collection.Add(ActualBrokerUrlClient);
                }
                TextBox_Url_Visibility = Visibility.Hidden;
                ComboBox_Url_Visibility = Visibility.Visible;

            }
        }
        #endregion


        #region Region Button_CancelAccount_Clicked_Command
        public ICommand Button_CancelAccount_Clicked_Command
        {
            get { return button_CancelAccount_Clicked_Command; }
            set { button_CancelAccount_Clicked_Command = value; }
        }
        #endregion

        #region Region Event Button_CancelAccount_Clicked_Action
        public void Button_CancelAccount_Clicked_Action(object obj)
        {
            BtnCreate_Visibility = Visibility.Hidden;
            BtnCancelAccount_Visibility = Visibility.Hidden;
            BtnDeleteAccount_Visibility = Visibility.Visible;
            BtnNewAccount_Visibility = Visibility.Visible;
            BtnUpdateAccount_Visibility = Visibility.Visible;
            TextBox_Url_Visibility = Visibility.Hidden;
            ComboBox_Url_Visibility = Visibility.Visible;
            ActualBrokerUrl = _lastBrokerUrl;
        }
        #endregion


        #region Region Button_DeleteAccount_Clicked_Command
        public ICommand Button_DeleteAccount_Clicked_Command
        {
            get { return button_DeleteAccount_Clicked_Command; }
            set { button_DeleteAccount_Clicked_Command = value; }
        }
        #endregion

        #region Region Event Button_DeleteAccount_Clicked_Action
        public void Button_DeleteAccount_Clicked_Action(object obj)
        {
            var messageBoxResult = MessageBox.Show(obj.ToString(), "Alert", MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                if (accountsDictionary.ContainsKey(ActualBrokerUrlClient))
                {
                    if (accountsDictionary[ActualBrokerUrlClient] != null)
                    {
                        MqttManager theManager = (MqttManager)accountsDictionary[ActualBrokerUrlClient].AccessInstance;
                        if (theManager != null)
                        {
                            theManager.Disconnect();
                            theManager.WorkThreadLoopShallRun = false;
                        }
                    }
                    accountsDictionary.Remove(ActualBrokerUrlClient);
                    ConnectColor = greyBrush;
                }

                if (url_Collection.Contains(ActualBrokerUrlClient))
                {
                    int indexToDelete = url_Collection.IndexOf(ActualBrokerUrlClient);
                    string oldBrokerUrl = ActualBrokerUrlClient;
                    if (url_Collection.Count > 1)
                    {
                        if (url_Collection.IndexOf(ActualBrokerUrlClient) == 0)
                        {
                            ActualBrokerUrlClient = url_Collection[1];
                        }
                        else
                        {
                            ActualBrokerUrlClient = url_Collection[0];
                        }
                        url_Collection.Remove(oldBrokerUrl);
                    }
                }
            }


            //ComboBox_Url_Visibility = ComboBox_Url_Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            //TextBox_Url_Visibility = ComboBox_Url_Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion


        #region Region button_CreateAccount_Clicked_Command
        public ICommand Button_CreateAccount_Clicked_Command
        {
            get { return button_CreateAccount_Clicked_Command; }
            set { button_CreateAccount_Clicked_Command = value; }
        }
        #endregion

        #region Region Event Button_CreateAccount_Clicked_Action
        public void Button_CreateAccount_Clicked_Action(object obj)
        {
            IPAddress iPAddress = GetIPAddress(ActualBrokerUrl);
            ActualBrokerIpAddress = iPAddress == null ? "No Ip-Addr. found" : iPAddress.ToString();
            if (iPAddress == null)
            {
                MessageBox.Show("Couldn't find Ip-Address for this Url", "Alert", MessageBoxButton.OK);
            }

            Update_AccountMembers_From_Gui(ActualBrokerUrl, (iPAddress != null));

            BtnDeleteAccount_Visibility = Visibility.Visible;
            BtnNewAccount_Visibility = Visibility.Visible;
            BtnUpdateAccount_Visibility = Visibility.Visible;


            Update_AccountMembers_From_Gui(ActualBrokerUrl, (iPAddress != null));

            if (!url_Collection.Contains(ActualBrokerUrlClient))
            {
                url_Collection.Add(ActualBrokerUrlClient);
            }
            TextBox_Url_Visibility = Visibility.Hidden;
            ComboBox_Url_Visibility = Visibility.Visible;

            BtnCreate_Visibility = Visibility.Hidden;
            BtnCancelAccount_Visibility = Visibility.Hidden;
        }
        #endregion


        #region Region button_NewAccount_Clicked_Command
        public ICommand Button_NewAccount_Clicked_Command
        {
            get { return button_NewAccount_Clicked_Command; }
            set { button_NewAccount_Clicked_Command = value; }
        }
        #endregion

        #region Region Event Button_NewAccount_Clicked_Action
        public void Button_NewAccount_Clicked_Action(object obj)
        {
            BtnCreate_Visibility = Visibility.Visible;
            BtnCancelAccount_Visibility = Visibility.Visible;
            BtnDeleteAccount_Visibility = Visibility.Hidden;
            BtnNewAccount_Visibility = Visibility.Hidden;
            BtnUpdateAccount_Visibility = Visibility.Hidden;

            ComboBox_Url_Visibility = ComboBox_Url_Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            TextBox_Url_Visibility = ComboBox_Url_Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion


        #region Region button_Connect_Clicked_Command
        public ICommand Button_Connect_Clicked_Command
        {
            get { return button_Connect_Clicked_Command; }
            set { button_Connect_Clicked_Command = value; }
        }
        #endregion

        #region Region Event Button_Connect_Clicked_Action
        public void Button_Connect_Clicked_Action(object obj)
        {
           
            IPAddress iPAddress = GetIPAddress(ActualBrokerUrl);
            ActualBrokerIpAddress = iPAddress == null ? "No Ip-Addr. found" : iPAddress.ToString();

            Update_AccountMembers_From_Gui(ActualBrokerUrl, (iPAddress != null));

            if (!url_Collection.Contains(ActualBrokerUrlClient))
            {
                url_Collection.Add(ActualBrokerUrlClient);
            }


            myMqttManager = (MqttManager)accountsDictionary[ActualBrokerUrlClient].AccessInstance;

            if (myMqttManager.HasClient)
            {
                try
                {
                    myMqttManager.Connect(ClientName, Connect_CleanSession);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Update_AccountMembers_From_Gui(ActualBrokerUrl, (iPAddress != null));
                myMqttManager = (MqttManager)accountsDictionary[ActualBrokerUrlClient].AccessInstance;
                if (iPAddress != null)
                {
                    try
                    {
                        myMqttManager.Connect(ClientName, Connect_CleanSession);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Cannot connect, found no Ip-Address for this Url");
                }
            }


            if (!myMqttManager.HasEventhandler)

            {

                myMqttManager.ConnectResultReceived += MyMqttManager_ConnectResultReceived;

                myMqttManager.MqttManagerSubscribedReceived += MyMqttManager_MqttManagerSubscribedReceived;

                myMqttManager.MqttManagerUnsubscribedReceived += MyMqttManager_MqttManagerUnsubscribedReceived;

                myMqttManager.MqttManagerPublishedAckReceived += MyMqttManager_MqttManagerPublishedAckReceived;

                myMqttManager.MqttManagerPublishReceived += MyMqttManager_MqttManagerPublishReceived;







                myMqttManager.MqttManagerMessageReceived += MyMqttManager_MqttManagerMessageReceived;



                myMqttManager.HasEventhandler = true;

            }

        }

        /*

      if (!myMqttManager.HasEventhandler)

            {

                myMqttManager.ConnectResultReceived += MyMqttManager_ConnectResultReceived;              

                myMqttManager.MqttManagerSubscribedReceived += MyMqttManager_MqttManagerSubscribedReceived;

                myMqttManager.MqttManagerUnsubscribedReceived += MyMqttManager_MqttManagerUnsubscribedReceived;

                myMqttManager.MqttManagerPublishedAckReceived += MyMqttManager_MqttManagerPublishedAckReceived;

                myMqttManager.MqttManagerPublishReceived += MyMqttManager_MqttManagerPublishReceived;

                





                myMqttManager.MqttManagerMessageReceived += MyMqttManager_MqttManagerMessageReceived;



                myMqttManager.HasEventhandler = true;

            }



            try

            {

                myMqttManager.Connect(ClientName, Connect_CleanSession);

            }

            catch (Exception ex)

            {

                Console.WriteLine(ex.Message);

            }
            */
            /*
            try
            {
                myMqttManager.Connect(ClientName, Connect_CleanSession);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            */
          
        


        #endregion


        #region Region Button_ChangeTopicSubscribe_Clicked_Command
        public ICommand Button_ChangeTopicSubscribe_Clicked_Command
        {
            get { return button_ChangeTopicSubscribe_Clicked_Command; }
            set { button_ChangeTopicSubscribe_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_ChangeTopicSubscribe_Clicked_Action
        public void Button_ChangeTopicSubscribe_Clicked_Action(object obj)
        {
            ComboBox_TopicsToSubsribe_Visibility = ComboBox_TopicsToSubsribe_Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            TextBox_TopicsToSubsribe_Visibility = ComboBox_TopicsToSubsribe_Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;          
        }
        #endregion


        #region Region Button_ChangeTopicPublish_Clicked_Command
        public ICommand Button_ChangeTopicPublish_Clicked_Command
        {
            get { return button_ChangeTopicPublish_Clicked_Command; }
            set { button_ChangeTopicPublish_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_ChangeTopicSubscribe_Clicked_Action
        public void Button_ChangeTopicPublish_Clicked_Action(object obj)
        {
           
            ComboBox_TopicsToPublish_Visibility = ComboBox_TopicsToPublish_Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

            TextBox_TopicsToPublish_Visibility = ComboBox_TopicsToPublish_Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }
        #endregion


        #region Region Button_ChangeMessages_Clicked_Command
        public ICommand Button_ChangeMessages_Clicked_Command
        {
            get { return button_ChangeMessages_Clicked_Command; }
            set { button_ChangeMessages_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_ChangeMessages_Clicked_Action
        public void Button_ChangeMessages_Clicked_Action(object obj)
        {
            ComboBox_Messages_Visibility = ComboBox_Messages_Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

            TextBox_Messages_Visibility = ComboBox_Messages_Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }
        #endregion

        
        #region Region Button_Help_Clicked_Command
        public ICommand Button_Help_Clicked_Command
        {
            get { return button_Help_Clicked_Command; }
            set { button_Help_Clicked_Command = value; }
        }
#endregion

        #region Event Button_Help_Clicked_Action
        public void Button_Help_Clicked_Action(object obj)
        {
            var dialogResult = MessageBox.Show("Help information is written to the 'Console' of this App. The actual content of the Console will be deleted.", "Mqtt-Commander Info", MessageBoxButton.OKCancel);
           

            if (dialogResult == MessageBoxResult.OK)
            {
                TbDataText = "Tips how to use Mqtt-Commander:\r\n\r\n"
                   + "Actually the App was tested with AWS IoT (TLS V. 1.2) and Mosquitto MQTT-Broker (without transport security).\r\n"
                   + "If it works with other Brokers has to be tested by yourself.\r\n"
                   + "To set up a new MQTT-Client you have to enter at least the broker url, the Client-Id and the Port.\r\n"
                   + "Click on 'New Client', enter the needed things and click 'Create'\r\n"
                   + "If the entered url is valid, you will see the IP-Address at the upper right corner.\r\n"
                   + "Now you can see the name of the new Client in the 'Client' Combobox. The name consists of the url, the separator '-&-' and the Client-Id.\r\n"
                   + "You can have numerous Client-Ids for each account.\r\n\r\n"
                   + "Settings for AWS-IoT:\r\n"
                   + "To accomplish AWS-IoT secure transmission and authentication you have to select the location of the AWS-IoT CaCertificate (e.g. 'AmazonRootCA1.crt')"
                   + "and the location of the Client-Certificate, which must be stored on your PC. Additionally 'TLS-Security' must be checked, the Port must be set to '8883' and the Password must be entered.\r\n"
                   + "The Client Certificate must be in PKCS12 Format (extension .pfx).\r\n"
                   + "To create the Client Certificate in this special format it has to be generated with the program 'OpenSsl' using the following command:\r\n"
                   + "'openssl pkcs12 -export -out YOURPFXFILE.pfx - inkey *****-private.pem.key -in *****-certificate.pem.crt'\r\n"
                   + "'*****-private.pem.key' and '*****-certificate.pem.crt' are the files which you got from Amazon.\r\n"
                   + "(-https://docs.aws.amazon.com/iot/latest/developerguide/device-certs-create.html)\r\n\r\n"
                   + "The password has to be entered new every time you start the 'Mqtt-Commander'\r\n" 
                   + "For security reasons it is not stored in a file on your PC.\r\n\r\n";
            }

// openssl pkcs12 -export -out YOURPFXFILE.pfx - inkey * ****-private.pem.key -in *****-certificate.pem.crt
        }
        #endregion


        #region Region Button_Publish_Clicked_Command
        public ICommand Button_Publish_Clicked_Command
        {
            get { return button_Publish_Clicked_Command; }
            set { button_Publish_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_Publish_Clicked_Action
        public void Button_Publish_Clicked_Action(object obj)
        {
            if (!String.IsNullOrEmpty(TopicPublish)) 
            {              
                string Topic = TopicPublish;

                byte qosLevel = (SelectedPuplishQoSLevel == QosLevel_0_Once) ? MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE 
                    : (SelectedPuplishQoSLevel == QosLevel_1_LeastOnce) ? MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE : MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;


                // publish a message with QoS 0, QoS 1 or QoS 2. Seems that Q0S2 doesn't work with AWS IoT
                myMqttManager.Publish(Topic, Encoding.UTF8.GetBytes(MessagePublish ?? ""), qosLevel, Publish_Retain );
            }
            else
            {
                System.Windows.MessageBox.Show("You have to enter a topic to publish!");
            }          
        }
        #endregion

       
        #region Region Button_UnSubscribe_Clicked_Command
        public ICommand Button_UnSubscribe_Clicked_Command
        {
            get { return button_UnSubscribe_Clicked_Command; }
            set { button_UnSubscribe_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_UnSubscribe_Clicked_Action
        public void Button_UnSubscribe_Clicked_Action(object obj)
        {
          
            if (!String.IsNullOrEmpty(TopicSubscribe))
            {               
                string Topic = TopicSubscribe;
                myMqttManager.Unsubscribe(new string[] { Topic });
            }
            else

            {
               System.Windows.MessageBox.Show("You have to enter a topic to unsubscribe!");
            }
        }
        #endregion


        #region Region Button_Subscribe_Clicked_Command
        public ICommand Button_Subscribe_Clicked_Command
        {
            get { return button_Subscribe_Clicked_Command; }
            set { button_Subscribe_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_Subscribe_Clicked_Action
        public void Button_Subscribe_Clicked_Action(object obj)
        {
            myMqttManager = (MqttManager)accountsDictionary[ActualBrokerUrlClient].AccessInstance;
            if (!String.IsNullOrEmpty(TopicSubscribe))
            {
                string Topic = TopicSubscribe;

                byte qosLevel = (SelectedSubscribeQoSLevel == QosLevel_0_Once) ? MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE
                    : (SelectedSubscribeQoSLevel == QosLevel_1_LeastOnce) ? MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE : MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;

                myMqttManager.Subscribe(new string[] { Topic }, new byte[] { qosLevel });   // we need arrays as parameters because we can subscribe to different topics with one call
            }
            else
            {
                System.Windows.MessageBox.Show("You have to enter a topic to subscribe!");
            }
        }
        #endregion


        #region Region Button_Disconnect_Clicked_Command
        public ICommand Button_Disconnect_Clicked_Command
        {
            get { return button_Disconnect_Clicked_Command; }
            set { button_Disconnect_Clicked_Command = value; }
        }
        #endregion

        #region Event Button_Disconnect_Clicked_Action
        public void Button_Disconnect_Clicked_Action(object obj)
        {
            myMqttManager = (MqttManager)accountsDictionary[ActualBrokerUrlClient].AccessInstance;            
            if (myMqttManager != null)
            {
                myMqttManager.Disconnect();
            }
        }
        #endregion


        #region Region Event MyMqttManager_ConnectResultReceived
        private void MyMqttManager_ConnectResultReceived(object sender, MqttManager.ConnectEventArgs e)
        {
            if (_showBrokerMessages)
            {
                int maxTextLength = 1000; // maximum text length in text box

                if ((TbDataText ?? "").Length > maxTextLength)
                {
                    TbDataText = TbDataText.Remove(0, TbDataText.Length - maxTextLength);                   
                }

                TbDataText = TbDataText + e.Sender_Id + ": " + (e.ConnectionState ? "Connected" : "Connection Closed") + "\r\n";
                //tbData.ScrollToEnd();
            }

            ConnectText = e.ConnectionState ? "Connected" : "Connection Closed";
            ConnectColor = e.ConnectionState ? greenBrush : greyBrush;

            Console.WriteLine(e.ConnectionState ? "Connected: " + sender.ToString() : "Connection closed: " + sender.ToString() + " " + e.ExceptionMessage);          
        }
        #endregion

        #region Region Event MyMqttManager_MqttManagerSubscribedReceived
        private void MyMqttManager_MqttManagerSubscribedReceived(object sender, MqttManager.SubscribedEventArgs e)
        {
            if (_showBrokerMessages)
            {
                int maxTextLength = 1000; // maximum text length in text box
                if ((TbDataText ?? "").Length > maxTextLength)
                {
                    TbDataText = TbDataText.Remove(0, TbDataText.Length - maxTextLength);
                }
                TbDataText = TbDataText + e.Sender_Id + ": Message: " + e.MessageId + " Granted QoSLevels " + e.GrantedQoSLevels.Length + "\r\n";
            }
            // Console.WriteLine("Subscribed");
        }
        #endregion

        #region Region Event MyMqttManager_MqttManagerUnsubscribedReceived
        private void MyMqttManager_MqttManagerUnsubscribedReceived(object sender, MqttManager.UnsubscribedEventArgs e)
        {
            if (_showBrokerMessages)
            {
                int maxTextLength = 1000; // maximum text length in text box
                if ((TbDataText ?? "").Length > maxTextLength)
                {
                    TbDataText = TbDataText.Remove(0, TbDataText.Length - maxTextLength);
                }
                TbDataText = TbDataText + e.Sender_Id + ": MessageId: " + e.MessageId + "\r\n";
            }
            // Console.WriteLine("Unsubscribed");
        }
        #endregion

        #region Region Event MyMqttManager_MqttManagerMessageReceived
        private void MyMqttManager_MqttManagerMessageReceived(object sender, MqttManager.MqttManagerMessageEventArgs e)
        {
            if (_showClientMessages)
            {
                if (e.Command == MqttManager.Cmd.Connect)
                {
                    // This is only to show how messages can be selected
                }
                int maxTextLength = 1000; // maximum text length in text box
                if ((TbDataText ?? "").Length > maxTextLength)
                {
                    TbDataText = TbDataText.Remove(0, TbDataText.Length - maxTextLength);
                }
                TbDataText = TbDataText + e.Sender_Id + ": Message: " + e.Message + " " + e.ExceptionMessage + "\r\n";
            }
        }
        #endregion

        #region Region Event MyMqttManager_MqttManagerPublishedAckReceived
        private void MyMqttManager_MqttManagerPublishedAckReceived(object sender, MqttManager.PublishedEventArgs e)
        {
            if (_showBrokerMessages)
            {
                int maxTextLength = 1000; // maximum text length in text box
                if ((TbDataText ?? "").Length > maxTextLength)
                {
                    TbDataText = TbDataText.Remove(0, TbDataText.Length - maxTextLength);
                }
                TbDataText = TbDataText + e.Sender_Id + ": MessageId: " + e.MessageId + " Result: " + (e.IsPublished ? "Published" : "Failed to publish") + "\r\n";
            }

            //Console.WriteLine("Published");
        }
        #endregion

        #region Region Event MyMqttManager_MqttManagerPublishReceived
        private void MyMqttManager_MqttManagerPublishReceived(object sender, MqttManager.PublishReceivedEventArg e)
        {
            string facultMessage = "No String Message";
            try
            {
                facultMessage = Encoding.UTF8.GetString(e.Message);
            }
            catch
            { }

            ReceivedMessage = facultMessage;

            if (_showBrokerMessages)
            {
                int maxTextLength = 1000; // maximum text length in text box
                if ((TbDataText ?? "").Length > maxTextLength)
                {
                    TbDataText = TbDataText.Remove(0, TbDataText.Length - maxTextLength);
                }
                
                TbDataText = TbDataText + e.Sender_Id + ", Topic: " + e.Topic + ", Message: " + facultMessage + ", QoS-Level: " + e.QoSLevel + ", Retain: " + (e.Retain ? "Retain" : "No Retain") + ", DupFlag: " + (e.DupFlag ? "true" : "false") + "\r\n";
            }
        }
        #endregion


       

          #region Binding The_p_w_d
        public SecureString The_p_w_d
        {
            get
            {
                return the_p_w_d;
            }
            set
            {
                if (SetProperty(ref the_p_w_d, value))
                {
                    try
                    {                      
                        _clientCert = new X509Certificate2(ClientCertPath, the_p_w_d);
                    }
                    catch
                    {
                            _clientCert = null;
                    }    
                }
            }

        }
        #endregion

        #region Binding ConnectColor
        public System.Windows.Media.Brush ConnectColor
        {
            get
            {
                return _connectColor;
            }
            set
            {
                if (SetProperty(ref _connectColor, value))
                { }
            }
            
        }
        #endregion

        #region Binding ConnectText
        public string ConnectText
        {
            get
            {
                return _connectText;
            }
            set
            {
                if (SetProperty(ref _connectText, value))
                { }
            }

        }
        #endregion

        #region Binding ClientName
        public string ClientName
        {
            get
            {
                return _clientName;
            }
            set
            {
                if (SetProperty(ref _clientName, value))
                { }
            }
        }
        #endregion
    
        #region Binding ReceivedMessage
        public string ReceivedMessage
        {
            get
            {
                return _receivedMessage;
            }
            set
            {
                if (SetProperty(ref _receivedMessage, value))
                { }
            }
        }
        #endregion

        #region Binding ActualBrokerUrlClient
        public string ActualBrokerUrlClient
        {
            get { return _actualBrokerUrlClient; }
            set
            {
                if (SetProperty(ref _actualBrokerUrlClient, value))
                {
                    ActualBrokerUrl = _actualBrokerUrlClient.Substring(0, _actualBrokerUrlClient.LastIndexOf("-&-") - 1);
                    IPAddress iPAddress = GetIPAddress(ActualBrokerUrl);
                    ActualBrokerIpAddress = iPAddress == null ? "No Ip-Addr. found" : iPAddress.ToString();                   
                    Update_Gui_From_AccountMembers(_actualBrokerUrlClient, (iPAddress != null));
                    // Yes, a second time is correct
                    Update_Gui_From_AccountMembers(_actualBrokerUrlClient, (iPAddress != null));
                    _lastBrokerUrlClient = _actualBrokerUrlClient;
                    ConnectColor = greyBrush;
                    WriteAccountsDictionaryToFile(accountsDictionary, fileAccounts);
                }
            }
        }
        #endregion

        #region Binding ActualBrokerUrl
        public string ActualBrokerUrl
        {
            get { return _actualBrokerUrl; }
            set
            {
                if (SetProperty(ref _actualBrokerUrl, value))
                {                   
                    _lastBrokerUrl = _actualBrokerUrl;
                }
            }
        }
        #endregion
       
        #region Binding QoSLevelsCollection
        public ObservableCollection<string> QoSLevelsCollection
        {
            get
            {
                return _qoSLevelsCollection;
            }
            set
            {
                _qoSLevelsCollection = value;
            }
        }
        #endregion
     
        #region QoSLevelsCollectionSubscribe
        public ObservableCollection<string> QoSLevelsCollectionSubscribe
        {
            get
            {
                return _qoSLevelsCollectionSubscribe;
            }
            set
            {
                _qoSLevelsCollectionSubscribe = value;
            }
        }
        #endregion

        #region Binding TopicsToSubscribeCollection
        public ObservableCollection<ComboBoxPropertiesItem> TopicsToSubscribeCollection
        {
            get
            {
                return _topicsToSubscribeCollection;
            }
            set
            {
                _topicsToSubscribeCollection = value;
            }
        }
        #endregion

        #region Binding TopicsToPublishCollection
        public ObservableCollection<ComboBoxPropertiesItem> TopicsToPublishCollection
        {
            get
            {
                return _topicsToPublishCollection;
            }
            set
            {
                _topicsToPublishCollection = value;
            }
        }
        #endregion
      
        #region Binding MessagesCollection
        public ObservableCollection<ComboBoxPropertiesItem> MessagesCollection
        {
            get
            {
                return _messagesCollection;
            }
            set
            {
                _messagesCollection = value;
            }
        }
        #endregion

        #region Binding Ports
        public ObservableCollection<string> Ports
        {
            get
            {
                return _portsCollection;
            }
            set
            {
                _portsCollection = value;
            }
        }
        #endregion

        #region Binding SelectedPort
        public string SelectedPort
        {
            get 
            { 
                return _selectedPort; 
            }
            set
            {
                if (SetProperty(ref _selectedPort, value))
                { }
            }
        }
        #endregion

        #region Binding User
        public string User
        {
            get
            {
                return _user;
            }
            set
            {
                if (SetProperty(ref _user, value))
                { }
            }
            
        }
        #endregion

        #region Binding Password
        public string Password
        {
            get { return _password; }
            set
            {
                if (SetProperty(ref _password, value))
                { }                 
            }
        }
        #endregion

        #region Binding TLS_Security
        public bool TLS_Security
        {
            get { return _tls_Security; }
            set
            {
                if (SetProperty(ref _tls_Security, value))
                { }
            }
        }
        #endregion

        #region Binding Connect_CleanSession
        public bool Connect_CleanSession
        {
            get { return _connect_CleanSession; }
            set
            {
                if (SetProperty(ref _connect_CleanSession, value))
                { }
            }
        }
        #endregion

        #region TextBox_TopicsToSubsribe_Visibility 
        public Visibility TextBox_TopicsToSubsribe_Visibility
        {
            get { return _textBox_TopicsToSubsribe_Visibility; }
            set
            {
                if (SetProperty(ref _textBox_TopicsToSubsribe_Visibility, value))
                { }
            }
        }

        #endregion

        #region ComboBox_TopicsToSubsribe_Visibility 
        public Visibility ComboBox_TopicsToSubsribe_Visibility
        {
            get { return _comboBox_TopicsToSubsribe_Visibility; }
            set
            {
                if (SetProperty(ref _comboBox_TopicsToSubsribe_Visibility, value))
                { }
            }
        }

        #endregion

        #region TextBox_TopicsToPublish_Visibility 
        public Visibility TextBox_TopicsToPublish_Visibility
        {
            get { return _textBox_TopicsToPublish_Visibility; }
            set
            {
                if (SetProperty(ref _textBox_TopicsToPublish_Visibility, value))
                { }
            }
        }

        #endregion

        #region ComboBox_TopicsToPublish_Visibility 
        public Visibility ComboBox_TopicsToPublish_Visibility
        {
            get { return _comboBox_TopicsToPublish_Visibility; }
            set
            {
                if (SetProperty(ref _comboBox_TopicsToPublish_Visibility, value))
                { }
            }
        }

        #endregion

        #region TextBox_Messages_Visibility 
        public Visibility TextBox_Messages_Visibility
        {
            get { return _textBox_Messages_Visibility; }
            set
            {
                if (SetProperty(ref _textBox_Messages_Visibility, value))
                { }
            }
        }

        #endregion

        #region ComboBox_Messages_Visibility 
        public Visibility ComboBox_Messages_Visibility
        {
            get { return _comboBox_Messages_Visibility; }
            set
            {
                if (SetProperty(ref _comboBox_Messages_Visibility, value))
                { }
            }
        }

        #endregion

        #region ComboBox_Url_Visibility 
        public Visibility ComboBox_Url_Visibility
        {
            get { return _comboBox_Url_Visibility; }
            set
            {
                if (SetProperty(ref _comboBox_Url_Visibility, value))
                { }
            }
        }

        #endregion
        
        #region TextBox_Url_Visibility
        public Visibility TextBox_Url_Visibility
        {
            get { return _textBox_Url_Visibility; }
            set
            {
                if (SetProperty(ref _textBox_Url_Visibility, value))
                { }
            }
        }
        #endregion

        #region BtnUpdateAccount_Visibility
        public Visibility BtnUpdateAccount_Visibility
        {
            get { return _btnUpdateAccount_Visibility; }
            set
            {
                if (SetProperty(ref _btnUpdateAccount_Visibility, value))
                { }
            }
        }
        #endregion

        #region BtnDeleteAccount_Visibility
        public Visibility BtnDeleteAccount_Visibility
        {
            get { return _btnDeleteAccount_Visibility; }
            set
            {
                if (SetProperty(ref _btnDeleteAccount_Visibility, value))
                { }
            }
        }
        #endregion

        #region BtnNewAccount_Visibility
        public Visibility BtnNewAccount_Visibility
        {
            get { return _btnNewAccount_Visibility; }
            set
            {
                if (SetProperty(ref _btnNewAccount_Visibility, value))
                { }
            }
        }
        #endregion

        #region BtnCancelAccount_Visibility
        public Visibility BtnCancelAccount_Visibility
        {
            get { return _btnCancelAccount_Visibility; }
            set
            {
                if (SetProperty(ref _btnCancelAccount_Visibility, value))
                { }
            }
        }
        #endregion

        #region BtnCreate_Visibility
        public Visibility BtnCreate_Visibility
        {
            get { return _btnCreate_Visibility; }
            set
            {
                if (SetProperty(ref _btnCreate_Visibility, value))
                { }
            }
        }
        #endregion

        #region Binding BrokerAddresses
        public ObservableCollection<string> BrokerAddresses
        {
            get
            {
                return url_Collection;
            }
            set
            {
                url_Collection = value;
            }
        }
        #endregion
        

        #region Binding TopicSubscribeShadow
        public ComboBoxPropertiesItem TopicSubscribeShadow
        {
            get { return _topicSubscribeShadow; }
            set
            {
                if (SetProperty(ref _topicSubscribeShadow, value))
                {
                    TopicSubscribe = _topicSubscribeShadow.Text;                  
                }
            }
        }
        #endregion

        #region Binding TopicPublishShadow
        public ComboBoxPropertiesItem TopicPublishShadow
        {
            get { return _topicPublishShadow; }
            set
            {
                if (SetProperty(ref _topicPublishShadow, value))
                {
                    TopicPublish = _topicPublishShadow.Text;
                    
                }
            }
        }
        #endregion
       
        #region Binding MessageShadow
        public ComboBoxPropertiesItem MessageShadow
        {
            get { return _messageShadow; }
            set
            {
                if (SetProperty(ref _messageShadow, value))
                {
                    MessagePublish = _messageShadow.Text;                  
                }
            }
        }
        #endregion

        #region Binding TopicSubscribe
        public string TopicSubscribe
        {
            get { return _topicSubscribe; }
            set
            {
                if (SetProperty(ref _topicSubscribe, value))
                {
                    if (TopicsToSubscribeCollection.Count(x => x.Text == TopicSubscribe) > 0)
                    {
                        TopicsToSubscribeCollection.First(x => x.Text == TopicSubscribe).TimeStamp = DateTime.UtcNow;
                    }
                    else
                    {
                        TopicSubscribeShadow = new ComboBoxPropertiesItem(DateTime.UtcNow, TopicSubscribe);
                        TopicsToSubscribeCollection.Add(TopicSubscribeShadow);

                        if (TopicsToSubscribeCollection.Count > 15)
                        {
                            int searchedIndex = 0;
                            DateTime oldestTimestamp = TopicsToSubscribeCollection[0].TimeStamp;
                            for (int i = 0; i < TopicsToSubscribeCollection.Count; i++)
                            {
                                if (TopicsToSubscribeCollection[i].TimeStamp < oldestTimestamp)
                                {
                                    searchedIndex = i;
                                    oldestTimestamp = TopicsToSubscribeCollection[i].TimeStamp;
                                }
                            }
                            TopicsToSubscribeCollection.RemoveAt(searchedIndex);
                        }                       
                    }
                    comboBoxesContentDictionary["SubscribeItems"] = TopicsToSubscribeCollection;
                    WriteComboBoxesContentDictionaryToFile(comboBoxesContentDictionary, fileComboBoxes);
                }               
            }
        }
        #endregion

        #region Binding TopicPublish
        public string TopicPublish
        {
            get { return _topicPublish; }
            set
            {
                if (SetProperty(ref _topicPublish, value))
                {
                    if (TopicsToPublishCollection.Count(x => x.Text == TopicPublish) > 0)
                    {
                        TopicsToPublishCollection.First(x => x.Text == TopicPublish).TimeStamp = DateTime.UtcNow;
                    }
                    else
                    {
                        TopicPublishShadow = new ComboBoxPropertiesItem(DateTime.UtcNow, TopicPublish);
                        TopicsToPublishCollection.Add(TopicPublishShadow);

                        if (TopicsToPublishCollection.Count > 15)
                        {
                            int searchedIndex = 0;
                            DateTime oldestTimestamp = TopicsToPublishCollection[0].TimeStamp;
                            for (int i = 0; i < TopicsToPublishCollection.Count; i++)
                            {
                                if (TopicsToPublishCollection[i].TimeStamp < oldestTimestamp)
                                {
                                    searchedIndex = i;
                                    oldestTimestamp = TopicsToPublishCollection[i].TimeStamp;
                                }
                            }
                            TopicsToPublishCollection.RemoveAt(searchedIndex);
                        }
                    }
                    comboBoxesContentDictionary["PublishItems"] = TopicsToPublishCollection;
                    WriteComboBoxesContentDictionaryToFile(comboBoxesContentDictionary, fileComboBoxes);
                }
            }
        }
        #endregion
       
        #region Binding MessagePublish
        public string MessagePublish
        {
            get { return _messagePublish; }
            set
            {
                if (SetProperty(ref _messagePublish, value))
                {
                    if (MessagesCollection.Count(x => x.Text == MessagePublish) > 0)
                    {
                        MessagesCollection.First(x => x.Text == MessagePublish).TimeStamp = DateTime.UtcNow;
                    }
                    else
                    {
                        MessageShadow = new ComboBoxPropertiesItem(DateTime.UtcNow, MessagePublish);
                        MessagesCollection.Add(MessageShadow);

                        if (MessagesCollection.Count > 15)
                        {
                            int searchedIndex = 0;
                            DateTime oldestTimestamp = MessagesCollection[0].TimeStamp;
                            for (int i = 0; i < MessagesCollection.Count; i++)
                            {
                                if (MessagesCollection[i].TimeStamp < oldestTimestamp)
                                {
                                    searchedIndex = i;
                                    oldestTimestamp = MessagesCollection[i].TimeStamp;
                                }
                            }
                            MessagesCollection .RemoveAt(searchedIndex);
                        }
                    }
                    comboBoxesContentDictionary["MessageItems"] = MessagesCollection;
                    WriteComboBoxesContentDictionaryToFile(comboBoxesContentDictionary, fileComboBoxes);

                }
            }
        }
        #endregion

        #region Binding Publish_Retain
        public bool Publish_Retain
        {
            get { return _publish_Retain; }
            set
            {
                if (SetProperty(ref _publish_Retain, value))
                { }
            }
        }
        #endregion

        #region Binding ShowClientMessages
        public bool ShowClientMessages
        {
            get { return _showClientMessages; }
            set
            {
                if (SetProperty(ref _showClientMessages, value))
                { }
            }
        }
        #endregion

        #region Binding ShowBrokerMessages
        public bool ShowBrokerMessages
        {
            get { return _showBrokerMessages; }
            set
            {
                if (SetProperty(ref _showBrokerMessages, value))
                { }
            }
        }
        #endregion

        #region Binding SelectedPuplishQoSLevel
        public string SelectedPuplishQoSLevel
        {
            get { return _selectedPuplishQoSLevel; }
            set
            {
                if (SetProperty(ref _selectedPuplishQoSLevel, value))
                { }
            }
        }
        #endregion

        #region Binding SelectedSubscribeQoSLevel
        public string SelectedSubscribeQoSLevel
        {
            get { return _selectedSubscribeQoSLevel; }
            set
            {
                if (SetProperty(ref _selectedSubscribeQoSLevel, value))
                { }
            }
        }
        #endregion

        #region Binding ActualBrokerIpAddress
        public string ActualBrokerIpAddress
        {
            get { return _actualBrokerIpAddress; }
            set
            {
                if (SetProperty(ref _actualBrokerIpAddress, value))
                {
                    

                }
            }
        }
        #endregion

        #region Binding ClientCertPath
        public string ClientCertPath
        {
            get { return _clientCertPath; }
            set
            {
                if (SetProperty(ref _clientCertPath, value))
                {
                    try
                    {
                        _clientCert = new X509Certificate2(ClientCertPath, the_p_w_d);                     
                    }
                    catch
                    {
                        _clientCert = null;
                    }
                }
            }
        }
        #endregion

        #region Binding CaCertPath
        public string CaCertPath
        {
            get { return _caCertPath; }
            set
            {
                if (SetProperty(ref _caCertPath, value))
                { 
                    try
                    {
                        _caCert = X509Certificate.CreateFromSignedFile(CaCertPath); // this doesn't have to be a new X509 type...
                    }
                    catch
                    {
                        _caCert = null;
                    }
                }
            }
        }
        #endregion

        #region Binding TbDataText
        public string TbDataText
        {
            get { return _tbDataText; }
            set
            {
                if (SetProperty(ref _tbDataText, value))
                { }              
            }
        }
        #endregion

        #region Method Update_AccountMembers_From_Gui
        private void Update_AccountMembers_From_Gui(string pBrokerUrl, bool pHaveIpAddress)
        {
            //AccountMembers members = new AccountMembers(null, ClientName ?? "myPcClient", TLS_Security, CaCertPath ?? "", ClientCertPath ?? "", User ?? "", Password ?? "", SelectedPort ?? (TLS_Security ? "8883" : "1883"));
            AccountMembers members = new AccountMembers(null, ClientName ?? "myPcClient", TLS_Security, CaCertPath ?? "", ClientCertPath ?? "", User ?? "", Password ?? "", the_p_w_d, SelectedPort ?? (TLS_Security ? "8883" : "18831"));

            if (_clientCert == null)
            {
                try
                {
                    _clientCert = new X509Certificate2(ClientCertPath, the_p_w_d);
                }
                catch
                {
                    _clientCert = null;
                }
            }


            string localBrokerUrl = pHaveIpAddress ? pBrokerUrl : null;
            string localActualBrokerUrlClient = pBrokerUrl + " -&- " + ClientName;

            if (accountsDictionary.ContainsKey(localActualBrokerUrlClient))
            {                                             
                members.AccessInstance = new MqttManager(localBrokerUrl, ClientName, TLS_Security, int.Parse(SelectedPort), _caCert, _clientCert, MqttSslProtocols.TLSv1_2);

                accountsDictionary[ActualBrokerUrlClient] = members;
            }
            else
            {             
                members.AccessInstance = new MqttManager(localBrokerUrl, ClientName, TLS_Security, int.Parse(SelectedPort ?? (TLS_Security ? "8883" : "1883")), _caCert, _clientCert, MqttSslProtocols.TLSv1_2);
                accountsDictionary.Add(localActualBrokerUrlClient, members);
            }
            ActualBrokerUrlClient = localActualBrokerUrlClient;               
        }
        #endregion

        #region Method Update_Gui_From_AccountMembers
        private void Update_Gui_From_AccountMembers(string pBrokerUrlClient, bool pHaveIpAddress)
        {
            AccountMembers members; 

            if (accountsDictionary.TryGetValue(pBrokerUrlClient, out members)) // Returns true.
            {
                
                CaCertPath = members.CaCertPath;
                ClientCertPath = members.ClientCertPath;
                TLS_Security = members.Security;
                User = members.User;             
                Password = members.Password;
                the_p_w_d = members.CertPassword;
                SelectedPort = members.Port;
                ClientName = members.Name;
            }
            else
            {
                string actualBrokerUrl = pBrokerUrlClient.Substring(0, pBrokerUrlClient.LastIndexOf("-&-") - 1);
                string client = pBrokerUrlClient.Substring(pBrokerUrlClient.LastIndexOf("-&-") + 4);
                string localBrokerUrl = pHaveIpAddress ? ActualBrokerUrl : null;
               
                accountsDictionary.Add(pBrokerUrlClient, new AccountMembers(new MqttManager(localBrokerUrl, client ?? "", TLS_Security, int.Parse(SelectedPort ?? (TLS_Security ? "8883" : "1883")), _caCert, _clientCert, MqttSslProtocols.TLSv1_2), client ?? "", TLS_Security, CaCertPath ?? "", ClientCertPath ?? "", User ?? "", Password ?? "", the_p_w_d , SelectedPort ?? (TLS_Security ? "8883" : "1883")));
            }          
        }
        #endregion

        #region Method WriteAccountsDictionaryToFile
        static void WriteAccountsDictionaryToFile(Dictionary<string, AccountMembers> dictionary, string file)
        {

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            using (FileStream fs = File.OpenWrite(file))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Put count.
                writer.Write(dictionary.Count);
                // Write pairs.
                foreach (var pair in dictionary)
                {
                    writer.Write(pair.Key);                   
                    AccountMembers members = new AccountMembers(null, pair.Value.Name, pair.Value.Security, pair.Value.CaCertPath, pair.Value.ClientCertPath, "", "", null, pair.Value.Port);
                    writer.Write(JsonSerializer.Serialize<AccountMembers>(members));
                }
            }
        }
        #endregion

        #region Method ReadAcccountsDictionaryFromFile
        static Dictionary<string, AccountMembers> ReadAcccountsDictionaryFromFile(string file)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var result = new Dictionary<string, AccountMembers>();
            using (FileStream fs = File.OpenRead(file))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // Get count.
                int count = reader.ReadInt32();
                // Read in all pairs.
                for (int i = 0; i < count; i++)
                {

                    string key = reader.ReadString();
                    string value = reader.ReadString();
                    result[key] = JsonSerializer.Deserialize<AccountMembers>(value);
                }
            }
            return result;
        }
        #endregion

        #region Method WriteComboBoxesContentDictionaryToFile
        static void WriteComboBoxesContentDictionaryToFile(Dictionary<string, ObservableCollection<ComboBoxPropertiesItem>> dictionary, string file)
        {
            if (!Directory.Exists(Path.GetDirectoryName(file)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }

            using (FileStream fs = File.OpenWrite(file))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Put count.
                writer.Write(dictionary.Count);
                // Write pairs.
                foreach (var pair in dictionary)
                {
                    writer.Write(pair.Key);

                    var theVal = JsonSerializer.Serialize<ObservableCollection<ComboBoxPropertiesItem>>(pair.Value);
                    writer.Write(JsonSerializer.Serialize<ObservableCollection<ComboBoxPropertiesItem>>(pair.Value));
                }
            }
        }
        #endregion

        #region Method ReadComboBoxesContentDictionaryFromFile
        static Dictionary<string, ObservableCollection<ComboBoxPropertiesItem>> ReadComboBoxesContentDictionaryFromFile(string file)
        {
            if (!Directory.Exists(Path.GetDirectoryName(file)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }
            var result = new Dictionary<string, ObservableCollection<ComboBoxPropertiesItem>>();
            using (FileStream fs = File.OpenRead(file))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // Get count.
                int count = reader.ReadInt32();
                // Read in all pairs.
                for (int i = 0; i < count; i++)
                {
                    string key = reader.ReadString();
                    string value = reader.ReadString();
                    result[key] = JsonSerializer.Deserialize<ObservableCollection<ComboBoxPropertiesItem>>(value);
                }
            }
            return result;
        }
        #endregion

        #region Method convertToUNSecureString   (commented out, not used in this App)
        /*
        public string convertToUNSecureString(SecureString secstrPassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secstrPassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
        */
        #endregion

        #region Method OnWindowClosing
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            var dialogResult = MessageBox.Show("Do you really want to close?","MQTT-Commander", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.No)
            {
                e.Cancel = true;
            }        
            foreach (KeyValuePair<string, AccountMembers> entry in accountsDictionary)
            {

                myMqttManager = (MqttManager)accountsDictionary[entry.Key].AccessInstance;

                if (myMqttManager != null)
                {
                    if (myMqttManager.HasClient)
                    {
                        myMqttManager.Disconnect();
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(100);
                        }

                        myMqttManager.WorkThreadLoopShallRun = false;
                        
                    }
                }
                for (int i = 0; i < 30; i++)
                {
                    Thread.Sleep(100);
                   
                    bool objectStillExists = (myMqttManager == null) ? false : true;                       
                    
                    if (!objectStillExists)
                    {                                        
                        break;
                    }                   
                }
            }
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(100);              
            }                   
        }
        #endregion
    }
}
