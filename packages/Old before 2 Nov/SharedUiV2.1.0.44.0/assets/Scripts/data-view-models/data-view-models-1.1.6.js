// content list view model - generic CMS content lists 
function ContentListModel(path) {
    this.items = [];
    this.randomItem = null;
    this.originId = null;
    this.title = null;
    this.path = "/" + HL.Locale.getLocale() + '/api/ContentListItem?id=' + path + "&tags=";
    this.loading = true;
    this.dataHandler = function (data) {
        if (data != null) {
            this.set('loading', false);
            this.set('loaded', true);
            this.set("title", data.Title);
            this.set("originId", data.OriginId);
            if (data.Items) {
                var item = data.Items[Math.floor(Math.random() * data.Items.length)];

                this.set('randomItem', {
                    originId: item.OriginId || '',
                    body: item.Body || ''
                });
                while (item = data.Items.shift()) {
                    this.items.push({
                        originId: item.OriginId || '',
                        body: item.Body || ''
                    });
                }
            } else {
                this.set('error', true);
                this.set('loading', false);
            }
        }
    };
}

function AnnouncementsListModel() {
    this.items = [];
    this.title = null;
    this.viewAllText = null;
    this.viewAllLink = null;
    this.path = "/" + HL.Locale.getLocale() + '/api/Announcements/';
    this.loading = true;

    this.dataHandler = function (data) {
        if (data.Items.length == 0) {
            this.set('emptyModel', true);
        } else {
            this.set('emptyModel', false);
        }
        if (data != null) {
            this.set('loading', false);
            this.set('loaded', true);
            this.set("title", data.Title);
            this.set("viewAllText", data.ViewAllText);
            this.set("viewAllLink", data.ViewAllLink);
            if (data.Items) {
                for (var index = 0; index < data.Items.length; index++) {
                    var item = data.Items[index];
                    var announcement = {
                        body: item.Body || ''
                    };
                    this.items.push(announcement);
                }
            }
        } else {
            this.set('error', true);
            this.set('loading', false);
        }
    };
}

// cms published page support (breadcrumbs, left nav, previous next navigation handles
function CmsPageViewModel(data) {
    var defaultClass = 'darkgreyB';
    this.determineCurrent = function (navItems) {
        this.relatedPages = [];
        for (var i = 0; i < navItems.length; i++) {
            if (isCurrentPage(navItems[i].Link)) {
                navItems[i].cssClass = 'active';
                this.currentPage = navItems[i];
                if (i > 0) this.previousPage = navItems[(i - 1)];
                if ((i + 1) < navItems.length) this.nextPage = navItems[i + 1];
            } else {
                this.relatedPages.push(navItems[i]);
            }
        }
        return navItems;
    };

    var isCurrentPage = function (url) {
        var currentLink = decodeURI(window.location.pathname.toLowerCase());
        return (url && url.toLowerCase() == currentLink);
    };

    this.currentPage = null;
    this.nextPage = null;
    this.previousPage = null;
    this.relatedPages = null;
    this.leftNav = null;
    this.breadCrumbs = null;
    this.title = null;
    this.description = null;
    this.cssClass = null;
    this.cssIconClass = null;
    this.canLoad = false;
    var cmsData = data || window.cmsData;
    // support for sub header and breadcrumbs
    if (cmsData) {
        this.leftNav = this.determineCurrent(cmsData.leftNav);

        this.breadCrumbs = cmsData.breadCrumbs;
        this.clickableCrumbs = [];
        this.title = cmsData.title;
        this.description = cmsData.description;

        // breadcrumb fixed elements
        if (this.breadCrumbs && this.breadCrumbs.length > 2) {
            var allCrumbs = this.breadCrumbs.slice(0);

            this.crumb1 = allCrumbs.shift();
            this.crumb2 = allCrumbs.shift();

            // cms creates L5 links for sibling match for 'category' 
            // use the depth 4 item as 3rd crumb
            if (allCrumbs.length == 4) {
                allCrumbs.shift();
                this.crumb3 = allCrumbs.shift();
            } else {
                // use own parent as 3rd crumb
                allCrumbs.pop(); // one before last (self)
                this.crumb3 = allCrumbs.pop();
            }

            // side by side support for dynamic crumbs
            allCrumbs = this.breadCrumbs.slice(2);
            var crumb;
            while (crumb = allCrumbs.shift()) {
                if (crumb.Link && crumb.Link.length > 2) {
                    this.clickableCrumbs.push(crumb);
                }
            }
            this.selfCrumb = this.clickableCrumbs.pop();
        }

        var classes = ('' + cmsData.cssClass).split(' ');
        this.cssIconClass = classes.length > 0 && classes[0].length > 1 ? classes[0] : defaultClass;
        this.cssClass = classes.length > 1 && classes[1].length > 1 ? classes[1] : defaultClass;
    }

    this.isArrayWithValues = function (name) {
        var value = this.get(name);
        return (value && value.length > 0);
    };

    this.hasLeftNav = function () {
        return this.isArrayWithValues('leftNav');
    };

    this.hasBreadCrumbs = function () {
        return this.isArrayWithValues('breadCrumbs');
    };

    this.showRelatedPreview = function (item) {
        var target = $(item.currentTarget).closest("li").find(".preview");
        targetMid = target.height() / 3;

        target.toggleClass("active");
        $(item.currentTarget).on('mousemove', function (e) {
            target.offset({
                left: e.pageX + 20,
                top: e.pageY - targetMid
            });
        });
    };

    this.hideRelatedPreview = function (item) {
        var target = $(item.currentTarget).closest("li").find(".preview");
        target.toggleClass("active");
    };
}

// profile
function ProfileViewModel(urlPrefix) {
    this.path = (urlPrefix ? urlPrefix : '') + "/" + HL.Locale.getLocale() + '/api/DistributorProfile/';
    this.targetInfo = false;
    this.Name = '';
    this.Salutation = '';
    this.TeamLevelName = '';
    this.DisplayBizworksStatus = '';
    this.loading = true;
    this.IsBizworksSubscriber = false;

    this.dataHandler = function (data) {
        if (data) {
            this.set("loading", false);
            this.set("loaded", true);
            this.set("Name", data.FullNamePattern);
            this.set("Salutation", data.Salutation);
            this.set("TeamLevelName", data.TeamLevelName);
            this.set("DisplayBizworksStatus", data.DisplayBizworksStatus);
            this.set("IsBizworksSubscriber", data.IsBizworksSubscriber);
            this.set("targetInfo", data.TargetInfo);
            this.targetCheck(data);
        }
    };

    this.targetCheck = function (data) {
        //Test&Target funcionality
        simpleMediator.publish("setTargetInfo", data.TargetInfo);
        //We create an object to store profile information
        var adobeFmtdProfileData = {
            //We include profile information on the call to be used by Adobe
            FullNamePattern: data.FullNamePattern,
            Salutation: data.Salutation,
            TeamLevelName: data.TeamLevelName,
            DisplayBizworksStatus: data.DisplayBizworksStatus,
            IsBizworksSubscriber: data.IsBizworksSubscriber,
            //We declare the campaigns as an empty string
            campaigns: ""
        };
        if (data.TargetInfo && data.TargetInfo != null) {

            for (c = 0; c < data.TargetInfo.length; c++) {
                //Mbox items
                element = "#" + data.TargetInfo[c].CampaignName + '.mbox-item';

                //Identify existing mbox in campigns
                if (data.TargetInfo[c].CampaignName.trim() != "" && $(element).length != 0) {
                    $(element).removeClass('mbox-item');
                    callMbox(data.TargetInfo[c].CampaignName, data.TargetInfo[c].SegmentName);
                }
                //We append campaign information to list of campaigns
                adobeFmtdProfileData.campaigns += (data.TargetInfo[c].CampaignName + "=" + data.TargetInfo[c].SegmentName + "&");
            }
        }
        $(".mbox-item").each(function () {
            $(this).show();
        });
        //call to callGlobalMbox(adobeFmtdProfileData)
        callGlobalMbox(adobeFmtdProfileData);
    };

    //Call to initialize .mbox-item
    function callMbox(mb, segment) {
        //Call to Adobe
        adobe.target.getOffer({
            "mbox": mb,
            "params": { "profile.Parameter1": segment },
            "success": function (response) {
                if (response.length != 0) {

                    adobe.target.applyOffer({
                        "selector": ("#" + mb),
                        "offer": [response[0]]
                    });

                    var element = $("#" + mb);
                    initViewModels(element);
                    element.show();
                }
                else {
                    $("#" + mb).show();
                    logger.log("No response to Mbox: " + mb);
                }
            },
            "error": function (status, error) {
                logger.log("Something went wrong with Mbox: " + mb);
            },
            "timeout": 5000
        });
    };

    //
    function callGlobalMbox(adobeFmtdProfileData) {
        //Call to Adobe
        adobe.target.getOffer({
            //We use the Global Mbox
            "mbox": "hl-global-mbox",
            //We send multiple values including a string with all Campaigns the User belongs to
            "params": adobeFmtdProfileData,
            "success": function (response) {
                //We expect multiple responses, since multiple Campaigns on Adobe may be triggered
                if (response.length != 0) {
                    //We iterate through them
                    for (r = 0; r < response.length; r++) {

                        //We wrap response[0], which is the content as a jquery object
                        var elementTgt = $(response[r]["content"]);

                        //We pull its ID which will help us target page location from Adobe for an HTML offer on the site
                        //on Adobe we will ID the offers with an ID plus TgtOffer as an identifier, IE: <div id="heroBannerTgtOffer"...
                        var elementTgtId = elementTgt.attr("id");
                        elementTgtId = elementTgtId.replace("TgtOffer", "");

                        //If there is a matching element on the page we have a target to replace
                        var element = $("#" + elementTgtId);
                        if (element.length != 0) {
                            adobe.target.applyOffer({

                                //We define as the container for the offer, any existing item with a same ID as the one retrieved
                                "selector": ("#" + elementTgtId),

                                //We replace the content of the element specified on the selector
                                "offer": [response[r]]
                            });

                            initViewModels(element);
                        }
                        else {
                            logger.log("callGlobalMbox matching id element does not exist on page for: " + elementTgtId);
                        }
                    }
                }
                else {
                    logger.log("No response to callGlobalMbox");
                }
            },
            "error": function (status, error) {
                logger.log("Something went wrong with callGlobalMbox");
            },
            "timeout": 5000
        });
    };

    function initViewModels(element) {
        $('[wire-model]', element).each(function () {
            viewModelHelpers.mvvmUtil.attachTo(new window[$(this).attr("wire-model")]()).wireModelsToElements([$(this)]);
        });
    };

};

// idomoo- deprecate if no longer in use
function IdomooVideoViewModel(flowPlayer) {
    var player = flowPlayer;
    this.path = "/" + HL.Locale.getLocale() + '/api/custom/Idomoo';
    this.baseUrl = "https://herbalife.idomoo.com/videos/1216/";
    this.videoId = "";
    this.shouldPlay = false;
    this.hasVideo = false;
    this.thumbNail = "";
    this.pauseVideo = function () {
        var api = flowplayer();
        api.pause();
    };
    this.video_href = function () {
        return this.get("baseUrl") + this.get("videoId") + ".mp4";
    };
    this.showVideo = function () {
        $('.idomooVideo').data('kendoWindow').open().center();
    };

    this.setCookie = function () {
        $.cookie("__IDOMOOVidCookie", this.videoId, {
            expires: 7,
            path: '/'
        });
    };

    this.dataHandler = function (data) {
        if (data && data.ok == true) {
            this.set("hasVideo", true);
            this.set("videoId", data.videoId);
            this.set("thumbNail", data.thumbNail);
            player.flowplayer();
            var cookieText = $.cookie("__IDOMOOVidCookie");
            if (!cookieText) {
                this.showVideo();
                this.setCookie();
            } else if (cookieText != this.videoId) {
                this.showVideo();
                this.setCookie();
            }
        }
    };

}

function CartViewModel() {
    this.id = null;
    this.quantity = 0;
    this.name = null;
    this.subTotal = 0.0;
    this.volumePoints = 0.0;
    this.sku = null;
    this.loaded = false;
    this.loading = true;
    this.error = false;
    this.canLoad = false;
    this.reset = function () {
        this.set("id", null);
        this.set("quantity", null);
        this.set("name", null);
        this.set("subTotal", null);
        this.set("volumePoints", null);
        this.set("sku", null);
        this.set("loading", true);
        this.set("loaded", false);
        this.set("error", false);
    };
    this.path = "/ordering/api/CartWidget/";
    this.dataHandler = function (data) {
        if (data) {
            this.set("id", data.Id);
            this.set("quantity", data.Quantity);
            this.set("name", data.Name);
            this.set("subTotal", data.DisplaySubtotal);
            this.set("volumePoints", data.VolumePoints);
            this.set("sku", data.Sku);
            this.set("loading", false);
            this.set("loaded", true);
        } else {
            this.set("error", true);
            this.set("loading", false);
        }
    };

    this.onLoadError = function () {
        this.set("error", true);
        this.set("loading", false);
    };

    this.checkout = function () {
        var scope = this;
        if (scope.id == null) {
            this.set("canLoad", true);
            this.load();
            this.onAfterLoad = function () {
                window.location.href = "/Ordering/Shoppingcart.aspx?CartID=" + scope.id;
            };
        } else window.location.href = "/Ordering/Shoppingcart.aspx?CartID=" + scope.id;
        return false;
    };

    this.viewSavedCarts = function () {
        window.location.href = "/ordering/savedcarts.aspx";
        return false;
    };

    this.activationHandler = function (shouldHide) {
        this.set("canLoad", true);
        shouldHide ? this.reset() : this.load();
    }
}

function VolumeViewModel(urlPrefix) {
    this.path = (urlPrefix ? urlPrefix : '') + "/" + HL.Locale.getLocale() + '/api/Volume/';
    this.CurrentMonthVolume = {
        "VolumeMonth": 0,
        "HeaderText": "...",
        "PlainHeaderText": "...",
        "PPV": 0,
        "DV": 0,
        "TV": 0,
        "PV": 0,
        "GV": 0,
        "HeaderVolume": 0
    };
    this.LastMonthVolume = null;
    this.ChartData = [{
        "name": "PPV",
        "value": 0
    }, {
        "name": "DLV",
        "value": 0
    }, {
        "name": "PV",
        "value": 0
    }, {
        "name": "GV",
        "value": 0
    }, {
        "name": "TV",
        "value": 0
    }];
    this.MonthName = "";
    this.loading = true;
    this.error = "";

    this.dataHandler = function (data, status, xhr) {
        if (xhr && xhr.status == 200 && data && $.isArray(data)) {
            for (var i = 0; i < data.length; i++) {
                var v = this.cleanHeaderText(data[i]);
                this.set("MonthName", v.MonthName);
                if (v.IsCurrentMonth) {
                    this.set("CurrentMonthVolume", v);
                    this.setChartValues(v);
                } else {
                    this.set("LastMonthVolume", v);
                }
            }
            this.createChart("#volChart");
            this.set("loading", false);
            if (this.get("LastMonthVolume") == null) this.set("loadSingle", true);
            else this.set("loadDual", true);
        } else {
            // no transport issue, but data not as expected.
            this.set("error", "inline");
        }
    };

    this.onLoadError = function (xhr, status, statusText) {
        this.set("error", "inline");
    };

    this.cleanHeaderText = function (volumeObject) {
        if (volumeObject && volumeObject.HeaderText) {
            volumeObject.PlainHeaderText = volumeObject.HeaderText.replace(/\<[^>]+\>/, ' ');
        }
        return volumeObject;
    };

    this.setChartValues = function (volume) {
        this.set(
            "ChartData", [{
                "name": "PPV",
                "value": volume.PPV
            }, {
                "name": "DLV",
                "value": volume.DV
            }, {
                "name": "PV",
                "value": volume.PV
            }, {
                "name": "GV",
                "value": volume.GV
            }, {
                "name": "TV",
                "value": volume.TV
            }]);
    };

    this.createChart = function (selector) {
        var chart = $(selector);
        if (!(chart && chart.length)) {
            return;
        }
        chart.kendoChart({
            dataSource: new kendo.data.DataSource({
                data: this.ChartData
            }),
            chartArea: {
                background: "#EEE"
            },
            title: {
                text: this.MonthName,
                position: "bottom"
            },
            legend: {
                visible: true,
                position: "top"
            },
            seriesDefaults: {
                type: "column",
                labels: {
                    visible: false
                }
            },
            series: [{
                field: "value",
                color: "#75c52a",
                gap: 0.9,
                border: 0,
                overlay: {
                    gradient: "none"
                }
            }],
            categoryAxis: {
                field: "name",
                majorGridLines: {
                    visible: false
                }
            },
            valueAxis: [{
                majorGridLines: {
                    visible: false
                }
            }],
            tooltip: {
                visible: true,
                template: "#= category #: #= value #",
                color: "gray",
                background: "white"
            }
        });
    };
}

function topNavViewModel() {
    var previousNavItem = null;
    var waiting = false;
    var nextNavItem = null;

    this.openLevel = function (e) {

        if (e.type == "click") {
            if (isMobile() || isSmallScreen()) {

                actualNav = e.currentTarget.parentNode;
                //mobile approach
                if (previousNavItem) {
                    hideNav(previousNavItem, true, true);
                }
                if (actualNav != previousNavItem) {
                    hideNav(actualNav, false, true);
                    previousNavItem = actualNav;
                } else
                    previousNavItem = null;
            }
        } else {
            if (!isSmallScreen() && !isMobile()) {
                //desktop approach
                if (e.type == "mouseover") {
                    hideNav(e.currentTarget, false, false);
                    previousNavItem = e.currentTarget;
                } else if (e.type == "mouseout" && $(e.relatedTarget).closest(e.currentTarget).length == 0) {
                    previousNavItem && hideNav(previousNavItem, true, false);
                    previousNavItem = null;
                }
            } else
                return false;
        }

    };

    function hideNav(navItem, flag, isMobileNav) {
        if (flag) {
            if (isMobileNav) {
                hideElement(navItem, true);
            } else {
                waiting = true;
                nextNavItem = null;
                setTimeout(
                    function () {
                        waiting = false;
                        hideElement(navItem, true);
                        if (nextNavItem) {
                            hideElement(nextNavItem, false);
                        }
                    }, 500);
            }
        } else {
            if ((!waiting || isMobileNav) && navItem.className != "active") {
                hideElement(navItem, false);
            } else
                nextNavItem = navItem;
        }
    };

    function hideElement(element, flag) {
        var e = getFirstChild(element);
        while (e.nodeName != "UL") e = e.nextSibling;
        if (flag) {
            e.style.display = "none";
            element.className = "";
        } else {
            e.style.display = "block";
            element.className = "active";
        }
        element.id == "cartInfoDropDown" && simpleMediator.publish("topNavCartEvent", flag);
    };

    function getFirstChild(e) {
        e = e.firstChild;
        while (e.nodeType != 1) e = e.nextSibling;
        return e;
    };

    function isMobile() {
        return HL.Util.Browser.isMobile.any();
    };

    function isSmallScreen() {
        return HL.Util.Browser.dimensions.Width() < 960;
    };

};

function mobileNavViewModel() {
    this.open = function (e) {
        $(".transformer").toggleClass("is-open");
    }
};

//View model to encrypt URL's
function UrlEncriptionViewModel() {
    var url;
    var data = new Array();

    this.sendURL = function (e) {
        first = true;
        url = $(e.delegateTarget).data("url");
        if (url.search("\\?") == -1) url = url + "?";
        data = $(e.delegateTarget).data("values").split(",");
        for (index = 0; index < data.length; ++index) {
            temp = data[index].split(":");
            url = url + (!first ? "&" : "") + temp[0] + "=" + encodeURIComponent(_AnalyticsFacts_[temp[1]]);
            first ? first = false : first;
        }
        $(e.delegateTarget).attr("href", url);
    }
};

//View modal for generic modal
function modalWindowViewModel(element, titleelement) {
    that = null;
    title = '';
    titleElement = titleelement || "h4";
    icon = 'icon-delete-fl-5';
    iconElement = ".k-window .k-icon";
    this.modalElement = element;
    UImodal = "kendoWindow";

    this.load = function () {
        that = this;
        if ($(titleElement, $(that.modalElement)).length != 0 && !$(titleElement, $(that.modalElement)).data('ignoretitle')) {
            title = $(titleElement, $(that.modalElement)).text();
        }
        $(that.modalElement)[UImodal]({
            modal: true,
            resizable: false,
            visible: false,
            title: title,
            animation: {
                open: {
                    effects: "fade:in"
                },
                close: {
                    effects: "fade:out"
                }
            }
        });

        //max-width
        $(that.modalElement).parent().css("max-width", $(that.modalElement).css("max-width"));

        //setting icon
        $(iconElement).addClass(icon).text("");

        $(that.modalElement).attr("data-centerwindow", true);

    }
};

function SessionMsgViewModel(cookieName, isOldIE) {
    var OldIE = isOldIE || false;

    this.isVisible = function () {
        if (HL.Cookie.getCookie(cookieName) != "true" &&
            ((OldIE && HL.Util.Browser.IE().isTheBrowser && HL.Util.Browser.IE().actualVersion <= 9) || !OldIE)) {
            $(this.obj).slideDown();
            return true;
        } else {
            return false;
        }
    };

    this.closeMessage = function () {
        $(this.obj).slideUp();
        document.cookie = cookieName + "=true";
    };

}

var HistoryNavigationViewModel = function () {
    this.goToPreviousPage = function (e) {
        e.preventDefault();
        window.history.back();
    };

    this.isVisible = function () {
        var markup = $("#hide-back-button");
        if (markup.length > 0) {
            return false;
        }
        else {
            return true;
        }
    }
}

var InternationAgreementViewModel = function () {

    if (HL.Cookie.getCookie('InternationalAgreement') != "true") {
        this.path = "/api/InternationalAgreement";
    }

    this.dataHandler = function (data) {
        if (!data) {
            this.get("actualWidget").open().center();
        } else {
            HL.Cookie.setCookie('InternationalAgreement', "true");
        }
    };

    this.accept = function (e) {
        e.preventDefault();
        var actualModel = this;
        $.ajax({
            type: "POST",
            url: "/api/InternationalAgreement?acceptAgreement=true"
        })
        .done(function (context, status, xhr) {
            if (!context) {
                console.log("Error with international Agreement: " + context);
            } else {
                HL.Cookie.setCookie('InternationalAgreement', "true");
            }
            actualModel.get("actualWidget").close();
        })
        .fail(function (context, status, error) {
            console.log("Error with international Agreement: " + context);
            actualModel.get("actualWidget").close();
        });
    };
};