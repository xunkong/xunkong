using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Services
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class InjectServiceAttribute : Attribute
    {

        public bool IsSingleton { get; set; }

        public InjectServiceAttribute(bool isSingleton = false)
        {
            IsSingleton = isSingleton;
        }

    }
}
