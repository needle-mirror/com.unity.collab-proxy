angular.module('ut.cloudbuild.setup', [
    'ut.services.cloudbuild',
    'ut.cloudbuild.constants',
    'ui.router',
    'ngSanitize',
    'ut.alert'
])
    .config(["$stateProvider", function($stateProvider) {
        $stateProvider
            .state('generic.build.project.setup',{
                meta: { title: 'Project Setup', linkable: false },
                url: '/setup',
                abstract: true,
                templateUrl: 'setup.html',
                controller: 'ProjectSetupCtrl'
            })
            .state('generic.build.project.setup.basic', {
                url: '/basic/',
                templateUrl: 'project/basic.html',
                controller: 'ProjectSetupBasicCtrl'
            })
            .state('generic.build.project.setup.start', {
                meta: { title: 'Start'},
                url: '/start/',
                templateUrl: 'start.html'
            })
            .state('generic.build.project.setup.scm', {
                meta: { title: 'Source Code Location', 'step': 1 },
                url: '/scm/',
                templateUrl: 'scm/step01.html',
                controller: 'ProjectSetupStep01Ctrl'
            })
            .state('generic.build.project.setup.scm_access', {
                meta: { title: 'Source Code Access', 'step': 2 },
                url: '/scm/access/',
                templateUrl: 'scm/step02.html',
                controller: 'ProjectSetupStep02Ctrl'
            })
            .state('generic.build.project.setup.platform_select', {
                meta: { title: 'Platform Selection', 'step': 3 },
                url: '/platform/',
                templateUrl: 'target/platform_select.html',
                controller: 'ProjectSetupPlatformSelectCtrl'
            })
            .state('generic.build.project.setup.new_target', {
                meta: { title: 'Build Target Setup', 'step': 4 },
                url: '/newtarget/?platform',
                templateUrl: 'target/basic.html',
                controller: 'ProjectSetupTargetBasicCtrl'
            })
            .state('generic.build.project.setup.target', {
                meta: { title: 'Build Target Setup', 'step': 4},
                url: '/buildtarget/:targetId',
                abstract: true,
                templateUrl: 'target/target_edit.html',
                controller: 'ProjectSetupTargetEditCtrl'
            })
            .state('generic.build.project.setup.target.basic', {
                meta: { title: 'Basic Info', 'step': 4 },
                url: '/basic/',
                templateUrl: 'target/basic.html',
                controller: 'ProjectSetupTargetBasicCtrl'
            })
            .state('generic.build.project.setup.target.advanced', {
                meta: { title: 'Advanced Info', 'step': 4 },
                url: '/advanced/',
                templateUrl: 'target/advanced.html',
                controller: 'ProjectSetupTargetAdvancedCtrl'
            })
            .state('generic.build.project.setup.target.asset_bundles', {
                url: '/assetbundles/',
                templateUrl: 'target/assetbundles.html',
                controller: 'ProjectSetupTargetAssetBundlesCtrl'
            })
            .state('generic.build.project.setup.target.tests', {
                url: '/tests/',
                templateUrl: 'target/tests.html',
                controller: 'ProjectSetupTargetTestsCtrl'
            })
            .state('generic.build.project.setup.target.credentials', {
                meta: { title: 'Credentials', 'step': 5 },
                url: '/credentials/?new',
                template: '<div ui-view></div>',
                controller: 'ProjectSetupCredentialsBaseCtrl',
                resolve: {
                    previousState: ["$state", "$stateParams", function ($state, $stateParams) {
                        return resolvePreviousState($state, $stateParams, 'generic.build.project.setup.target.basic');
                    }]
                }
            })
            .state('generic.build.project.setup.target.credentials.ios', {
                meta: { title: 'iOS' },
                url: 'ios/',
                templateUrl: 'credentials/credentials_ios.html',
                controller: 'ProjectSetupCredentialsIosCtrl'
            })
            .state('generic.build.project.setup.target.credentials.android', {
                meta: { title: 'Android' },
                url: 'android/',
                templateUrl: 'credentials/credentials_android.html',
                controller: 'ProjectSetupCredentialsAndroidCtrl'
            });

        function resolvePreviousState($state, $stateParams, defaultState) {
            if($state.current.name) {
                return {
                    name: $state.current.name,
                    params: $state.params,
                    url: $state.href($state.current.name, $state.params)
                };
            }
            else {
                return {
                    name: defaultState,
                    params: $stateParams,
                    url: $state.href(defaultState, $stateParams)
                };
            }
        }
    }])
    .controller('ProjectSetupCtrl', ["$state", "$stateParams", "$scope", "utAlert", "modals", "cloudBuildService", function($state, $stateParams, $scope, utAlert, modals, cloudBuildService) {
        $scope.ui = $scope.ui || {};
        $scope.ui.pageName = 'Setup';

        // error handling
        $scope.showError = function(error, details) {
            console.log(error);
            var message = error.message || error.data || error;
            utAlert.setWarning(message);
        };

        $scope.clearError = function() {
            $scope.error = null;
        };

        $scope.updateBuildTargetInScope = function(buildtargetid, updatedTarget) {
            if(!buildtargetid) {
                $scope.buildTargets.push(updatedTarget);
                return;
            }

            var index = _.findIndex($scope.buildTargets, {buildtargetid: buildtargetid});
            $scope.buildTargets[index] = updatedTarget;
        };

        $scope.needsProjectSetup = function() {
            return cloudBuildService.needsProjectSetup($scope.project);
        };

        // track if we are editing or setting up for the first time
        $scope.isEditing = !$scope.needsProjectSetup();


        $scope.startBuilds = function(targetId, clean){
            var modal = modals.progress('Starting builds...');
            cloudBuildService.projects.startBuilds($scope.orgId, $scope.projectId, targetId, clean)
                .then(function() {})
                .catch(function(error) {
                    var message = 'Unable to build project';
                    if(error && error.data && error.data.error) {
                        message = error.data.error;
                    }
                    modals.error(message);
                })
                .finally(function() {
                    // go to history even on error (target should be set up at this point anyway)
                    $scope.$broadcast('time_to_poll');
                    $scope.goToHistory();
                    modal.close();
                });
        };

        // processing
        $scope.isProcessing = false;
        $scope.setProcessing = function(newVal) {
            $scope.isProcessing = newVal;
        };
    }]);
