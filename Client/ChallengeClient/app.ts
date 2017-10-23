'use strict';

// declare modules
angular.module('Authentication', []);
angular.module('Tweeter', []);

var angular: any;

angular.module('BasicHttpAuthExample', [
    'Authentication',
    'Tweeter',
    'ngRoute',
    'ngCookies'
])

    .config(['$routeProvider', function ($routeProvider) {

        $routeProvider
            .when('/login', {
                controller: 'LoginController',
                templateUrl: 'src/modules/authentication/views/login.html',
                hideMenus: true
            })

            .when('/', {
                controller: 'TweetController',
                templateUrl: 'src/modules/tweets/views/tweets.html'
            })

            .otherwise({ redirectTo: '/login' });
    }])

    .run(['$rootScope', '$location', '$cookieStore', '$http',
        function ($rootScope, $location, $cookieStore, $http) {
            // keep user logged in after page refresh
            $rootScope.globals = $cookieStore.get('globals') || {};
            if ($rootScope.globals.currentUser) {
                $http.defaults.headers.common['Authorization'] = 'Basic ' + $rootScope.globals.currentUser.authdata; // jshint ignore:line
            }

            $rootScope.$on('$locationChangeStart', function (event, next, current) {
                // redirect to login page if not logged in
                if ($location.path() !== '/login' && !$rootScope.globals.currentUser) {
                    $location.path('/login');
                }
            });
        }]);