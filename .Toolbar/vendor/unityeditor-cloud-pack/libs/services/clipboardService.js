angular.module('ngUnity.clipboardService', [
	'ngUnity.unityService'
])
.factory('unityClipboardService', ["$q", "unityService", function ($q, unityService){
	var service = unityService.getUnityObject('unity/ClipboardAccess');
	var readyPromise = $q.defer();

	return service;
}]);
