require([
		"swfobject",
    	"commonFunctions"
        ], function () {
            $(".video_player").each(function () {
                t_id = $(this).attr("id");
                t_arr = t_id.split("_");
                $(this).width(t_arr[2]);
                $(this).width(t_arr[3]);
                embedVideoPlayer(t_arr[1], t_arr[2], t_arr[3], t_id);
            });
        });

function embedVideoPlayer(aID, w, h, divID) {
    (function () {
        var secureProtocol = "http:";//location.protocol;
        var playerDivId = divID;
        var assetId = aID;
        var width = w;
        var height = h;
        var uuid = "q68p3258";
        var clientId = "3819";
        var autoPlay = "false";
        var deliveryType = "protocol_hd";
        var languageCode = "en";
        var timestamp = new Date().getTime();
        var swfURL = secureProtocol + "//appasset.appstudio.kitd.com/compiled/mediasuite_" + clientId + "/AppShell_" + uuid + ".swf?timestamp=" + timestamp;
        var htmlvars = 'mediaSuiteURL=http://player.multicastmedia.com/ws/get_asset_project/asset/' + assetId + '/' + deliveryType + '/api_f4d84551-2e85-103a-a1ba-00e08655b9fc/apiv_3.5/gft_0/' + assetId + '.xml&autoPlay=' + autoPlay + '&herbalLifeLanguageCode=' + languageCode + '&revision=1&scaleMODE=letterbox&width=' + width + '&height=' + height + '&getEmbed=http%3A//player.multicastmedia.com/ws/get_single_asset_embed/a_' + assetId + '/p_' + uuid + '/h_' + height + '/w_' + width + '/ap_' + autoPlay + '/dt_' + deliveryType + '&src=' + swfURL;
        var flashvars = {
            mediaSuiteURL: "http%3A//player.multicastmedia.com/ws/get_asset_project/asset/" + assetId + "/" + deliveryType + "/api_f4d84551-2e85-103a-a1ba-00e08655b9fc/apiv_3.5/gft_0/" + assetId + ".xml",
            autoPlay: autoPlay,
            herbalLifeLanguageCode: languageCode,
            revision: "1",
            scaleMODE: "letterbox",
            width: width,
            height: height,
            getEmbed: "http%3A//player.multicastmedia.com/ws/get_single_asset_embed/a_" + assetId + "/p_" + uuid + "/h_" + height + "/w_" + width + "/ap_false/dt_" + deliveryType,
            src: swfURL,
            analyticsConfigURL: "http%3A//herbalife.kicknetwork.com/analytics/beacon-ondemand.xml",
            akamaiConfigData: "am|region_created_for|RegionCreatedFor,am|country_created_for|CountryCreatedFor,am|creation_date|ContentCreateDate,am|expiration_date|ExpirationDate,am|hbnfeatured_video|FeaturedVideo,am|title_english|TitleEnglish,at|tags|Keywords,am|channel|category,af|duration|contentLength,am|content_type|contentType,am|data_set|show,at|title|title,am|asset_id|assetId,am|asset_id|hbnAssetId,am|hbn_number|hbnNumber,am|title_line_2|subTitle,at|tags|tags,at|title|eventName,am|data_set|DataSet",
            js: 1
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
            expressInstallURL = "http://www.appstudio.kitd.com//kickFlash/scripts/expressInstall2.swf?2",
            swfURL = secureProtocol + "//appasset.appstudio.kitd.com/compiled/mediasuite_" + clientId + "/AppShell_" + uuid + ".swf?timestamp=" + timestamp,
            attributes = {
                id: playerDivId,
                name: playerDivId
            },
            callback = function (e) {
                if (e.success == false) {
                    document.getElementById(playerDivId).innerHTML = '<iframe src="http://serve.a-widget.com/kickFlash/js/compiler/v2/iframe.html?affiliateSiteId=mediasuite_73&' + htmlvars + '&widgetId=' + uuid + '&widgetHost=http%3A//serve.a-widget.com&src=http%3A//player.multicastmedia.com/ws/get_appstudio_player_json/callback/KickUtils.scriptLoadCallbacks%5b0%5d/' + uuid + '.json&referralUrl=' + escape(window.location.href) + 'height="' + height + '" width="' + width + '" frameborder="0" scrolling="no" style="border:none;overflow:hidden;width:' + width + 'px;height:' + height + 'px"></iframe>'
                }
            };
        swfobject.embedSWF(swfURL, playerDivId, width, height, "10", expressInstallURL, flashvars, params, attributes, callback);
    })();
}
