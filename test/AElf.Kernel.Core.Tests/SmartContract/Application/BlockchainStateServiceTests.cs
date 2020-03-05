using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.SmartContract.Domain;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElf.Kernel.SmartContract.Application
{
    public sealed class BlockchainStateServiceTests : AElfKernelWithChainTestBase
    {
        private readonly IBlockchainStateManager _blockchainStateManager;
        private readonly IBlockchainStateService _blockchainStateService;
        private readonly IBlockchainService _blockchainService;

        public BlockchainStateServiceTests()
        {
            _blockchainStateManager = GetRequiredService<IBlockchainStateManager>();
            _blockchainStateService = GetRequiredService<IBlockchainStateService>();
            _blockchainService = GetRequiredService<IBlockchainService>();
        }

        [Fact]
        public async Task BlockState_NoNeed_To_Merge_Test()
        {
            var lastIrreversibleBlockHeight = -2;
            var lastIrreversibleBlockHash = Hash.FromString("hash");

            await _blockchainStateService.MergeBlockStateAsync(lastIrreversibleBlockHeight,
                lastIrreversibleBlockHash);

            var chainStateInfo = await _blockchainStateManager.GetChainStateInfoAsync();
            chainStateInfo.BlockHeight.ShouldNotBe(lastIrreversibleBlockHeight);
            chainStateInfo.MergingBlockHash.ShouldNotBe(lastIrreversibleBlockHash);
        }

        [Fact]
        public async Task BlockState_Merge_GotException_Test()
        {
            var lastIrreversibleBlockHeight = 1;
            var lastIrreversibleBlockHash = Hash.FromString("hash");

            await Should.ThrowAsync<InvalidOperationException>(()=>_blockchainStateService.MergeBlockStateAsync(lastIrreversibleBlockHeight,
                lastIrreversibleBlockHash));
            
            var chainStateInfo = await _blockchainStateManager.GetChainStateInfoAsync();
            chainStateInfo.BlockHeight.ShouldNotBe(lastIrreversibleBlockHeight);
            chainStateInfo.MergingBlockHash.ShouldNotBe(lastIrreversibleBlockHash);
        }

        [Fact]
        public async Task BlockState_MergeBlock_Normal_Test()
        {
            var blockStateSet1 = new BlockStateSet()
            {
                BlockHeight = 1,
                BlockHash = Hash.FromString("hash"),
                PreviousHash = Hash.Empty
            };
            var blockStateSet2 = new BlockStateSet()
            {
                BlockHeight = 2,
                BlockHash = Hash.FromString("hash2"),
                PreviousHash = blockStateSet1.BlockHash
            };
            var blockStateSet3 = new BlockStateSet()
            {
                BlockHeight = 3,
                BlockHash = Hash.FromString("hash3"),
                PreviousHash = blockStateSet2.BlockHash
            };

            //test merge block height 1
            {
                await _blockchainStateManager.SetBlockStateSetAsync(blockStateSet1);

                await _blockchainStateService.MergeBlockStateAsync(blockStateSet1.BlockHeight,
                    blockStateSet1.BlockHash);

                var chainStateInfo = await _blockchainStateManager.GetChainStateInfoAsync();
                chainStateInfo.BlockHeight.ShouldBe(1);
                chainStateInfo.BlockHash.ShouldBe(blockStateSet1.BlockHash);
            }

            //test merge block height 2
            {
                await _blockchainStateManager.SetBlockStateSetAsync(blockStateSet2);
                await _blockchainStateService.MergeBlockStateAsync(blockStateSet2.BlockHeight,
                    blockStateSet2.BlockHash);

                var chainStateInfo = await _blockchainStateManager.GetChainStateInfoAsync();
                chainStateInfo.BlockHeight.ShouldBe(2);
                chainStateInfo.BlockHash.ShouldBe(blockStateSet2.BlockHash);
            }

            //test merge height 3 without block state set before
            {
                await Should.ThrowAsync<InvalidOperationException>(()=> _blockchainStateService.MergeBlockStateAsync(blockStateSet3.BlockHeight,
                    blockStateSet3.BlockHash));

                var chainStateInfo = await _blockchainStateManager.GetChainStateInfoAsync();
                chainStateInfo.BlockHeight.ShouldBe(2);
                chainStateInfo.BlockHash.ShouldBe(blockStateSet2.BlockHash);
            }
        }
        
        [Fact]
        public async Task BlockExecutedData_Test()
        {
            var chain = await _blockchainService.GetChainAsync();
            var blockStateSet = new BlockStateSet
            {
                BlockHash = chain.BestChainHash,
                BlockHeight = chain.BestChainHeight,
            };
            await _blockchainStateManager.SetBlockStateSetAsync(blockStateSet);
            
            var transactionDic = new Dictionary<Hash, Transaction>();
            for (int i = 0; i < 5; i++)
            {
                var transaction = new Transaction
                {
                    From = SampleAddress.AddressList[i],
                    To = SampleAddress.AddressList[i + 1],
                    RefBlockNumber = chain.BestChainHeight - 1,
                    MethodName = "Test"
                };
                transactionDic.Add(transaction.GetHash(), transaction);
            }

            await _blockchainStateService.AddBlockExecutedDataAsync<Hash, Transaction>(chain.BestChainHash,
                transactionDic);
            var transactionResult = new TransactionResult
            {
                TransactionId = transactionDic.First().Key
            };
            await _blockchainStateService.AddBlockExecutedDataAsync(chain.BestChainHash,
                transactionResult.TransactionId, transactionResult);
            await _blockchainStateService.AddBlockExecutedDataAsync(chain.BestChainHash, chain);

            var newBlockStateSet = await _blockchainStateManager.GetBlockStateSetAsync(chain.BestChainHash);
            newBlockStateSet.BlockHash.ShouldBe(blockStateSet.BlockHash);
            newBlockStateSet.BlockHeight.ShouldBe(blockStateSet.BlockHeight);
            newBlockStateSet.BlockExecutedCache.Count.ShouldBe(7);
            newBlockStateSet.BlockExecutedCache.Keys.ShouldContain(key=>key.Contains(typeof(Transaction).Name));
            newBlockStateSet.BlockExecutedCache.Keys.ShouldContain(key=>key.Contains(typeof(TransactionResult).Name));
            newBlockStateSet.BlockExecutedCache.Keys.ShouldContain(key=>key.Contains(typeof(Chain).Name)); 
            
            var chainContext = new ChainContext
            {
                BlockHash = chain.BestChainHash,
                BlockHeight = chain.BestChainHeight
            };
            var chainFromBlockExecutedCache = await _blockchainStateService.GetBlockExecutedDataAsync<Chain>(chainContext);
            chainFromBlockExecutedCache.ShouldBe(chain);

            var transactionResultFromBlockExecutedCache =
                await _blockchainStateService.GetBlockExecutedDataAsync<Hash, TransactionResult>(chainContext,
                    transactionResult.TransactionId);
            transactionResultFromBlockExecutedCache.ShouldBe(transactionResult);
            foreach (var keyPair in transactionDic)
            {
                var transaction = await _blockchainStateService.GetBlockExecutedDataAsync<Hash,Transaction>(chainContext,keyPair.Key);
                transaction.ShouldBe(keyPair.Value);
            }
        }
    }
}