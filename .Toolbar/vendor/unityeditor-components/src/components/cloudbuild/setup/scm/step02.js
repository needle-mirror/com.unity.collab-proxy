angular.module('ut.cloudbuild.setup').controller('ProjectSetupStep02Ctrl',["$stateParams", "$scope", "$state", "$controller", "scmUtils", "cloudBuildService", function($stateParams, $scope, $state, $controller, scmUtils, cloudBuildService) {
    // force you back to step 1 if we don't have scm info...
    if(!$scope.project.newSettings || !$scope.project.newSettings.scm || !$scope.project.newSettings.scm.type) {
        $state.go('generic.build.project.setup.scm');
        return;
    }

    $scope.project.newSettings.scm.pass = $scope.project.settings.scm.pass;
    $scope.initialPass = $scope.project.newSettings.scm.pass;
    $scope.project.newSettings.scm.p4authtype = typeof($scope.project.newSettings.scm.p4authtype) !== "undefined" ? $scope.project.newSettings.scm.p4authtype : "ticket";

    // Options for P4 auth type
    $scope.p4AuthTypes = [
        {
            'value': 'ticket',
            'text': 'Ticket'
        },
        {
            'value': 'password',
            'text': 'Password'
        }
    ];

    // add base scm setup controller
    $controller('ProjectSetupScmBaseCtrl', {$scope: $scope});

    // populate some info
    $scope.repoTypeName = scmUtils.getScmTypeNameFromType($scope.project.newSettings.scm.type);
    $scope.repoHost = scmUtils.detectScmHost($scope.project.newSettings.scm.url);

    // get the ssh key for this project
    $scope.sshPublicKey = "Loading...";
    cloudBuildService.projects.sshkey($scope.project.orgid, $scope.project.projectid).then(function(result) {
        $scope.sshPublicKey = result.publickey;
    }, function(error) {
        $scope.sshPublicKey = "Error loading SSH Key!";
    });

    $scope.canContinueToNextStep = function() {
        var scmSettings = $scope.project.newSettings.scm;
        if(scmSettings.type === 'git') {
            return true;
        }
        else {
            return scmSettings.user && scmSettings.pass;
        }
    };

    $scope.nextStep = function() {
        // mark as processing
        $scope.setProcessing(true);

        // clear errors
        $scope.clearError();

        // only include the password if it changed
        if($scope.project.newSettings.scm.pass === $scope.initialPass) {
            delete $scope.project.newSettings.scm.pass;
        }

        // re-check the url with new settings
        var failureMessage = "Repo is not accessible. Please check your URL and repo settings.";
        cloudBuildService.projects.scm.checkUrl($scope.project.orgid, $scope.project.projectid, $scope.project.newSettings.scm)
            .then(function(result) {
                // save settings
                if(result.isAccessible) {
                    if (result.capturedInfo) {
                        $scope.project.newSettings.scm.fingerprint = result.capturedInfo.fingerprint;
                    }

                    $scope.saveScmSettings();
                }
                else {
                    $scope.setProcessing(false);
                    $scope.showError(failureMessage, result.errorMessages);
                }
            }, function(error) {
                $scope.setProcessing(false);
                $scope.showError(failureMessage);
            });
    };
}]);
