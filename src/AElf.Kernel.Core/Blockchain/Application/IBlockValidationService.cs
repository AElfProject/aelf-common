using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace AElf.Kernel.Blockchain.Application
{
    public interface IBlockValidationService
    {
        Task<bool> ValidateBlockAsync(IBlock block);
        Task<bool> ValidateBlockAfterExecuteAsync(IBlock block);
    }

    public class BlockValidationService : IBlockValidationService, ITransientDependency
    {
        public ILogger<BlockValidationService> Logger { get; set; }

        private readonly IEnumerable<IBlockValidationProvider> _blockValidationProviders;

        public BlockValidationService(IEnumerable<IBlockValidationProvider> blockValidationProviders)
        {
            Logger = NullLogger<BlockValidationService>.Instance;
            _blockValidationProviders = blockValidationProviders;
        }

        public async Task<bool> ValidateBlockAsync(IBlock block){
            foreach (var provider in _blockValidationProviders)
            {
                if (!await provider.ValidateBlockAsync(block))
                    return false;
            }

            return true;
        }

        public async Task<bool> ValidateBlockAfterExecuteAsync(IBlock block)
        {
            foreach (var provider in _blockValidationProviders)
            {
                if (!await provider.ValidateBlockAfterExecuteAsync(block))
                    return false;
            }

            return true;
        }
    }
}