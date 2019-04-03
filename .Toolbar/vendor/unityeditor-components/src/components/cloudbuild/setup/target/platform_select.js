angular.module('ut.cloudbuild.setup')
  .value('userAgent', navigator.userAgent)
  .controller('ProjectSetupPlatformSelectCtrl', ["$stateParams", "$scope", "setupConstants", "userAgent", "unityVersionService", function ($stateParams, $scope, setupConstants, userAgent, unityVersionService) {

      var unityVersion = unityVersionService.userAgentMatch(userAgent);
      $scope.platforms = _.filter(setupConstants.platforms, function (val) {
          if (unityVersion && val.version && !unityVersionService.isSupported(unityVersion, val.version.min, val.version.max, {alpha: true, beta: true})) {
              return false;
          }
          return !val.hidden;
      });
  }]);