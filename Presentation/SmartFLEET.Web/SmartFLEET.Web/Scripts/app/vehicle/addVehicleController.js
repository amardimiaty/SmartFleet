angular.module('app.controllers').controller('addVehicleController', vehicleController);

vehicleController.$inject = ['$scope', 'vehicleService', '$state', '$rootScope'];

function addVehicleController($scope, vehicleService, $state, $rootScope) {
    $scope.vehicle = {};
    vehicleService.getNewVehicle().then(function (response) {
        // console.log(response.data);
        $scope.vehicle = response.data;
    });
    $scope.addVehicle = function (vehicle) {

        vehicleService.addVehicle(vehicle).then(function (resp) {
            console.log(resp.data);
            if (resp.data.ValidationStatus === 2) {
                var msg = "";
                for (var i = 0; i < resp.data.Errors.length; i++) {
                    msg = msg + resp.data.Errors[i];
                }
                $.bootstrapGrowl(msg,
                    {
                        ele: 'body', // which element to append to
                        type: 'danger' // (null, 'info', 'danger', 'success')
                    });
            } else {
                $.bootstrapGrowl("Operation has been terminated  successfully !", {
                    ele: 'body', // which element to append to
                    type: 'success' // (null, 'info', 'danger', 'success')
                });
                $scope.vehicle = {};
               // $("#vehicleModal").modal("hide");
            }
        });
    };
}