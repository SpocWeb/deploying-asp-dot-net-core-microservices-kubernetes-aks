using AutoMapper;
using GloboTicket.Services.Marketing.Entities;
using GloboTicket.Services.Marketing.Repositories;
using GloboTicket.Services.Marketing.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GloboTicket.Services.Marketing.Worker
{
    public class TimedBasketChangeEventService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IBasketChangeEventService basketChangeEventService;
        private readonly BasketChangeEventRepository basketChangeEventRepository;
        private readonly IMapper mapper;
        readonly ILogger<TimedBasketChangeEventService> _logger;
        private DateTime lastRun;

        public TimedBasketChangeEventService(IBasketChangeEventService basketChangeEventService
	        , BasketChangeEventRepository basketChangeEventRepository
	        , IMapper mapper, ILogger<TimedBasketChangeEventService> logger)
        {
            this.basketChangeEventService = basketChangeEventService;
            this.basketChangeEventRepository = basketChangeEventRepository;
            this.mapper = mapper;
            _logger = logger;
            lastRun = DateTime.Now;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var events = await basketChangeEventService.GetBasketChangeEvents(lastRun, 10);
            foreach (var basketChangeEvent in events)
	            try
            {
		            await basketChangeEventRepository.AddBasketChangeEvent(
			            mapper.Map<BasketChangeEvent>(basketChangeEvent));
	            }
	            catch (Exception x)
	            {
                    _logger.LogError(x, " on adding BasketChangeEvent {event}", basketChangeEvent);
            }
            lastRun = DateTime.Now;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}