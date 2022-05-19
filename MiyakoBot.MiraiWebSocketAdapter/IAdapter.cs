using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiyakoBot.Adapter
{
    public interface IAdapter
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
