using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace NeoErp.Core.Caching
{
    /// <summary>
    /// Represents a manager for caching between HTTP requests (long term caching)
    /// </summary>
    public partial class MemoryCacheManager : ICacheManager
    {
        protected ObjectCache Cache
        {
            get
            {
                return MemoryCache.Default;
            }
        }
        
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public virtual T Get<T>(string key)
        {
            return (T)Cache[key];
        }

      

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="data">Data</param>
        /// <param name="cacheTime">Cache time</param>
        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data == null)
                return;

            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime);
            Cache.Add(new CacheItem(key, data), policy);
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        public virtual bool IsSet(string key)
        {
            return (Cache.Contains(key));
        }


       //readymade function
        public List<string> GetAllKeys()
        {
            List<string> recordList = new List<string>();
            List<string> record = new List<string>();
            
            foreach (var item in Cache)
            {
                recordList.Add(item.Key);
            }

            return recordList;
        }

        public void RemoveCacheByKey(List<string> keystart,List<string> Record)
        {
            List<string> RemoveRecordList = new List<string>();
            foreach (var statickeystart in keystart)
            {
                foreach (var item in Record)
                {
                    var data = (item.ToString()).Split(new Char[] { '+', '_' })[0];

                    if (statickeystart == data)
                    {
                        RemoveRecordList.Add(item);
                    }
                }
            }
          
            foreach (var item in RemoveRecordList)
            {
                this.Remove(item);
            }

           
        }
        //


        //public List<string> GetAllKeys()
        //{
        //    List<string> a = new List<string>();


        //    IDictionaryEnumerator en = Cache.GetEnumerator(); while (en.MoveNext())
        //    {
        //        this.Response.Write(en.Key.ToString() + " :: " + en.Value.GetType().ToString() + ""); }
        //    return a;
        //}
        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public virtual void Remove(string key)
        {
            Cache.Remove(key);
        }


        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        public virtual void RemoveByPattern(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<String>();

            foreach (var item in Cache)
                if (regex.IsMatch(item.Key))
                    keysToRemove.Add(item.Key);

            foreach (string key in keysToRemove)
            {
                Remove(key);
            }
        }
        public virtual void Dispose()
        {
        }
        /// <summary>
        /// Clear all cache data
        /// </summary>
        public virtual void Clear()
        {
            foreach (var item in Cache)
                Remove(item.Key);
        }

       
    }


    
}