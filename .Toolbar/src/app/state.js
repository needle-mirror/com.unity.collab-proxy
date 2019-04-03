angular.module('ngUnity')
.service('ToolbarStateService', ["$state", "$q", "$http", "editorcollab", "cloudcoreUi", "unityCloudPanelService", "unityConnectService", "unityProjectService", function($state, $q, $http, editorcollab, cloudcoreUi, unityCloudPanelService, unityConnectService, unityProjectService) {
    var transitioning;
    var localHealthCheckPath = '/unity/service/collab/error';
    var timeoutConfig = {
        timeout: 3000 // milliseconds
    };

    var transitionStart = function(event, toState, toParams, fromState, fromParams) {
        transitioning = toState;
    };

    var transitionEnd = function(event, toState, toParams, fromState, fromParams) {
        transitioning = undefined;
    };

    var localConnectionStatus = function() {
        var deferred  = $q.defer();
        unityProjectService.IsReady().then(function() {
            unityProjectService.GetRESTServiceURI().then(function (restServiceUrl) {
                $http.get(restServiceUrl + localHealthCheckPath, timeoutConfig).then(
                    function success(response) {
                        deferred.resolve(true);
                    },
                    function error(response) {
                        if (response.data == null) {
                            // Known indicator of a failing local connection
                            deferred.resolve(false);
                        } else {
                            deferred.resolve(true);
                        }
                    }
                );
            });
        });
        return deferred.promise;
    };

    var goToCurrentState = function() {
        cloudcoreUi.CheckTeamSeats(false).then(function (result) {
            editorcollab.SetSeatAvailable({seat: result});
            $q.all([localConnectionStatus(), unityCloudPanelService.IsReady(), result]).then(goToCalculatedState);
        }, function() {
            editorcollab.SetSeatAvailable({seat: false});
            $q.all([localConnectionStatus(), unityCloudPanelService.IsReady(), false]).then(goToCalculatedState);
        });
    };

    var goToCalculatedState = function(promiseResults) {
        var target;

        if (!unityConnectService.connectInfo.online) {
            target = 'nointernet';
        } else if (!promiseResults[0]) {
            target = 'nolocalhost';
        } else if (!unityConnectService.connectInfo.loggedIn) {
            target = 'loggedout';
        } else if (!unityConnectService.projectInfo.projectBound) {
            if(unityConnectService.projectInfo.projectGUID) {
                target = 'refreshproject';
            } else {
                target = 'noproject';
            }
        } else if(!promiseResults[2]) {
            target = 'noseat';
        } else if (!unityCloudPanelService.panelInfo.enabled) {
            target = 'noproject';
        } else {
            var prefix = 'project.';
            var state = editorcollab.getCollabState();
            if (state === 'noproject' || state === 'noseat' || state === 'nointernet' || state === 'nolocalhost') {
                prefix = '';
            }

            target = prefix + state;
        }

        var isCurrentSame = $state.current.name === target;
        var isTransitioningTo = transitioning && transitioning.name === target;
        var shouldTransition = !isCurrentSame && !isTransitioningTo;

        if (shouldTransition || (event && event.forceTransition)) {
            if(isCurrentSame) {
                $state.reload();
            } else {
                $state.go(target);
            }
        }
    };

    this.registerCallbacks = function(scope) {
        scope.$on('$stateChangeStart', transitionStart);
        scope.$on('$stateChangeSuccess', transitionEnd);
        scope.$on('$stateChangeError', transitionEnd);
        scope.$on('$stateNotFound', transitionEnd);
        scope.$on('connectInfo.online', goToCurrentState);
        scope.$on('projectInfo.projectBound', goToCurrentState);
        scope.$on('panelInfo.enabled', goToCurrentState);
        scope.$on('stateChanged', goToCurrentState);
        scope.$on('refreshState', goToCurrentState);
    };

    this.refreshState = goToCurrentState;
}]);