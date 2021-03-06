/* AppMeasurement Version 1.6.2 */
/* Last Modified - 07.26.2016 (m.d.Y) */
/* Last Modified By - Alejandro Fernandez */
var s_omntr = {};

function OmnitureModule() {
    var _OmnitureModuleInstance = this;

    _OmnitureModuleInstance.Init = function () {
        // Check if jQuery Library Exists
        if (typeof jQuery != 'undefined' && typeof _AnalyticsFacts_ != 'undefined' && _AnalyticsFacts_) {
            refactorAnalyticsFacts(_AnalyticsFacts_);
        }
    };

    function errorHandling(err) {
        var error = err.stack;
        //console.log(error);
    }

    /* Grabs the AnalyticsFacts object and reassigns the values for the omniture variables */
    function refactorAnalyticsFacts(analyticsFacts) {
        var customAnalyticsFacts = {};
        customAnalyticsFacts.reportSuiteId = "";
        customAnalyticsFacts.fullSiteName = "myherbalife";
        customAnalyticsFacts.siteName = "";
        customAnalyticsFacts.countryCode = "";
        customAnalyticsFacts.languageCode = "";
        customAnalyticsFacts.locale = "";
        customAnalyticsFacts.currencyCode = "";
        customAnalyticsFacts.localPageTitle = "";
        customAnalyticsFacts.searchTerms = "";
        customAnalyticsFacts.bcLevel1 = "";
        customAnalyticsFacts.bcLevel2 = "";
        customAnalyticsFacts.bcLevel3 = "";
        customAnalyticsFacts.isLoggedIn = false;
        customAnalyticsFacts.WaitingRoom = false;
        customAnalyticsFacts.dsId = "";
        customAnalyticsFacts.dsTeam = "";
        customAnalyticsFacts.countryOfProcessing = "";
        customAnalyticsFacts.dsIsBizworksSubscriber = "false";
        customAnalyticsFacts.dsIsDwsOwner = "false";
        customAnalyticsFacts.productDetail = null;
        customAnalyticsFacts.orderId = "";
        customAnalyticsFacts.products = null;
        customAnalyticsFacts.events = "";

        if (typeof analyticsFacts.OmnitureSiteId && analyticsFacts.OmnitureSiteId != "" && analyticsFacts.OmnitureSiteId != null) {
            customAnalyticsFacts.reportSuiteId = analyticsFacts.OmnitureSiteId.toLowerCase();
        }
        if (typeof analyticsFacts.Events && analyticsFacts.Events != "" && analyticsFacts.Events != null) {
            customAnalyticsFacts.events = analyticsFacts.Events;
        }
        if (typeof analyticsFacts.OmnitureSiteCountryId != "undefined" && analyticsFacts.OmnitureSiteCountryId != "" && analyticsFacts.OmnitureSiteCountryId != null) {
            if (customAnalyticsFacts.reportSuiteId != "") {
                customAnalyticsFacts.reportSuiteId = customAnalyticsFacts.reportSuiteId.toLowerCase() + "," + analyticsFacts.OmnitureSiteCountryId.toLowerCase();
            } else {
                customAnalyticsFacts.reportSuiteId = analyticsFacts.OmnitureSiteCountryId.toLowerCase();
            }
        }
        if (typeof analyticsFacts.OmnitureSiteName && analyticsFacts.OmnitureSiteName != "" && analyticsFacts.OmnitureSiteName != null) {
            customAnalyticsFacts.siteName = analyticsFacts.OmnitureSiteName.toLowerCase();
        }

        if (typeof analyticsFacts.CountryCode != "undefined" && analyticsFacts.CountryCode != "" && analyticsFacts.CountryCode != null) {
            customAnalyticsFacts.countryCode = analyticsFacts.CountryCode.toLowerCase();
        }

        if (typeof analyticsFacts.CurrencyCode != "undefined" && analyticsFacts.CurrencyCode != "" && analyticsFacts.CurrencyCode != null) {
            customAnalyticsFacts.currencyCode = analyticsFacts.CurrencyCode.toUpperCase();
        }

        if (typeof analyticsFacts.LanguageCode != "undefined" && analyticsFacts.LanguageCode != "" && analyticsFacts.LanguageCode != null) {
            customAnalyticsFacts.languageCode = analyticsFacts.LanguageCode.toLowerCase();
        }

        if (customAnalyticsFacts.languageCode != "" && customAnalyticsFacts.countryCode != "" && customAnalyticsFacts.countryCode != null) {
            customAnalyticsFacts.locale = analyticsFacts.LanguageCode.toLowerCase() + "-" + customAnalyticsFacts.countryCode.toUpperCase();
        }

        if (typeof analyticsFacts.Title != "undefined" && analyticsFacts.Title != "" && analyticsFacts.Title != null) {
            customAnalyticsFacts.localPageTitle = analyticsFacts.Title.toLowerCase();
        }

        if (typeof analyticsFacts.SearchTerms != "undefined" && analyticsFacts.SearchTerms != "" && analyticsFacts.SearchTerms != null) {
            customAnalyticsFacts.searchTerms = analyticsFacts.SearchTerms.toLowerCase();
        }

        if (typeof analyticsFacts.IsLoggedIn != "undefined" && analyticsFacts.IsLoggedIn && analyticsFacts.IsLoggedIn != null) {
            customAnalyticsFacts.isLoggedIn = analyticsFacts.IsLoggedIn;
        }

        if (typeof analyticsFacts.WaitingRoom != "undefined" && analyticsFacts.WaitingRoom && analyticsFacts.WaitingRoom != null) {
            customAnalyticsFacts.WaitingRoom = analyticsFacts.WaitingRoom;
        }

        if (typeof analyticsFacts.Id != "undefined" && analyticsFacts.Id != "" && analyticsFacts.Id != null) {
            customAnalyticsFacts.dsId = analyticsFacts.Id.toUpperCase();
        }

        if (typeof analyticsFacts.SubtypeCode != "undefined" && analyticsFacts.SubtypeCode != "" && analyticsFacts.SubtypeCode != null) {
            customAnalyticsFacts.dsTeam = analyticsFacts.SubtypeCode.toUpperCase();
        }

        if (typeof analyticsFacts.ProcessingCountryCode != "undefined" && analyticsFacts.ProcessingCountryCode != "" && analyticsFacts.ProcessingCountryCode != null) {
            customAnalyticsFacts.countryOfProcessing = analyticsFacts.ProcessingCountryCode.toUpperCase();
        }

        var i = 0;
        if (typeof analyticsFacts.Roles != "undefined" && analyticsFacts.Roles != null) {
            for (i = 0; i < analyticsFacts.Roles.length; i++) {
                if (analyticsFacts.Roles[i].match(/Bizworks_(Sub|1|2|3)/i)) {
                    customAnalyticsFacts.dsIsBizworksSubscriber = "true";
                } else if (analyticsFacts.Roles[i] == "DWSOwner") {
                    customAnalyticsFacts.dsIsDwsOwner = "true";
                }
            }
        }

        if (typeof analyticsFacts.Breadcrumbs != "undefined" && analyticsFacts.Breadcrumbs != null) {
            for (i = 0; i < analyticsFacts.Breadcrumbs.length && i < 3; i++) {
                customAnalyticsFacts['bcLevel' + (i + 1)] = analyticsFacts.Breadcrumbs[i].toLowerCase();
            }
        }

        if (typeof analyticsFacts.ProductDetail != "undefined" && analyticsFacts.ProductDetail != null) {
            customAnalyticsFacts.productDetail = analyticsFacts.ProductDetail;
        }

        if (typeof analyticsFacts.OrderId != "undefined" && analyticsFacts.OrderId != "" && analyticsFacts.OrderId != null) {
            customAnalyticsFacts.orderId = analyticsFacts.OrderId;
        }

        if (typeof analyticsFacts.PricedCart != "undefined" && analyticsFacts.PricedCart != null) {
            customAnalyticsFacts.products = analyticsFacts.PricedCart;
        }

        pluginsConf(customAnalyticsFacts);

    }

    /* Assigns all of the site specific variables: ex: pageName, channel, eVars, props, events etc... */

    function setCustomValues(analyticsFacts) {
        var values = getCustomValues(analyticsFacts);

        if (typeof values != 'undefined') {
            try {
                /* Add all of the site specific variables here */

                // Server. ex: server = uk.myherbalife.com
                s_omntr.server = values.host;

                // Page Name. ex: pageName = myhl:us:en:ordering:hoppingCart
                // https://www.myherbalife.com/Ordering/ShoppingCart.aspx
                s_omntr.pageName = values.pageName;

                // Page Name Copy. Conversion Variable 
                s_omntr.eVar3 = values.pageNameBreadCrumbs;

                // Page Title. Traffic Variable ex prop33 = toolsAndTrainingOverview
                s_omntr.prop33 = values.pageTitle;
                s_omntr.eVar33 = "D=c33";

                // Local PageTitle. Traffic Variable ex prop35 = herramientas y capacitacion
                s_omntr.prop35 = values.localPageTitle;
                s_omntr.eVar35 = values.localPageTitle;

                // Section. ex: s_channel = myherbalife:toolsandtraining
                s_omntr.channel = values.parentSection;

                // Region. Traffic Variable ex: s_prop1 = myhl3
                //s_omntr.prop1 = values.region;

                // Country. Traffic Variable ex: prop2 = myhl3:us   
                s_omntr.prop2 = values.country;
                s_omntr.eVar12 = "D=c2";

                // Language. Traffic Variable ex: prop6 = en
                s_omntr.prop6 = values.language;

                // Locale. Traffic Variable ex: prop29 = en-US
                s_omntr.prop29 = values.locale;
                s_omntr.eVar29 = values.locale;

                // Sub Sections (Level 1). Traffic Variable ex: prop3 = myhl:na:us:toolsandtraining 
                s_omntr.prop3 = values.sectionLevel1;
                s_omntr.eVar13 = "D=c3";

                // Sub Sections (Level 2). Traffic Variable ex: prop4 = myhl:na:us:toolsandtraining:resourcelibrary
                if (values.sectionLevel2 != "") {
                    s_omntr.prop4 = values.sectionLevel2;
                    s_omntr.eVar14 = "D=c4";
                }

                // Sub Sections (Level 3). Traffic Variable ex: prop5 = myhl:na:us:toolsandtraining:resourcelibrary:tutorialsandquizzes
                if (values.sectionLevel3 != "") {
                    s_omntr.prop5 = values.sectionLevel3;
                    s_omntr.eVar15 = "D=c5";
                }

                // Sub Sections (Level 4). Traffic Variable ex: prop16 = myhl:na:us:toolsandtraining:resourcelibrary:tutorialsandquizzes:subsection4
                if (values.sectionLevel4 != "") {
                    s_omntr.prop16 = values.sectionLevel4;
                }

                // Sub Sections (Level 5). Traffic Variable ex: prop17 = myhl:na:us:toolsandtraining:resourcelibrary:tutorialsandquizzes:subsection4:subsection5
                if (values.sectionLevel5 != "") {
                    s_omntr.prop17 = values.sectionLevel5;
                }

                // Hierarchy. ex: hier1 = myhl:na:us:toolsandtraining|myhl:na:us:toolsandtraining:resourcelibrary|myhl:na:us:toolsandtraining:resourcelibrary:tutorialsandquizzes 
                s_omntr.hier1 = values.hierarchy1;

                // Currency Code. ex: currencyCode = USD
                s_omntr.currencyCode = values.currencyCode;

                // Distributor Profile.
                if (values.distributorId != "") {
                    // DS ID. Traffic Variable ex prop19 = STAFF 
                    s_omntr.prop19 = values.distributorId;
                    // Distributor ID. Conversion Variable ex: eVar22 = STAFF
                    s_omntr.eVar22 = values.distributorId;

                    if (values.distributorTeam != "") {
                        // DS Team. Traffic Variable ex: prop26 = PT 
                        s_omntr.prop26 = values.distributorTeam;
                        // DS Team. Conversion Variable ex eVar8 = PT
                        s_omntr.eVar8 = values.distributorTeam;
                        // Levels pathing. Traffic Variable ex: prop18 = PT:myhl:na:us:toolsandtraining
                        s_omntr.prop18 = values.distributorTeam + ":" + values.pageName;
                    }
                    if (values.countryOfProcessing != "") {
                        // Country of Processing. Traffic Variable ex prop25 = US
                        s_omntr.prop25 = values.countryOfProcessing;
                        // Country of Processing. Conversion Variable ex eVar25 = US
                        s_omntr.eVar25 = values.countryOfProcessing;
                    }
                    if (values.dsIsBizworksSubscriber != "") {
                        // Bizworks Subscriber. Traffic Variable ex: prop27 = false
                        s_omntr.prop27 = values.dsIsBizworksSubscriber;
                        // Bizworks Subscriber. Conversion Variable ex: eVar27 = false
                        s_omntr.eVar27 = values.dsIsBizworksSubscriber;
                    }
                    if (values.dsIsDwsOwner != "") {
                        // Ds Owns DWS. Traffic Variable ex: prop34 = false 
                        s_omntr.prop34 = values.dsIsDwsOwner;
                        // Ds Owns DWS. Conversion Variable ex: eVar34 = false
                        s_omntr.eVar34 = values.dsIsDwsOwner;
                    }
                }

                // Logged in Status.
                if (values.loggedStatus != "") {
                    // Custom eVar28. Conversion Variable ex: eVar28 = logged in
                    s_omntr.eVar28 = values.loggedStatus;
                    // copy eVar28 "logged in" status. Traffic Variable ex: prop28 = logged in 
                    s_omntr.prop28 = values.loggedStatus;
                }

                //Waiting Room
                if (values.WaitingRoom) {
                    s_omntr.events = "event10";
                }


                // Product View
                if (values.productDetail != "") {
                    // Product Name. Traffic Variable ex: prop15 = formula 1 instant healthy meal nutritional shake mix
                    s_omntr.prop15 = values.productName;
                    s_omntr.events = "prodView";
                    s_omntr.products = values.productDetail;
                }

                //Shopping Cart
                if (values.products != "") {
                    // Products. ex: products = ";formula 1 nutritional shake mix-french vanilla  750g (3106);1;19.87,;formula 1 nutritional shake mix-wild berry   750g (3108);2;39.74";
                    s_omntr.products = values.products;
                    if (values.purchaseId == "scView") {
                        s_omntr.events = "scView";
                    } else if (values.purchaseId == "scCheckout") {
                        s_omntr.events = "scCheckout";
                    } else if (values.purchaseId != "" && values.purchaseId != "scView" && values.purchaseId != "scCheckout") {
                        s_omntr.events = "purchase";
                        s_omntr.purchaseId = values.purchaseId; // Purchase ID. Traffic Variable ex: eVar9 = DA00020508
                        s_omntr.eVar9 = s_omntr.purchaseId;
                    }
                }
                /* End */
                if (values.events) {
                    s_omntr.events = s_omntr.apl(s_omntr.events, values.events, ',', 1);
                }
                return true;

            } catch (err) {
                return false;
            }
        } else {
            return false;
        }
    }

    /* Obtains and builds all of the site specific variables: pageName, Section, eVars, props, events etc... */

    function getCustomValues(siteData) {
        try {
            var customValues = {
                host: window.location.host.toLowerCase(),
                currentFullURL: window.location.href.toLowerCase(),
                currentURL: window.location.protocol.toLowerCase() + "//" + window.location.host.toLowerCase() + window.location.pathname.toLowerCase(),
                referringDomain: document.referrer.toLowerCase(),
                pageName: "",
                pageNameBreadCrumbs: "",
                pageTitle: "",
                localPageTitle: "",
                parentSection: "",
                sectionLevel1: "",
                sectionLevel2: "",
                sectionLevel3: "",
                sectionLevel4: "",
                sectionLevel5: "",
                country: siteData.siteName + ":" + siteData.countryCode,
                language: siteData.languageCode,
                locale: siteData.locale,
                currencyCode: siteData.currencyCode,
                distributorId: siteData.dsId,
                distributorTeam: siteData.dsTeam,
                countryOfProcessing: siteData.countryOfProcessing,
                dsIsBizworksSubscriber: siteData.dsIsBizworksSubscriber,
                dsIsDwsOwner: siteData.dsIsDwsOwner,
                bizworksPageTitle: s_omntr.Util.getQueryParam('ReportType').toLowerCase(),
                titleQueryStringExists: s_omntr.Util.getQueryParam('Title').toLowerCase(),
                loggedStatus: "",
                WaitingRoom: siteData.WaitingRoom,
                events: siteData.events,
                hierarchy1: "",
                clientIpAddress: "",
                purchaseId: "",
                productDetail: "",
                productName: "",
                products: ""
            };

            var path = window.location.pathname.toLowerCase();
            var arrPath = path.split('/');
            var subSections = [];

            // Parsing the current URL
            for (var i = 1; i < arrPath.length; i++) {
                if (i + 1 == arrPath.length && arrPath[i] != "") {
                    if (customValues.titleQueryStringExists != "") {
                        customValues.pageTitle = customValues.titleQueryStringExists;
                    } else if (customValues.bizworksPageTitle != "") {
                        customValues.pageTitle = arrPath[i].substring(0, arrPath[i].indexOf('.')) + ":" + customValues.bizworksPageTitle;
                    } else if (arrPath[i].substring(0, arrPath[i].indexOf('.')) == "") {
                        customValues.pageTitle = arrPath[i];
                    } else {
                        customValues.pageTitle = arrPath[i].substring(0, arrPath[i].indexOf('.'));
                    }
                } else {
                    if (arrPath[i] != "default" && arrPath[i] != "") {
                        subSections.push(arrPath[i]);
                    }
                }
            }

            function pageTitleExists(previousPageName) {
                var newPageName = previousPageName;
                if (customValues.pageTitle != "") {
                    newPageName = newPageName + ":" + customValues.pageTitle;
                }

                return newPageName;
            }

            // Sets the values for the Omniture pageName variable and also for the section and subsections variables ex: myhl:us:en:ordering:shoppingCart
            function formatPageName() {
                customValues.pageName = customValues.sectionLevel1;
                customValues.hierarchy1 = customValues.pageName;
                if (subSections.length > 0) {
                    customValues.parentSection = customValues.parentSection + ":" + subSections[0];
                    customValues.sectionLevel1 = customValues.sectionLevel1 + ":" + subSections[0];

                    for (var j = 0; j < subSections.length; j++) {
                        customValues.pageName = customValues.pageName + ":" + subSections[j];
                        customValues.hierarchy1 = customValues.hierarchy1 + "|" + customValues.pageName;
                        if (j + 1 == subSections.length) {
                            customValues.pageName = pageTitleExists(customValues.pageName);
                            customValues.hierarchy1 = customValues.hierarchy1 + "|" + customValues.pageName;
                            if (j >= 1) {
                                customValues.sectionLevel2 = customValues.sectionLevel1 + ":" + subSections[1];
                                if (siteData.bcLevel2 != "") {
                                    customValues.pageNameBreadCrumbs = customValues.pageNameBreadCrumbs + ":" + siteData.bcLevel2;
                                }
                                if (j >= 2) {
                                    if (siteData.bcLevel3 != "") {
                                        customValues.pageNameBreadCrumbs = customValues.pageNameBreadCrumbs + ":" + siteData.bcLevel3;
                                    }
                                    customValues.sectionLevel3 = customValues.sectionLevel2 + ":" + subSections[2];
                                    if (j >= 3) {
                                        customValues.sectionLevel4 = customValues.sectionLevel3 + ":" + subSections[3];
                                        if (j >= 4) {
                                            customValues.sectionLevel5 = customValues.sectionLevel4 + ":" + subSections[4];
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {
                    customValues.pageName = pageTitleExists(customValues.pageName);
                    customValues.hierarchy1 = customValues.pageName;
                }
            }

            // Concatenate the values from the Parsed URL
            customValues.localPageTitle = s_omntr.normalizeText(siteData.localPageTitle);
            customValues.parentSection = siteData.fullSiteName;
            customValues.sectionLevel1 = siteData.siteName + ":" + siteData.countryCode + ":" + siteData.languageCode;
            if (siteData.bcLevel1 != "") {
                customValues.pageNameBreadCrumbs = customValues.sectionLevel1 + ":" + siteData.bcLevel1;
            }

            if (siteData.isLoggedIn == false) {
                customValues.loggedStatus = "not logged in";
                customValues.parentSection = customValues.parentSection + ":" + "distributor login";
                customValues.sectionLevel1 = customValues.sectionLevel1 + ":" + "distributor login";
            } else {
                customValues.loggedStatus = "logged in";
            }

            formatPageName();

            // Products Detail
            if (siteData.productDetail != null) {
                var skus = "";
                for (var y = 0; y < siteData.productDetail.Skus.length; y++) {
                    skus = skus + siteData.productDetail.Skus[y];
                }
                var product = s_omntr.normalizeText(siteData.productDetail.Name);
                var category = siteData.productDetail.Category;

                customValues.productName = product;
                customValues.productDetail = ";" + product + " (" + skus + ");;;;evar18=" + category.toString();
            }

            // Shopping Cart 
            if (siteData.products != null) {
                customValues.purchaseId = siteData.orderId;
                var productsArr = [];
                var shippingCost = 0;
                var tax = 0;
                $(siteData.products).each(function () {
                    shippingCost = parseInt(this.Freight);
                    tax = parseInt(this.ItemTax);
                    var description = s_omntr.normalizeText(this.Description) + " (" + this.Sku + ")";
                    description = description.replace(/,/g, " ");
                    var quantity = this.Quantity;
                    var productTotalCost = this.DiscountedPrice;
                    var product = ";" + description + ";" + quantity.toString() + ";" + productTotalCost.toString();
                    productsArr.push(product);
                });

                for (var x = 0; x < productsArr.length; x++) {
                    if (x + 1 == productsArr.length) {
                        customValues.products = customValues.products + productsArr[x];
                    } else {
                        customValues.products = customValues.products + productsArr[x] + ",";
                    }
                }
            }


            return customValues;
        } catch (err) {
            errorHandling(err);
        }
    }

    /* Assigns and configures the plugins */

    function pluginsConf(analyticsFacts) {
        /************************** CONFIG SECTION **************************/
        try {
            if (analyticsFacts.reportSuiteId != "") {
                //s_omntr = s_gi(analyticsFacts.reportSuiteId);
                s_omntr = new AppMeasurement();


                /* Plugin Config */
                s_omntr.usePlugins = true;

                /* Conversion Config */
                s_omntr.currencyCode = analyticsFacts.currencyCode;

                /* Link Tracking Config */
                s_omntr.trackDownloadLinks = true;
                s_omntr.trackExternalLinks = true;
                s_omntr.trackInlineStats = true;
                s_omntr.linkDownloadFileTypes = "exe,zip,wav,mp3,mov,mpg,avi,wmv,pdf,doc,docx,xls,xlsx,ppt,pptx,pps,fla,csv,jpg,png,eps,gif";
                // Holds the site domains to consider it as internal
                s_omntr.linkInternalFilters = "javascript:,myherbalife,hrbl.net" + "," + window.location.href;
                s_omntr.linkLeaveQueryString = false;
                s_omntr.linkTrackVars = "None";
                s_omntr.linkTrackEvents = "None";
                s_omntr.useForcedLinkTracking = false;

                /* Form Analysis Config */
                s_omntr.formList = "";
                s_omntr.trackFormList = true;
                s_omntr.trackPageName = true;
                s_omntr.useCommerce = false;
                s_omntr.varUsed = "";
                s_omntr.eventList = ""; //Abandon,Success,Error

                /* DynamicObjectIDs v.1.4 - Configuration of Dynamic Object IDs for ClickMap */

                function s_getObjectID(o) {
                    var ID = o.id;
                    return ID;
                }

                s_omntr.getObjectID = s_getObjectID;


                //Addthis integration

                function addthisConf(reportSuiteId) {
                    if (reportSuiteId == "hlcandy" || reportSuiteId.indexOf("hlherbalifeglobalprod") > -1) {
                        if (typeof addthis != 'undefined') {

                            function shareEventHandler(evt) {
                                if (evt.type == 'addthis.menu.share') {
                                    s_omntr.prop20 = s_omntr.prop2;
                                    s_omntr.prop21 = s_omntr.pageName;
                                    s_omntr.prop22 = s_omntr.channel;
                                    s_omntr.linkTrackVars = 'action,events,prop20,prop21,prop51,prop22';
                                    s_omntr.linkTrackEvents = 'AddThisShareEvents';
                                    s_omntr.events = 'AddThisShareEvents';
                                    s_omntr.action = evt.data.service;
                                    s_omntr.tl(this, 'o', 'addthis:' + evt.data.service);
                                }
                            }

                            // Listen for the share event
                            addthis.addEventListener('addthis.menu.share', shareEventHandler);
                        }
                    }
                }

                /*Custom Links, Clickmap, ShoppingCart add to Cart 
                    Looks for every element that has tracklink on their ClassName
                    and adds the customlink Tag on the onclick function.
                      Requirement:
                        -The page needs to have the jquery library.
                        -The elements needs to have an ID or a title. */

                function customLinksConf() {
                    $(".tracklink").each(function () {
                        if (this) {
                            var linkId = $(this).attr('data-analytics-id');
                            if (typeof linkId != "undefined" && linkId != "" && linkId != null) {
                                createObjClickFunction(this, linkId);
                            } else if (this.id) {
                                createObjClickFunction(this, this.id);
                            } else if (this.title) {
                                createObjClickFunction(this, this.title);
                            }
                        }
                    });

                    /*Custom track of Add to Cart button 
                    Looks for the button Add to Cart and adds the customTrack Tag on the onclick function. */
                    $(".trackAddToCart").each(function () {
                        createObjClickFunction(this, "addToCartBtn");
                    });
                }

                /*Creates the onclick function of the object that it's been pass so the customlink and the clickmap can be tracked.
                @param obj:Object
                @param objId:String */

                function createObjClickFunction(obj, objId) {
                    var correlationLinks = '';
                    if (obj.onclick != null) {
                        obj.oldfunction = obj.onclick;
                    }
                    if (s_omntr.prop19 && s_omntr.prop19 != '') {
                        correlationLinks = ',prop19';
                    }

                    if (objId == "addToCartBtn") {
                        correlationLinks = 'events,products,prop2,prop19,prop21,prop22,prop51' + correlationLinks;
                        obj.onclick = function () {
                            s_objectID = s_omntr.formatTrackingName(objId);
                            s_omntr.prop21 = s_omntr.pageName;
                            s_omntr.prop22 = s_omntr.channel;
                            s_omntr.linkTrackVars = correlationLinks;
                            s.linkTrackEvents = "scAdd";
                            s_omntr.tl();
                            obj.oldfunction ? obj.oldfunction() : true;
                            return false;
                        };
                    } else if ($(obj).attr('data-analytics-id')) {
                        correlationLinks = 'prop20,prop2,prop21,prop22,prop37,eVar37,prop38,eVar38,prop39,eVar39,prop51,prop70,prop71' + correlationLinks;
                        var linkSubSection = $(obj).attr('data-analytics-pagesubsection');
                        var linkContainer = $(obj).attr('data-analytics-parentcontainername');
                        var linkPosition = $(obj).attr('data-analytics-position');
                        if (linkSubSection == "dashboard") {
                            linkPosition = s_omntr.currrDashPanel;
                        }
                        obj.onclick = function () {
                            s_objectID = s_omntr.formatTrackingName(objId);
                            s_omntr.prop21 = s_omntr.pageName;
                            s_omntr.prop22 = s_omntr.channel;
                            s_omntr.prop37 = linkSubSection;
                            s_omntr.eVar37 = "D=c37";
                            s_omntr.prop38 = linkContainer;
                            s_omntr.eVar38 = "D=c38";
                            s_omntr.prop39 = linkPosition;
                            s_omntr.eVar39 = "D=c39";
                            s_omntr.prop70 = "D=oid"; //clickmap oid
                            s_omntr.prop71 = "D=pid"; //clickmap pid
                            s_omntr.linkTrackVars = correlationLinks;
                            s_omntr.tl(obj, 'o', s_omntr.formatTrackingName(objId), null, this.target != '_blank' ? 'navigate' : '');
                            this.target == '_blank' ? window.open(this.href) : true;
                            obj.oldfunction ? obj.oldfunction() : true;
                            return false;
                        };
                    } else {
                        correlationLinks = 'prop20,prop2,prop21,prop22,prop51,prop70,prop71' + correlationLinks;
                        obj.onclick = function () {
                            s_objectID = s_omntr.formatTrackingName(objId);
                            s_omntr.prop21 = s_omntr.pageName;
                            s_omntr.prop22 = s_omntr.channel;
                            s_omntr.prop70 = "D=oid"; //clickmap oid
                            s_omntr.prop71 = "D=pid"; //clickmap pid
                            s_omntr.linkTrackVars = correlationLinks;
                            s_omntr.tl(obj, 'o', s_omntr.formatTrackingName(objId), null, this.target != '_blank' ? 'navigate' : '');
                            this.target == '_blank' ? window.open(this.href) : true;
                            obj.oldfunction ? obj.oldfunction() : true;
                            return false;
                        };
                    }
                }

                //Add dasboard navigation event listeners
                function initDashNavListeners() {
                    if (typeof $('#myDashBoard') != "undefined" && $('#myDashBoard') != "" && $('#myDashBoard') != null) {
                        s_omntr.currrDashPanel = 1;
                        $('.myDashBoardSets_Nav_Prev').bind('click', function () {
                            if (s_omntr.currrDashPanel == 1) {
                                s_omntr.currrDashPanel = $('.myDashBoardSets_Nav ul li').length;
                            } else {
                                s_omntr.currrDashPanel = s_omntr.currrDashPanel - 1;
                            }
                        });

                        $('.myDashBoardSets_Nav_Next').bind('click', function () {
                            if (s_omntr.currrDashPanel == $('.myDashBoardSets_Nav ul li').length) {
                                s_omntr.currrDashPanel = 1;
                            } else {
                                s_omntr.currrDashPanel = s_omntr.currrDashPanel + 1;
                            }
                        });

                        $('.myDashBoardSets_Nav ul li').each(function (index) {
                            index = parseInt(index) + 1;
                            $(this).bind('click', function () {
                                s_omntr.currrDashPanel = index;
                            });
                        });
                    }
                }


                /* Adds the pagename to a string. 
                    Returns a string with the format: ""pagename:objectID"" */
                s_omntr.formatTrackingName = function (objectid) {
                    var res = '';
                    if (objectid.indexOf('tracklink') > 0) {
                        res = s_omntr.pageName + ':' + objectid.substring(0, (objectid.indexOf('tracklink') - 1));
                    } else {
                        res = s_omntr.pageName + ':' + objectid;
                    }
                    return res;
                }; /********************************************* PLUGINS SECTION **********************************************/
                /* You may insert any plugins you wish to use here.                 */
                s_omntr.normalizeText = function (text) {
                    var specialCharArr = ["Ã", "À", "Á", "Ä", "Â", "È", "É", "Ë", "Ê", "Ì", "Í", "Ï", "Î", "Ò", "Ó", "Ö", "Ô", "Ù", "Ú", "Ü", "Û", "ã", "à", "á", "ä", "â", "è", "é", "ë", "ê", "ì", "í", "ï", "î", "ò", "ó", "ö", "ô", "ù", "ú", "ü", "û", "Ñ", "ñ", "Ç", "ç", "Á", "á", "É", "é", "Í", "í", "Ó", "ó", "Ú", "ú", "Ñ", "ñ"];
                    var original = ["A", "A", "A", "A", "A", "E", "E", "E", "E", "I", "I", "I", "I", "O", "O", "O", "O", "U", "U", "U", "U", "a", "a", "a", "a", "a", "e", "e", "e", "e", "i", "i", "i", "i", "o", "o", "o", "o", "u", "u", "u", "u", "n", "n", "c", "c", "A", "a", "E", "e", "I", "i", "O", "o", "U", "u", "N", "n"];
                    for (var i = 0; i < specialCharArr.length; i++) {
                        text = text.replace(specialCharArr[i], original[i]);
                    }
                    text = text.replace(/;/g, "").toLowerCase();
                    return text;
                };
                /* Gets the URL of the element that was clicked */
                s_omntr.p_gh = new Function("" + "var s=this;if(!s.eo&&!s.lnk)return '';var o=s.eo?s.eo:s.lnk,y=s.ot(" + "o),n=s.oid(o),x=o.s_oidt;if(s.eo&&o==s.eo){while(o&&!n&&y!='BODY'){" + "o=o.parentElement?o.parentElement:o.parentNode;if(!o)return '';y=s." + "ot(o);n=s.oid(o);x=o.s_oidt}}return o.href?o.href:'';");
                /* split a string */
                s_omntr.split = new Function("l", "d", "" + "var i,x=0,a=new Array;while(l){i=l.indexOf(d);i=i>-1?i:l.length;a[x" + "++]=l.substring(0,i);l=l.substring(i+d.length);}return a");
                s_omntr.p_c = new Function("v", "c", "" + "var x=v.indexOf('=');return c.toLowerCase()==v.substring(0,x<0?v.le" + "ngth:x).toLowerCase()?v:0");
                /* Append List */
                s_omntr.apl = new Function("L", "v", "d", "u", "" + "var s=this,m=0;if(!L)L='';if(u){var i,n,a=s.split(L,d);for(i=0;i<a." + "length;i++){n=a[i];m=m||(u==1?(n==v):(n.toLowerCase()==v.toLowerCas" + "e()));}}if(!m)L=L?L+d+v:v;return L");
                /* Replace */
                s_omntr.repl = new Function("x", "o", "n", "" + "var i=x.indexOf(o),l=n.length;while(x&&i>=0){x=x.substring(0,i)+n+x." + "substring(i+o.length);i=x.indexOf(o,i+l)}return x");
                s_omntr.c_rr = s_omntr.c_r;
                s_omntr.c_wr = s_omntr.c_w;
                /* Plugin Utility - first only */
                s_omntr.p_fo = new Function("n", "" + "var s=this;if(!s.__fo){s.__fo=new Object;}if(!s.__fo[n]){s.__fo[n]=" + "new Object;return 1;}else {return 0;}");
                /* Joins an array into a string */
                s_omntr.join = new Function("v", "p", "" + "var s=this;var f,b,d,w;if(p){f=p.front?p.front:'';b=p.back?p.back" + ":'';d=p.delim?p.delim:'';w=p.wrap?p.wrap:'';}var str='';for(var x=0" + ";x<v.length;x++){if(typeof(v[x])=='object' )str+=s.join( v[x],p);el" + "se str+=w+v[x]+w;if(x<v.length-1)str+=d;}return f+str+b;");
                /* vpr - set the variable vs with value v */
                s_omntr.vpr = new Function("vs", "v", "if(typeof(v)!='undefined'){var s=this; eval('s.'+vs+'=\"'+v+'\"')}");
                /* Plugin: Form Analysis 2.1 (Success, Error, Abandonment) */
                s_omntr.setupFormAnalysis = new Function("" + "var s=this;if(!s.fa){s.fa=new Object;var f=s.fa;f.ol=s.wd.onload;s." + "wd.onload=s.faol;f.uc=s.useCommerce;f.vu=s.varUsed;f.vl=f.uc?s.even" + "tList:'';f.tfl=s.trackFormList;f.fl=s.formList;f.va=new Array('',''" + ",'','')}");
                s_omntr.sendFormEvent = new Function("t", "pn", "fn", "en", "" + "var s=this,f=s.fa;t=t=='s'?t:'e';f.va[0]=pn;f.va[1]=fn;f.va[3]=t=='" + "s'?'Success':en;s.fasl(t);f.va[1]='';f.va[3]='';");
                s_omntr.faol = new Function("e", "" + "var s=s_c_il[" + s_omntr._in + "],f=s.fa,r=true,fo,fn,i,en,t,tf;if(!e)e=s.wd." + "event;f.os=new Array;if(f.ol)r=f.ol(e);if(s.d.forms&&s.d.forms.leng" + "th>0){for(i=s.d.forms.length-1;i>=0;i--){fo=s.d.forms[i];fn=fo.name" + ";tf=f.tfl&&s.pt(f.fl,',','ee',fn)||!f.tfl&&!s.pt(f.fl,',','ee',fn);" + "if(tf){f.os[fn]=fo.onsubmit;fo.onsubmit=s.faos;f.va[1]=fn;f.va[3]='" + "No Data Entered';for(en=0;en<fo.elements.length;en++){el=fo.element" + "s[en];t=el.type;if(t&&t.toUpperCase){t=t.toUpperCase();var md=el.on" + "mousedown,kd=el.onkeydown,omd=md?md.toString():'',okd=kd?kd.toStrin" + "g():'';if(omd.indexOf('.fam(')<0&&okd.indexOf('.fam(')<0){el.s_famd" + "=md;el.s_fakd=kd;el.onmousedown=s.fam;el.onkeydown=s.fam}}}}}f.ul=s" + ".wd.onunload;s.wd.onunload=s.fasl;}return r;");
                s_omntr.faos = new Function("e", "" + "var s=s_c_il[" + s_omntr._in + "],f=s.fa,su;if(!e)e=s.wd.event;if(f.vu){s[f.v" + "u]='';f.va[1]='';f.va[3]='';}su=f.os[this.name];return su?su(e):tru" + "e;");
                s_omntr.fasl = new Function("e", "" + "var s=s_c_il[" + s_omntr._in + "],f=s.fa,a=f.va,l=s.wd.location,ip=s.trackPag" + "eName,p=s.pageName;if(a[1]!=''&&a[3]!=''){a[0]=!p&&ip?l.host+l.path" + "name:a[0]?a[0]:p;if(!f.uc&&a[3]!='No Data Entered'){if(e=='e')a[2]=" + "'Error';else if(e=='s')a[2]='Success';else a[2]='Abandon'}else a[2]" + "='';var tp=ip?a[0]+':':'',t3=e!='s'?':('+a[3]+')':'',ym=!f.uc&&a[3]" + "!='No Data Entered'?tp+a[1]+':'+a[2]+t3:tp+a[1]+t3,ltv=s.linkTrackV" + "ars,lte=s.linkTrackEvents,up=s.usePlugins;if(f.uc){s.linkTrackVars=" + "ltv=='None'?f.vu+',events':ltv+',events,'+f.vu;s.linkTrackEvents=lt" + "e=='None'?f.vl:lte+','+f.vl;f.cnt=-1;if(e=='e')s.events=s.pt(f.vl,'" + ",','fage',2);else if(e=='s')s.events=s.pt(f.vl,',','fage',1);else s" + ".events=s.pt(f.vl,',','fage',0)}else{s.linkTrackVars=ltv=='None'?f." + "vu:ltv+','+f.vu}s[f.vu]=ym;s.usePlugins=false;var faLink=new Object" + "();faLink.href='#';s.tl(faLink,'o','Form Analysis');s[f.vu]='';s.us" + "ePlugins=up}return f.ul&&e!='e'&&e!='s'?f.ul(e):true;");
                s_omntr.fam = new Function("e", "" + "var s=s_c_il[" + s_omntr._in + "],f=s.fa;if(!e) e=s.wd.event;var o=s.trackLas" + "tChanged,et=e.type.toUpperCase(),t=this.type.toUpperCase(),fn=this." + "form.name,en=this.name,sc=false;if(document.layers){kp=e.which;b=e." + "which}else{kp=e.keyCode;b=e.button}et=et=='MOUSEDOWN'?1:et=='KEYDOW" + "N'?2:et;if(f.ce!=en||f.cf!=fn){if(et==1&&b!=2&&'BUTTONSUBMITRESETIM" + "AGERADIOCHECKBOXSELECT-ONEFILE'.indexOf(t)>-1){f.va[1]=fn;f.va[3]=e" + "n;sc=true}else if(et==1&&b==2&&'TEXTAREAPASSWORDFILE'.indexOf(t)>-1" + "){f.va[1]=fn;f.va[3]=en;sc=true}else if(et==2&&kp!=9&&kp!=13){f.va[" + "1]=fn;f.va[3]=en;sc=true}if(sc){nface=en;nfacf=fn}}if(et==1&&this.s" + "_famd)return this.s_famd(e);if(et==2&&this.s_fakd)return this.s_fak" + "d(e);");
                s_omntr.ee = new Function("e", "n", "" + "return n&&n.toLowerCase?e.toLowerCase()==n.toLowerCase():false;");
                s_omntr.fage = new Function("e", "a", "" + "var s=this,f=s.fa,x=f.cnt;x=x?x+1:1;f.cnt=x;return x==a?e:'';");
                /* Plugin: clickPast - version 1.0 */
                s_omntr.clickPast = new Function("scp", "ct_ev", "cp_ev", "cpc", "" + "var s=this,scp,ct_ev,cp_ev,cpc,ev,tct;if(s.p_fo(ct_ev)==1){if(!cpc)" + "{cpc='s_cpc';}ev=s.events?s.events+',':'';if(scp){s.events=ev+ct_ev" + ";s.c_w(cpc,1,0);}else{if(s.c_r(cpc)>=1){s.events=ev+cp_ev;s.c_w(cpc" + ",0,0);}}}");
                /* Plugin: getQueryParam 2.1 - return query string parameter(s) */
                s_omntr.getQueryParam = new Function("p", "d", "u", "" + "var s=this,v='',i,t;d=d?d:'';u=u?u:(s.pageURL?s.pageURL:s.wd.locati" + "on);if(u=='f')u=s.gtfs().location;while(p){i=p.indexOf(',');i=i<0?p" + ".length:i;t=s.p_gpv(p.substring(0,i),u+'');if(t)v+=v?d+t:t;p=p.subs" + "tring(i==p.length?i:i+1)}return v");
                /* Plugin getFolderName (URL path)*/
                s_omntr.getFolderName = new Function("n", "" + "var p=s.wd.location.pathname.substring(0, document.location.pathname.lastIndexOf('/') + 1),pa=p.split('/');if(pa[0]==''){for(var " + "i=0;i<pa.length;i++){pa[i]=i!=pa.length?pa[i+1]:null;}}return n?pa[" + "parseInt(n)-1]:'';");

                s_omntr.p_gpv = new Function("k", "u", "" + "var s=this,v='',i=u.indexOf('?'),q;if(k&&i>-1){q=u.substring(i+1);v" + "=s.pt(q,'&','p_gvf',k)}return v");
                s_omntr.p_gvf = new Function("t", "k", "" + "if(t){var s=this,i=t.indexOf('='),p=i<0?t:t.substring(0,i),v=i<0?'T" + "rue':t.substring(i+1);if(p.toLowerCase()==k.toLowerCase())return s." + "epa(v)}return ''");
                /* Plugin: getValOnce_v1.11 - get a value once per session or number of days */
                s_omntr.getValOnce = new Function("v", "c", "e", "t", "" + "var s=this,a=new Date,v=v?v:'',c=c?c:'s_gvo',e=e?e:0,i=t=='m'?6000" + "0:86400000,k=s.c_r(c);if(v){a.setTime(a.getTime()+e*i);s.c_w(c,v,e" + "==0?0:a);}return v==k?'':v");
                /* Plugin: getTimeParting 1.4 - Set timeparting values based on time zone */
                s_omntr.getTimeParting = new Function("t", "z", "y", "" + "dc=new Date('1/1/2000');var f=15;var ne=8;if(dc.getDay()!=6||" + "dc.getMonth()!=0){return'Data Not Available'}else{;z=parseInt(z);" + "if(y=='2009'){f=8;ne=1};gmar=new Date('3/1/'+y);dsts=f-gmar.getDay(" + ");gnov=new Date('11/1/'+y);dste=ne-gnov.getDay();spr=new Date('3/'" + "+dsts+'/'+y);fl=new Date('11/'+dste+'/'+y);cd=new Date();" + "if(cd>spr&&cd<fl){z=z+1}else{z=z};utc=cd.getTime()+(cd.getTimezoneO" + "ffset()*60000);tz=new Date(utc + (3600000*z));thisy=tz.getFullYear(" + ");var days=['Sunday','Monday','Tuesday','Wednesday','Thursday','Fr" + "iday','Saturday'];if(thisy!=y){return'Data Not Available'}else{;thi" + "sh=tz.getHours();thismin=tz.getMinutes();thisd=tz.getDay();var dow=" + "days[thisd];var ap='AM';var dt='Weekday';var mint='00';if(thismin>1" + "5&&thismin<30){mint='15'}if(thismin>30&&thismin<45){mint='30'}if(th" + "ismin>45&&thismin<60){mint='45'}" + "if(thish>=12){ap='PM';thish=thish-12};if (thish==0){th" + "ish=12};if(thisd==6||thisd==0){dt='Weekend'};var timestring=thish+'" + ":'+mint+ap;var daystring=dow;var endstring=dt;if(t=='h'){return tim" + "estring}if(t=='d'){return daystring};if(t=='w'){return en" + "dstring}}};");
                /* Plugin: getNewRepeat 1.2 - Returns whether user is new or repeat */
                s_omntr.getNewRepeat = new Function("d", "cn", "" + "var s=this,e=new Date(),cval,sval,ct=e.getTime();d=d?d:30;cn=cn?cn:" + "'s_nr';e.setTime(ct+d*24*60*60*1000);cval=s.c_r(cn);if(cval.length=" + "=0){s.c_w(cn,ct+'-New',e);return'New';}sval=s.split(cval,'-');if(ct" + "-sval[0]<30*60*1000&&sval[1]=='New'){s.c_w(cn,ct+'-New',e);return'N" + "ew';}else{s.c_w(cn,ct+'-Repeat',e);return'Repeat';}");
                /* Plugin: getPreviousValue_v1.0 - return previous value of designated variable (requires split utility)*/
                s_omntr.getPreviousValue = new Function("v", "c", "el", "" + "var s=this,t=new Date,i,j,r='';t.setTime(t.getTime()+1800000);if(el" + "){if(s.events){i=s.split(el,',');j=s.split(s.events,',');for(x in i" + "){for(y in j){if(i[x]==j[y]){if(s.c_r(c)) r=s.c_r(c);v?s.c_w(c,v,t)" + ":s.c_w(c,'no value',t);return r}}}}}else{if(s.c_r(c)) r=s.c_r(c);v?" + "s.c_w(c,v,t):s.c_w(c,'no value',t);return r}");
                /* Plugin: getPercentPageViewed v1.71 */
                s_omntr.getPercentPageViewed = new Function("n", "" + "var s=this,W=window,EL=W.addEventListener,AE=W.attachEvent,E=['load" + "','unload','scroll','resize','zoom','keyup','mouseup','touchend','o" + "rientationchange','pan'];W.s_Obj=s;s_PPVid=(n=='-'?s.pageName:n)||s" + ".pageName||location.href;if(!W.s_PPVevent){s.s_PPVg=function(n,r){v" + "ar k='s_ppv',p=k+'l',c=s.c_r(n||r?k:p),a=c.indexOf(',')>-1?c.split(" + "',',10):[''],l=a.length,i;a[0]=unescape(a[0]);r=r||(n&&n!=a[0])||0;" + "a.length=10;if(typeof a[0]!='string')a[0]='';for(i=1;i<10;i++)a[i]=" + "!r&&i<l?parseInt(a[i])||0:0;if(l<10||typeof a[9]!='string')a[9]='';" + "if(r){s.c_w(p,c);s.c_w(k,'?')}return a};W.s_PPVevent=function(e){va" + "r W=window,D=document,B=D.body,E=D.documentElement,S=window.screen|" + "|0,Ho='offsetHeight',Hs='scrollHeight',Ts='scrollTop',Wc='clientWid" + "th',Hc='clientHeight',C=100,M=Math,J='object',N='number',s=W.s_Obj|" + "|W.s||0;e=e&&typeof e==J?e.type||'':'';if(!e.indexOf('on'))e=e.subs" + "tring(2);s_PPVi=W.s_PPVi||0;if(W.s_PPVt&&!e){clearTimeout(s_PPVt);s" + "_PPVt=0;if(s_PPVi<2)s_PPVi++}if(typeof s==J){var h=M.max(B[Hs]||E[H" + "s],B[Ho]||E[Ho],B[Hc]||E[Hc]),X=W.innerWidth||E[Wc]||B[Wc]||0,Y=W.i" + "nnerHeight||E[Hc]||B[Hc]||0,x=S?S.width:0,y=S?S.height:0,r=M.round(" + "C*(W.devicePixelRatio||1))/C,b=(D.pageYOffset||E[Ts]||B[Ts]||0)+Y,p" + "=h>0&&b>0?M.round(C*b/h):0,O=W.orientation,o=!isNaN(O)?M.abs(o)%180" + ":Y>X?0:90,L=e=='load'||s_PPVi<1,a=s.s_PPVg(s_PPVid,L),V=function(i," + "v,f,n){i=parseInt(typeof a==J&&a.length>i?a[i]:'0')||0;v=typeof v!=" + "N?i:v;v=f||v>i?v:i;return n?v:v>C?C:v<0?0:v};if(new RegExp('(iPod|i" + "Pad|iPhone)').exec(navigator.userAgent||'')&&o){o=x;x=y;y=o}o=o?'P'" + ":'L';a[9]=L?'':a[9].substring(0,1);s.c_w('s_ppv',escape(W.s_PPVid)+" + "','+V(1,p,L)+','+(L||!V(2)?p:V(2))+','+V(3,b,L,1)+','+X+','+Y+','+x" + "+','+y+','+r+','+a[9]+(a[9]==o?'':o))}if(!W.s_PPVt&&e!='unload')W.s" + "_PPVt=setTimeout(W.s_PPVevent,333)};for(var f=W.s_PPVevent,i=0;i<E." + "length;i++)if(EL)EL(E[i],f,false);else if(AE)AE('on'+E[i],f);f()};v" + "ar a=s.s_PPVg();return!n||n=='-'?a[1]:a");
                //s_omntr.getPPVCalc = new Function("", "" + "var s=s_omntr; var dh=Math.max(Math.max(s.d.body.scrollHeight,s.d.documentElement." + "scrollHeight),Math.max(s.d.body.offsetHeight,s.d.documentElement.of" + "fsetHeight),Math.max(s.d.body.clientHeight,s.d.documentElement.clie" + "ntHeight)),vph=s.d.clientHeight||Math.min(s.d.documentElement.clien" + "tHeight,s.d.body.clientHeight),st=s.wd.pageYOffset||(s.wd.document." + "documentElement.scrollTop||s.wd.document.body.scrollTop),vh=st+vph," + "pv=Math.round(vh/dh*100),cp=s.c_r('s_ppv');if(pv>100){s.c_w('s_ppv'" + ",'');}else if(pv>cp){s.c_w('s_ppv',pv);}");
                //s_omntr.getPPVSetup = new Function("", "" + "var s=this; if(s.wd.addEventListener){s.wd.addEventListener('load',s.getPPVCalc" + ",false);s.wd.addEventListener('scroll',s.getPPVCalc,false);s.wd.add" + "EventListener('resize',s.getPPVCalc,false);}else if(s.wd.attachEven" + "t){s.wd.attachEvent('onload',s.getPPVCalc);s.wd.attachEvent('onscro" + "ll',s.getPPVCalc);s.wd.attachEvent('onresize',s.getPPVCalc);}");
                //s_omntr.getPPVSetup();
                /* Plugin: crossVisitParticipation v1.9 with Recency */
                s_omntr.crossVisitParticipation = new Function("v", "cn", "ex", "ct", "dl", "ev", "dv", "" + "var s=this,ce;if(typeof(dv)==='undefined')dv=0;if(s.events&&ev){var" + " ay=s.split(ev,',');var ea=s.split(s.events,',');for(var u=0;u<ay.l" + "ength;u++){for(var x=0;x<ea.length;x++){if(ay[u]==ea[x]){ce=1;}}}}i" + "f(!v||v==''){if(ce){s.c_w(cn,'');return'';}else return'';}v=escape(" + "v);var arry=new Array(),a=new Array(),c=s.c_r(cn),g=0,h=new Array()" + ";if(c&&c!=''){arry=s.split(c,'],[');for(q=0;q<arry.length;q++){z=ar" + "ry[q];z=s.repl(z,'[','');z=s.repl(z,']','');z=s.repl(z,\"'\",'');arry" + "[q]=s.split(z,',')}}var e=new Date();e.setFullYear(e.getFullYear()+" + "5);if(dv==0&&arry.length>0&&arry[arry.length-1][0]==v)arry[arry.len" + "gth-1]=[v,new Date().getTime()];else arry[arry.length]=[v,new Date(" + ").getTime()];var start=arry.length-ct<0?0:arry.length-ct;var td=new" + " Date();for(var x=start;x<arry.length;x++){var diff=Math.round((td." + "getTime()-arry[x][1])/86400000);if(diff<ex){h[g]=unescape(arry[x][0" + "]);a[g]=[arry[x][0],arry[x][1]];g++;}}var data=s.join(a,{delim:','," + "front:'[',back:']',wrap:\"'\"});s.c_w(cn,data,e);var r=s.join(h,{deli" + "m:dl});if(ce)s.c_w(cn,'');return r;");
                s_omntr.CVPwRecency = function (cn) {
                    var s = this;
                    arry = s.split(s.c_r(cn).toLowerCase(), '],[');
                    for (q = 0; q < arry.length; q++) {
                        z = arry[q];
                        z = s.repl(z, '[', '');
                        z = s.repl(z, ']', '');
                        z = s.repl(z, "'", '');
                        z = s.repl(z, "%20", ' ');
                        arry[q] = s.split(z, ',');
                    }
                    var td = new Date();
                    var recent = '';
                    var chan = '';
                    for (var x = 0; x < arry.length; x++) {
                        var diff = Math.round((td.getTime() - arry[x][1]) / 86400000);
                        if (x != 0) {
                            recent = recent + '>';
                            chan = chan + '>';
                        }
                        chan = chan + arry[x][0];
                        recent = diff <= 0 ? recent + 'S' : recent;
                        recent = diff >= 1 && diff <= 3 ? recent + '1-3' : recent;
                        recent = diff >= 4 && diff <= 14 ? recent + '4-14' : recent;
                        recent = diff >= 15 && diff <= 30 ? recent + '15-30' : recent;
                        recent = diff > 30 ? recent + '30+' : recent;
                    }
                    if (chan && recent) {
                        return chan + '::' + recent;
                    } else {
                        return '';
                    }
                }; /* Plugin: downloadLinkHandler 0.5 - identify and report download links */
                s_omntr.downloadLinkHandler = new Function("p", "" + "var s=this,h=s.p_gh(),n='linkDownloadFileTypes',i,t;if(!h||(s.linkT" + "ype&&(h||s.linkName)))return '';i=h.indexOf('?');t=s[n];s[n]=p?p:t;" + "if(s.lt(h)=='d')s.linkType='d';else h='';s[n]=t;return h;");
                /* Plugin: DynamicObjectIDs v1.5 */
                //s_omntr.setupDynamicObjectIDs = new Function("" + "var s=this;if(!s.doi){s.doi=1;if(s.apv>3&&(!s.isie||!s.ismac||s.apv" + ">=5)){if(s.wd.attachEvent)s.wd.attachEvent('onload',s.setOIDs);else" + " if(s.wd.addEventListener)s.wd.addEventListener('load',s.setOIDs,fa" + "lse);else{s.doiol=s.wd.onload;s.wd.onload=s.setOIDs}}s.wd.s_semapho" + "re=1}");
                s_omntr.setOIDs = new Function("e", "" + "var s=s_c_il[" + s_omntr._in + "],b=s.eh(s.wd,'onload'),o='onclick',x,l,u,c,i" + ",a=new Array;if(s.doiol){if(b)s[b]=s.wd[b];s.doiol(e)}if(s.d.links)" + "{for(i=0;i<s.d.links.length;i++){l=s.d.links[i];c=l[o]?''+l[o]:'';b" + "=s.eh(l,o);z=l[b]?''+l[b]:'';u=s.getObjectID(l);if(u&&c.indexOf('s_" + "objectID')<0&&z.indexOf('s_objectID')<0){u=s.repl(u,'\"','');u=s.re" + "pl(u,'\\n','').substring(0,97);l.s_oc=l[o];a[u]=a[u]?a[u]+1:1;x='';" + "if(l.href.indexOf('javascript:void')<0&&l.className.indexOf('tracklink')<0){" + "if(c.indexOf('.t(')>=0||c.indexOf('.tl(')>=0||c.indexOf('s_gs(')>=0" + ")x='var x=\".tl(\";';x+='s_objectID=\"'+u+'_'+a[u]+'\";return this." + "s_oc?this.s_oc(e):true';" + "if(s.isns&&s.apv>=5)l.setAttribute(o,x);l[o]=new Function('e',x);" + "}}}}s.wd.s_semaphore=0;return true");
                /* Plugin: Days since last Visit 1.0.H */
                s_omntr.getDaysSinceLastVisit = new Function("" + "var s=this,e=new Date(),cval,ct=e.getTime(),c='s_lastvisit',day=24*" + "60*60*1000;e.setTime(ct+3*365*day);cval=s.c_r(c);if(!cval){s.c_w(c," + "ct,e);return 'First page view or cookies not supported';}else{var d" + "=ct-cval;if(d>30*60*1000){if(d>30*day){s.c_w(c,ct,e);return 'More t" + "han 30 days';}if(d<30*day+1 && d>7*day){s.c_w(c,ct,e);return 'More " + "than 7 days';}if(d<7*day+1 && d>day){s.c_w(c,ct,e);return 'Less tha" + "n 7 days';}if(d<day+1){s.c_w(c,ct,e);return 'Less than 1 day';}}els" + "e return '';}");
                /* Plugin: Visit Number By Month 2.0 */
                s_omntr.getVisitNum = new Function("" + "var s=this,e=new Date(),cval,cvisit,ct=e.getTime(),c='s_vnum',c2='s" + "_invisit';e.setTime(ct+30*24*60*60*1000);cval=s.c_r(c);if(cval){var" + " i=cval.indexOf('&vn='),str=cval.substring(i+4,cval.length),k;}cvis" + "it=s.c_r(c2);if(cvisit){if(str){e.setTime(ct+30*60*1000);s.c_w(c2,'" + "true',e);return str;}else return 'unknown visit number';}else{if(st" + "r){str++;k=cval.substring(0,i);e.setTime(k);s.c_w(c,k+'&vn='+str,e)" + ";e.setTime(ct+30*60*1000);s.c_w(c2,'true',e);return str;}else{s.c_w" + "(c,ct+30*24*60*60*1000+'&vn=1',e);e.setTime(ct+30*60*1000);s.c_w(c2" + ",'true',e);return 1;}}");
                /* Plugin: getVisitStart v2.0 */
                s_omntr.getVisitStart = new Function("c", "" + "var s=this,v=1,t=new Date;t.setTime(t.getTime()+1800000);if(s.c_r(c" + ")){v=0}if(!s.c_w(c,1,t)){s.c_w(c,1,0)}if(!s.c_r(c)){v=0}return v;");
                /* Plugin: detectRIA v0.1 - detect and set Flash, Silverlight versions */
                //s_omntr.detectRIA = new Function("cn", "fp", "sp", "mfv", "msv", "sf", "" + "cn=cn?cn:'s_ria';msv=msv?msv:2;mfv=mfv?mfv:10;var s=this,sv='',fv=-" + "1,dwi=0,fr='',sr='',w,mt=s.n.mimeTypes,uk=s.c_r(cn),k=s.c_w('s_cc'," + "'true',0)?'Y':'N';fk=uk.substring(0,uk.indexOf('|'));sk=uk.substrin" + "g(uk.indexOf('|')+1,uk.length);if(k=='Y'&&s.p_fo('detectRIA')){if(u" + "k&&!sf){if(fp){s[fp]=fk;}if(sp){s[sp]=sk;}return false;}if(!fk&&fp)" + "{if(s.pl&&s.pl.length){if(s.pl['Shockwave Flash 2.0'])fv=2;x=s.pl['" + "Shockwave Flash'];if(x){fv=0;z=x.description;if(z)fv=z.substring(16" + ",z.indexOf('.'));}}else if(navigator.plugins&&navigator.plugins.len" + "gth){x=navigator.plugins['Shockwave Flash'];if(x){fv=0;z=x.descript" + "ion;if(z)fv=z.substring(16,z.indexOf('.'));}}else if(mt&&mt.length)" + "{x=mt['application/x-shockwave-flash'];if(x&&x.enabledPlugin)fv=0;}" + "if(fv<=0)dwi=1;w=s.u.indexOf('Win')!=-1?1:0;if(dwi&&s.isie&&w&&exec" + "Script){result=false;for(var i=mfv;i>=3&&result!=true;i--){execScri" + "pt('on error resume next: result = IsObject(CreateObject(\"Shockwav" + "eFlash.ShockwaveFlash.'+i+'\"))','VBScript');fv=i;}}fr=fv==-1?'flas" + "h not detected':fv==0?'flash enabled (no version)':'flash '+fv;}if(" + "!sk&&sp&&s.apv>=4.1){var tc='try{x=new ActiveXObject(\"AgControl.A'" + "+'gControl\");for(var i=msv;i>0;i--){for(var j=9;j>=0;j--){if(x.is'" + "+'VersionSupported(i+\".\"+j)){sv=i+\".\"+j;break;}}if(sv){break;}'" + "+'}}catch(e){try{x=navigator.plugins[\"Silverlight Plug-In\"];sv=x'" + "+'.description.substring(0,x.description.indexOf(\".\")+2);}catch('" + "+'e){}}';eval(tc);sr=sv==''?'silverlight not detected':'silverlight" + " '+sv;}if((fr&&fp)||(sr&&sp)){s.c_w(cn,fr+'|'+sr,0);if(fr)s[fp]=fr;" + "if(sr)s[sp]=sr;}}");
                /* Plugin: getTimeToComplete 0.4 */
                s_omntr.getTimeToComplete = new Function("v", "cn", "e", "" + "var s=this,d=new Date,x=d,k;if(!s.ttcr){e=e?e:0;if(v=='start'||v=='" + "stop')s.ttcr=1;x.setTime(x.getTime()+e*86400000);if(v=='start'){s.c" + "_w(cn,d.getTime(),e?x:0);return '';}if(v=='stop'){k=s.c_r(cn);if(!s" + ".c_w(cn,'',d)||!k)return '';v=(d.getTime()-k)/1000;var td=86400,th=" + "3600,tm=60,r=5,u,un;if(v>td){u=td;un='days';}else if(v>th){u=th;un=" + "'hours';}else if(v>tm){r=2;u=tm;un='minutes';}else{r=.2;u=1;un='sec" + "onds';}v=v*r/u;return (Math.round(v)/r)+' '+un;}}return '';");
                /* channelManager v2.4 - Tracking External Traffic */
                s_omntr.channelManager = new Function("a", "b", "c", "d", "e", "f", "" + "var s=this,A,B,g,l,m,M,p,q,P,h,k,u,S,i,O,T,j,r,t,D,E,F,G,H,N,U,v=0," + "X,Y,W,n=new Date;n.setTime(n.getTime()+1800000);if(e){v=1;if(s.c_r(" + "e)){v=0}if(!s.c_w(e,1,n)){s.c_w(e,1,0)}if(!s.c_r(e)){v=0}}g=s.refer" + "rer?s.referrer:document.referrer;g=g.toLowerCase();if(!g){h=1}i=g.i" + "ndexOf('?')>-1?g.indexOf('?'):g.length;j=g.substring(0,i);k=s.linkI" + "nternalFilters.toLowerCase();k=s.split(k,',');l=k.length;for(m=0;m<" + "l;m++){B=j.indexOf(k[m])==-1?'':g;if(B)O=B}if(!O&&!h){p=g;U=g.index" + "Of('//');q=U>-1?U+2:0;Y=g.indexOf('/',q);r=Y>-1?Y:i;t=g.substring(q" + ",r);t=t.toLowerCase();u=t;P='Referrers';S=s.seList+'>'+s._extraSear" + "chEngines;if(d==1){j=s.repl(j,'oogle','%');j=s.repl(j,'ahoo','^');g" + "=s.repl(g,'as_q','*')}A=s.split(S,'>');T=A.length;for(i=0;i<T;i++){" + "D=A[i];D=s.split(D,'|');E=s.split(D[0],',');F=E.length;for(G=0;G<F;" + "G++){H=j.indexOf(E[G]);if(H>-1){i=s.split(D[1],',');U=i.length;for(" + "k=0;k<U;k++){l=s.util.getQueryParam(i[k],'',g);if(l){l=l.toLowerCase();M" + "=l;if(D[2]){u=D[2];N=D[2]}else{N=t}if(d==1){N=s.repl(N,'#',' - ');g" + "=s.repl(g,'*','as_q');N=s.repl(N,'^','ahoo');N=s.repl(N,'%','oogle'" + ");}}}}}}}if(!O||f!='1'){O=s.getQueryParam(a,b);if(O){u=O;if(M){P='P" + "aid Search'}else{P='Paid Non-Search';}}if(!O&&M){u=N;P='Natural Sea" + "rch'}}if(h==1&&!O&&v==1){u=P=t=p='Direct Load'}X=M+u+t;c=c?c:'c_m';" + "if(c!='0'){X=s.getValOnce(X,c,0);}g=s._channelDomain;if(g&&X){k=s.s" + "plit(g,'>');l=k.length;for(m=0;m<l;m++){q=s.split(k[m],'|');r=s.spl" + "it(q[1],',');S=r.length;for(T=0;T<S;T++){Y=r[T];Y=Y.toLowerCase();i" + "=j.indexOf(Y);if(i>-1)P=q[0]}}}g=s._channelParameter;if(g&&X){k=s.s" + "plit(g,'>');l=k.length;for(m=0;m<l;m++){q=s.split(k[m],'|');r=s.spl" + "it(q[1],',');S=r.length;for(T=0;T<S;T++){U=s.getQueryParam(r[T]);if" + "(U)P=q[0]}}}g=s._channelPattern;if(g&&X){k=s.split(g,'>');l=k.lengt" + "h;for(m=0;m<l;m++){q=s.split(k[m],'|');r=s.split(q[1],',');S=r.leng" + "th;for(T=0;T<S;T++){Y=r[T];Y=Y.toLowerCase();i=O.toLowerCase();H=i." + "indexOf(Y);if(H==0)P=q[0]}}}if(X)M=M?M:'n/a';p=X&&p?p:'';t=X&&t?t:'" + "';N=X&&N?N:'';O=X&&O?O:'';u=X&&u?u:'';M=X&&M?M:'';P=X&&P?P:'';s._re" + "ferrer=p;s._referringDomain=t;s._partner=N;s._campaignID=O;s._campa" + "ign=u;s._keywords=M;s._channel=P");
                /* non-custom list */
                s_omntr.seList = "search.aol.com,search.aol.ca|query,q|AOL.com Search>ask.com" + ",ask.co.uk|ask,q|Ask Jeeves>google.co,googlesyndication.com|q,as_q|" + "Google>google.com.ar|q,as_q|Google - Argentina>google.com.au|q,as_q" + "|Google - Australia>google.be|q,as_q|Google - Belgium>google.com.br" + "|q,as_q|Google - Brasil>google.ca|q,as_q|Google - Canada>google.cl|" + "q,as_q|Google - Chile>google.cn|q,as_q|Google - China>google.com.co" + "|q,as_q|Google - Colombia>google.dk|q,as_q|Google - Denmark>google." + "com.do|q,as_q|Google - Dominican Republic>google.fi|q,as_q|Google -" + " Finland>google.fr|q,as_q|Google - France>google.de|q,as_q|Google -" + " Germany>google.gr|q,as_q|Google - Greece>google.com.hk|q,as_q|Goog" + "le - Hong Kong>google.co.in|q,as_q|Google - India>google.co.id|q,as" + "_q|Google - Indonesia>google.ie|q,as_q|Google - Ireland>google.co.i" + "l|q,as_q|Google - Israel>google.it|q,as_q|Google - Italy>google.co." + "jp|q,as_q|Google - Japan>google.com.my|q,as_q|Google - Malaysia>goo" + "gle.com.mx|q,as_q|Google - Mexico>google.nl|q,as_q|Google - Netherl" + "ands>google.co.nz|q,as_q|Google - New Zealand>google.com.pk|q,as_q|" + "Google - Pakistan>google.com.pe|q,as_q|Google - Peru>google.com.ph|" + "q,as_q|Google - Philippines>google.pl|q,as_q|Google - Poland>google" + ".pt|q,as_q|Google - Portugal>google.com.pr|q,as_q|Google - Puerto R" + "ico>google.ro|q,as_q|Google - Romania>google.com.sg|q,as_q|Google -" + " Singapore>google.co.za|q,as_q|Google - South Africa>google.es|q,as" + "_q|Google - Spain>google.se|q,as_q|Google - Sweden>google.ch|q,as_q" + "|Google - Switzerland>google.co.th|q,as_q|Google - Thailand>google." + "com.tr|q,as_q|Google - Turkey>google.co.uk|q,as_q|Google - United K" + "ingdom>google.co.ve|q,as_q|Google - Venezuela>bing.com|q|Microsoft " + "Bing>naver.com,search.naver.com|query|Naver>yahoo.com,search.yahoo." + "com|p|Yahoo!>ca.yahoo.com,ca.search.yahoo.com|p|Yahoo! - Canada>yah" + "oo.co.jp,search.yahoo.co.jp|p,va|Yahoo! - Japan>sg.yahoo.com,sg.sea" + "rch.yahoo.com|p|Yahoo! - Singapore>uk.yahoo.com,uk.search.yahoo.com" + "|p|Yahoo! - UK and Ireland>search.cnn.com|query|CNN Web Search>sear" + "ch.earthlink.net|q|Earthlink Search>search.comcast.net|q|Comcast Se" + "arch>search.rr.com|qs|RoadRunner Search>optimum.net|q|Optimum Searc" + "h";
                /* Plugin: exitLinkHandler 0.5 - identify and report exit links */
                s_omntr.exitLinkHandler = new Function("p", "" + "var s=this,h=s.p_gh(),n='linkInternalFilters',i,t;if(!h||(s.linkTyp" + "e&&(h||s.linkName)))return '';i=h.indexOf('?');t=s[n];s[n]=p?p:t;h=" + "s.linkLeaveQueryString||i<0?h:h.substring(0,i);if(s.lt(h)=='e')s.li" + "nkType='e';else h='';s[n]=t;return h;");
                /* Plugin: s_getLoadTime v1.36 - Get page load time in units of 1/10 seconds */

                s_omntr.s_getLoadTime = function s_getLoadTime() {
                    if (!window.s_loadT) {
                        var b = new Date().getTime(),
                            o = window.performance ? performance.timing : 0,
                            a = o ? o.requestStart : window.inHeadTS || 0;
                        s_loadT = a ? Math.round((b - a) / 100) : '';
                    }
                    return s_loadT;
                };

                /* Get Full Referring Domains */
                s_omntr.getFullReferringDomains = new Function("" + "var s=this,dr=window.document.referrer,n=s.linkInternalFilters.spli" + "t(',');if(dr){var r=dr.split('/')[2],l=n.length;for(i=0;i<=l;i++){i" + "f(r.indexOf(n[i])!=-1){r='';i=l+1;}}return r}");

                /* Configure Modules and Plugins */
                s_omntr.loadModule("Media");
                /* Media Variables   */
                s_omntr.Media.autoTrack = false;
                s_omntr.Media.trackMilestones = "10,25,50,75,90";
                s_omntr.Media.segmentByMilestones = true;
                s_omntr.Media.playerName = "Flowplayer";
                s_omntr.Media.trackWhilePlaying = true;
                s_omntr.Media.trackUsingContextData = true;
                s_omntr.Media.trackVars = "events,eVar40,eVar41,eVar42,eVar43,eVar44,prop36";
                s_omntr.Media.trackEvents = "event40,event41,event42,event43,event44,event45,event46,event47,event48";
                s_omntr.Media.contextDataMapping = {
                    "a.media.name": "eVar40,prop36",
                    "a.media.segment": "eVar41",
                    "a.contentType": "eVar42",
                    "a.media.timePlayed": "event40",
                    "a.media.view": "event41",
                    "a.media.segmentView": "event42",
                    "a.media.complete": "event48",
                    "a.media.milestones": {
                        10: "event43",
                        25: "event44",
                        50: "event45",
                        75: "event46",
                        90: "event47"
                    }
                };

                /* WARNING: Changing any of the below variables will cause drastic
                changes to how your visitor data is collected.  Changes should only be
                made when instructed to do so by your account manager.*/
                s_omntr.visitorNamespace = "herbalife";
                s_omntr.trackingServer = "metrics.herbalife.com";
                s_omntr.trackingServerSecure = "smetrics.herbalife.com";
                s_omntr.dc = "122";


                /****************************** MODULES *****************************/
                /*Start ActivityMap Module
				
                 The following module enables ActivityMap tracking in Adobe Analytics. ActivityMap
                 allows you to view data overlays on your links and content to understand how
                 users engage with your web site. If you do not intend to use ActivityMap, you
                 can remove the following block of code from your AppMeasurement.js file.
                 Additional documentation on how to configure ActivityMap is available at:
                 https://marketing.adobe.com/resources/help/en_US/analytics/activitymap/getting-started-admins.html
                */
                function AppMeasurement_Module_ActivityMap(f) {
                    function g(a, d) {
                        var b, c, n;
                        if (a && d && (b = e.c[d] || (e.c[d] = d.split(","))))
                            for (n = 0; n < b.length && (c = b[n++]) ;)
                                if (-1 < a.indexOf(c)) return null;
                        p = 1;
                        return a;
                    }

                    function q(a, d, b, c, e) {
                        var g, h;
                        if (a.dataset && (h = a.dataset[d])) g = h;
                        else if (a.getAttribute)
                            if (h = a.getAttribute("data-" + b)) g = h;
                            else if (h = a.getAttribute(b)) g = h;
                        if (!g && f.useForcedLinkTracking && e && (g = "", d = a.onclick ? "" + a.onclick : "")) {
                            b = d.indexOf(c);
                            var l, k;
                            if (0 <= b) {
                                for (b += 10; b < d.length && 0 <= "= \t\r\n".indexOf(d.charAt(b)) ;) b++;
                                if (b < d.length) {
                                    h = b;
                                    for (l = k = 0; h < d.length && (";" != d.charAt(h) || l) ;) l ? d.charAt(h) != l || k ? k = "\\" == d.charAt(h) ? !k : 0 : l = 0 : (l = d.charAt(h), '"' != l && "'" != l && (l = 0)), h++;
                                    if (d = d.substring(b, h)) a.e = new Function("s", "var e;try{s.w." + c + "=" + d + "}catch(e){}"), a.e(f);
                                }
                            }
                        }
                        return g || e && f.w[c];
                    }

                    function r(a, d, b) {
                        var c;
                        return (c = e[d](a, b)) && (p ? (p = 0, c) : g(k(c), e[d + "Exclusions"]));
                    }

                    function s(a, d, b) {
                        var c;
                        if (a && !(1 === (c = a.nodeType) && (c = a.nodeName) && (c = c.toUpperCase()) && t[c]) && (1 === a.nodeType && (c = a.nodeValue) && (d[d.length] = c), b.a ||
                                b.t || b.s || !a.getAttribute || ((c = a.getAttribute("alt")) ? b.a = c : (c = a.getAttribute("title")) ? b.t = c : "IMG" == ("" + a.nodeName).toUpperCase() && (c = a.getAttribute("src") || a.src) && (b.s = c)), (c = a.childNodes) && c.length))
                            for (a = 0; a < c.length; a++) s(c[a], d, b);
                    }

                    function k(a) {
                        if (null == a || void 0 == a) return a;
                        try {
                            return a.replace(RegExp("^[\\s\\n\\f\\r\\t\t-\r \u00a0\u1680\u180e\u2000-\u200a\u2028\u2029\u205f\u3000\ufeff]+", "mg"), "").replace(RegExp("[\\s\\n\\f\\r\\t\t-\r \u00a0\u1680\u180e\u2000-\u200a\u2028\u2029\u205f\u3000\ufeff]+$",
                                "mg"), "").replace(RegExp("[\\s\\n\\f\\r\\t\t-\r \u00a0\u1680\u180e\u2000-\u200a\u2028\u2029\u205f\u3000\ufeff]{1,}", "mg"), " ").substring(0, 254);
                        } catch (d) { }
                    }
                    var e = this;
                    e.s = f;
                    var m = window;
                    m.s_c_in || (m.s_c_il = [], m.s_c_in = 0);
                    e._il = m.s_c_il;
                    e._in = m.s_c_in;
                    e._il[e._in] = e;
                    m.s_c_in++;
                    e._c = "s_m";
                    e.c = {};
                    var p = 0,
                        t = {
                            SCRIPT: 1,
                            STYLE: 1,
                            LINK: 1,
                            CANVAS: 1
                        };
                    e._g = function () {
                        var a, d, b, c = f.contextData,
                            e = f.linkObject;
                        (a = f.pageName || f.pageURL) && (d = r(e, "link", f.linkName)) && (b = r(e, "region")) && (c["a.activitymap.page"] = a.substring(0,
                            255), c["a.activitymap.link"] = 128 < d.length ? d.substring(0, 128) : d, c["a.activitymap.region"] = 127 < b.length ? b.substring(0, 127) : b, c["a.activitymap.pageIDType"] = f.pageName ? 1 : 0);
                    };
                    e.link = function (a, d) {
                        var b;
                        if (d) b = g(k(d), e.linkExclusions);
                        else if ((b = a) && !(b = q(a, "sObjectId", "s-object-id", "s_objectID", 1))) {
                            var c, f;
                            (f = g(k(a.innerText || a.textContent), e.linkExclusions)) || (s(a, c = [], b = {
                                a: void 0,
                                t: void 0,
                                s: void 0
                            }), (f = g(k(c.join("")))) || (f = g(k(b.a ? b.a : b.t ? b.t : b.s ? b.s : void 0))) || !(c = (c = a.tagName) && c.toUpperCase ? c.toUpperCase() :
                                "") || ("INPUT" == c || "SUBMIT" == c && a.value ? f = g(k(a.value)) : a.src && "IMAGE" == c && (f = g(k(a.src)))));
                            b = f;
                        }
                        return b;
                    };
                    e.region = function (a) {
                        for (var d, b = e.regionIDAttribute || "id"; a && (a = a.parentNode) ;) {
                            if (d = q(a, b, b, b)) return d;
                            if ("BODY" == a.nodeName) return "BODY";
                        }
                    };
                }
                /* End ActivityMap Module */

                /* Module: Media */
                function AppMeasurement_Module_Media(q) {
                    var b = this;
                    b.s = q;
                    q = window;
                    q.s_c_in || (q.s_c_il = [], q.s_c_in = 0);
                    b._il = q.s_c_il;
                    b._in = q.s_c_in;
                    b._il[b._in] = b;
                    q.s_c_in++;
                    b._c = "s_m";
                    b.list = [];
                    b.open = function (d, c, e, k) {
                        var f = {},
                            a = new Date,
                            l = "",
                            g;
                        c || (c = -1);
                        if (d && e) {
                            b.list || (b.list = {});
                            b.list[d] && b.close(d);
                            k && k.id && (l = k.id);
                            if (l)
                                for (g in b.list) !Object.prototype[g] && b.list[g] && b.list[g].R == l && b.close(b.list[g].name);
                            f.name = d;
                            f.length = c;
                            f.offset = 0;
                            f.e = 0;
                            f.playerName = b.playerName ? b.playerName : e;
                            f.R = l;
                            f.C = 0;
                            f.a = 0;
                            f.timestamp =
                                Math.floor(a.getTime() / 1E3);
                            f.k = 0;
                            f.u = f.timestamp;
                            f.c = -1;
                            f.n = "";
                            f.g = -1;
                            f.D = 0;
                            f.I = {};
                            f.G = 0;
                            f.m = 0;
                            f.f = "";
                            f.B = 0;
                            f.L = 0;
                            f.A = 0;
                            f.F = 0;
                            f.l = !1;
                            f.v = "";
                            f.J = "";
                            f.K = 0;
                            f.r = !1;
                            f.H = "";
                            f.complete = 0;
                            f.Q = 0;
                            f.p = 0;
                            f.q = 0;
                            b.list[d] = f;
                        }
                    };
                    b.openAd = function (d, c, e, k, f, a, l, g) {
                        var h = {};
                        b.open(d, c, e, g);
                        if (h = b.list[d]) h.l = !0, h.v = k, h.J = f, h.K = a, h.H = l;
                    };
                    b.M = function (d) {
                        var c = b.list[d];
                        b.list[d] = 0;
                        c && c.monitor && clearTimeout(c.monitor.interval);
                    };
                    b.close = function (d) {
                        b.i(d, 0, -1);
                    };
                    b.play = function (d, c, e, k) {
                        var f = b.i(d, 1, c, e, k);
                        f && !f.monitor &&
                            (f.monitor = {}, f.monitor.update = function () {
                                1 == f.k && b.i(f.name, 3, -1);
                                f.monitor.interval = setTimeout(f.monitor.update, 1E3);
                            }, f.monitor.update());
                    };
                    b.click = function (d, c) {
                        b.i(d, 7, c);
                    };
                    b.complete = function (d, c) {
                        b.i(d, 5, c);
                    };
                    b.stop = function (d, c) {
                        b.i(d, 2, c);
                    };
                    b.track = function (d) {
                        b.i(d, 4, -1);
                    };
                    b.P = function (d, c) {
                        var e = "a.media.",
                            k = d.linkTrackVars,
                            f = d.linkTrackEvents,
                            a = "m_i",
                            l, g = d.contextData,
                            h;
                        c.l && (e += "ad.", c.v && (g["a.media.name"] = c.v, g[e + "pod"] = c.J, g[e + "podPosition"] = c.K), c.G || (g[e + "CPM"] = c.H));
                        c.r && (g[e + "clicked"] = !0, c.r = !1);
                        g["a.contentType"] = "video" + (c.l ? "Ad" : "");
                        g["a.media.channel"] = b.channel;
                        g[e + "name"] = c.name;
                        g[e + "playerName"] = c.playerName;
                        0 < c.length && (g[e + "length"] = c.length);
                        g[e + "timePlayed"] = Math.floor(c.a);
                        0 < Math.floor(c.a) && (g[e + "timePlayed"] = Math.floor(c.a));
                        c.G || (g[e + "view"] = !0, a = "m_s", b.Heartbeat && b.Heartbeat.enabled && (a = c.l ? b.__primetime ? "mspa_s" : "msa_s" : b.__primetime ? "msp_s" : "ms_s"), c.G = 1);
                        c.f && (g[e + "segmentNum"] = c.m, g[e + "segment"] = c.f, 0 < c.B && (g[e + "segmentLength"] = c.B), c.A && 0 < c.a && (g[e + "segmentView"] = !0));
                        !c.Q && c.complete && (g[e + "complete"] = !0, c.S = 1);
                        0 < c.p && (g[e + "milestone"] = c.p);
                        0 < c.q && (g[e + "offsetMilestone"] = c.q);
                        if (k)
                            for (h in g) Object.prototype[h] || (k += ",contextData." + h);
                        l = g["a.contentType"];
                        d.pe = a;
                        d.pev3 = l;
                        var q, s;
                        if (b.contextDataMapping)
                            for (h in d.events2 || (d.events2 = ""), k && (k += ",events"), b.contextDataMapping)
                                if (!Object.prototype[h]) {
                                    a = h.length > e.length && h.substring(0, e.length) == e ? h.substring(e.length) : "";
                                    l = b.contextDataMapping[h];
                                    if ("string" == typeof l)
                                        for (q = l.split(","), s = 0; s < q.length; s++) l =
                                            q[s], "a.contentType" == h ? (k && (k += "," + l), d[l] = g[h]) : "view" == a || "segmentView" == a || "clicked" == a || "complete" == a || "timePlayed" == a || "CPM" == a ? (f && (f += "," + l), "timePlayed" == a || "CPM" == a ? g[h] && (d.events2 += (d.events2 ? "," : "") + l + "=" + g[h]) : g[h] && (d.events2 += (d.events2 ? "," : "") + l)) : "segment" == a && g[h + "Num"] ? (k && (k += "," + l), d[l] = g[h + "Num"] + ":" + g[h]) : (k && (k += "," + l), d[l] = g[h]);
                                    else if ("milestones" == a || "offsetMilestones" == a) h = h.substring(0, h.length - 1), g[h] && b.contextDataMapping[h + "s"][g[h]] && (f && (f += "," + b.contextDataMapping[h +
                                        "s"][g[h]]), d.events2 += (d.events2 ? "," : "") + b.contextDataMapping[h + "s"][g[h]]);
                                    g[h] && (g[h] = 0);
                                    "segment" == a && g[h + "Num"] && (g[h + "Num"] = 0);
                                }
                        d.linkTrackVars = k;
                        d.linkTrackEvents = f;
                    };
                    b.i = function (d, c, e, k, f) {
                        var a = {},
                            l = (new Date).getTime() / 1E3,
                            g, h, q = b.trackVars,
                            s = b.trackEvents,
                            t = b.trackSeconds,
                            u = b.trackMilestones,
                            v = b.trackOffsetMilestones,
                            w = b.segmentByMilestones,
                            x = b.segmentByOffsetMilestones,
                            p, n, r = 1,
                            m = {},
                            y;
                        b.channel || (b.channel = b.s.w.location.hostname);
                        if (a = d && b.list && b.list[d] ? b.list[d] : 0)
                            if (a.l && (t = b.adTrackSeconds,
                                    u = b.adTrackMilestones, v = b.adTrackOffsetMilestones, w = b.adSegmentByMilestones, x = b.adSegmentByOffsetMilestones), 0 > e && (e = 1 == a.k && 0 < a.u ? l - a.u + a.c : a.c), 0 < a.length && (e = e < a.length ? e : a.length), 0 > e && (e = 0), a.offset = e, 0 < a.length && (a.e = a.offset / a.length * 100, a.e = 100 < a.e ? 100 : a.e), 0 > a.c && (a.c = e), y = a.D, m.name = d, m.ad = a.l, m.length = a.length, m.openTime = new Date, m.openTime.setTime(1E3 * a.timestamp), m.offset = a.offset, m.percent = a.e, m.playerName = a.playerName, m.mediaEvent = 0 > a.g ? "OPEN" : 1 == c ? "PLAY" : 2 == c ? "STOP" : 3 == c ? "MONITOR" :
                                4 == c ? "TRACK" : 5 == c ? "COMPLETE" : 7 == c ? "CLICK" : "CLOSE", 2 < c || c != a.k && (2 != c || 1 == a.k)) {
                                f || (k = a.m, f = a.f);
                                if (c) {
                                    1 == c && (a.c = e);
                                    if ((3 >= c || 5 <= c) && 0 <= a.g && (r = !1, q = s = "None", a.g != e)) {
                                        h = a.g;
                                        h > e && (h = a.c, h > e && (h = e));
                                        p = u ? u.split(",") : 0;
                                        if (0 < a.length && p && e >= h)
                                            for (n = 0; n < p.length; n++) (g = p[n] ? parseFloat("" + p[n]) : 0) && h / a.length * 100 < g && a.e >= g && (r = !0, n = p.length, m.mediaEvent = "MILESTONE", a.p = m.milestone = g);
                                        if ((p = v ? v.split(",") : 0) && e >= h)
                                            for (n = 0; n < p.length; n++) (g = p[n] ? parseFloat("" + p[n]) : 0) && h < g && e >= g && (r = !0, n = p.length, m.mediaEvent =
                                                "OFFSET_MILESTONE", a.q = m.offsetMilestone = g);
                                    }
                                    if (a.L || !f) {
                                        if (w && u && 0 < a.length) {
                                            if (p = u.split(","))
                                                for (p.push("100"), n = h = 0; n < p.length; n++)
                                                    if (g = p[n] ? parseFloat("" + p[n]) : 0) a.e < g && (k = n + 1, f = "M:" + h + "-" + g, n = p.length), h = g;
                                        } else if (x && v && (p = v.split(",")))
                                            for (p.push("" + (0 < a.length ? a.length : "E")), n = h = 0; n < p.length; n++)
                                                if ((g = p[n] ? parseFloat("" + p[n]) : 0) || "E" == p[n]) {
                                                    if (e < g || "E" == p[n]) k = n + 1, f = "O:" + h + "-" + g, n = p.length;
                                                    h = g;
                                                }
                                        f && (a.L = !0);
                                    } (f || a.f) && f != a.f && (a.F = !0, a.f || (a.m = k, a.f = f), 0 <= a.g && (r = !0));
                                    (2 <= c || 100 <= a.e) && a.c < e &&
                                        (a.C += e - a.c, a.a += e - a.c);
                                    if (2 >= c || 3 == c && !a.k) a.n += (1 == c || 3 == c ? "S" : "E") + Math.floor(e), a.k = 3 == c ? 1 : c;
                                    !r && 0 <= a.g && 3 >= c && (t = t ? t : 0) && a.a >= t && (r = !0, m.mediaEvent = "SECONDS");
                                    a.u = l;
                                    a.c = e;
                                }
                                if (!c || 3 >= c && 100 <= a.e) 2 != a.k && (a.n += "E" + Math.floor(e)), c = 0, q = s = "None", m.mediaEvent = "CLOSE";
                                7 == c && (r = m.clicked = a.r = !0);
                                if (5 == c || b.completeByCloseOffset && (!c || 100 <= a.e) && 0 < a.length && e >= a.length - b.completeCloseOffsetThreshold) r = m.complete = a.complete = !0;
                                l = m.mediaEvent;
                                "MILESTONE" == l ? l += "_" + m.milestone : "OFFSET_MILESTONE" == l && (l +=
                                    "_" + m.offsetMilestone);
                                a.I[l] ? m.eventFirstTime = !1 : (m.eventFirstTime = !0, a.I[l] = 1);
                                m.event = m.mediaEvent;
                                m.timePlayed = a.C;
                                m.segmentNum = a.m;
                                m.segment = a.f;
                                m.segmentLength = a.B;
                                b.monitor && 4 != c && b.monitor(b.s, m);
                                b.Heartbeat && b.Heartbeat.enabled && 0 <= a.g && (r = !1);
                                0 == c && b.M(d);
                                r && a.D == y && (d = {
                                    contextData: {}
                                }, d.linkTrackVars = q, d.linkTrackEvents = s, d.linkTrackVars || (d.linkTrackVars = ""), d.linkTrackEvents || (d.linkTrackEvents = ""), b.P(d, a), d.linkTrackVars || (d["!linkTrackVars"] = 1), d.linkTrackEvents || (d["!linkTrackEvents"] =
                                    1), b.s.track(d), a.F ? (a.m = k, a.f = f, a.A = !0, a.F = !1) : 0 < a.a && (a.A = !1), a.n = "", a.p = a.q = 0, a.a -= Math.floor(a.a), a.g = e, a.D++);
                            }
                        return a;
                    };
                    b.O = function (d, c, e, k, f) {
                        var a = 0;
                        if (d && (!b.autoTrackMediaLengthRequired || c && 0 < c)) {
                            if (b.list && b.list[d]) a = 1;
                            else if (1 == e || 3 == e) b.open(d, c, "HTML5 Video", f), a = 1;
                            a && b.i(d, e, k, -1, 0);
                        }
                    };
                    b.attach = function (d) {
                        var c, e, k;
                        d && d.tagName && "VIDEO" == d.tagName.toUpperCase() && (b.o || (b.o = function (c, a, d) {
                            var e, h;
                            b.autoTrack && (e = c.currentSrc, (h = c.duration) || (h = -1), 0 > d && (d = c.currentTime), b.O(e, h, a,
                                d, c));
                        }), c = function () {
                            b.o(d, 1, -1);
                        }, e = function () {
                            b.o(d, 1, -1);
                        }, b.j(d, "play", c), b.j(d, "pause", e), b.j(d, "seeking", e), b.j(d, "seeked", c), b.j(d, "ended", function () {
                            b.o(d, 0, -1);
                        }), b.j(d, "timeupdate", c), k = function () {
                            d.paused || d.ended || d.seeking || b.o(d, 3, -1);
                            setTimeout(k, 1E3);
                        }, k());
                    };
                    b.j = function (b, c, e) {
                        b.attachEvent ? b.attachEvent("on" + c, e) : b.addEventListener && b.addEventListener(c, e, !1);
                    };
                    void 0 == b.completeByCloseOffset && (b.completeByCloseOffset = 1);
                    void 0 == b.completeCloseOffsetThreshold && (b.completeCloseOffsetThreshold =
                        1);
                    b.Heartbeat = {};
                    b.N = function () {
                        var d, c;
                        if (b.autoTrack && (d = b.s.d.getElementsByTagName("VIDEO")))
                            for (c = 0; c < d.length; c++) b.attach(d[c]);
                    };
                    b.j(q, "load", b.N);
                }



                var sendOmntrImage = false;

                //Check if is an ERROR page
                var page = window.location.pathname.toLowerCase().substring(window.location.pathname.toLowerCase().lastIndexOf('/') + 1);
                if (page.indexOf("pagenotfound") > -1) {
                    s_omntr.pageType = "errorPage";
                } else {
                    sendOmntrImage = setCustomValues(analyticsFacts);
                }

                try {
                    addthisConf(analyticsFacts.reportSuiteId);
                } catch (err) {
                    errorHandling(err);
                }

                try {
                    initDashNavListeners();
                } catch (err) {
                    errorHandling(err);
                }

                try {
                    customLinksConf();
                } catch (err) {
                    errorHandling(err);
                }

                // call doPlugins function
                s_omntr.doPlugins = s_omntr_doPlugins;


                if (sendOmntrImage) {
                    s_omntr.account = analyticsFacts.reportSuiteId;
                    if (window) {
                        window.s_account = s_omntr.account;
                    }
                }
            }
        } catch (err) {
            errorHandling(err);
        }
    }


    /* Global variables */

    function s_omntr_doPlugins(s_omntr) {
        try {
            /* Add calls to plugins here */

            // Internal Campaigns.
            s_omntr.prop41 = s_omntr.Util.getQueryParam('intcmp');

            // External Campaigns.
            var regexcmp = /[A|M]\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:\d){8}/; //Decided o use this more general form as many others wouldn´t be captured in case we use a stronger Regex
            var valCmp = s_omntr.Util.getQueryParam('cmp');
            var cleanCmp = regexcmp.exec(valCmp);
            if (cleanCmp) {
                s_omntr.campaign = cleanCmp; //Takes the clean value (No extra characters or double tracking codes) in case the regex meets the value
            } else {
                s_omntr.campaign = valCmp; //In case the CMP is wrongly formatted (doesn´t match the regex), capture it anyway to avoid losing wrong tracking codes (Better to capture it at least as an statistic)
            }

            s_omntr.campaign = s_omntr.getValOnce(s_omntr.campaign, "currentCampaign", "30", "m");

            //Capture EventID query string value in Events & Promotions pages
            if (s_omntr.prop19 && s_omntr.prop19 != '') { //Prop19 contains the DSID in case user is loggedIn                    
                s_omntr.prop43 = s_omntr.Util.getQueryParam('EventID');
                if (s_omntr.prop43) {
                    s_omntr.eVar43 = s_omntr.prop43;
                }
            }

            if (s_omntr.campaign) {
                // Custom 63. Event Variable - Disabled
                s_omntr.clickPast(s_omntr.firstPage, 'event62', 'event63');
                // Campaign Click Through. Event Variable - Counter
                s_omntr.events = s_omntr.apl(s_omntr.events, 'event53', ',', 1);
            }

            // Internal Site Search.
            s_omntr.prop12 = s_omntr.Util.getQueryParam('q');
            if (s_omntr.prop12) {
                // Internal Search Terms. Traffic Variable ex: prop12 = herbalife 
                s_omntr.prop12 = s_omntr.prop12;
                // Internal Search. Terms Conversion Variable ex: eVar5 = herbalife 
                s_omntr.eVar5 = s_omntr.prop12;
                var t_search = s_omntr.getValOnce(s_omntr.eVar5, 'ev5', 0);
                if (t_search) {
                    // Onsite Searches. Event Variable - Counter
                    s_omntr.events = s_omntr.apl(s_omntr.events, "event5", ",", 2);
                }
                // Search Orgination Page. Traffic Variable ex: prop11 = herbalife 
                s_omntr.prop11 = s_omntr.getPreviousValue(s_omntr.pageName, 'gpv_p11', 'event5');

                // Internal Search Pathing. Traffic Variable ex: prop30 = toolsandtrainingoverview:herbalife 
                s_omntr.prop30 = s_omntr.prop11 + ":" + s_omntr.prop12;

                // Campaign Click Through. Event Variable - Counter
                //s_omntr.events = s_omntr.apl(s_omntr.events, 'event53', ',', 1);
            }

            s_omntr.charSet = "UTF-8";

            /* Time Parting */
            var curdate = new Date();
            var temphr = s_omntr.getTimeParting('h', '-8', curdate.getFullYear());
            var tempday = s_omntr.getTimeParting('d', '-8', curdate.getFullYear());
            var temphour = curdate.getHours();
            var tempmin = curdate.getMinutes();
            if (tempmin < 10) {
                tempmin = "0" + tempmin;
            }
            var tempdate = curdate.getDate();
            if (tempdate < 10) {
                tempdate = '0' + tempdate;
            }
            var tempmonth = curdate.getMonth() + 1;
            if (tempmonth < 10) {
                tempmonth = '0' + tempmonth;
            }
            var tempyear = curdate.getFullYear();
            if (temphr) {
                s_omntr.prop53 = tempday + ":" + temphr;
                s_omntr.eVar53 = "D=c53";
                s_omntr.eVar54 = tempmonth + "/" + tempdate + "/" + tempyear;
            }

            // Full URL. Traffic Variable ex: prop50 = https://www.myherbalife.com/bizworks/mydownline.aspx?ReportType=SPVOLRPT    
            s_omntr.prop50 = window.location.href.toLowerCase();
            s_omntr.eVar48 = "D=c50";

            // URL. Traffic Variable ex: prop51 = https://www.myherbalife.com/bizworks/mydownline.aspx
            s_omntr.prop51 = window.location.protocol.toLowerCase() + "//" + window.location.host.toLowerCase() + window.location.pathname.toLowerCase();
            s_omntr.eVar49 = "D=c51";

            // Previous Page Name. Traffic Variable ex: prop65 = myhl:na:us:home*/
            var s_prevPage = s_omntr.getPreviousValue(s_omntr.pageName, "s_pv");
            s_omntr.prop65 = s_prevPage ? s_prevPage : '[No Previous Page Available]';

            s_omntr.prop55 = "";

            // Percent Page Viewed. Traffic Variable ex: prop55 = 71
            if (s_prevPage) {
                s_omntr.prop55 = s_omntr.getPercentPageViewed();
            }

            // New vs Repeat user. Traffic Variable ex: prop54 = Repeat
            s_omntr.prop54 = s_omntr.getNewRepeat();
            // Days Since Last Visit. Conversion Variable ex: eVar59 = 2 
            s_omntr.eVar59 = s_omntr.getDaysSinceLastVisit();
            // Days Since Last Visit. Traffic Variable ex: prop59 = 2
            s_omntr.prop59 = s_omntr.getDaysSinceLastVisit();
            // Omniture Visitor ID. Conversion Variable
            s_omntr.eVar60 = "D=s_vi"; //omniture visitor ID
            // Omniture Visitor ID. Traffic Variable
            s_omntr.prop68 = "D=s_vi";
            // Visit Number. Traffic Variable ex:  prop64 = 3
            s_omntr.prop64 = s_omntr.getVisitNum();
            // Visit Number. Traffic Variable
            s_omntr.eVar64 = s_omntr.getVisitNum();
            // Full Referring Domains. Traffic Variable ex: prop72 = www.google.com
            s_omntr.prop72 = s_omntr.getFullReferringDomains();
            // Full Referring Domains. Conversion Variable ex: 
            s_omntr.eVar72 = s_omntr.getFullReferringDomains();

            //Determine bounce rate for all visits
            var temp_prop = s_omntr.getVisitStart("s_visit");
            if (temp_prop && temp_prop == 1) {
                s_omntr.firstPage = 'firstpage';
            }
            /*Time to Load Tracking*/
            s_omntr.prop40 = s_omntr.s_getLoadTime();
            // First Page Event. Event Variabe - Counter
            s_omntr.clickPast(s_omntr.firstPage, 'event32', 'event33');

            s_omntr.prop75 = (typeof (Visitor) != "undefined" ? "VisitorAPI Present" : "VisitorAPI Missing");
        } catch (err) {
            errorHandling(err);
        }
    }

}

/*
 ============== DO NOT ALTER ANYTHING BELOW THIS LINE ! ===============

AppMeasurement for JavaScript version: 1.6.2
Copyright 1996-2016 Adobe, Inc. All Rights Reserved
More info available at http://www.adobe.com/marketing-cloud.html
*/
function AppMeasurement() {
    var a = this;
    a.version = "1.6.2";
    var k = window;
    k.s_c_in || (k.s_c_il = [], k.s_c_in = 0);
    a._il = k.s_c_il;
    a._in = k.s_c_in;
    a._il[a._in] = a;
    k.s_c_in++;
    a._c = "s_c";
    var q = k.AppMeasurement.Ib;
    q || (q = null);
    var r = k,
        n, t;
    try {
        for (n = r.parent, t = r.location; n && n.location && t && "" + n.location != "" + t && r.location && "" + n.location != "" + r.location && n.location.host == t.host;) r = n, n = r.parent
    } catch (u) { }
    a.xb = function (a) {
        try {
            console.log(a)
        } catch (b) { }
    };
    a.Ga = function (a) {
        return "" + parseInt(a) == "" + a
    };
    a.replace = function (a, b, d) {
        return !a ||
            0 > a.indexOf(b) ? a : a.split(b).join(d)
    };
    a.escape = function (c) {
        var b, d;
        if (!c) return c;
        c = encodeURIComponent(c);
        for (b = 0; 7 > b; b++) d = "+~!*()'".substring(b, b + 1), 0 <= c.indexOf(d) && (c = a.replace(c, d, "%" + d.charCodeAt(0).toString(16).toUpperCase()));
        return c
    };
    a.unescape = function (c) {
        if (!c) return c;
        c = 0 <= c.indexOf("+") ? a.replace(c, "+", " ") : c;
        try {
            return decodeURIComponent(c)
        } catch (b) { }
        return unescape(c)
    };
    a.ob = function () {
        var c = k.location.hostname,
            b = a.fpCookieDomainPeriods,
            d;
        b || (b = a.cookieDomainPeriods);
        if (c && !a.cookieDomain &&
            !/^[0-9.]+$/.test(c) && (b = b ? parseInt(b) : 2, b = 2 < b ? b : 2, d = c.lastIndexOf("."), 0 <= d)) {
            for (; 0 <= d && 1 < b;) d = c.lastIndexOf(".", d - 1), b--;
            a.cookieDomain = 0 < d ? c.substring(d) : c
        }
        return a.cookieDomain
    };
    a.c_r = a.cookieRead = function (c) {
        c = a.escape(c);
        var b = " " + a.d.cookie,
            d = b.indexOf(" " + c + "="),
            f = 0 > d ? d : b.indexOf(";", d);
        c = 0 > d ? "" : a.unescape(b.substring(d + 2 + c.length, 0 > f ? b.length : f));
        return "[[B]]" != c ? c : ""
    };
    a.c_w = a.cookieWrite = function (c, b, d) {
        var f = a.ob(),
            e = a.cookieLifetime,
            g;
        b = "" + b;
        e = e ? ("" + e).toUpperCase() : "";
        d && "SESSION" !=
            e && "NONE" != e && ((g = "" != b ? parseInt(e ? e : 0) : -60) ? (d = new Date, d.setTime(d.getTime() + 1E3 * g)) : 1 == d && (d = new Date, g = d.getYear(), d.setYear(g + 5 + (1900 > g ? 1900 : 0))));
        return c && "NONE" != e ? (a.d.cookie = c + "=" + a.escape("" != b ? b : "[[B]]") + "; path=/;" + (d && "SESSION" != e ? " expires=" + d.toGMTString() + ";" : "") + (f ? " domain=" + f + ";" : ""), a.cookieRead(c) == b) : 0
    };
    a.J = [];
    a.ga = function (c, b, d) {
        if (a.za) return 0;
        a.maxDelay || (a.maxDelay = 250);
        var f = 0,
            e = (new Date).getTime() + a.maxDelay,
            g = a.d.visibilityState,
            m = ["webkitvisibilitychange", "visibilitychange"];
        g || (g = a.d.webkitVisibilityState);
        if (g && "prerender" == g) {
            if (!a.ha)
                for (a.ha = 1, d = 0; d < m.length; d++) a.d.addEventListener(m[d], function () {
                    var b = a.d.visibilityState;
                    b || (b = a.d.webkitVisibilityState);
                    "visible" == b && (a.ha = 0, a.delayReady())
                });
            f = 1;
            e = 0
        } else d || a.p("_d") && (f = 1);
        f && (a.J.push({
            m: c,
            a: b,
            t: e
        }), a.ha || setTimeout(a.delayReady, a.maxDelay));
        return f
    };
    a.delayReady = function () {
        var c = (new Date).getTime(),
            b = 0,
            d;
        for (a.p("_d") ? b = 1 : a.ua() ; 0 < a.J.length;) {
            d = a.J.shift();
            if (b && !d.t && d.t > c) {
                a.J.unshift(d);
                setTimeout(a.delayReady,
                    parseInt(a.maxDelay / 2));
                break
            }
            a.za = 1;
            a[d.m].apply(a, d.a);
            a.za = 0
        }
    };
    a.setAccount = a.sa = function (c) {
        var b, d;
        if (!a.ga("setAccount", arguments))
            if (a.account = c, a.allAccounts)
                for (b = a.allAccounts.concat(c.split(",")), a.allAccounts = [], b.sort(), d = 0; d < b.length; d++) 0 != d && b[d - 1] == b[d] || a.allAccounts.push(b[d]);
            else a.allAccounts = c.split(",")
    };
    a.foreachVar = function (c, b) {
        var d, f, e, g, m = "";
        e = f = "";
        if (a.lightProfileID) d = a.N, (m = a.lightTrackVars) && (m = "," + m + "," + a.la.join(",") + ",");
        else {
            d = a.g;
            if (a.pe || a.linkType) m = a.linkTrackVars,
                f = a.linkTrackEvents, a.pe && (e = a.pe.substring(0, 1).toUpperCase() + a.pe.substring(1), a[e] && (m = a[e].Gb, f = a[e].Fb));
            m && (m = "," + m + "," + a.F.join(",") + ",");
            f && m && (m += ",events,")
        }
        b && (b = "," + b + ",");
        for (f = 0; f < d.length; f++) e = d[f], (g = a[e]) && (!m || 0 <= m.indexOf("," + e + ",")) && (!b || 0 <= b.indexOf("," + e + ",")) && c(e, g)
    };
    a.r = function (c, b, d, f, e) {
        var g = "",
            m, p, k, w, n = 0;
        "contextData" == c && (c = "c");
        if (b) {
            for (m in b)
                if (!(Object.prototype[m] || e && m.substring(0, e.length) != e) && b[m] && (!d || 0 <= d.indexOf("," + (f ? f + "." : "") + m + ","))) {
                    k = !1;
                    if (n)
                        for (p =
                            0; p < n.length; p++) m.substring(0, n[p].length) == n[p] && (k = !0);
                    if (!k && ("" == g && (g += "&" + c + "."), p = b[m], e && (m = m.substring(e.length)), 0 < m.length))
                        if (k = m.indexOf("."), 0 < k) p = m.substring(0, k), k = (e ? e : "") + p + ".", n || (n = []), n.push(k), g += a.r(p, b, d, f, k);
                        else if ("boolean" == typeof p && (p = p ? "true" : "false"), p) {
                            if ("retrieveLightData" == f && 0 > e.indexOf(".contextData.")) switch (k = m.substring(0, 4), w = m.substring(4), m) {
                                case "transactionID":
                                    m = "xact";
                                    break;
                                case "channel":
                                    m = "ch";
                                    break;
                                case "campaign":
                                    m = "v0";
                                    break;
                                default:
                                    a.Ga(w) && ("prop" ==
                                        k ? m = "c" + w : "eVar" == k ? m = "v" + w : "list" == k ? m = "l" + w : "hier" == k && (m = "h" + w, p = p.substring(0, 255)))
                            }
                            g += "&" + a.escape(m) + "=" + a.escape(p)
                        }
                }
            "" != g && (g += "&." + c)
        }
        return g
    };
    a.usePostbacks = 0;
    a.rb = function () {
        var c = "",
            b, d, f, e, g, m, p, k, n = "",
            r = "",
            s = e = "";
        if (a.lightProfileID) b = a.N, (n = a.lightTrackVars) && (n = "," + n + "," + a.la.join(",") + ",");
        else {
            b = a.g;
            if (a.pe || a.linkType) n = a.linkTrackVars, r = a.linkTrackEvents, a.pe && (e = a.pe.substring(0, 1).toUpperCase() + a.pe.substring(1), a[e] && (n = a[e].Gb, r = a[e].Fb));
            n && (n = "," + n + "," + a.F.join(",") + ",");
            r && (r = "," + r + ",", n && (n += ",events,"));
            a.events2 && (s += ("" != s ? "," : "") + a.events2)
        }
        if (a.visitor && 1.5 <= parseFloat(a.visitor.version) && a.visitor.getCustomerIDs) {
            e = q;
            if (g = a.visitor.getCustomerIDs())
                for (d in g) Object.prototype[d] || (f = g[d], e || (e = {}), f.id && (e[d + ".id"] = f.id), f.authState && (e[d + ".as"] = f.authState));
            e && (c += a.r("cid", e))
        }
        a.AudienceManagement && a.AudienceManagement.isReady() && (c += a.r("d", a.AudienceManagement.getEventCallConfigParams()));
        for (d = 0; d < b.length; d++) {
            e = b[d];
            g = a[e];
            f = e.substring(0, 4);
            m = e.substring(4);
            !g && "events" == e && s && (g = s, s = "");
            if (g && (!n || 0 <= n.indexOf("," + e + ","))) {
                switch (e) {
                    case "supplementalDataID":
                        e = "sdid";
                        break;
                    case "timestamp":
                        e = "ts";
                        break;
                    case "dynamicVariablePrefix":
                        e = "D";
                        break;
                    case "visitorID":
                        e = "vid";
                        break;
                    case "marketingCloudVisitorID":
                        e = "mid";
                        break;
                    case "analyticsVisitorID":
                        e = "aid";
                        break;
                    case "audienceManagerLocationHint":
                        e = "aamlh";
                        break;
                    case "audienceManagerBlob":
                        e = "aamb";
                        break;
                    case "authState":
                        e = "as";
                        break;
                    case "pageURL":
                        e = "g";
                        255 < g.length && (a.pageURLRest = g.substring(255), g = g.substring(0,
                            255));
                        break;
                    case "pageURLRest":
                        e = "-g";
                        break;
                    case "referrer":
                        e = "r";
                        break;
                    case "vmk":
                    case "visitorMigrationKey":
                        e = "vmt";
                        break;
                    case "visitorMigrationServer":
                        e = "vmf";
                        a.ssl && a.visitorMigrationServerSecure && (g = "");
                        break;
                    case "visitorMigrationServerSecure":
                        e = "vmf";
                        !a.ssl && a.visitorMigrationServer && (g = "");
                        break;
                    case "charSet":
                        e = "ce";
                        break;
                    case "visitorNamespace":
                        e = "ns";
                        break;
                    case "cookieDomainPeriods":
                        e = "cdp";
                        break;
                    case "cookieLifetime":
                        e = "cl";
                        break;
                    case "variableProvider":
                        e = "vvp";
                        break;
                    case "currencyCode":
                        e =
                            "cc";
                        break;
                    case "channel":
                        e = "ch";
                        break;
                    case "transactionID":
                        e = "xact";
                        break;
                    case "campaign":
                        e = "v0";
                        break;
                    case "latitude":
                        e = "lat";
                        break;
                    case "longitude":
                        e = "lon";
                        break;
                    case "resolution":
                        e = "s";
                        break;
                    case "colorDepth":
                        e = "c";
                        break;
                    case "javascriptVersion":
                        e = "j";
                        break;
                    case "javaEnabled":
                        e = "v";
                        break;
                    case "cookiesEnabled":
                        e = "k";
                        break;
                    case "browserWidth":
                        e = "bw";
                        break;
                    case "browserHeight":
                        e = "bh";
                        break;
                    case "connectionType":
                        e = "ct";
                        break;
                    case "homepage":
                        e = "hp";
                        break;
                    case "events":
                        s && (g += ("" != g ? "," : "") + s);
                        if (r)
                            for (m =
                                g.split(","), g = "", f = 0; f < m.length; f++) p = m[f], k = p.indexOf("="), 0 <= k && (p = p.substring(0, k)), k = p.indexOf(":"), 0 <= k && (p = p.substring(0, k)), 0 <= r.indexOf("," + p + ",") && (g += (g ? "," : "") + m[f]);
                        break;
                    case "events2":
                        g = "";
                        break;
                    case "contextData":
                        c += a.r("c", a[e], n, e);
                        g = "";
                        break;
                    case "lightProfileID":
                        e = "mtp";
                        break;
                    case "lightStoreForSeconds":
                        e = "mtss";
                        a.lightProfileID || (g = "");
                        break;
                    case "lightIncrementBy":
                        e = "mti";
                        a.lightProfileID || (g = "");
                        break;
                    case "retrieveLightProfiles":
                        e = "mtsr";
                        break;
                    case "deleteLightProfiles":
                        e =
                            "mtsd";
                        break;
                    case "retrieveLightData":
                        a.retrieveLightProfiles && (c += a.r("mts", a[e], n, e));
                        g = "";
                        break;
                    default:
                        a.Ga(m) && ("prop" == f ? e = "c" + m : "eVar" == f ? e = "v" + m : "list" == f ? e = "l" + m : "hier" == f && (e = "h" + m, g = g.substring(0, 255)))
                }
                g && (c += "&" + e + "=" + ("pev" != e.substring(0, 3) ? a.escape(g) : g))
            }
            "pev3" == e && a.e && (c += a.e)
        }
        return c
    };
    a.D = function (a) {
        var b = a.tagName;
        if ("undefined" != "" + a.Lb || "undefined" != "" + a.Bb && "HTML" != ("" + a.Bb).toUpperCase()) return "";
        b = b && b.toUpperCase ? b.toUpperCase() : "";
        "SHAPE" == b && (b = "");
        b && (("INPUT" == b ||
            "BUTTON" == b) && a.type && a.type.toUpperCase ? b = a.type.toUpperCase() : !b && a.href && (b = "A"));
        return b
    };
    a.Ca = function (a) {
        var b = a.href ? a.href : "",
            d, f, e;
        d = b.indexOf(":");
        f = b.indexOf("?");
        e = b.indexOf("/");
        b && (0 > d || 0 <= f && d > f || 0 <= e && d > e) && (f = a.protocol && 1 < a.protocol.length ? a.protocol : l.protocol ? l.protocol : "", d = l.pathname.lastIndexOf("/"), b = (f ? f + "//" : "") + (a.host ? a.host : l.host ? l.host : "") + ("/" != h.substring(0, 1) ? l.pathname.substring(0, 0 > d ? 0 : d) + "/" : "") + b);
        return b
    };
    a.K = function (c) {
        var b = a.D(c),
            d, f, e = "",
            g = 0;
        return b &&
            (d = c.protocol, f = c.onclick, !c.href || "A" != b && "AREA" != b || f && d && !(0 > d.toLowerCase().indexOf("javascript")) ? f ? (e = a.replace(a.replace(a.replace(a.replace("" + f, "\r", ""), "\n", ""), "\t", ""), " ", ""), g = 2) : "INPUT" == b || "SUBMIT" == b ? (c.value ? e = c.value : c.innerText ? e = c.innerText : c.textContent && (e = c.textContent), g = 3) : "IMAGE" == b && c.src && (e = c.src) : e = a.Ca(c), e) ? {
                id: e.substring(0, 100),
                type: g
            } : 0
    };
    a.Jb = function (c) {
        for (var b = a.D(c), d = a.K(c) ; c && !d && "BODY" != b;)
            if (c = c.parentElement ? c.parentElement : c.parentNode) b = a.D(c), d = a.K(c);
        d && "BODY" != b || (c = 0);
        c && (b = c.onclick ? "" + c.onclick : "", 0 <= b.indexOf(".tl(") || 0 <= b.indexOf(".trackLink(")) && (c = 0);
        return c
    };
    a.Ab = function () {
        var c, b, d = a.linkObject,
            f = a.linkType,
            e = a.linkURL,
            g, m;
        a.ma = 1;
        d || (a.ma = 0, d = a.clickObject);
        if (d) {
            c = a.D(d);
            for (b = a.K(d) ; d && !b && "BODY" != c;)
                if (d = d.parentElement ? d.parentElement : d.parentNode) c = a.D(d), b = a.K(d);
            b && "BODY" != c || (d = 0);
            if (d && !a.linkObject) {
                var p = d.onclick ? "" + d.onclick : "";
                if (0 <= p.indexOf(".tl(") || 0 <= p.indexOf(".trackLink(")) d = 0
            }
        } else a.ma = 1;
        !e && d && (e = a.Ca(d));
        e &&
            !a.linkLeaveQueryString && (g = e.indexOf("?"), 0 <= g && (e = e.substring(0, g)));
        if (!f && e) {
            var n = 0,
                r = 0,
                q;
            if (a.trackDownloadLinks && a.linkDownloadFileTypes)
                for (p = e.toLowerCase(), g = p.indexOf("?"), m = p.indexOf("#"), 0 <= g ? 0 <= m && m < g && (g = m) : g = m, 0 <= g && (p = p.substring(0, g)), g = a.linkDownloadFileTypes.toLowerCase().split(","), m = 0; m < g.length; m++) (q = g[m]) && p.substring(p.length - (q.length + 1)) == "." + q && (f = "d");
            if (a.trackExternalLinks && !f && (p = e.toLowerCase(), a.Fa(p) && (a.linkInternalFilters || (a.linkInternalFilters = k.location.hostname),
                    g = 0, a.linkExternalFilters ? (g = a.linkExternalFilters.toLowerCase().split(","), n = 1) : a.linkInternalFilters && (g = a.linkInternalFilters.toLowerCase().split(",")), g))) {
                for (m = 0; m < g.length; m++) q = g[m], 0 <= p.indexOf(q) && (r = 1);
                r ? n && (f = "e") : n || (f = "e")
            }
        }
        a.linkObject = d;
        a.linkURL = e;
        a.linkType = f;
        if (a.trackClickMap || a.trackInlineStats) a.e = "", d && (f = a.pageName, e = 1, d = d.sourceIndex, f || (f = a.pageURL, e = 0), k.s_objectID && (b.id = k.s_objectID, d = b.type = 1), f && b && b.id && c && (a.e = "&pid=" + a.escape(f.substring(0, 255)) + (e ? "&pidt=" + e : "") +
            "&oid=" + a.escape(b.id.substring(0, 100)) + (b.type ? "&oidt=" + b.type : "") + "&ot=" + c + (d ? "&oi=" + d : "")))
    };
    a.sb = function () {
        var c = a.ma,
            b = a.linkType,
            d = a.linkURL,
            f = a.linkName;
        b && (d || f) && (b = b.toLowerCase(), "d" != b && "e" != b && (b = "o"), a.pe = "lnk_" + b, a.pev1 = d ? a.escape(d) : "", a.pev2 = f ? a.escape(f) : "", c = 1);
        a.abort && (c = 0);
        if (a.trackClickMap || a.trackInlineStats || a.ActivityMap) {
            var b = {},
                d = 0,
                e = a.cookieRead("s_sq"),
                g = e ? e.split("&") : 0,
                m, p, k, e = 0;
            if (g)
                for (m = 0; m < g.length; m++) p = g[m].split("="), f = a.unescape(p[0]).split(","), p = a.unescape(p[1]),
                    b[p] = f;
            f = a.account.split(",");
            m = {};
            for (k in a.contextData) k && !Object.prototype[k] && "a.activitymap." == k.substring(0, 14) && (m[k] = a.contextData[k], a.contextData[k] = "");
            a.e = a.r("c", m) + (a.e ? a.e : "");
            if (c || a.e) {
                c && !a.e && (e = 1);
                for (p in b)
                    if (!Object.prototype[p])
                        for (k = 0; k < f.length; k++)
                            for (e && (g = b[p].join(","), g == a.account && (a.e += ("&" != p.charAt(0) ? "&" : "") + p, b[p] = [], d = 1)), m = 0; m < b[p].length; m++) g = b[p][m], g == f[k] && (e && (a.e += "&u=" + a.escape(g) + ("&" != p.charAt(0) ? "&" : "") + p + "&u=0"), b[p].splice(m, 1), d = 1);
                c || (d = 1);
                if (d) {
                    e =
                        "";
                    m = 2;
                    !c && a.e && (e = a.escape(f.join(",")) + "=" + a.escape(a.e), m = 1);
                    for (p in b) !Object.prototype[p] && 0 < m && 0 < b[p].length && (e += (e ? "&" : "") + a.escape(b[p].join(",")) + "=" + a.escape(p), m--);
                    a.cookieWrite("s_sq", e)
                }
            }
        }
        return c
    };
    a.tb = function () {
        if (!a.Eb) {
            var c = new Date,
                b = r.location,
                d, f, e = f = d = "",
                g = "",
                m = "",
                k = "1.2",
                n = a.cookieWrite("s_cc", "true", 0) ? "Y" : "N",
                q = "",
                s = "";
            if (c.setUTCDate && (k = "1.3", (0).toPrecision && (k = "1.5", c = [], c.forEach))) {
                k = "1.6";
                f = 0;
                d = {};
                try {
                    f = new Iterator(d), f.next && (k = "1.7", c.reduce && (k = "1.8", k.trim &&
                        (k = "1.8.1", Date.parse && (k = "1.8.2", Object.create && (k = "1.8.5")))))
                } catch (t) { }
            }
            d = screen.width + "x" + screen.height;
            e = navigator.javaEnabled() ? "Y" : "N";
            f = screen.pixelDepth ? screen.pixelDepth : screen.colorDepth;
            g = a.w.innerWidth ? a.w.innerWidth : a.d.documentElement.offsetWidth;
            m = a.w.innerHeight ? a.w.innerHeight : a.d.documentElement.offsetHeight;
            try {
                a.b.addBehavior("#default#homePage"), q = a.b.Kb(b) ? "Y" : "N"
            } catch (u) { }
            try {
                a.b.addBehavior("#default#clientCaps"), s = a.b.connectionType
            } catch (x) { }
            a.resolution = d;
            a.colorDepth =
                f;
            a.javascriptVersion = k;
            a.javaEnabled = e;
            a.cookiesEnabled = n;
            a.browserWidth = g;
            a.browserHeight = m;
            a.connectionType = s;
            a.homepage = q;
            a.Eb = 1
        }
    };
    a.O = {};
    a.loadModule = function (c, b) {
        var d = a.O[c];
        if (!d) {
            d = k["AppMeasurement_Module_" + c] ? new k["AppMeasurement_Module_" + c](a) : {};
            a.O[c] = a[c] = d;
            d.Wa = function () {
                return d.$a
            };
            d.ab = function (b) {
                if (d.$a = b) a[c + "_onLoad"] = b, a.ga(c + "_onLoad", [a, d], 1) || b(a, d)
            };
            try {
                Object.defineProperty ? Object.defineProperty(d, "onLoad", {
                    get: d.Wa,
                    set: d.ab
                }) : d._olc = 1
            } catch (f) {
                d._olc = 1
            }
        }
        b && (a[c + "_onLoad"] =
            b, a.ga(c + "_onLoad", [a, d], 1) || b(a, d))
    };
    a.p = function (c) {
        var b, d;
        for (b in a.O)
            if (!Object.prototype[b] && (d = a.O[b]) && (d._olc && d.onLoad && (d._olc = 0, d.onLoad(a, d)), d[c] && d[c]())) return 1;
        return 0
    };
    a.vb = function () {
        var c = Math.floor(1E13 * Math.random()),
            b = a.visitorSampling,
            d = a.visitorSamplingGroup,
            d = "s_vsn_" + (a.visitorNamespace ? a.visitorNamespace : a.account) + (d ? "_" + d : ""),
            f = a.cookieRead(d);
        if (b) {
            f && (f = parseInt(f));
            if (!f) {
                if (!a.cookieWrite(d, c)) return 0;
                f = c
            }
            if (f % 1E4 > v) return 0
        }
        return 1
    };
    a.P = function (c, b) {
        var d,
            f, e, g, m, k;
        for (d = 0; 2 > d; d++)
            for (f = 0 < d ? a.va : a.g, e = 0; e < f.length; e++)
                if (g = f[e], (m = c[g]) || c["!" + g]) {
                    if (!b && ("contextData" == g || "retrieveLightData" == g) && a[g])
                        for (k in a[g]) m[k] || (m[k] = a[g][k]);
                    a[g] = m
                }
    };
    a.Pa = function (c, b) {
        var d, f, e, g;
        for (d = 0; 2 > d; d++)
            for (f = 0 < d ? a.va : a.g, e = 0; e < f.length; e++) g = f[e], c[g] = a[g], b || c[g] || (c["!" + g] = 1)
    };
    a.nb = function (a) {
        var b, d, f, e, g, k = 0,
            p, n = "",
            q = "";
        if (a && 255 < a.length && (b = "" + a, d = b.indexOf("?"), 0 < d && (p = b.substring(d + 1), b = b.substring(0, d), e = b.toLowerCase(), f = 0, "http://" == e.substring(0,
                7) ? f += 7 : "https://" == e.substring(0, 8) && (f += 8), d = e.indexOf("/", f), 0 < d && (e = e.substring(f, d), g = b.substring(d), b = b.substring(0, d), 0 <= e.indexOf("google") ? k = ",q,ie,start,search_key,word,kw,cd," : 0 <= e.indexOf("yahoo.co") && (k = ",p,ei,"), k && p)))) {
            if ((a = p.split("&")) && 1 < a.length) {
                for (f = 0; f < a.length; f++) e = a[f], d = e.indexOf("="), 0 < d && 0 <= k.indexOf("," + e.substring(0, d) + ",") ? n += (n ? "&" : "") + e : q += (q ? "&" : "") + e;
                n && q ? p = n + "&" + q : q = ""
            }
            d = 253 - (p.length - q.length) - b.length;
            a = b + (0 < d ? g.substring(0, d) : "") + "?" + p
        }
        return a
    };
    a.Va = function (c) {
        var b =
            a.d.visibilityState,
            d = ["webkitvisibilitychange", "visibilitychange"];
        b || (b = a.d.webkitVisibilityState);
        if (b && "prerender" == b) {
            if (c)
                for (b = 0; b < d.length; b++) a.d.addEventListener(d[b], function () {
                    var b = a.d.visibilityState;
                    b || (b = a.d.webkitVisibilityState);
                    "visible" == b && c()
                });
            return !1
        }
        return !0
    };
    a.ca = !1;
    a.H = !1;
    a.cb = function () {
        a.H = !0;
        a.j()
    };
    a.aa = !1;
    a.T = !1;
    a.Za = function (c) {
        a.marketingCloudVisitorID = c;
        a.T = !0;
        a.j()
    };
    a.da = !1;
    a.U = !1;
    a.eb = function (c) {
        a.visitorOptedOut = c;
        a.U = !0;
        a.j()
    };
    a.X = !1;
    a.Q = !1;
    a.Ra = function (c) {
        a.analyticsVisitorID =
            c;
        a.Q = !0;
        a.j()
    };
    a.Z = !1;
    a.S = !1;
    a.Ta = function (c) {
        a.audienceManagerLocationHint = c;
        a.S = !0;
        a.j()
    };
    a.Y = !1;
    a.R = !1;
    a.Sa = function (c) {
        a.audienceManagerBlob = c;
        a.R = !0;
        a.j()
    };
    a.Ua = function (c) {
        a.maxDelay || (a.maxDelay = 250);
        return a.p("_d") ? (c && setTimeout(function () {
            c()
        }, a.maxDelay), !1) : !0
    };
    a.ba = !1;
    a.G = !1;
    a.ua = function () {
        a.G = !0;
        a.j()
    };
    a.isReadyToTrack = function () {
        var c = !0,
            b = a.visitor,
            d, f, e;
        a.ca || a.H || (a.Va(a.cb) ? a.H = !0 : a.ca = !0);
        if (a.ca && !a.H) return !1;
        b && b.isAllowed() && (a.aa || a.marketingCloudVisitorID || !b.getMarketingCloudVisitorID ||
            (a.aa = !0, a.marketingCloudVisitorID = b.getMarketingCloudVisitorID([a, a.Za]), a.marketingCloudVisitorID && (a.T = !0)), a.da || a.visitorOptedOut || !b.isOptedOut || (a.da = !0, a.visitorOptedOut = b.isOptedOut([a, a.eb]), a.visitorOptedOut != q && (a.U = !0)), a.X || a.analyticsVisitorID || !b.getAnalyticsVisitorID || (a.X = !0, a.analyticsVisitorID = b.getAnalyticsVisitorID([a, a.Ra]), a.analyticsVisitorID && (a.Q = !0)), a.Z || a.audienceManagerLocationHint || !b.getAudienceManagerLocationHint || (a.Z = !0, a.audienceManagerLocationHint = b.getAudienceManagerLocationHint([a,
                a.Ta
            ]), a.audienceManagerLocationHint && (a.S = !0)), a.Y || a.audienceManagerBlob || !b.getAudienceManagerBlob || (a.Y = !0, a.audienceManagerBlob = b.getAudienceManagerBlob([a, a.Sa]), a.audienceManagerBlob && (a.R = !0)), c = a.aa && !a.T && !a.marketingCloudVisitorID, b = a.X && !a.Q && !a.analyticsVisitorID, d = a.Z && !a.S && !a.audienceManagerLocationHint, f = a.Y && !a.R && !a.audienceManagerBlob, e = a.da && !a.U, c = c || b || d || f || e ? !1 : !0);
        a.ba || a.G || (a.Ua(a.ua) ? a.G = !0 : a.ba = !0);
        a.ba && !a.G && (c = !1);
        return c
    };
    a.o = q;
    a.u = 0;
    a.callbackWhenReadyToTrack = function (c,
        b, d) {
        var f;
        f = {};
        f.ib = c;
        f.hb = b;
        f.fb = d;
        a.o == q && (a.o = []);
        a.o.push(f);
        0 == a.u && (a.u = setInterval(a.j, 100))
    };
    a.j = function () {
        var c;
        if (a.isReadyToTrack() && (a.bb(), a.o != q))
            for (; 0 < a.o.length;) c = a.o.shift(), c.hb.apply(c.ib, c.fb)
    };
    a.bb = function () {
        a.u && (clearInterval(a.u), a.u = 0)
    };
    a.Xa = function (c) {
        var b, d, f = q,
            e = q;
        if (!a.isReadyToTrack()) {
            b = [];
            if (c != q)
                for (d in f = {}, c) f[d] = c[d];
            e = {};
            a.Pa(e, !0);
            b.push(f);
            b.push(e);
            a.callbackWhenReadyToTrack(a, a.track, b);
            return !0
        }
        return !1
    };
    a.pb = function () {
        var c = a.cookieRead("s_fid"),
            b = "",
            d = "",
            f;
        f = 8;
        var e = 4;
        if (!c || 0 > c.indexOf("-")) {
            for (c = 0; 16 > c; c++) f = Math.floor(Math.random() * f), b += "0123456789ABCDEF".substring(f, f + 1), f = Math.floor(Math.random() * e), d += "0123456789ABCDEF".substring(f, f + 1), f = e = 16;
            c = b + "-" + d
        }
        a.cookieWrite("s_fid", c, 1) || (c = 0);
        return c
    };
    a.t = a.track = function (c, b) {
        var d, f = new Date,
            e = "s" + Math.floor(f.getTime() / 108E5) % 10 + Math.floor(1E13 * Math.random()),
            g = f.getYear(),
            g = "t=" + a.escape(f.getDate() + "/" + f.getMonth() + "/" + (1900 > g ? g + 1900 : g) + " " + f.getHours() + ":" + f.getMinutes() + ":" + f.getSeconds() +
                " " + f.getDay() + " " + f.getTimezoneOffset());
        a.visitor && (a.visitor.getAuthState && (a.authState = a.visitor.getAuthState()), !a.supplementalDataID && a.visitor.getSupplementalDataID && (a.supplementalDataID = a.visitor.getSupplementalDataID("AppMeasurement:" + a._in, a.expectSupplementalData ? !1 : !0)));
        a.p("_s");
        a.Xa(c) || (b && a.P(b), c && (d = {}, a.Pa(d, 0), a.P(c)), a.vb() && !a.visitorOptedOut && (a.analyticsVisitorID || a.marketingCloudVisitorID || (a.fid = a.pb()), a.Ab(), a.usePlugins && a.doPlugins && a.doPlugins(a), a.account && (a.abort ||
            (a.trackOffline && !a.timestamp && (a.timestamp = Math.floor(f.getTime() / 1E3)), f = k.location, a.pageURL || (a.pageURL = f.href ? f.href : f), a.referrer || a.Qa || (a.referrer = r.document.referrer), a.Qa = 1, a.referrer = a.nb(a.referrer), a.p("_g")), a.sb() && !a.abort && (a.tb(), g += a.rb(), a.zb(e, g), a.p("_t"), a.referrer = ""))), c && a.P(d, 1));
        a.abort = a.supplementalDataID = a.timestamp = a.pageURLRest = a.linkObject = a.clickObject = a.linkURL = a.linkName = a.linkType = k.s_objectID = a.pe = a.pev1 = a.pev2 = a.pev3 = a.e = a.lightProfileID = 0
    };
    a.tl = a.trackLink =
        function (c, b, d, f, e) {
            a.linkObject = c;
            a.linkType = b;
            a.linkName = d;
            e && (a.l = c, a.A = e);
            return a.track(f)
        };
    a.trackLight = function (c, b, d, f) {
        a.lightProfileID = c;
        a.lightStoreForSeconds = b;
        a.lightIncrementBy = d;
        return a.track(f)
    };
    a.clearVars = function () {
        var c, b;
        for (c = 0; c < a.g.length; c++)
            if (b = a.g[c], "prop" == b.substring(0, 4) || "eVar" == b.substring(0, 4) || "hier" == b.substring(0, 4) || "list" == b.substring(0, 4) || "channel" == b || "events" == b || "eventList" == b || "products" == b || "productList" == b || "purchaseID" == b || "transactionID" == b || "state" ==
                b || "zip" == b || "campaign" == b) a[b] = void 0
    };
    a.tagContainerMarker = "";
    a.zb = function (c, b) {
        var d, f = a.trackingServer;
        d = "";
        var e = a.dc,
            g = "sc.",
            k = a.visitorNamespace;
        f ? a.trackingServerSecure && a.ssl && (f = a.trackingServerSecure) : (k || (k = a.account, f = k.indexOf(","), 0 <= f && (k = k.substring(0, f)), k = k.replace(/[^A-Za-z0-9]/g, "")), d || (d = "2o7.net"), e = e ? ("" + e).toLowerCase() : "d1", "2o7.net" == d && ("d1" == e ? e = "112" : "d2" == e && (e = "122"), g = ""), f = k + "." + e + "." + g + d);
        d = a.ssl ? "https://" : "http://";
        e = a.AudienceManagement && a.AudienceManagement.isReady() ||
            0 != a.usePostbacks;
        d += f + "/b/ss/" + a.account + "/" + (a.mobile ? "5." : "") + (e ? "10" : "1") + "/JS-" + a.version + (a.Db ? "T" : "") + (a.tagContainerMarker ? "-" + a.tagContainerMarker : "") + "/" + c + "?AQB=1&ndh=1&pf=1&" + (e ? "callback=s_c_il[" + a._in + "].doPostbacks&et=1&" : "") + b + "&AQE=1";
        a.lb(d);
        a.ia()
    };
    a.Oa = /{(%?)(.*?)(%?)}/;
    a.Hb = RegExp(a.Oa.source, "g");
    a.mb = function (c) {
        if ("object" == typeof c.dests)
            for (var b = 0; b < c.dests.length; ++b)
                if (o = c.dests[b], "string" == typeof o.c && "aa." == o.id.substr(0, 3))
                    for (var d = o.c.match(a.Hb), b = 0; b < d.length; ++b) {
                        match =
                            d[b];
                        var f = match.match(a.Oa),
                            e = "";
                        "%" == f[1] && "timezone_offset" == f[2] ? e = (new Date).getTimezoneOffset() : "%" == f[1] && "timestampz" == f[2] && (e = a.qb());
                        o.c = o.c.replace(match, a.escape(e))
                    }
    };
    a.qb = function () {
        var c = new Date,
            b = new Date(6E4 * Math.abs(c.getTimezoneOffset()));
        return a.k(4, c.getFullYear()) + "-" + a.k(2, c.getMonth() + 1) + "-" + a.k(2, c.getDate()) + "T" + a.k(2, c.getHours()) + ":" + a.k(2, c.getMinutes()) + ":" + a.k(2, c.getSeconds()) + (0 < c.getTimezoneOffset() ? "-" : "+") + a.k(2, b.getUTCHours()) + ":" + a.k(2, b.getUTCMinutes())
    };
    a.k = function (a, b) {
        return (Array(a + 1).join(0) + b).slice(-a)
    };
    a.qa = {};
    a.doPostbacks = function (c) {
        if ("object" == typeof c)
            if (a.mb(c), "object" == typeof a.AudienceManagement && "function" == typeof a.AudienceManagement.isReady && a.AudienceManagement.isReady() && "function" == typeof a.AudienceManagement.passData) a.AudienceManagement.passData(c);
            else if ("object" == typeof c && "object" == typeof c.dests)
                for (var b = 0; b < c.dests.length; ++b) dest = c.dests[b], "object" == typeof dest && "string" == typeof dest.c && "string" == typeof dest.id &&
                    "aa." == dest.id.substr(0, 3) && (a.qa[dest.id] = new Image, a.qa[dest.id].alt = "", a.qa[dest.id].src = dest.c)
    };
    a.lb = function (c) {
        a.i || a.ub();
        a.i.push(c);
        a.ka = a.C();
        a.Ma()
    };
    a.ub = function () {
        a.i = a.wb();
        a.i || (a.i = [])
    };
    a.wb = function () {
        var c, b;
        if (a.pa()) {
            try {
                (b = k.localStorage.getItem(a.na())) && (c = k.JSON.parse(b))
            } catch (d) { }
            return c
        }
    };
    a.pa = function () {
        var c = !0;
        a.trackOffline && a.offlineFilename && k.localStorage && k.JSON || (c = !1);
        return c
    };
    a.Da = function () {
        var c = 0;
        a.i && (c = a.i.length);
        a.q && c++;
        return c
    };
    a.ia = function () {
        if (a.q &&
            (a.B && a.B.complete && a.B.timeout && a.B.ta(), a.q)) return;
        a.Ea = q;
        if (a.oa) a.ka > a.M && a.Ka(a.i), a.ra(500);
        else {
            var c = a.gb();
            if (0 < c) a.ra(c);
            else if (c = a.Aa()) a.q = 1, a.yb(c), a.Cb(c)
        }
    };
    a.ra = function (c) {
        a.Ea || (c || (c = 0), a.Ea = setTimeout(a.ia, c))
    };
    a.gb = function () {
        var c;
        if (!a.trackOffline || 0 >= a.offlineThrottleDelay) return 0;
        c = a.C() - a.Ja;
        return a.offlineThrottleDelay < c ? 0 : a.offlineThrottleDelay - c
    };
    a.Aa = function () {
        if (0 < a.i.length) return a.i.shift()
    };
    a.yb = function (c) {
        if (a.debugTracking) {
            var b = "AppMeasurement Debug: " +
                c;
            c = c.split("&");
            var d;
            for (d = 0; d < c.length; d++) b += "\n\t" + a.unescape(c[d]);
            a.xb(b)
        }
    };
    a.Ya = function () {
        return a.marketingCloudVisitorID || a.analyticsVisitorID
    };
    a.W = !1;
    var s;
    try {
        s = JSON.parse('{"x":"y"}')
    } catch (x) {
        s = null
    }
    s && "y" == s.x ? (a.W = !0, a.V = function (a) {
        return JSON.parse(a)
    }) : k.$ && k.$.parseJSON ? (a.V = function (a) {
        return k.$.parseJSON(a)
    }, a.W = !0) : a.V = function () {
        return null
    };
    a.Cb = function (c) {
        var b, d, f;
        a.Ya() && 2047 < c.length && ("undefined" != typeof XMLHttpRequest && (b = new XMLHttpRequest, "withCredentials" in b ?
            d = 1 : b = 0), b || "undefined" == typeof XDomainRequest || (b = new XDomainRequest, d = 2), b && (a.AudienceManagement && a.AudienceManagement.isReady() || 0 != a.usePostbacks) && (a.W ? b.wa = !0 : b = 0));
        !b && a.Na && (c = c.substring(0, 2047));
        !b && a.d.createElement && (0 != a.usePostbacks || a.AudienceManagement && a.AudienceManagement.isReady()) && (b = a.d.createElement("SCRIPT")) && "async" in b && ((f = (f = a.d.getElementsByTagName("HEAD")) && f[0] ? f[0] : a.d.body) ? (b.type = "text/javascript", b.setAttribute("async", "async"), d = 3) : b = 0);
        b || (b = new Image, b.alt =
            "", b.abort || "undefined" === typeof k.InstallTrigger || (b.abort = function () {
                b.src = q
            }));
        b.ya = function () {
            try {
                b.timeout && (clearTimeout(b.timeout), b.timeout = 0)
            } catch (a) { }
        };
        b.onload = b.ta = function () {
            b.ya();
            a.kb();
            a.ea();
            a.q = 0;
            a.ia();
            if (b.wa) {
                b.wa = !1;
                try {
                    a.doPostbacks(a.V(b.responseText))
                } catch (c) { }
            }
        };
        b.onabort = b.onerror = b.Ba = function () {
            b.ya();
            (a.trackOffline || a.oa) && a.q && a.i.unshift(a.jb);
            a.q = 0;
            a.ka > a.M && a.Ka(a.i);
            a.ea();
            a.ra(500)
        };
        b.onreadystatechange = function () {
            4 == b.readyState && (200 == b.status ? b.ta() : b.Ba())
        };
        a.Ja = a.C();
        if (1 == d || 2 == d) {
            var e = c.indexOf("?");
            f = c.substring(0, e);
            e = c.substring(e + 1);
            e = e.replace(/&callback=[a-zA-Z0-9_.\[\]]+/, "");
            1 == d ? (b.open("POST", f, !0), b.send(e)) : 2 == d && (b.open("POST", f), b.send(e))
        } else if (b.src = c, 3 == d) {
            if (a.Ha) try {
                f.removeChild(a.Ha)
            } catch (g) { }
            f.firstChild ? f.insertBefore(b, f.firstChild) : f.appendChild(b);
            a.Ha = a.B
        }
        b.timeout = setTimeout(function () {
            b.timeout && (b.complete ? b.ta() : (a.trackOffline && b.abort && b.abort(), b.Ba()))
        }, 5E3);
        a.jb = c;
        a.B = k["s_i_" + a.replace(a.account, ",", "_")] =
            b;
        if (a.useForcedLinkTracking && a.I || a.A) a.forcedLinkTrackingTimeout || (a.forcedLinkTrackingTimeout = 250), a.fa = setTimeout(a.ea, a.forcedLinkTrackingTimeout)
    };
    a.kb = function () {
        if (a.pa() && !(a.Ia > a.M)) try {
            k.localStorage.removeItem(a.na()), a.Ia = a.C()
        } catch (c) { }
    };
    a.Ka = function (c) {
        if (a.pa()) {
            a.Ma();
            try {
                k.localStorage.setItem(a.na(), k.JSON.stringify(c)), a.M = a.C()
            } catch (b) { }
        }
    };
    a.Ma = function () {
        if (a.trackOffline) {
            if (!a.offlineLimit || 0 >= a.offlineLimit) a.offlineLimit = 10;
            for (; a.i.length > a.offlineLimit;) a.Aa()
        }
    };
    a.forceOffline =
        function () {
            a.oa = !0
        };
    a.forceOnline = function () {
        a.oa = !1
    };
    a.na = function () {
        return a.offlineFilename + "-" + a.visitorNamespace + a.account
    };
    a.C = function () {
        return (new Date).getTime()
    };
    a.Fa = function (a) {
        a = a.toLowerCase();
        return 0 != a.indexOf("#") && 0 != a.indexOf("about:") && 0 != a.indexOf("opera:") && 0 != a.indexOf("javascript:") ? !0 : !1
    };
    a.setTagContainer = function (c) {
        var b, d, f;
        a.Db = c;
        for (b = 0; b < a._il.length; b++)
            if ((d = a._il[b]) && "s_l" == d._c && d.tagContainerName == c) {
                a.P(d);
                if (d.lmq)
                    for (b = 0; b < d.lmq.length; b++) f = d.lmq[b], a.loadModule(f.n);
                if (d.ml)
                    for (f in d.ml)
                        if (a[f])
                            for (b in c = a[f], f = d.ml[f], f) !Object.prototype[b] && ("function" != typeof f[b] || 0 > ("" + f[b]).indexOf("s_c_il")) && (c[b] = f[b]);
                if (d.mmq)
                    for (b = 0; b < d.mmq.length; b++) f = d.mmq[b], a[f.m] && (c = a[f.m], c[f.f] && "function" == typeof c[f.f] && (f.a ? c[f.f].apply(c, f.a) : c[f.f].apply(c)));
                if (d.tq)
                    for (b = 0; b < d.tq.length; b++) a.track(d.tq[b]);
                d.s = a;
                break
            }
    };
    a.Util = {
        urlEncode: a.escape,
        urlDecode: a.unescape,
        cookieRead: a.cookieRead,
        cookieWrite: a.cookieWrite,
        getQueryParam: function (c, b, d) {
            var f;
            b || (b = a.pageURL ?
                a.pageURL : k.location);
            d || (d = "&");
            return c && b && (b = "" + b, f = b.indexOf("?"), 0 <= f && (b = d + b.substring(f + 1) + d, f = b.indexOf(d + c + "="), 0 <= f && (b = b.substring(f + d.length + c.length + 1), f = b.indexOf(d), 0 <= f && (b = b.substring(0, f)), 0 < b.length))) ? a.unescape(b) : ""
        }
    };
    a.F = "supplementalDataID timestamp dynamicVariablePrefix visitorID marketingCloudVisitorID analyticsVisitorID audienceManagerLocationHint authState fid vmk visitorMigrationKey visitorMigrationServer visitorMigrationServerSecure charSet visitorNamespace cookieDomainPeriods fpCookieDomainPeriods cookieLifetime pageName pageURL referrer contextData currencyCode lightProfileID lightStoreForSeconds lightIncrementBy retrieveLightProfiles deleteLightProfiles retrieveLightData".split(" ");
    a.g = a.F.concat("purchaseID variableProvider channel server pageType transactionID campaign state zip events events2 products audienceManagerBlob tnt".split(" "));
    a.la = "timestamp charSet visitorNamespace cookieDomainPeriods cookieLifetime contextData lightProfileID lightStoreForSeconds lightIncrementBy".split(" ");
    a.N = a.la.slice(0);
    a.va = "account allAccounts debugTracking visitor visitorOptedOut trackOffline offlineLimit offlineThrottleDelay offlineFilename usePlugins doPlugins configURL visitorSampling visitorSamplingGroup linkObject clickObject linkURL linkName linkType trackDownloadLinks trackExternalLinks trackClickMap trackInlineStats linkLeaveQueryString linkTrackVars linkTrackEvents linkDownloadFileTypes linkExternalFilters linkInternalFilters useForcedLinkTracking forcedLinkTrackingTimeout trackingServer trackingServerSecure ssl abort mobile dc lightTrackVars maxDelay expectSupplementalData usePostbacks AudienceManagement".split(" ");
    for (n = 0; 250 >= n; n++) 76 > n && (a.g.push("prop" + n), a.N.push("prop" + n)), a.g.push("eVar" + n), a.N.push("eVar" + n), 6 > n && a.g.push("hier" + n), 4 > n && a.g.push("list" + n);
    n = "pe pev1 pev2 pev3 latitude longitude resolution colorDepth javascriptVersion javaEnabled cookiesEnabled browserWidth browserHeight connectionType homepage pageURLRest".split(" ");
    a.g = a.g.concat(n);
    a.F = a.F.concat(n);
    a.ssl = 0 <= k.location.protocol.toLowerCase().indexOf("https");
    a.charSet = "UTF-8";
    a.contextData = {};
    a.offlineThrottleDelay = 0;
    a.offlineFilename =
        "AppMeasurement.offline";
    a.Ja = 0;
    a.ka = 0;
    a.M = 0;
    a.Ia = 0;
    a.linkDownloadFileTypes = "exe,zip,wav,mp3,mov,mpg,avi,wmv,pdf,doc,docx,xls,xlsx,ppt,pptx";
    a.w = k;
    a.d = k.document;
    try {
        if (a.Na = !1, navigator) {
            var y = navigator.userAgent;
            if ("Microsoft Internet Explorer" == navigator.appName || 0 <= y.indexOf("MSIE ") || 0 <= y.indexOf("Trident/") && 0 <= y.indexOf("Windows NT 6")) a.Na = !0
        }
    } catch (z) { }
    a.ea = function () {
        a.fa && (k.clearTimeout(a.fa), a.fa = q);
        a.l && a.I && a.l.dispatchEvent(a.I);
        a.A && ("function" == typeof a.A ? a.A() : a.l && a.l.href && (a.d.location =
            a.l.href));
        a.l = a.I = a.A = 0
    };
    a.La = function () {
        a.b = a.d.body;
        a.b ? (a.v = function (c) {
            var b, d, f, e, g;
            if (!(a.d && a.d.getElementById("cppXYctnr") || c && c["s_fe_" + a._in])) {
                if (a.xa)
                    if (a.useForcedLinkTracking) a.b.removeEventListener("click", a.v, !1);
                    else {
                        a.b.removeEventListener("click", a.v, !0);
                        a.xa = a.useForcedLinkTracking = 0;
                        return
                    }
                else a.useForcedLinkTracking = 0;
                a.clickObject = c.srcElement ? c.srcElement : c.target;
                try {
                    if (!a.clickObject || a.L && a.L == a.clickObject || !(a.clickObject.tagName || a.clickObject.parentElement || a.clickObject.parentNode)) a.clickObject =
                        0;
                    else {
                        var m = a.L = a.clickObject;
                        a.ja && (clearTimeout(a.ja), a.ja = 0);
                        a.ja = setTimeout(function () {
                            a.L == m && (a.L = 0)
                        }, 1E4);
                        f = a.Da();
                        a.track();
                        if (f < a.Da() && a.useForcedLinkTracking && c.target) {
                            for (e = c.target; e && e != a.b && "A" != e.tagName.toUpperCase() && "AREA" != e.tagName.toUpperCase() ;) e = e.parentNode;
                            if (e && (g = e.href, a.Fa(g) || (g = 0), d = e.target, c.target.dispatchEvent && g && (!d || "_self" == d || "_top" == d || "_parent" == d || k.name && d == k.name))) {
                                try {
                                    b = a.d.createEvent("MouseEvents")
                                } catch (n) {
                                    b = new k.MouseEvent
                                }
                                if (b) {
                                    try {
                                        b.initMouseEvent("click",
                                            c.bubbles, c.cancelable, c.view, c.detail, c.screenX, c.screenY, c.clientX, c.clientY, c.ctrlKey, c.altKey, c.shiftKey, c.metaKey, c.button, c.relatedTarget)
                                    } catch (q) {
                                        b = 0
                                    }
                                    b && (b["s_fe_" + a._in] = b.s_fe = 1, c.stopPropagation(), c.stopImmediatePropagation && c.stopImmediatePropagation(), c.preventDefault(), a.l = c.target, a.I = b)
                                }
                            }
                        }
                    }
                } catch (r) {
                    a.clickObject = 0
                }
            }
        }, a.b && a.b.attachEvent ? a.b.attachEvent("onclick", a.v) : a.b && a.b.addEventListener && (navigator && (0 <= navigator.userAgent.indexOf("WebKit") && a.d.createEvent || 0 <= navigator.userAgent.indexOf("Firefox/2") &&
            k.MouseEvent) && (a.xa = 1, a.useForcedLinkTracking = 1, a.b.addEventListener("click", a.v, !0)), a.b.addEventListener("click", a.v, !1))) : setTimeout(a.La, 30)
    };
    a.La();
    a.loadModule("ActivityMap")
}

function s_gi(a) {
    var k, q = window.s_c_il,
        r, n, t = a.split(","),
        u, s, x = 0;
    if (q)
        for (r = 0; !x && r < q.length;) {
            k = q[r];
            if ("s_c" == k._c && (k.account || k.oun))
                if (k.account && k.account == a) x = 1;
                else
                    for (n = k.account ? k.account : k.oun, n = k.allAccounts ? k.allAccounts : n.split(","), u = 0; u < t.length; u++)
                        for (s = 0; s < n.length; s++) t[u] == n[s] && (x = 1);
            r++
        }
    x || (k = new AppMeasurement);
    k.setAccount ? k.setAccount(a) : k.sa && k.sa(a);
    return k
}
AppMeasurement.getInstance = s_gi;
window.s_objectID || (window.s_objectID = 0);

function s_pgicq() {
    var a = window,
        k = a.s_giq,
        q, r, n;
    if (k)
        for (q = 0; q < k.length; q++) r = k[q], n = s_gi(r.oun), n.setAccount(r.un), n.setTagContainer(r.tagContainerName);
    a.s_giq = 0
}
s_pgicq();


var omniture = new OmnitureModule();
omniture.Init();