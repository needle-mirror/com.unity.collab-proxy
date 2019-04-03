angular.module('ngUnity.errors', ['ngUnity.connectService', 'ngUnity.general', 'ngUnity.projectService', 'ut.components'])
.factory('httpErrorConfigs', ["defaultHttpErrors", "utAlert", function (defaultHttpErrors, utAlert) {
	// Mapping of methods to error handler
	return {
		GetProjectMembers: defaultHttpErrors.extendClone({
			400: 'You do not have permissions for this project.',
			403: 'You do not have permissions for this project.'
		}),

		SetMemberRole: defaultHttpErrors.extendClone({
			422: 'Could not change member role. Role already set.',
			404: 'Could not change member role. Member no longer exists.',
			'500-599': 'Unable to change member role.'
		}),

		InviteMember: defaultHttpErrors.extendClone({
			403: 'You do not have permissions to invite members to this project.',
			422: 'Could not invite member. User already exists.',
			404: 'Unable to invite member. User may not be registered.',
			'400-499': 'Unable to invite member. User may not be registered.',
			'500-599': 'Unable to invite member. User may not be registered.'
		}),

		RemoveProjectMember: defaultHttpErrors.extendClone({
			400: 'You do not have permissions to remove members from this project.',
			403: 'You do not have permissions to remove members from this project.',
			404: 'Could not remove user. User no longer exists.',
			'400-499': 'Unable to remove user.',
			'500-599': 'Unable to remove user.'
		}),

		RenameProject: defaultHttpErrors.extendClone({
			400: 'You do not have permissions to modify project settings.',
			403: 'You do not have permissions to modify project settings.',
			422: 'Project name already taken.',
			'400-499': 'Unable to rename project.',
			'500-599': 'Unable to rename project.'
		}),

		MoveProject: defaultHttpErrors.extendClone({
			400: 'You do not have permissions to modify project settings.',
			403: 'You do not have permissions to modify project settings.',
			422: 'Project name already taken in new organization.',
			'400-499': 'Unable to move project.',

			500: {message: '', handler: function(error, origin) {
					var message = 'Service unavailable. Internal server error.';

					if (error.data && error.data.detail && error.data.detail.match(/duplicate/i).length) {
						message = 'Project name already exists in new organization.';
					}

					utAlert.setError(message);
				}
			},

			'500-599': 'Unable to move project.'
		}),

		EnableProject: defaultHttpErrors.extendClone({
			403: 'You do not have permissions to create projects in this organization.',

			unknown: {
				message: '', handler: function (error, origin) {
					var message = error.message;
					if (message) {
						// Give more information if there was a local exception
						utAlert.setError("Project enabling failed with error: [" + message + '] while enabling organization.');
					} else {
						utAlert.setError("Project enabling failed with status: " + error.status);
					}
				}
			}
		})
	};
}])
.factory('defaultHttpErrors', ["unityConnectService", "MessageGroup", function (unityConnectService, MessageGroup) {
	// Refresh project when there is a possible difference with server  (eg: get updated project permissions...)
	function refreshProject(error, origin) {
		unityConnectService.IsReady().then(function() {
			var promise = unityConnectService.RefreshProject();

			if (promise) {
				promise.success(function () {});	// Promise needs to be used in order to run..
			}
		});
	}

	/**
	 * Perhaps only turn on when in dev mode? Useful to see error data, but could easily pollute console.
	 * @param error Error object. {data:, status:, headers:, config:}
	 * @param origin Object {method:}
	 */
	function report(error, origin) {
		console.log(origin.method + " failed : " + JSON.stringify(error));
	}

	/**
	 * Default Message group for Http Error Handling
	 */
	var httpErrors = new MessageGroup({
		400: {message: 'You do not have permissions for this action.', handler: refreshProject},
		401: {message: 'You do not have permissions for this action.', handler: refreshProject},
		403: {message: 'You do not have permissions for this action.', handler: refreshProject},

		402: 'Action failed. Payment Required.',
		404: 'Action failed. The requested resource could not be found.',
		405: 'Action failed. Method Not Allowed',
		406: 'Action failed. Resource creation not acceptable according to Accept header.',
		407: 'Action failed. Proxy Authentication Required.',
		408: 'Action failed. Request Timeout.',
		409: 'Action failed. Server reports a conflict.',
		410: 'Action failed. The resource is no longer available.',
		411: 'Action failed. The request did not specify the length of its content.',
		412: 'Action failed. The server does not meet one of the preconditions that the requester put on the request.',
		413: 'Action failed. The request is larger than the server is willing or able to process.',
		414: 'Action failed. The URI provided was too long for the server to process.',
		415: 'Action failed. Unsupported Media Type',
		416: 'Action failed. Requested Range Not Satisfiable. The client has asked for a portion of the file (byte serving), but the server cannot supply that portion.',
		417: 'Action failed. The server cannot meet the requirements of the Expect request-header field.',
		418: "Action failed. I'm a teapot.",
		419: 'Action failed. Authentication Timeout.',
		420: 'Action failed. Method Failure.',
		421: 'Action failed. Misdirected Request.',
		422: 'Action failed. Service unavailable.',
		423: 'Action failed. The resource that is being accessed is locked.',
		424: 'Action failed. Failed Dependency.',
		426: 'Action failed. Upgrade Required. The client should switch to a different protocol.',
		428: 'Action failed. Precondition Required.',
		429: 'Action failed. You have sent too many requests in a given amount of time. Please try again shortly.',
		431: 'Action failed. Request Header Fields Too Large.',
		440: 'Action failed. Login Timeout.',
		444: 'Action failed. No Response.',
		449: 'Action failed. Retry With.',
		450: 'Action failed. Blocked by Windows Parental Controls.',
		451: 'Action failed. Redirect.',
		494: 'Action failed. Request Header Too Large.',
		495: 'Action failed. Cert Error.',
		496: 'Action failed. No Cert.',
		497: 'Action failed. HTTP request sent to HTTPS.',
		498: 'Action failed. Token expired/invalid.',
		499: 'Action failed. The editor has closed the request.',

		'400-499': 'The request could not be processed by the server.',

		500: 'Service unavailable. Internal Server Error.',
		501: 'Service unavailable. Action Not Implemented.',
		502: 'Service unavailable. Bad Gateway.',
		503: 'Service Unavailable (because it is overloaded or down for maintenance). This may be temporary.',
		504: 'Service Unavailable. Gateway Timeout',
		505: 'Service Unavailable. HTTP Version Not Supported',
		506: 'Service Unavailable.',
		507: 'Service Unavailable. Server reported Insufficient Storage',
		508: 'Service Unavailable. Server reported Loop Detected',
		509: 'Service Unavailable. Bandwidth Limit Exceeded.',
		510: 'Service Unavailable. Server reported further extensions to the request are required for the server to fulfil it.',
		511: 'Service Unavailable. Network Authentication Required.',
		520: 'Service Unavailable. Unknown Error',
		522: 'Service Unavailable. Origin Connection Time-out.',
		598: 'Service Unavailable. Network read timeout error.',
		599: 'Service Unavailable. Network connect timeout error',
		
		'500-599': 'Unable to use cloud services.',

		unknown: 'Unknown error. Unable to use cloud services.',

		all: {handler: report}
	});

	return httpErrors;
}]);
