angular.module('ut.cloudbuild.setup').controller('ProjectSetupCredentialsAndroidCtrl',["$stateParams", "$scope", "$state", "$timeout", "$q", "$controller", "modals", "cloudBuildService", function($stateParams, $scope, $state, $timeout, $q, $controller, modals, cloudBuildService) {
    if($scope.target.platform !== 'android') {
        $state.go('generic.build.project.targets');
        return;
    }

    // NOTE: ProjectSetupCredentialsBaseCtrl handles setting up credOptions and selectedCredentialInfo

    $scope.form = {};
    $scope.target.settings.platform = $scope.target.settings.platform || {};

    //-------------- NEXT ---------------------
    $scope.canContinueToNextStep = function() {
        // verify form valid
        if(!$scope.form.android.$valid) {
            return false;
        }

        // label and file required when adding new creds
        var credentialid = $scope.target.credentials.signing.credentialid;
        if(!credentialid) {
            return false;
        }
        else if(credentialid === '__addnew__') {
            return $scope.form.newcreds.$valid && $scope.newCredentials.file;
        }
        else {
            return true;
        }
    };

    $scope.nextStep = function() {
        // upload new credentials (or just return the selected ones)
        var upload = $q.when($scope.selectedCredentialInfo);
        var credentialid = $scope.target.credentials.signing.credentialid;
        if(credentialid === '__addnew__') {
            upload = cloudBuildService.projects.uploadCredentials($scope.orgId, $scope.projectId, 'android', $scope.newCredentials);
        }

        var modal = modals.progress('Saving...');
        upload
            .then(function(credentials) {
                return $scope.updateBuildTargetSettings($scope.target, credentials);
            })
            .then(function(buildtarget) {
                $scope.updateBuildTargetInScope($stateParams.targetId, buildtarget);
                $scope.nextState();
            })
            .catch(function(error) {
                if(error !== 'cancel') {
                    error = _.isObject(error) ? error.data.error : error;
                    var message = "Failed to update credentials! " + error;
                    modals.error(message);
                }
            })
            .finally(function() {
                modal.close();
            });
    };

}]);
