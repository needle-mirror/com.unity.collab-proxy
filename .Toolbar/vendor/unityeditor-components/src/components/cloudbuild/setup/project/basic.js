angular.module('ut.cloudbuild.setup').controller('ProjectSetupBasicCtrl',["$stateParams", "$scope", "$state", "$timeout", "modals", "setupConstants", "cloudBuildService", function($stateParams, $scope, $state, $timeout, modals, setupConstants, cloudBuildService) {
    $scope.isEditing = true;
    $scope.initialRemoteCacheStrategy = $scope.project.settings.remoteCacheStrategy;

    $scope.nextStep = function() {
        // handle saving the project
        var updates = {
            name: $scope.project.name,
            settings: {
                remoteCacheStrategy: $scope.project.settings.remoteCacheStrategy
            }
        };
        var request = cloudBuildService.projects.edit($scope.project.orgid, $scope.project.projectid, updates);

        var modal = modals.progress('Saving...');
        request
            .then(function(project) {
                $scope.project = project;
                nextState(project);
            })
            .catch(function(error) {
                modals.error("Failed to update project settings: " + error.data.error);
            })
            .finally(function() {
                modal.close();
            });
    };

    function nextState(project) {
        $state.go('generic.build.project.targets');
    }
}]);
