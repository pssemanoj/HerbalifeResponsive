var _array = document.location.href.split('/');
_array = _array[2].split('.');
languageCode = _array[0];
if (languageCode == '' || languageCode == 'undefined') {
    languageCode = 'en';
}
var secureProtocol = location.protocol;
var pageFormat = location.pathname;


player_div_name = '';

function playVideo(playerType, id) {
    if (playerType.toLowerCase() == 'asset-single') {
        playAssetSingle(id);
    } else if (playerType.toLowerCase() == 'project-single') {
        playProjectSingle(id);
    } else if (playerType.toLowerCase() == 'project-playlist') {
        player_div_name = 'player_playlist_2row';
        playProjectPlaylist2Row(id, player_div_name);
    } else if (playerType.toLowerCase() == 'project-playlist2') {
        player_div_name = 'player_playlist_1row';
        playProjectPlaylist2Row2(id, player_div_name);
    } else if (playerType.toLowerCase() == 'playlist-1row') {
        player_div_name = 'inline_playlist_1row';
        playProjectPlaylist1Row(id, player_div_name);
    } else if (playerType.toLowerCase() == 'playlist-1row-panel') {
        player_div_name = 'inline_playlist_1row_panel';
        playProjectPlaylist1RowPanel(id, player_div_name);
    } else if (playerType.toLowerCase() == 'playlist-2row-panel') {
        player_div_name = 'inline_playlist_2row_panel';
        playProjectPlaylist2RowPanel(id, player_div_name);
    } else if (playerType.toLowerCase() == 'category-1row') {
        player_div_name = 'player_category_1row';
        playProjectCategory1Row(id, player_div_name);
    } else if (playerType.toLowerCase() == 'category-2row') {
        player_div_name = 'player_category_2row';
        playProjectCategory2Row(id, player_div_name);
    }

}

(function (window) {
    "use strict";
    var document = window.document,
        CONFIG = {
            //BASE_URL : "//player.piksel.com",
            BASE_URL: "//player.multicastmedia.com",
            ANALYTICS_BEACON: "//ma198-r.analytics.edgesuite.net/config/beacon-4896.xml?beaconVersion=1.1",
            API: "7ccba1e8-1592-102b-8824-000c291cbe49",
            API_VERSION: "4.0",
            EXPRESS_INSTALL: "//www.appstudio.kitd.com//kickFlash/scripts/expressInstall2.swf?2",
            IFRAME: '<iframe src="//www.appstudio.kitd.com//kickFlash/js/compiler/v2/iframe.html?{{htmlVars}}" height="{{height}}" width="{{width}}" frameborder="0" scrolling="no" style="border:none;overflow:hidden;width:{{width}}px;height:{{height}}px"></iframe>'
        };
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
    function getJSONURL(config) {
        var url = CONFIG.BASE_URL + "/ws/get_appstudio_player_json/";
        url += config.clientId.charAt(0) + "/" + config.clientId.charAt(1) + "/" + config.clientId + "/";
        url += "/callback/KickUtils.scriptLoadCallbacks[0]/";
        url += "/" + config.playerId + ".json";
        return url;
    }
    function getSWFURL(config) {
        return render("//appasset.appstudio.kitd.com/compiled/mediasuite_{{clientId}}/AppShell_{{playerId}}.swf", config);
    }
    function getFeedURL(config, html) {
        if (config.assetId) {
            if (html) {
                return render("http:{{baseURL}}/ws/get_asset_details/api/{{api}}/a/{{assetId}}/mode/json/apiv/{{apiv}}", config);
            } else {
                return render("http:{{baseURL}}/ws/get_asset_project/asset/{{assetId}}/{{deliveryType}}/api_f4d84551-2e85-103a-a1ba-00e08655b9fc/apiv_3.5/gft_0/{{assetId}}.xml", config);
            }
        } else if (config.projectId) {
            return render("http:{{baseURL}}/ws/get_vod_player_info/p_{{projectId}}/api_{{api}}/apiv_3.6/gft_0/{{projectId}}.xml", config);
        }
        return "";
    }
    function getEmbedURL(config) {
        if (config.assetId) {
            return render("http:{{baseURL}}/ws/get_single_asset_embed/a_{{assetId}}/p_{{playerId}}/h_{{height}}/w_{{width}}/ap_{{autoPlay}}/dt_{{deliveryType}}", config);
        } else if (config.projectId) {
            return render("http:{{baseURL}}/ws/get_appstudio_embed/v1/vodproject/project_uuid/{{projectId}}.xml", config);
        }
        return "";
    }
    function getPlayerVars(config) {
        return {
            autoPlay: config.autoPlay,
            herbalLifeLanguageCode: config.languageCode,
            scaleMODE: "letterbox",
            width: config.width,
            height: config.height,
            getEmbed: getEmbedURL(config),
            src: getJSONURL(config),
            analyticsConfigURL: CONFIG.ANALYTICS_BEACON
        };
    }
    function getHTMLVars(config) {
        var vars = getPlayerVars(config),
            params = [],
            prop;
        vars.mediaSuiteURL = getFeedURL(config, true);
        vars.referralUrl = window.location.href.replace(/http[s]*:\/\//g, "");
        vars.pageReferrer = document.referrer.replace(/http[s]*:\/\//g, "");
        vars.widgetAkHost = "www.appstudio.kitd.com";
        for (prop in vars) {
            params.push(prop + "=" + escape(vars[prop]));
        }
        return params.join("&");
    }
    function getFlashVars(config) {
        var vars = getPlayerVars(config);
        vars.mediaSuiteURL = getFeedURL(config);
        vars.js = 1;
        return vars;
    }
    function createPlayer(config) {
        config.clientId = config.clientId || "3819";
        config.languageCode = config.languageCode || "en";
        config.autoPlay = config.autoPlay || "false";
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
                if (e.success === false) {
                    config.htmlVars = getHTMLVars(config);
                    document.getElementById(config.playerDivID).innerHTML = render(CONFIG.IFRAME, config);
                }
            };
        window.swfobject.embedSWF(swfURL, config.playerDivID, config.width, config.height, "10", CONFIG.EXPRESS_INSTALL, flashvars, params, attributes, callback);

    }

    function buildAssetPlayer(playerDivID, assetId, width, height, playerId, clientId) {
        createPlayer({
            playerDivID: playerDivID,
            assetId: assetId,
            width: width,
            height: height,
            playerId: playerId,
            clientId: clientId
        });
    }

    function buildProjectPlayer(playerDivID, projectId, width, height, playerId, clientId) {
        createPlayer({
            playerDivID: playerDivID,
            projectId: projectId,
            width: width,
            height: height,
            playerId: playerId,
            clientId: clientId
        });
    }

    function playAssetSingle(assetId) {
        createPlayer({
            assetId: assetId,
            width: "480",
            height: "270",
            playerId: "f494021s",
            clientId: "3819",
            playerDivID: "player_single"
        });
    }

    function playProjectSingle(projectId) {
        createPlayer({
            projectId: projectId,
            width: "480",
            height: "270",
            playerId: "t9vyd19q",
            clientId: "3819",
            playerDivID: "player_single"
        });
    }

    function playProjectPlaylist1Row(projectId, playerDivID) {
        playerDivID = playerDivID || "inline_playlist_1row";
        createPlayer({
            projectId: projectId,
            playerDivID: playerDivID,
            width: "640",
            height: "506",
            playerId: "oiig00k8",
            clientId: "3819"
        });
    }

    function playProjectPlaylist2Row(projectId, playerDivID) {
        playerDivID = playerDivID || "player_playlist_2row";
        createPlayer({
            projectId: projectId,
            playerDivID: playerDivID,
            width: "640",
            height: "617",
            playerId: "qrob4m85",
            clientId: "3819"
        });
    }

    function playProjectPlaylist1RowPanel(projectId, playerDivID) {
        playerDivID = playerDivID || "inline_playlist_1row_panel";
        createPlayer({
            projectId: projectId,
            playerDivID: playerDivID,
            width: "960",
            height: "506",
            playerId: "wh9sl71d",
            clientId: "3819"
        });
    }

    function playProjectPlaylist2RowPanel(projectId, playerDivID) {
        playerDivID = playerDivID || "player_playlist_2row_panel";
        createPlayer({
            projectId: projectId,
            playerDivID: playerDivID,
            width: "960",
            height: "612",
            playerId: "s9g46no9",
            clientId: "3819"
        });
    }

    function playProjectCategory1Row(projectId, playerDivID) {
        playerDivID = playerDivID || "player_category_1row";
        createPlayer({
            projectId: projectId,
            playerDivID: playerDivID,
            width: "960",
            height: "570",
            playerId: "sex656r8",
            clientId: "3819"
        });
    }

    function playProjectCategory2Row(projectId, playerDivID) {
        playerDivID = playerDivID || "player_category_2row";
        createPlayer({
            projectId: projectId,
            playerDivID: playerDivID,
            width: "960",
            height: "645",
            playerId: "ho11mvla",
            clientId: "3819"
        });
    }

    function playVideoDws(assetId) { //used on DWS sites
        $("html, body").animate({ scrollTop: 0 }, 600);
        createPlayer({
            assetId: assetId,
            width: "615",
            height: "320",
            playerId: "uv4496ka",
            clientId: "3819",
            playerDivID: "videoPlayer"
        });
    }

    function embedVideoPlayer(projectId, width, height, playerDivID) { //used on MyHL3.0
        createPlayer({
            projectId: projectId,
            width: width,
            height: height,
            playerId: "q68p3258",
            clientId: "3819",
            playerDivID: playerDivID
        });
    }


    window.buildAssetPlayer = buildAssetPlayer;
    window.buildProjectPlayer = buildProjectPlayer;
    window.playProjectPlaylist1Row = playProjectPlaylist1Row;
    window.playProjectPlaylist2Row = playProjectPlaylist2Row;
    window.playProjectPlaylist1RowPanel = playProjectPlaylist1RowPanel;
    window.playProjectPlaylist2RowPanel = playProjectPlaylist2RowPanel;
    window.playProjectCategory1Row = playProjectCategory1Row;
    window.playProjectCategory2Row = playProjectCategory2Row;
    window.playAssetSingle = playAssetSingle;
    window.playProjectSingle = playProjectSingle;
    window.playVideo = playVideo;
    window.embedVideoPlayer = embedVideoPlayer;
    window.as_createPlayer = createPlayer;
}(window));


$(document).ready(function () {
    
    $(".video_player").each(function () {
        t_id = $(this).attr("id");
        t_arr = t_id.split("_");
        $(this).width(t_arr[1]);
        $(this).height(t_arr[2]);
        if (isNaN(t_arr[0])) {
            buildProjectPlayer(t_id, t_arr[0], t_arr[1], t_arr[2], t_arr[3], t_arr[4]);
        } else {
            buildAssetPlayer(t_id, t_arr[0], t_arr[1], t_arr[2], t_arr[3], t_arr[4]);
        }
    });

    //for MyHL 3.0 Brown using proper camelnotation convention
    $(".videoPlayer").each(function () {
        t_id = $(this).attr("id");
        t_arr = t_id.split("_");
        embedVideoPlayer(t_arr[1], t_arr[2], t_arr[3], t_id);
    });


    $(".video_embed").each(function () { //gstnvafk_640_617_qrob4m85
        t_id = $(this).attr("id");
        t_arr = t_id.split("_");
        if (isNaN(t_arr[0])) {
        buildProjectPlayer(t_id, t_arr[0], t_arr[1], t_arr[2], t_arr[3], t_arr[4]);
        } else {
            buildAssetPlayer(t_id, t_arr[0], t_arr[1], t_arr[2], t_arr[3], t_arr[4]);
        }
    });


    var pageType = pageFormat.split('.');    
    if (pageType[pageType.length - 1].search("htm") != -1) {
        if ($(".inline").length) {
            $(".inline").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "568px",
                    height: "368px"
                });
                $('body').append('<div style="display:none;width:540px;height:358px"><div style="padding:0px; background:#000" id="inline"><div id="video_wb9s1521_540_304_q68p3258"></div></div></div>');
            }

            if ($(".inline-single").length) {
                $(".inline-single").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "508px",
                    height: "333px"
                });
                $('body').append('<div style="display:none;width:480px;height:327px"><div style="padding:0px; background:#000" id="inline_single"><div id="player_single"></div></div></div>');
            }

            if ($(".inline-playlist-1row").length) {
                $(".inline-playlist-1row").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "668px",
                    height: "569px"
                });
                $('body').append('<div style="display:none;width:640px;height:569px"><div style="padding:0px; background:#000" id="inline_playlist_1row" class="player-container"><div id="player_playlist_1row" ></div></div></div>');
            }

            if ($(".inline-playlist").length) {
                $(".inline-playlist").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "668px",
                    height: "680px; overflow:hidden"
                });
                $('body').append('<div style="display:none;width:640px;height:674px"><div style="padding:0px; background:#000" id="inline_playlist" class="player-container"><div id="player_playlist_2row"></div></div></div>');
            }

            if ($(".inline-playlist-1row-panel").length) {
                $(".inline-playlist-1row-panel").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "988px",
                    height: "569px"
                });
                $('body').append('<div style="display:none;width:960px;height:569px"><div style="padding:0px; background:#000" id="inline_playlist_1row_panel" class="player-container"><div id="player_playlist_1row_panel"></div></div></div>');
            }

            if ($(".inline-playlist-2row-panel").length) {
                $(".inline-playlist-2row-panel").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "988px",
                    height: "675px"
                });
                $('body').append('<div style="display:none;width:960px;height:675px"><div style="padding:0px; background:#000" id="inline_playlist_2row_panel" class="player-container"><div id="player_playlist_2row_panel"></div></div></div>');
            }

            if ($(".inline-category-1row").length) {
                $(".inline-category-1row").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "978px",
                    height: "633px"
                });
                $('body').append('<div style="display:none;width:978px;height:627px"><div style="padding:0px; background:#000" id="inline_category_1row" class="player-container"><div id="player_category_1row"></div></div></div>');
            }

            if ($(".inline-category-2row").length) {
                $(".inline-category-2row").colorbox({
                    transition: "fade",
                    inline: true,
                    width: "988px",
                    height: "708px"
                });
                $('body').append('<div style="display:none;width:988px;height:702px"><div style="padding:0px; background:#000" id="inline_category_2row" class="player-container"><div id="player_category_2row"></div></div></div>');
            }
        }  

});

function playVideo2(pType, aID, pID, cID, w, h) {
    var playerClassName = ".player_" + pID;

    if ($(".player_" + pID).length > 0) {
        if ($("#" + pID).length == 0) {
            console.log("#" + pID + " does not exist yet. Writing.");
            $('body').append('<div style="display:none;width:' + w + 'px;height:' + (parseInt(h) + 57) + 'px"><div class="player-container" style="padding:0px; background:#000" id="' + pID + '"><div id="player_' + aID + '"></div></div></div>');
        } else {
            console.log("#" + pID + " DOES exist.");
        }
        /*$(".player_wh9sl71d").colorbox({
				transition: "fade",
				inline: true,
				width: "960px",
				height: "506px"
		});*/
        $(playerClassName).colorbox({
            transition: "fade",
            inline: true,
            width: (parseInt(w) + 28) + "px",
            height: (parseInt(h) + 63) + "px"
        });
    }

    if (pType.toLowerCase() == 'asset') {
        buildAssetPlayer("player_" + aID, aID, w, h, pID, cID);
    } else {
        buildProjectPlayer("player_" + aID, aID, w, h, pID, cID);
    }
}

//new function added per Lawrence M. and Anthony Malach's request
function playVideo3(pType, aID, pID, cID, w, h) {
    var playerClassName = ".player_" + pID;
    if ($(".player_" + pID).length > 0) {
        if ($("#" + pID).length == 0) {
            //console.log("#"+ pID + " does not exist yet. Writing.");
            $('body').append('<div style="display:none;width:' + w + 'px;height:' + (parseInt(h) + 57) + 'px"><div class="player-container" style="padding:0px; background:#000" id="' + pID + '"><div id="player_' + pID + '"></div></div></div>');
        } else {
            //console.log("#"+ pID + " DOES exist.");
        }
        $(playerClassName).colorbox({
            transition: "fade",
            inline: true,
            width: (parseInt(w) + 28) + "px",
            height: (parseInt(h) + 63) + "px"
        });
    }
    if (pType.toLowerCase() == 'asset') {
        buildAssetPlayer("player_" + pID, aID, w, h, pID, cID);
    } else {
        buildProjectPlayer("player_" + pID, aID, w, h, pID, cID);
    }
}

