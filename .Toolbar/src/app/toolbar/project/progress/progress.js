angular.module('ngPanel.toolbar.progress', [
  'ngUnity.editorcollab',
  'ui.router'
  ])
.config(["$stateProvider", function config($stateProvider) {
  $stateProvider.state('project.progress', {
    url: 'progress',
    templateUrl: 'toolbar/project/progress/progress.tpl.html',
    controller: 'ProgressCtrl'
  });
}])
.controller('ProgressCtrl', ["$scope", "$state", "$localStorage", "$rootScope", "unityProjectService", "editorcollab", function ProgressCtrl($scope, $state, $localStorage, $rootScope, unityProjectService, editorcollab) {
  $scope.title = 'Transfer Queued';
  $scope.info = '';
  $scope.working = false;
  $scope.current = 0;
  $scope.max = 100;
  $scope.canCancel = false;
  
  var onTransferProgressChange = function(transferProgress){
    $scope.canCancel = transferProgress.canCancel;

    if(transferProgress.completed) {
      if(!transferProgress.errorOccured) {
        $localStorage.comment = '';
        $scope.title = 'Transfer Completed';
        $scope.info = '100%';
      }
    } else {    
      $scope.current = transferProgress.percentComplete;
      $scope.info = transferProgress.percentComplete+'%';
      if(transferProgress.extraInfo.length > 0) {
        $scope.title = transferProgress.extraInfo;
      } else {
        $scope.title = transferProgress.title;
      }
    }
  };

  $scope.doCancel = function() {
    if (editorcollab.canCancelJob()) {
      $scope.working = true;
      editorcollab.cancelJob();
      $scope.$emit('refreshState');
    }
  };

  editorcollab.onTransferProgress($scope, onTransferProgressChange);
}]);
