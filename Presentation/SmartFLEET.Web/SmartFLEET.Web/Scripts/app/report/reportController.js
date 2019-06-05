angular.module('app.controllers').controller('reportController', reportController);
reportController.$inject = ['$scope', 'reportService', '$compile','$http'];
function reportController($scope, reportService, $compile, $http) {
    var template = '<report-vehicle></report-vehicle>';
    var tpl1 ='<div class="col-md-12" id="gps-activity-2" style="width: 99%; height: 100px"></div>';
    $scope.startPeriod = "";
    $scope.BeginService = "";
    $scope.EndService = "";
    $scope.endPeriod = "";
    $scope.VehicleName = "";
    $scope.ReportDate = "";
    $scope.Distance = 0;
    $scope.vehicleId = "";
    $scope.optionsAvg = {};
    $scope.options = {}
    $scope.targetList = [];
    $scope.enableThresholds = true;
    $scope.activities = [];
    $scope.thresholds = {
        '0': { color: 'green' },
        '50': { color: 'yellow' },
        '70': { color: "orange" },
        '90': { color: 'red' }
    }
    //$scope.vehicles = [];
    //reportService.getVehicles().then(function(resp) {
    // //   console.log(resp.data);
    //    $scope.vehicles = resp.data;
    //});
    $scope.ExportToPdf =function() {
        var url = '../VehicleReport/ExportReportPdf/?vehicleId=' + $scope.vehicleId + "&startPeriod=" + $scope.startPeriod;
        window.open(url);
    }
    $scope.Download = function() {
// ReSharper disable once RedundantUnits
        console.log("im here");
        if ($scope.startPeriod === "" || $scope.vehicleId === "") {
            alert("il faut choisir une date de début et un véhicule");
            return;
        }
       $http.get('../VehicleReport/GetDailyVehicleReport/?vehicleId=' + $scope.vehicleId + "&startPeriod=" + $scope.startPeriod)
            .then(function (resp) {
                console.log(resp);
                    $scope.optionsAvg = setOptions(resp.data.AvgSpeed != null ? resp.data.AvgSpeed : 0);
                    $scope.options = setOptions(resp.data.MaxSpeed != null ? resp.data.MaxSpeed : 0);
                    $scope.VehicleName = resp.data.VehicleName;
                    $scope.ReportDate = $scope.startPeriod;
                    $scope.Distance = resp.data.Distance;

                    if (resp.data.Positions != null && resp.data.Positions.length > 0) {
                        $scope.BeginService = resp.data.Positions[0].BeginService !== ""
                            ? resp.data.Positions[0].BeginService
                            : "inconnu";
                        $scope.EndService = resp.data.Positions[resp.data.Positions.length - 1].EndService;

                    } else {
                        $scope.BeginService = "inconnu";
                        $scope.EndService = "inconnu";

                    }
                    $("#gps-activity-2").html("");
                    // 
                    $scope.activities = resp.data.Positions;
                    initGpsData(resp.data.Positions, [], "gps-activity-2");
              
                    $scope.targetList = [];

                    for (var i = 0; i < resp.data.Positions.length; i++) {
                        var item = resp.data.Positions[i];
                        item.Duration = secondsToHms(item.Duration);
                        if (item.MotionStatus === "Stopped")
                            item.MotionStatus = "Arrêt";
                        else item.MotionStatus = "Conduite";
                        $scope.targetList.push(item);
                    }
                    $("#daily-report").show();
                    $("#report-content").html("");
                    var com = $compile(template)($scope);
                    $("#report-content").append(com);
                   
                    $("#report-win").append($("#report-content"));
                    $("#prg-wwin").window('close');
   
              
                
                
            });
               
        
    }
    //$scope.$watch('startPeriod', function () {
    //    // ReSharper disable once RedundantUnits
    //    $("#prg .progress-bar").css("width", "0%").removeClass("progress-bar-striped").html("0%");
    //});
    //$scope.$watch('vehicleId', function () {
    //    // ReSharper disable once RedundantUnits
    //    $("#prg .progress-bar").css("width", "0%").removeClass("progress-bar-striped").html("0%");
    //});
    $scope.downloadFullReport = function() {
        console.log("here !!");
    }
}
function setOptions(value) {
    return {
        type: 'arch',
        cap: 'round',
        size: 150,
        value: value,
        thick: 10,
        label: 'KM/H',
        //append: 'Km/h',
        min: 0,
        max: 160,
        foregroundColor: 'rgba(0, 150, 136, 1)',
        backgroundColor: 'rgba(0, 0, 0, 0.1)'
    };
}
