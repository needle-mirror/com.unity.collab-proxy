angular.module('ngUnity.directives', [
	'ngUnity.connectService',
	'ngUnity.projectService',
	'ngUnity.cloudPanelService',
	'ngUnity.clipboardService',
	'ngPanel.status',
	'angular-loading-bar',
	'ut.components',
	'angulartics',
	'angulartics.google.analytics',
	'ngUnity.cloudcore',
	'ngUnity.tooltips'
])
.config(['cfpLoadingBarProvider', function (cfpLoadingBarProvider) {
	cfpLoadingBarProvider.includeSpinner = false;
}])

.directive('nagPrism', ['$compile', function($compile) {
		return {
				restrict: 'A',
				transclude: true,
				scope: {
					source: '@',
					language: '@'
				},
				link: function(scope, element, attrs, controller, transclude) {

						scope.$watch('source', function(v) {
							element.find("code").html(v);
							Prism.highlightElement(element.find("code")[0]);
						});



						transclude(function(clone) {
							if (clone.html() !== undefined) {
								element.find("code").html(clone.html());
								$compile(element.contents())(scope.$parent);
							}
						});
				},
				template: "<code></code>"
		};
}])

// deprecated should migrate ut-code-sample
.directive('unityCodeSample', ["unityConnectService", function(unityConnectService) {
	return {
		restrict: 'EA',
		scope: {
			sampleList: '=',
			projectId: '='
		},
		templateUrl: "libs/sourceSample.tpl.html",
		link: function(scope, elem, attrs) {

			scope.projectInfo= unityConnectService.projectInfo;

			scope.setSampleIndex = function (index){
				scope.sampleIndex= index;
				if (index > -1){
					scope.currentCode = scope.currentCode.replace(/%%UPID%%/g, projectInfo.projectGUID);
					scope.formattedCurrentCode = scope.currentCode.replace(/</g, "&lt;");
					scope.formattedCurrentCode = scope.formattedCurrentCode.replace(/>/g, "&gt;");
					Prism.highlightAll();
				}

			};

			scope.$watch('sampleList', function(newValue, oldValue) {
				if (newValue){
					scope.setSampleIndex((scope.sampleList && scope.sampleList.length)?0:-1);
				}
			});
		}
	};
}])

.directive('unityCollapseSection', function() {
	return {
		restrict: 'E',
		replace: true,
		transclude: true,
		scope: {
			title: '=',
			collapsed: '@'
		},
		templateUrl: "libs/unityCollapseSection.tpl.html"
	};

})

.directive('ladda', function() {
	return {
		restrict: 'A',
		link: function(scope, element, attrs) {
			if (element && element[0]) {
				var l = Ladda.create(element[0]);
				scope.$watch(attrs.ladda, function(newVal, oldVal) {
					if (newVal !== undefined) {
						if (newVal) {
							l.start();
						} else {
							l.stop();
						}
					}
				});
			}
		}
	};
})

.directive('legalPanel', ["unityService", "unityProjectService", "utAlert", "unityConnectService", "cloudcoreUi", function (unityService, unityProjectService, utAlert, unityConnectService, cloudcoreUi) {
	return {
		restrict: 'E',
		replace: true,
		templateUrl: "libs/legalPanel.tpl.html",
		scope: {
			panelInfo: '='
		},
		link: function ($scope) {
			function coppaToBool(coppaValue) {
				var compliance = coppaValue;

				// Legacy: compliance used to be either true/false/'undefined' (string). It now is only a bool value
				if (compliance === 'undefined') {
					compliance = false;
				}

				// Convert from service value to bool
				if (coppaValue === 'compliant') {
					compliance = true;
				} else if (coppaValue === 'not_compliant') {
					compliance = false;
				}

				return compliance;
			}

			function boolToCoppa(coppa) {
				return coppa ? 'compliant' : 'not_compliant';
			}

			$scope.continueMode = !$scope.panelInfo.isHub;

			$scope.compliance = false;
			$scope.oldCompliance = $scope.compliance;

			$scope.isSaving = false;

			$scope.openExternalLink = function (link) {
				unityProjectService.ready(function () {
					unityProjectService.OpenLink(link);
				});
			};

			$scope.modified = function () {
				return $scope.compliance !== $scope.oldCompliance;
			};

			$scope.cancel = function () {
				// not so nice, but we have to make sure toggle is in unchecked state
				$("#cmn-toggle-enable-service").attr("checked", false);
				if ($scope.panelInfo.preEnable) {
					$scope.panelInfo.preEnable = false;
				}
				$scope.panelInfo.enabled = false;
			};

			$scope.saveChanges = function () {
				if ($scope.isSaving) {
					return false;
				}

				$scope.isSaving = true;
				utAlert.clearAlert();

				// Convert to format expected by cloud API.
				var compliance = boolToCoppa($scope.compliance);

				unityConnectService.ready(function () {
					cloudcoreUi.SetCOPPACompliance(compliance).success(function (list) {
						utAlert.setInfo("COPPA compliance changed");
						$scope.isSaving = false;
						$scope.oldCompliance = $scope.compliance;
						if ($scope.panelInfo.confirmedLegal !== undefined) {
							$scope.panelInfo.confirmedLegal();
						}
					}).error(function (status) {
						$scope.isSaving = false;
						$scope.compliance = coppaToBool(unityConnectService.projectInfo.COPPA);
					});
				});
			};

			$scope.showInfo = function () {
				utAlert.setInfo("Project is currently locked. This feature is unavailable.");
			};

			unityConnectService.ready(function () {
				$scope.compliance = coppaToBool(unityConnectService.projectInfo.COPPA);
				$scope.oldCompliance = $scope.compliance;

				$scope.$watch(function () {
					return unityConnectService.projectInfo.COPPA;
				},
				function (newVal, oldVal) {
					$scope.compliance = coppaToBool(newVal);
				}, true);
			});

			unityConnectService.ready(function () {
				$scope.hasEditRights = function () {
					return unityConnectService.projectInfo.permissions.hasEditRights() && !unityConnectService.projectInfo.coppaLock;
				}
			});
		}
	};
}])

.directive('ucDocLinks', ["unityCloudPanelService", "unityProjectService", function (unityCloudPanelService, unityProjectService) {
return {
		restrict: 'E',
		replace: true,
		templateUrl: "libs/doc-links.tpl.html",
		link: function(scope) {
			scope.panelInfo= unityCloudPanelService.panelInfo;
			scope.openExternalLink = function (link){
				var destUrl= unityCloudPanelService.getEnvLinkValue(link.ref);
				unityProjectService.OpenLink(destUrl);
			};
		}
	};
}])

.directive('ucSupportedPlatform', ["unityCloudPanelService", "unityProjectService", function (unityCloudPanelService, unityProjectService) {
return {
		restrict: 'E',
		replace: true,
		templateUrl: "libs/supported-platform.tpl.html",
		link: function(scope) {
			scope.supportedPlatforms= unityCloudPanelService.panelInfo.supportedPlatforms;
		}
	};
}])

.directive('ucCodeSample', ["unityCloudPanelService", "unityConnectService", "unityClipboardService", function(unityCloudPanelService, unityConnectService, unityClipboardService) {
	return {
		restrict: 'EA',
		templateUrl: "libs/sourceSample.tpl.html",
		link: function(scope) {

			scope.projectInfo= unityConnectService.projectInfo;
			scope.sampleList= unityCloudPanelService.codeSamples;
			scope.setSampleIndex= 0;

			scope.copyToClipboard = function (content){
				unityClipboardService.CopyToClipboard(content);
			}

			scope.setSampleIndex = function (index){
				scope.sampleIndex= index;
				if (index > -1){
					scope.currentCode = scope.sampleList[index].code.replace(/%%UPID%%/g, scope.projectInfo.projectGUID);
					scope.formattedCurrentCode = scope.currentCode.replace(/</g, "&lt;");
					scope.formattedCurrentCode = scope.formattedCurrentCode.replace(/>/g, "&gt;");
					Prism.highlightAll();
				}

			};

			scope.$watchCollection('sampleList', function(newValue, oldValue) {
				if (newValue){
					scope.setSampleIndex((scope.sampleList && scope.sampleList.length)?0:-1);
				}
			});
		}
	};
}])

.directive('ucPanelHeader', ["$state", "$timeout", "$q", "unityCloudPanelService", "unityConnectService", "unityProjectService", "utAlert", function($state, $timeout, $q, unityCloudPanelService, unityConnectService, unityProjectService, utAlert) {
	return {
		restrict: 'E',
		replace: true,
		templateUrl: "libs/panelHeader.tpl.html",
		scope: {
			showToggle: '='
		},
		link: function(scope) {
				if(!scope.hasOwnProperty('showToggle')) {
					scope['showToggle'] = true;
				}

				scope.projectInfo= unityConnectService.projectInfo;
				scope.panelInfo= unityCloudPanelService.panelInfo;

				var enableDeferred;

				scope.panelInfo.setEnabled = function (){
					enableDeferred = $q.defer();

					if (!scope.panelInfo.requiresCOPPA){
						enableDeferred.promise = unityCloudPanelService.EnableService(!scope.panelInfo.enabled);
						return enableDeferred.promise;
					}

					if (scope.panelInfo.preEnable){
				        scope.panelInfo.preEnable= false;
				        this.enabled= false;
						enableDeferred.resolve(true);
				        return enableDeferred.promise;
				    }

				    if (!scope.panelInfo.enabled && scope.projectInfo.COPPA=='undefined'){
			  			scope.panelInfo.preEnable= true;
						enableDeferred.resolve();
				    }else {
						enableDeferred.promise = unityCloudPanelService.EnableService(!scope.panelInfo.enabled);
				    }

					return enableDeferred.promise;
				};

				if (scope.panelInfo.requiresCOPPA){
					scope.panelInfo.confirmedLegal= function (){
					  unityCloudPanelService.EnableService(true).
						  then(function() {
							  if (enableDeferred) {
								  enableDeferred.resolve(true);
							  }
						  }).
						final(function() {
						  scope.panelInfo.preEnable= false;
						  scope.panelInfo.enabled= true;
						});
					};
				}

				scope.connectInfo= unityConnectService.connectInfo;
    			scope.userInfo= unityConnectService.userInfo;

				scope.showValidateEnableMsg = function (){
					if (!unityCloudPanelService.canEnableService()){
						utAlert.setError("You need owner privilege to enable or disable "+scope.panelInfo.title);
						return;
					}
				};

				// display can't enable msg once
				scope.showValidateEnableMsg();

				scope.canEnableService = function (){
					return unityCloudPanelService.canEnableService();
				};

				scope.inState = function (stateName){
					return $state.includes(stateName);
				};

				scope.showGoToDashboard = function (){
					if (scope.isHub()) {
						if (!unityConnectService.projectBindState || !unityConnectService.projectBindState.cloudProjectFullAccess) {
							return false;
						}

						if (!scope.projectInfo){
							return false;
						} else if (!unityConnectService.connectInfo.loggedIn) {
							return false;
						}

						return scope.projectInfo.projectGUID;
					}
					return scope.panelInfo.enabled;
				};

				scope.goIfActivated = function (path){

					if (scope.panelInfo.preEnable){
						if (!scope.panelInfo.enabled){
							// not so nice, but we have to make sure toggle is in unchecked state
							$("#cmn-toggle-enable-service").attr("checked",false);
						}else{
							scope.panelInfo.enabled= false;
						}
						scope.panelInfo.preEnable= false;
					}

					if (!scope.isCloudPossible()) {
						return;
					}

					unityConnectService.GoToHub (path);
				};

				scope.isCloudPossible = function (){
					if (scope.connectInfo === undefined) {
						return false;
					}

					if (!scope.connectInfo.ready || !scope.connectInfo.online || !scope.connectInfo.loggedIn ) {
						return false;
					}

					if (!scope.projectInfo.projectBound) {
						return false;
					}

					return true;
				};

				scope.getIcon = function () {
					if (scope.projectInfo !== undefined && scope.projectInfo.icon !== undefined) {
						return scope.projectInfo.icon;
					}

					return "assets/serviceIcon.png";
				};

				scope.defaultProjectName= scope.projectInfo.projectName;


				scope.goToDashboard = function (){
					if (scope.panelInfo !== undefined){
						var link= unityCloudPanelService.getEnvLink('dashboardLink');
						unityConnectService.OpenAuthorizedURLInWebBrowser(link);
					}
				};

				scope.isHub = function() {
					return scope.panelInfo.serviceFlag === 'connect';
				};
		}
	};
}])
.directive('unityCollapseTabs', ["$state", function($state) {
	return {
		restrict: 'A',
		replace: true,
		transclude: true,
		scope: {
		},

		templateUrl: "libs/unityTabs.tpl.html",
		link: function(scope, elem, iAttrs,ctrl,transclude){

			scope.items= [];
			var transcludedContent = transclude();

			scope.hasMultipleItems = function (){
				return scope.items.length > 1;
			};

			transcludedContent.each(function(k,itm){
				if ($(itm).is("li")){
					var utif = scope.$parent.$eval($(itm).attr('ut-if'));
					if (utif === true || utif === undefined)
					{
						scope.items.push({
							sref: $("a", itm).attr("ui-sref"),
							text: $("a", itm).text()
						});
					}
				}
			});


			scope.currentItem = function (){
					var currentItem= _.find(scope.items, function (itm){
						return $state.is(itm.sref);
					});

					if (currentItem){
						return currentItem.text;
					}

					return "Select...";
			};

		}
	};
}])
.directive('utSwitch', ["utDirectives", function (utDirectives) {
	return {
		templateUrl: "libs/switch.tpl.html",
		scope: {enabled: '=ngModel'},
		link: function (scope, elements, attrs, ngModel) {
			scope.toggle = function () {
				scope.enabled = !scope.enabled;
			};
		}
	};
}])
.service('utDirectives', function () {
	/**
	 * Connect utilities for Directives
	 */
	return {
		/**
		 * Standard handler for ng-model directive
		 *
		 * same as using scope: {enabled: '=ngModel'}, except that it handles validation and formatters better
		 *
		 * @param scope
		 * @param ngModel NgModelController
		 * @param varName Variable name on the current scope that the controller should be found to
		 */
		ngModelHandler: function (scope, ngModel, varName) {
			ngModel.$parsers.push(function (viewValue) {
				scope[varName] = viewValue;
				return viewValue;
			});

			scope.$watch(varName, function (newval, oldval) {
				ngModel.$setViewValue(newval);
			});
		}
	};
}).directive('utFocusOn', function () {
	/**
	 *
	 * Allows setting focus on an element using broadcast.		From: http://stackoverflow.com/questions/14833326/how-to-set-focus-on-input-field
	 * Doesn't work well with objects that have just been shown since broadcast won't affect them
	 */
	return function (scope, elem, attr) {
		scope.$on(attr.utFocusOn, function (e) {
			elem[0].focus();
		});
	};
}).directive('utDisabledAll', function() {
	/**
	 *
	 * Allows disabling element and all of its children 			 Based on: http://code.realcrowd.com/angularjs-automatically-disable-groups-of-elements-during-http-calls/
	 */
	return {
		restrict: 'A',
		link: function (scope, element, attrs) {

			scope.$watch(attrs.utDisabledAll, function (isDisabled) {
				var jqElement = jQuery(element);

				var elements = jqElement
					.find(':not([ut-disabled-all])')
					.filter(function (index) {
						// Filter out all elements who have a parent that has ut-disabled-all which isn't the current element
						return jQuery(this)
						.parents()
						.not(jqElement)
						.filter('[ut-disabled-all]').length === 0;
					})
					.filter(':not([ng-disabled])')
					.add(jqElement);

				elements.attr('disabled', isDisabled);
			});
		}
	};
}).directive('utSetFocus',["$timeout", function($timeout) {
	/**
	 * Sets focus when element in shown. Useful for hidden inputs				From on: http://stackoverflow.com/questions/24415168/setting-focus-on-an-input-field-after-ng-show
	 */
	return {
		restrict : 'A',
		link : function($scope,$element,$attr) {
			$scope.$watch($attr.utSetFocus,function(_focusVal) {
				$timeout(function() {
					_focusVal ? $element.focus() :
					$element.blur();
				});
			});
		}
	}
}]).directive('utInfo',["$compile", function($compile) {
	/**
	 * Sets info icon on Button and creates an identical impostor to receive the click and invoke the desired action
	 */
	return {
		restrict : 'A',
		link : function(scope, $element, attrs) {
			var impostor = $element.clone().appendTo($element.parent());
			impostor.prepend('<i ut-disabled-all="false" class="icon-info info-icon"></i>');
			impostor.addClass("ut-button-info").removeClass("ut-button-outline");
			impostor.removeAttr("ng-click");
			impostor.removeAttr("ut-info");
			impostor.removeAttr("ng-disabled");
			impostor.attr("ut-disabled-all", false);
			if (impostor.attr("ng-show")) {
				impostor.attr("ng-show", "!"+$element.attr("ng-show"));
			} else {
				console.error("missing ng-show attribute for directive ut-info");
				impostor.attr("ng-show", false);
			}
			impostor.attr("ng-click", attrs["utInfo"]);
			$compile(impostor)(scope);
		}
	}
}]).factory('airbrake', function() {
	var airbrake = new airbrakeJs.Client({projectId: 1, projectKey: 'a6e7249d3e79b1675ff8055dfcdb0a08'});
	return airbrake;
}).run(["airbrake", function(airbrake) {
	//save error and send to server
	window.onerror = function (message, url, lineNumber) {
		airbrake.push(new Error({message: message, url: url, lineNumber: lineNumber}));
		return false;			// False lets the default handler keep running
	};
}])
.directive('disableOnCondition', function () {
	return {
		restrict: 'A',
		scope: {
			disableOnCondition: '='
		},
		link: function (scope, element, attrs) {
			// Set the initial state
			element.prop('disabled', scope.disableOnCondition);
			scope.$watch(function () {
				return scope.disableOnCondition;
			}, function (condition) {
				element.prop('disabled', condition);
			});
		}
	};
})
.filter('hiddenServices', function () {
	return function (services) {
		return _.filter(services, function (service) {
			var result = !_.includes(['Hub', 'ErrorHub'], service.name);
			return result;
		});
	};
})
.factory('utInitState', ["$timeout", function($timeout) {
	return {
		// State that the application is correctly loaded and visible so we are certain to not show any 'loading' overlay.
		visible: function() {
			if (window._clearInitErrorMessage) {
				$timeout(window._clearInitErrorMessage);
			}
		}
	};
}]);
