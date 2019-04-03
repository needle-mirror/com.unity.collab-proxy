angular.module('ut.cloudbuild.setup').controller('ProjectSetupTargetTestsCtrl',["$stateParams", "$scope", "$controller", function($stateParams, $scope, $controller) {
    // extend from advanced settings controler
    $controller('ProjectSetupTargetAdvancedCtrl', {$scope: $scope });

    // parse options from advanced settings
    var settings = $scope.target.settings.advanced || {};

    $scope.runUnitTests = $scope.getValueFromPath(settings, 'unity.runUnitTests', false);
    $scope.unitTestMethod = $scope.getValueFromPath(settings, 'unity.unitTestMethod', 'UnityTest.Batch.RunUnitTests');
    $scope.failedUnitTestFailsBuild = $scope.getValueFromPath(settings, 'unity.failedUnitTestFailsBuild', false);

    $scope.nextStep = function() {

        var params = {
            settings : {
                advanced: {
                    unity: {
                        runUnitTests : $scope.runUnitTests,
                        unitTestMethod: $scope.unitTestMethod,
                        failedUnitTestFailsBuild: $scope.failedUnitTestFailsBuild
                    }
                }
            }
        };

        $scope.saveBuildTarget(params);
    };
}]);