﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using AccountManager.Application.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Caching
{
    public class MemoryCacheManager : ICacheManager
    {
        private readonly IMemoryCache _memoryCache;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public MemoryCacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public T Get<T>(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        /// <summary>
        ///     Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="data">Data</param>
        /// <param name="cacheTime">Cache time</param>
        public void Set(string key, object data, int cacheTime, bool sliding = false)
        {
            if (data == null) return;

            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (sliding)
            {
                cacheEntryOptions.SetAbsoluteExpiration(DateTime.Now + TimeSpan.FromMinutes(cacheTime));
            }
            else
            {
                cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromMinutes(cacheTime));
            }

            cacheEntryOptions.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            try
            {
                _memoryCache.Set(key, data, cacheEntryOptions);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        public bool IsSet(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        /// <summary>
        ///     Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <summary>
        ///     Clear all cache data
        /// </summary>
        public void Clear()
        {
            _resetCacheToken.Cancel();

            _resetCacheToken.Dispose();
            _resetCacheToken = new CancellationTokenSource();
        }
    }
}