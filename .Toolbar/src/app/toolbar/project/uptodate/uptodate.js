angular.module('ngPanel.toolbar.uptodate', [
	'ngUnity.editorcollab',
	'ui.router'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('project.uptodate', {
		url: 'uptodate',
		templateUrl: 'toolbar/project/uptodate/uptodate.tpl.html',
		controller: 'UpToDateCtrl',
		resolve: {
			revisionInfo: ["$q", "cloudcollab", "unityConnectService", "editorcollab", "services", "utAlert", function ($q, cloudcollab, unityConnectService, editorcollab, services, utAlert) {
				return cloudcollab.getRevision(unityConnectService.projectInfo, editorcollab.collabInfo.tip)
					.then(function success(result) {
						editorcollab.SeeAll("");
						return result;
					}, function error(obj) {
						if (obj && obj.message) {
							utAlert.setError(obj.message);

							return $q.reject({redirect: true, newState: 'nointernet'});
						} else {
							return $q.reject(obj);
						}
					});
			}]
		}
	});
}])
.controller('UpToDateCtrl', ["$scope", "revisionInfo", function UpToDateCtrl($scope, revisionInfo) {
	$scope.revisionInfo = revisionInfo;
}]);
