using System;
using System.Collections.Generic;
using System.Linq;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.Domain.Vehicles;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.DailyRports
{
    public class CompleteDailyReport
    {
        public CompleteDailyReport()
        {
            
        }
        public CompleteDailyReport(List<Position> positions, Vehicle vehicle)
        {
            AvgSpeed = Math.Round(positions.Average(x => x.Speed),2);
            MaxSpeed = Math.Round(positions.Max(x => x.Speed),2);
            ReportDate = positions.FirstOrDefault().Timestamp.Date.ToShortDateString();
            VehicleName = vehicle.VehicleName;
            Positions = new List<TargetViewModel>();
            var positionReport = new PositionReport();
            Positions.AddRange( positionReport.GetTargetViewModels(positions, positions.FirstOrDefault().Timestamp.Date, vehicle.VehicleName));
            Distance = Positions.Where(x=>x.MotionStatus == MotionStatus.Moving.ToString()).Sum(x => x.Distance);
            Distance = Math.Round(Distance, 2);

        }

        public string Day { get; set; }
        public double MaxSpeed { get; set; }
        public double AvgSpeed { get; set; }
        public double Distance { get; set; }
        public string VehicleName { get; set; }
        public string ReportDate { get; set; }
        public List<TargetViewModel> Positions { get; set; }
    }
}