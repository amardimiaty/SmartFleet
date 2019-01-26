using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.Geofence;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.DailyRports
{
    /// <summary>
    /// 
    /// </summary>
    public class PositionReport
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionsOfVehicle"></param>
        /// <returns></returns>
        public  List<PositionViewModel> PositionViewModels(List<Position> positionsOfVehicle )
        {
            var positions = new List<PositionViewModel>();

            // get current account's vehicles
            foreach (var p in positionsOfVehicle )
                positions.Add(new PositionViewModel(p, p.Vehicle));

            return positions;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="startPeriod"></param>
        /// <param name="vehicleName"></param>
        /// <returns></returns>
        public List<TargetViewModel> BuidDailyReport(List<Position> positions, DateTime startPeriod, string vehicleName)
        {
           // string timeZone = "W. Central Africa Standard Time";
          //  TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            //foreach (var position in positions.OrderBy(w=>w.Timestamp))
            //    position.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(position.Timestamp, cstZone);
            // ReSharper disable once TooManyChainedReferences
            var firstPos = positions.OrderBy(p => p.Timestamp).ThenBy(p => p.MotionStatus).FirstOrDefault();
            var currentStatus =firstPos ?.MotionStatus;
            var start = positions.FirstOrDefault()?.Timestamp;
            var periods = new List<Periods>();
            var orderedEnumerable = positions.OrderBy(p => p.Timestamp).ThenBy(p=>p.MotionStatus);
            // ReSharper disable once PossibleMultipleEnumeration
            var motionStatusCount = orderedEnumerable.GroupBy(x => x.MotionStatus).Count();
            var motionStatusChanged = false;
            // rearrange periods by date / status of driving on 24 hour scale
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var position in orderedEnumerable)
            {
                motionStatusChanged = MotionStatusChanged(position, periods, ref currentStatus, ref start);
            }
           
            // check if there is only one period or the last period is missing  to add both to the periods list 
            // ReSharper disable once ComplexConditionExpression
            if (motionStatusCount == 1 || !motionStatusChanged)
            {
                if (motionStatusCount == 1)
                {
                    // if theres is only one period we add it to the list
                    // ReSharper disable once PossibleMultipleEnumeration
                    // ReSharper disable once PossibleNullReferenceException
                    var poriod = new Periods(orderedEnumerable.LastOrDefault().Timestamp, start, currentStatus);
                    periods.Add(poriod);
                }
                else if (!motionStatusChanged)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var poriod = GetLastPeriods(periods, orderedEnumerable);
                    periods.Add(poriod);
                }
            }
            var tmp = new List<Periods>();
            // get rid of the periods with duration less than 60 sec
            GetRidOfShortPeriods(periods, tmp);
            // create the final report
            
            return BuildReport(positions, startPeriod, vehicleName, periods);
        }

        private static void ReMergePeriods(List<Periods> periods, List<Periods> tmp)
        {
            foreach (var period in periods)
            {
                var index = periods.FindIndex(p => p == period);
                // ReSharper disable once ComplexConditionExpression
                if (index + 1 >= periods.Count || period.MotionStatus != periods[index + 1].MotionStatus) continue;
                // ReSharper disable once ComplexConditionExpression
                while (index + 1 < periods.Count && period.MotionStatus == periods[index + 1].MotionStatus)
                {
                    period.End = periods[index + 1].End;
                    tmp.Add(periods[index + 1]);
                    index++;
                }
            }
            // get rid of the periods have been merged

            DeleteTempPeriods(tmp, periods);


        }

        private static void GetRidOfShortPeriods(List<Periods> periods, List<Periods> tmp)
        {
            foreach (var period in periods)
                if ((period.End - period.Start).TotalSeconds < 60)
                    tmp.Add(period);
            DeleteTempPeriods(tmp, periods);
            tmp = new List<Periods>();
            // re merge periods after removing short periods
            ReMergePeriods(periods, tmp);
        }

        private static Periods GetLastPeriods(List<Periods> periods, IOrderedEnumerable<Position> orderedEnumerable)
        {
            var end = periods.LastOrDefault()?.End;
            var query = orderedEnumerable.Where(x => x.Timestamp >= end);
            var enumerable
                = query as Position[] ?? query.ToArray();
            var poriod = new Periods(enumerable.LastOrDefault()?.Timestamp, end, enumerable.LastOrDefault()?.MotionStatus);
            return poriod;
        }

        // ReSharper disable once MethodTooLong
        // ReSharper disable once TooManyArguments
        private static List<TargetViewModel> BuildReport(List<Position> positions, DateTime startPeriod, string vehicleName, List<Periods> periods)
        {
            List<TargetViewModel> targets = new List<TargetViewModel>();
            foreach (var position in periods.Distinct())
            {
                var trgt = new TargetViewModel();
                // get the first period 
                var firstPosition = GetFirstPosition(positions, position);
                //get  last period
                var lastPosition = GetLastPosition(positions, position);
                // set the begnings of the period
                SetBegningsTrip(firstPosition, trgt);
                // set the ends of the period
                SetEndsTrip(lastPosition, trgt);
                // calculate distance , speeds , and duration
                GetDistanceAndDuration(positions, lastPosition, firstPosition, trgt);
                var index = GetPreviousPositionIndexx(positions, lastPosition);
                if (index == -1)
                    index = 0;
                trgt.Speed = positions.ElementAt(index).Speed;
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

        // ReSharper disable once TooManyArguments
        private static bool MotionStatusChanged(Position position, List<Periods> periods, ref MotionStatus? currentStatus,
            ref DateTime? start)
        {
            Trace.WriteLine("Timestamp: " + position.Timestamp + " MotionStatus: " + position.MotionStatus);
            if (currentStatus == position.MotionStatus)
                return false;
            var poriod = new Periods(position.Timestamp, start, currentStatus);
            currentStatus = position.MotionStatus;
            start = position.Timestamp;
            periods.Add(poriod);
            Trace.WriteLine("period added at : " + position.Timestamp + " MotionStatus: " + position.MotionStatus);
            return true;
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

        // ReSharper disable once MethodTooLong
        // ReSharper disable once TooManyArguments
        private static void GetDistanceAndDuration(List<Position> positions, Position lastPosition, Position firstPosition,
            TargetViewModel trgt)
        {
            // ReSharper disable once ComplexConditionExpression
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
                // ReSharper disable once ComplexConditionExpression
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