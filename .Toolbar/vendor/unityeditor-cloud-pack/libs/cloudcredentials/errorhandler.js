angular.module('ngUnity.cloudcredentials')
    .factory('errorHandlingInterceptor', ["$rootScope", "$q", function($rootScope, $q){
        var SEAT_UPDATED_MESSAGE = "seatUpdated";
        var HTTP_ERROR_DETAIL_MESSAGE = "httpErrorDetailMessage";

        var errorHandlingInterceptor = {
            responseError: function(rejection) {
                if(rejection.status === 403 && rejection.data.code === 'NO_SEAT') {
                    $rootScope.$broadcast(SEAT_UPDATED_MESSAGE, {
                        seat: false
                    });
                }
                if(rejection.data && rejection.data.detail) {
                    $rootScope.$broadcast(HTTP_ERROR_DETAIL_MESSAGE, rejection.data.detail);
                }
                return $q.reject(rejection);
            },
            onSeatUpdated: function ($scope, callback) {
                $scope.$on(SEAT_UPDATED_MESSAGE, function (event, data) {
                    callback(data);
                });
            },
            onHttpError: function ($scope, callback) {
                $scope.$on(HTTP_ERROR_DETAIL_MESSAGE, function (event, data) {
                    callback(data);
                });
            }
        };

        return errorHandlingInterceptor;
    }])
    .config(['$httpProvider', function($httpProvider) {
        $httpProvider.interceptors.push('errorHandlingInterceptor');
    }]);