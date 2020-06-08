angular.module('templates-common', ['../unityeditor-components/src/components/alert/alert.tpl.html', '../unityeditor-components/src/components/checkbox/checkbox.tpl.html', '../unityeditor-components/src/components/progress/progress.tpl.html', '../unityeditor-components/src/components/switch/switch.tpl.html', '../unityeditor-components/src/components/wizard-progress/wizard-progress.tpl.html']);

angular.module("../unityeditor-components/src/components/alert/alert.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../unityeditor-components/src/components/alert/alert.tpl.html",
    "<alert class=\"alert\" ng-show=\"alertText\" type=\"{{alertType}}\" ng-click=\"resetAlert()\" close><div ng-bind-html=\"alertText\"></div><div class=\"more-info\" ng-show=\"getShowMoreinfo()\"><span ng-bind-html=\"linkPreText\"></span>&nbsp;<span class=\"alertlink\" ng-click=\"linkClickHandler()\" ng-bind-html=\"linkText\"></span><span ng-bind-html=\"linkSuffixText\"></span></div></alert>");
}]);

angular.module("../unityeditor-components/src/components/checkbox/checkbox.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../unityeditor-components/src/components/checkbox/checkbox.tpl.html",
    "<label class=\"ut-checkbox toggleswitch toggleswitch-green\" style=\"margin-top: 3px\"><input type=\"checkbox\" ng-model=\"utCheckboxModel\" name=\"checkbox{{$id}}\" id=\"checkbox{{$id}}\" class=\"toggleswitch-input\"> <span for=\"checkbox{{$id}}\" class=\"ut-checkmark toggleswitch-label\" data-on=\"On\" data-off=\"Off\"></span> <span for=\"checkbox{{$id}}\" class=\"ut-checkbox-label toggleswitch-handle\"><span class=\"ut-checkbox-label-content toggleswitch-label-content\">{{utCheckboxLabel}}</span></span></label>");
}]);

angular.module("../unityeditor-components/src/components/progress/progress.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../unityeditor-components/src/components/progress/progress.tpl.html",
    "<div class=\"ut-progress-container\"><span class=\"ut-progress-label\" ng-if=\"utProgressLabel\" ng-bind=\"utProgressLabel\"></span> <img class=\"spinner\" src=\"assets/icons/svg/spinner.svg\"></div>");
}]);

angular.module("../unityeditor-components/src/components/switch/switch.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../unityeditor-components/src/components/switch/switch.tpl.html",
    "<div class=\"slider\" ng-click=\"toggleValue()\" ng-class=\"{checked: checked, enabled: value}\"><div class=\"circle\"></div><div class=\"ut-bar-in-progress\" ng-if=\"inProgress\"></div></div>");
}]);

angular.module("../unityeditor-components/src/components/wizard-progress/wizard-progress.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../unityeditor-components/src/components/wizard-progress/wizard-progress.tpl.html",
    "<ul ng-if=\"utStep\" class=\"wizard-progress\"><li ng-repeat=\"steps in utSteps track by $index\"><div class=\"line {{$first ? 'first' : ''}} {{$index + 1 > utStep ? 'incomplete' : 'complete'}}\"></div><div ng-if=\"$index + 1 == utStep\" class=\"dot surround\"></div><div class=\"dot {{$index + 1 > utStep ? 'incomplete' : 'complete'}}\"></div><div class=\"line {{$last ? 'last' : ''}} {{$index + 1 >= utStep ? 'incomplete' : 'complete'}}\"></div></li></ul>");
}]);
