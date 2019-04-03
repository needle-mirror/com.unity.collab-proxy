angular.module('ngUnity', [
  'templates-app',
  'templates-common',
  'ngAnimate',
  'ngSanitize',
  'ngUnity.filters',
  'ngUnity.cloudcredentials',
  'ngUnity.directives',
  'ngUnity.editorcollab',
  'ngUnity.connectService',
  'ngPanel.toolbar',
  'ngPanel.nointernet',
  'ngPanel.nolocalhost',
  'ngPanel.loggedout',
  'ngPanel.refreshproject',
  'ngPanel.noproject',
  'ngPanel.noseat',
  'ngUnity.projectService',
  'ngPanel.toolbar.progress',
  'ui.router',
  'ut.components',
  'ut.services.notification',
  'ngStorage',
  'ngPanel.common',
  'angular-svg-round-progress',
  'ngAria',
  'ngMaterial',
  'angulartics',
  'angulartics.google.analytics'
])
.config(["$urlRouterProvider", function ($urlRouterProvider) {
  // Redirect for any unmatched url
  $urlRouterProvider.otherwise(function() {
    return '/';
  });
}])
.run(["$rootScope", "$state", function($rootScope, $state) {
  $rootScope.$on('$stateChangeError', function(event, toState, toParams, fromState, fromParams, error){
    if(error && error.redirect) {
      if(error.newState === "nointernet" || error.newState === "noproject") {
        $rootScope.hideFooter = true;
      } else {
        $rootScope.hideFooter = false;
      }
      $state.go(error.newState);
      return;
    }
    console.error("Error changing state, detail: event:", event, "toState:", toState, "toParams:",
          toParams, "fromState:", fromState, "fromParams:", fromParams, "error:", error);
  });
}])
.controller('AppCtrl', ["$rootScope", "$scope", "$q", "$window", "$state", "$timeout", "$analytics", "errorHandlingInterceptor", "unityService", "unityProjectService", "notifications", "editorcollab", "cloudcollab", "unityConnectService", "cloudcoreUi", "utAlert", "ToolbarStateService", function AppCtrl($rootScope, $scope, $q, $window, $state, $timeout, $analytics, errorHandlingInterceptor, unityService, unityProjectService, notifications, editorcollab, cloudcollab, unityConnectService, cloudcoreUi, utAlert, ToolbarStateService) {
  var collabConnect = unityService.getUnityObject('unity/project/cloud/collab');
  var membersLink = {
    "development": "https://dev-developer.cloud.unity3d.com/orgs/%%ORGID%%/projects/%%UPID%%/users",
    "staging": "https://staging-developer.cloud.unity3d.com/orgs/%%ORGID%%/projects/%%UPID%%/users",
    "production": "https://developer.cloud.unity3d.com/orgs/%%ORGID%%/projects/%%UPID%%/users"
  };

  // Set up notifications
  notifications.init(notifications.backends.SOCKET_BACKEND, editorcollab.refreshSocketAccess);

  // Set up seat error handling
  editorcollab.IsReady().then(function () {
    errorHandlingInterceptor.onSeatUpdated($scope, editorcollab.SetSeatAvailable);
  });

  // Set up generic error handling
  errorHandlingInterceptor.onHttpError($scope, function(detail) {
      utAlert.setError(detail);
  });

  // Set up GA custom dimensions
  unityProjectService.IsReady().then(function() {
    $q.all([
      unityProjectService.GetEnvironment(),
      unityProjectService.GetProjectEditorVersion()
    ]).then(function(results) {
      $window.ga('set', 'dimension2', results[0]);
      $window.ga('set', 'dimension3', results[1]);
    });
  });

  $scope.onGoToHub = function () {
    unityConnectService.GoToHub('');
  };

  $scope.onGoToMembers = function () {
    var env = unityConnectService.configuration;
    var link = membersLink[env];
    link = link.replace(/%%UPID%%/g, unityConnectService.projectInfo.projectGUID);
    link = link.replace(/%%ORGID%%/g, unityConnectService.projectInfo.organizationId);

    unityConnectService.OpenAuthorizedURLInWebBrowser(link);
  };

  $scope.onEnableCollab = function () {
    editorcollab.enableCollab();
    if (!unityConnectService.projectInfo.projectBound) {
      cloudcoreUi.GetOrganizations(true).success(function (organizations) {
      });
    }
  };

  $scope.onGoToHistory = function () {
    unityProjectService.GoToHistory();
  };

  $scope.onGoToSettings = function () {
    collabConnect.ShowServicePage();
  };

  function clearError() {
    // No need to clear utAlert here, since this will clear the error, which will end up causing another broadcast with error code 0
    editorcollab.clearErrorUI();
  }

  $rootScope.$on('showWebView', function(event, visibile) {
    var hasHideOnCloseError = editorcollab.currentError &&
                                              editorcollab.currentError.behaviour === editorcollab.Errors.Behaviour.HideOnClose;

    if (!visibile && hasHideOnCloseError) {
      clearError();
    }
  });

  var alertId;
  editorcollab.onErrorChanged($scope, function(error) {
    if (error.code !== editorcollab.Errors.Codes.None) {
      // Was there already an error not from this subsystem?
      var alreadyHasError = utAlert.alertText && utAlert.id !== alertId;
      var shouldSkipError = error.behaviour === editorcollab.Errors.Behaviour.Hidden ||
                                        error.behaviour === editorcollab.Errors.Behaviour.ConsoleOnly;
      var shouldDisplayError = !alreadyHasError && !shouldSkipError;

      if (shouldDisplayError) {
        // Report the error in analytics
        $analytics.eventTrack('Error', {
          category: 'Toolbar', label: error.codeStr
        });

        utAlert.cancelAutoClear();

        // Set the error in the UI
        alertId = utAlert.set({
          type: editorcollab.errorPriorityToAlert(error.priority),
          code: error.code,
          text: error.msg,
          autoClear: error.behaviour === editorcollab.Errors.Behaviour.Automatic,
          closeable: error.behaviour !== editorcollab.Errors.Behaviour.Uncloseable,
          options: {clearHandler: clearError}
        }).id;
      }
    } else {
      alertId = utAlert.clearAlertById(alertId);
    }
  });

  ToolbarStateService.registerCallbacks($rootScope);
  notifications.subscribe(notifications.channels.COLLAB_REVISIONS_UPDATE_CHANNEL, ToolbarStateService.refreshState.bind(this, {forceTransition: true}));
  notifications.subscribe(notifications.channels.CLOUD_CONNECT_INFO_CHANNEL, function(connectInfo) {
      unityConnectService.connectInfo = connectInfo;
      ToolbarStateService.refreshState();
  });

  ToolbarStateService.refreshState();
}]);
