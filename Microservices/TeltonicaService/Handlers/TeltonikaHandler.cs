using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Data;
using TeltonicaService.Infrastucture;

namespace TeltonicaService.Handlers
{

    public class TeltonikaHandler : IConsumer<CreateTeltonikaGps>
        , IConsumer<CreateBoxCommand>
    {
        private readonly IScopeRepository<Box> _boxRepository;
        private readonly IScopeRepository<Position> _posiRepository;
        // handel the dbcontext instances using  ambient DbContextScope  approach for more details visit :http://mehdi.me/ambient-dbcontext-in-ef6/

        readonly IDbContextScopeFactory _dbContextScopeFactory;
        private SmartFleetObjectContext _db;

        public TeltonikaHandler()
        {
            _dbContextScopeFactory = DependencyRegistrar.ResolveDbContextScopeFactory();
            _boxRepository = DependencyRegistrar.ScopeRepository<Box>();
            _posiRepository = DependencyRegistrar.ScopeRepository<Position>();
        }

        private async Task<Box> Item(CreateTeltonikaGps context)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Boxes.SingleOrDefaultAsync(b => b.Imei == context.Imei);

            }

        }


        public Task Consume(ConsumeContext<CreateBoxCommand> context)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();

                var item = _db.Boxes.FirstOrDefaultAsync(b => b.Imei == context.Message.Imei).Result;
                if (item != null)
                    return Task.FromResult(false);
                var box = new Box();
                box.Id = Guid.NewGuid();
                box.BoxStatus = BoxStatus.WaitPreparation;
                box.CreationDate = DateTime.UtcNow;
                box.Icci = String.Empty;
                box.PhoneNumber = String.Empty;
                box.Vehicle = null;
                box.Imei = context.Message.Imei;
                box.SerialNumber = String.Empty;

                try
                {
                    _boxRepository.Add(box);
                    contextFScope.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception e)

                {
                    Trace.WriteLine(e);
                    throw;
                }

                return Task.FromResult(false);
            }
        }

        public Task Consume(ConsumeContext<CreateTeltonikaGps> context)
        {

            try
            {
                var item = Item(context.Message);
                if (item.Result != null)
                {
                    using (var contextFScope = _dbContextScopeFactory.Create())
                    {
                        var position = new Position();
                        position.Box_Id = item.Result?.Id;
                        position.Altitude = context.Message.Altitude;
                        position.Direction = context.Message.Direction;
                        position.Lat = context.Message.Lat;
                        position.Long = context.Message.Long;
                        position.Speed = context.Message.Speed;
                        position.Id = Guid.NewGuid();
                        position.Priority = context.Message.Priority;
                        position.Satellite = context.Message.Satellite;
                        position.Timestamp = context.Message.Timestamp;
                        position.MotionStatus = context.Message.Speed>0.0 ? MotionStatus.Moving : MotionStatus.Stopped;

                        _posiRepository.Add(position);
                        contextFScope.SaveChangesAsync().ConfigureAwait(false);
                    }


                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                //throw;
            }
            return Task.FromResult(false);
        }
    }
}
