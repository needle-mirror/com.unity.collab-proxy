angular.module('ngPanel.nolocalhost', [
    'ui.router'
])
.config(["$stateProvider", function config($stateProvider) {
    $stateProvider.state('nolocalhost', {
        url: '/no-localhost',
        views: {
            'root': {
                templateUrl: 'toolbar/nolocalhost/nolocalhost.tpl.html',
                controller: 'NoLocalhostCtrl'
            }
        }
    });
}]).controller('NoLocalhostCtrl', ["$scope", "unityProjectService", function($scope, unityProjectService) {
    unityProjectService.IsReady().then(function() {
        unityProjectService.GetRESTServiceURI().then(function (restServiceUrl) {
            $scope.localUrl = restServiceUrl;
        });
    });
}]);
