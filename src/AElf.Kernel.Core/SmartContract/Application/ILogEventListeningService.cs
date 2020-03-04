using System.Collections.Generic;
using System.Threading.Tasks;

namespace AElf.Kernel.SmartContract.Application
{
    public interface ILogEventListeningService<T> where T : ILogEventProcessor
    {
        Task ProcessAsync(IEnumerable<Block> blocks);
    }
}