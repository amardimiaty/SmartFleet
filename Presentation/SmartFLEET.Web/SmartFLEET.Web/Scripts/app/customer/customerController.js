angular.module('app.controllers').controller('customerController', customerController);

customerController.$inject = ['$scope', 'customerService'];
function customerController($scope, customerService) {
    $scope.customer = {};
    $scope.onValidate = function (customer) {
        
        customerService.addCustomer(JSON.stringify(customer)).then(function(data) {
            console.log(data);
        });
    }
    customerService.getNewCustomer().then(function (response) {
        console.log(response.data);
        $scope.customer = response.data;
    });

}
