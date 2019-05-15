using System;
using System.Collections.Generic;
using RESTModule;

namespace JourneyCoreServer.System.Net
{
    public class ClientCluster
    {
        public List<RESTClient> Clients { get; }

        public ClientCluster()
        {
            Clients = new List<RESTClient>();
        }

        public void BroadcastData(string instanceGuid, RequestMethod method, string rawData)
        {
            // TODO INSTANCE GUID ND STUFF  
            Clients.Where()
        }
    }
}
