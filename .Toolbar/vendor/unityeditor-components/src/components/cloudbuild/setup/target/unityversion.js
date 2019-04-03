angular.module('ut.cloudbuild.setup').factory('unityVersionService', ["setupConstants", "cloudBuildService", "$q", function (setupConstants, cloudBuildService, $q) {

    var LATEST_VERSION_PATTERN = /latest(\d+)?(?:_(\d+))?(?:_(\d+))?_?(a?)(b?)/i; // DB pattern for latest versions
    var NORMAL_VERSION_PATTERN = /(\d+)(?:_(\d+))(?:_(\d+))/; // DB pattern for regular versions
    var EDITOR_VERSION_PATTERN = /(\d+)\.(\d+)\.(\d+)(?:([abf])(\d+))?/; // unityVersion as supplied by unityProjectService.editorVersion
    var USERAGENT_VERSION_PATTERN = /Unity\/(\d+)\.(\d+)\.(\d+)(a?)(b?)/i; // unityVersion as supplied by userAgent
    var MIN_MAX_VERSION_PATTERN = /(\d+)?(?:\.(\d+))?(?:\.(\d+))?(a?)(b?)/;

    var INITIAL_VERSION = 'latest0_0_0';
    var HIGH_NUM = 99;
    var LESSER = -1;
    var GREATER = 1;
    var EQUAL = 0;

    var service = {};

    // Is this unityVersion supported, given minVersion and maxVersion
    // Options e.g.: {alpha: false, beta: true}
    // unityVersion must be formatted in accordance with cleanupRegexMatch, use functions like userAgentMatch to expose this
    service.isSupported = function (unityVersion, minVersion, maxVersion, options) {
        if (!options) {
            options = {};
        }
        if (!isAlphaBetaSupported(unityVersion, options)) {
            return false;
        }
        var minVersionMatch = minVersion && cleanupRegexMatch(minVersion.match(MIN_MAX_VERSION_PATTERN), 0);
        if (minVersionMatch && compareVersions(unityVersion, minVersionMatch) === LESSER) {
            return false;
        }
        var maxVersionMatch = maxVersion && cleanupRegexMatch(maxVersion.match(MIN_MAX_VERSION_PATTERN), HIGH_NUM);
        if (maxVersionMatch && compareVersions(unityVersion, maxVersionMatch) === GREATER) {
            return false;
        }
        return true;
    };

    // Gets versions from the UCB server and uses these to determine what is the maximum supported version for this build platform
    service.getLatestValidVersion = function (platformValue, options) {
        var dfd = $q.defer();
        var platform = _.find(setupConstants.platforms, {value: platformValue});
        cloudBuildService.versions.get('unity').then(function (data) {
            var minVersion = platform.version ? platform.version.min : null;
            var maxVersion = platform.version ? platform.version.max : null;
            dfd.resolve(getLatestVersion(data, minVersion, maxVersion, options));
        });
        return dfd.promise;
    };

    // Attempts to get a version of Unity based on UCB unityVersions that is similar to the one provided.
    // First tries to find a "latest" version closely matching the current version, if it fails it will attempt to provide the exact same version the user is running.
    service.maxSimilarVersion = function (versions, unityVersion) {
        var major, minor;
        var matches = unityVersion.match(EDITOR_VERSION_PATTERN);
        if (!matches || matches.length < 4) {
            return;
        }
        major = matches[1];
        minor = matches[2];

        var options = {allowHidden: true};
        if (matches.length === 6) {
            options.alpha = matches[4] === 'a';
            options.beta = matches[4] === 'b';
        }

        var maxVersion = getMaxVersion(major, minor);
        var minVersion = major;

        var latestVersion = getLatestVersion(versions, minVersion, maxVersion, options);
        if (latestVersion) {
            return latestVersion;
        }
        var unityVersionValue = unityVersion.replace(/\./g, '_');
        var self = _.find(versions, {value: unityVersionValue});
        if (self) {
            return self.value;
        }
        // Unlikely to work but worth a try I guess?
        var self = _.find(versions, {value: unityVersion});
        if (self) {
            return self.value;
        }
    };

    // Filters out versions of Unity that are unsupported by the given build platform, mostly for use with supportedVersions filter.
    service.filterUnsupportedVersions = function (unityVersions, platformValue) {
        var options = {alpha: true, beta: true};
        var platform = _.find(setupConstants.platforms, {value: platformValue});
        if (platform && platform.version) {
            return _.filter(unityVersions, function (unityVersion) {
                var match = unityVersion.value.match(NORMAL_VERSION_PATTERN);
                if (match) {
                    match = cleanupRegexMatch(match, HIGH_NUM);
                    return service.isSupported(match, platform.version.min, platform.version.max, options);
                }
                match = unityVersion.value.match(LATEST_VERSION_PATTERN);
                if (match) {
                    match = cleanupRegexMatch(match, HIGH_NUM);
                    return service.isSupported(match, platform.version.min, platform.version.max, options);
                }
                // Can't figure out what this unityVersion represents, give up and return true
                return true;
            });
        }
        return unityVersions;
    };

    service.userAgentMatch = function (userAgentString) {
        var unityVersion = userAgentString.match(USERAGENT_VERSION_PATTERN);
        return cleanupRegexMatch(unityVersion, HIGH_NUM);
    };

    // Get the max version corresponding to this version in dot-separated string format
    function getMaxVersion(major, minor) {
        var maxVersionArray = [];
        maxVersionArray.push(major);
        if (angular.isDefined(minor)) {
            maxVersionArray.push(minor);
        }
        while (maxVersionArray.length < 3) {
            maxVersionArray.push(HIGH_NUM);
        }
        return maxVersionArray.join('.');
    }

    function cleanupRegexMatch(match, defaultValue) {
        if (match) {
            var major = match[1] || defaultValue;
            var minor = match[2] || defaultValue;
            var patch = match[3] || defaultValue;
            var alpha = match[4] === 'A';
            var beta = match[5] === 'B';
            return [major, minor, patch, alpha, beta];
        }
    }

    function getLatestVersion(versions, minVersion, maxVersion, options) {
        var latestVersions = _.filter(versions, function (version) {
            if ((!options || !options.allowHidden) && version.hidden === true) {
                return false;
            }
            return version.value.match(LATEST_VERSION_PATTERN);
        });
        var highestValidVersion = INITIAL_VERSION;
        latestVersions.forEach(function (version) {
            var versionNumbers = cleanupRegexMatch(version.value.match(LATEST_VERSION_PATTERN), HIGH_NUM);
            if (!service.isSupported(versionNumbers, minVersion, maxVersion, options)) {
                return;
            }
            var latestVersionNumbers = cleanupRegexMatch(highestValidVersion.match(LATEST_VERSION_PATTERN), HIGH_NUM);
            if (compareVersions(versionNumbers, latestVersionNumbers, true) === GREATER) {
                highestValidVersion = version.value;
            }
        });
        if (highestValidVersion != INITIAL_VERSION) {
            return highestValidVersion;
        }
    }

    // Compare a dot-separated compareVersion with a regex-based unityVersion
    function compareVersions(left, right, compareAlphaBeta) {
        if (!right) {
            return GREATER;
        }
        if (!left) {
            return LESSER;
        }
        for (var i = 0; i < 3; i++) {
            if (Number(left[i]) > Number(right[i])) {
                return GREATER;
            } else if (Number(left[i]) < Number(right[i])) {
                return LESSER;
            }
        }
        if(!compareAlphaBeta){
            return EQUAL;
        }
        return compareAlphaBetaVersions(left, right);
    }

    function compareAlphaBetaVersions(left, right) {
        if (right[4] && !left[4]) {
            return LESSER;
        } else if (left[4] && !right[4]){
            return GREATER;
        }
        if (right[3] && !left[3]) {
            return LESSER;
        } else if(left[3] && !right[3]){
            return GREATER;
        }
        return EQUAL;
    }

    function isAlphaBetaSupported(unityVersion, options) {
        if (unityVersion[3]) {
            if (unityVersion[4]) {
                // AB version must have A or B supported
                return options.alpha || options.beta;
            }
            // A version must have A supported
            return options.alpha;
        }
        // B version must have B supported
        return !unityVersion[4] || options.beta;
    }

    return service;
}]).filter('supportedVersions', ["unityVersionService", function (unityVersionService) {
    return unityVersionService.filterUnsupportedVersions;
}]);