angular.module('ut.cloudbuild.setup').controller('ProjectSetupScmBaseCtrl',["$stateParams", "$scope", "$state", "cloudBuildService", function($stateParams, $scope, $state, cloudBuildService) {
    // save settings
    $scope.saveScmSettings = function() {
        var updates = {
            settings: {
                scm: $scope.project.newSettings.scm
            }
        };

        var isEditing = $scope.isEditing;
        cloudBuildService.projects.edit($scope.orgId, $scope.projectId, updates)
            .then(function (result) {
                $scope.project.settings = result.settings;
                if(isEditing) {
                    $state.go('generic.build.project.targets');
                }
                else {
                    $state.go('generic.build.project.setup.platform_select');
                }
            })
            .catch(function (error) {
                $scope.showError('An error occurred while saving the project settings.');
            })
            .finally(function() {
                $scope.setProcessing(false);
            });
    };
}]);
