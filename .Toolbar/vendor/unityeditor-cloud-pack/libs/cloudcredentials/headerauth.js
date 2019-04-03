angular.module('ngUnity.cloudcredentials')
    .factory('authInterceptor', ["unityConnectService", "unityProjectService", function(unityConnectService, unityProjectService){
        var authInterceptor = {
            request: function(config) {
                var isUrlCloudCore = _.some(unityConnectService.urls, function(domainUrl){
                    return _.startsWith(config.url, domainUrl);
                });

                if(isUrlCloudCore) {
                    var authorization = {
                        'AUTHORIZATION': 'Bearer ' + unityConnectService.userInfo.accessToken,
                        'Content-Type': 'application/json',
                        'X-UNITY-VERSION': unityProjectService.editorVersion
                    };

                    // Only overwrite if there was no authorization
                    config.headers = _.extend(authorization, config.headers);
                }

                return config;
            }
        };

        return authInterceptor;
    }])
    .config(['$httpProvider', function($httpProvider) {
        $httpProvider.interceptors.push('authInterceptor');
    }]);