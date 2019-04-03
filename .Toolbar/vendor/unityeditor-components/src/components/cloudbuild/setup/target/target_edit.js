angular.module('ut.cloudbuild.setup')
  .controller('ProjectSetupTargetEditCtrl', ["$stateParams", "$scope", "$state", "$timeout", "modals", "cloudBuildService", "$q", function ($stateParams, $scope, $state, $timeout,
                                                      modals, cloudBuildService, $q) {

    var buildTargetLoader = $q.when($scope.buildTargets);
    if (angular.isUndefined($scope.buildTargets) || $scope.buildTargets.length === 0) {
       buildTargetLoader = cloudBuildService.projects.getBuildTargets($scope.orgId, $scope.projectId);
    }
    buildTargetLoader.then(function(buildTargets) {
      $scope.buildTargets = buildTargets;
      var buildtargetid = $stateParams.targetId;

      // verify the target exists
      var foundTarget = _.find($scope.buildTargets, {buildtargetid: buildtargetid});
      if (!foundTarget) {
        modals.error("Build target '" + buildtargetid + "' does not exist!").result.then(function () {
          $state.go('generic.build.project.targets');
        });
      }
      else {
        // use a copy of the target for editing
        $scope.target = _.merge({}, foundTarget);
      }
    });
  }]);