﻿angular.module('app.services').service('reportService', ['$http', function ($http) {

    this.getReport = function getReport(vehicleId, startPeriod) {
        return $http({
            method: 'GET',
            url: '../VehicleReport/GetDailyVehicleReport/?vehicleId=' + vehicleId + "&startPeriod=" + startPeriod
        });
    };
    this.getVehicles = function() {
        return $http({
            method: 'GET',
            url: '../VehicleReport/GetVehicles'
        });
    }
    this.getReportContent = function getReportContent() {
        return $http({
            method: 'GET',
            url: '../VehicleReport/Index'
        });
    }

}]);
