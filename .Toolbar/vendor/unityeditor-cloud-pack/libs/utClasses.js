

var ut = {};

// --------------------------------------------------------------------------------
// legalPanelInfo class
// --------------------------------------------------------------------------------
ut.panelInfo = function (panelService, title, description,webUrl, unityConnectService) {

  this.panelService= panelService;
  this.hideEnableSwitch= panelService.hideEnableSwitch;
  this.enabled= !panelService.hideEnableSwitch;
  this.title= title;
  this.goToWebUrl= webUrl;
  this.description= description;
  this.isHub= false;
  this.unityConnectService = unityConnectService;
  this.pendingRequest = false;
};

ut.panelInfo.prototype.getDashboardLink = function (){
  return this.goToWebUrl;
};


ut.panelInfo.prototype.setEnabled =  function (){
    if (this.pendingRequest) {
      return;
    }

    this.pendingRequest = true;
    var value = !this.enabled;
    var _caller = this;
    if (this.unityConnectService) {
        this.unityConnectService.UpdateServiceFlag(this.panelService.serviceFlag, value)
            .success(function(flags) {
                return _caller.panelService.EnableService(value);
            })
            .error(function(status) {
            })
            .final(function() {
                _caller.pendingRequest = false;
            })
        return;
    }
    
    this.panelService.EnableService(value);
    this.pendingRequest = false;
};


ut.panelInfo.prototype.cancelLegal = function (){};
ut.panelInfo.prototype.confirmedLegal= function (){};


// --------------------------------------------------------------------------------
// legalPanelInfo class
// --------------------------------------------------------------------------------
ut.legalPanelInfo = function (panelService, title, description,webUrl, unityConnectService) {

	this.panelService= panelService;
	this.enabled= false;
    this.preEnable= false
    this.complianceSet= false;
    this.title= title;
    this.goToWebUrl= webUrl;
    this.description= description;
    this.unityConnectService = unityConnectService;
    this.pendingRequest = false;
};

ut.legalPanelInfo.prototype.getDashboardLink = function (){
	return this.goToWebUrl;
};


ut.legalPanelInfo.prototype.setEnabled =  function (){
    var _caller= this;

    if (this.preEnable){
        this.cancelLegal();
        return;
    }

    if (!this.enabled && !this.complianceSet){
      this.preEnable= true; 
    }else {

      if (this.pendingRequest) {
        return;
      }

      this.pendingRequest = true;
      var value = !this.enabled;
      if (_caller.unityConnectService) {
          _caller.unityConnectService.UpdateServiceFlag(this.panelService.serviceFlag, value)
              .success(function(flags) {
                  _caller.panelService.EnableService(value);
              })
              .error(function(status) {
              })
              .final(function() {
                  _caller.pendingRequest = false;
              })
          return;
      }
      
      _caller.panelService.EnableService(value);
      _caller.pendingRequest = false;

    }
};


ut.legalPanelInfo.prototype.cancelLegal = function (){
  this.preEnable= false;
  this.enabled= false;
};

ut.legalPanelInfo.prototype.confirmedLegal= function (){
  var _caller= this;

  if (this.pendingRequest) {
    return;
  }

  this.complianceSet= true;
  this.preEnable= false;
  this.enabled= true;

  this.pendingRequest = true;
  var value = !this.enabled;
  if (_caller.unityConnectService) {
    _caller.unityConnectService.UpdateServiceFlag(this.panelService.serviceFlag, true)
      .success(function(flags) {
         return  _caller.panelService.EnableService(true);
      })
      .error(function(status) {
      })
      .final(function() {
          _caller.pendingRequest = false;
      })
    return;
  }
};