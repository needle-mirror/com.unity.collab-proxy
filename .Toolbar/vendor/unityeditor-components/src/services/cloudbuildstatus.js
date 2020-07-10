angular.module('ut.services.cloudbuild').factory('ucbStatusService', ["coreConfigService", "$q", "$log", "$resource", function (coreConfigService, $q, $log, $resource) {
  var service = {};
  var statusResource;
  coreConfigService.getConfigUrl('build_api_url').then(function (url) {
    statusResource = $resource(url + '/api/v1/status');
  });

  service.getStatus = function (projectDetails, buildTargets, billingPlan) {
    var dfd = $q.defer();
    var scmType = getScmType(projectDetails);
    var billingPlanLabel = getBillingPlanLabel(billingPlan);

    statusResource.query().$promise.then(function (data) {
      var statusMessage = null;
      data.forEach(function (message) {
        if (statusMessageApplies(message, scmType, buildTargets, billingPlanLabel) && isHigherPriority(statusMessage, message)) {
          statusMessage = message;
        }
      });
      dfd.resolve(statusMessage);
    }, function (error) {
      dfd.reject(error)
    });

    return dfd.promise;
  };

  function statusMessageApplies(statusMessage, scmType, buildTargets, billingPlanLabel) {
    if (statusMessage.scmType && scmType !== statusMessage.scmType) {
      return false;
    }
    if (statusMessage.billingPlan && billingPlanLabel !== statusMessage.billingPlan) {
      return false;
    }
    if (statusMessage.platform && angular.isUndefined(_.find(buildTargets, {platform:statusMessage.platform}))) {
      return false;
    }
    return true;
  }

  function getScmType(projectDetails) {
    if (projectDetails && projectDetails.settings && projectDetails.settings.scm) {
      return projectDetails.settings.scm.type;
    }
  }

  function getBillingPlanLabel(billingPlan){
    if(billingPlan && billingPlan.billing){
      return billingPlan.billing.label;
    }
  }

  function isHigherPriority(oldMessage, newMessage) {
    return !oldMessage || !oldMessage.priority || oldMessage.priority < newMessage.priority
  }

  return service;
}]);