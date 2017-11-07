/*
 * Function to check if the device is an apple mobile prdouct (ipad/iphone/ipod)
 * returns true if its compatible with apple product (ipad/iphone/ipod)and false if not 
 */
function isBrowseriDeviceCompatible(){

	var mobileBrowser=["iphone","ipod","ipad"];
	var uagent = navigator.userAgent.toLowerCase();
	var length=mobileBrowser.length;
	for(i=0;i<length;i++){
		if (uagent.search(mobileBrowser[i]) > -1 && uagent.search('webkit') > -1){
		
			return true;
			
		}
	}

	return false;
}
function KitCloudAPIController() {
	this.parseQueryString= function(name) {	
		name = name.replace(/[\[]/,"\\[").replace(/[\]]/,"\\]"); 
		var regex = new RegExp("[\?&|&amp;]"+name+"=([^&#]*)"); 
		var results = regex.exec(window.location.href);
		return results == null ? "" : results[1];
	}
	this.parseQueryStringWithDefault= function(name, defVal) {	
		name = name.replace(/[\[]/,"\\[").replace(/[\]]/,"\\]"); 
		var regex = new RegExp("[\?&|&amp;]"+name+"=([^&#]*)"); 
		var results = regex.exec(window.location.href);
		return results == null ? defVal : results[1];
	}
	
	this.parseQueryStringArray= function(params) {
		try{
			for(i in params){
				name=params[i];				
				name = name.replace(/[\[]/,"\\[").replace(/[\]]/,"\\]"); 
				var regex = new RegExp("[\?&|&amp;]"+name+"=([^&#]*)"); 
				var results = regex.exec(window.location.href);
				if(results!=null){
					return  results[1];
				}
			}
		}catch(err){
			return "";
			console.log(err);
		}
		return "";
	}
}

var KitCloudAPI = new KitCloudAPIController();


function addCustomScriptsforHTML5Live(playerURL){
	    if(!playerURL){
	    	//to be changed
	    	playerURL="http://mediasuite.multicastmedia.com";
	    }
		var s='';
		s=s+'<script type="text/javascript" src="'+playerURL+'/jsapi/livePlayer.js"></script>';
		s=s+'<script type="text/javascript" src="'+playerURL+'/jsapi/3/jquery.countdown.js"></script>';
		if(AKAMAI_MEDIA_ANALYTICS_CONFIG_FILE_PATH && AKAMAI_MEDIA_ANALYTICS_CONFIG_FILE_PATH != ''){
			s=s+"<script type='text/javascript' src='"+playerURL+"/jsapi/akamaihtml5-min.js'></script>";
		}
	    $("head").append(s);
}
