
var app = angular.module("smart-fleet", ['ui.router', 'angularjs-gauge','app.controllers', 'app.services']);
angular.module('app.services', []);
angular.module('app.controllers', []);
app.config([
    '$stateProvider',function ($stateProvider) {
        $stateProvider.state("about",
            {
                url: '/About',
                templateUrl: '/Home/About'
            });
       // $urlRouterProvider.otherwise('/Home');
    }
]);


