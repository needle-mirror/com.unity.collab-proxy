angular.module('ngUnity.editorcollab', [
	'ngUnity.unityService',
	'ngUnity.projectService',
	'ngUnity.cloudPanelService',
	'ngUnity.cloudcore',
	'ut.services.notification'
])
.provider('editorcollab', function (){
	this.ShouldPollConflicts = true;

	this.EnableConflictsPolling = function (enable) {
		this.ShouldPollConflicts = enable;
	}.bind(this);

	var self = this;
	return {
		$get: ["editorCollabNative", "editorCollabREST", function (editorCollabNative, editorCollabREST){
			var service = editorCollabNative;

			service.ShouldPollConflicts = self.ShouldPollConflicts;
			service.GetMainError = editorCollabREST.GetMainError;

			service.ready(function(err){
				// These are now handled by CEF Native endpoints
				service.IsReady = editorCollabNative.IsReady;
				service.States = editorCollabNative.States;
				service.getCollabState = editorCollabNative.getCollabState;
				service.cancelJob = editorCollabNative.cancelJob;
				service.enableCollab = editorCollabNative.enableCollab;
				service.GetCollabConflicts = editorCollabNative.GetCollabConflicts;
				service.ResyncToTip = editorCollabNative.ResyncToTip;
				service.SeeAll = editorCollabNative.SeeAll;
				service.ready = editorCollabNative.ready;

				// These are now handled by Unity REST API endpoints
				service.GetChangesToPublish = editorCollabREST.GetChangesToPublish;
				service.GetChangesToPublishHash = editorCollabREST.GetChangesToPublishHash;
				service.SetSeatAvailable = editorCollabREST.SetSeatAvailable;
				service.ShouldStartPolling = true;
			});
			return service;
		}],
		EnableConflictsPolling: this.EnableConflictsPolling
	}
});
