using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.Metadata
{
    internal record MetadataDto<T>(int Count, IEnumerable<T>? List) where T : class { }
}
