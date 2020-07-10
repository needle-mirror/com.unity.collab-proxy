angular.module('ngUnity.pout', [])
/**
 * Web services error handling decoration
 */
.factory('cloudbuildUi', ["errorHandlerInterface", "cloudbuild", "ucPromise", function (errorHandlerInterface, cloudbuild, ucPromise) {
  var cloudcoreUi = {};

  /**
   * Decorate cloudcore methods with default error handling.
   */
  var methods = _.keys(cloudcore);

  //
  // Decorate service generically with error handler
  var decorated = methods.map(function (methodName) {
    var method = cloudcore[methodName];

    cloudcoreUi[methodName] = function genericRestErrorHandler() {
      var promise = method.apply(cloudcore, arguments);

      promise = ucPromise.decoratePromise(errorHandlerInterface.bind(this, {method: methodName}), promise);

      // Always handle errors
      var handled = promise.handleErrors();

      return handled;
    };
  });

  return cloudcoreUi;
}]);
