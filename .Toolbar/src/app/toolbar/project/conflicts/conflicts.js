angular.module('ngPanel.toolbar.conflicts', [
	'ngUnity.editorcollab',
	'ui.router',
	'ut.components'
])
.config(["$stateProvider", function config($stateProvider) {
  $stateProvider.state('project.conflicts', {
    url: 'conflicts',
	templateUrl: 'toolbar/project/conflicts/conflicts.tpl.html',
	controller: 'ConflictCtrl'
  });
}])
.controller('ConflictCtrl', ["$scope", "editorcollab", "$state", "$rootScope", "cloudcollab", "unityProjectService", function ConflictCtrl($scope, editorcollab, $state, $rootScope, cloudcollab, unityProjectService) {
	var cleanupCallbacks = [];

	$scope.rebuildChanges = function() {
		var changes = [];
		var assets = [];
		var conflicts = editorcollab.conflicts;
		var conflictCount = 0;

		_.each(conflicts, function (change) {
			if (change.isConflict && change.relatedTo === "") {
				change.children = [];
				assets.push(change);
				conflictCount++;
			}
		});
		_.each(conflicts, function (change) {
			if (change.isConflict && change.relatedTo !== "") {
				var relatedTo = _.find(assets, function(asset) {
					return asset.path === change.relatedTo;
				});
				if (relatedTo) {
					relatedTo.children.push(change);
					//conflictCount++;
				} else {
					assets.push(change);
					conflictCount++;
				}
			}
		});

		$scope.conflictCount = conflictCount;
		$scope.conflictChanges = assets;
	};

	$scope.onUpdate = function () {
		editorcollab.clearErrorUI();

		cloudcollab.getRevisions(0, -1).then(function (revisions) {
			editorcollab.Update(revisions[0].id, true);
		});
	};

	$scope.seeAll = function() {
		editorcollab.SeeAll("conflicted");
	};

	$scope.rebuildChanges();
	$scope.needsUpdate = editorcollab.collabInfo.update;

	// TODO: Instead of using these functions for cleanup, put the registration
	// logic in editorCollabNative and pass in the controller scope
	cleanupCallbacks.push($rootScope.$on('collabInfo.update', function() {
		$scope.needsUpdate = editorcollab.collabInfo.update;
	}));
	cleanupCallbacks.push($rootScope.$on('collab.changelist.changed', $scope.rebuildChanges));
	cleanupCallbacks.push($rootScope.$on('collab.conflicts.changed', $scope.rebuildChanges));

	$scope.$on('$destroy', function() {
		cleanupCallbacks.forEach(function(callback) {
			callback();
		});
	});
}]);
