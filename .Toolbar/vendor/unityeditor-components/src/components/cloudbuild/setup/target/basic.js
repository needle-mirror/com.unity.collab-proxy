angular.module('ut.cloudbuild.setup').controller('ProjectSetupTargetBasicCtrl',["$stateParams", "$scope", "$state", "$timeout", "modals", "setupConstants", "cloudBuildService", "buildConstants", "scmUtils", "unityVersionService", function($stateParams, $scope, $state, $timeout, modals, setupConstants, cloudBuildService, buildConstants, scmUtils, unityVersionService) {
    // make a copy of constants (so we can edit them if necessary)
    $scope.constants = _.merge({}, setupConstants);

    // figure out if we are editing or creating a new buildtarget
    var buildtargetid = $stateParams.targetId;
    if(buildtargetid) {
        $scope.isEditing = true;
    }
    else {
        $scope.isEditing = false;
        var existingUnityVersion = $scope.target && $scope.target.settings && $scope.target.settings.unityVersion;
        $scope.target = {
            'platform': '',
            'name': '',
            'enabled': true,
            'settings': {
                'autoBuild': true,
                'remoteCacheStrategy': 'library',
                'unityVersion': existingUnityVersion,
                'scm': {}
            }
        };

        if($stateParams.platform) {
            if(!$scope.target.settings.unityVersion) {
                unityVersionService.getLatestValidVersion($stateParams.platform).then(function(version){
                    $scope.target.settings.unityVersion = version;
                });
            }
            $scope.target.platform = $stateParams.platform;

            var platformMapping = _.find(setupConstants.platforms, {value:$stateParams.platform});
            var platformPretty = platformMapping ? platformMapping.name : $stateParams.platform;
            $scope.target.name = 'Default ' + platformPretty;
        }
    }

    // set default branch value for mercurial
    if($scope.project.settings.scm.type === 'hg') {
        $scope.target.settings.scm.branch = $scope.target.settings.scm.branch || 'default';
    } else {
        scmUtils.getScmBranches($scope);
    }

    $scope.gitBranchModal = function() {
        modals.gitBranchSelect($scope.project, $scope.target).result.then(function(branch) {
            $scope.target.settings.scm.branch = branch;
        });
    };

    $scope.svnFolderModal = function() {
        modals.svnFolderSelect($scope.project, $scope.target).result.then(function(path) {
            $scope.target.settings.scm.branch = path;
        });
    };

    $scope.p4ClientModal = function() {
        modals.p4ClientSelect($scope.project, $scope.target).result.then(function(client) {
            $scope.target.settings.scm.client = client;
        });
    };

    $scope.needsCredentials = function() {
        var target = $scope.target;
        if(target.platform !== 'android' && target.platform !== 'ios') {
            return false;
        }

        if($scope.isEditing) {
            return !target.credentials || !target.credentials.signing || !target.credentials.signing.credentialid;
        }
        else {
            return true;
        }
    };

    $scope.WebGlSupportCheck = function() {
        if($scope.target.platform === 'webgl' && ($scope.target.settings.unityVersion.match(/^4_/) || $scope.target.settings.unityVersion === 'latest')) {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.canContinueToNextStep = function() {
        var missingBranch = false;
        if($scope.project.settings.scm.type !== 'p4' && $scope.project.settings.scm.type !== 'collab') {
            missingBranch = !$scope.target.settings.scm.branch;
        }
        if($scope.project.settings.scm.type === 'p4') {
            if(!$scope.target.settings.scm.client) {
                return false;
            }
        }
        else if(missingBranch || !$scope.WebGlSupportCheck()) {
            return false;
        }

        return $scope.target.platform && $scope.target.name;
    };

    $scope.nextStep = function() {
        // handle saving or creating the build target
        var request = null;
        if($scope.isEditing) {
            request = cloudBuildService.projects.updateBuildTarget($scope.project.orgid, $scope.project.projectid, $scope.target.buildtargetid, $scope.target);
        }
        else {
            request = cloudBuildService.projects.createBuildTarget($scope.project.orgid, $scope.project.projectid, $scope.target);
        }

        var modal = modals.progress('Saving...');
        request
          .then(function(buildtarget) {
              $scope.updateBuildTargetInScope(buildtargetid, buildtarget);
              nextState(buildtarget);
          })
          .catch(function(error) {
              modals.error("Failed to update build target: " + error.data.error);
          })
          .finally(function() {
              modal.close();
          });
    };

    function nextState(buildtarget) {
        if($scope.needsCredentials()) {
            $state.go('generic.build.project.setup.target.credentials', {targetId: buildtarget.buildtargetid, new:true});
        }
        else if(!$scope.isEditing) {
            $scope.startBuilds(buildtarget.buildtargetid, false);
        }
        else {
            $state.go('generic.build.project.targets');
        }
    }
}]);
