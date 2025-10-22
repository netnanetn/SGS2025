using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Networks
{
    public interface INetworkStatusProvider
    {
        bool IsConnected { get; }
    }

}
