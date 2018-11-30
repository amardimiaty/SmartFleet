using System;
using System.Collections.Generic;
using System.Linq;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.Geofence;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.DailyRports
{
    public class PositionReport
    {
        public  List<PositionViewModel> PositionViewModels(List<Position> positionsOfVehicle )
        {
            var positions = new List<PositionViewModel>();

            // get current account's vehicles
            foreach (var p in positionsOfVehicle )
                positions.Add(new PositionViewModel(p, p.Vehicle));

            return positions;
        }
        
        public List<TargetViewModel> GetTargetViewModels(List<Position> positions, DateTime startPeriod, string vehicleName)
        {
            string timeZone = "W. Central Africa Standard Time";
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            foreach (var position in positions)
                position.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(position.Timestamp, cstZone);
            var targets = new List<TargetViewModel>();
            var currentStatus = positions.FirstOrDefault()?.MotionStatus;
            var maxSpeed = positions.FirstOrDefault()?.Speed;
            var minSeed = positions.FirstOrDefault()?.Speed;
            int i = 0;
            var s =new  DateTime();
            foreach (var position in positions.OrderBy(x=>x.Timestamp))
            {
                i++;
                var trgt = new TargetViewModel();
                if (currentStatus == position.MotionStatus)
                {
                    trgt.StartPeriod = startPeriod.ToString("O");
                    trgt.StartPeriod1 = startPeriod.ToString("g");

                    trgt.MotionStatus = currentStatus.ToString();
                    if (maxSpeed < position.Speed) maxSpeed = position.Speed;
                    if (minSeed > position.Speed) minSeed = position.Speed;
                    trgt.Latitude = position.Lat;
                    trgt.Logitude = position.Long;
                    trgt.VehicleName = vehicleName;
                    trgt.CurrentDate = startPeriod.Date.ToShortDateString();

                    if (targets.LastOrDefault()?.MotionStatus != currentStatus.ToString())
                    {
                        if (i == 1)
                            trgt.StartAddres = position.Address;
                        else
                            trgt.StartAddres = targets.LastOrDefault()?.ArrivalAddres;
                        
                        trgt.Latitude = position.Lat;
                        trgt.Logitude = position.Long;
                        targets.Add(trgt);
                    }
                    continue;


                }

                if (targets.LastOrDefault() != null)
                // ReSharper disable once PossibleNullReferenceException
                {
                    targets.LastOrDefault().Distance =Math.Round(GeofenceHelper.CalculateDistance(
                        targets.LastOrDefault().Latitude, targets.LastOrDefault().Logitude, position.Lat,
                        position.Long),2);
                    targets.LastOrDefault().EndPeriod = position.Timestamp.ToString("O");
                    targets.LastOrDefault().EndPeriod1 = position.Timestamp.ToString("g");
                    s =DateTime.Parse(targets.LastOrDefault().StartPeriod);
                    // ReSharper disable once PossibleNullReferenceException
                    targets.LastOrDefault().Duration = (position.Timestamp - s).TotalSeconds;
                    if (maxSpeed != null)
                        targets.LastOrDefault().MaxSpeed = (double)maxSpeed;
                    // ReSharper disable once PossibleNullReferenceException

                //    var address = await reverseGeoCodingService.ExecuteQuery(position.Lat, position.Long);
                    // ReSharper disable once PossibleNullReferenceException
                   
                        targets.LastOrDefault().ArrivalAddres = position.Address;

                    if (minSeed != null)
                        // ReSharper disable once PossibleNullReferenceException
                        targets.LastOrDefault().MinSpeed = (double)minSeed;
                    if (maxSpeed != null)
                        if (minSeed != null)
                            // ReSharper disable once PossibleNullReferenceException
                        {
                            targets.LastOrDefault().Speed = Math.Round((double) maxSpeed , 2);
                            targets.LastOrDefault().AvgSpeed = Math.Round(((double)maxSpeed + (double)minSeed)/2, 2);

                        }
                }
                startPeriod = position.Timestamp;
                currentStatus = position.MotionStatus;
                maxSpeed = position.Speed;
                minSeed = position.Speed;
                i++;

            }
            // ReSharper disable once PossibleNullReferenceException

            if (targets.Any())
            {
                var firstItem = targets.FirstOrDefault(x => x.MotionStatus == "Moving");
                if (firstItem != null)
                    targets.FirstOrDefault().BeginService = firstItem.StartPeriod1;
                // ReSharper disable once PossibleNullReferenceException
                //   targets.LastOrDefault().Duration = (position.Timestamp - startPeriod).TotalSeconds;
                if (targets.LastOrDefault().MotionStatus == "Stopped")
                {
                    targets.LastOrDefault().EndService = targets.LastOrDefault().StartPeriod;
                    targets.LastOrDefault().EndPeriod = startPeriod.Date != DateTime.Now.Date
                        ? startPeriod.Date.AddDays(1).AddTicks(-1).ToString("O")
                        : DateTime.Now.Date.AddDays(1).AddTicks(-1).ToString("O");

                    targets.LastOrDefault().EndPeriod1 = startPeriod.Date != DateTime.Now.Date
                        ? startPeriod.Date.AddDays(1).AddTicks(-1).ToString("g")
                        : DateTime.Now.Date.AddDays(1).AddTicks(-1).ToString("g");

                    targets.LastOrDefault().EndService = targets.LastOrDefault().StartPeriod1;
                    targets.LastOrDefault().Duration =
                        (startPeriod.Date.AddDays(1).AddTicks(-1) - DateTime.Parse(targets.LastOrDefault().StartPeriod))
                        .TotalSeconds;

                }
                else
                {
                    var end = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
                    targets.LastOrDefault().Duration = (end - DateTime.Parse(targets.LastOrDefault().StartPeriod)).TotalSeconds;
                    targets.LastOrDefault().EndPeriod = end.ToString("O");
                    targets.LastOrDefault().EndPeriod1 = end.ToString("g");

                }
            }
            else
            {
                var trgt = new TargetViewModel();
                trgt.VehicleName = vehicleName;
                trgt.CurrentDate = startPeriod.Date.ToShortDateString();
                targets.Add(trgt);

            }

            return targets;
        }

    }
}