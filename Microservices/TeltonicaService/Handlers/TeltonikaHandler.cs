using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using SmartFleet.Core.Contracts;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Data;
using TeltonicaService.Infrastucture;

namespace TeltonicaService.Handlers
{

    public class TeltonikaHandler : IConsumer<CreateTeltonikaGps>
    {
        private SmartFleetObjectContext _db;
        private IMapper _mappe;
        public IDbContextScopeFactory DbContextScopeFactory { get; }

        public TeltonikaHandler()
        {

            DbContextScopeFactory = DependencyRegistrar.ResolveDbContextScopeFactory();
            InitMapper();
        }

        private void InitMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => { cfg.AddProfile(new TeltonikaMappings()); });
            _mappe = mapperConfiguration.CreateMapper();
        }

        private async Task<Box> Getbox(CreateTeltonikaGps context)
        {
            using (var contextFScope = DbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Boxes.Include(x => x.Vehicle).SingleOrDefaultAsync(b => b.Imei == context.Imei);
            }
        }
        public async Task Consume(ConsumeContext<CreateTeltonikaGps> context)
        {

            try
            {
                var box = await Getbox(context.Message);
                if (box != null)
                {
                    // envoi des données GPs
                    var gpsDataEvent = _mappe.Map<TLGpsDataEvent>(context.Message);
                    gpsDataEvent.BoxId = box.Id;
                     Trace.WriteLine(gpsDataEvent.DateTimeUtc + " lat:" + gpsDataEvent.Lat + " long:" + gpsDataEvent.Long);
                    await context.Publish(gpsDataEvent);
                    if (box.Vehicle != null)
                    {
                        InitAllIoElements(context.Message);
                        var canInfo = ProceedTNCANFilters(context.Message);
                        canInfo.VehicleId = box.Vehicle.Id;
                        await context.Publish(canInfo);
                        // ReSharper disable once ComplexConditionExpression
                        if (box.Vehicle.MaxSpeed <= context.Message.Speed && box.Vehicle.MaxSpeed>0
                            || context.Message.Speed > 85)
                        {
                            var alertExeedSpeed = ProceedTLSpeedingAlert(context.Message, box.Vehicle.Id,
                                box.Vehicle.CustomerId);
                            await context.Publish(alertExeedSpeed);
                            
                        }
                        var ecoDriveEvent = ProceedEcoDriverEvents(context.Message, box.Vehicle.Id,
                            box.Vehicle.CustomerId);
                       if(ecoDriveEvent!=default(TLEcoDriverAlertEvent))
                           await context.Publish(ecoDriveEvent);
                    }
                }

            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message + " details:" + e.StackTrace);
                //throw;
            }

        }

        private static void InitAllIoElements(CreateTeltonikaGps context)
        {
            context.AllIoElements = new Dictionary<TNIoProperty, long>();
            foreach (var ioElement in context.IoElements_1B)
                context.AllIoElements.Add((TNIoProperty)ioElement.Key, ioElement.Value);
            foreach (var ioElement in context.IoElements_2B)
                context.AllIoElements.Add((TNIoProperty)ioElement.Key, ioElement.Value);
            foreach (var ioElement in context.IoElements_4B)
                context.AllIoElements.Add((TNIoProperty)ioElement.Key, ioElement.Value);
            foreach (var ioElement in context.IoElements_8B)
                context.AllIoElements.Add((TNIoProperty)ioElement.Key, ioElement.Value);
        }

        private TLFuelMilstoneEvent ProceedTNCANFilters(CreateTeltonikaGps data)
        {
            var fuelLevel = default(UInt32?);
            var milestone = default(UInt32?);
            var fuelUsed = default(UInt32?);
            if (data.AllIoElements != null &&
                data.AllIoElements.ContainsKey(TNIoProperty.High_resolution_total_vehicle_distance_X))
                milestone = Convert.ToUInt32(data.AllIoElements[TNIoProperty.High_resolution_total_vehicle_distance_X]);

            if (data.AllIoElements != null && data.AllIoElements.ContainsKey(TNIoProperty.Engine_total_fuel_used))
                fuelUsed = Convert.ToUInt32(data.AllIoElements[TNIoProperty.Engine_total_fuel_used]);

            if (data.AllIoElements != null && data.AllIoElements.ContainsKey(TNIoProperty.Fuel_level_1_X))
                fuelLevel = Convert.ToUInt32(data.AllIoElements[TNIoProperty.Fuel_level_1_X]);
            return new TLFuelMilstoneEvent
            {
                FuelConsumption = Convert.ToInt32(fuelUsed),
                Milestone = Convert.ToInt32(milestone),
                DateTimeUtc = data.Timestamp,
                FuelLevel = Convert.ToInt32(fuelLevel)
            };
        }

        private TLExcessSpeedEvent ProceedTLSpeedingAlert(CreateTeltonikaGps data, Guid vehicleId, Guid? customerId)
        {
            return new TLExcessSpeedEvent
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                VehicleId = vehicleId,
                VehicleEventType = VehicleEvent.EXCESS_SPEED,
                EventUtc = data.Timestamp,
                Latitude = (float?)data.Lat,
                Longitude = (float?)data.Long,
                Address = data.Address,
                Speed = data.Speed
            };


        }


        private TLEcoDriverAlertEvent ProceedEcoDriverEvents(CreateTeltonikaGps data, Guid vehicleId, Guid? customerId)
        {
            var @event = default(TLEcoDriverAlertEvent);
            if (data.DataEventIO == (int)TNIoProperty.Engine_speed_X)
                @event = new TLEcoDriverAlertEvent
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    VehicleId = vehicleId,
                    VehicleEventType = VehicleEvent.EXCESS_ENGINE_SPEED,
                    EventUtc = data.Timestamp,
                    Latitude = (float?)data.Lat,
                    Longitude = (float?)data.Long,
                    Address = data.Address
                    //Speed = data.Speed
                };
            else  if(data.AllIoElements.ContainsKey(TNIoProperty.ECO_driving_type))
            {
                switch (Convert.ToByte(data.AllIoElements[TNIoProperty.ECO_driving_type]))
                {
                    case 1:
                        if (Convert.ToByte(data.AllIoElements[TNIoProperty.ECO_driving_value]) > 31)
                            @event = new TLEcoDriverAlertEvent
                            {
                                Id = Guid.NewGuid(),
                                CustomerId = customerId,
                                VehicleId = vehicleId,
                                VehicleEventType = VehicleEvent.EXCESS_ACCELERATION,
                                EventUtc = data.Timestamp,
                                Latitude = (float?)data.Lat,
                                Longitude = (float?)data.Long,
                                Address = data.Address
                                //Speed = data.Speed
                            };
                        break;
                    case 2:
                        if (Convert.ToByte(data.AllIoElements[TNIoProperty.ECO_driving_value]) > 38)
                            @event = new TLEcoDriverAlertEvent
                            {
                                Id = Guid.NewGuid(),
                                CustomerId = customerId,
                                VehicleId = vehicleId,
                                VehicleEventType = VehicleEvent.SUDDEN_BRAKING,
                                EventUtc = data.Timestamp,
                                Latitude = (float?)data.Lat,
                                Longitude = (float?)data.Long,
                                Address = data.Address
                                //Speed = data.Speed
                            };
                        break;
                    case 3:
                        if (Convert.ToByte(data.AllIoElements[TNIoProperty.ECO_driving_value]) > 45)
                            @event = new TLEcoDriverAlertEvent
                            {
                                Id = Guid.NewGuid(),
                                CustomerId = customerId,
                                VehicleId = vehicleId,
                                VehicleEventType = VehicleEvent.FAST_CORNER,
                                EventUtc = data.Timestamp,
                                Latitude = (float?)data.Lat,
                                Longitude = (float?)data.Long,
                                Address = data.Address
                                //Speed = data.Speed
                            };
                        break;
                    default:
                        break;

                }
            }
            return @event;
        }


    }

    public class TeltonikaMappings : Profile
    {
        public TeltonikaMappings()
        {
            CreateMap< CreateTeltonikaGps, TLGpsDataEvent>()
                .ForMember(x => x.DateTimeUtc, o => o.MapFrom(v => v.Timestamp))
                .ReverseMap();
        }
    }
}
