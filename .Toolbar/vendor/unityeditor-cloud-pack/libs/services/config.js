angular.module('ut.unityeditor-components.cloud-config').decorator('coreConfigService', ["$delegate", "$q", "unityCloudPanelService", function ($delegate, $q, unityCloudPanelService) {

    $delegate.getConfigUrl = function (key) {
        return unityCloudPanelService.promise.then(function() {
          return $q.when(unityCloudPanelService.getEnvLink(key));
        });
    };
    $delegate.getConfigUrls = function () {
        // we don't support this currently
        return $q.when(unityCloudPanelService.getEnvLinks());
    };
    return $delegate;
}]);
