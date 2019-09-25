using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Domain;
using AElf.Kernel.Blockchain.Events;
using AElf.Types;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace AElf.Kernel.Blockchain.Application
{
    public interface ITransactionResultQueryService
    {
        Task<TransactionResult> GetTransactionResultAsync(Hash transactionId);
        Task<TransactionResult> GetTransactionResultAsync(Hash transactionId, Hash blockHash);
    }

    public interface ITransactionResultService : ITransactionResultQueryService
    {
        Task AddTransactionResultAsync(TransactionResult transactionResult, BlockHeader blockHeader);
    }


    public class TransactionResultService : ITransactionResultService, ITransientDependency
    {
        private readonly ITransactionResultManager _transactionResultManager;
        private readonly ITransactionBlockIndexService _transactionBlockIndexService;
        private readonly IBlockchainService _blockchainService;

        public TransactionResultService(ITransactionResultManager transactionResultManager, 
            IBlockchainService blockchainService, ITransactionBlockIndexService transactionBlockIndexService)
        {
            _transactionResultManager = transactionResultManager;
            _blockchainService = blockchainService;
            _transactionBlockIndexService = transactionBlockIndexService;
        }

        public async Task AddTransactionResultAsync(TransactionResult transactionResult, BlockHeader blockHeader)
        {
            var disambiguatingHash = blockHeader.IsMined() ? blockHeader.GetHash() : blockHeader.GetPreMiningHash();
            await _transactionResultManager.AddTransactionResultAsync(transactionResult, disambiguatingHash);
        }

        public async Task<TransactionResult> GetTransactionResultAsync(Hash transactionId)
        {
            var transactionBlockIndex =
                await _transactionBlockIndexService.GetTransactionBlockIndexAsync(transactionId);

            var chain = await _blockchainService.GetChainAsync();
            if (transactionBlockIndex != null)
                return await _transactionResultManager.GetTransactionResultAsync(transactionId,
                    transactionBlockIndex.BlockHash);
//
//            var hash = chain.BestChainHash;
//            var until = chain.LastIrreversibleBlockHeight > Constants.GenesisBlockHeight
//                ? chain.BestChainHeight - 1
//                : Constants.GenesisBlockHeight;
//            while (true)
//            {
//                var result = await _transactionResultManager.GetTransactionResultAsync(transactionId, hash);
//                if (result != null)
//                {
//                    return result;
//                }
//
//                var header = await _blockchainService.GetBlockHeaderByHashAsync(hash);
//                result = await _transactionResultManager.GetTransactionResultAsync(transactionId,
//                    header.GetPreMiningHash());
//                if (result != null)
//                {
//                    return result;
//                }
//
//                if (header.Height <= until)
//                {
//                    // do until 1 block below LIB, in case the TransactionBlockIndex is not already added during
//                    // NewIrreversibleBlockFoundEvent handling
//                    break;
//                }
//
//                hash = header.PreviousBlockHash;
//            }

            return null;
        }

        public async Task<TransactionResult> GetTransactionResultAsync(Hash transactionId, Hash blockHash)
        {
            var txResult = await _transactionResultManager.GetTransactionResultAsync(transactionId, blockHash);
            if (txResult != null)
            {
                return txResult;
            }
            
            var header = await _blockchainService.GetBlockHeaderByHashAsync(blockHash);
            txResult = await _transactionResultManager.GetTransactionResultAsync(transactionId,
                header.GetPreMiningHash());
            
            return txResult;
        }
    }
}