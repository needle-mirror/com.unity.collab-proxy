function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

angular.module('ngUnity.cloudcollab', [
    'ngUnity.cloudcredentials',
	'ngUnity.connectService',
	'ngUnity.errors',
	'ngUnity.general',
	'ut.components'])

.factory('cloudcollab', ["$q", "$cookies", "$http", "$localStorage", "unityConnectService", function ($q, $cookies, $http, $localStorage, unityConnectService) {
	var service = {};

	function filterMetaFiles(revision) {
		var assetFiles = {};
		var revisionItems = [];
		_.each (revision.changes, function(item) {
			if(!endsWith(item.path, '.meta')) {
				assetFiles[item.path] = true;
				item.isMeta = false;
				revisionItems.push(item);
			}
		});

		_.each (revision.changes, function(item) {
			if(!assetFiles.hasOwnProperty(item.path)) {
				if (!assetFiles.hasOwnProperty(item.path.substr(0, item.path.length - 5))) {
					item.isMeta = true;
					revisionItems.push(item);
				} else {
					//hide meta file if there is a corresponding asset file to avoid having double entries
				}
			}
		});

		revision.changes = revisionItems;
		revision.totalCount = revisionItems.length;
	}

	/**
	 * Get organizations that user is a member of AND that user has a project with
	 * @param forceRefresh
	 * @returns {promise}
	 */
	service.getRevisions = function (project, tip, includeChanges, limit) {
		var projectId = (project && project.id) || unityConnectService.projectInfo.projectGUID;

		return unityConnectService.IsReady().then(function (ready) {
			var config = {
				ignoreLoadingBar: true
			};

			var full_url = unityConnectService.urls.collab + "/api/projects/"+projectId+"/branches/master/revisions?";
			full_url = (typeof(includeChanges) !== "undefined" || includeChanges)? full_url + "include=changes&" : full_url;
			full_url = (typeof(limit) !== "undefined" || limit)? full_url + "limit=" + limit + "&": full_url;
			return $http.get(full_url, config).then(
			function success(response) {
				if(typeof tip !== "undefined") {
					var found = false;
					// Iterate over and mark the revisions based on whether we have them
					_.each (response.data.revisions, function(item, index){
						if(item.hasOwnProperty("changes")) {
							filterMetaFiles(item);
						}
						if(item.id == tip) {
							item.currentRevision = true;
							found = true;
						}
						if(found) {
							item.obtained = true;
						}
					});
				}
				return response.data.revisions;
			});
		});
	};

	service.getRevision = function (project, revisionID) {
		var config = {
			ignoreLoadingBar: true
		};

		var projectId = project.id || unityConnectService.projectInfo.projectGUID;
		var full_url = unityConnectService.urls.collab + "/api/projects/" + projectId + "/branches/master/revisions/" + revisionID;

		return $http.get(full_url, config).then(
			function success(response) {
				return response.data;
			});
	};

	service.getRevisionDetails = function (project, revisionID) {
		var config = {
			ignoreLoadingBar: true
		};

		var projectId = project.id || unityConnectService.projectInfo.projectGUID;

		var full_url = unityConnectService.urls.collab + "/api/projects/" + projectId + "/branches/master/revisions/" + revisionID + "?include=changes";

		return $http.get(full_url, config).then(
			function success(response) {
				return _.filter(response.data.changes, function (item){
					return !endsWith(item.path, ".meta");
				});
			});
	};

	service.refreshChannelAccess = function () {
		var deferred = $q.defer();
		unityConnectService.IsReady().then(function (ready) {
			deferred.resolve({
				project_id: unityConnectService.projectInfo.projectGUID,
				full_url: unityConnectService.urls.collab + "/api/refresh_channel_access"
			});
		});
		return deferred.promise;
	};

	return service;
}]);
