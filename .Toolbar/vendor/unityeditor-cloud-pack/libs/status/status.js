angular.module( 'ngPanel.status', [
	'ui.router',
	'ui.bootstrap',
	'ngUnity.connectService',
	'ut.components'
]).controller( 'StatusCtrl', ["$sce", "$state", "$scope", "$window", "$location", "unityConnectService", "utAlert", "$timeout", "$interval", function StatusCtrl( $sce, $state, $scope, $window, $location, unityConnectService, utAlert, $timeout, $interval) {
	$scope.load_error = false;
	$scope.reload_url = "";
	$scope.uc_error = false;
	$scope.message1 = "";
	$scope.message2 = "";
	$scope.message1AsHtml = "";
	$scope.message2AsHtml = "";
	$scope.description = "";
	$scope.showLogin = false;
	$scope.showReload = false;
	$scope.showActivate = false;
	$scope.showIcon = false;
	$scope.isSigningIn = false;
	$scope.isReloading = false;
	$scope.isActivating = false;
	$scope.isProcessing = false;

	function init() {
		var queryParams = $location.search();
		if (queryParams.failure !== undefined) {
			if (queryParams.failure == "load_error") {
				$scope.load_error = true;
				$scope.reload_url = decodeURIComponent(queryParams.reload_url);
			}
			else if (queryParams.failure == "unity_connect") {
				$scope.uc_error = true;
			}
		}
	}

	init();

	function processStatusState() {
		if (!$scope.connectInfo || !$scope.projectInfo) {return;}

		$scope.showIcon = false;
		$scope.showLogin = false;
		$scope.showReload = false;
		$scope.showActivate = false;

		if ($scope.load_error) {
			$scope.showIcon = true;
			$scope.message1 = "This is not the page";
			$scope.message2 = "you are looking for!!!";
			$scope.showReload = true;
		} else if ($scope.connectInfo.ready) {
			if ($scope.load_error) {
				$scope.showIcon = true;
				$scope.message1 = "This is not the page";
				$scope.message2 = "you are looking for!!!";
				$scope.showReload = true;
			} else {
				if (!$scope.connectInfo.online) {
					$scope.showIcon = true;
					$scope.message1 = "No network";
					$scope.message2 = "connection <span class='nobr'>:(</span>";
				} else if ($scope.connectInfo.workOffline) {
					$scope.showIcon = true;
					$scope.message1 = "You are";
					$scope.message2 = "working offline";
					$scope.showLogin = true;
				} else if (!$scope.connectInfo.loggedIn) {
					$scope.showIcon = true;
					$scope.message1 = "You are";
					$scope.message2 = "not logged in";
					$scope.showLogin = true;
				} else {
					$scope.showIcon = true;
					$scope.message1 = "Oooops!";
					$scope.message2 = "Something went wrong";
					$scope.showReload = true;
				}
			}
		} else {
			$scope.message1 = "Connecting...";
			$scope.message2 = "";
		}

		$scope.message1AsHtml = $sce.trustAsHtml($scope.message1);
		$scope.message2AsHtml = $sce.trustAsHtml($scope.message2);
	}

	$scope.login = function () {
		$scope.isSigningIn = true;
		unityConnectService.ready(function(){
			unityConnectService.ShowLogin().then(function () {
				$scope.isSigningIn = false;
			});
		});
	};

	$scope.reload = function () {
		if ($scope.reload_url === "") {
			$scope.isReloading = true;
			unityConnectService.ready(function(){
				unityConnectService.GoToHub('cloud-menu').then(function () {
					$scope.isReloading = false;
				});
			});
		} else {
			$window.location.href = $scope.reload_url;
		}
	};

	$scope.activate = function () {
		$scope.isActivating = true;
		utAlert.clearAlert();
		
		unityConnectService.ready(function() {
			var projectName = unityConnectService.projectInfo.projectName;

			// Rename/Move/Activate project
			unityConnectService.RenameProject(projectName).
				success(function(status){
					unityConnectService.ActivateProject().
						success(function(status){
							unityConnectService.RefreshProject().
								then(function(){
									$scope.isActivating = false;
									$state.go('connect.cloudServices');
								});
						}).
						error(function(status) {
							$scope.isActivating = false;
							utAlert.setError('Unable to activate project.', status);
						});
				}).
				error(function(status) {
					$scope.isActivating = false;
					if (status == 422)
					{
						utAlert.setError('Project name already taken.', status);
					}
					else
					{
						utAlert.setError('Unable to rename project.', status);
					}
				});
		});
	};

	$scope.$watch(function () {
		return unityConnectService.connectInfo;
	},
	function(newVal, oldVal) {
		$scope.connectInfo = newVal;
		processStatusState();
	}, true);

	$scope.$watch(function () {
		return unityConnectService.projectInfo;
	},
	function(newVal, oldVal) {
		$scope.projectInfo = newVal;
		processStatusState();
	}, true);
}])
.run(["unityConnectService", "$rootScope", function(unityConnectService, $rootScope) {
	unityConnectService.ready(function () {
		// True sets status to visible
		$rootScope.checkVisibility = function () {
			if (!unityConnectService.connectInfo || !unityConnectService.connectInfo.ready) {
				return false;
			}

			if (unityConnectService.connectInfo === undefined) {
				return true;
			}

			if (!unityConnectService.connectInfo.ready) {
				return true;
			}

			if (unityConnectService.connectInfo.online && unityConnectService.connectInfo.loggedIn) {
				return false;
			}

			return true;
		};
	});
}]);
