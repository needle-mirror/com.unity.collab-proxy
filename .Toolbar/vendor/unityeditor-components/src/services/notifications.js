angular.module('ut.services.notification', [
    'ut.routes'
  ])
  .service('notifications', ["pubnubNotifications", "socketNotifications", function(pubnubNotifications, socketNotifications) {
    this.channels = {};
    this.backends = {
      PUBNUB_BACKEND: pubnubNotifications,
      SOCKET_BACKEND: socketNotifications
    };

    this.init = function(backend, dependencyResolver) {
      _.extend(this.channels, backend.channels);
      dependencyResolver().then(function(deps) {
        backend.init(deps);
      }.bind(this));
    }.bind(this);

    this.subscribe = function(channel, messageCallback) {
      for(var backend in this.backends) {
        if(_.includes(this.backends[backend].channels, channel)) {
          this.backends[backend].subscribe(channel, messageCallback);
        }
      }
    }.bind(this);
  }]);
