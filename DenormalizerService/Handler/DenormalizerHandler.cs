using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;
using DenormalizerService.Infrastructure;
using MassTransit;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.ReverseGeoCoding;
using SmartFleet.Data;

namespace DenormalizerService.Handler
{
    public class DenormalizerHandler : IConsumer<CreateTk103Gps>, IConsumer<CreateNewBoxGps>
    {
        private readonly IDbContextScopeFactory _dbContextScopeFactory;
        private readonly ReverseGeoCodingService _geoCodingService;

        private SmartFleetObjectContext _db;

        public DenormalizerHandler()
        {
            _geoCodingService = new ReverseGeoCodingService();
            _dbContextScopeFactory = DependencyRegistrar.ResolveDbContextScopeFactory();
        }

        public async Task Consume(ConsumeContext<CreateTk103Gps> context)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                var box = await _db.Boxes.FirstOrDefaultAsync(x => x.SerialNumber == context.Message.SerialNumber)
                    .ConfigureAwait(false);
                if (box == null)
                {
                    box = new Box();
                    box.Id = Guid.NewGuid();
                    box.BoxStatus = BoxStatus.Prepared;
                    box.CreationDate = DateTime.UtcNow;
                    box.LastGpsInfoTime = context.Message.TimeStampUtc;

                    box.Icci = String.Empty;
                    box.PhoneNumber = String.Empty;
                    box.Vehicle = null;
                    box.Imei = context.Message.IMEI;
                    box.SerialNumber = context.Message.SerialNumber;
                    try
                    {
                        _db.Boxes.Add(box);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                    }

                }

                if (box.BoxStatus == BoxStatus.WaitInstallation)
                    box.BoxStatus = BoxStatus.Prepared;
                box.LastGpsInfoTime = context.Message.TimeStampUtc;
                var address = await _geoCodingService.ExecuteQuery(context.Message.Latitude, context.Message.Longitude);
                Position position = new Position
                {
                    Box_Id = box.Id,
                    Altitude = 0,
                    Direction = 0,

                    Lat = context.Message.Latitude,
                    Long = context.Message.Longitude,
                    Speed = context.Message.Speed,
                    Id = Guid.NewGuid(),
                    Priority = 0,
                    Satellite = 0,
                    Timestamp = context.Message.TimeStampUtc,
                    Address = address.display_name,
                    MotionStatus = (int)context.Message.Speed > 2 ? MotionStatus.Moving : MotionStatus.Stopped
                };
                _db.Positions.Add(position);
                await contextFScope.SaveChangesAsync().ConfigureAwait(false);

            }
        }

        public async Task Consume(ConsumeContext<CreateNewBoxGps> context)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                var box = await _db.Boxes.FirstOrDefaultAsync(x => x.Imei == context.Message.IMEI)
                    .ConfigureAwait(false);
                if (box == null)
                {
                    box = new Box();
                    box.Id = Guid.NewGuid();
                    box.BoxStatus = BoxStatus.Prepared;
                    box.CreationDate = DateTime.UtcNow;
                    box.LastGpsInfoTime = context.Message.TimeStampUtc;

                    box.Icci = String.Empty;
                    box.PhoneNumber = String.Empty;
                    box.Vehicle = null;
                    box.Imei = context.Message.IMEI;
                    box.SerialNumber = context.Message.SerialNumber;
                    try
                    {
                        _db.Boxes.Add(box);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                    }

                }
                if (box.BoxStatus == BoxStatus.WaitInstallation)
                    box.BoxStatus = BoxStatus.Prepared;
                box.LastGpsInfoTime = context.Message.TimeStampUtc;
                var address = await _geoCodingService.ExecuteQuery(context.Message.Latitude, context.Message.Longitude);
                Position position = new Position
                {
                    Box_Id = box.Id,
                    Altitude = 0,
                    Direction = 0,
                    Lat = context.Message.Latitude,
                    Long = context.Message.Longitude,
                    Speed = context.Message.Speed,
                    Id = Guid.NewGuid(),
                    Priority = 0,
                    Satellite = 0,
                    Timestamp = context.Message.TimeStampUtc,
                    Address = address.display_name,
                    MotionStatus = (int)context.Message.Speed > 2 ? MotionStatus.Moving : MotionStatus.Stopped
                };
                _db.Positions.Add(position);
                await contextFScope.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
