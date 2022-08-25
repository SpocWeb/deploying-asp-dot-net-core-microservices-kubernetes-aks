using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GloboTicket.Gateway.Shared.Event;

namespace GloboTicket.Gateway.WebBff.Services
{
    public interface ICatalogStub
    {
        Task<List<EventDto>> GetEventsPerCategory(Guid categoryId);
        Task<EventDto> GetEventById(Guid eventId);

        Task<List<CategoryDto>> GetAllCategories();
        Task<List<EventDto>> GetAllEvents();
    }
}