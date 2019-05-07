using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Domain;

namespace AElf.Kernel.Miner.Application
{
    public class SystemTransactionGenerationService : ISystemTransactionGenerationService
    {
        private readonly IEnumerable<ISystemTransactionGenerator> _systemTransactionGenerators;
        private readonly ITransactionManager _transactionManager;

        public SystemTransactionGenerationService(IEnumerable<ISystemTransactionGenerator> systemTransactionGenerators, ITransactionManager txManager)
        {
            _systemTransactionGenerators = systemTransactionGenerators;
            _transactionManager = txManager;
        }

        public async Task<List<Transaction>> GenerateSystemTransactions(Address from, long preBlockHeight, Hash preBlockHash)
        {
            var generatedTxns = new List<Transaction>();
            foreach (var generator in _systemTransactionGenerators)
            {
                generator.GenerateTransactions(@from, preBlockHeight, preBlockHash, ref generatedTxns);
            }
            
            return generatedTxns;
        }
    }
}