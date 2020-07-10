angular.module('ngUnity.cloudcore', [
	'ngUnity.connectService',
	'ut.components',
	'ngUnity.errors',
	'ngUnity.general'
])

.factory('Organization', ["Roles", function (Roles) {
	/***************************************************************************
	 * Organization object, which allows to transfer to/from webauth and core's organizations formats.
	 * @constructor
	 ***************************************************************************/
	function Organization(name, foreign_key, id, billable_user_fk, role) {
		this.name = name;
		this.foreign_key = foreign_key;

		// Optional
		this.id = id;
		this.billable_user_fk = billable_user_fk;

		this.role = Roles.FromString(role);
	}

	Organization.FromOAuth2 = function (org) {
		return new Organization(org.name, org.foreign_key, org.id, org.billable_user_fk, org.role);
	};

	return Organization;
}])

.factory('RoleValues', function () {
	return {
		owner: 30,
		manager: 20,
		user: 10
	};
})

.factory('Roles', ["Role", "RoleValues", function (Role, RoleValues) {
	var Roles = _.mapValues(RoleValues, function(value, name) {
		return new Role(name, value);
	});

	Roles.FromString = function (roleName) {
		if (roleName instanceof Role) {
			return roleName;
		}

		return _.find(_.values(Roles), function (role) {
			return role.name.toLowerCase() == roleName.toLowerCase();
		});
	}

	return Roles;
}])

.factory('Role', ["RoleValues", function (RoleValues) {
	/***************************************************************************
	 * Role object describing an access role
	 * @constructor
	 ***************************************************************************/
	function Role(name, value) {
		this.name = name;
		this.value = value;
	}

	Role.prototype = {
		hasEditRights: function () {
			return this.value >= RoleValues.manager;
		}
	};

	return Role;
}])

.factory('ServiceFlags', ["$q", "$injector", "$rootScope", "cloudcoreUi", function ($q, $injector, $rootScope, cloudcoreUi) {
	/***************************************************************************
	 * ServiceFlags object, which gives the current service flags for project
	 * @constructor
	 ***************************************************************************/
	function ServiceFlags() {
		this._ready = $q.defer();
		this._fetching = false;

		this.fetched = false;
		this.clear();
	}

	ServiceFlags.prototype = {

		/**
		 * Gives a way to know if service flags have at least been fetched once.
		 */
		ready: function () {
			return this._ready.promise;
		},

		/**
		 * clear the state of the flags
		 */
		clear: function () {
			this.flags = undefined;
		},

		get: function(flagname) {
			if (!this.flags) {return;}

			return this.flags[flagname];
		},

		/**
		 * Update service flags
		 */
		update: function (projectInfo) {

			if (this._fetching) {
				return $q.when();
			}

			if (!projectInfo.projectBound || !projectInfo.projectGUID) {
				this.fetched = true;
				this.clear();
				$rootScope.$broadcast('servicesChanged', this.flags);
				this._ready.resolve(this);
				return $q.when();
			}

			this._fetching = true;

			return cloudcoreUi.GetServiceFlags(true).
				success(function (result) {
					this.flags = result.service_flags;
					$rootScope.$broadcast('servicesChanged', this.flags);
				}.bind(this)).
				error(function (response) {
					this.flags = {};
					$rootScope.$broadcast('servicesChanged', this.flags);
				}.bind(this)).
				final(function () {
					this.fetched = true;
					this._fetching = false;
					this._ready.resolve(this);
				}.bind(this));
		}
	};

	return ServiceFlags;
}])

.factory('Rights', function() {
	return {
		organization: 'org',
		project: 'project'
	};
})

.factory('Permissions', ["$q", "$injector", "Role", "cloudcoreUi", "Rights", "Roles", function ($q, $injector, Role, cloudcoreUi, Rights, Roles) {
	/***************************************************************************
	 * Permissions object, which gives the current user's permissions for project/organization
	 * @constructor
	 ***************************************************************************/
	function Permissions() {
		this._ready = $q.defer();
		this._fetching = false;
		this._unauthorized = false;		// Whether to allow fetching permissions on 403's (causes loops otherwise)

		this.fetched = false;
		this.clear();
	}

	Permissions.prototype = {
		_getSelf: function (members) {
			var unityConnectService = $injector.get('unityConnectService');			// DEPENDENCY LOOP!

			return _.find(members, function (member) {
				return member.email === unityConnectService.userInfo.userName;
			});
		},

		/**
		 * Gives a way to know if permissions have at least been fetched once        (eg: in resolvers).
		 */
		ready: function () {
			return this._ready.promise;
		},

		/**
		 * reset permissions
		 */
		clear: function () {
			this.project = undefined;		// Role
			this.organization = undefined;	// Role
			this.organizationAccess = false;// Boolean (true if granted by org instead of project)
		},

		/**
		 * Update permissions
		 */
		update: function (projectInfo) {

			if (this._fetching) {
				return;
			}
			if (this._unauthorized) {
				return;
			}

			if (!projectInfo.projectBound || !projectInfo.projectGUID) {
				this.fetched = true;
				this.clear();
				this._ready.resolve(this);
				return;
			}

			this._fetching = true;

			var project = {id: projectInfo.projectGUID, orgId: projectInfo.organizationId};

			var members = cloudcoreUi.GetProjectMembers(project, true).
				success(function (members) {
					var user = this._getSelf(members);
					if (user) {
						this.project = Roles.FromString(user.role);
						this.organizationAccess = user.access_granted_by === Rights.organization;
					}
				}.bind(this)).
				error(function () {
					// We already have an answer, Don't refresh again until user login changes.
					// Because an unauthorized response triggers a project state change which causes
					// another permissions refresh (and loops..).
					this._unauthorized = true;
				}.bind(this));

			var organization = cloudcoreUi.GetProjectOrganization().
				success(function (organization) {
					this.organization = organization ? organization.role : undefined;
				}.bind(this));

			$q.all([members, organization]).
				success(function (members) {
					// Used to make sure promise gets called
				}.bind(this)).
				final(function () {
					this.fetched = true;
					this._fetching = false;
					this._ready.resolve(this);
				}.bind(this));
		},

		userChanged: function () {
			this._unauthorized = false;				// Re-allow fetching on 403's
		},

		hasEditRights: function () {
			if (!this.project) {
				return false;
			}
			return this.project.hasEditRights();
		}
	};

	return Permissions;
}])

.factory('cloudcore', ["$q", "$cookies", "$http", "$localStorage", "unityConnectService", "Organization", "Roles", "unityProjectService", function ($q, $cookies, $http, $localStorage, unityConnectService, Organization, Roles, unityProjectService) {
	var service = {};

	/**
	 *
	 */
	function createExpiryFromDeltaTime(deltaMinutes) {
		var expiry = new Date();
		expiry.setMinutes(expiry.getMinutes() + deltaMinutes);

		var options = {
			'expires': expiry
		};

		return options;
	}

	/**
	 *
	 */
	function orderProjects(projects) {
		projects = projects.filter(function (project) {
			return !project.archived && project.active;
		});

		projects.sort(function (project, otherProject) {
			return project.name.localeCompare(otherProject.name);
		});

		return projects;
	}

	/**
	 * Get organizations that user is a member of AND that user has a project with
	 * @param forceRefresh
	 * @returns {promise}
	 */
	service.GetOrganizations = function (forceRefresh) {
		if (forceRefresh === undefined || forceRefresh === false) {
			var organizations = $cookies.getObject("organizations");
			if (organizations !== undefined) {
				return $q.when(organizations);
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var full_url = unityConnectService.urls.core + "/api/orgs";

			return $http.get(full_url).then(
			function success(response) {
				response.data.orgs.sort(function (organization, otherOrganization) {
					return organization.name.localeCompare(otherOrganization.name);
				});

				$cookies.putObject("organizations", response.data.orgs, createExpiryFromDeltaTime(15));
				return response.data.orgs;
			},
			function error(response) {
				$cookies.remove("organizations");
				return $q.reject(response);
			});
		});
	};

	/**
	 *
	 * @param project
	 * @param forceRefresh
	 * @returns {promise}
	 */
	service.GetProjectMembers = function (project, forceRefresh) {
		if (forceRefresh === undefined || forceRefresh === false) {
			var canReadMembers = $cookies.get(project.id + "-members");
			if (canReadMembers !== undefined) {
				var members = $localStorage[project.id + "-members"];
				if (members !== undefined) {
					return $q.when(members);
				}
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var projectId = project.id || unityConnectService.projectInfo.projectGUID;
			var org = project.orgId || unityConnectService.projectInfo.organizationId;

			if (!org) {
				return $q.reject('No valid organization id present to retrieve project members.');
			}

			var config = {
				ignoreLoadingBar: true
			};

			var full_url = unityConnectService.urls.core + "/api/orgs/" + org + "/projects/" + projectId + "/users";

			return $http.get(full_url, config).then(
				function success(response) {
					$cookies.put(project.id + "-members", "InLocalStorage", createExpiryFromDeltaTime(15));
					$localStorage[project.id + "-members"] = response.data.users;

					return response.data.users;
				},
				function error(response) {
					$cookies.remove(project.id + "-members");
					delete $localStorage[project.id + "-members"];

					return $q.reject(response);
				});
		});
	};

	/**
	 *
	 * @param member
	 * @param role
	 * @returns {promise}
	 */
	service.SetMemberRole = function (member, role) {
		return unityConnectService.IsReady().then(function (ready) {
			var project = unityConnectService.projectInfo.projectGUID;
			var org = unityConnectService.projectInfo.organizationId

			var json = {
				role: role
			};

			var full_url = unityConnectService.urls.core + "/api/orgs/" + org + "/projects/" + project + "/users/" + member.foreign_key;

			return $http.put(full_url, json).then(
			function success(response) {
				$cookies.remove(project.id + "-members");
				delete $localStorage[project.id + "-members"];
			},
			function error(response) {
				$cookies.remove(project.id + "-members");
				delete $localStorage[project.id + "-members"];

				return $q.reject(response);
			});
		});
	};

	/**
	 *
	 * @param email
	 * @param role
	 * @returns {promise}
	 */
	service.InviteMember = function (email, role) {
		return unityConnectService.IsReady().then(function (ready) {
			var project = unityConnectService.projectInfo.projectGUID;
			var org = unityConnectService.projectInfo.organizationId

			var json = {
				email: email,
				role: role
			};

			var full_url = unityConnectService.urls.core + "/api/orgs/" + org + "/projects/" + project + "/users";

			return $http.post(full_url, json).then(
			function success(response) {
				return response.data;
			});
		});
	};

	/**
	 *
	 * @param member
	 * @returns {promise}
	 */
	service.RemoveProjectMember = function (member) {
		return unityConnectService.IsReady().then(function (ready) {
			var project = unityConnectService.projectInfo.projectGUID;
			var org = unityConnectService.projectInfo.organizationId

			var full_url = unityConnectService.urls.core + "/api/orgs/" + org + "/projects/" + project + "/users/" + member.foreign_key;

			return $http.delete(full_url).then(
			function success(response) {
				$cookies.remove(project.id + "-members");
				delete $localStorage[project.id + "-members"];

				return response.data;
			},
			function error(response) {
				$cookies.remove(project.id + "-members");
				delete $localStorage[project.id + "-members"];

				return $q.reject(response);
			});
		});
	};

	/**
	 * Get organizations that I am a member of
	 *
	 * @returns {promise}
	 */
	service.GetMemberOrganizations = function () {
		return unityConnectService.IsReady().then(function (ready) {
			var config = {
				ignoreLoadingBar: true
			};

			var full_url = unityConnectService.urls.core + "/api/users/me?include=orgs";

			return $http.get(full_url, config).then(
				function success(response) {
					var orgs = [];
					if (response.data.orgs) {
						orgs = response.data.orgs.map(Organization.FromOAuth2);
					}
					return orgs;
				}
			);
		});
	};

	/**
	 * Get Organizations using a filter
	 * @param filter Function(organization)
	 */
	service.GetFilteredMemberOrganizations = function (filter) {
		return service.GetMemberOrganizations().success(function (organizations) {
			return organizations.filter(filter);
		});
	};

	/**
	 * Get Organization of a project
	 */
	service.GetProjectOrganization = function () {
		return service.GetMemberOrganizations().success(function (organizations) {
			var org = organizations.filter(function (organization) {
				return organization.id === unityConnectService.projectInfo.organizationId;
			});

			// BAD FIX: Check name if id does not match!!!! SEA issue
			if (org.length == 0) {
				org = organizations.filter(function (organization) {
					return organization.name === unityConnectService.projectInfo.organizationId;
				});
			}

			return _.head(org);
		});
	};

	/**
	 * Get Organizations where user has same rights (or higher) then current project
	 */
	service.GetOrganizationsByProjectRole = function () {
		var projectRole = unityConnectService.projectInfo.permissions.project;

		return service.GetFilteredMemberOrganizations(function (organization) {
			return organization.role.value >= projectRole.value;
		});
	};

	/**
	 * Get organizations in which the current user is a manager
	 */
	service.GetMemberManagerOrganizations = function () {
		return service.GetFilteredMemberOrganizations(function (organization) {
			return organization.role.value >= Roles.manager.value;
		});
	};

	/**
	 *
	 * @param projectName
	 * @returns {promise}
	 */
	service.RenameProject = function (projectName) {
		if (unityConnectService.projectInfo.projectName == projectName) {
			$q.when(204);
		}

		return unityConnectService.IsReady().then(function (ready) {
			var project = unityConnectService.projectInfo.projectGUID;
			var org = unityConnectService.projectInfo.organizationId;
			var url = unityConnectService.urls.core;

			var json = {
				name: projectName
			};

			var full_url = url + "/api/projects/" + project;

			return $http.put(full_url, json).then(
			function success(response) {
				var putStatus = response.status;

				if (putStatus == 204) {
					full_url = url + "/api/projects/" + project;

					return $http.get(full_url, response.config).then(
					function success(response) {
						var projectInfo = response.data;

						return unityConnectService._BindProject(project, projectInfo.name, org).
						success(function () {
							unityConnectService.projectInfo.projectName = projectInfo.name;

							return putStatus;
						});
					}
					);
				} else {
					return $q.reject(response);
				}
			}
			);
		});
	};

	/**
	 *
	 * @param organization
	 * @returns {promise}
	 */
	service.MoveProject = function (organization) {
		if (unityConnectService.projectInfo.organizationId == organization.id) {
			$q.when(204);
		}

		return unityConnectService.IsReady().then(function (ready) {
			return unityConnectService.GetCoreConfigurationUrl().
				success(function (url) {
					var project = unityConnectService.projectInfo.projectGUID;
					var org = unityConnectService.projectInfo.organizationId;

					var json = {
						destination_org_fk: organization.foreign_key
					};

					var full_url = url + "/api/orgs/" + org + "/projects/" + project + "/move";

					return $http.put(full_url, json).then(
						function success(response) {
							unityConnectService.projectInfo.organizationId = organization.id;

							return response.status;
						}
					);
				});
		});
	};


	/**
	 *
	 * @param organizationId
	 * @returns {promise}
	 */
	service.EnableProject = function (organizationId) {
		return unityProjectService.IsReady().then(function () {
			return unityProjectService.GetProjectName().then(function (projectName) {
				return TryEnableProject(projectName, organizationId, 0);
			});
		});
	}

	function TryEnableProject(projectName, organizationId, suffixNumber) {
		var json = {
			name: projectName + ((suffixNumber == 0) ? "" : " (" + suffixNumber + ")"),
			active: true
		};

		var full_url = unityConnectService.urls.core + "/api/orgs/" + organizationId + "/projects";

		return $http.post(full_url, json).then(
		function success(response) {
			var projectInfo = response.data;

			if (response.status == 201) {
				return unityConnectService._BindProject(projectInfo.guid, projectInfo.name, projectInfo.org_id).
				success(function () {
					unityConnectService.projectInfo.projectGUID = projectInfo.guid;
					unityConnectService.projectInfo.projectName = projectInfo.name;
					unityConnectService.projectInfo.organizationId = projectInfo.org_id;
					unityConnectService.projectInfo.projectBound = true;

					return true;
				});
			} else {
				return $q.reject(response);
			}
		},
		function error(response) {
			if (response.status == 422) {
				return TryEnableProject(projectName, organizationId, suffixNumber + 1);
			} else {
				unityConnectService.projectInfo.projectBound = false;
				return $q.reject(response);
			}
		});
	}

	/**
	 *
	 * @param organizationId
	 * @param forceRefresh
	 * @returns {promise}
	 */
	service.GetProjects = function (organizationId, forceRefresh) {
		if (forceRefresh === undefined || forceRefresh === false) {
			var canReadProjects = $cookies.get(organizationId + "-projects");
			if (canReadProjects !== undefined) {
				var projects = $localStorage[organizationId + "-projects"];
				if (projects !== undefined) {
					return $q.when(projects);
				}
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var full_url = unityConnectService.urls.core + "/api/orgs/" + organizationId + "/projects";

			return $http.get(full_url).then(
			function success(response) {
				var projects = orderProjects(response.data.projects);

				$cookies.put(organizationId + "-projects", "InLocalStorage", createExpiryFromDeltaTime(15));
				$localStorage[organizationId + "-projects"] = projects;

				return projects;
			},
			function error(response) {
				$cookies.remove(organizationId + "-projects");
				delete $localStorage[organizationId + "-projects"];

				return $q.reject(response);
			});
		});
	};

	/**
	 *
	 * @param projectGUID
	 * @param forceRefresh
	 * returns {promise}
	 */
	service.GetProject = function (projectGUID, forceRefresh) {
		if (forceRefresh === undefined || forceRefresh === false) {
			var canReadProject = $cookies.get(projectGUID);
			if (canReadProject !== undefined) {
				var project = $localStorage[projectGUID];
				if (project !== undefined) {
					return $q.when(project);
				}
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var full_url = unityConnectService.urls.core + "/api/projects/" + projectGUID;

			return $http.get(full_url).then(
			function success(response) {
				var project = response.data;

				$cookies.put(projectGUID, "InLocalStorage", createExpiryFromDeltaTime(15));
				$localStorage[projectGUID] = project;

				return project;
			},
			function error(response) {
				$cookies.remove(projectGUID);
				delete $localStorage[projectGUID];

				return $q.reject(response);
			});
		});
	};

	/**
	 *
	 */
	service.InvalidateCachedProject = function (projectGUID) {
 		$cookies.remove(projectGUID);
	    delete $localStorage[projectGUID];
	}

	/**
	 *
	 * @param forceRefresh
	 * @returns {promise}
	 */
	service.GetAllProjects = function (forceRefresh) {
		if (forceRefresh === undefined || forceRefresh === false) {
			var canReadProjects = $cookies.get("projects");
			if (canReadProjects !== undefined) {
				var projects = $localStorage.projects;
				if (projects !== undefined) {
					return $q.when(projects);
				}
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var full_url = unityConnectService.urls.core + "/api/projects";

			return $http.get(full_url).then(
				function success(response) {
					var projects = orderProjects(response.data.projects);

					$cookies.put("projects", "InLocalStorage", createExpiryFromDeltaTime(15));
					$localStorage.projects = projects;

					return projects;
				},
				function error(response) {
					$cookies.remove("projects");
					delete $localStorage.projects;

					return $q.reject(response);
				});
		});
	};

	/**
	 *
	 * @param organization
	 * @param forceRefresh
	 * @returns {promise}
	 */
	service.GetOrganizationMembers = function (organization, forceRefresh) {
		if (forceRefresh === undefined || forceRefresh === false) {
			var canReadMembers = $cookies.get(organization.id + "-members");
			if (canReadMembers !== undefined) {
				var members = $localStorage[organization.id + "-members"];
				if (members !== undefined) {
					return $q.when(members);
				}
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var full_url = unityConnectService.urls.core + "/api/orgs/" + organization.id + "/users";

			return $http.get(full_url).then(
				function success(response) {
					$cookies.put(organization.id + "-members", "InLocalStorage", createExpiryFromDeltaTime(15));
					$localStorage[organization.id + "-members"] = response.data.users;

					return  response.data.users;
				},
				function error(response) {
					$cookies.remove(organization.id + "-members");
					delete $localStorage[organization.id + "-members"];

					return $q.reject(response);
				});
		});
	};


	/**
	 * Returns whether the user has all the permissions of a 'Team seat' for the current project's organisation.
	 * @return {Promise} The promise is rejected on network failures, else returns a boolean.  Defaults to true for any non-network issues.
	 * https://unity3d.com/teams
	 * https://confluence.hq.unity3d.com/display/OS/Unity+Teams
	 */
	service.CheckTeamSeats = function (priviligedOnly) {
		return service.getConfigEntry('seat_required').then(function success(seatRequired) {
			if(!!seatRequired) {
				return unityConnectService.IsReady().then(function () {
					var full_url = unityConnectService.urls.identity + "/v1/entitlements";
					var config = {
						params: {
							userId: unityConnectService.userInfo.userId,
							assignFrom: unityConnectService.projectInfo.organizationForeignKey,
							isActive: true,
							namespace: 'unity_teams',
							type: 'TEAMS',
							tag: ['UnityTeamsFree', 'UnityTeamsStandard', 'UnityTeamsPro']
						}
					};

					return $http.get(full_url, config).then(
						function success(response) {
							var licenses = _.map(response.data.results, 'tag');
							if (licenses.length > 0) {
								if (priviligedOnly) {
									return !!_.intersection(licenses, ['UnityTeamsStandard', 'UnityTeamsPro']).length;
								}
								return true;
							}
							return false;
						},
						function error() {
							return $q.reject('Issue fetching entitlements');
						});
				});
			}

			if (typeof seatRequired === 'undefined') {
				return $q.reject('Issue fetching configuration');
			}
			else {
				return true;
			}
		}, function error() {
				return $q.reject('Issue fetching configuration');
		});
	};

	/**
	 *
	 * @param compliance
	 * @returns {promise}
	 */
	service.SetCOPPACompliance = function (compliance) {
		return unityConnectService.IsReady().then(function (ready) {
			var project = unityConnectService.projectInfo.projectGUID;
			var org = unityConnectService.projectInfo.organizationId;

			var json = {
				coppa: compliance
			};

			var full_url = unityConnectService.urls.core + "/api/orgs/" + org + "/projects/" + project + "/coppa";

			return $http.put(full_url, json).then(
				function success(response) {
					if (response.status == 204) {
						var compCode = 0;
						if (compliance == "compliant") {
							compCode = 1;
						} else if (compliance == "not_compliant") {
							compCode = 2;
						}

						return unityConnectService.SetCOPPACompliance(compCode).
							success(function () {
								unityConnectService.projectInfo.COPPA = compliance;
							});
					} else {
						return $q.reject(response);
					}
				});
		});
	};

	/**
	 *
	 * @returns {promise}
	 */
	service.InvalidateCachedServiceFlags = function () {
		var projectId = unityConnectService.projectInfo.projectGUID;
		var defer = $q.defer();
		if (!projectId) {
			return defer.reject('No projectId found');
		}

		$cookies.remove(projectId + "-serviceflags");
		$cookies.remove(projectId + "-serviceflags-request");
		delete $localStorage[projectId + "-serviceflags"];
		nextCookieRetryTimePos = 0;
		defer.resolve();

		return defer.promise;
	};

	var nextCookieRetryTimeOnErrors = [1, 2, 4, 7, 10, 15];
	var nextCookieRetryTimePos = 0;

	/**
	 *
	 * @returns {promise}
	 */
	service.GetServiceFlags = function (forceRefresh) {
		var projectId = unityConnectService.projectInfo.projectGUID;
		if (!projectId) {
			var response = {
				status: 422,
				statusText: "No valid project id present to retrieve services flags"
			};
			return $q.reject(response);
		}

		if (forceRefresh === undefined || forceRefresh === false) {
			var canReadServiceFlags = $cookies.get(projectId + "-serviceflags");
			var serviceFlagsRequestSent = $cookies.get(projectId + "-serviceflags-request");

			if (canReadServiceFlags !== undefined || serviceFlagsRequestSent !== undefined) {
				var flags = $localStorage[projectId + "-serviceflags"];
				if (flags !== undefined) {
					return $q.when(flags);
				}
			}
		}

		return unityConnectService.IsReady().then(function (ready) {
			var config = {
				ignoreLoadingBar: true
			};

			var full_url = unityConnectService.urls.core + "/api/projects/" + projectId + "/service_flags";

			$cookies.put(projectId + "-serviceflags-request", "Sent", createExpiryFromDeltaTime(1));
			return $http.get(full_url, config).then(
				function success(response) {
					$cookies.remove(projectId + "-serviceflags-request");
					$cookies.put(projectId + "-serviceflags", "InLocalStorage", createExpiryFromDeltaTime(15));
					$localStorage[projectId + "-serviceflags"] = response.data;
					nextCookieRetryTimePos = 0;

					return response.data;
				},
				function error(response) {
					$cookies.remove(projectId + "-serviceflags-request");
					$cookies.put(projectId + "-serviceflags", "InLocalStorage", createExpiryFromDeltaTime(nextCookieRetryTimeOnErrors[nextCookieRetryTimePos]));

					if (nextCookieRetryTimePos < (nextCookieRetryTimeOnErrors.length-1))
					{
						nextCookieRetryTimePos++;
					}

					// Do not remove localStorage, re-use last ones
					var flags = $localStorage[projectId + "-serviceflags"];
					if (flags !== undefined)
					{
						return flags;
					}

					var unknownFlags = {
						service_flags: {}
					};

					$localStorage[projectId + "-serviceflags"] = unknownFlags;
					return unknownFlags;
				});
		});
	};

	/**
	 *
	 * @param flag
	 * @param enabled
	 * @returns {promise}
	 */
	service.UpdateServiceFlag = function (flag, enabled) {
		return unityConnectService.IsReady().then(function (ready) {
			var projectId = unityConnectService.projectInfo.projectGUID;
			if (!projectId) {
				var response = {
					status: 422,
					statusText: "No valid project id present to update services flags"
				};
				return $q.reject(response);
			}

			var json = {
				service_flags: {}
			};
			json.service_flags[flag] = enabled;

			if (flag === "") {
				return $q.when(json);
			} else {
				var full_url = unityConnectService.urls.core + "/api/projects/" + projectId + "/service_flags";

				return $http.put(full_url, json).then(
					function success(response) {
						var canWriteServiceFlags = $cookies.get(projectId + "-serviceflags");
						if (canWriteServiceFlags !== undefined) {
							var flags = $localStorage[projectId + "-serviceflags"];
							if (flags !== undefined) {
								flags.service_flags[flag] = enabled;
								$localStorage[projectId + "-serviceflags"] = flags;
							}
						}

						return response.data;
					},
					function error(response) {
						return $q.reject(response);
					});
			}
		});
	};

    service.configEntries = null;

    service.getConfigEntry = function (entryName){
        var deferred = $q.defer();

        if (service.configEntries !== null){
            deferred.resolve(service.configEntries[entryName]);
        } else {
            unityConnectService.IsReady().then(function() {
                var k_configUrl = "https://public-cdn.cloud.unity3d.com/config/" + unityConnectService.configuration;
                var config = {
                    ignoreLoadingBar: true,
                    cache: false
                };
                $http.get(k_configUrl, config).then(
                    function success(configs) {
                        service.configEntries = configs.data;
                        deferred.resolve(service.configEntries[entryName]);
                    },
                    function error(data, status, headers, config) {
                        deferred.reject();
                    }
                );
            });
        }

        return deferred.promise;
    };

	return service;
}]);
