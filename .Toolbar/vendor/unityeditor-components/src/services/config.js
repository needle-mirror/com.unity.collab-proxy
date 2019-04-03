/*
Each site that uses this module is required to define a decorator version that
overrides the factory version.

e.g. if you have a global variable named `ucCoreConfig` that contains a hash
of all the config keys, here is a simple implementation:

    angular.module('ut.unityeditor-components.cloud-config')
      .decorator('coreConfigService', function ($delegate, $q) {
        $delegate.getConfigUrl = function (key) {
          return $q.when(ucCoreConfig[key]);
        };
        $delegate.getConfigUrls = function () {
          return $q.when(ucCoreConfig);
        };
        return $delegate;
    });

Then make sure you compile the decorator code after including this module. The
.factory() version has to come first.

Then in your code (after you've included the module and injected coreConfigService)
here's an example usage:

    coreConfigService.getConfigUrl('collab_service_url').then(function (value) {
      console.log('collab_service_url:', value);
    });
*/
angular.module('ut.unityeditor-components.cloud-config', [])
  .factory('coreConfigService', ["$q", function coreConfigService($q) {
    return {
      getConfigUrl: function (key) {
        return $q.when("");
      },
      getConfigUrls: function () {
        return $q.when({});
      }
    };
  }]);
