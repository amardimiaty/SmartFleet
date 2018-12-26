using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //foreach (var position in positions.OrderBy(w=>w.Timestamp))
            //    position.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(position.Timestamp, cstZone);
            var targets = new List<TargetViewModel>();
            var currentStatus = positions.OrderBy(p => p.Timestamp).ThenBy(p => p.MotionStatus).FirstOrDefault()?.MotionStatus;
            var start = positions.FirstOrDefault()?.Timestamp;
            var periods = new List<Periods>();
            var orderdPositions = positions.OrderBy(p => p.Timestamp).ThenBy(p=>p.MotionStatus);
            var motionStatusCount = orderdPositions.GroupBy(x => x.MotionStatus).Count();
            var motionStatusChanged = false;
            foreach (var position in orderdPositions)
            {
                motionStatusChanged = MotionStatusChanged(position, periods, ref currentStatus, ref start);
            }
           

            if (motionStatusCount == 1 || !motionStatusChanged)
            {
                if (motionStatusCount == 1)
                {
                    var poriod = new Periods(orderdPositions.LastOrDefault().Timestamp, start, currentStatus);
                    periods.Add(poriod);
                }
                else if (!motionStatusChanged)
                {
                    var end = periods.LastOrDefault()?.End;
                    var query = orderdPositions.Where(x => x.Timestamp >= end);
                    var enumerable
                        = query as Position[] ?? query.ToArray();
                    var poriod = new Periods(enumerable.LastOrDefault()?.Timestamp,end, enumerable.LastOrDefault()?.MotionStatus);
                    periods.Add(poriod);

                }
            }
            var tmp = new List<Periods>();
            foreach (var period in periods)
                if ((period.End - period.Start).TotalSeconds < 60)
                    tmp.Add(period);

            DeleteTempPeriods(tmp, periods);
           tmp= new List<Periods>();
            foreach (var period in periods)
            {
                var index = periods.FindIndex(p=>p == period);
                if (index + 1 >= periods.Count ||period.MotionStatus != periods[index + 1].MotionStatus ) continue;
                while ((index + 1) < periods.Count &&period.MotionStatus == periods[index + 1].MotionStatus)
                {
                    period.End = periods[index + 1].End;
                    tmp.Add(periods[index + 1]);
                    index++;
                }
               
            }
            DeleteTempPeriods(tmp, periods);

            foreach (var position in periods.Distinct())
            {
                var trgt = new TargetViewModel();
                var firstPosition = GetFirstPosition(positions, position);
                var lastPosition = GetLastPosition(positions, position);
                SetBegningsTrip(firstPosition, trgt);
                SetEndsTrip(lastPosition, trgt);
                GetDistanceAndDuration(positions, lastPosition, firstPosition, trgt);
                var index = GetPreviousPositionIndexx(positions, lastPosition);
                if (index == -1)
                    index = 0;
                trgt.Speed = positions.ElementAt(index) .Speed ;
                trgt.VehicleName = vehicleName;
                trgt.CurrentDate = startPeriod.Date.ToShortDateString();
                targets.Add(trgt);
                
            }
            return targets;
        }

        private static void DeleteTempPeriods(List<Periods> tmp, List<Periods> periods)
        {
            foreach (var period in tmp)
                periods.Remove(period);
        }

        private static bool MotionStatusChanged(Position position, List<Periods> periods, ref MotionStatus? currentStatus,
            ref DateTime? start)
        {
            bool motionStatusChanged;
            Trace.WriteLine("Timestamp: " + position.Timestamp + " MotionStatus: " + position.MotionStatus);
            if (currentStatus == position.MotionStatus)
            {
                motionStatusChanged = false;
                return motionStatusChanged;
            }

            var poriod = new Periods(position.Timestamp, start, currentStatus);
            currentStatus = position.MotionStatus;
            start = position.Timestamp;
            periods.Add(poriod);
            motionStatusChanged = true;
            Trace.WriteLine("period added at : " + position.Timestamp + " MotionStatus: " + position.MotionStatus);
            return motionStatusChanged;
        }

        private static int GetPreviousPositionIndexx(List<Position> positions, Position lastPosition)
        {
            int index = positions.ToList().FindIndex(x => x == lastPosition);
            return index -1;
        }

        private static Position GetLastPosition(List<Position> positions, Periods position)
        {
            var lastPosition = positions.LastOrDefault(x => x.Timestamp == position.End);
            return lastPosition;
        }

        private static void GetDistanceAndDuration(List<Position> positions, Position lastPosition, Position firstPosition,
            TargetViewModel trgt)
        {
            if (lastPosition != null && firstPosition != null)
            {
                var index = GetPreviousPositionIndexx(positions, lastPosition);
                if (index == -1)
                    index = 0;
              var p1 = new GeofenceHelper.Position();
                p1.Latitude = positions.ElementAt(index).Lat;
                p1.Longitude = positions.ElementAt(index).Long;
                var p2 = new GeofenceHelper.Position();
                p2.Latitude = firstPosition.Lat;
                p2.Longitude = firstPosition.Long;

                trgt.Distance = Math.Round(GeofenceHelper.HaversineFormula(p1,p2, GeofenceHelper.DistanceType.Kilometers), 2);
                var avgSpeed = GetAvgSpeed(positions, firstPosition, lastPosition);
                trgt.AvgSpeed = Math.Round(avgSpeed, 2);
                trgt.MaxSpeed = Math.Round(GetMaxSpeed(positions, firstPosition, lastPosition), 2);
              
            }

            if (double.IsNaN(trgt.Distance))
                trgt.Distance = 0;
            trgt.Duration = (DateTime.Parse(trgt.EndPeriod) - DateTime.Parse(trgt.StartPeriod)).TotalSeconds;
        }

        private static Position GetFirstPosition(List<Position> positions, Periods position)
        {
            var firstPosition =
                positions.FirstOrDefault(x => x.Timestamp == position.Start && x.MotionStatus == position.MotionStatus);
            return firstPosition;
        }

        private static void SetEndsTrip(Position lastPosition, TargetViewModel trgt)
        {
            if (lastPosition != null)
            {
                trgt.EndPeriod = lastPosition.Timestamp.ToString("O");
                trgt.EndPeriod1 = lastPosition.Timestamp.ToString("g");

                //   trgt.Latitude = firstPosition.Lat;
                // trgt.Logitude = firstPosition.Long;
                trgt.ArrivalAddres = lastPosition.Address;
            }
        }

        private static void SetBegningsTrip(Position firstPosition, TargetViewModel trgt)
        {
            if (firstPosition != null)
            {
                trgt.MotionStatus = firstPosition.MotionStatus.ToString();

                trgt.StartPeriod = firstPosition.Timestamp.ToString("O");
                trgt.StartPeriod1 = firstPosition.Timestamp.ToString("g");

                trgt.Latitude = firstPosition.Lat;
                trgt.Logitude = firstPosition.Long;
                trgt.StartAddres = firstPosition.Address;
                trgt.MotionStatus = firstPosition.MotionStatus.ToString();
            }
        }

        private static double GetAvgSpeed(List<Position> positions, Position firstPosition, Position lastPosition)
        {
            return positions.Where(x =>
                    x.Timestamp >= firstPosition.Timestamp && x.Timestamp >= lastPosition.Timestamp)
                .Average(x => x.Speed);
        }
        private static double GetMaxSpeed(List<Position> positions, Position firstPosition, Position lastPosition)
        {
            return positions.Where(x =>
                    x.Timestamp >= firstPosition.Timestamp && x.Timestamp >= lastPosition.Timestamp)
                .Max(x => x.Speed);
        }
    }
}