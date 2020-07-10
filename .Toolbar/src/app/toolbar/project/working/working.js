angular.module('ngPanel.toolbar.working', [
    'ui.router',
    'ngUnity.cloudPanelService'
])
.config(["$stateProvider", function config($stateProvider) {
    $stateProvider.state('project.working', {
        url: 'working',
        templateUrl: 'toolbar/project/working/working.tpl.html',
        controller: 'WorkingCtrl'
    });
}])
.controller('WorkingCtrl', ["$scope", "editorcollab", function PreparingCtrl($scope, editorcollab) {
    $('[data-toggle="tooltip"]').tooltip({
        animation: false,
        delay: {
            show: 0,
            hide: 0
        },
        placement: 'top'
    });
}]);
