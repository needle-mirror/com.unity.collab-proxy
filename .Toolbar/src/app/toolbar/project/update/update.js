angular.module('ngPanel.toolbar.update', [
  'ngUnity.editorcollab',
  'ngUnity.cloudcollab',
  'ngUnity.connectService',
  'ngUnity.unityService',
  'ui.router'
  ])
.config(["$stateProvider", function config($stateProvider) {
  $stateProvider.state('project.update', {
    url: 'update',
    templateUrl: 'toolbar/project/update/update.tpl.html',
    controller: 'UpdateCtrl',
    resolve: {
      collabRevisions: ["$q", "cloudcollab", "editorcollab", "unityConnectService", function ($q, cloudcollab, editorcollab, unityConnectService) {
        var dfd = $q.defer();
        editorcollab.IsReady().then(function () {
          cloudcollab.getRevisions(unityConnectService.projectInfo, editorcollab.collabInfo.tip).then(function(results) {
            dfd.resolve(results);
          });
        });
        return dfd.promise;
      }]
    }
  });
}])
.controller('UpdateCtrl', ["$scope", "$state", "editorcollab", "notifications", "collabRevisions", "unityProjectService", "utAlert", function UpdateCtrl ($scope, $state, editorcollab, notifications, collabRevisions, unityProjectService, utAlert) {
  $scope.revisions = collabRevisions;

  $.fn.textWidth = function () {
    var html_org = $(this).html();
    var html_calc = '<span>' + html_org + '</span>';
    $(this).html(html_calc);
    var width = $(this).find('span:first').width();
    $(this).html(html_org);
    return width;
  };

  $scope.missingFilter = function (revision) {
    return !revision.obtained;
  };

  $scope.onUpdate = function () {
    editorcollab.clearErrorUI();
      unityProjectService.SaveCurrentModifiedScenesIfUserWantsTo().success(function(continueToPublish) {
			if (continueToPublish) {
        editorcollab.Update($scope.revisions[0].id, true);
			}
		});
  };

  $scope.scrollElement = function (event) {
    var $this = $(event.target);
    var maxScroll = $this.textWidth();
    $this.css('text-overflow', 'inherit');
    $this.animate({
      scrollLeft: maxScroll // Maximum scroll limit
    }, maxScroll*15, 'linear');
  };

  $scope.unscrollElement = function (event) {
    var $this = $(event.target);
    $this.css('text-overflow', 'ellipsis');
    $this.stop();
    $this.animate({
      scrollLeft: 0
    }, 'slow');
  };

  $scope.revisionAvailable = function () {
	var obtained = _.reduce($scope.revisions, function(memo, rev){ return memo + (rev.obtained? 1 : 0); }, 0);
	return $scope.revisions.length - obtained;
  };

  $scope.showRevision = function (revision) {
    unityProjectService.GoToHistory();
  };

  notifications.subscribe(notifications.channels.COLLAB_REVISIONS_UPDATE_CHANNEL, function() {
    $state.reload();
  });
}]);
