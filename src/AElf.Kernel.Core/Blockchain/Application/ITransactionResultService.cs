using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Domain;
using AElf.Types;
using Volo.Abp.DependencyInjection;

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

        Task ProcessTransactionResultAfterExecutionAsync(BlockHeader blockHeader, List<Hash> transactionIds);
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

            if (transactionBlockIndex != null)
                return await _transactionResultManager.GetTransactionResultAsync(transactionId,
                    transactionBlockIndex.BlockHash);

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

        public async Task ProcessTransactionResultAfterExecutionAsync(BlockHeader blockHeader,
            List<Hash> transactionIds)
        {
            var preMiningHash = blockHeader.GetPreMiningHash();
            var blockIndex = new BlockIndex
            {
                BlockHash = blockHeader.GetHash(),
                BlockHeight = blockHeader.Height
            };

            if (transactionIds.Count == 0)
            {
                // This will only happen during test environment
                return;
            }

            var firstTransaction = transactionIds.First();
            var withBlockHash = await _transactionResultManager.GetTransactionResultAsync(
                firstTransaction, blockHeader.GetHash());
            var withPreMiningHash = await _transactionResultManager.GetTransactionResultAsync(
                firstTransaction, preMiningHash);

            if (withBlockHash == null)
            {
                // TransactionResult is not saved with real BlockHash
                // Save results with real (post mining) Hash, so that it can be queried with TransactionBlockIndex
                foreach (var txId in transactionIds)
                {
                    var result = await _transactionResultManager.GetTransactionResultAsync(txId, preMiningHash);
                    await _transactionResultManager.AddTransactionResultAsync(result, blockIndex.BlockHash);
                }
            }

            // Add TransactionBlockIndex
            var toBeRemovedTransactionResults = new List<Hash>();
            foreach (var txId in transactionIds)
            {
                if (withPreMiningHash != null)
                {
                    toBeRemovedTransactionResults.Add(txId);
                }
                await _transactionBlockIndexService.UpdateTransactionBlockIndexAsync(txId, blockIndex);
            }

            await _transactionResultManager.RemoveTransactionResultAsync(toBeRemovedTransactionResults, preMiningHash);
        }
    }
}