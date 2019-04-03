angular.module('templates-common', ['libs/doc-links.tpl.html', 'libs/legalPanel.tpl.html', 'libs/panelHeader.tpl.html', 'libs/sourceSample.tpl.html', 'libs/status/status.tpl.html', 'libs/supported-platform.tpl.html', 'libs/switch.tpl.html', 'libs/unityCollapseSection.tpl.html', 'libs/unityTabs.tpl.html', '../unityeditor-components/src/components/alert/alert.tpl.html', '../unityeditor-components/src/components/checkbox/checkbox.tpl.html', '../unityeditor-components/src/components/progress/progress.tpl.html', '../unityeditor-components/src/components/switch/switch.tpl.html', '../unityeditor-components/src/components/wizard-progress/wizard-progress.tpl.html']);

angular.module("libs/doc-links.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/doc-links.tpl.html",
    "<ul class=\"link-list\"><li ng-repeat=\"link in panelInfo.docLinks\"><a analytics-on class=\"uni-link\" ng-click=\"openExternalLink(link)\">{{link.title}} <i class=\"icon-outbound\"></i></a></li></ul>");
}]);

angular.module("libs/legalPanel.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/legalPanel.tpl.html",
    "<div class=\"legal-panel\"><div class=\"panel-body\"><form ut-disabled-all=\"!hasEditRights()\"><div class=\"title\">Designation for Apps Directed to Children Under the Age of 13</div><div><ut-checkbox class=\"line\" ut-checkbox-model=\"compliance\" ut-checkbox-label=\"This app is primarily targeted to children under age 13.\"></ut-checkbox><label></label></div><div class=\"tools right\"><div ng-if=\"continueMode\"><button class=\"ut-button-outline\" analytics-on=\"click\" analytics-event=\"Coppa cancel\" ng-click=\"cancel()\">Cancel</button> <button class=\"ut-button-outline\" analytics-on=\"click\" analytics-event=\"Coppa save\" ng-show=\"hasEditRights()\" ng-click=\"isSaving || saveChanges()\" ng-disabled=\"isSaving\">Continue</button></div><div ng-if=\"!continueMode\"><div ng-if=\"modified()\"><button class=\"ut-button-outline\" analytics-on=\"click\" analytics-event=\"Coppa save\" ng-show=\"hasEditRights()\" ng-click=\"isSaving || compliance === oldCompliance || saveChanges()\" ng-disabled=\"isSaving\" ut-info=\"showInfo()\">Save Changes</button></div></div></div></form></div></div>");
}]);

angular.module("libs/panelHeader.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/panelHeader.tpl.html",
    "<div class=\"project-title-block\" ng-class=\"{'service-header':!isHub()}\"><div class=\"tools\"><a ng-if=\"!isHub()\" class=\"link pull-left\" ng-click=\"goIfActivated('cloud-menu')\"><i class=\"icon-back\"></i> Back to services</a> <a ng-if=\"showGoToDashboard()\" class=\"link pull-right\" ng-click=\"goToDashboard()\">Go to Dashboard<i class=\"icon-outbound\"></i></a></div><ut-alert></ut-alert><div class=\"panel-body\"><div ng-if=\"isHub()\" class=\"row title-row h2\"><div class=\"project-title\" ng-bind=\"projectInfo.projectName || defaultProjectName\"></div></div><div class=\"row title-row\"><img ng-if=\"!isHub()\" class=\"icon pull-left\" ng-src=\"{{getIcon()}}\"><h3 class=\"service-title\">{{panelInfo.title}} <sup class=\"postfix\">{{panelInfo.postfix}}</sup></h3></div><div class=\"row detail-row\"><div class=\"detail\" ng-bind=\"panelInfo.description || projectInfo.description\"></div><div ng-if=\"!isHub() && !panelInfo.hideEnableSwitch && showToggle\" class=\"switch pull-right\"><div class=\"disabled-mask\" ng-if=\"!canEnableService()\" ng-click=\"showValidateEnableMsg()\"></div><input ng-class=\"{'uni-checked': panelInfo.enabled || panelInfo.preEnable, 'uni-unchecked' : !panelInfo.enabled && !panelInfo.preEnable}\" ng-disabled=\"!canEnableService()\" id=\"cmn-toggle-enable-service\" class=\"cmn-toggle cmn-toggle-round-flat\" ng-checked=\"panelInfo.enabled\" ng-click-once=\"panelInfo.setEnabled()\" type=\"checkbox\"/><label for=\"cmn-toggle-enable-service\"></label></div></div></div></div>");
}]);

angular.module("libs/sourceSample.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/sourceSample.tpl.html",
    "<div class=\"row\" ng-show=\"sampleIndex != -1\"><div class=\"btn-group\" role=\"group\"><button type=\"button\" class=\"btn btn-xs btn-default col-md-4\" ng-class=\"{'selected':(sampleIndex==$index)}\" ng-repeat=\"sample in sampleList\" ng-click=\"setSampleIndex($index)\">{{sample.title}}</button></div><div class=\"code-sample\"><pre id=\"sample-box\" nag-prism source=\"{{formattedCurrentCode}}\" class=\"{{sampleList[sampleIndex].language}}\"></pre><div class=\"uni-link\"><a class=\"pull-right\" ng-click=\"copyToClipboard(sampleList[sampleIndex].code)\">Copy to Clipboard</a></div></div></div>");
}]);

angular.module("libs/status/status.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/status/status.tpl.html",
    "<div ng-controller=\"StatusCtrl\"><alert ng-repeat=\"alert in alerts\" class=\"alert alert-danger\" role=\"alert\" close=\"closeAlert($index)\">{{alert.msg}}</alert><div class=\"status\"><i class=\"icon-attention\" ng-if=\"showIcon\"></i><div class=\"messages\"><p class=\"title\" ng-bind-html=\"message1AsHtml\"></p><p class=\"title\" ng-if=\"message2 != ''\" ng-bind-html=\"message2AsHtml\"></p></div><div class=\"button-holder\" ng-if=\"showLogin\"><button type=\"button\" class=\"btn btn-success action\" ng-click=\"login()\" ng-disabled=\"isSigningIn\">Sign in...</button></div><div class=\"button-holder\" ng-if=\"showReload\"><button type=\"button\" class=\"btn btn-success action\" ng-click=\"reload()\" ng-disabled=\"isReloading\">Reload</button></div></div></div>");
}]);

angular.module("libs/supported-platform.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/supported-platform.tpl.html",
    "<div class=\"panel\"><div class=\"panel-body\"><h3>Supported Platforms</h3><div class=\"label-group\"><span ng-repeat=\"platform in supportedPlatforms\" class=\"label label-default\" ng-bind=\"platform\"></span></div><div class=\"spacer\"></div></div></div>");
}]);

angular.module("libs/switch.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/switch.tpl.html",
    "<input id=\"cmn-toggle-enable-service\" class=\"cmn-toggle cmn-toggle-round-flat\" ng-checked=\"enabled\" ng-click=\"toggle()\" type=\"checkbox\"><label for=\"cmn-toggle-enable-service\" analytics-on=\"click\"></label>");
}]);

angular.module("libs/unityCollapseSection.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/unityCollapseSection.tpl.html",
    "<div class=\"panel uni-collapse\" ng-class=\"{'collapsed':collapsed}\"><div class=\"panel-body\"><h4><a ng-click=\"collapsed=!collapsed\">{{title}}<i class=\"icon-caret pull-right\"></i></a></h4><div ng-transclude ng-hide=\"collapsed\"></div></div></div>");
}]);

angular.module("libs/unityTabs.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("libs/unityTabs.tpl.html",
    "<div class=\"unity-tabs\"><div class=\"collapsed-tabs dropdown\"><button ng-class=\"{'single-item':!hasMultipleItems()}\" class=\"btn btn-default dropdown-toggle\" type=\"button\" id=\"dropdownMenu1\" data-toggle=\"dropdown\" aria-expanded=\"true\">{{currentItem()}} <i ng-if=\"hasMultipleItems()\" class=\"tabs-caret icon-menu pull-right\"></i></button><ul ng-if=\"hasMultipleItems()\" class=\"dropdown-menu\" role=\"menu\" aria-labelledby=\"dropdownMenu1\"><li ng-repeat=\"item in items\" role=\"presentation\" ui-sref-active=\"active\"><a role=\"menuitem\" tabindex=\"-1\" href ui-sref=\"{{item.sref}}\">{{item.text}}</a></li></ul></div><ul class=\"expanded-tabs nav nav-tabs\"><li ng-repeat=\"item in items\" ui-sref-active=\"active\"><a href ui-sref=\"{{item.sref}}\" aria-controls=\"{{item.sref}}\" role=\"tab\">{{item.text}}</a></li></ul></div>");
}]);

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
