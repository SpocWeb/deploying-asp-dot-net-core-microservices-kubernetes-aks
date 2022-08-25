using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GloboTicket.Gateway.WebBff.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace GloboTicket.Gateway.WebBff.Services
{
	public class CachingStub
	{
		readonly HttpClient Client_;
		readonly IDistributedCache Cache_;

		public CachingStub(HttpClient client, IDistributedCache cache)
		{
			Client_ = client;
			Cache_ = cache;
		}

		public async Task<T> CacheGetOrCreateAsync<T>(string url)
		{
			T data;
			var encodedData = await Cache_.GetAsync(url);
			if (encodedData != null)
			{
				data = JsonSerializer.Deserialize<T>(encodedData);
				Console.WriteLine($"CACHE: Data from cache with key: {url}");
			}
			else
			{
				var response = await Client_.GetAsync(url);
				data = await response.ReadContentAs<T>();

				byte[] dataByteArray = JsonSerializer.SerializeToUtf8Bytes(data);

				var distCacheOptions = new DistributedCacheEntryOptions()
					.SetSlidingExpiration(TimeSpan.FromSeconds(20))
					.SetAbsoluteExpiration(TimeSpan.FromSeconds(100));

				await Cache_.SetAsync(url, dataByteArray, distCacheOptions);
				Console.WriteLine($"API Call: Data from {url}");
			}
			return data;
		}
	}
}