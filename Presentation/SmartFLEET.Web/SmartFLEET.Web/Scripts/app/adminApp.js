'use strict';
var app = angular.module("smart-admin", ['ui.router', 'app.controllers', 'app.services']);
angular.module('app.services', []);
angular.module('app.controllers', []);

app.config([
    '$stateProvider', function ($stateProvider) {
        $stateProvider.state("users" ,
            {
                url: 'users',
                templateUrl: '../Administrator/User/Index',
                service:'userService',
                controller: 'userController',
            }).state("boxes",
            {
                url: 'gpsDevices',
                templateUrl: '../Administrator/GpsDevice/Index'
            }).state("vehicles",
            {
                url: 'vehicles',
                templateUrl: '../Administrator/Vehicle/Index',
                service: 'vehicleService',
                controller: 'vehicleController'
            })
            .state("vehicleDetail",
                {
                    url: '/detail',
                    templateUrl: '../Administrator/Vehicle/Detail',
                  
                    params: {
                        vehicleId: null
                    }
                })
            .state("customers",
            {
                url: 'customers',
                templateUrl: '../Administrator/Customer/Index',
                service:'customerService',
                controller: 'customerController'
            });
        // $urlRouterProvider.otherwise('/Home');
    }
]);

app.run(function ($rootScope, $state) {

    $rootScope.navigate = function ($event, to, params) {
        console.log(params);
        // If the command key is down, open the URL in a new tab
        if ($event.metaKey) {
            var url = $state.href(to, params, { absolute: true });
            window.open(url, '_blank');

        } else {

            try {
                $state.go(to, params);
            } catch (e) {

            } 
        }

    };

});

/*
app.controller('customerCtrl', ['$scope', 'customerService',
    function ($scope, customerService) {
        $scope.customer = {};
        $scope.onValidate = function () {
            console.log("im here...");
        }
    }]);*/