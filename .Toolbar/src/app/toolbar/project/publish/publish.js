angular.module('ngPanel.toolbar.publish', [
	'ngUnity.editorcollab',
	'ui.router',
	'ngUnity.unityService',
	'ngPanel.common',
	'ut.components',
	'ut.progress',
	'uc.editor.components',
	'ngUnity.projectService',
	'ngUnity.projectUnityVersion',
	'ngUnity.connectService',
	'ut.alert',
	'ngUnity.cloudPanelService',
	'angulartics',
	'angulartics.google.analytics'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('project.publish', {
		url: 'publish',
		templateUrl: 'toolbar/project/publish/publish.tpl.html',
		controller: 'PublishCtrl'
	});
}])
.controller('PublishCtrl', ["$scope", "$window", "$localStorage", "editorcollab", "$rootScope", "unityProjectService", "$q", "utAlert", "unityCloudPanelService", "projectUnityVersion", function PublishCtrl($scope, $window, $localStorage, editorcollab, $rootScope, unityProjectService, $q, utAlert,
												unityCloudPanelService, projectUnityVersion) {
	$scope.$storage = $localStorage;
	$scope.publishButtonLabel = editorcollab.collabInfo.error?"Publish again":"Publish now!";
	$scope.isFiltered = editorcollab.filter;

	var stripExtension = function (filePath) {
		var re = /[.]([^\/.])*$/;
		return filePath.replace(re, '');
	};

	var startsWith = function (str, value) {
		return str.substring(0, value.length) === value;
	};

	var isMeta = function(path) {
		return path.substr(path.length - 5) === '.meta';
	};

	$scope.changelist = [];
	var DynamicChanges = function() {
		this.loadedPages = {};
		this.PAGE_SIZE = 5;
		this.rebuildChanges_();
	};
	// Required.
	DynamicChanges.prototype.getItemAtIndex = function(index) {
		var pageNumber = Math.floor(index / this.PAGE_SIZE);
		var page = this.loadedPages[pageNumber];
		if (page) {
			return page[index % this.PAGE_SIZE];
		} else if (page !== null) {
			this.fetchPage_(pageNumber);
		}
	};
	// Required.
	DynamicChanges.prototype.getLength = function() {
		return $scope.changelist.length;
	};

 	DynamicChanges.prototype.fetchPage_ = function(pageNumber) {
		// Set the page to null so we know it is already being fetched.
		this.loadedPages[pageNumber] = null;
		// For we use the localChnages, in the future we could fetch using $http
		this.loadedPages[pageNumber] = [];
		var pageOffset = pageNumber * this.PAGE_SIZE;
		for (var i = pageOffset; i < pageOffset + this.PAGE_SIZE; i++) {
			var entry = $scope.changelist[i];
			this.loadedPages[pageNumber].push(entry);
		}
	};

	DynamicChanges.prototype.rebuildChanges_ = function (filteredChanges) {
		this.loadedPages = {};

		var changesBeingReverted = _.map(_.filter($scope.changelist, {reverting: true}), 'path');
		var localChanges = editorcollab.localChanges || [];
		var assets = localChanges.slice(0);

		// Remove all non-folder non-orphan meta files, and only give back the
		// changes matching the filter (if a filter was specified)
		var stems = {};
		var filtered = {};

		if (typeof(filteredChanges) !== 'undefined') {
			filteredChanges.forEach(function (item) {
				filtered[item.path] = true;
			});
		}

		assets.forEach(function (change) {
			stems[change.path] = true;
		});

		assets = assets.filter(function(change) {
			var allowInList = false;
			// Ensure that it was selected in our filter
			if (typeof(filteredChanges) !== 'undefined') {
				if (typeof(filtered[change.path]) === 'undefined') {
					return allowInList;
				}
			}

			if (change.isFolderMeta) {
				allowInList = true;
			} else if (!isMeta(change.path)) {
				allowInList = true;
			} else if (typeof(stems[stripExtension(change.path)]) === 'undefined') {
				allowInList = true;
			}

			return allowInList;
		});

		// Keep previous reverting status
		assets.forEach(function(change) {
			if (_.includes(changesBeingReverted, change.path)) {
				change.reverting = true;
			}
		});

		$scope.changelist = assets;
		$scope.isFiltered = editorcollab.filter;
	};

	var alertId;
	function checkUnityVersion(assets) {
		$scope.versionResults = projectUnityVersion.checkProjectVersion({assets: assets}).then(
			function (data) {
				var hasCommits = editorcollab.collabInfo.tip !== 'none',
					displayPublishWarning = data.projectVersionChanged && hasCommits;

				if (displayPublishWarning) {
					utAlert.setWarning(
						'Your version of Unity has changed. This will impact other members of this project.',
						0,
						{	showMoreInfo: true,
							linkPreText: 'Before publishing',
							linkText: 'click here for more details',
							linkSuffixText: '.',
							linkHandler: function () {
								$scope.jumpToDocumentation();
							}
						});
					alertId = utAlert.id;
				} else {
					utAlert.clearAlertById(alertId);
					alertId = undefined;
				}
			});
	}

	$scope.jumpToDocumentation = function () {
		$q.all([
			unityCloudPanelService.IsReady(),
			unityProjectService.IsReady()
		]).then(function () {
			var link = unityCloudPanelService.getContextHelpUrlById('unity-collaborate-upgrade-considerations');
			if (link) {
				unityProjectService.OpenLink(link);
			}
		});
	};

	$scope.onPublish = function (useSelectedAssetsAndConfirm) {
		editorcollab.clearErrorUI();
		unityProjectService.SaveCurrentModifiedScenesIfUserWantsTo().success(function(continueToPublish) {
			if (continueToPublish) {
				editorcollab.Publish($scope.$storage.comment || '', useSelectedAssetsAndConfirm, useSelectedAssetsAndConfirm);
				$scope.$storage.comment = '';
			}
		});
	};

	$scope.allowTabs = function (event) {
		var val, start, end;
		if (event.keyCode === 9) { // keyCode 9 is 'tab'
			event.preventDefault();
			// get caret position/selection
			val = event.target.value;
			start = event.target.selectionStart;
			end = event.target.selectionEnd;
			// set textarea value to: text before caret + tab + text after caret
			event.target.value = val.substring(0, start) + '\t' + val.substring(end);
			// put caret at right position again
			event.target.selectionStart = event.target.selectionEnd = start + 1;
		}
	};

	editorcollab.onErrorChanged($scope, function(inErrorState) {
		$scope.publishButtonLabel = inErrorState ? "Publish again" : "Publish now!";
	});

	editorcollab.onLocalChangesFiltered($scope, function(filteredChanges) {
		$scope.dynamicChanges.rebuildChanges_(filteredChanges);
		$scope.isFiltered = true;
	});

	var listChangedListener = $rootScope.$on('collab.changelist.changed', function () {
		// $scope.clearSelection();
		$scope.dynamicChanges.rebuildChanges_();
		checkUnityVersion($scope.changelist);
	});

	$scope.showActions = function(localChange) {
		return localChange && localChange.isRevertable && !localChange.reverting;
	};

	$scope.$on('$destroy', function() {
		listChangedListener();		// DeRegister...
	});

	$scope.seeAll = function() {
		editorcollab.SeeAll("any");
	};

	$scope.clearSelection = function() {
		editorcollab.ClearSelectedChangesToPublish();
	};

	$scope.dynamicChanges = new DynamicChanges();
	checkUnityVersion($scope.changelist);
}]);
