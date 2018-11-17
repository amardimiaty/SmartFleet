angular.module('app.services').service('vehicleService', ['$http', function ($http) {

    this.getVehicle = function getVehicle(vehicleId) {
        return $http({
            method: 'GET',
            url: '../Administrator/Vehicle/GetVehicleDetail/?vehicleId=' + vehicleId
        });
    };
    this.getAllVehicles = function getAllVehicles() {
        return $http({
            method: 'GET',
            url: '../Administrator/Vehicle/GetAllVehicles/' 
        });
    };
    this.getNewVehicle = function getNewVehicle() {
        return $http({
            method: 'GET',
            url: '../Administrator/Vehicle/GetNewVehicle/'
        });
    }
    this.addVehicle = function addVehicle(vehicle) {
        //  console.log(vehicle);
        return $http.post("../Administrator/Vehicle/AddNewVehicle", JSON.stringify(vehicle));
    }
}]);
