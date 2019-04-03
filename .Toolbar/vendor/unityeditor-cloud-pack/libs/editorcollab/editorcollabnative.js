angular.module('ngUnity.editorcollab')
.factory('editorCollabNative', ["$q", "$rootScope", "$timeout", "unityService", "cloudcoreUi", "unityConnectService", "unityCloudPanelService", "unityProjectService", "notifications", "StreamEvent", "utAlert", function ($q, $rootScope, $timeout, unityService, cloudcoreUi, unityConnectService, unityCloudPanelService, unityProjectService, notifications, StreamEvent, utAlert){
	// dfd is resolved when at least one poll is complete
	var dfd = $q.defer();
	var service = unityService.getUnityObject('unity/collab');
    var proxyClient = unityService.getUnityObject('unity/collab/proxy');
	var collabConnect = unityService.getUnityObject('unity/project/cloud/collab');
	var hubConnect = unityService.getUnityObject('unity/project/cloud/hub');
	var jobId = 0;
	var lastTip;
	var TRANSFER_PROGRESS_MESSAGE = "transferProgress";
	var TIP_UPDATED_MESSAGE = "tipUpdated";
	var STATE_CHANGED_MESSAGE = "stateChanged";
	var ERROR_STATE_CHANGED_MESSAGE = "errorChanged";
	var LOCAL_CHANGES_FILTERED_MESSAGE = "filterChanged";
	var lastConflictsHash = 0;
	var currState;

	var States = {
		kCollabNone: 0,
		kCollabLocal: 1,
		kCollabSynced: 2,
		kCollabOutOfSync: 4,
		kCollabMissing: 8,
		kCollabCheckedOutLocal: 16,
		kCollabCheckedOutRemote: 32,
		kCollabDeletedLocal: 64,
		kCollabDeletedRemote: 128,
		kCollabAddedLocal: 256,
		kCollabAddedRemote: 512,
		kCollabConflicted: 1024,
		kCollabMovedLocal: 2048,
		kCollabMovedRemote: 4096,
		kCollabUpdating: 8192,
		kCollabReadOnly: 16384,
		kCollabMetaFile: 32768,
		kCollabUseMine: 65536,
		kCollabUseTheir: 131072,
		kCollabChanges: 262144,
		kCollabMerged: 524288,
		kCollabPendingMerge: 1048576,
		kCollabAny: 0xFFFFFFFF
	};

	var events = {
		update: new StreamEvent('collabInfo.update')
	};

	var tickJobProgress = {call:'GetJobProgress', args: jobId, into: 'jobProgress'};

	var turnOnCollab = function () {
		collabConnect.IsServiceEnabled().then(function(enabled) {
			if (!enabled) {
				return unityCloudPanelService.EnableService(true).then(function () {
					// Success (then needs to be called to run the promise..)
					service.SaveAssets().then(function() {
						//Save assets to update project settings file.
					});
				});
			}
		});
	};

	var tryChangeState = function () {
		var newState = service.getCollabState();

		if (currState !== newState) {
			$rootScope.$broadcast(STATE_CHANGED_MESSAGE, {
				oldState: currState,
				newState: newState
			});
			currState = newState;
		}
	};

	var speedHash = function (jsObject) {
		var s = JSON.stringify(jsObject);
		return s.split("").reduce(function(a,b){a=((a<<5)-a)+b.charCodeAt(0);return a&a},0);
	};

	service.IsReady = function () {
		return dfd.promise;
	};

	service.States = States;
	service.Errors = {
		// Error Codes From CollabErrors.h
		Codes: {
			None: 0
		},

		// Matches c++ code
		Priorities: {
			Critical: 0,
			Error: 1,
			Warning: 2,
			Info: 3,
			None: 4
		},

		Behaviour: {
			Alert: 0,
			Automatic: 1,				// Automatically dismiss after a while
			Hidden: 2,
			ConsoleOnly: 3,
			Reconnect: 4,
			HideOnClose: 5,				// Hide error when window has been dismissed
			Uncloseable: 6				// Cannot be closed by the user. System will do it (eg: no connection)
		}
	}

	service.lastErrorCode = service.Errors.Codes.None;

	service.ShouldStartPolling = false;

	service.getCollabState = function() {
		if (service.collabInfo === undefined) {
			return 'noproject';
		} else if (!service.collabInfo.seat) {
			return 'noseat';
		} else if (service.collabInfo.inProgress) {
			return 'progress';
		} else if (service.collabInfo.conflict) {
			return 'conflicts';
		} else if (service.collabInfo.update) {
			return 'update';
		} else if (service.collabInfo.publish) {
			return 'publish';
		} else {
			return 'uptodate';
		}
	};

	service.errorPriorityToAlert = function(priority) {
		if (priority === service.Errors.Priorities.Info) {
			return 'info';
		} else if (priority === service.Errors.Priorities.Warning) {
			return 'warning'
		} else if (priority === service.Errors.Priorities.Critical) {
			return 'critical';
		} else {
			return 'danger';
		}
	}

	service.onTransferProgress = function ($scope, callback) {
		$scope.$on(TRANSFER_PROGRESS_MESSAGE, function (event, transferProgress) {
			callback(transferProgress);
		});
	};

	service.onTipUpdated = function ($scope, callback) {
		$scope.$on(TIP_UPDATED_MESSAGE, function (event, data) {
			callback(data.oldTip, data.newTip);
		});
	};

	service.onStateChanged = function ($scope, callback) {
		$scope.$on(STATE_CHANGED_MESSAGE, function (event, data) {
			callback(data.oldState, data.newState);
		});
	};

	service.onErrorChanged = function ($scope, callback) {
		$scope.$on(ERROR_STATE_CHANGED_MESSAGE, function (event, inErrorState) {
			callback(inErrorState);
		});
	};

	service.onLocalChangesFiltered = function ($scope, callback) {
		$scope.$on(LOCAL_CHANGES_FILTERED_MESSAGE, function(event, filteredChanges) {
			callback(filteredChanges);
		});
	};

	service.canCancelJob = function() {
		return service.jobProgress && service.jobProgress.canCancel;
	};

	service.cancelJob = function () {
		service.CancelJob(jobId);
	};

	service.enableCollab = function () {
		return collabConnect.IsServiceEnabled().then(function (enabled) {
			if (enabled) {
				return $q.when();
			}

			if (unityConnectService.projectInfo.projectBound) {
				return turnOnCollab();
			} else {
				return cloudcoreUi.GetOrganizations(true).success(function (organizations) {
					if (organizations.length === 1) {
						// Create cloud project
						var orgId = organizations[0].id;
						return cloudcoreUi.EnableProject(orgId).success(function () {
							return turnOnCollab();
						});
					} else {
						// Show enable
						hubConnect.ShowServicePage();
						return $q.when();
					}
				});
			}
		});
	};

	service.refreshSocketAccess = function () {
		var deferred = $q.defer();
		unityProjectService.IsReady().then(function() {
			unityProjectService.GetRESTServiceURI().then(function (restServiceUrl) {
				deferred.resolve({
					rest_url: restServiceUrl
				})
			});
		});
		return deferred.promise;
	};

	//  ClearErrors which will send a notification with error none, so don't reset lastErrorCode to None here.
	service.clearErrorUI = function() {
		service.ClearErrors();
	};

	service.ResyncToTip = function() {
		return service.ResyncToRevision(service.collabInfo.tip);
	};

	service.SeeAll = function(filter) {
		service.ShowInProjectBrowser(filter);
	}

	function dirname(path) {
		return path.replace(/\\/g, '/').replace(/\/[^\/]*$/, '');
	}

	// Sort by folder name.
	// Also this sorts files before folder in order to ensure that all files in a folder are placed before its containing folders
	// Eg: file 'x', 'z' will be listed together before folder 'y'. Otherwise, 'y' and its content will be listed before 'z'
	function sortPathFolder(change) {
		var output;

		if (change.isFolderMeta) {
			output = change.path.replace('.meta', '');
		} else {
			output = dirname(change.path);
		}

		return output.toLowerCase();
	}

	// Need to remove .meta when sorting, otherwise assets/folder.meta goes after assets/folder/a_file.txt
	function sortPathName(change) {
		var path = change.path.toLowerCase().replace('.meta', '');
		return path;
	}

	/**
	 * Algorithm
	 * 	1. Group by folder
	 * 	2. Sort folder files by name
	 * 	3. Sort folders by name
	 *
	 * 	Result:
	 * 		Assets/c.txt
	 * 		Assets/Cool/a.txt
	 *    		Assets/Cool/b.txt
	 *         Assets/Done/a.txt
	 */
	service.orderChanges = function(changes) {
		if (!_.isArray(changes)) {return changes;}

		var sorted = _.orderBy(changes, [{reverting: true}, sortPathFolder, sortPathName]);
		return sorted;
	}


	service.conflictsNotificationHandler = function(conflicts) {
		service.conflicts = service.orderChanges(conflicts);
		if (typeof(service.conflicts) !== 'undefined') {
			var conflictsHash = speedHash(service.conflicts);
			if (lastConflictsHash !== conflictsHash) {
				lastConflictsHash = conflictsHash;
				$rootScope.$broadcast('collab.conflicts.changed', service.conflicts);
			}
		}
	};

	service.localChangesNotificationHandler = function(localChangeInfo) {
		service.filter = localChangeInfo.filter;
		service.localChanges = service.orderChanges(localChangeInfo.changes);
		$rootScope.$broadcast('collab.changelist.changed', service.localChanges);
		// If conflicts changed, this will fire collab.conflicts.changed
		service.GetCollabConflicts().then(service.conflictsNotificationHandler);
		service.GetCollabInfo().then(service.collabInfoNotificationProxy);
	};

	service.collabInfoNotificationHandler = function(collabInfo) {
		service.collabInfo = collabInfo;

		if (service.collabInfo.conflict) {
			service.GetCollabConflicts().then(service.conflictsNotificationHandler);
		}

		if (service.collabInfo.inProgress) {
			var jobProgressPromise = service[tickJobProgress.call](tickJobProgress.arg);
			jobProgressPromise.then(function(result) {
				service[tickJobProgress.into] = result;

				if (service.jobProgress != null) {
					$rootScope.$broadcast(TRANSFER_PROGRESS_MESSAGE, service.jobProgress);
				}
			});
		}

		// Tip updated
		if (lastTip !== service.collabInfo.tip) {
			$rootScope.$broadcast(TIP_UPDATED_MESSAGE, {
				oldTip: lastTip,
				newTip: service.collabInfo.tip
			});
		}
		lastTip = service.collabInfo.tip;

		// Check state and broadcast
		tryChangeState();

		events.update.update(service.collabInfo.update);
	};

	service.collabErrorNotificationHandler = function (error) {
		if (error && error.code !== service.lastErrorCode) {
			service.currentError = error;
			$rootScope.$broadcast(ERROR_STATE_CHANGED_MESSAGE, error);
			service.lastErrorCode = error.code;
		}
	};

	// Add data obtained from the proxy client to the collab info
	service.collabInfoNotificationProxy = function (collabInfo) {
		var jobPromise = proxyClient.IsJobRunning();

		jobPromise.then(function(isJobRunning) {
			if(isJobRunning) {
				collabInfo.inProgress = true;
			}

			service.collabInfoNotificationHandler(collabInfo);
		});
	};

	service.ready(function (err) {
		notifications.subscribe(notifications.channels['CHANGES_TO_PUBLISH_CHANNEL'], service.localChangesNotificationHandler);
		notifications.subscribe(notifications.channels['COLLAB_INFO_CHANNEL'], service.collabInfoNotificationProxy);
		notifications.subscribe(notifications.channels['COLLAB_ERROR_CHANNEL'], service.collabErrorNotificationHandler);

		// Fetch manually for startup
		var p1 = service.GetChangesToPublish().then(service.localChangesNotificationHandler);
		var p2 = service.GetCollabInfo().then(service.collabInfoNotificationProxy);
		var p3 = service.GetCollabConflicts().then(service.conflictsNotificationHandler);
		var p4 = service.GetMainError().then(service.collabErrorNotificationHandler);

		$q.all([p1,p2,p3,p4]).then(function() {
			dfd.resolve();
		});
	});

	return service;
}]);
