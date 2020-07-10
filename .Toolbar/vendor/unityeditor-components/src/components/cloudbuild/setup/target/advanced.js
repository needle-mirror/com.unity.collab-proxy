angular.module('ut.cloudbuild.setup').controller('ProjectSetupTargetAdvancedCtrl',["$stateParams", "$scope", "$state", "$timeout", "modals", "setupConstants", "cloudBuildService", "ENV", function($stateParams, $scope, $state, $timeout, modals, setupConstants, cloudBuildService, ENV) {
    // make a copy of constants (so we can edit them if necessary)
    $scope.constants = _.merge({}, setupConstants);

    $scope.ENV = ENV;
    $scope.isEditing = true;
    $scope.form = {};

    // copied from build-service -> common.getObjectValueAtPath()
    $scope.getValueFromPath = function(config, path, defaultVal) {
        var returnDefault = false;
        return path.split(".").reduce(
            function(o, x) {
                if(!returnDefault && o.hasOwnProperty(x)) {
                    $scope.hasCustomConfig = true;
                    return o[x];
                }
                else {
                    returnDefault = true;
                    return defaultVal;
                }
            }, config);
    };

    $scope.saveBuildTarget = function(params) {
        // update build target
        var buildtargetid = $scope.target.buildtargetid;
        var request = cloudBuildService.projects.updateBuildTarget($scope.project.orgid, $scope.project.projectid, buildtargetid, params);
        var modal = modals.progress('Saving...');
        modal
          .opened
          .then(function() {
            return request;
          })
          .then(function(buildtarget) {
              $scope.updateBuildTargetInScope(buildtargetid, buildtarget);
              $state.go('generic.build.project.targets');
          })
          .catch(function(error) {
              modals.error("Failed to update build target: " + error.data.error);
          })
          .finally(modal.close);
    };

    // parse options from basic settings
    $scope.executableName = $scope.target.settings.executablename;

    // parse options from advanced settings
    var advanced = $scope.target.settings.advanced || {};
    $scope.preExportMethod = $scope.getValueFromPath(advanced, 'unity.preExportMethod', '');
    $scope.postExportMethod = $scope.getValueFromPath(advanced, 'unity.postExportMethod', '');
    $scope.preBuildScript = $scope.getValueFromPath(advanced, 'unity.preBuildScript', '');
    $scope.postBuildScript = $scope.getValueFromPath(advanced, 'unity.postBuildScript', '');
    $scope.scenes = $scope.getValueFromPath(advanced, 'unity.playerExporter.sceneList', []);

    $scope.buildOptions = $scope.getValueFromPath(advanced, 'unity.playerExporter.buildOptions', []);
    $scope.buildForDebug = ($scope.buildOptions.indexOf('Development') !== -1);
    $scope.splitBinaryBuild = $scope.getValueFromPath(advanced, 'unity.playerSettings.Android.useAPKExpansionFiles', false);
    $scope.hadSplitBinaryBuild = $scope.splitBinaryBuild;

    var definesString = $scope.getValueFromPath(advanced, 'unity.scriptingDefineSymbols', '');
    var definesArray = (definesString.length > 0) ? definesString.split(';') : [];
    $scope.customDefines = [];
    for(var i=0; i<definesArray.length; i++) {
        var define = definesArray[i].trim();
        if(define && $scope.customDefines.indexOf(define) === -1) {
            $scope.customDefines.push(define);
        }
    }

    // update buildOptions whenever buildForDebug changes
    $scope.$watch('buildForDebug', function(newValue, oldValue) {
        if(newValue) {
            if($scope.buildOptions.indexOf('Development') === -1) {
                $scope.buildOptions.push('Development');
                $scope.buildOptions.push('AllowDebugging');
            }
        }
        else {
            var id = $scope.buildOptions.indexOf('Development');
            if(id !== -1) {
                $scope.buildOptions.splice(id, 1);
            }

            var ia = $scope.buildOptions.indexOf('AllowDebugging');
            if(ia !== -1) {
                $scope.buildOptions.splice(ia, 1);
            }
        }
    });

    $scope.addCustomDefine = function() {
        // strip out semicolon (since we use this to join the defines)
        var toAdd = $scope.customDefinesText || '';
        toAdd = toAdd.replace(/;/g, '');
        toAdd = toAdd.trim();
        if(toAdd && $scope.customDefines.indexOf(toAdd) === -1) {
            $scope.customDefines.push(toAdd);
        }
        $scope.customDefinesText = '';
    };

    $scope.removeCustomDefine = function(idx) {
        $scope.customDefines.splice(idx, 1);
    };

    $scope.addScene = function() {
        // get scene and strip off beginning slash if necessary
        var toAdd = $scope.scenesText || '';
        if(toAdd.substr(0,1) === '/') {
            toAdd = toAdd.substr(1, toAdd.length-1);
        }
        toAdd = toAdd.trim();

        if(!toAdd) {
            return;
        }

        // prepend /Assets to the path
        if(toAdd.substr(0, 7) !== 'Assets/') {
            toAdd = 'Assets/' + toAdd;
        }

        // append .unity to the path
        if(toAdd.indexOf('.unity') === -1) {
            toAdd = toAdd + '.unity';
        }

        if(toAdd && $scope.scenes.indexOf(toAdd) === -1) {
            $scope.scenes.push(toAdd);
        }
        $scope.scenesText = '';
    };

    $scope.removeScene = function(idx) {
        $scope.scenes.splice(idx, 1);
    };

    $scope.canContinueToNextStep = function() {
        return true;
    };

    $scope.nextStep = function() {
        // convert custom defines to semi-colon separated string
        var defines = $scope.customDefines.join('; ');

        // build params
        var params = {
            'settings' : {
                'advanced': {
                    'unity': {
                        'preExportMethod': $scope.preExportMethod,
                        'postExportMethod': $scope.postExportMethod,
                        'preBuildScript': $scope.preBuildScript,
                        'postBuildScript': $scope.postBuildScript,
                        'scriptingDefineSymbols': defines,
                        'playerExporter': {
                            'sceneList': $scope.scenes,
                            'buildOptions': $scope.buildOptions
                        }
                    }
                }
            }
        };

        if($scope.executableName) {
            params.settings.executablename = $scope.executableName;
        }

        if($scope.splitBinaryBuild || $scope.hadSplitBinaryBuild) {
            params.settings.advanced.unity.playerSettings = {
                'Android' : {
                    'useAPKExpansionFiles': $scope.splitBinaryBuild
                }
            };
        }

        $scope.saveBuildTarget(params);
    };
}]);
