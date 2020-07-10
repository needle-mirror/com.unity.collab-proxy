angular.module('ut.cloudbuild.history', ['ut.cloudbuild.constants'])
    .filter('platformIcon', ["buildConstants", function(buildConstants) {
        return function (platform) {
            var foundPlatform = _.find(buildConstants.platforms, {value: platform});
            if(foundPlatform) {
                return foundPlatform.icon;
            }
            else {
                return "unityicon unityicon-os-globe";
            }
        }
    }])
    .filter('platformName', ["buildConstants", function(buildConstants) {
        return function(platform) {
            return buildConstants.platformName(platform);
        };
    }])
    .filter('statusLabel', ["buildConstants", function(buildConstants) {
        return function(status) {
            var foundStatus = _.find(buildConstants.status, {value: status});
            if (foundStatus) {
                return foundStatus.name;
            }
            else {
                return "Queued";
            }
        }
    }])
    .filter('statusClass', ["buildConstants", function(buildConstants) {
        return function(status) {
            var foundStatus = _.find(buildConstants.status, {value: status});
            if (foundStatus) {
                return foundStatus.class;
            }
            else {
                return "queued";
            }
        }
    }])
    .filter('isFinishedBuilding', ["buildConstants", function(buildConstants) {
        return function(status) {
            var foundStatus = _.find(buildConstants.status, {value: status});
            if (foundStatus) {
                return foundStatus.class !== 'building' && foundStatus.class !== 'queued';
            }
            return true;
        }
    }]);