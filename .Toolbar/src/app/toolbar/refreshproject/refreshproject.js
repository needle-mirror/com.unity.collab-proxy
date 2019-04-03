angular.module('ngPanel.refreshproject', [
  'ui.router',
  'ngUnity.cloudPanelService'
])
.config(["$stateProvider", function config($stateProvider) {
  $stateProvider.state('refreshproject', {
    url: '/refresh-project',
    views: {
      'root': {
        templateUrl: 'toolbar/refreshproject/refreshproject.tpl.html'
      }
    }
  });
}]);
