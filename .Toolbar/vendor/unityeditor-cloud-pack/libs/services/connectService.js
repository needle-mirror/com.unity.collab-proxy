angular.module('ngUnity.connectService', [
	'ngUnity.unityService',
	'ngCookies',
	'ngStorage',
	'ngUnity.general',
	'ngUnity.cloudcore',
	'ut.alert'
])

.run(["$rootScope", "unityConnectService", "Permissions", "ServiceFlags", function($rootScope, unityConnectService, Permissions, ServiceFlags) {
	/**
	 * Create cloudcore-related project info  		(Prevents dependency loops between unityConnectService and cloudcore)
	 */
	$rootScope.$on('connect.init', function(service) {
		unityConnectService.projectInfo.permissions = unityConnectService.projectInfo.permissions || new Permissions();
		unityConnectService.projectInfo.serviceFlags = unityConnectService.projectInfo.serviceFlags || new ServiceFlags();
	});
}])
/**
 * Connection with the Unity Editor
 */
.factory('unityConnectService', ["$q", "$interval", "$window", "$cookies", "$localStorage", "$sessionStorage", "$rootScope", "$timeout", "unityProjectService", "unityService", "utAlert", "StreamEvent", function ($q, $interval, $window, $cookies, $localStorage, $sessionStorage, $rootScope, $timeout,
										  					 unityProjectService, unityService, utAlert, StreamEvent) {
	var service = unityService.getUnityObject('unity/connect');
	var readyPromise = $q.defer();
	var bindingProject = false;				//  Are we currently binding a project?
	var CONNECTINFO_ONLINE_MESSAGE = 'connectInfo.online';

	/**
	 * Get bind state from project info
	 */
	function getBindState(projectInfo) {
		var bound = projectInfo.projectBound;
		var guid = projectInfo.projectGUID;

		if (bound === undefined && guid === undefined) {
			return;
		}							// Not yet initialized

		var bindState = {
			localProject: !guid,											// Status: bind project request
			cloudProjectNoAccess: !bound && guid,			// Status: unbind / request
			cloudProjectFullAccess: bound && guid		   	// Status: valid (no status screen)
		};

		return bindState;
	}

	function getSelf(members) {
		return _.find(members, function (member) {
			return member.email === service.userInfo.userName;
		});
	}

	var check;
	function readyCheck() {
		if ((service.connectInfo && service.connectInfo.ready) && service.urls !== undefined) {
			$interval.cancel(check);
			readyPromise.resolve(true);
		}
	}

	check = $interval(readyCheck, 50);
	readyCheck();

	var events = {
		online: new StreamEvent(CONNECTINFO_ONLINE_MESSAGE),
		projectBound: new StreamEvent('projectInfo.projectBound')
	};

	service.onOnlineStatus = function ($scope, callback) {
		$scope.$on(CONNECTINFO_ONLINE_MESSAGE, function (event, value) {
			callback(value);
		});
	};

	service.IsReady = function () {
		return readyPromise.promise;
	};

	service.ready(function(err){

		if (err){
			console.log("connect service not available, reason: "+err);
			return service;
		}
		service.connectInfo.ready = false;
		var pendingRequests  = {
			urls: false,
			connect: false,
			project: false,
			user: false
		};

		function init() {
			service.projectBindState = getBindState(service.projectInfo);

			$rootScope.$broadcast('connect.init', service);

			$rootScope.$on('$stateChangeStart',
				function(event, toState, toParams, fromState, fromParams){
				    service.RefreshProject();
				}
			);

			$rootScope.$on('userChanged', function (listener, userInfo) {
				
				_.forIn (service.projectBindState, function (value,key){
					 delete service.projectBindState[key];
				});

				service.RefreshProject();
			});

			$rootScope.$on('showWebView', function (listener, userInfo) {
				if (userInfo){
					service.RefreshProject();
				}
			});

		}

		init();

		function tickInfo() {

			if (service.projectInfo.valid && !bindingProject){
				$.extend(service.projectBindState, getBindState(service.projectInfo));
			}

			if (!pendingRequests.connect) {
				pendingRequests.connect = true;

				service.GetConnectInfo().
					success(function (connectInfo) {
						var stateChanged = connectInfo.ready !== service.connectInfo.ready; 	// Validity changed
						var errorChanged = connectInfo.error !== service.connectInfo.error;

						$.extend(service.connectInfo, connectInfo);

						var userChanged = stateChanged && service.connectInfo.ready;

						// hack to force userChanged flag since above stateChange doesn't seem
						// to work properly --  _unauthorized can never be reset to false
						if (service.projectInfo.permissions && service.projectInfo.permissions._unauthorized && connectInfo.initialized && connectInfo.loggedIn){
							userChanged= true;
						}

						if (userChanged) {
							service.projectInfo.permissions.userChanged();
						}

						events.online.update(connectInfo.online);
						
						if (errorChanged && service.connectInfo.error) {
							var options = {
								clearHandler: function () {
									service.ClearErrors();
								}	
							};
							
							if (!utAlert.alertText) {
								utAlert.setError(service.connectInfo.lastErrorMsg, 0, options);
							}
						}
					}).
					error (function (status) {
					}).
					final (function () {
						pendingRequests.connect = false;
					});
			}

			if (!pendingRequests.project) {
				pendingRequests.project = true;
				service.GetProjectInfo().
					success(function (projectInfo) {
						var stateChanged = projectInfo.valid !== service.projectInfo.valid; 	// Validity changed

						var prevBindState = getBindState(service.projectInfo);
						var bindState = getBindState(projectInfo);
						var bindingChanged = (prevBindState === undefined && bindState !== undefined) ||
							(prevBindState !== undefined && bindState !== undefined && bindState.cloudProjectFullAccess !== prevBindState.cloudProjectFullAccess); 	// Binding changed

						$.extend(service.projectInfo, projectInfo);

						if (projectInfo.COPPA === undefined) {
							service.projectInfo.COPPA = "undefined";
						} else {
							if (projectInfo.COPPA.value__ == 0) service.projectInfo.COPPA = "undefined";
							if (projectInfo.COPPA.value__ == 1) service.projectInfo.COPPA = "compliant";
							if (projectInfo.COPPA.value__ == 2) service.projectInfo.COPPA = "not_compliant";
						}

						// Don't update permissions while binding a project, since projectInfo (orgId) won't be the correct one yet.
						if (!bindingProject) {
							var permissions = service.projectInfo.permissions;
							if (stateChanged || (permissions && !permissions.fetched)) {
								if (projectInfo.valid) {
									permissions.update(projectInfo);
								}
							}

							var serviceFlags = service.projectInfo.serviceFlags;
							if (stateChanged || bindingChanged || !serviceFlags.fetched) {
								if (projectInfo.valid) {
									serviceFlags.update(projectInfo);
								}
							}
						}

						// Consider this update a refresh if we have a new valid state
						if (stateChanged && projectInfo.valid) {
							$rootScope.$broadcast('projectInfo.refreshed', service.projectInfo);
						}

						events.projectBound.update(projectInfo.projectBound);
					}).
					error (function (status) {
					}).
					final (function () {
						pendingRequests.project = false;
					});
			}

			if (!pendingRequests.user) {
				pendingRequests.user = true;
				service.GetUserInfo().
					success(function (userInfo) {
						var userChanged = userInfo.userName !== service.userInfo.userName;

						$.extend(service.userInfo, userInfo);

						if (userChanged) {
							$rootScope.$broadcast('userChanged', service.userInfo);
						}
					}).
					error (function (status) {
					}).
					final (function () {
						pendingRequests.user = false;
					});
			}
		}

		tickInfo();
		$interval(tickInfo, 100);

		function tickUrls() {

			if (!pendingRequests.urls) {
				pendingRequests.urls = true;

				var allUrls = $q.all([
					service.GetConfigurationUrlByIndex(0),
					service.GetConfigurationUrlByIndex(1),
					service.GetConfigurationUrlByIndex(2),
					service.GetConfigurationUrlByIndex(3),
					service.GetConfigurationUrlByIndex(6)
				]);

				allUrls.
					success(function (urls) {
						service.urls = {
							core: urls[0],
							collab: urls[1],
							webauth: urls[2],
							login: urls[3],
							identity: urls[4]
						};
					}).
					error (function (status) {
					}).
					final (function () {
						pendingRequests.urls = false;
					});
			}

		}

		tickUrls();
		$interval(tickUrls, 15*60*1000);

		service.IsProjectValid = function () {
			var deferred = $q.defer();

			if (service.projectInfo.valid)
			{
				deferred.resolve(true);
				return deferred.promise;
			}

			var check = $interval(function() {
				if (service.projectInfo.valid) {
					$interval.cancel(check);
					deferred.resolve(true);
				}
			}, 50);

			return deferred.promise;
		}

		service.RequestInvite = function() {
			var deferred = $q.defer();

			service.IsReady().
				then(function(ready) {
					var message = "Please contact the administrator of this project.";
					service.DisplayDialog("Request Invite", message, "OK", "").
						then(function(response) {
							deferred.resolve(response);
						});
				});

			return deferred.promise;
		}

		// Wrap real function because we need a UI confirmation
		service._UnbindProject = service.UnbindProject;
		service.UnbindProject = function(showConfirm) {
			var deferred = $q.defer();

			service.IsReady().
				then(function(ready) {

					if (showConfirm) {
						var message = "Are you sure you want to unlink this project?";
						service.DisplayDialog("Unlink Project", message, "No", "Yes").
							then(function(response) {
								if (!response) {
									service._UnbindProject().
										success(function() {
											deferred.resolve(true);
										}).
										error(function(status) {
											deferred.reject(status);
										});
								}
								else
									deferred.resolve(false);
							});
						} else {
							service._UnbindProject().
								success(function() {
									deferred.resolve(true);
								}).
								error(function(status) {
									deferred.reject(status);
								});
						}
				});

			return deferred.promise;
		}

		// Wrap real function because we need a UI confirmation
		service._legacyBindProject = service.BindProject;
		service._BindProject = function(projectGUID, projectName, projectOrgId) {
			var deferred = $q.defer();
			service._legacyBindProject(projectGUID, projectName, projectOrgId).
				success(function(){
					deferred.resolve(true);
				}).
				error(function(status){
					if (status.errorClass && status.errorClass == "TargetParameterCountException"){
						service._legacyBindProject(projectGUID, "", projectName, projectOrgId).
							success(function() {
								deferred.resolve(true);
							}).
							error(function(status){
								deferred.reject(status);
							});
					} else {
						deferred.reject(status);
					}
				});
			return deferred.promise;
		};

		service.BindProject = function(project, organization) {
			var deferred = $q.defer();

			service.IsReady().
				then(function(ready) {
					var message = "Are you sure you want to link to the project '";
					message += project.name;
					message += "' in organization '";
					message += organization.name;
					message += "'?";
					service.DisplayDialog("Link Project", message, "Yes", "No").
						then(function(response) {
							bindingProject = true;

							if (response) {
								service._BindProject(project.guid, project.name, project.org_id).
									success(function() {
										bindingProject = false;
										deferred.resolve(true);
									}).
									error(function(status) {
										bindingProject = false;
										deferred.reject(status);
									});
							} else {
								bindingProject = false;
								deferred.resolve(false);
							}
						});
				});

			return deferred.promise;
		};

		service._RefreshProject = service.RefreshProject;
		service.RefreshProject = function() {
			if (service.connectInfo && service.connectInfo.loggedIn){
				return service._RefreshProject();
			}
		}
	});

	return service;
}]);
