angular.module('ut.cloudbuild.setup').controller('ProjectSetupCredentialsIosCtrl',["$stateParams", "$scope", "$state", "$q", "$controller", "modals", "utilsService", "cloudBuildService", function($stateParams, $scope, $state, $q, $controller, modals, utilsService, cloudBuildService) {
    if($scope.target.platform !== 'ios') {
        $state.go('generic.build.project.targets');
        return;
    }

    // NOTE: ProjectSetupCredentialsBaseCtrl handles setting up credOptions and selectedCredentialInfo

    $scope.form = {};
    $scope.target.settings.platform = $scope.target.settings.platform || {};
    $scope.target.settings.platform.xcodeVersion = $scope.target.settings.platform.xcodeVersion || 'latest';

    //-------------- XCODE VERSION ---------------------
    $scope.xcodeVersionCompatible = function() {
        if(!$scope.availableXcodeVersions) {
            return true;
        }
        return _.contains($scope.availableXcodeVersions, $scope.target.settings.platform.xcodeVersion);
    };

    // wait for xcode versions to load
    utilsService.waitForScopeValueToLoad($scope, 'xcodeVersions', 500).then(function() {
        $scope.availableXcodeVersions = _.pluck($scope.xcodeVersions, 'value');
        var unityVersion = _.find($scope.unityVersions, {value: $scope.target.settings.unityVersion});
        if(unityVersion) {
            $scope.availableXcodeVersions = unityVersion.xcode_versions || [];
        }
    });

    //-------------- NEXT ---------------------
    $scope.canContinueToNextStep = function() {
        // verify form valid
        if(!$scope.form.ios.$valid) {
            return false;
        }

        // label/file required when adding new creds
        var credentialid = $scope.target.credentials.signing.credentialid;
        if(!credentialid) {
            return false;
        }
        else if(credentialid === '__addnew__') {
            return $scope.newCredentials.label && $scope.newCredentials.fileProvisioningProfile && $scope.newCredentials.fileCertificate;
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
            upload = cloudBuildService.projects.uploadCredentials($scope.orgId, $scope.projectId, 'ios', $scope.newCredentials);
        }

        var modal = modals.progress('Saving...');
        var credentials = null;
        upload
            .then(function(uploadedCredentials) {
                credentials = uploadedCredentials;

                // if this is an appstore profile, show a confirm modal
                if(credentials.provisioningProfile && credentials.provisioningProfile.type === 'appstore') {
                    return showAppStoreWarningModal();
                }
                else {
                    return $q.when(credentials);
                }
            })
            .then(function() {
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

    function showAppStoreWarningModal() {
        var message = 'App Store provisioning profile detected. You will not be able to test builds of this project on your device.';
        return modals.confirm('Warning', message).result;
    }

}]);
