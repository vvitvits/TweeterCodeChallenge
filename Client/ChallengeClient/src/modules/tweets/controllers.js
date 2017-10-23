'use strict';

angular.module('Tweeter')

    .controller('TweetController',
    ['$scope', '$http', '$timeout', '$location', 'AuthenticationService',
        function ($scope, $http, $timeout, $location, AuthenticationService) {
            $scope.logout = function () {
                AuthenticationService.ClearCredentials();
                $location.path('/login');
            }
            if (!AuthenticationService.IsCredentialsSet) {
                $location.path('/login');
            }

            var loadTweets = function (filter) {
                $scope.loading = true;
                //load tweets
                $http.get('https://codechallengevv.azurewebsites.net/Service1.svc/GetTweets?filter=' + ((filter == undefined) ? '' : filter))
                    .then(function (response) {
                        $scope.tweets = response.data
                        $scope.loading = false;
                    }, function errorCallback(response) {
                        //TODO: Display error
                    });
            }

            //Set timer to refresh tweets every min
            $timeout(function () {
                if (!$scope.loading) {
                    loadTweets($scope.filter);
                }
            }, 60000)

            //Watch filter change;
            $scope.$watch("filter", function () {
                loadTweets($scope.filter);
            });
        }]);