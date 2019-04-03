angular.module('ngPanel.common', [
	'ngUnity.editorcollab',
	'ut.components'
])

.directive('ucChangelistItem', ["editorcollab", "utAlert", "$q", "toolbarServices", "$rootScope", function (editorcollab, utAlert, $q, toolbarServices, $rootScope) {
	return {
		restrict: 'EAC',
		scope: {
			item: "=",
			onUpdate: "="
		},
		templateUrl: 'toolbar/common/changelistitem.tpl.html',
		controller: ["$scope", function ($scope) {
			var messages = {
				cannotDiffDeletedLocal : "Cannot show difference, change deleted locally.",
				cannotDiffDeletedRemote : "Cannot show difference, change deleted on server.",
				cannotDiffBothMoved : "No difference, change moved on both side.",
				cannotMergeDeletedLocal : "Cannot launch merge tool, change deleted locally.",
				cannotMergeDeletedRemote : "Cannot launch merge tool, change deleted on server.",
				cannotMergeBothMoved : "No need to merge, change moved on both side.",
				cancelMessage : "Ok",

				confirmChooseTheir : "By accepting server change, you'll lose local change!",
				cancelChooseTheir : "No"
			};

			var messageClearListener = $rootScope.$on('conflict.message.clear.others', function (event, item) {
				if (!($scope.closeMessage && item.path && $scope.item.path)) {return;}
				if ($scope._broadcastGuard) {return;}		// Prevent infinite loops

				if (item.path !== $scope.item.path) {
					$scope.closeMessage();
				}
			});

			$scope.$on('$destroy', function () {
				messageClearListener();
			});

			function setMessage(message, cancel, confirm) {
				if(message || cancel || confirm) {
					$scope._broadcastGuard = true;
					$rootScope.$broadcast('conflict.message.clear.others', $scope.item);
					delete $scope._broadcastGuard;
				}

				$scope.message = message;
				$scope.confirm = confirm;
				$scope.cancel = cancel;
			}

			$scope.onUpdate = $scope.onUpdate || angular.noop;
			$scope.resolved = false;
			setMessage();

			function userError(error) {
				if (error) {
					utAlert.setError("Error resolving conflict: ", error);
				} else {
					utAlert.clearAlert();
				}
				$scope.resolved = false;
			}

			function postConflictHandler(promise, errorFunc) {
				errorFunc = errorFunc || userError;

				$scope.resolved = true;
				$scope.closeMessage();

				return promise.success(function (result) {
					$scope.onUpdate();
					$scope.resolved = false;
				}).error(errorFunc);
			}

			$scope.mouseover = function () {
				$scope.hover = true;
			};

			$scope.mouseleave = function () {
				$scope.hover = false;
			};

			$scope.showMessage = function () {
				return $scope.message !== undefined;
			};

			$scope.closeMessage = function () {
				setMessage();		// Clears
			};

			$scope.mine = function () {
				var paths = [];
				paths.push($scope.item.path);
				paths = _.union(paths, _.map($scope.item.children, "path"));
				paths = _.uniq(paths);
				return postConflictHandler(editorcollab.SetConflictsResolvedMine(paths));
			};

			$scope.theirs = function () {
				if ($scope.confirm) {
					return;
				}

				setMessage(messages.confirmChooseTheir, messages.cancelChooseTheir, true);
			};

			$scope.theirsConfirmed = function () {
				var paths = [];
				paths.push($scope.item.path);
				paths = _.union(paths, _.map($scope.item.children, "path"));
				paths = _.uniq(paths);
				return postConflictHandler(editorcollab.SetConflictsResolvedTheirs(paths));
			};

			$scope.showDiff = function () {
				if ($scope.item.localStatus === "deleted") {
					setMessage(messages.cannotDiffDeletedLocal, messages.cancelMessage);
					return;
				}
				if ($scope.item.remoteStatus === "deleted") {
					setMessage(messages.cannotDiffDeletedRemote, messages.cancelMessage);
					return;
				}
				if ($scope.item.localStatus === "moved" && $scope.item.remoteStatus === "moved") {
					setMessage(messages.cannotDiffBothMoved, messages.cancelMessage);
					return;
				}
				toolbarServices.showConflictDifferences($scope.item.path);
			};

			$scope.externalMerge = function () {
				if ($scope.item.localStatus === "deleted") {
					setMessage(messages.cannotMergeDeletedLocal, messages.cancelMessage);
					return;
				}
				if ($scope.item.remoteStatus === "deleted") {
					setMessage(messages.cannotMergeDeletedRemote, messages.cancelMessage);
					return;
				}
				if ($scope.item.localStatus === "moved" && $scope.item.remoteStatus === "moved") {
					setMessage(messages.cannotMergeBothMoved, messages.cancelMessage);
					return;
				}
				toolbarServices.requireDiffMerge().then(function () {
					postConflictHandler(editorcollab.LaunchConflictExternalMerge($scope.item.path));
				}).error(function () {
					$scope.resolved = false;
				});
			};

			$scope.showEdit = function() {
				return $scope.hover && !$scope.resolved && !$scope.confirm && !$scope.message;
			};
		}]
	};
}])
.factory('toolbarServices', ["$q", "editorcollab", "utAlert", function($q, editorcollab, utAlert) {
	function diffError(error) {
		error = error || 'Could not show conflict differences. Unknown error.';
		var message = error.message ? error.message : error;

		utAlert.setError(message);
	}

	function ToolbarServices() {
	}

	ToolbarServices.prototype.requireDiffMerge = function () {
		return editorcollab.IsDiffToolsAvailable().then(function (result) {
			if (!result) {
				utAlert.setInfo('You have not set any Diff/Merge tools. Check your Unity preferences.');
				return $q.reject();
			}
		});
	};

	ToolbarServices.prototype.showConflictDifferences = function (itemPath) {
		this.requireDiffMerge().then(function () {
			return editorcollab.ShowConflictDifferences(itemPath).error(diffError);
		});
	};

	ToolbarServices.prototype.showDifferences = function (itemPath) {
		this.requireDiffMerge().then(function () {
			return editorcollab.ShowDifferences(itemPath).error(diffError);
		});
	};

	return new ToolbarServices();
}]);
