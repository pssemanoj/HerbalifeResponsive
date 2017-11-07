(function () {
    "use strict";
    // Add ECMA262-5 Array methods if not supported natively
    //
    if ((typeof Array.prototype.indexOf === "undefined")) {
        Array.prototype.indexOf = function (find, i) {
            var n;
            i = (i === undefined) ? 0 : i;
            if (i < 0) i += this.length;
            if (i < 0) i = 0;
            for (n = this.length; i < n; i++)
                if (i in this && this[i] === find)
                    return i;
            return -1;
        };
    }
}());
(function(window, environment) {
    "use strict";
    var document = window.document,
        libraries = {
            jquery: false,
            swfObject: false
        },
        swfObject = window.swfObject,
        $ = window.$,
        CONFIG = {
            //BASE_URL : "//player.piksel.com",
            BASE_URL : "//player.piksel.com",
            ANALYTICS_BEACON : "http://ma198-r.analytics.edgesuite.net/config/beacon-4896.xml?beaconVersion=1.1",
//            ANALYTICS_BEACON : "https://video.herbalife.com/config/beacon-4896.xml",
            API: "7ccba1e8-1592-102b-8824-000c291cbe49",
            API_VERSION: "4.0",
            EXPRESS_INSTALL: "http://www.appstudio.kitd.com/kickFlash/scripts/expressInstall2.swf?2",
            IFRAME: '<iframe src="//www.appstudio.kitd.com/kickFlash/js/compiler/v2/iframe.html?{{htmlVars}}" height="{{height}}" width="{{width}}" frameborder="0" scrolling="no" style="border:none;overflow:hidden;width:{{width}}px;height:{{height}}px"></iframe>',
            video_player_class: 'as_video_player'
        },
        instances = {},
        api = {},
        module,
        configClasses;
    function getDefault() {
        return $.extend({}, CONFIG, {
            player_id: "q68p3258",
            client_id: "3819",
            width: "640",
            height: "360"
        });
    }
    function getPlaylistEmbed() {
        return $.extend({}, CONFIG, {
            player_id: "y78550b9",
            client_id: "3819",
            width: "640",
            height: "506"
        });
    }
    function getChannelEmbed() {
        return $.extend({}, CONFIG, {
            player_id: "de4ishn1",
            client_id: "3819",
            width: "640",
            height: "536"
        });
    }
    function getConfigClasses() {
        if (!$) {
            return;
        }
        configClasses = {
            defaultClass: getDefault(),
            singleAssetPlayer: getDefault(),
            playlistEmbed: getPlaylistEmbed(),
            channelEmbed: getChannelEmbed()
        }
    }
    function render(template, data) {
        function resolve(str) {
            var i,
                prop,
                pointer = data;
            str = str.split(".");
            for (i = 0; i < str.length; i++) {
                prop = str[i];
                if (pointer.hasOwnProperty(prop)) {
                    pointer = pointer[prop];
                }
            }
            return pointer;
        }
        var tokenRegex = new RegExp(/\{\{.*?\}\}/g);
        return template.replace(tokenRegex, function (s) {
            s = s.replace("{{", "").replace("}}", "");
            return resolve(s);
        });
    }
    function getJSONURL(config, callback) {
        callback = callback || "";
        return environment.protocol + CONFIG.BASE_URL + "/ws/get_appstudio_player_json" + callback + "/" + config.player_id + ".json";
    }
    function getSWFURL(config) {
        return environment.protocol + "//appasset.appstudio.piksel.com/app/AppLoader.swf?src=" + getJSONURL(config);
    }
    function getFeedURL(config, html) {
        if (config.asset_id) {
            if (html) {
                return render("http:{{baseURL}}/ws/get_asset_details/api/{{api}}/a/{{asset_id}}/mode/json/apiv/{{apiv}}", config);
            } else {
                return render("http:{{baseURL}}/ws/get_asset_project/asset/{{asset_id}}/{{deliveryType}}/api_f4d84551-2e85-103a-a1ba-00e08655b9fc/apiv_3.5/gft_0/{{asset_id}}.xml", config);
            }
        } else if (config.project_uuid) {
            return render("http:{{baseURL}}/ws/get_vod_player_info/p_{{project_uuid}}/api_{{api}}/apiv_3.6/gft_0/{{project_uuid}}.xml", config);
        }
        return "";
    }
    function getPlayerVars(config) {
        var playerVars = {
            autoPlay: config.autoPlay,
            scaleMODE: "letterbox",
            width: config.width,
            height: config.height,
            src: getJSONURL(config),
            analyticsConfigURL: CONFIG.ANALYTICS_BEACON
        };
        playerVars.lang = config.lang || "us_en";
        if (config.hasOwnProperty("debug")) {
            playerVars.debug = config.debug;
        }
        if (config.force_logout) {
            playerVars.force_logout = config.force_logout;
        }
        if (config.video_uuid) {
            playerVars.video_uuid = config.video_uuid;
        }
        if (config.video_uuid) {
            playerVars.categoryId = config.category_id;
        }
        return playerVars;
    }
    function getHTMLVars(config) {
        var vars = getPlayerVars(config),
            params = [],
            prop,
            iWidth = vars.width,
            iHeight = vars.height;
        vars.mediaSuiteURL = getFeedURL(config, true);
        vars.referralUrl = window.location.href.replace(/http[s]*:\/\//g, "");
        vars.pageReferrer = document.referrer.replace(/http[s]*:\/\//g, "");
        vars.widgetAkHost = "www.appstudio.kitd.com";
        vars.width = String(vars.width);
        vars.height = String(vars.height);
        vars.src = getJSONURL(config, "/callback/KickUtils.scriptLoadCallbacks[0]");
        if (vars.width.indexOf("%") !== -1) {
            vars.width = "";
        }
        if (vars.height.indexOf("%") !== -1) {
            vars.height = "";
        }
        for (prop in vars) {
            params.push(prop + "=" + encodeURIComponent(vars[prop]));
        }
        vars.width = iWidth;
        vars.height = iHeight;
        return params.join("&");
    }
    function getFlashVars(config) {
        var vars = getPlayerVars(config);
        vars.mediaSuiteURL = getFeedURL(config);
        vars.js = 1;
        return vars;
    }
    function createPlayer(config) {
        config.client_id = config.client_id || "3819";
        config.languageCode = config.languageCode || "en";
        config.autoPlay = config.autoPlay || config.autoplay || "false";
        config.deliveryType = config.deliveryType || "protocol_hd";
        config.baseURL = config.baseURL || CONFIG.BASE_URL;
        config.api = config.api || CONFIG.API;
        config.apiv = config.api_v || CONFIG.API_VERSION;
        var flashvars = getFlashVars(config),
            swfURL = getSWFURL(config),
            attributes = {
                id: config.playerDivID,
                name: config.playerDivID
            },
            params = {
                movie: '"' + swfURL + '"',
                pluginspage: "http%3A//www.adobe.com/go/getflashplayer",
                align: "middle",
                allowscriptAccess: "always",
                quality: "high",
                allowFullScreen: "true",
                wmode: "transparent",
                bgcolor: "#FFFFFF",
                menu: "false"
            },
            callback = function (e) {
                var div;
                if (e.success === false) {
                    config.htmlVars = getHTMLVars(config);
                    div = $("#" + config.playerDivID);
                    if (config.width) {
                        div.width(config.width);
                    }
                    if (config.height) {
                        div.height(config.height);
                    }
                    div.html(render(CONFIG.IFRAME, config));
                }
            };
        swfObject.embedSWF(swfURL, config.playerDivID, config.width, config.height, "10", CONFIG.EXPRESS_INSTALL, flashvars, params, attributes, callback);
    }
    function setProperty(obj, key, value) {
        var type = typeof obj[key];
        switch(type) {
        case "string":
            if (typeof value ===  "string") {
                obj[key] = value;
            }
          break;
        case "number":
          if (String(value) === String(Number(value))) {
              obj[key] = Number(value);
          }
          break;
        case "undefined":
            if (!obj.hasOwnProperty(key)) {
                obj[key] = value;
            }
            break;
        default:
            if (typeof value ===  type) {
                obj[key] = value;
            }
        }
    }
    function createConfig(elem, className, init) {
        getConfigClasses();
        var config = $.extend(configClasses[className], init),
            $div = $(elem);
        config.playerDivID = elem.id;
        config.width = config.width || $div.width();
        config.height = config.height || $div.height();
        $.each(elem.attributes, function(i, attrib){
            var name = attrib.name;
            if (name.indexOf("data-as-") !== -1) {
                name = name.replace("data-as-", "").replace("-", "_");
                setProperty(config, name, attrib.value);
            } else if (name.indexOf("data-") !== -1) {
                name = name.replace("data-", "").replace("-", "_");
                setProperty(config, name, attrib.value);
            }
            if (name === "id") {
                //config.id = attrib.value;
            }
        });
        return config;
    }
    function loadScript(url) {
        var head = document.getElementsByTagName('head')[0],
            script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = url;
        head.appendChild(script);
    }
    function checkLibraries(init) {
        var jQueryVersion = ($ && $.fn && $.fn.jquery) ? $.fn.jquery.split(".") : [0, 0, 0];
        if (jQueryVersion.length && Number(jQueryVersion[0]) > 0 && Number(jQueryVersion[1]) > 6) {
            libraries.jquery = true;
        } else {
            libraries.jquery = false;
            api.ingest_jquery = function (jQuery) {
                api.ingest_jquery = function() {};
                $ = jQuery;
                find(init);
            }
        }
        //object may exist with lowercase name from swfobject.js
        swfObject = swfObject || swfobject;
        if (swfObject && swfObject.embedSWF) {
            libraries.swfObject = true;
        } else {
            libraries.swfObject = false;
            api.ingest_swfobject = function (swfO) {
                api.ingest_swfobject = function() {};
                swfObject = swfO;
                find(init);
            }
        }
    }
    function find(init) {
        init = init || CONFIG;
        checkLibraries(init);
        function ingestPlayer(elem) {
            var className,
                $this = $(elem),
                templateClass = "defaultClass",
                config;
            for (className in configClasses) {
                if ($this.hasClass(className)) {
                    templateClass = className;
                }
            }
            config = createConfig(elem, templateClass, init);
            createPlayer(config);
        }
        if (libraries.jquery && libraries.swfObject) {
            getConfigClasses();
            $(function () {
                $("." + init.video_player_class.replace("-", "_")).each(function () {
                    ingestPlayer(this);
                });
                $("." + init.video_player_class.replace("_", "-")).each(function () {
                    ingestPlayer(this);
                });
            });
            return instances;
        }
        return null;
    }
    function setConfig(init) {
        var prop;
        init = init || {};
        for (prop in init) {
            if (init.hasOwnProperty(prop)
                && CONFIG.hasOwnProperty(prop)
                && typeof CONFIG[prop] === typeof init[prop]) {
                    CONFIG[prop] = init[prop];
            }
        }
    }
    api.createPlayer = createPlayer;
    api.find = find;
    window.asWidgetManager = window.asWidgetManager || api;
    api = window.asWidgetManager;
    find(CONFIG);
    module = function (init) {
        setConfig(init);
        find(CONFIG);
        return api;
    }
}(window, (function () {
    "use strict";
    function getQSParams() {
        var paramList = String(String(window.location.search).split("?")[1]).split("&amp;").join("&").split("&"),
            i,
            params = {},
            param;
        for (i = 0; i < paramList.length; i = i + 1) {
            param = String(paramList[i]).split("=");
            params[param[0]] = param[1];
        }
        return params;
    }
    var environment = {};
    try {
        environment.isIPad = (navigator.userAgent.indexOf("iPad") !== -1);
        environment.isIPhone = (navigator.userAgent.indexOf("iPhone") !== -1);
    } catch (e) {
        environment.isIPad = false;
        environment.isIPhone = false;
    }
    environment.subaddress = window.location.href.split("?");
    if (environment.subaddress && environment.subaddress.length > 1) {
        environment.subaddress.shift();
        environment.subaddress = "?" + environment.subaddress.join("?");
    } else {
        environment.subaddress = "";
    }
    environment.qs = environment.subaddress.split("#").shift() || "";
    environment.params = getQSParams();
    environment.isiOS = (environment.isIPad || environment.isIPhone);
    environment.ua = window.navigator.userAgent.toString().toLowerCase();
    environment.browser = (function getDevice() {
        var ua = environment.ua,
            browserTypes = {
                "ie 11.0": "IE11",  
                "msie 10.0": "IE10",
                "msie 9.0": "IE9",
                "msie 8.0": "IE8",
                "msie 7.0": "IE7",
                "msie 6.0": "IE6",
                "opera": "Opera",
                "opr": "Opera",
                "android": "Android",
                "ipad": "iPad",
                "iphone": "iPhone",
                "chrome": "Chrome",
                "firefox": "Firefox",
                "safari": "Safari"
            },
            detected;
        for (detected in browserTypes) {
            if (browserTypes.hasOwnProperty(detected)
                    && ua.indexOf(detected) !== -1) {
                return browserTypes[detected];
            }
        }
        return "Other";
    }());
    try {
        environment.isAndroid = (navigator.userAgent.indexOf("Android") !== -1);
    } catch (e) {
        environment.isAndroid = false;
    }
    environment.isMobile = (environment.isiOS || environment.isAndroid);
    environment.device = (environment.isIPad) ? "iPad"
                       : (environment.isIPhone) ? "iPhone"
                       : (environment.isAndroid) ? "Android"
                       : "Desktop";
    environment.urlParts = document.location.href.split("/");
    function stripTrailingSlash(url) {
        return String(url).replace(/\/+$/, "");
    }
    environment.referrer = stripTrailingSlash((function () {
        return window.document.referrer.replace(/http[s]*:\/\//g, "") || "direct";
    }()));
    environment.landingPage = stripTrailingSlash((function () {
        return window.location.href.replace(/http[s]*:\/\//g, "") || "";
    }()));
    environment.domain = environment.params.host || String(environment.landingPage.split("/")[0]);
    environment.protocol = window.location.protocol;
    environment.path = window.location.href
        .split("?").shift()
        .split("#").shift()
        .replace(environment.protocol + "//", "")
        .replace(environment.domain, "");
    return environment;
}())));
