using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GloboTicket.Gateway.Shared.Event;
using GloboTicket.Gateway.WebBff.Url;
using Microsoft.Extensions.Caching.Distributed;

namespace GloboTicket.Gateway.WebBff.Services
{
	public class CatalogStub : CachingStub, ICatalogStub
	{
		readonly HttpClient Client_;
		readonly IDistributedCache Cache_;

		public CatalogStub(HttpClient client, IDistributedCache cache) : base(client, cache)
		{
		}

		public async Task<List<EventDto>> GetAllEvents()
			=> await CacheGetOrCreateAsync<List<EventDto>>(EventCatalogOperations.GetAllEvents());

		public async Task<List<EventDto>> GetEventsPerCategory(Guid categoryId)
			=> await CacheGetOrCreateAsync<List<EventDto>>(EventCatalogOperations.GetEventsPerCategory(categoryId));

		public async Task<EventDto> GetEventById(Guid eventId)
			=> await CacheGetOrCreateAsync<EventDto>(EventCatalogOperations.GetEventById(eventId));

		public async Task<List<CategoryDto>> GetAllCategories()
			=> await CacheGetOrCreateAsync<List<CategoryDto>>(EventCatalogOperations.GetAllcategories());
	}
}