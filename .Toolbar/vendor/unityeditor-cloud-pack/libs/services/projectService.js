angular.module('ngUnity.projectService', [
	'ngUnity.unityService'
])
.run(["$rootScope", function ($rootScope) {
	// Convoluted way to start changing the unity theme. We cannot add the ng-href attribute on the stylesheet, otherwise
	// there is a brief white flash when the page loads while the theme is yet undefined.
	// Therefore, there is a global script that sets the theme at the very beginning of the page load, and once the project service
	// has finished its bootstrapping and retrieve the current theme, it starts handling which theme is displayed, since, unlike the
	// global script, it can listen to the editor and update when it changed.
	var themeInit = false;
	$rootScope.$on('themeChanged', function (event, theme) {
		// Only set it when there is a valid theme.
		if (theme && theme.name && !themeInit) {
			var path = window.unityGlobal ? window.unityGlobal.getStyleSheetPath() : 'unity-stylesheet-';

			var element = angular.element(document.querySelector(".main-theme"));
			element.attr('ng-href', path + "{{theme.name || 'dark'}}.css");
			themeInit = true;
		}
	});
}])
.factory('unityProjectService', ["$q", "unityService", "$rootScope", "$interval", function ($q,unityService, $rootScope, $interval){
	var Themes = {
		light: {
			name: 'light'
		},
		dark: {
			name: 'dark'
		}
	};

	function updateTheme() {
		$rootScope.theme = service.theme;
		$rootScope.$broadcast('themeChanged', service.theme);
	}

	var service = unityService.getUnityObject('unity/project');
	service.isPlayMode = false;
	service.projectPath = "";
	service.editorVersion = "";
	service.theme = window.unityGlobal ? {name: window.unityGlobal.getTheme()} : Themes.dark;
	updateTheme();

	service.IsReady = function () {
		var readyPromise = $q.defer();

		service.ready(function() {
			readyPromise.resolve(true);
		});

		return readyPromise.promise;
	};

	var _promise = service.promise;
		if (_promise && _promise.then) {
		service.promise = service.promise.then(function(promiseResult) {
	    return service.ready(function() {
				return promiseResult;
			});
	 });
 }

	service.ready(function(err){

		if (err){
			return service;
		}

		if (service.IsProjectCloudEnabled === undefined) {
			service.IsProjectCloudEnabled = service.IsProjectBound;
		}

		service.GetProjectEditorVersion().
			success(function(editorVersion) {
				service.editorVersion = editorVersion;
		});

		service.GetProjectPath().
			success(function (path) {
				service.projectPath = path;
			});

		service.GetBuildTarget().
			success(function (buildTarget) {
				service.buildTarget = buildTarget;
			});

		service.GetEnvironment().
			success(function (env) {
				if (env == "staging")
					service.dashboardUrl= "http://staging-core.cloud.unity3d.com/landing";
				else
					service.dashboardUrl= "http://cloud.unity3d.com";
			});

		function tickInfo() {
			// Editor version Compatibility
			if (service.GetEditorSkinIndex) {
				service.GetEditorSkinIndex().success(function (skinIndex) {
					var changed = service.skinIndex !== skinIndex;
					service.skinIndex = skinIndex;

					if (changed) {
						if (skinIndex === 0) {
							service.theme = Themes.light;
						} else {
							service.theme = Themes.dark;
						}

						updateTheme();
					}
				});
			}

			if (service.IsPlayMode) {
				service.IsPlayMode().success(function (value) {
					service.isPlayMode = value;
				});
			}
		}

		tickInfo();
		$interval(tickInfo, 250);

	});

	return service;
}]);
