angular.module('ngUnity.cloudbuild', [
	'ngUnity.cloudcore',
	'ut.components',
	'ngUnity.general'
])

.factory('cloudbuild', ["$q", "$http", "cloudcore", "unityService", function ($q, $http, cloudcore, unityService) {
	var service = {};

	service.webUrl = 'https://build.cloud.unity3d.com';
	var buildConnect = unityService.getUnityObject('unity/project/cloud/build');

	service.ShowServicePage = function() {
		buildConnect.ShowServicePage();
	};

	service.ShowBuildForCommit = function(commitId) {
		buildConnect.ShowBuildForCommit(commitId);
	};

	service.IsReady = function (){
		var deferred = $q.defer();
		cloudcore.getConfigEntry('build').then(function (res) {
			if (res !== undefined){
				service.webUrl = res;
			} else {
				console.log('Unable to find a config entry for "build"');
			}
			deferred.resolve(true);
		});
		return deferred.promise;
	};

	service.getUCBUrl = function (){
		return service.webUrl;
	};

	return service;
}]);
