using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.XunkongApi
{

    [Flags]
    public enum ChannelType
    {


        Stable = 1,


        Preview = 2,


        Development = 4,


        All = 7,

    }
}
