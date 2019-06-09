using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Shapp
{
    /// <summary>
    /// Interface designed to be a carrier for Shapp internal messages.
    /// 
    /// If you want add a new message type, derivie by this class and add
    /// appropierate handler in @ParentMessageDispatcher.
    /// </summary>
    public interface ISystemMessage
    {
        void Dispatch(Socket sender);
    }
}
