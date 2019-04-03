angular.module('templates-app', ['toolbar/common/changelistitem.tpl.html', 'toolbar/loggedout/loggedout.tpl.html', 'toolbar/nointernet/nointernet.tpl.html', 'toolbar/nolocalhost/nolocalhost.tpl.html', 'toolbar/noproject/noproject.tpl.html', 'toolbar/noseat/noseat.tpl.html', 'toolbar/project/conflicts/conflicts.tpl.html', 'toolbar/project/progress/progress.tpl.html', 'toolbar/project/publish/publish.tpl.html', 'toolbar/project/publish/publishitem.tpl.html', 'toolbar/project/update/update.tpl.html', 'toolbar/project/uptodate/uptodate.tpl.html', 'toolbar/refreshproject/refreshproject.tpl.html', 'toolbar/root/root.tpl.html', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/choose_mine.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/choose_theirs.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/cloud.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/external_merge.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/eye.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/history.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/invite.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/menu.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/no_internet.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/revert.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/up_to_date.svg', '../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/update.svg']);

angular.module("toolbar/common/changelistitem.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/common/changelistitem.tpl.html",
    "<div ng-mouseover=\"mouseover()\" ng-mouseleave=\"mouseleave()\" ng-disabled=\"resolved\"><div><div class=\"list-item-icon file\" ng-class=\"{conflict: item.isConflict, resolved: item.isResolved}\"></div><div class=\"list-item-details\"><div ng-if=\"!showMessage()\"><div class=\"list-item-title\" ng-bind=\"item.path | filename\"></div><div class=\"list-item-info\" ng-bind=\"item.path\"></div></div><div ng-if=\"showMessage()\" class=\"list-item-message\"><div class=\"conflict-message-text\" ng-bind=\"message\"></div><div class=\"conflict-message-tools\"><button class=\"conflict-button cancel\" analytics-on=\"click\" analytics-event=\"ConflictCancel\" analytics-category=\"Toolbar\" analytics-label=\"Cancel On Conflict Dialog\" ng-click-once=\"closeMessage()\" ng-bind=\"cancel\"></button> <button class=\"conflict-button ok\" analytics-on=\"click\" analytics-event=\"ConflictTheirs\" analytics-category=\"Toolbar\" analytics-label=\"Take Theirs Conflict Dialog\" ng-if=\"confirm\" ng-click-once=\"theirsConfirmed()\">Yes</button></div></div></div></div><div class=\"conflict-tools\" ng-if=\"showEdit()\"><button class=\"showDiff\" ng-click-once=\"showDiff()\" analytics-on=\"click\" analytics-event=\"ConflictDiff\" analytics-category=\"Toolbar\" analytics-label=\"Show Diff On Conflict Dialog\" uc-tooltip=\"See differences\"><i uc-icon=\"collab/eye\"></i></button> <button class=\"externalMerge\" ng-click-once=\"externalMerge()\" analytics-on=\"click\" analytics-event=\"ConflictMerge\" analytics-category=\"Toolbar\" analytics-label=\"Merge On Conflict Dialog\" uc-tooltip=\"Launch external tool\"><i uc-icon=\"collab/external_merge\"></i></button> <button class=\"mine\" ng-click-once=\"mine()\" analytics-on=\"click\" analytics-event=\"ConflictMine\" analytics-category=\"Toolbar\" analytics-label=\"Choose Mine On Conflict Dialog\" uc-tooltip=\"Choose mine\"><i uc-icon=\"collab/choose_mine\"></i></button> <button class=\"theirs\" ng-click-once=\"theirs()\" analytics-on=\"click\" analytics-event=\"ConflictTheirs\" analytics-category=\"Toolbar\" analytics-label=\"Choose Theirs On Conflict Dialog\" uc-tooltip=\"Choose theirs\"><i uc-icon=\"collab/choose_theirs\"></i></button></div></div>");
}]);

angular.module("toolbar/loggedout/loggedout.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/loggedout/loggedout.tpl.html",
    "<div class=\"hero-panel\"><div class=\"header\"><div class=\"header-content\"><i class=\"icon-attention\"></i><span class=\"text\">You are logged out</span></div></div><div class=\"content\"><p class=\"message\">Sign in to access Collaborate</p><button type=\"button\" class=\"btn btn-success action\" ng-click=\"login()\" analytics-on=\"click\" analytics-event=\"Login\" analytics-category=\"Toolbar\" analytics-label=\"Sign In From Logged Out\">Sign in...</button></div></div>");
}]);

angular.module("toolbar/nointernet/nointernet.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/nointernet/nointernet.tpl.html",
    "<div class=\"no-internet hero-panel\"><div class=\"header\">Service Unavailable</div><div class=\"panel-body content-centered\"><div class=\"error-dialog centered-content\"><div uc-icon=\"collab/no_internet\"></div><div class=\"message\">No Internet Connection</div></div></div></div>");
}]);

angular.module("toolbar/nolocalhost/nolocalhost.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/nolocalhost/nolocalhost.tpl.html",
    "<div class=\"header\">Localhost Unavailable</div><div class=\"hero-icon warning\">&#x26A0;</div><div class=\"hero-text center\">Collaborate was unable to establish a connection to Unity. <span ng-if=\"localUrl\">Please ensure you are able to visit <i ng-bind=\"localUrl\"></i> in your browser and reopen the project.</span></div>");
}]);

angular.module("toolbar/noproject/noproject.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/noproject/noproject.tpl.html",
    "<div class=\"header\"><button class=\"ut-button-flat primary\" ng-click=\"onEnableCollab()\" analytics-on=\"click\" analytics-event=\"StartNow\" analytics-category=\"Toolbar\" analytics-label=\"Start Now\">Start now!</button></div><div class=\"header-arrow-up\"></div><div uc-icon=\"collab/cloud\" class=\"hero-icon\"></div><div class=\"hero-text center\"><p ng-if=\"userMessageInnerHTML\" ng-bind-html=\"userMessageInnerHTML\"></p><p ng-if=\"userMessage\">{{userMessage}}</p></div>");
}]);

angular.module("toolbar/noseat/noseat.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/noseat/noseat.tpl.html",
    "<div class=\"hero-panel\"><div class=\"header\"><div class=\"header-content\"><i class=\"icon-attention\"></i><span class=\"text\">Requires Unity Teams</span></div></div><div class=\"content\"><p class=\"message\">Ask the organization owner for access to Unity Teams</p></div><div class=\"footer\"><div class=\"wrapper\"><span class=\"footnote\"><button class=\"refresh\" ng-click=\"requestUnityTeamsStatus()\">Refresh</button> if you&#39;ve recently received access.</span> <a class=\"ut-button-text\" ng-click=\"openSeatInfoUrl()\">Learn More<i class=\"icon-outbound\"></i></a></div></div></div>");
}]);

angular.module("toolbar/project/conflicts/conflicts.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/project/conflicts/conflicts.tpl.html",
    "<div class=\"update-info\" ng-if=\"needsUpdate\">New revision on the server, <a class=\"update-info-link\" ng-click=\"onUpdate()\" analytics-on=\"click\" analytics-event=\"UpdateConflict\" analytics-category=\"Toolbar\" analytics-label=\"Update in Conflict Screen\">update now</a></div><div class=\"header\"><div><div>There were merge conflicts!</div></div></div><div class=\"header-arrow-down\"></div><div class=\"scrollable\"><div class=\"subheading\"><ng-pluralize count=\"conflictCount\" when=\"{'one': '{{conflictCount}} Conflicting file', 'other': '{{conflictCount}} Conflicting files'}\"></ng-pluralize><div class=\"pull-right\"><a ng-click=\"seeAll()\">See all in project</a></div></div><div class=\"list\"><uc-changelist-item item=\"change\" on-update=\"rebuildChanges\" class=\"list-item common\" ng-repeat=\"change in conflictChanges\"></uc-changelist-item></div></div>");
}]);

angular.module("toolbar/project/progress/progress.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/project/progress/progress.tpl.html",
    "<div class=\"header\" ng-bind=\"title\"></div><div class=\"header-arrow-up\"></div><div class=\"panel-progress\"><div class=\"outer-progress\"><div round-progress max=\"max\" current=\"current\" color=\"#0cb4cc\" bgcolor=\"#000000\" radius=\"80\" stroke=\"6\" semi=\"false\" rounded=\"false\" clockwise=\"true\" responsive=\"false\" duration=\"800\" animation=\"easeInOutQuart\" animation-delay=\"0\"></div><div class=\"progress-percent\" ng-bind=\"info\"></div></div><a class=\"btn-small\" ng-click=\"doCancel()\" ng-class=\"{'btn-disabled':working || !canCancel}\" analytics-on=\"click\" analytics-event=\"CancelOperation\" analytics-category=\"Toolbar\" analytics-label=\"Cancel Operation\">Cancel</a></div>");
}]);

angular.module("toolbar/project/publish/publish.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/project/publish/publish.tpl.html",
    "<div class=\"publish\"><div class=\"header\"><textarea autofocus ng-trim=\"false\" ng-keydown=\"allowTabs($event)\" ng-model=\"$storage.comment\" id=\"user-comment\" placeholder=\"Describe your changes here\"></textarea><button ng-hide=\"isFiltered\" class=\"ut-button-flat primary\" ng-click=\"onPublish(false)\" analytics-on=\"click\" analytics-event=\"Publish\" analytics-category=\"Toolbar\" analytics-label=\"Publish Changes\">{{publishButtonLabel}}</button> <button ng-show=\"isFiltered\" class=\"ut-button-flat primary\" ng-click=\"onPublish(true)\" analytics-on=\"click\" analytics-event=\"Publish\" analytics-category=\"Toolbar\" analytics-label=\"Publish Selected\">Publish Selected ({{changelist.length}})</button></div><div class=\"header-arrow-up\"></div><div class=\"scrollable\"><div class=\"list\" md-virtual-repeat-container id=\"vertical-container\"><div class=\"subheading\"><a ng-show=\"isFiltered\" ng-click=\"clearSelection()\">&#x24e7; clear selection</a><ng-pluralize ng-hide=\"isFiltered\" count=\"changelist.length\" when=\"{'one': 'One Change', 'other': '{{changelist.length}} Changes'}\"></ng-pluralize><div class=\"pull-right\"><a ng-hide=\"isFiltered\" ng-click=\"seeAll()\">See all in project</a></div></div><div class=\"list-item local-change group\" md-virtual-repeat=\"localChange in dynamicChanges\" md-on-demand><uc-publish-item local-change=\"localChange\"></uc-publish-item></div></div></div></div>");
}]);

angular.module("toolbar/project/publish/publishitem.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/project/publish/publishitem.tpl.html",
    "<div class=\"container-content\"><div class=\"list-item-icon\" ng-class=\"getClass()\"></div><div class=\"list-item-details\"><div ng-if=\"!revertListItem\"><div class=\"list-item-title\" ng-if=\"!localChange.isFolderMeta && !localChange.isMeta\" ng-bind=\"localChange.path | filename\"></div><div class=\"list-item-title\" ng-if=\"!localChange.isFolderMeta && localChange.isMeta\" ng-bind=\"localChange.path | filename | stripExtension\"></div><div class=\"list-item-info file\" ng-if=\"!localChange.reverting && !localChange.isFolderMeta && !localChange.isMeta\" ng-bind=\"localChange.path\"></div><div class=\"list-item-info file\" ng-if=\"!localChange.reverting && !localChange.isFolderMeta && localChange.isMeta\" ng-bind=\"localChange.path | stripExtension\"></div><div class=\"list-item-info folder\" ng-if=\"!localChange.reverting && localChange.isFolderMeta\" ng-bind=\"localChange.path | stripExtension\"></div><ut-progress ng-show=\"localChange.reverting\" ut-progress-label=\"Reverting...\"></ut-progress></div><div class=\"confirm-revert-text\" ng-if=\"revertListItem\">{{revertMessage()}}</div></div><button ng-if=\"revertListItem && localChange.isRevertable\" class=\"confirm-revert-button\" ng-click=\"revertFile(localChange)\" analytics-on=\"click\" analytics-event=\"ConfirmY\" analytics-category=\"Toolbar\" analytics-label=\"Confirm revert (Yes) \">Yes!</button> <button ng-if=\"revertListItem && localChange.isRevertable\" class=\"confirm-revert-button\" ng-click=\"cancelRevert(localChange)\" analytics-on=\"click\" analytics-event=\"ConfirmN\" analytics-category=\"Toolbar\" analytics-label=\"Confirm revert (No)\">No</button> <button ng-if=\"revertListItem && !localChange.isRevertable\" class=\"confirm-revert-button\" ng-click=\"cancelRevert(localChange)\" analytics-on=\"click\" analytics-event=\"ConfirmO\" analytics-category=\"Toolbar\" analytics-label=\"Confirm revert (OK)\">Ok</button><div class=\"publish-tools\" ng-if=\"!localChange.reverting && !revertListItem\"><button class=\"showDiff\" uc-icon=\"collab/eye\" uc-tooltip=\"See differences\" ng-click=\"showDiff(localChange)\" analytics-on=\"click\" analytics-event=\"DiffShow\" analytics-category=\"Toolbar\" analytics-label=\"Show Diff\"></button> <button class=\"revert\" uc-icon=\"collab/revert\" uc-tooltip=\"Revert asset\" ng-click=\"askRevert(localChange)\" analytics-on=\"click\" analytics-event=\"DiffHide\" analytics-category=\"Toolbar\" analytics-label=\"Ask For Revert\"></button></div></div>");
}]);

angular.module("toolbar/project/update/update.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/project/update/update.tpl.html",
    "<div class=\"header\"><button ng-click=\"onUpdate()\" class=\"ut-button-flat primary\" analytics-on=\"click\" analytics-event=\"Update\" analytics-category=\"Toolbar\" analytics-label=\"Update Now\">Update now!</button></div><div class=\"header-arrow-down\"></div><div class=\"scrollable\"><div class=\"subheading\"><ng-pluralize count=\"revisionAvailable()\" when=\"{'0': 'No revision available.',\n" +
    "	                     'one': '1 revision available.',\n" +
    "	                     'other': '{} revisions available.'}\"></ng-pluralize></div><div class=\"list\"><div class=\"list-item group common\" ng-repeat=\"revision in revisions | filter: missingFilter\" ng-click=\"showRevision(revision)\"><div class=\"list-item-details\"><div class=\"list-item-title\"><strong>#{{revisions.length - revisions.indexOf(revision)}} -</strong> {{revision.author.name}} <small><time-ago from-time=\"{{revision.committed_date}}\"></time-ago></small></div><div ng-mouseover=\"scrollElement($event)\" ng-mouseleave=\"unscrollElement($event)\" class=\"list-item-info\" ng-bind=\"revision.comment\"></div></div></div></div></div>");
}]);

angular.module("toolbar/project/uptodate/uptodate.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/project/uptodate/uptodate.tpl.html",
    "<div class=\"header\">You are up to date!</div><div class=\"uptodate hero-content\"><div uc-icon=\"collab/up_to_date\" class=\"hero-icon\"></div><span ng-if=\"false\">#32</span></div><div class=\"hero-text\"><strong ng-bind=\"revisionInfo.author.name\"></strong> <small><time-ago from-time=\"{{revisionInfo.committed_date}}\"></time-ago></small><p ng-bind=\"revisionInfo.comment\"></p></div>");
}]);

angular.module("toolbar/refreshproject/refreshproject.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/refreshproject/refreshproject.tpl.html",
    "<div class=\"hero-panel\"><div class=\"header\"><div class=\"header-content\"><i class=\"icon-attention\"></i><span class=\"text\">Link Lost</span></div></div><div class=\"content\"><p class=\"message\">Unity seems to have lost its link to your project</p><button type=\"button\" class=\"btn btn-success action\" ng-click=\"onGoToHub()\" analytics-on=\"click\" analytics-event=\"MoreInfo\" analytics-category=\"Toolbar\" analytics-label=\"Link Lost Info\">More Info</button></div></div>");
}]);

angular.module("toolbar/root/root.tpl.html", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("toolbar/root/root.tpl.html",
    "<div class=\"pane\"><div class=\"content\" ui-view></div><div class=\"footer\" ut-disabled-all=\"hideFooter\"><div class=\"tools\"><button ng-click=\"onGoToHistory()\" uc-icon=\"collab/history\" uc-tooltip=\"View History\" analytics-on=\"click\" analytics-event=\"OpenHistory\" analytics-category=\"Toolbar\" analytics-label=\"Opened history from toolbar\"></button> <button ng-click=\"onGoToMembers()\" uc-icon=\"collab/invite\" uc-tooltip=\"Invite Teammate\" analytics-on=\"click\" analytics-event=\"OpenMembers\" analytics-category=\"Toolbar\" analytics-label=\"Opened members in web dashboard from toolbar\"></button></div><div class=\"tools\" ng-hide=\"true\"><button ng-click=\"onGoToSettings()\" uc-icon=\"collab/menu\" uc-tooltip=\"More Info\" analytics-on=\"click\" analytics-event=\"OpenSettings\" analytics-category=\"Toolbar\" analytics-label=\"Opened settings from toolbar\"></button></div></div></div>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/choose_mine.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/choose_mine.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 14.2 19\" style=\"enable-background:new 0 0 14.2 19\" xml:space=\"preserve\"><path d=\"M14.1,6.8L7.4,0.1C7.2,0,6.9,0,6.7,0.1L0.2,6.8c-0.2,0.2-0.2,0.5,0,0.7c0.3,0.3,0.7,0.1,0.8,0l5.7-5.7v16.7\n" +
    "	c0,0.3,0.2,0.5,0.5,0.5s0.5-0.2,0.5-0.5V1.8l5.7,5.7c0.2,0.2,0.5,0.2,0.7,0S14.3,6.9,14.1,6.8z\"/></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/choose_theirs.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/choose_theirs.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 14.7 18.9\" style=\"enable-background:new 0 0 14.7 18.9\" xml:space=\"preserve\"><path d=\"M0.4,12.1L7,18.7c0.2,0.2,0.5,0.2,0.7,0l6.6-6.6c0.2-0.2,0.2-0.5,0-0.7c-0.3-0.3-0.7-0.1-0.8,0l-5.7,5.7V0.5\n" +
    "	C7.8,0.2,7.6,0,7.3,0S6.8,0.2,6.8,0.5v16.7l-5.7-5.8c-0.2-0.2-0.5-0.2-0.7,0S0.2,12,0.4,12.1z\"/></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/cloud.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/cloud.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 115.1 87.3\" style=\"enable-background:new 0 0 115.1 87.3\" xml:space=\"preserve\"><g><path d=\"M89,22.1c-2.7,0-5.2,0.4-7.5,1.1c-2.9-12.3-14-21.3-26.8-21.3c-14.3,0-26.1,10.9-27.4,24.9c-1.2-0.2-2.4-0.3-3.5-0.3\n" +
    "		c-11.3,0-21.3,10.7-21.3,23c0,12.2,9.9,23,21.3,23h17c1.1,0,2-0.9,2-2s-0.9-2-2-2h-17c-9,0-17.3-9-17.3-19c0-9.9,8.2-19,17.3-19\n" +
    "		c1.6,0,3.2,0.2,4.8,0.7c0.6,0.2,1.3,0.1,1.8-0.3s0.8-1,0.8-1.6C31.3,16.4,41.8,5.9,54.7,5.9c11.7,0,21.7,8.7,23.3,20.4\n" +
    "		c0.1,0.6,0.5,1.2,1,1.5c0.6,0.3,1.2,0.3,1.8,0.1c2.4-1.1,5.2-1.7,8.2-1.7c10.4,0,19.6,9.8,19.6,21c0,11.3-9.1,21.3-19.6,21.3H68.8\n" +
    "		c-1.1,0-2,0.9-2,2s0.9,2,2,2H89c12.6,0,23.6-11.8,23.6-25.3C112.5,33.8,101.5,22.1,89,22.1z\"/><path d=\"M71.1,59.8c0.4,0.4,0.9,0.6,1.4,0.6c0.5,0,1-0.2,1.4-0.6c0.8-0.8,0.8-2,0.1-2.8L57.2,39.5c-0.8-0.8-2-0.8-2.8-0.1\n" +
    "		L36.7,56.2c-0.8,0.8-0.8,2-0.1,2.8c0.8,0.8,2,0.8,2.8,0.1l13.3-12.7v37c0,1.1,0.9,2,2,2s2-0.9,2-2V44.9L71.1,59.8z\"/></g></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/external_merge.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/external_merge.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 14.2 19.2\" style=\"enable-background:new 0 0 14.2 19.2\" xml:space=\"preserve\"><path d=\"M14.1,6.9L7.4,0.3c-0.2-0.2-0.5-0.2-0.7,0L0.2,6.9c-0.2,0.2-0.2,0.5,0,0.7s0.5,0.2,0.7,0l5.7-5.7v11.2c0,0.5-0.1,5-5,5\n" +
    "	c-0.3,0-0.5,0.2-0.5,0.5s0.2,0.5,0.5,0.5c3.2,0,4.8-1.7,5.5-3.5c0.7,1.8,2.3,3.5,5.5,3.5c0.3,0,0.5-0.2,0.5-0.5s-0.2-0.5-0.5-0.5\n" +
    "	c-4.9,0-5-4.5-5-5V1.9l5.7,5.7c0.1,0.1,0.2,0.1,0.4,0.1s0.3,0,0.4-0.1C14.3,7.4,14.3,7,14.1,6.9z\"/></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/eye.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/eye.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 23.4 13.6\" style=\"enable-background:new 0 0 23.4 13.6\" xml:space=\"preserve\"><g><path d=\"M11.7,13.6c-6.3,0-11.3-6.3-11.5-6.5c-0.1-0.2-0.1-0.4,0-0.6C0.4,6.2,5.4,0,11.7,0S23,6.3,23.2,6.5c0.1,0.2,0.1,0.4,0,0.6\n" +
    "		C23,7.3,18,13.6,11.7,13.6z M1.2,6.7c1.1,1.2,5.4,5.9,10.5,5.9S21.1,8,22.2,6.7c-1.1-1.2-5.4-5.9-10.5-5.9S2.3,5.5,1.2,6.7z\"/><circle cx=\"11.7\" cy=\"6.7\" r=\"3.2\"/></g></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/history.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/history.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 20.4 18.5\" style=\"enable-background:new 0 0 20.4 18.5\" xml:space=\"preserve\"><g><path d=\"M3.7,9.2c0-0.7-0.4-1.2-1-1.4V4.7c0.6-0.2,1-0.7,1-1.4s-0.4-1.2-1-1.4V0.7c0-0.3-0.2-0.5-0.5-0.5S1.7,0.5,1.7,0.7v1.2\n" +
    "		C1.2,2.1,0.8,2.7,0.8,3.3s0.4,1.1,0.9,1.3v3.3C1.2,8.1,0.8,8.6,0.8,9.2s0.4,1.1,0.9,1.3v3.3c-0.5,0.2-0.9,0.7-0.9,1.3\n" +
    "		s0.4,1.1,0.9,1.3v1.2c0,0.3,0.2,0.5,0.5,0.5s0.5-0.2,0.5-0.5v-1.2c0.6-0.2,1-0.7,1-1.4c0-0.7-0.4-1.2-1-1.4v-3.2\n" +
    "		C3.3,10.4,3.7,9.9,3.7,9.2z\"/><path d=\"M18.9,4.3h-12l-1.4-1l1.4-1h12c0.4,0,0.7,0.3,0.7,0.7v0.6C19.6,4,19.3,4.3,18.9,4.3z\"/><path d=\"M18.9,10.2h-12l-1.4-1l1.4-1h12c0.4,0,0.7,0.3,0.7,0.7v0.6C19.6,9.9,19.3,10.2,18.9,10.2z\"/><path d=\"M18.9,16.2h-12l-1.4-1l1.4-1h12c0.4,0,0.7,0.3,0.7,0.7v0.6C19.6,15.9,19.3,16.2,18.9,16.2z\"/></g></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/invite.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/invite.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 24.7 18.5\" style=\"enable-background:new 0 0 24.7 18.5\" xml:space=\"preserve\"><path d=\"M24.7,13c0-2.7-1.8-4.9-4.2-5.6c1.1-0.6,1.9-1.7,1.9-3.1c0-1.9-1.6-3.5-3.5-3.5c-1.1,0-2.1,0.5-2.7,1.4\n" +
    "	C15.3,0.9,13.9,0,12.3,0s-3,0.9-3.8,2.2C7.9,1.4,6.9,0.9,5.8,0.9c-1.9,0-3.5,1.6-3.5,3.5c0,1.3,0.8,2.5,1.9,3.1C1.8,8.1,0,10.3,0,13\n" +
    "	c0,1.8,2.7,2.6,5.1,2.7c0.2,0.7,0.9,1.4,2.3,2c0.1,0,0.2,0.1,0.3,0.1c0.2,0.1,0.4,0.1,0.6,0.2c0.1,0,0.3,0.1,0.4,0.1\n" +
    "	c0.3,0.1,0.5,0.1,0.8,0.2c0.1,0,0.2,0,0.3,0.1c0.3,0.1,0.7,0.1,1,0.1c0.1,0,0.1,0,0.2,0c0.4,0,0.9,0,1.3,0l0,0c0.2,0,0.4,0,0.6,0\n" +
    "	c0.1,0,0.3,0,0.4,0c0.1,0,0.1,0,0.2,0c0.2,0,0.4,0,0.5,0l0,0c2.6-0.3,4.9-1.1,5.4-2.7C22,15.6,24.7,14.8,24.7,13z M3.4,4.3\n" +
    "	c0-1.4,1.1-2.5,2.5-2.5c1,0,2,0.7,2.3,1.7c0.1,0.2,0.3,0.3,0.5,0.3s0.4-0.2,0.5-0.4C9.5,2,10.8,1,12.3,1s2.9,1,3.2,2.6\n" +
    "	C15.6,3.8,15.7,4,16,4c0.2,0,0.4-0.1,0.5-0.3c0.3-1,1.3-1.7,2.3-1.7c1.4,0,2.5,1.1,2.5,2.5S20.2,7,18.8,7c-1.1,0-2-0.8-2.3-1.9\n" +
    "	c-0.1-0.2-0.3-0.5-0.5-0.5l0,0c-0.2,0-0.4,0.3-0.5,0.5c-0.4,1.5-1.7,2.6-3.2,2.6S9.5,6.6,9.1,5.1c0-0.3-0.2-0.6-0.5-0.6l0,0\n" +
    "	C8.4,4.5,8.2,4.8,8.2,5C7.8,6,6.9,6.7,5.8,6.7C4.5,6.7,3.4,5.7,3.4,4.3z M18.6,15.2c-0.1,0.9-1.4,1.6-3.2,2c-0.1,0-0.1,0-0.2,0\n" +
    "	c0,0-0.1,0-0.2,0c-0.7,0.1-1.4,0.2-2.1,0.2c-0.2,0-0.4,0-0.6,0l0,0c0,0,0,0-0.1,0c-0.2,0-0.4,0-0.5,0c-0.7,0-1.4-0.1-2.1-0.2\n" +
    "	c-0.1,0-0.1,0-0.2,0c-0.1,0-0.1,0-0.2,0c-1.8-0.4-3.1-1.1-3.2-2c0-0.1,0-0.1,0-0.2c0-2.5,1.4-4.6,3.4-5.7l0,0c1.8-0.9,4-0.9,5.8,0\n" +
    "	l0,0c2.1,1,3.4,3.1,3.4,5.7C18.7,15.1,18.6,15.1,18.6,15.2z M9.2,8.3C8.7,7.9,8.1,7.6,7.4,7.4c0.4-0.2,0.8-0.6,1.2-1\n" +
    "	C8.9,7.1,9.5,7.6,10.1,8C9.8,8.1,9.5,8.2,9.2,8.3z M14.6,8c0.6-0.4,1.1-0.9,1.5-1.6c0.3,0.4,0.7,0.8,1.2,1c-0.6,0.2-1.3,0.5-1.8,0.9\n" +
    "	C15.1,8.2,14.8,8.1,14.6,8z M1,13c0-2.7,2.1-4.8,4.8-4.8c0.9,0,1.7,0.2,2.4,0.7C6.3,10.1,5,12.3,5,14.8C2.7,14.6,1,13.9,1,13z\n" +
    "	 M19.6,14.7c-0.1-2.5-1.3-4.6-3.2-5.9c0.7-0.4,1.6-0.7,2.4-0.7c2.7,0,4.8,2.1,4.8,4.8C23.7,13.9,22,14.6,19.6,14.7z\"/></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/menu.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/menu.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 19.2 4.1\" style=\"enable-background:new 0 0 19.2 4.1\" xml:space=\"preserve\"><g><circle cx=\"2.1\" cy=\"2.1\" r=\"2.1\"/><circle cx=\"9.6\" cy=\"2.1\" r=\"2.1\"/><circle cx=\"17.1\" cy=\"2.1\" r=\"2.1\"/></g></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/no_internet.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/no_internet.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 68.8 57.4\" style=\"enable-background:new 0 0 68.8 57.4\" xml:space=\"preserve\"><g><path d=\"M67.2,18.8c-3.4-3.4-7.2-6.3-11.3-8.5l6-6c0.8-0.8,0.8-2,0-2.8c-0.8-0.8-2-0.8-2.8,0l-7,7c-5.6-2.4-11.7-3.6-18-3.6\n" +
    "		c-12.2,0-23.7,4.7-32.5,13.3c-0.8,0.8-0.8,2,0,2.8c0.8,0.8,2,0.8,2.8,0c8-7.8,18.5-12.2,29.7-12.2c5.2,0,10.2,0.9,14.9,2.7\n" +
    "		l-5.6,5.6c-3-0.8-6.1-1.2-9.3-1.2c-9.3,0-18,3.6-24.7,10.1c-0.8,0.8-0.8,2,0,2.8s2,0.8,2.8,0c5.9-5.8,13.7-8.9,21.9-8.9\n" +
    "		c2,0,4,0.2,6,0.6l-6.4,6.4c-6.1,0.1-12,2.5-16.5,6.8c-0.8,0.8-0.8,2,0,2.8c0.8,0.8,2,0.8,2.8,0.1c2.6-2.5,5.7-4.2,9-5.1L9.7,50.8\n" +
    "		c-0.8,0.8-0.8,2,0,2.8c0.4,0.4,0.9,0.6,1.4,0.6s1-0.2,1.4-0.6l11.1-11.1c0.1,0.3,0.2,0.6,0.5,0.8c0.8,0.8,2,0.8,2.8,0.1\n" +
    "		c2-1.9,4.5-2.9,7.2-2.9c2.9,0,5.7,1.2,7.7,3.4c0.4,0.4,0.9,0.6,1.5,0.6c0.5,0,1-0.2,1.4-0.5c0.8-0.8,0.8-2,0.1-2.8\n" +
    "		c-2.8-3-6.6-4.6-10.6-4.6c-2,0-4,0.4-5.8,1.2l7-7c5.1,0.3,9.8,2.4,13.5,6.2c0.4,0.4,0.9,0.6,1.4,0.6c0.5,0,1-0.2,1.4-0.6\n" +
    "		c0.8-0.8,0.8-2,0.1-2.8c-3.5-3.6-7.9-6-12.7-7l5.7-5.7c4.5,1.6,8.6,4.2,12,7.7c0.4,0.4,0.9,0.6,1.4,0.6c0.5,0,1-0.2,1.4-0.6\n" +
    "		c0.8-0.8,0.8-2,0-2.8c-3.4-3.5-7.4-6.2-11.7-8l5.3-5.3c4.2,2.1,8,4.9,11.4,8.3c0.4,0.4,0.9,0.6,1.4,0.6c0.5,0,1-0.2,1.4-0.6\n" +
    "		C68,20.8,68,19.5,67.2,18.8z\"/><circle cx=\"34.1\" cy=\"51.2\" r=\"5.2\"/></g></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/revert.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/revert.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 21.8 22\" style=\"enable-background:new 0 0 21.8 22\" xml:space=\"preserve\"><path d=\"M21.8,17.9c0-2.2-1.8-4.1-4.1-4.1c-1.2,0-2.3,0.5-3.1,1.4l0.2-1.6c0-0.3-0.2-0.5-0.4-0.6c-0.3,0-0.5,0.2-0.6,0.4l-0.3,2.9\n" +
    "	c0,0.3,0.2,0.5,0.4,0.6l2.9,0.3c0,0,0,0,0.1,0c0.3,0,0.5-0.2,0.5-0.4c0-0.3-0.2-0.5-0.4-0.6L15.3,16c0.6-0.8,1.5-1.3,2.5-1.3\n" +
    "	c1.7,0,3.1,1.4,3.1,3.1S19.5,21,17.8,21H1V1h10c0,0,0,4.5,0,4.6c0,0.2,0.3,0.3,0.5,0.3H15V11c0,0.3,0.2,0.5,0.5,0.5S16,11.3,16,11\n" +
    "	V5.4c0,0,0-0.2-0.1-0.3L11.7,0H0.5C0.2,0,0,0.2,0,0.5v21C0,21.8,0.2,22,0.5,22h17.3C20,22,21.8,20.2,21.8,17.9z M12,1.9l2.4,3H12\n" +
    "	V1.9z\"/></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/up_to_date.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/up_to_date.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 62.5 43.8\" style=\"enable-background:new 0 0 62.5 43.8\" xml:space=\"preserve\"><path d=\"M22.5,42.3c-0.8,0-1.6-0.3-2.1-0.9L3.1,24.1c-1.2-1.2-1.2-3.1,0-4.2c1.2-1.2,3.1-1.2,4.2,0l15.2,15.2L55.2,2.4\n" +
    "	c1.2-1.2,3.1-1.2,4.2,0c1.2,1.2,1.2,3.1,0,4.2L24.7,41.4C24.1,42,23.3,42.3,22.5,42.3z\"/></svg>");
}]);

angular.module("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/update.svg", []).run(["$templateCache", function ($templateCache) {
  $templateCache.put("../../vendor/unityeditor-cloud-pack/assets/icons/svg/collab/update.svg",
    "<?xml version=\"1.0\" encoding=\"utf-8\"?> <svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\" viewBox=\"0 0 22.1 19.2\" style=\"enable-background:new 0 0 22.1 19.2\" xml:space=\"preserve\"><g><path d=\"M3.3,10.4l1.7-2c0.2-0.2,0.2-0.5,0-0.7c-0.2-0.2-0.5-0.2-0.7,0l-0.9,1C3.6,6,5.2,3.7,8,2.3c2-1,4.2-1.1,6.2-0.4\n" +
    "		c2,0.7,3.7,2.1,4.6,4.1L19,6.2c0.3,0.5,0.4,0.8,0.5,1.6c0,0.3,0.3,0.5,0.6,0.4c0.3,0,0.5-0.3,0.4-0.6c-0.1-0.9-0.3-1.4-0.6-1.9\n" +
    "		l-0.1-0.3c-2.2-4.5-7.6-6.3-12.2-4.1c-3.1,1.5-5,4.2-5.2,7.3l-1-0.9c-0.2-0.2-0.5-0.2-0.7,0c-0.2,0.2-0.2,0.5,0,0.7l1.9,1.8\n" +
    "		c0.1,0.1,0.2,0.2,0.3,0.2c0,0,0,0,0,0C3.1,10.6,3.2,10.5,3.3,10.4z\"/><path d=\"M21.5,13.7l-0.9-2.4c-0.1-0.3-0.4-0.4-0.6-0.3L17.5,12c-0.3,0.1-0.4,0.4-0.3,0.6c0.1,0.3,0.4,0.4,0.6,0.3l1.3-0.5\n" +
    "		c-0.7,1.6-2,3.5-4.1,4.5c-2,1-4.2,1.1-6.2,0.4c-2-0.7-3.7-2.2-4.7-4.3c-0.1-0.2-0.4-0.3-0.7-0.2c-0.2,0.1-0.4,0.4-0.2,0.7\n" +
    "		c1.1,2.3,3,4,5.3,4.8c1,0.3,2,0.5,3,0.5c1.4,0,2.7-0.3,4-0.9c2.3-1.1,3.8-3.2,4.6-5l0.5,1.2c0.1,0.2,0.3,0.3,0.5,0.3\n" +
    "		c0.1,0,0.1,0,0.2,0C21.4,14.2,21.6,13.9,21.5,13.7z\"/></g></svg>");
}]);
