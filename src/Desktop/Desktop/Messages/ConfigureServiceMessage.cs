using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Messages
{
    internal class ConfigureServiceMessage : AsyncRequestMessage<IServiceProvider>
    {
    }
}
