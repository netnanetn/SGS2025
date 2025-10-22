using CMS_Data.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client
{
    public class MauiNetworkStatusProvider : INetworkStatusProvider
    {
        public bool IsConnected =>
            Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }
}
