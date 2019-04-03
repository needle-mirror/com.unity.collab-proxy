angular.module('ut.cloudbuild.setup').controller('ProjectSetupTargetAssetBundlesCtrl', ["$scope", "$controller", function($scope, $controller) {
    // Extend from advanced settings controller
    $controller('ProjectSetupTargetAdvancedCtrl', {$scope: $scope });

    var advanced = $scope.target.settings.advanced || {};
    $scope.buildAssetBundles = $scope.getValueFromPath(advanced, "unity.assetBundles.buildBundles", false);
    $scope.bundlesBasePath = $scope.getValueFromPath(advanced, "unity.assetBundles.basePath", "");
    $scope.copyToStreamingAssets = $scope.getValueFromPath(advanced, "unity.assetBundles.copyToStreamingAssets", false);
    var buildAssetBundleOptions = $scope.getValueFromPath(advanced, "unity.assetBundles.buildAssetBundleOptions", "");

    $scope.compression = '';
    $scope.buildAssetBundleOptions = {};
    _.each(buildAssetBundleOptions.split(','), function(value) {
      value = value.trim();
      if (value === 'ChunkBasedCompression' || value === 'UncompressedAssetBundle') {
          $scope.compression = value;
          return;
      }
      $scope.buildAssetBundleOptions[value] = true;
    });

    $scope.copyBundlePatterns = _.uniq($scope.getValueFromPath(advanced, "unity.assetBundles.copyBundlePatterns", []));

    $scope.addFilePattern = function() {
        var toAdd = $scope.copyFilePatternText || '';

        // a little sanitation
        toAdd.replace(/[\n]+/g, '');
        if($scope.copyBundlePatterns.indexOf(toAdd) < 0) {
            $scope.copyBundlePatterns.push(toAdd);
        }

        $scope.copyFilePatternText = '';
    };

    $scope.removeFilePattern = function(item) {
        var index = $scope.copyBundlePatterns.indexOf(item);
        $scope.copyBundlePatterns.splice(index, 1);
    };

    $scope.nextStep = function() {
        var basePath = $scope.bundlesBasePath || "";

        // Ensure path is relative
        basePath = basePath.replace(/^\/+/, '');


        // keys get marked as false when they're disabled, remove those
        $scope.buildAssetBundleOptions = _.omit($scope.buildAssetBundleOptions, function(value, key) { return !value || _.isEmpty(key)});

        var buildAssetBundleOptions = Object.keys($scope.buildAssetBundleOptions);
        if ($scope.compression) {
          buildAssetBundleOptions.push($scope.compression);
        }

        var params = {
            settings: {
                advanced: {
                    unity: {
                        assetBundles: {
                            buildBundles: $scope.buildAssetBundles,
                            copyToStreamingAssets: $scope.copyToStreamingAssets,
                            copyBundlePatterns: $scope.copyBundlePatterns,
                            buildAssetBundleOptions: buildAssetBundleOptions.join(','),
                            basePath: basePath
                        }
                    }
                }
            }
        };

        $scope.saveBuildTarget(params);
    };
}]);