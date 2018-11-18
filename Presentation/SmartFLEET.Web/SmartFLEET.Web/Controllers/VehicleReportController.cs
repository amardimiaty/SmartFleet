using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SmartFleet.Data;
using SmartFleet.Service.Tracking;
using SmartFLEET.Web.DailyRports;
using SmartFLEET.Web.Helpers;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Controllers
{
    public class VehicleReportController : BaseController
    {
        private readonly IPositionService _positionService;

        // GET: VehicleReport
        public ActionResult Index()
        {
            return PartialView();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetVehicles()
        {
            var cst = ObjectContext.UserAccounts.Include(x => x.Customer)
                .FirstOrDefault(x => x.UserName == User.Identity.Name)?.Customer;
            var vehicles = await ObjectContext.Vehicles.Where(x => x.CustomerId == cst.Id).Select(x=>new {x.VehicleName, x.Id}).ToArrayAsync();
            return Json(vehicles, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetDailyVehicleReport(string vehicleId, string startPeriod)
        {
            var id = Guid.Parse(vehicleId);
            var start = new DateTime();
            try
            {
                DateTime.TryParseExact(startPeriod, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out start);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            var endPeriod = start.AddHours(24).AddTicks(-1);
            var vehicle = await ObjectContext.Vehicles.FindAsync(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, start, endPeriod);
            if (positions.Any())
                return Json(new CompleteDailyReport(positions, vehicle), JsonRequestBehavior.AllowGet);
            return Json(
                new CompleteDailyReport
                {
                    VehicleName = vehicle?.VehicleName,
                    ReportDate = start.ToShortDateString(),
                    Positions = new List<TargetViewModel>()
                }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// export daily report to pdf
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <param name="startPeriod"></param>
        /// <returns></returns>
        public async Task<FileResult> ExportReportPdf(string vehicleId, string startPeriod)
        {
            var id = Guid.Parse(vehicleId);
            var start = new DateTime();
            try
            {
                DateTime.TryParseExact(startPeriod, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out start);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            var endPeriod = start.AddHours(24).AddTicks(-1);
            var vehicle = await ObjectContext.Vehicles.FindAsync(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, start, endPeriod);
            Document doc = new Document(PageSize.A4, 15, 15, 15, 15);
            MemoryStream stream = new MemoryStream();

            if (positions.Any())
            {
                var report = new CompleteDailyReport(positions, vehicle);
                string[] headers =
                    {"Conduite", "Début", "Fin", "Distance", "Adresse départ", "Adresse d'arrivée", "Vitesse moy"};
                float[] columnWidths = {140f, 140, 140, 120, 200f, 200f, 100};
                PdfWriter.GetInstance(doc, stream).CloseStream = false;
                doc.Open();
                PdfPTable tblHeader = new PdfPTable(2);
                tblHeader.WidthPercentage = 100;

                PdfPCell leftCell = new PdfPCell();
                leftCell.Border = 0;
                iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph($"Rapport de conduite",
                    FontFactory.GetFont("Calibri", 18, BaseColor.BLACK));
                paragraph.Alignment = Element.ALIGN_LEFT;
                leftCell.AddElement(paragraph);
                leftCell.AddElement(null);
                leftCell.AddElement(null);
                PdfPCell rightCell = new PdfPCell();
                rightCell.Border = 0;
                var prg = new Paragraph("Véhicule : " + report.VehicleName);
                rightCell.AddElement(prg);
                prg = new Paragraph("Date : " + report.ReportDate);
                rightCell.AddElement(prg);
                PdfPCell emptyCell = new PdfPCell();
                emptyCell.Border = 0;
                tblHeader.AddCell(leftCell);
                tblHeader.AddCell(rightCell);
                doc.Add(tblHeader);
                doc.Add(new Chunk(Environment.NewLine));
                iTextSharp.text.Paragraph p = new iTextSharp.text.Paragraph(new Chunk(
                    new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                // call the below to add the line when required.
                doc.Add(p);
                doc.Add(new Chunk(Environment.NewLine));
                doc.Add(new Chunk(Environment.NewLine));
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 90;
                var bodyLeftCell = new PdfPCell();
                bodyLeftCell.Border = 0;
                bodyLeftCell.AddElement(
                    new Paragraph("Distance : " + report.Distance.ToString(CultureInfo.InvariantCulture) +" Km.") );
                bodyLeftCell.AddElement(
                    new Paragraph("Vitesse moyene: " + report.AvgSpeed.ToString(CultureInfo.InvariantCulture) + " Km/h"));
                bodyLeftCell.AddElement(new Paragraph("Vitesse max: " + report.MaxSpeed + " Km/h"));

                var bodyRightCell = new PdfPCell()
                    ;
                bodyRightCell.Border = 0;
                table.AddCell(bodyLeftCell);
                // table.AddCell(emptyCell);
                table.AddCell(bodyRightCell);
                table.DefaultCell.Border = 0;
                ;

                doc.Add(table);
                doc.Add(new Chunk(Environment.NewLine));
                p.Alignment = Element.ALIGN_CENTER;


                doc.Add(new Chunk(Environment.NewLine));
                doc.Add(new Chunk(Environment.NewLine));

                PdfPTable pTable = new PdfPTable(headers.Length);
                pTable.WidthPercentage = 90;
                pTable.SetWidths(columnWidths);
                foreach (string t in headers)
                {
                    var cell = new PdfPCell(new Phrase(t, FontFactory.GetFont("Calibri", 12, BaseColor.BLACK)))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        BackgroundColor = new BaseColor(230, 230, 250)
                    };


                    pTable.AddCell(cell);
                }

                foreach (var position in report.Positions.OrderBy(x=>x.StartPeriod))
                {
                    pTable.AddCell(ItextSharpHelper.Cell(position.MotionStatus == "Stopped"? "Arrêt":"Conduite", BaseColor.BLACK));
                    pTable.AddCell(ItextSharpHelper.Cell(DateTime.Parse(position.StartPeriod).ToShortTimeString().ToString(), BaseColor.BLACK));
                    pTable.AddCell(ItextSharpHelper.Cell(DateTime.Parse(position.EndPeriod).ToShortTimeString(), BaseColor.BLACK));
                    pTable.AddCell(ItextSharpHelper.Cell(position.Distance.ToString(CultureInfo.InvariantCulture),
                        BaseColor.BLACK));
                    pTable.AddCell(ItextSharpHelper.Cell(position.StartAddres, BaseColor.BLACK));
                    pTable.AddCell(ItextSharpHelper.Cell(position.ArrivalAddres, BaseColor.BLACK));
                    pTable.AddCell(ItextSharpHelper.Cell(position.AvgSpeed.ToString(CultureInfo.InvariantCulture),
                        BaseColor.BLACK));
                }

             //   doc.Add(p);
                doc.Add(pTable);
                doc.Close();
            }

            stream.Flush(); //Always catches me out
            stream.Position = 0; //Not sure if this is required

            return File(stream, "application/pdf", $"reeport_{vehicle?.VehicleName}_{DateTime.Now.Date:yyyy-MM-dd}.pdf");


        }
        public VehicleReportController(SmartFleetObjectContext objectContext, IMapper mapper, IPositionService positionService) : base(objectContext, mapper)
        {
            _positionService = positionService;
        }
    }
}