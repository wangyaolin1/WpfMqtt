using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace MQTT_Client.Models
{
    public class AccountMembers

    {
        public object AccessInstance { get; set; }

        public string Name { get; set; }
        public bool Security { get; set; }
        public string CaCertPath { get; set; }
        public string ClientCertPath { get; set; }
        public string User { get; set; }

        // RoSchmi
        public string Password { get; set; }

        public SecureString CertPassword { get; set; }
        public string Port { get; set; }

        public AccountMembers() { }
        //public AccountMembers(object pAccessInstance, string pName, bool pSecurity, string pCaCertPath, string pClientCertPat, string pUser, string pPassword, string pPort)
        public AccountMembers(object pAccessInstance, string pName, bool pSecurity, string pCaCertPath, string pClientCertPat, string pUser, string pPassword, SecureString pCertPassword, string pPort)
        {
            Name = pName;
            AccessInstance = pAccessInstance;          
            Security = pSecurity;
            CaCertPath = pCaCertPath;
            ClientCertPath = pClientCertPat;
            User = pUser;
            Password = pPassword;
            CertPassword = pCertPassword;
            Port = pPort;
        }
    }
}
