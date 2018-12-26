angular.module('app.controllers').controller('reportController', reportController);
reportController.$inject = ['$scope' , 'reportService'];
function reportController($scope, reportService) {

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
    $scope.thresholds = {
        '0': { color: 'green' },
        '50': { color: 'yellow' },
        '70': { color: "orange" },
        '90': { color: 'red' }
    }
    $scope.vehicles = [];
    reportService.getVehicles().then(function(resp) {
     //   console.log(resp.data);
        $scope.vehicles = resp.data;
    });
    $scope.ExportToPdf =function() {
        var url = '../VehicleReport/ExportReportPdf/?vehicleId=' + $scope.vehicleId + "&startPeriod=" + $scope.startPeriod;
        window.open(url);
    }
    $scope.Download = function() {
// ReSharper disable once RedundantUnits
     
        if ($scope.startPeriod === "" || $scope.vehicleId === "") {
            alert("il faut choisir une date de début et un véhicule");
            return;
        }
        $.when(
            $("#prg .progress-bar").css("width", "20%").addClass("progress-bar-striped active").html("30%")).then(function() {
                reportService.getReport($scope.vehicleId, $scope.startPeriod).then(function (resp) {
                    $scope.optionsAvg = setOptions(resp.data.AvgSpeed != null ? resp.data.AvgSpeed : 0);
                    $scope.options = setOptions(resp.data.MaxSpeed != null ? resp.data.MaxSpeed : 0);
                    $scope.VehicleName = resp.data.VehicleName;
                    $scope.ReportDate = $scope.startPeriod;
                    $scope.Distance = resp.data.Distance;
                    $("#prg .progress-bar").css("width", "80%").addClass("progress-bar-striped active").html("50%");

                    if (resp.data.Positions != null && resp.data.Positions.length > 0) {
                        $scope.BeginService = resp.data.Positions[0].BeginService !== "" ? resp.data.Positions[0].BeginService : "inconnu";
                        $scope.EndService = resp.data.Positions[resp.data.Positions.length - 1].EndService;

                    } else {
                        $scope.BeginService = "inconnu";
                        $scope.EndService = "inconnu";

                    }
                    $("#gps-activity-2").html("");
                    $scope.targetList = [];
                    initGpsData(resp.data.Positions, [], "gps-activity-2");

                    for (var i = 0; i < resp.data.Positions.length; i++) {
                        var item = resp.data.Positions[i];
                         item.Duration = secondsToHms(item.Duration);
                        if (item.MotionStatus === "Stopped")
                            item.MotionStatus = "Arrêt";
                        else item.MotionStatus = "Conduite";
                        $scope.targetList.push(item);
                    }
                     initReportBox();

                    $("#daily-report").show();
                    $("#prg .progress-bar").css({ "width": "100%" }).removeClass("active").html("100%");
                });

        })
               
         $scope.$watch('startPeriod', function () {
// ReSharper disable once RedundantUnits
            $("#prg .progress-bar").css("width", "0%").removeClass("progress-bar-striped").html("0%");
        });
        $scope.$watch('vehicleId', function () {
// ReSharper disable once RedundantUnits
            $("#prg .progress-bar").css("width", "0%").removeClass("progress-bar-striped").html("0%");
        });
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
