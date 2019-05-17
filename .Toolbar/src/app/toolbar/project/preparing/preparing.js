angular.module('ngPanel.toolbar.preparing', [
  'ui.router',
  'ngUnity.cloudPanelService'
])
.config(["$stateProvider", function config($stateProvider) {
  $stateProvider.state('project.preparing', {
    url: 'preparing',
    templateUrl: 'toolbar/project/preparing/preparing.tpl.html',
    controller: 'PreparingCtrl'
  });
}])
.controller('PreparingCtrl', ["$scope", "editorcollab", function PreparingCtrl($scope, editorcollab) {
  $('[data-toggle="tooltip"]').tooltip({
    animation: false,
    delay: {
      show: 0,
      hide: 0
    },
    placement: 'top'
  });
}]);