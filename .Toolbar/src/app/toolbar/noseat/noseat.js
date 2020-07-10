angular.module('ngPanel.noseat', [
	'ui.router',
	'ngUnity.cloudcore'
])
.config(["$stateProvider", function config($stateProvider) {
	$stateProvider.state('noseat', {
		url: '/no-seat',
		views: {
			'root': {
				templateUrl: 'toolbar/noseat/noseat.tpl.html',
				controller: 'NoSeatCtrl'
			}
		},
		resolve: {
			seatInfoUrl: ["$q", "cloudcore", "unityConnectService", function($q, cloudcore, unityConnectService) {
				return $q.all([cloudcore.getConfigEntry('coreui'), unityConnectService.GetProjectInfo()])
					.then(function(results) {
					return results[0] + '/orgs/' + results[1].organizationId + '/projects/'+
                        results[1].projectGUID +'/unity-teams/';
				});
			}]
		}
	});

}])
/**
 * Displays a message to the user that they require Unity Teams to use the Collaborate service
 * @param {Object} '$scope'
 * @param {angular directive} 'utAlert' Displays error messages.
 * @param {angular factory} 'unityConnectService' Service for network interaction.
 * @param {angular factory} 'editorcollab' Binds to C++ Collab state, used to update seat status.
 * @param {angular factory} 'cloudcoreUi' See cloudcore.js. Provides project, organisation, and service interface.
 * @param {String} 'seatInfoUrl' Provides the URL the user will be sent to when they wish to learn more about teams.
 */
.controller('NoSeatCtrl', ["$scope", "utAlert", "unityConnectService", "editorcollab", "cloudcoreUi", "seatInfoUrl", function ($scope, utAlert, unityConnectService, editorcollab, cloudcoreUi, seatInfoUrl) {

		var isRefreshing = false;

		/**
		 * UI event from user.
		 * Performs a network call to check seat status.
		 */
		$scope.requestUnityTeamsStatus = function() {
			if (isRefreshing) {
				return;
			}
			isRefreshing = true;
			utAlert.setInfo("Retrieving Unity Team status...");

			cloudcoreUi.CheckTeamSeats(false).then(function(hasSeat) {
				editorcollab.SetSeatAvailable({seat: hasSeat});
				utAlert.clearAlert();
				isRefreshing = false;
			}).catch(function(error){
				utAlert.setError("Refresh complete.  Access required for Unity Teams");
				isRefreshing = false;
			});
		};

		/**
		 * UI event to load help link.
		 */
		$scope.openSeatInfoUrl = function () {
			unityConnectService.OpenAuthorizedURLInWebBrowser(seatInfoUrl);
		};

}]);

