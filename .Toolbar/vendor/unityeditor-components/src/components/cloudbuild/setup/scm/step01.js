angular.module('ut.cloudbuild.setup').controller('ProjectSetupStep01Ctrl',["$stateParams", "$scope", "$state", "$controller", "scmUtils", "cloudBuildService", function($stateParams, $scope, $state, $controller, scmUtils, cloudBuildService) {
    // add base scm setup controller
    $controller('ProjectSetupScmBaseCtrl', {$scope: $scope});

    // copy project scm settings
    if(!$scope.project.newSettings) {
        $scope.project.newSettings = {};
        $scope.project.newSettings.scm = _.merge({}, $scope.project.settings.scm);
    }

    // populate constants
    $scope.repoTypes = _.clone(scmUtils.data().repoTypes);
    $scope.repoHosts = scmUtils.data().repoHosts;

    // try to auto-detect the scm type as the user enters the url
    $scope.$watch('project.newSettings.scm.url', function(newValue, oldValue) {
        var detectedScmType = scmUtils.detectScmType(newValue);
        if(detectedScmType) {
            $scope.project.newSettings.scm.type = detectedScmType;
        }
    });

    $scope.$watch('project.newSettings.scm.type', function(newValue, oldValue) {
        if(newValue === 'collab') {
            $scope.project.newSettings.scm.url = 'Unity Collab';
        }
    });

    $scope.canContinueToNextStep = function() {
        if(!$scope.reposetup.$valid) {
            return false;
        }

        var scmSettings = $scope.project.newSettings.scm;
        return scmSettings.url && scmSettings.type;
    };

    $scope.nextStep = function() {
        // mark as processing
        $scope.setProcessing(true);

        // clear errors
        $scope.clearError();

        // get the allowed scm types from the billing plan
        var allowedScmTypes = [];
        if($scope.plan && $scope.plan.effective) {
            allowedScmTypes = $scope.plan.effective.scmTypes || [];
        }

        // verify this billing plan supports the selected scm type
        var repoType = $scope.project.newSettings.scm.type;
        if(allowedScmTypes && allowedScmTypes.length > 0 && allowedScmTypes.indexOf(repoType) === -1) {
            var repoTypeName = scmUtils.getScmTypeNameFromType(repoType);
            $scope.showError(repoTypeName + " is not supported in your current Billing Plan. <a ui-sref='generic.plans'>Please upgrade</a> your plan to continue.");
            return;
        }

        // provide empty user/pass since this could be an edit and don't want to use the saved versions
        var scmSettings = {
            type: $scope.project.newSettings.scm.type,
            url: $scope.project.newSettings.scm.url,
            user: '',
            pass: ''
        };

        // check the url
        cloudBuildService.projects.scm.checkUrl($scope.project.orgid, $scope.project.projectid, scmSettings)
            .then(function(result) {
                // cache normalized url
                $scope.project.newSettings.scm.url = result.normalizedUrl;
                $scope.project.newSettings.scm.ticket = result.ticket;
                $scope.project.newSettings.scm.fingerprint = result.fingerprint;

                // if the repo is already accessible, save settings. otherwise, go to next step
                if(result.isAccessible) {
                    $scope.saveScmSettings();
                }
                else {
                    $scope.setProcessing(false);
                    $state.go('generic.build.project.setup.scm_access');
                }
            }, function(error) {
                $scope.setProcessing(false);
                var msg = (error.data && error.data.error) ? error.data.error : 'We were not able to connect to your URL.';
                $scope.showError(msg);
            });
    };

    // handle collab
    if($scope.project.serviceFlags.collab) {
        // if this is first time setting up project and project is configured for collab, set it up for collab automatically
        if(!$scope.project.newSettings.scm.type) {
            $scope.project.newSettings.scm.type = 'collab';
            $scope.project.newSettings.scm.url = 'Unity Collab';
            $scope.nextStep();
        }
    }
    else {
        // remove collab unless the project is already configured for collab
        $scope.repoTypes = _.reject($scope.repoTypes, {value: 'collab'});
    }
}]);
