angular.module('app.controllers').controller('addCustomerController', addCustomerController);

customerController.$inject = ['$scope', 'customerService','userService', '$state', '$rootScope'];

function addCustomerController($scope, customerService, userService,$state, $rootScope) {
    $scope.customer = {};
    $scope.users = [];
    $scope.user = {};
    var timeZoneInfos = [];
    $scope.onValidate = function (customer) {

        customerService.addCustomer(JSON.stringify(customer)).then(function (data) {
            console.log(data);
        });
    }
    userService.getTimeZones().then(function (res) {
       
        $scope.user.TimeZoneInfos = timeZoneInfos= res.data.map(z => z.DisplayName);
    });
    $scope.loadUser = function() {
        jQuery("#add-user").modal("show");
    }
    $scope.save = function() {
        $scope.users.push($scope.user);
        $scope.user = {};
        $scope.user.TimeZoneInfos = timeZoneInfos;
    }
    customerService.getNewCustomer().then(function (response) {
        console.log(response.data);
        $scope.customer = response.data;
    });
}