angular.module('ngPanel.toolbar.publish')
.directive('ucPublishItem', ["$animate", function($animate) {
	return {
		scope: {
			localChange: '='
		},
		templateUrl: 'toolbar/project/publish/publishitem.tpl.html',
		controller: 'ucPublishItemCtrl',
		link: function($scope, element, attrs) {
			// md-virtual-repeat seems to have issues with ng-animate. In conjunction with ng-class, it was causing our icon classes to
			// be removed at the wrong time and behaved incorrectly. Hopefully this gets fixed in the future.
			$animate.enabled(element, false);
		}
	};
}])
.controller('ucPublishItemCtrl', ["$scope", "editorcollab", "toolbarServices", "$analytics", "$rootScope", function ($scope, editorcollab, toolbarServices, $analytics, $rootScope) {
	function removeRevertItem() {
		if ($scope.revertListItem) {
			delete $scope.revertListItem._hasError;
		}

		$scope.revertListItem = null;
	}

	removeRevertItem();

	function handleError(err, localChange) {
		if (err.message === undefined) {
			return;
		}

		var msg = JSON.parse(err.message);
		if (msg.code === errorCodes.FileAreadyExists) {
			localChange.forceOverwrite = true;
			localChange._hasError = errorCodes.FileAreadyExists;
			$scope.revertListItem = localChange;
			//$analytics.eventTrack('PublishError', {category: 'Publish', label: 'Error: ' +msg.code});
		}
	}
	
	var errorCodes = {
		NoError: 0,
		FileAreadyExists: 36
	};

	var revertableStates = {
		Revertable: 1 << 0,
		NotRevertable: 1 << 1,

		Revertable_File: 1 << 2,
		Revertable_Folder: 1 << 3,
		Revertable_EmptyFolder: 1 << 4,

		NotRevertable_File: 1 << 5,
		NotRevertable_Folder: 1 << 6,
		NotRevertable_FileAdded: 1 << 7,
		NotRevertable_FolderAdded: 1 << 8,
		NotRevertable_FolderContainsAdd: 1 << 9,

		// do not exceed Javascript Number range
		InvalidRevertableState: 1 << 53
	};

	var messages = [
		{ state: revertableStates.NotRevertable | revertableStates.NotRevertable_Folder,
			message: "Folder cannot be reverted" },
		{ state: revertableStates.NotRevertable | revertableStates.NotRevertable_FolderAdded,
			message: "This folder has just been created. There are no changes to revert to." },
		{ state: revertableStates.NotRevertable | revertableStates.NotRevertable_FolderContainsAdd,
			message: "This folder contains a new file. Folder cannot be reverted." },
		{ state: revertableStates.NotRevertable | revertableStates.NotRevertable_File,
			message: "File cannot be reverted." },
		{ state: revertableStates.NotRevertable | revertableStates.NotRevertable_FileAdded,
			message: "This file has just been created. There are no changes to revert to." },

		{ state: revertableStates.Revertable | revertableStates.Revertable_Folder,
			message: "Really revert entire folder? You'll revert all its contents" },
		{ state: revertableStates.Revertable | revertableStates.Revertable_EmptyFolder,
			message: "Really revert this folder? You'll lose local changes!" },
		{ state: revertableStates.Revertable | revertableStates.Revertable_File,
			message: "Really revert this file? You'll lose local changes!" }
	];

	function revertMessage(change) {
		var message;

		if (!change.isRevertable) {
			// Revert status message
			message = _.map(_.filter(messages, {state: change.revertableState}), 'message')[0];
		} else {
			// Revert Confirm message
			if (change.localStatus == "deleted" && (change.revertableState & revertableStates.Revertable_File)) {
				message = "Reverting this file will restore the last updated version of this file";
			} else {
				message = _.map(_.filter(messages, {state: change.revertableState}), 'message')[0];
			}
		}

		return message;
	}

	$scope.revertMessage = function () {
		if (!$scope.revertListItem) {
			return;
		}

		var message;

		if ($scope.revertListItem._hasError === errorCodes.FileAreadyExists) {
			message = "Really revert this file? It'll overwrite a local file!";
		} else {
			message = revertMessage($scope.revertListItem);
		}

		return message;
	};

	var messageClearListener = $rootScope.$on('changes.message.clear.others', function(event, change) {
		if ($scope._broadcastGuard) {return;}		// Prevent infinite loops

		if (change !== $scope.localChange) {
			removeRevertItem();
		}
	});

	$scope.$on('$destroy', function() {
		messageClearListener();
	});

	$scope.revertFile = function (localChange) {
		removeRevertItem();
		localChange.reverting = true;

		editorcollab.RevertFile(localChange.path, localChange.forceOverwrite).then(function () {
			// Do nothing. fix case 803434
		}, function (err) {
			localChange.reverting = false;
			handleError(err, localChange);
		});
	};

	$scope.askRevert = function (localChange) {
		$scope._broadcastGuard = true;
		$rootScope.$broadcast('changes.message.clear.others', localChange);		// Clear other messages
		delete $scope._broadcastGuard;

		$scope.revertListItem = localChange;
	};

	$scope.cancelRevert = function (localChange) {
		localChange.forceOverwrite = false;
		removeRevertItem();
	};

	$scope.showDiff = function (change) {
		toolbarServices.showDifferences(change.path);
	};


	$scope.getClass = function () {
		if (!$scope.localChange) {
			return;
		}

		var classes = [];
		classes.push($scope.localChange.localStatus);

		if ($scope.localChange.isFolderMeta) {
			classes.push('folder');
		} else {
			classes.push('file');
		}

		return classes;
	};

}]);
