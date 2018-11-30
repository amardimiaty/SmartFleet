angular.module('app.controllers').controller('positionController', positionController);

positionController.$inject = ['$scope', 'positionService', 'reportService'];
function positionController($scope, positionService,  reportService) {
    $scope.vehicleId = "";
    $scope.startPeriod = "";
    reportService.getVehicles().then(function (resp) {
        //   console.log(resp.data);
        $scope.vehicles = resp.data;
    });
    $scope.$watch("startPeriod",
        function(date) {
            if ($scope.vehicleId !== "") {
                $("#gps-activity").html("");
                $("#vehicle-name").html("");
                $("#date").html("");
                $("#begin-ser").html("");
                $("#end-ser").html("");
                if (!positionModalOpend) {
                    iniJBOX().open();
                    positionModalOpend = true;
                }
                initWait();

                positionService.getPosition($scope.vehicleId, date).then(function(resp) {

                    console.log(resp.data);

                    if (resp.data === null || resp.data === undefined) {
                        $('#jBox1').waitMe("hide");

                        return;

                    }
                    $("#vehicle-name").html("<b>Véhicule: </b>" + resp.data.Vehiclename);
                    if (resp.data.Periods != null && resp.data.Periods.length > 0) {
                        $("#date-pos").html("<b>Date:</b> " + resp.data.Periods[0].CurrentDate);
                        if (resp.data.Periods[0].BeginService != null)
                            $("#begin-ser").html("<b> Début de conduite : </b>" + resp.data.Periods[0].BeginService.split(" ")[1]);
                       else $("#begin-ser").html("<b>Début de conduite</b>: inconnu");
                       var length = resp.data.Periods.length - 1;
                       if (resp.data.Periods[length].EndService != null) $("#end-ser").html("<b>Fin de conduite :</b> " + resp.data.Periods[length].EndService.split(" ")[1]);
                       initGpsData(resp.data.Periods, resp.data.GpsCollection, "gps-activity");
                       var lat = resp.data.Periods[length].Latitude;
                       var lng = resp.data.Periods[length].Logitude;
                       map.setView([lat, lng], 15);
                   }
                    $('#jBox1').waitMe("hide");
                });
            }
        });
}