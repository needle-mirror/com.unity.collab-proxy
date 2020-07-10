/**************************
Service: projectUnityVersion

"Determine if the Project's Unity Version has changed"

******************/

angular.module('ngUnity.projectUnityVersion', [
  'ngUnity.projectService'
])
.factory('projectUnityVersion', ["$q", "unityProjectService", function($q, unityProjectService) {

  service = {};

  service.hasProjectVersionTxt = function (assets) {
    return _.some(assets, function(filePath) {
      return filePath.path == 'ProjectSettings/ProjectVersion.txt';
    });
  };

  service.checkProjectVersion = function (params) {
    var assets = params.assets,
      projectVersionChanged;

    if (service.hasProjectVersionTxt(assets)) {
      projectVersionChanged = true;
    } else {
      projectVersionChanged = false;
    }

    return $q.when({projectVersionChanged: projectVersionChanged});
  };

  return service;
}]);
