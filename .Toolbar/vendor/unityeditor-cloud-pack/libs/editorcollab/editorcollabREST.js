angular.module('ngUnity.editorcollab')
.factory('editorCollabREST', ["$q", "$rootScope", "$http", "unityProjectService", function ($q, $rootScope, $http, unityProjectService){
	var service = {};
	var localChangesHash;
	var cachedData;

	var standardConfig = {
		ignoreLoadingBar: true
	};

	service.GetChangesToPublish = function () {
		var config = {
			ignoreLoadingBar: true
		};
		if (localChangesHash !== undefined){
			config.headers = {
				"if-none-match" : localChangesHash
			}
		}
		return unityProjectService.GetRESTServiceURI().then(function(restServiceUrl) {
			var full_url = restServiceUrl + "/unity/service/collab/localChanges";
			return $http.get(full_url, config).then(
				function success(response) {
					localChangesHash = response.headers("if-none-match");
					cachedData = response.data;
					return response.data;
				},
				function error(response) {
					if (response.status == 304 && typeof(cachedData) !== "undefined") {
						// no changes since last call
						return cachedData;
					}
					return $q.reject(response);
				}
			);
		});
	};

	service.GetChangesToPublishHash = function () {
		if(typeof(localChangesHash) !== undefined) {
			return localChangesHash;
		}
		return '';
	};

	service.GetMainError = function () {
		return unityProjectService.GetRESTServiceURI().then(function (restServiceUrl) {
			var full_url = restServiceUrl + "/unity/service/collab/error";
			return $http.get(full_url, standardConfig).then(
				function success(response) {
					return response.data;
				}
			);
		});
	};

	service.SetSeatAvailable = function (seatAvailability) {
        return unityProjectService.GetRESTServiceURI().then(function (restServiceUrl) {
            var full_url = restServiceUrl + "/unity/service/collab/seat";
            return $http.post(full_url, seatAvailability).then(
                function success(response) {
                    return response.data;
                }
            );
        });
	};

	return service;
}]);
