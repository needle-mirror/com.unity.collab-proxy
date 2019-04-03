angular.module('ut.cloudbuild.setup').controller('ProjectSetupCredentialsBaseCtrl',["$stateParams", "$scope", "$state", "modals", "cloudBuildService", "previousState", function($stateParams, $scope, $state, modals, cloudBuildService, previousState) {

    // redirect to appropriate state based on platform
    var platform = $scope.target.platform;
    $state.go('generic.build.project.setup.target.credentials.'+platform, {targetId: $stateParams.targetId});

    // track previous state (for back button)
    $scope.previousState = previousState;

    // setup scope
    $scope.target.credentials.signing.credentialid = $scope.target.credentials.signing.credentialid || '';
    $scope.newCredentials = {};

    $scope.credOptions = [
        {credentialid: "", label: "Select", disabled: true},
        {credentialid: "__addnew__", label: "Add new provisioning credentials"},
        {credentialid: "__disabled__", label: "------------------", disabled: true},
    ];

    cloudBuildService.projects.listCredentials($scope.orgId, $scope.projectId, platform)
        .then(function(results) {
            // sort results alphabetically
            results = _.sortBy(results, function(credential) {
                // force debug keystore to show up first
                if(credential.keystore && credential.keystore.debug) {
                    return '_';
                }
                return credential.label.toLowerCase();
            });

            $scope.credentials = results;
            $scope.credOptions = $scope.credOptions.concat(results);

            updateSelectedCredentialInfo($scope.target.credentials.signing.credentialid);
        })
        .catch(function(error) {
            $scope.credentials = [];
            modals.error("Failed to list credentials for project!");
        });


    function updateSelectedCredentialInfo(credentialid) {
        $scope.selectedCredentialInfo = {};
        if(!credentialid || !$scope.credentials || credentialid === '__addnew__') {
            return;
        }

        $scope.selectedCredentialInfo = _.find($scope.credentials, {credentialid: credentialid}) || {};
    }

    $scope.$watch('target.credentials.signing.credentialid', function(newValue, oldValue) {
        updateSelectedCredentialInfo(newValue);
    });

    $scope.isEditing = !$stateParams.new;
    $scope.nextState = function() {
        if($scope.isEditing) {
            $state.go('generic.build.project.targets');
        }
        else {
            $scope.startBuilds($scope.target.buildtargetid, false);
        }
    };

    $scope.updateBuildTargetSettings = function(target, credentials) {
        var updates = {
            settings: {
                platform: target.settings.platform
            },
            credentials: {
                signing: {
                    credentialid: credentials.credentialid
                }
            }
        };
        return cloudBuildService.projects.updateBuildTarget($scope.orgId, $scope.projectId, $stateParams.targetId, updates);
    };
}]);
