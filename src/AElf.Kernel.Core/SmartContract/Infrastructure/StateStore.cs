using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Kernel.Infrastructure;
using Google.Protobuf;

namespace AElf.Kernel.SmartContract.Infrastructure
{
    public class StateStore<T> : KeyValueStoreBase<StateKeyValueDbContext, T>, IStateStore<T>
        where T : class, IMessage<T>, new()
    {
        public StateStore(StateKeyValueDbContext keyValueDbContext, IStoreKeyPrefixProvider<T> prefixProvider) : base(
            keyValueDbContext, prefixProvider)
        {
        }
    }

    public interface INotModifiedCachedStateStore<T> : IStateStore<T>
        where T : IMessage<T>, new()
    {
    }

    public class NotModifiedCachedStateStore<T> : INotModifiedCachedStateStore<T>
        where T : class, IMessage<T>, new()
    {
        private readonly IStateStore<T> _stateStoreImplementation;

        private readonly ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();

        public NotModifiedCachedStateStore(IStateStore<T> stateStoreImplementation)
        {
            _stateStoreImplementation = stateStoreImplementation;
        }

        public async Task SetAsync(string key, T value)
        {
            await _stateStoreImplementation.SetAsync(key, value);
        }

        public async Task SetAllAsync(Dictionary<string, T> pipelineSet)
        {
            await _stateStoreImplementation.SetAllAsync(pipelineSet);
        }

        public async Task<T> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                return item;
            }

            var state = await _stateStoreImplementation.GetAsync(key);
            if (state != null)
            {
                _cache[key] = state;
            }

            return state;
        }

        public async Task RemoveAsync(string key)
        {
            _cache.TryRemove(key, out _);
            await _stateStoreImplementation.RemoveAsync(key);
        }

        public async Task<bool> IsExistsAsync(string key)
        {
            return _cache.ContainsKey(key);
        }
    }
}