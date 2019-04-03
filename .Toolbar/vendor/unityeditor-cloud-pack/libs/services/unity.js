angular.module('ngUnity.unity', [])

.factory('unityAsync', ["$window", function($window) {
	var object = $window.unityAsync || false;
	return object;
}])

.factory('unityObj', ["$q", "$timeout", "$window", "unityAsync", function ($q, $timeout, $window, unityAsync) {
	$window.onbeforeunload = function (e) {
		if (unityObj.m_isRemote){
			if (unityObj.timeoutID){
				clearTimeout(unityObj.timeoutID);
			}

			unityObj.closeSocket();
		}
	};

	var k_max_historyitem       = 100;
	var k_rmv_historyitem       = 20;

	var unityObj = {

		k_protocolVersion: "1.0",
		k_serviceName: 'json-rmc',
		k_typGetStubInfo: 'GETSTUBINFO',
		k_typInvoke: 'INVOKE',
		k_typOnEvent: 'ONEVENT',
		k_typGlobalEvent: 'GLOBALEVENT',
		k_connectionTimout: 5000,

		k_errNoDirectUnityObject: 'no-unity-object',
		k_remoteAddress: 'localhost',
		k_remotePort: 8789,

		m_requestID: 1,
		m_isRemote: (!unityAsync),
		m_wsConnection: null,

		m_requestCallbacks: {},
		m_eventCallbacks: {},
		m_waitingEvents: {},

		m_requestHistory: [],
		m_requestTimeouts: {},
		unityAsync: unityAsync,

		findRequestHistory : function (a_id){
		   return  _.find (this.m_requestHistory, function (itm){
				return itm.messageID == a_id;
			});
		},

		makeReplyData: function (a_exception,a_reply){
			return {exception:a_exception,serverReply:a_reply,originalCall:this.findRequestHistory(a_reply.messageID)};
		},

		cancelEvent: function (a_eventName){

			var _caller= this;

			_.find(this.m_eventCallbacks, function (itm, k){

				if (itm.event == a_eventName){
					delete _caller.m_eventCallbacks[k];
					return true;
				}
				return false;
			});


			_.find(this.m_waitingEvents, function (itm, k){

				if (itm.info.event==a_eventName){
					delete _caller.m_waitingEvents[k];
				}
				return true;
			});

			return false;
		},

		onDomainReload: function (){

			this.m_eventCallbacks = {};

			var waitingEventsCopy = _.map(this.m_waitingEvents, function (itm,k){
				return itm.callback;
			});

			this.m_waitingEvents = {};

			_.each (waitingEventsCopy, function (itm){
				itm({cancelled:true});
			});
				  
			//Fail all promises
			_.each(this.m_requestCallbacks, function (callback, key) {
				callback({},null);
			});
			
			this.m_requestTimeouts = {};
			this.m_requestCallbacks = {};
		},

		/**
		 *
		 */
		processResult: function (a_result){
			var robj= null;
			var _caller= this;
			try {
				robj = a_result;
				if (typeof a_result == "string"){
					robj= JSON.parse(a_result);
				}
			}catch(ex){
				console.error("Couldn't parse the reply from the unity engine",ex);
				return;
			}

			//Handle 'Domain Reload' event
			if (robj.type == this.k_typGlobalEvent) {
				this.onDomainReload();
				return;
			}

			if(this.m_requestTimeouts.hasOwnProperty(robj.messageID)) {
				delete this.m_requestTimeouts[robj.messageID];
			} else {
				//console.log("Request received after timeout, callback already triggered. ", robj.messageID);
				return;
			}

			var cbk= this.m_requestCallbacks[robj.messageID];

			if (cbk !== undefined){
				// remove callback from list
				delete this.m_requestCallbacks[robj.messageID];
			}else{
				cbk= this.m_eventCallbacks[robj.messageID];
			}

			if (robj.status < 0){
				if (cbk){
					cbk(robj, robj.message);
				} else {
					console.error("processResult error: " + JSON.stringify(robj));
				}
				return;
			}

			var cres= null;

			if (robj.type == this.k_typGetStubInfo){
				var stub= {};

				stub.events= robj.result.events;
				_.each(robj.result.properties, function (itm,k){
					stub[itm.name]= itm.value;
				});

				_.each(robj.result.methods, function (itm,k){
					stub[itm.name]= function(){

						var callback= null;

						// Check if last argument is a callback
						if (typeof arguments[arguments.length-1] == "function"){
							callback= arguments[arguments.length-1];
						}

						var callInfo= {
							type: _caller.k_typInvoke,
							destination: robj.reference,
							method: itm.name,
							params: []
						};

						for (var i=0; i< arguments.length-(callback?1:0); i++){
							callInfo.params.push(arguments[i]);
						}

						_caller.callProxyMgr(callInfo,callback);
					};
				});

				stub.on= function (a_event, a_callback){
					if (this.events.indexOf(a_event) == -1){
						throw "Cannot register to unknown event <"+a_event+">";
					}

					var callInfo= {
						type: _caller.k_typOnEvent,
						destination: robj.reference,
						event: a_event
					};

					_caller.callProxyMgr(callInfo,a_callback);
				};

				stub.off= function (a_eventName){
					_caller.cancelEvent(a_eventName);
				};

				cres= stub;
			}else if (robj.type == this.k_typInvoke){
				cres= robj.result;

			}else if (robj.type == this.k_typOnEvent){
				cres= robj.result;
			}


			try {
				if (cbk){
					cbk(null,cres);
				}
			}catch (e){
				console.error("An error occured in the callback for the request", this.makeReplyData(e,robj));
			}
		},

		/**
		 *
		 * @param a_callInfo
		 * @param a_callback
		 */
		callProxyMgr: function (a_callInfo,a_callback){
			var _caller= this;

			if (unityObj.m_isRemote){
				a_callback("remote mode not supported");
				return;
			}
			//console.log("calling unity: "+JSON.stringify(a_callInfo));

			a_callInfo.version= this.k_protocolVersion;
			a_callInfo.messageID= this.m_requestID++;

			if (a_callInfo.type==this.k_typOnEvent){
				this.m_eventCallbacks[a_callInfo.messageID]= a_callback;
				this.m_waitingEvents[a_callInfo.messageID]= {info:a_callInfo, callback: a_callback};
			}else{
				this.m_requestCallbacks[a_callInfo.messageID]= a_callback;
			}

			this.m_requestHistory.push(a_callInfo);

			if (this.m_requestHistory.length > k_max_historyitem){
				this.m_requestHistory.splice(0, k_rmv_historyitem);
			}


			var messageID = a_callInfo.messageID;
			this.m_requestTimeouts[messageID] = true;
			//console.log("Added ", messageID, " to request timeouts.");
			$timeout(function () {
				if (this.m_requestTimeouts.hasOwnProperty(messageID)) {
					//console.log("Canceling request ", messageID, ", timeout reached.");
					delete this.m_requestTimeouts[messageID];
					delete this.m_eventCallbacks[messageID];
					delete this.m_waitingEvents[messageID];
					delete this.m_requestCallbacks[messageID];
					a_callback({cancelled:true}, null);
				}
			}.bind(this), 17000);

			unityObj.unityAsync({
				className: 'window.webScriptObject',
				funcName: 'ProcessMessage',
				funcArgs: [JSON.stringify(a_callInfo)],
				onSuccess: function (response) {
					//console.log('processing result for: ', a_callInfo, response);
					unityObj.processResult(response);
				}
			});
		},

		/**
		 * getUnityObject
		 * retreive a stub to talk to a unity c# object
		 * @param a_reference
		 * @param a_callback
		 */
		getUnityObject : function (a_reference, a_callback){
			var _caller= this;

			var ocallInfo = {
				type: this.k_typGetStubInfo,
				reference: a_reference
			};

			this.callProxyMgr(ocallInfo,a_callback);
		},

		onEvent : function (event, data){
			console.log(event+" "+data);
		}

	};

	$window.unityObj= unityObj;
	$window.unityOnEvent = function(arg) {
		unityObj.processResult(arg);
	};

	return unityObj;
}]);
