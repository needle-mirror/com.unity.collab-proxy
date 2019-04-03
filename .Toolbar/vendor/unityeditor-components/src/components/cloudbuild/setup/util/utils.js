angular.module('ut.cloudbuild.setup').provider('utilsService', function utilsServiceProvider() {
    this.$get = ["$q", "$timeout", function utilsServiceFactory($q, $timeout) {
        var utils = {};

        // wait for a value to be loaded into scope (with specified interval)
        utils.waitForScopeValueToLoad = function(scope, valueKey, interval) {
            var deferred = $q.defer();
            waitForScopeValueToLoadInternal(scope, valueKey, interval || 500, deferred);
            return deferred.promise;
        };

        function waitForScopeValueToLoadInternal(scope, valueKey, interval, deferred) {
            if (scope[valueKey]) {
                deferred.resolve();
            }
            else {
                var wait = waitForScopeValueToLoadInternal.bind(null, scope, valueKey, interval, deferred);
                $timeout(wait, interval);

            }
        }

        return utils;
    }];
}).directive('utTooltipToggle', function utTooltipToggle() {
    return {
        restrict: 'E',
        replace: true,
        template: '<a class="tooltip-toggle">?</a>',
        link: function(scope, elem, attrs) {
            elem.bind('click', function () {
                $(this).parents('.form-group').find('.tooltip').toggleClass('tooltip-visible');
            });
        }
    };
});