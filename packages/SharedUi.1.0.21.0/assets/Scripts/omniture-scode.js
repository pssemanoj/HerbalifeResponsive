/* SiteCatalyst Version H27.5 */
/* Last Modified - 10.21.2015 (m.d.Y) */
/* Last Modified By - Juan Herrera */

function OmnitureModule() {
    var _OmnitureModuleInstance = this;

    _OmnitureModuleInstance.Init = function () {
        // Check if jQuery Library Exists
        if (typeof jQuery != 'undefined' && typeof _AnalyticsFacts_ != 'undefined' && _AnalyticsFacts_) {
            refactorAnalyticsFacts(_AnalyticsFacts_);
        }
    }; /* Grabs the AnalyticsFacts object and reassigns the values for the omniture variables */

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
                bizworksPageTitle: s_omntr.getQueryParam('ReportType').toLowerCase(),
                titleQueryStringExists: s_omntr.getQueryParam('Title').toLowerCase(),
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
                    }
                    else if (arrPath[i].substring(0, arrPath[i].indexOf('.')) == "") {
                        customValues.pageTitle = arrPath[i];
                    }
                    else {
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

        }
    }

    /* Assigns and configures the plugins */

    function pluginsConf(analyticsFacts) {
        /************************** CONFIG SECTION **************************/
        try {
            if (analyticsFacts.reportSuiteId != "") {
                s_omntr = s_gi(analyticsFacts.reportSuiteId);

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
                            this.target == '_blank' ? window.open(this.href) : true; obj.oldfunction ? obj.oldfunction() : true;
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
                            this.target == '_blank' ? window.open(this.href) : true; obj.oldfunction ? obj.oldfunction() : true;
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
                    var specialCharArr = ["Ã", "À", "Á", "Ä", "Â", "È", "É", "Ë", "Ê", "Ì", "Í", "Ï", "Î", "Ò", "Ó", "Ö", "Ô", "Ù", "Ú", "Ü", "Û", "ã", "à", "á", "ä", "â", "è", "é", "ë", "ê", "ì", "í", "ï", "î", "ò", "ó", "ö", "ô", "ù", "ú", "ü", "û", "Ñ", "ñ", "Ç", "ç", "&Aacute", "&aacute", "&Eacute", "&eacute", "&Iacute", "&iacute", "&Oacute", "&oacute", "&Uacute", "&uacute", "&Ntilde", "&ntilde"];
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
                /* Function - read combined cookies */
                s_omntr.c_r = new Function("k", "" + "var s=this,d=new Date,v=s.c_rr(k),c=s.c_rr('s_pers'),i,m,e;if(v)ret" + "urn v;k=s.ape(k);i=c.indexOf(' '+k+'=');c=i<0?s.c_rr('s_sess'):c;i=" + "c.indexOf(' '+k+'=');m=i<0?i:c.indexOf('|',i);e=i<0?i:c.indexOf(';'" + ",i);m=m>0?m:e;v=i<0?'':s.epa(c.substring(i+2+k.length,m<0?c.length:" + "m));if(m>0&&m!=e)if(parseInt(c.substring(m+1,e<0?c.length:e))<d.get" + "Time()){d.setTime(d.getTime()-60000);s.c_w(s.epa(k),'',d);v='';}ret" + "urn v;");
                s_omntr.c_wr = s_omntr.c_w;
                /* Function - write combined cookies */
                s_omntr.c_w = new Function("k", "v", "e", "" + "var s=this,d=new Date,ht=0,pn='s_pers',sn='s_sess',pc=0,sc=0,pv,sv," + "c,i,t;d.setTime(d.getTime()-60000);if(s.c_rr(k)) s.c_wr(k,'',d);k=s" + ".ape(k);pv=s.c_rr(pn);i=pv.indexOf(' '+k+'=');if(i>-1){pv=pv.substr" + "ing(0,i)+pv.substring(pv.indexOf(';',i)+1);pc=1;}sv=s.c_rr(sn);i=sv" + ".indexOf(' '+k+'=');if(i>-1){sv=sv.substring(0,i)+sv.substring(sv.i" + "ndexOf(';',i)+1);sc=1;}d=new Date;if(e){if(e.getTime()>d.getTime())" + "{pv+=' '+k+'='+s.ape(v)+'|'+e.getTime()+';';pc=1;}}else{sv+=' '+k+'" + "='+s.ape(v)+';';sc=1;}if(sc) s.c_wr(sn,sv,0);if(pc){t=pv;while(t&&t" + ".indexOf(';')!=-1){var t1=parseInt(t.substring(t.indexOf('|')+1,t.i" + "ndexOf(';')));t=t.substring(t.indexOf(';')+1);ht=ht<t1?t1:ht;}d.set" + "Time(ht);s.c_wr(pn,pv,d);}return v==s.c_r(s.epa(k));");
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
                /* Plugin: getValOnce 0.2 - get a value once per session or number of days */
                s_omntr.getValOnce = new Function("v", "c", "e", "" + "var s=this,k=s.c_r(c),a=new Date;e=e?e:0;if(v){a.setTime(a.getTime(" + ")+e*86400000);s.c_w(c,v,e?a:0);}return v==k?'':v");
                /* Plugin: getTimeParting 1.4 - Set timeparting values based on time zone */
                s_omntr.getTimeParting = new Function("t", "z", "y", "" + "dc=new Date('1/1/2000');var f=15;var ne=8;if(dc.getDay()!=6||" + "dc.getMonth()!=0){return'Data Not Available'}else{;z=parseInt(z);" + "if(y=='2009'){f=8;ne=1};gmar=new Date('3/1/'+y);dsts=f-gmar.getDay(" + ");gnov=new Date('11/1/'+y);dste=ne-gnov.getDay();spr=new Date('3/'" + "+dsts+'/'+y);fl=new Date('11/'+dste+'/'+y);cd=new Date();" + "if(cd>spr&&cd<fl){z=z+1}else{z=z};utc=cd.getTime()+(cd.getTimezoneO" + "ffset()*60000);tz=new Date(utc + (3600000*z));thisy=tz.getFullYear(" + ");var days=['Sunday','Monday','Tuesday','Wednesday','Thursday','Fr" + "iday','Saturday'];if(thisy!=y){return'Data Not Available'}else{;thi" + "sh=tz.getHours();thismin=tz.getMinutes();thisd=tz.getDay();var dow=" + "days[thisd];var ap='AM';var dt='Weekday';var mint='00';if(thismin>1" + "5&&thismin<30){mint='15'}if(thismin>30&&thismin<45){mint='30'}if(th" + "ismin>45&&thismin<60){mint='45'}" + "if(thish>=12){ap='PM';thish=thish-12};if (thish==0){th" + "ish=12};if(thisd==6||thisd==0){dt='Weekend'};var timestring=thish+'" + ":'+mint+ap;var daystring=dow;var endstring=dt;if(t=='h'){return tim" + "estring}if(t=='d'){return daystring};if(t=='w'){return en" + "dstring}}};");
                /* Plugin: getNewRepeat 1.2 - Returns whether user is new or repeat */
                s_omntr.getNewRepeat = new Function("d", "cn", "" + "var s=this,e=new Date(),cval,sval,ct=e.getTime();d=d?d:30;cn=cn?cn:" + "'s_nr';e.setTime(ct+d*24*60*60*1000);cval=s.c_r(cn);if(cval.length=" + "=0){s.c_w(cn,ct+'-New',e);return'New';}sval=s.split(cval,'-');if(ct" + "-sval[0]<30*60*1000&&sval[1]=='New'){s.c_w(cn,ct+'-New',e);return'N" + "ew';}else{s.c_w(cn,ct+'-Repeat',e);return'Repeat';}");
                /* Plugin: getPreviousValue_v1.0 - return previous value of designated variable (requires split utility)*/
                s_omntr.getPreviousValue = new Function("v", "c", "el", "" + "var s=this,t=new Date,i,j,r='';t.setTime(t.getTime()+1800000);if(el" + "){if(s.events){i=s.split(el,',');j=s.split(s.events,',');for(x in i" + "){for(y in j){if(i[x]==j[y]){if(s.c_r(c)) r=s.c_r(c);v?s.c_w(c,v,t)" + ":s.c_w(c,'no value',t);return r}}}}}else{if(s.c_r(c)) r=s.c_r(c);v?" + "s.c_w(c,v,t):s.c_w(c,'no value',t);return r}");
                /* Plugin: getPercentPageViewed v1.1 */
                s_omntr.getPercentPageViewed = new Function("", "" + "var s=this; if(typeof(s.linkType)=='undefined'||s.linkType=='e'){var v=s.c_r('s" + "_ppv');s.c_w('s_ppv',0);return v;}");
                s_omntr.getPPVCalc = new Function("", "" + "var s=s_omntr; var dh=Math.max(Math.max(s.d.body.scrollHeight,s.d.documentElement." + "scrollHeight),Math.max(s.d.body.offsetHeight,s.d.documentElement.of" + "fsetHeight),Math.max(s.d.body.clientHeight,s.d.documentElement.clie" + "ntHeight)),vph=s.d.clientHeight||Math.min(s.d.documentElement.clien" + "tHeight,s.d.body.clientHeight),st=s.wd.pageYOffset||(s.wd.document." + "documentElement.scrollTop||s.wd.document.body.scrollTop),vh=st+vph," + "pv=Math.round(vh/dh*100),cp=s.c_r('s_ppv');if(pv>100){s.c_w('s_ppv'" + ",'');}else if(pv>cp){s.c_w('s_ppv',pv);}");
                s_omntr.getPPVSetup = new Function("", "" + "var s=this; if(s.wd.addEventListener){s.wd.addEventListener('load',s.getPPVCalc" + ",false);s.wd.addEventListener('scroll',s.getPPVCalc,false);s.wd.add" + "EventListener('resize',s.getPPVCalc,false);}else if(s.wd.attachEven" + "t){s.wd.attachEvent('onload',s.getPPVCalc);s.wd.attachEvent('onscro" + "ll',s.getPPVCalc);s.wd.attachEvent('onresize',s.getPPVCalc);}");
                s_omntr.getPPVSetup();
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
                s_omntr.setupDynamicObjectIDs = new Function("" + "var s=this;if(!s.doi){s.doi=1;if(s.apv>3&&(!s.isie||!s.ismac||s.apv" + ">=5)){if(s.wd.attachEvent)s.wd.attachEvent('onload',s.setOIDs);else" + " if(s.wd.addEventListener)s.wd.addEventListener('load',s.setOIDs,fa" + "lse);else{s.doiol=s.wd.onload;s.wd.onload=s.setOIDs}}s.wd.s_semapho" + "re=1}");
                s_omntr.setOIDs = new Function("e", "" + "var s=s_c_il[" + s_omntr._in + "],b=s.eh(s.wd,'onload'),o='onclick',x,l,u,c,i" + ",a=new Array;if(s.doiol){if(b)s[b]=s.wd[b];s.doiol(e)}if(s.d.links)" + "{for(i=0;i<s.d.links.length;i++){l=s.d.links[i];c=l[o]?''+l[o]:'';b" + "=s.eh(l,o);z=l[b]?''+l[b]:'';u=s.getObjectID(l);if(u&&c.indexOf('s_" + "objectID')<0&&z.indexOf('s_objectID')<0){u=s.repl(u,'\"','');u=s.re" + "pl(u,'\\n','').substring(0,97);l.s_oc=l[o];a[u]=a[u]?a[u]+1:1;x='';" + "if(l.href.indexOf('javascript:void')<0&&l.className.indexOf('tracklink')<0){" + "if(c.indexOf('.t(')>=0||c.indexOf('.tl(')>=0||c.indexOf('s_gs(')>=0" + ")x='var x=\".tl(\";';x+='s_objectID=\"'+u+'_'+a[u]+'\";return this." + "s_oc?this.s_oc(e):true';" + "if(s.isns&&s.apv>=5)l.setAttribute(o,x);l[o]=new Function('e',x);" + "}}}}s.wd.s_semaphore=0;return true");
                /* Plugin: Days since last Visit 1.0.H */
                s_omntr.getDaysSinceLastVisit = new Function("" + "var s=this,e=new Date(),cval,ct=e.getTime(),c='s_lastvisit',day=24*" + "60*60*1000;e.setTime(ct+3*365*day);cval=s.c_r(c);if(!cval){s.c_w(c," + "ct,e);return 'First page view or cookies not supported';}else{var d" + "=ct-cval;if(d>30*60*1000){if(d>30*day){s.c_w(c,ct,e);return 'More t" + "han 30 days';}if(d<30*day+1 && d>7*day){s.c_w(c,ct,e);return 'More " + "than 7 days';}if(d<7*day+1 && d>day){s.c_w(c,ct,e);return 'Less tha" + "n 7 days';}if(d<day+1){s.c_w(c,ct,e);return 'Less than 1 day';}}els" + "e return '';}");
                /* Plugin: Visit Number By Month 2.0 */
                s_omntr.getVisitNum = new Function("" + "var s=this,e=new Date(),cval,cvisit,ct=e.getTime(),c='s_vnum',c2='s" + "_invisit';e.setTime(ct+30*24*60*60*1000);cval=s.c_r(c);if(cval){var" + " i=cval.indexOf('&vn='),str=cval.substring(i+4,cval.length),k;}cvis" + "it=s.c_r(c2);if(cvisit){if(str){e.setTime(ct+30*60*1000);s.c_w(c2,'" + "true',e);return str;}else return 'unknown visit number';}else{if(st" + "r){str++;k=cval.substring(0,i);e.setTime(k);s.c_w(c,k+'&vn='+str,e)" + ";e.setTime(ct+30*60*1000);s.c_w(c2,'true',e);return str;}else{s.c_w" + "(c,ct+30*24*60*60*1000+'&vn=1',e);e.setTime(ct+30*60*1000);s.c_w(c2" + ",'true',e);return 1;}}");
                /* Plugin: getVisitStart v2.0 */
                s_omntr.getVisitStart = new Function("c", "" + "var s=this,v=1,t=new Date;t.setTime(t.getTime()+1800000);if(s.c_r(c" + ")){v=0}if(!s.c_w(c,1,t)){s.c_w(c,1,0)}if(!s.c_r(c)){v=0}return v;");
                /* Plugin: detectRIA v0.1 - detect and set Flash, Silverlight versions */
                s_omntr.detectRIA = new Function("cn", "fp", "sp", "mfv", "msv", "sf", "" + "cn=cn?cn:'s_ria';msv=msv?msv:2;mfv=mfv?mfv:10;var s=this,sv='',fv=-" + "1,dwi=0,fr='',sr='',w,mt=s.n.mimeTypes,uk=s.c_r(cn),k=s.c_w('s_cc'," + "'true',0)?'Y':'N';fk=uk.substring(0,uk.indexOf('|'));sk=uk.substrin" + "g(uk.indexOf('|')+1,uk.length);if(k=='Y'&&s.p_fo('detectRIA')){if(u" + "k&&!sf){if(fp){s[fp]=fk;}if(sp){s[sp]=sk;}return false;}if(!fk&&fp)" + "{if(s.pl&&s.pl.length){if(s.pl['Shockwave Flash 2.0'])fv=2;x=s.pl['" + "Shockwave Flash'];if(x){fv=0;z=x.description;if(z)fv=z.substring(16" + ",z.indexOf('.'));}}else if(navigator.plugins&&navigator.plugins.len" + "gth){x=navigator.plugins['Shockwave Flash'];if(x){fv=0;z=x.descript" + "ion;if(z)fv=z.substring(16,z.indexOf('.'));}}else if(mt&&mt.length)" + "{x=mt['application/x-shockwave-flash'];if(x&&x.enabledPlugin)fv=0;}" + "if(fv<=0)dwi=1;w=s.u.indexOf('Win')!=-1?1:0;if(dwi&&s.isie&&w&&exec" + "Script){result=false;for(var i=mfv;i>=3&&result!=true;i--){execScri" + "pt('on error resume next: result = IsObject(CreateObject(\"Shockwav" + "eFlash.ShockwaveFlash.'+i+'\"))','VBScript');fv=i;}}fr=fv==-1?'flas" + "h not detected':fv==0?'flash enabled (no version)':'flash '+fv;}if(" + "!sk&&sp&&s.apv>=4.1){var tc='try{x=new ActiveXObject(\"AgControl.A'" + "+'gControl\");for(var i=msv;i>0;i--){for(var j=9;j>=0;j--){if(x.is'" + "+'VersionSupported(i+\".\"+j)){sv=i+\".\"+j;break;}}if(sv){break;}'" + "+'}}catch(e){try{x=navigator.plugins[\"Silverlight Plug-In\"];sv=x'" + "+'.description.substring(0,x.description.indexOf(\".\")+2);}catch('" + "+'e){}}';eval(tc);sr=sv==''?'silverlight not detected':'silverlight" + " '+sv;}if((fr&&fp)||(sr&&sp)){s.c_w(cn,fr+'|'+sr,0);if(fr)s[fp]=fr;" + "if(sr)s[sp]=sr;}}");
                /* Plugin: getTimeToComplete 0.4 */
                s_omntr.getTimeToComplete = new Function("v", "cn", "e", "" + "var s=this,d=new Date,x=d,k;if(!s.ttcr){e=e?e:0;if(v=='start'||v=='" + "stop')s.ttcr=1;x.setTime(x.getTime()+e*86400000);if(v=='start'){s.c" + "_w(cn,d.getTime(),e?x:0);return '';}if(v=='stop'){k=s.c_r(cn);if(!s" + ".c_w(cn,'',d)||!k)return '';v=(d.getTime()-k)/1000;var td=86400,th=" + "3600,tm=60,r=5,u,un;if(v>td){u=td;un='days';}else if(v>th){u=th;un=" + "'hours';}else if(v>tm){r=2;u=tm;un='minutes';}else{r=.2;u=1;un='sec" + "onds';}v=v*r/u;return (Math.round(v)/r)+' '+un;}}return '';");
                /* channelManager v2.4 - Tracking External Traffic */
                s_omntr.channelManager = new Function("a", "b", "c", "d", "e", "f", "" + "var s=this,A,B,g,l,m,M,p,q,P,h,k,u,S,i,O,T,j,r,t,D,E,F,G,H,N,U,v=0," + "X,Y,W,n=new Date;n.setTime(n.getTime()+1800000);if(e){v=1;if(s.c_r(" + "e)){v=0}if(!s.c_w(e,1,n)){s.c_w(e,1,0)}if(!s.c_r(e)){v=0}}g=s.refer" + "rer?s.referrer:document.referrer;g=g.toLowerCase();if(!g){h=1}i=g.i" + "ndexOf('?')>-1?g.indexOf('?'):g.length;j=g.substring(0,i);k=s.linkI" + "nternalFilters.toLowerCase();k=s.split(k,',');l=k.length;for(m=0;m<" + "l;m++){B=j.indexOf(k[m])==-1?'':g;if(B)O=B}if(!O&&!h){p=g;U=g.index" + "Of('//');q=U>-1?U+2:0;Y=g.indexOf('/',q);r=Y>-1?Y:i;t=g.substring(q" + ",r);t=t.toLowerCase();u=t;P='Referrers';S=s.seList+'>'+s._extraSear" + "chEngines;if(d==1){j=s.repl(j,'oogle','%');j=s.repl(j,'ahoo','^');g" + "=s.repl(g,'as_q','*')}A=s.split(S,'>');T=A.length;for(i=0;i<T;i++){" + "D=A[i];D=s.split(D,'|');E=s.split(D[0],',');F=E.length;for(G=0;G<F;" + "G++){H=j.indexOf(E[G]);if(H>-1){i=s.split(D[1],',');U=i.length;for(" + "k=0;k<U;k++){l=s.getQueryParam(i[k],'',g);if(l){l=l.toLowerCase();M" + "=l;if(D[2]){u=D[2];N=D[2]}else{N=t}if(d==1){N=s.repl(N,'#',' - ');g" + "=s.repl(g,'*','as_q');N=s.repl(N,'^','ahoo');N=s.repl(N,'%','oogle'" + ");}}}}}}}if(!O||f!='1'){O=s.getQueryParam(a,b);if(O){u=O;if(M){P='P" + "aid Search'}else{P='Paid Non-Search';}}if(!O&&M){u=N;P='Natural Sea" + "rch'}}if(h==1&&!O&&v==1){u=P=t=p='Direct Load'}X=M+u+t;c=c?c:'c_m';" + "if(c!='0'){X=s.getValOnce(X,c,0);}g=s._channelDomain;if(g&&X){k=s.s" + "plit(g,'>');l=k.length;for(m=0;m<l;m++){q=s.split(k[m],'|');r=s.spl" + "it(q[1],',');S=r.length;for(T=0;T<S;T++){Y=r[T];Y=Y.toLowerCase();i" + "=j.indexOf(Y);if(i>-1)P=q[0]}}}g=s._channelParameter;if(g&&X){k=s.s" + "plit(g,'>');l=k.length;for(m=0;m<l;m++){q=s.split(k[m],'|');r=s.spl" + "it(q[1],',');S=r.length;for(T=0;T<S;T++){U=s.getQueryParam(r[T]);if" + "(U)P=q[0]}}}g=s._channelPattern;if(g&&X){k=s.split(g,'>');l=k.lengt" + "h;for(m=0;m<l;m++){q=s.split(k[m],'|');r=s.split(q[1],',');S=r.leng" + "th;for(T=0;T<S;T++){Y=r[T];Y=Y.toLowerCase();i=O.toLowerCase();H=i." + "indexOf(Y);if(H==0)P=q[0]}}}if(X)M=M?M:'n/a';p=X&&p?p:'';t=X&&t?t:'" + "';N=X&&N?N:'';O=X&&O?O:'';u=X&&u?u:'';M=X&&M?M:'';P=X&&P?P:'';s._re" + "ferrer=p;s._referringDomain=t;s._partner=N;s._campaignID=O;s._campa" + "ign=u;s._keywords=M;s._channel=P");
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
                s_omntr.dc = "122"; /****************************** MODULES *****************************/
                /* Module: Media */
                /* Module: Media */
                s_omntr.m_Media_c = "var m=s_omntr.m_i('Media');if(m.completeByCloseOffset==undefined)m.completeByCloseOffset=1;if(m.completeCloseOffsetThreshold==undefined)m.completeCloseOffsetThreshold=1;m.cn=function(n){var m=" + "this;return m.s.rep(m.s.rep(m.s.rep(n,\"\\n\",''),\"\\r\",''),'--**--','')};m.open=function(n,l,p,b){var m=this,i=new Object,tm=new Date,a='',x;n=m.cn(n);if(!l)l=-1;if(n&&p){if(!m.l)m.l=new Object;" + "if(m.l[n])m.close(n);if(b&&b.id)a=b.id;if(a)for (x in m.l)if(m.l[x]&&m.l[x].a==a)m.close(m.l[x].n);i.n=n;i.l=l;i.o=0;i.x=0;i.p=m.cn(m.playerName?m.playerName:p);i.a=a;i.t=0;i.ts=0;i.s=Math.floor(tm" + ".getTime()/1000);i.lx=0;i.lt=i.s;i.lo=0;i.e='';i.to=-1;i.tc=0;i.fel=new Object;i.vt=0;i.sn=0;i.sx=\"\";i.sl=0;i.sg=0;i.sc=0;i.us=0;i.ad=0;i.adpn;i.adpp;i.adppp;i.clk;i.CPM;i.co=0;i.cot=0;i.lm=0;i.l" + "om=0;m.l[n]=i}};m.openAd=function(n,l,p,pn,pp,ppp,CPM,b){var m=this,i=new Object;n=m.cn(n);m.open(n,l,p,b);i=m.l[n];if(i){i.ad=1;i.adpn=m.cn(pn);i.adpp=pp;i.adppp=ppp;i.CPM=CPM}};m._delete=function" + "(n){var m=this,i;n=m.cn(n);i=m.l[n];m.l[n]=0;if(i&&i.m)clearTimeout(i.m.i)};m.close=function(n){this.e(n,0,-1)};m.play=function(n,o,sn,sx,sl){var m=this,i;i=m.e(n,1,o,sn,sx,sl);if(i&&!i.m){i.m=new " + "Object;i.m.m=new Function('var m=s_c_il['+m._in+'],i;if(m.l){i=m.l[\"'+m.s.rep(i.n,'\"','\\\\\"')+'\"];if(i){if(i.lx==1)m.e(i.n,3,-1);i.m.i=setTimeout(i.m.m,1000)}}');i.m.m()}};m.click=function(n,o" + "){this.e(n,7,o)};m.complete=function(n,o){this.e(n,5,o)};m.stop=function(n,o){this.e(n,2,o)};m.track=function(n){this.e(n,4,-1)};m.bcd=function(vo,i){var m=this,ns='a.media.',v=vo.linkTrackVars,e=v" + "o.linkTrackEvents,pe='m_i',pev3,c=vo.contextData,x;if(i.ad){ns+='ad.';if(i.adpn){c['a.media.name']=i.adpn;c[ns+'pod']=i.adpp;c[ns+'podPosition']=i.adppp;}if(!i.vt)c[ns+'CPM']=i.CPM;}if (i.clk) {c[n" + "s+'clicked']=true;i.clk=0}c['a.contentType']='video'+(i.ad?'Ad':'');c['a.media.channel']=m.channel;c[ns+'name']=i.n;c[ns+'playerName']=i.p;if(i.l>0)c[ns+'length']=i.l;if(Math.floor(i.ts)>0)c[ns+'ti" + "mePlayed']=Math.floor(i.ts);if(!i.vt){c[ns+'view']=true;pe='m_s';i.vt=1}if(i.sx){c[ns+'segmentNum']=i.sn;c[ns+'segment']=i.sx;if(i.sl>0)c[ns+'segmentLength']=i.sl;if(i.sc&&i.ts>0)c[ns+'segmentView'" + "]=true}if(!i.cot&&i.co){c[ns+\"complete\"]=true;i.cot=1}if(i.lm>0)c[ns+'milestone']=i.lm;if(i.lom>0)c[ns+'offsetMilestone']=i.lom;if(v)for(x in c)v+=',contextData.'+x;pev3=c['a.contentType'];vo.pe=" + "pe;vo.pev3=pev3;var d=m.contextDataMapping,y,a,l,n;if(d){vo.events2='';if(v)v+=',events';for(x in d){if(x.substring(0,ns.length)==ns)y=x.substring(ns.length);else y=\"\";a=d[x];if(typeof(a)=='strin" + "g'){l=m.s.sp(a,',');for(n=0;n<l.length;n++){a=l[n];if(x==\"a.contentType\"){if(v)v+=','+a;vo[a]=c[x]}else if(y=='view'||y=='segmentView'||y=='clicked'||y=='complete'||y=='timePlayed'||y=='CPM'){if(" + "e)e+=','+a;if(y=='timePlayed'||y=='CPM'){if(c[x])vo.events2+=(vo.events2?',':'')+a+'='+c[x];}else if(c[x])vo.events2+=(vo.events2?',':'')+a}else if(y=='segment'&&c[x+'Num']){if(v)v+=','+a;vo[a]=c[x" + "+'Num']+':'+c[x]}else{if(v)v+=','+a;vo[a]=c[x]}}}else if(y=='milestones'||y=='offsetMilestones'){x=x.substring(0,x.length-1);if(c[x]&&d[x+'s'][c[x]]){if(e)e+=','+d[x+'s'][c[x]];vo.events2+=(vo.even" + "ts2?',':'')+d[x+'s'][c[x]]}}if(c[x])c[x]=undefined;if(y=='segment'&&c[x+'Num'])c[x+\"Num\"]=undefined}}vo.linkTrackVars=v;vo.linkTrackEvents=e};m.bpe=function(vo,i,x,o){var m=this,pe='m_o',pev3,d='" + "--**--';pe='m_o';if(!i.vt){pe='m_s';i.vt=1}else if(x==4)pe='m_i';pev3=m.s.ape(i.n)+d+Math.floor(i.l>0?i.l:1)+d+m.s.ape(i.p)+d+Math.floor(i.t)+d+i.s+d+(i.to>=0?'L'+Math.floor(i.to):'')+i.e+(x!=0&&x!" + "=2?'L'+Math.floor(o):'');vo.pe=pe;vo.pev3=pev3};m.e=function(n,x,o,sn,sx,sl,pd){var m=this,i,tm=new Date,ts=Math.floor(tm.getTime()/1000),c,l,v=m.trackVars,e=m.trackEvents,ti=m.trackSeconds,tp=m.tr" + "ackMilestones,to=m.trackOffsetMilestones,sm=m.segmentByMilestones,so=m.segmentByOffsetMilestones,z=new Array,j,t=1,w=new Object,x,ek,tc,vo=new Object;if(!m.channel)m.channel=m.s.wd.location.hostnam" + "e;n=m.cn(n);i=n&&m.l&&m.l[n]?m.l[n]:0;if(i){if(i.ad){ti=m.adTrackSeconds;tp=m.adTrackMilestones;to=m.adTrackOffsetMilestones;sm=m.adSegmentByMilestones;so=m.adSegmentByOffsetMilestones}if(o<0){if(i" + ".lx==1&&i.lt>0)o=(ts-i.lt)+i.lo;else o=i.lo}if(i.l>0)o=o<i.l?o:i.l;if(o<0)o=0;i.o=o;if(i.l>0){i.x=(i.o/i.l)*100;i.x=i.x>100?100:i.x}if(i.lo<0)i.lo=o;tc=i.tc;w.name=n;w.ad=i.ad;w.length=i.l;w.openTi" + "me=new Date;w.openTime.setTime(i.s*1000);w.offset=i.o;w.percent=i.x;w.playerName=i.p;if(i.to<0)w.mediaEvent=w.event='OPEN';else w.mediaEvent=w.event=(x==1?'PLAY':(x==2?'STOP':(x==3?'MONITOR':(x==4?" + "'TRACK':(x==5?'COMPLETE':(x==7?'CLICK':('CLOSE')))))));if(!pd){if(i.pd)pd=i.pd}else i.pd=pd;w.player=pd;if(x>2||(x!=i.lx&&(x!=2||i.lx==1))) {if(!sx){sn=i.sn;sx=i.sx;sl=i.sl}if(x){if(x==1)i.lo=o;if(" + "(x<=3||x>=5)&&i.to>=0){t=0;v=e=\"None\";if(i.to!=o){l=i.to;if(l>o){l=i.lo;if(l>o)l=o}z=tp?m.s.sp(tp,','):0;if(i.l>0&&z&&o>=l)for(j=0;j<z.length;j++){c=z[j]?parseFloat(''+z[j]):0;if(c&&(l/i.l)*100<c" + "&&i.x>=c){t=1;j=z.length;w.mediaEvent=w.event='MILESTONE';i.lm=w.milestone=c}}z=to?m.s.sp(to,','):0;if(z&&o>=l)for(j=0;j<z.length;j++){c=z[j]?parseFloat(''+z[j]):0;if(c&&l<c&&o>=c){t=1;j=z.length;w" + ".mediaEvent=w.event='OFFSET_MILESTONE';i.lom=w.offsetMilestone=c}}}}if(i.sg||!sx){if(sm&&tp&&i.l>0){z=m.s.sp(tp,',');if(z){z[z.length]='100';l=0;for(j=0;j<z.length;j++){c=z[j]?parseFloat(''+z[j]):0" + ";if(c){if(i.x<c){sn=j+1;sx='M:'+l+'-'+c;j=z.length}l=c}}}}else if(so&&to){z=m.s.sp(to,',');if(z){z[z.length]=''+(i.l>0?i.l:'E');l=0;for(j=0;j<z.length;j++){c=z[j]?parseFloat(''+z[j]):0;if(c||z[j]==" + "'E'){if(o<c||z[j]=='E'){sn=j+1;sx='O:'+l+'-'+c;j=z.length}l=c}}}}if(sx)i.sg=1}if((sx||i.sx)&&sx!=i.sx){i.us=1;if(!i.sx){i.sn=sn;i.sx=sx}if(i.to>=0)t=1}if((x>=2||i.x>=100)&&i.lo<o){i.t+=o-i.lo;i.ts+" + "=o-i.lo}if(x<=2||(x==3&&!i.lx)){i.e+=(x==1||x==3?'S':'E')+Math.floor(o);i.lx=(x==3?1:x)}if(!t&&i.to>=0&&x<=3){ti=ti?ti:0;if(ti&&i.ts>=ti){t=1;w.mediaEvent=w.event='SECONDS'}}i.lt=ts;i.lo=o}if(!x||(" + "x<=3&&i.x>=100)){if(i.lx!=2)i.e+='E'+Math.floor(o);x=0;v=e=\"None\";w.mediaEvent=w.event=\"CLOSE\"}if(x==7){w.clicked=i.clk=1;t=1}if(x==5||(m.completeByCloseOffset&&(!x||i.x>=100)&&i.l>0&&o>=i.l-m." + "completeCloseOffsetThreshold)){w.complete=i.co=1;t=1}ek=w.mediaEvent;if(ek=='MILESTONE')ek+='_'+w.milestone;else if(ek=='OFFSET_MILESTONE')ek+='_'+w.offsetMilestone;if(!i.fel[ek]) {w.eventFirstTime" + "=true;i.fel[ek]=1}else w.eventFirstTime=false;w.timePlayed=i.t;w.segmentNum=i.sn;w.segment=i.sx;w.segmentLength=i.sl;if(m.monitor&&x!=4)m.monitor(m.s,w);if(x==0)m._delete(n);if(t&&i.tc==tc){vo=new " + "Object;vo.contextData=new Object;vo.linkTrackVars=v;vo.linkTrackEvents=e;if(!vo.linkTrackVars)vo.linkTrackVars='';if(!vo.linkTrackEvents)vo.linkTrackEvents='';if(m.trackUsingContextData)m.bcd(vo,i)" + ";else m.bpe(vo,i,x,o);m.s.t(vo);if(i.us){i.sn=sn;i.sx=sx;i.sc=1;i.us=0}else if(i.ts>0)i.sc=0;i.e=\"\";i.lm=i.lom=0;i.ts-=Math.floor(i.ts);i.to=o;i.tc++}}}return i};m.ae=function(n,l,p,x,o,sn,sx,sl," + "pd,b){var m=this,r=0;if(n&&(!m.autoTrackMediaLengthRequired||(length&&length>0)) &&p){if(!m.l||!m.l[n]){if(x==1||x==3){m.open(n,l,p,b);r=1}}else r=1;if(r)m.e(n,x,o,sn,sx,sl,pd)}};m.a=function(o,t){" + "var m=this,i=o.id?o.id:o.name,n=o.name,p=0,v,c,c1,c2,xc=m.s.h,x,e,f1,f2='s_media_'+m._in+'_oc',f3='s_media_'+m._in+'_t',f4='s_media_'+m._in+'_s',f5='s_media_'+m._in+'_l',f6='s_media_'+m._in+'_m',f7" + "='s_media_'+m._in+'_c',tcf,w;if(!i){if(!m.c)m.c=0;i='s_media_'+m._in+'_'+m.c;m.c++}if(!o.id)o.id=i;if(!o.name)o.name=n=i;if(!m.ol)m.ol=new Object;if(m.ol[i])return;m.ol[i]=o;if(!xc)xc=m.s.b;tcf=new" + " Function('o','var e,p=0;try{if(o.versionInfo&&o.currentMedia&&o.controls)p=1}catch(e){p=0}return p');p=tcf(o);if(!p){tcf=new Function('o','var e,p=0,t;try{t=o.GetQuickTimeVersion();if(t)p=2}catch(" + "e){p=0}return p');p=tcf(o);if(!p){tcf=new Function('o','var e,p=0,t;try{t=o.GetVersionInfo();if(t)p=3}catch(e){p=0}return p');p=tcf(o)}}v=\"var m=s_c_il[\"+m._in+\"],o=m.ol['\"+i+\"']\";if(p==1){p=" + "'Windows Media Player '+o.versionInfo;c1=v+',n,p,l,x=-1,cm,c,mn;if(o){cm=o.currentMedia;c=o.controls;if(cm&&c){mn=cm.name?cm.name:c.URL;l=cm.duration;p=c.currentPosition;n=o.playState;if(n){if(n==8" + ")x=0;if(n==3)x=1;if(n==1||n==2||n==4||n==5||n==6)x=2;}';c2='if(x>=0)m.ae(mn,l,\"'+p+'\",x,x!=2?p:-1,0,\"\",0,0,o)}}';c=c1+c2;if(m.s.isie&&xc){x=m.s.d.createElement('script');x.language='jscript';x." + "type='text/javascript';x.htmlFor=i;x.event='PlayStateChange(NewState)';x.defer=true;x.text=c;xc.appendChild(x);o[f6]=new Function(c1+'if(n==3){x=3;'+c2+'}setTimeout(o.'+f6+',5000)');o[f6]()}}if(p==" + "2){p='QuickTime Player '+(o.GetIsQuickTimeRegistered()?'Pro ':'')+o.GetQuickTimeVersion();f1=f2;c=v+',n,x,t,l,p,p2,mn;if(o){mn=o.GetMovieName()?o.GetMovieName():o.GetURL();n=o.GetRate();t=o.GetTime" + "Scale();l=o.GetDuration()/t;p=o.GetTime()/t;p2=o.'+f5+';if(n!=o.'+f4+'||p<p2||p-p2>5){x=2;if(n!=0)x=1;else if(p>=l)x=0;if(p<p2||p-p2>5)m.ae(mn,l,\"'+p+'\",2,p2,0,\"\",0,0,o);m.ae(mn,l,\"'+p+'\",x,x" + "!=2?p:-1,0,\"\",0,0,o)}if(n>0&&o.'+f7+'>=10){m.ae(mn,l,\"'+p+'\",3,p,0,\"\",0,0,o);o.'+f7+'=0}o.'+f7+'++;o.'+f4+'=n;o.'+f5+'=p;setTimeout(\"'+v+';o.'+f2+'(0,0)\",500)}';o[f1]=new Function('a','b',c" + ");o[f4]=-1;o[f7]=0;o[f1](0,0)}if(p==3){p='RealPlayer '+o.GetVersionInfo();f1=n+'_OnPlayStateChange';c1=v+',n,x=-1,l,p,mn;if(o){mn=o.GetTitle()?o.GetTitle():o.GetSource();n=o.GetPlayState();l=o.GetL" + "ength()/1000;p=o.GetPosition()/1000;if(n!=o.'+f4+'){if(n==3)x=1;if(n==0||n==2||n==4||n==5)x=2;if(n==0&&(p>=l||p==0))x=0;if(x>=0)m.ae(mn,l,\"'+p+'\",x,x!=2?p:-1,0,\"\",0,0,o)}if(n==3&&(o.'+f7+'>=10|" + "|!o.'+f3+')){m.ae(mn,l,\"'+p+'\",3,p,0,\"\",0,0,o);o.'+f7+'=0}o.'+f7+'++;o.'+f4+'=n;';c2='if(o.'+f2+')o.'+f2+'(o,n)}';if(m.s.wd[f1])o[f2]=m.s.wd[f1];m.s.wd[f1]=new Function('a','b',c1+c2);o[f1]=new" + " Function('a','b',c1+'setTimeout(\"'+v+';o.'+f1+'(0,0)\",o.'+f3+'?500:5000);'+c2);o[f4]=-1;if(m.s.isie)o[f3]=1;o[f7]=0;o[f1](0,0)}};m.as=new Function('e','var m=s_c_il['+m._in+'],l,n;if(m.autoTrack" + "&&m.s.d.getElementsByTagName){l=m.s.d.getElementsByTagName(m.s.isie?\"OBJECT\":\"EMBED\");if(l)for(n=0;n<l.length;n++)m.a(l[n]);}');if(s.wd.attachEvent)s.wd.attachEvent('onload',m.as);else if(s.wd." + "addEventListener)s.wd.addEventListener('load',m.as,false);if(m.onLoad)m.onLoad(s,m)";
                s_omntr.m_i("Media");

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

                }

                try {
                    initDashNavListeners();
                } catch (err) {

                }

                try {
                    customLinksConf();
                } catch (err) {

                }

                // call doPlugins function
                s_omntr.doPlugins = s_doPlugins;


                if (sendOmntrImage) {
                    //send image to the sytecatalyst servers 
                    s_omntr.t();
                }
            }
        } catch (err) {

        }
    }


    /* Global variables */

    function s_doPlugins(s_omntr) {
        try {
            /* Add calls to plugins here */

            /* Form Analysis */
            s_omntr.setupFormAnalysis();

            // Internal Campaigns.
            s_omntr.prop41 = s_omntr.getQueryParam('intcmp');

            // External Campaigns.
            var regexcmp = /[A|M]\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:.[^\_]+)\_(?:\d){8}/; //Decided o use this more general form as many others wouldn´t be captured in case we use a stronger Regex
            var valCmp = s_omntr.getQueryParam('cmp');
            var cleanCmp = regexcmp.exec(valCmp);
            if (cleanCmp) {
                s_omntr.campaign = cleanCmp; //Takes the clean value (No extra characters or double tracking codes) in case the regex meets the value
            }
            else {
                s_omntr.campaign = valCmp; //In case the CMP is wrongly formatted (doesn´t match the regex), capture it anyway to avoid losing wrong tracking codes (Better to capture it at least as an statistic)
            }

            //Capture EventID query string value in Events & Promotions pages
            if (s_omntr.prop19 && s_omntr.prop19 != '') {  //Prop19 contains the DSID in case user is loggedIn                    
                s_omntr.prop43 = s_omntr.getQueryParam('EventID');
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
            s_omntr.prop12 = s_omntr.getQueryParam('q');
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

            /* Enhanced download tracking */
            s_omntr.url = s_omntr.downloadLinkHandler();
            if (s_omntr.url) {
                // FileName Download. Traffic Variable ex: prop60 = GDO_FAQs.pdf
                s_omntr.prop60 = s_omntr.eVar7 = s_omntr.url.substring(s_omntr.url.lastIndexOf("/") + 1, s_omntr.url.length);
                // File Downloads. Event Variable - Counter
                s_omntr.events = s_omntr.apl(s_omntr.events, "event4", ",", 2); //Track eVar & Event
                s_omntr.linkTrackVars = "prop60,eVar7,events";
                s_omntr.linkTrackEvents = "event4";
            }

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

            // Percent Page Viewed. Traffic Variable ex: prop55 = 71
            if (s_prevPage) {
                s_omntr.prop55 = s_omntr.getPercentPageViewed();
            }

            // Exit URL. Traffic Variable ex: prop56 = http://www.herbalifeapparel.com/ 
            var url = s_omntr.exitLinkHandler(s_omntr.linkInternalFilters);
            if (url) {
                s_omntr.prop56 = url;
                if (s_prevPage) {
                    s_omntr.linkTrackVars = 'prop54,prop55,prop56';
                    s_omntr.prop55 = s_omntr.getPercentPageViewed();
                }
            }


            s_omntr.setupDynamicObjectIDs(); //dynamic object IDs

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

            // Detects flash and silverlight
            s_omntr.detectRIA('s_ria', 'prop61', 'prop62', '', '', '');

            //Determine bounce rate for all visits
            var temp_prop = s_omntr.getVisitStart("s_visit");
            if (temp_prop && temp_prop == 1) {
                s_omntr.firstPage = 'firstpage';
            }
            /*Time to Load Tracking*/
            s_omntr.prop40 = s_omntr.s_getLoadTime();
            // First Page Event. Event Variabe - Counter
            s_omntr.clickPast(s_omntr.firstPage, 'event32', 'event33');
        } catch (err) {

        }
    }

    /************* DO NOT ALTER ANYTHING BELOW THIS LINE ! **************/
    var s_objectID;

    function s_gi(un, pg, ss) {
        var c = "s.version='H.27.5';s.an=s_an;s.logDebug=function(m){var s=this,tcf=new Function('var e;try{console.log(\"'+s.rep(s.rep(s.rep(m,\"\\\\\",\"\\\\" + "\\\\\"),\"\\n\",\"\\\\n\"),\"\\\"\",\"\\\\\\\"\")+'\");}catch(e){}');tcf()};s.cls=function(x,c){var i,y='';if(!c)c=this.an;for(i=0;i<x.length;i++){n=x.substring(i,i+1);if(c.indexOf(n)>=0)y+=n}retur" + "n y};s.fl=function(x,l){return x?(''+x).substring(0,l):x};s.co=function(o){return o};s.num=function(x){x=''+x;for(var p=0;p<x.length;p++)if(('0123456789').indexOf(x.substring(p,p+1))<0)return 0;ret" + "urn 1};s.rep=s_rep;s.sp=s_sp;s.jn=s_jn;s.ape=function(x){var s=this,h='0123456789ABCDEF',f=\"+~!*()'\",i,c=s.charSet,n,l,e,y='';c=c?c.toUpperCase():'';if(x){x=''+x;if(s.em==3){x=encodeURIComponent(" + "x);for(i=0;i<f.length;i++) {n=f.substring(i,i+1);if(x.indexOf(n)>=0)x=s.rep(x,n,\"%\"+n.charCodeAt(0).toString(16).toUpperCase())}}else if(c=='AUTO'&&('').charCodeAt){for(i=0;i<x.length;i++){c=x.su" + "bstring(i,i+1);n=x.charCodeAt(i);if(n>127){l=0;e='';while(n||l<4){e=h.substring(n%16,n%16+1)+e;n=(n-n%16)/16;l++}y+='%u'+e}else if(c=='+')y+='%2B';else y+=escape(c)}x=y}else x=s.rep(escape(''+x),'+" + "','%2B');if(c&&c!='AUTO'&&s.em==1&&x.indexOf('%u')<0&&x.indexOf('%U')<0){i=x.indexOf('%');while(i>=0){i++;if(h.substring(8).indexOf(x.substring(i,i+1).toUpperCase())>=0)return x.substring(0,i)+'u00" + "'+x.substring(i);i=x.indexOf('%',i)}}}return x};s.epa=function(x){var s=this,y,tcf;if(x){x=s.rep(''+x,'+',' ');if(s.em==3){tcf=new Function('x','var y,e;try{y=decodeURIComponent(x)}catch(e){y=unesc" + "ape(x)}return y');return tcf(x)}else return unescape(x)}return y};s.pt=function(x,d,f,a){var s=this,t=x,z=0,y,r;while(t){y=t.indexOf(d);y=y<0?t.length:y;t=t.substring(0,y);r=s[f](t,a);if(r)return r" + ";z+=y+d.length;t=x.substring(z,x.length);t=z<x.length?t:''}return ''};s.isf=function(t,a){var c=a.indexOf(':');if(c>=0)a=a.substring(0,c);c=a.indexOf('=');if(c>=0)a=a.substring(0,c);if(t.substring(" + "0,2)=='s_')t=t.substring(2);return (t!=''&&t==a)};s.fsf=function(t,a){var s=this;if(s.pt(a,',','isf',t))s.fsg+=(s.fsg!=''?',':'')+t;return 0};s.fs=function(x,f){var s=this;s.fsg='';s.pt(x,',','fsf'" + ",f);return s.fsg};s.mpc=function(m,a){var s=this,c,l,n,v;v=s.d.visibilityState;if(!v)v=s.d.webkitVisibilityState;if(v&&v=='prerender'){if(!s.mpq){s.mpq=new Array;l=s.sp('webkitvisibilitychange,visi" + "bilitychange',',');for(n=0;n<l.length;n++){s.d.addEventListener(l[n],new Function('var s=s_c_il['+s._in+'],c,v;v=s.d.visibilityState;if(!v)v=s.d.webkitVisibilityState;if(s.mpq&&v==\"visible\"){whil" + "e(s.mpq.length>0){c=s.mpq.shift();s[c.m].apply(s,c.a)}s.mpq=0}'),false)}}c=new Object;c.m=m;c.a=a;s.mpq.push(c);return 1}return 0};s.si=function(){var s=this,i,k,v,c=s_gi+'var s=s_gi(\"'+s.oun+'\")" + ";s.sa(\"'+s.un+'\");';for(i=0;i<s.va_g.length;i++){k=s.va_g[i];v=s[k];if(v!=undefined){if(typeof(v)!='number')c+='s.'+k+'=\"'+s_fe(v)+'\";';else c+='s.'+k+'='+v+';'}}c+=\"s.lnk=s.eo=s.linkName=s.li" + "nkType=s.wd.s_objectID=s.ppu=s.pe=s.pev1=s.pev2=s.pev3='';\";return c};s.c_d='';s.c_gdf=function(t,a){var s=this;if(!s.num(t))return 1;return 0};s.c_gd=function(){var s=this,d=s.wd.location.hostnam" + "e,n=s.fpCookieDomainPeriods,p;if(!n)n=s.cookieDomainPeriods;if(d&&!s.c_d){n=n?parseInt(n):2;n=n>2?n:2;p=d.lastIndexOf('.');if(p>=0){while(p>=0&&n>1){p=d.lastIndexOf('.',p-1);n--}s.c_d=p>0&&s.pt(d,'" + ".','c_gdf',0)?d.substring(p):d}}return s.c_d};s.c_r=function(k){var s=this;k=s.ape(k);var c=' '+s.d.cookie,i=c.indexOf(' '+k+'='),e=i<0?i:c.indexOf(';',i),v=i<0?'':s.epa(c.substring(i+2+k.length,e<" + "0?c.length:e));return v!='[[B]]'?v:''};s.c_w=function(k,v,e){var s=this,d=s.c_gd(),l=s.cookieLifetime,t;v=''+v;l=l?(''+l).toUpperCase():'';if(e&&l!='SESSION'&&l!='NONE'){t=(v!=''?parseInt(l?l:0):-6" + "0);if(t){e=new Date;e.setTime(e.getTime()+(t*1000))}}if(k&&l!='NONE'){s.d.cookie=k+'='+s.ape(v!=''?v:'[[B]]')+'; path=/;'+(e&&l!='SESSION'?' expires='+e.toGMTString()+';':'')+(d?' domain='+d+';':''" + ");return s.c_r(k)==v}return 0};s.eh=function(o,e,r,f){var s=this,b='s_'+e+'_'+s._in,n=-1,l,i,x;if(!s.ehl)s.ehl=new Array;l=s.ehl;for(i=0;i<l.length&&n<0;i++){if(l[i].o==o&&l[i].e==e)n=i}if(n<0){n=i" + ";l[n]=new Object}x=l[n];x.o=o;x.e=e;f=r?x.b:f;if(r||f){x.b=r?0:o[e];x.o[e]=f}if(x.b){x.o[b]=x.b;return b}return 0};s.cet=function(f,a,t,o,b){var s=this,r,tcf;if(s.apv>=5&&(!s.isopera||s.apv>=7)){tc" + "f=new Function('s','f','a','t','var e,r;try{r=s[f](a)}catch(e){r=s[t](e)}return r');r=tcf(s,f,a,t)}else{if(s.ismac&&s.u.indexOf('MSIE 4')>=0)r=s[b](a);else{s.eh(s.wd,'onerror',0,o);r=s[f](a);s.eh(s" + ".wd,'onerror',1)}}return r};s.gtfset=function(e){var s=this;return s.tfs};s.gtfsoe=new Function('e','var s=s_c_il['+s._in+'],c;s.eh(window,\"onerror\",1);s.etfs=1;c=s.t();if(c)s.d.write(c);s.etfs=0" + ";return true');s.gtfsfb=function(a){return window};s.gtfsf=function(w){var s=this,p=w.parent,l=w.location;s.tfs=w;if(p&&p.location!=l&&p.location.host==l.host){s.tfs=p;return s.gtfsf(s.tfs)}return " + "s.tfs};s.gtfs=function(){var s=this;if(!s.tfs){s.tfs=s.wd;if(!s.etfs)s.tfs=s.cet('gtfsf',s.tfs,'gtfset',s.gtfsoe,'gtfsfb')}return s.tfs};s.mrq=function(u){var s=this,l=s.rl[u],n,r;s.rl[u]=0;if(l)fo" + "r(n=0;n<l.length;n++){r=l[n];s.mr(0,0,r.r,r.t,r.u)}};s.flushBufferedRequests=function(){};s.tagContainerMarker='';s.mr=function(sess,q,rs,ta,u){var s=this,dc=s.dc,t1=s.trackingServer,t2=s.trackingS" + "erverSecure,tb=s.trackingServerBase,p='.sc',ns=s.visitorNamespace,un=s.cls(u?u:(ns?ns:s.fun)),r=new Object,l,imn='s_i_'+s._in+'_'+un,im,b,e;if(!rs){if(t1){if(t2&&s.ssl)t1=t2}else{if(!tb)tb='2o7.net" + "';if(dc)dc=(''+dc).toLowerCase();else dc='d1';if(tb=='2o7.net'){if(dc=='d1')dc='112';else if(dc=='d2')dc='122';p=''}t1=un+'.'+dc+'.'+p+tb}rs='http'+(s.ssl?'s':'')+'://'+t1+'/b/ss/'+s.un+'/'+(s.mobi" + "le?'5.1':'1')+'/'+s.version+(s.tcn?'T':'')+(s.tagContainerMarker?\"-\"+s.tagContainerMarker:\"\")+'/'+sess+'?AQB=1&ndh=1'+(q?q:'')+'&AQE=1';if(s.isie&&!s.ismac)rs=s.fl(rs,2047)}if(s.d.images&&s.apv" + ">=3&&(!s.isopera||s.apv>=7)&&(s.ns6<0||s.apv>=6.1)){if(!s.rc)s.rc=new Object;if(!s.rc[un]){s.rc[un]=1;if(!s.rl)s.rl=new Object;s.rl[un]=new Array;setTimeout('if(window.s_c_il)window.s_c_il['+s._in+" + "'].mrq(\"'+un+'\")',750)}else{l=s.rl[un];if(l){r.t=ta;r.u=un;r.r=rs;l[l.length]=r;return ''}imn+='_'+s.rc[un];s.rc[un]++}if(s.debugTracking){var d='AppMeasurement Debug: '+rs,dl=s.sp(rs,'&'),dln;fo" + "r(dln=0;dln<dl.length;dln++)d+=\"\\n\\t\"+s.epa(dl[dln]);s.logDebug(d)}im=s.wd[imn];if(!im)im=s.wd[imn]=new Image;im.alt=\"\";im.s_l=0;im.onload=im.onerror=new Function('e','this.s_l=1;var wd=windo" + "w,s;if(wd.s_c_il){s=wd.s_c_il['+s._in+'];s.bcr();s.mrq(\"'+un+'\");s.nrs--;if(!s.nrs)s.m_m(\"rr\")}');if(!s.nrs){s.nrs=1;s.m_m('rs')}else s.nrs++;im.src=rs;if(s.useForcedLinkTracking||s.bcf){if(!s." + "forcedLinkTrackingTimeout)s.forcedLinkTrackingTimeout=250;setTimeout('if(window.s_c_il)window.s_c_il['+s._in+'].bcr()',s.forcedLinkTrackingTimeout);}else if((s.lnk||s.eo)&&(!ta||ta=='_self'||ta=='_" + "top'||ta=='_parent'||(s.wd.name&&ta==s.wd.name))){b=e=new Date;while(!im.s_l&&e.getTime()-b.getTime()<500)e=new Date}return ''}return '<im'+'g sr'+'c=\"'+rs+'\" width=1 height=1 border=0 alt=\"\">'" + "};s.gg=function(v){var s=this;if(!s.wd['s_'+v])s.wd['s_'+v]='';return s.wd['s_'+v]};s.glf=function(t,a){if(t.substring(0,2)=='s_')t=t.substring(2);var s=this,v=s.gg(t);if(v)s[t]=v};s.gl=function(v)" + "{var s=this;if(s.pg)s.pt(v,',','glf',0)};s.rf=function(x){var s=this,y,i,j,h,p,l=0,q,a,b='',c='',t;if(x&&x.length>255){y=''+x;i=y.indexOf('?');if(i>0){q=y.substring(i+1);y=y.substring(0,i);h=y.toLo" + "werCase();j=0;if(h.substring(0,7)=='http://')j+=7;else if(h.substring(0,8)=='https://')j+=8;i=h.indexOf(\"/\",j);if(i>0){h=h.substring(j,i);p=y.substring(i);y=y.substring(0,i);if(h.indexOf('google'" + ")>=0)l=',q,ie,start,search_key,word,kw,cd,';else if(h.indexOf('yahoo.co')>=0)l=',p,ei,';if(l&&q){a=s.sp(q,'&');if(a&&a.length>1){for(j=0;j<a.length;j++){t=a[j];i=t.indexOf('=');if(i>0&&l.indexOf('," + "'+t.substring(0,i)+',')>=0)b+=(b?'&':'')+t;else c+=(c?'&':'')+t}if(b&&c)q=b+'&'+c;else c=''}i=253-(q.length-c.length)-y.length;x=y+(i>0?p.substring(0,i):'')+'?'+q}}}}return x};s.s2q=function(k,v,vf" + ",vfp,f){var s=this,qs='',sk,sv,sp,ss,nke,nk,nf,nfl=0,nfn,nfm;if(k==\"contextData\")k=\"c\";if(v){for(sk in v)if((!f||sk.substring(0,f.length)==f)&&v[sk]&&(!vf||vf.indexOf(','+(vfp?vfp+'.':'')+sk+'," + "')>=0)&&(!Object||!Object.prototype||!Object.prototype[sk])){nfm=0;if(nfl)for(nfn=0;nfn<nfl.length;nfn++)if(sk.substring(0,nfl[nfn].length)==nfl[nfn])nfm=1;if(!nfm){if(qs=='')qs+='&'+k+'.';sv=v[sk]" + ";if(f)sk=sk.substring(f.length);if(sk.length>0){nke=sk.indexOf('.');if(nke>0){nk=sk.substring(0,nke);nf=(f?f:'')+nk+'.';if(!nfl)nfl=new Array;nfl[nfl.length]=nf;qs+=s.s2q(nk,v,vf,vfp,nf)}else{if(ty" + "peof(sv)=='boolean'){if(sv)sv='true';else sv='false'}if(sv){if(vfp=='retrieveLightData'&&f.indexOf('.contextData.')<0){sp=sk.substring(0,4);ss=sk.substring(4);if(sk=='transactionID')sk='xact';else " + "if(sk=='channel')sk='ch';else if(sk=='campaign')sk='v0';else if(s.num(ss)){if(sp=='prop')sk='c'+ss;else if(sp=='eVar')sk='v'+ss;else if(sp=='list')sk='l'+ss;else if(sp=='hier'){sk='h'+ss;sv=sv.subs" + "tring(0,255)}}}qs+='&'+s.ape(sk)+'='+s.ape(sv)}}}}}if(qs!='')qs+='&.'+k}return qs};s.hav=function(){var s=this,qs='',l,fv='',fe='',mn,i,e;if(s.lightProfileID){l=s.va_m;fv=s.lightTrackVars;if(fv)fv=" + "','+fv+','+s.vl_mr+','}else{l=s.va_t;if(s.pe||s.linkType){fv=s.linkTrackVars;fe=s.linkTrackEvents;if(s.pe){mn=s.pe.substring(0,1).toUpperCase()+s.pe.substring(1);if(s[mn]){fv=s[mn].trackVars;fe=s[m" + "n].trackEvents}}}if(fv)fv=','+fv+','+s.vl_l+','+s.vl_l2;if(fe){fe=','+fe+',';if(fv)fv+=',events,'}if (s.events2)e=(e?',':'')+s.events2}for(i=0;i<l.length;i++){var k=l[i],v=s[k],b=k.substring(0,4),x" + "=k.substring(4),n=parseInt(x),q=k;if(!v)if(k=='events'&&e){v=e;e=''}if(v&&(!fv||fv.indexOf(','+k+',')>=0)&&k!='linkName'&&k!='linkType'){if(k=='supplementalDataID')q='sdid';else if(k=='timestamp')q" + "='ts';else if(k=='dynamicVariablePrefix')q='D';else if(k=='visitorID')q='vid';else if(k=='marketingCloudVisitorID')q='mid';else if(k=='analyticsVisitorID')q='aid';else if(k=='audienceManagerLocatio" + "nHint')q='aamlh';else if(k=='audienceManagerBlob')q='aamb';else if(k=='authState')q='as';else if(k=='pageURL'){q='g';if(v.length>255){s.pageURLRest=v.substring(255);v=v.substring(0,255);}}else if(k" + "=='pageURLRest')q='-g';else if(k=='referrer'){q='r';v=s.fl(s.rf(v),255)}else if(k=='vmk'||k=='visitorMigrationKey')q='vmt';else if(k=='visitorMigrationServer'){q='vmf';if(s.ssl&&s.visitorMigrationS" + "erverSecure)v=''}else if(k=='visitorMigrationServerSecure'){q='vmf';if(!s.ssl&&s.visitorMigrationServer)v=''}else if(k=='charSet'){q='ce';if(v.toUpperCase()=='AUTO')v='ISO8859-1';else if(s.em==2||s" + ".em==3)v='UTF-8'}else if(k=='visitorNamespace')q='ns';else if(k=='cookieDomainPeriods')q='cdp';else if(k=='cookieLifetime')q='cl';else if(k=='variableProvider')q='vvp';else if(k=='currencyCode')q='" + "cc';else if(k=='channel')q='ch';else if(k=='transactionID')q='xact';else if(k=='campaign')q='v0';else if(k=='resolution')q='s';else if(k=='colorDepth')q='c';else if(k=='javascriptVersion')q='j';els" + "e if(k=='javaEnabled')q='v';else if(k=='cookiesEnabled')q='k';else if(k=='browserWidth')q='bw';else if(k=='browserHeight')q='bh';else if(k=='connectionType')q='ct';else if(k=='homepage')q='hp';else" + " if(k=='plugins')q='p';else if(k=='events'){if(e)v+=(v?',':'')+e;if(fe)v=s.fs(v,fe)}else if(k=='events2')v='';else if(k=='contextData'){qs+=s.s2q('c',s[k],fv,k,0);v=''}else if(k=='lightProfileID')q" + "='mtp';else if(k=='lightStoreForSeconds'){q='mtss';if(!s.lightProfileID)v=''}else if(k=='lightIncrementBy'){q='mti';if(!s.lightProfileID)v=''}else if(k=='retrieveLightProfiles')q='mtsr';else if(k==" + "'deleteLightProfiles')q='mtsd';else if(k=='retrieveLightData'){if(s.retrieveLightProfiles)qs+=s.s2q('mts',s[k],fv,k,0);v=''}else if(s.num(x)){if(b=='prop')q='c'+n;else if(b=='eVar')q='v'+n;else if(" + "b=='list')q='l'+n;else if(b=='hier'){q='h'+n;v=s.fl(v,255)}}if(v)qs+='&'+s.ape(q)+'='+(k.substring(0,3)!='pev'?s.ape(v):v)}}return qs};s.ltdf=function(t,h){t=t?t.toLowerCase():'';h=h?h.toLowerCase(" + "):'';var qi=h.indexOf('?'),hi=h.indexOf('#');if(qi>=0){if(hi>=0&&hi<qi)qi=hi;}else qi=hi;h=qi>=0?h.substring(0,qi):h;if(t&&h.substring(h.length-(t.length+1))=='.'+t)return 1;return 0};s.ltef=functi" + "on(t,h){t=t?t.toLowerCase():'';h=h?h.toLowerCase():'';if(t&&h.indexOf(t)>=0)return 1;return 0};s.lt=function(h){var s=this,lft=s.linkDownloadFileTypes,lef=s.linkExternalFilters,lif=s.linkInternalFi" + "lters;lif=lif?lif:s.wd.location.hostname;h=h.toLowerCase();if(s.trackDownloadLinks&&lft&&s.pt(lft,',','ltdf',h))return 'd';if(s.trackExternalLinks&&h.indexOf('#')!=0&&h.indexOf('about:')!=0&&h.inde" + "xOf('javascript:')!=0&&(lef||lif)&&(!lef||s.pt(lef,',','ltef',h))&&(!lif||!s.pt(lif,',','ltef',h)))return 'e';return ''};s.lc=new Function('e','var s=s_c_il['+s._in+'],b=s.eh(this,\"onclick\");s.ln" + "k=this;s.t();s.lnk=0;if(b)return this[b](e);return true');s.bcr=function(){var s=this;if(s.bct&&s.bce)s.bct.dispatchEvent(s.bce);if(s.bcf){if(typeof(s.bcf)=='function')s.bcf();else if(s.bct&&s.bct." + "href)s.d.location=s.bct.href}s.bct=s.bce=s.bcf=0};s.bc=new Function('e','if(e&&e.s_fe)return;var s=s_c_il['+s._in+'],f,tcf,t,n,nrs,a,h;if(s.d&&s.d.all&&s.d.all.cppXYctnr)return;if(!s.bbc)s.useForce" + "dLinkTracking=0;else if(!s.useForcedLinkTracking){s.b.removeEventListener(\"click\",s.bc,true);s.bbc=s.useForcedLinkTracking=0;return}else s.b.removeEventListener(\"click\",s.bc,false);s.eo=e.srcEl" + "ement?e.srcElement:e.target;nrs=s.nrs;s.t();s.eo=0;if(s.nrs>nrs&&s.useForcedLinkTracking&&e.target){a=e.target;while(a&&a!=s.b&&a.tagName.toUpperCase()!=\"A\"&&a.tagName.toUpperCase()!=\"AREA\")a=a" + ".parentNode;if(a){h=a.href;if(h.indexOf(\"#\")==0||h.indexOf(\"about:\")==0||h.indexOf(\"javascript:\")==0)h=0;t=a.target;if(e.target.dispatchEvent&&h&&(!t||t==\"_self\"||t==\"_top\"||t==\"_parent" + "\"||(s.wd.name&&t==s.wd.name))){tcf=new Function(\"s\",\"var x;try{n=s.d.createEvent(\\\\\"MouseEvents\\\\\")}catch(x){n=new MouseEvent}return n\");n=tcf(s);if(n){tcf=new Function(\"n\",\"e\",\"var" + " x;try{n.initMouseEvent(\\\\\"click\\\\\",e.bubbles,e.cancelable,e.view,e.detail,e.screenX,e.screenY,e.clientX,e.clientY,e.ctrlKey,e.altKey,e.shiftKey,e.metaKey,e.button,e.relatedTarget)}catch(x){n" + "=0}return n\");n=tcf(n,e);if(n){n.s_fe=1;e.stopPropagation();if (e.stopImmediatePropagation) {e.stopImmediatePropagation();}e.preventDefault();s.bct=e.target;s.bce=n}}}}}');s.oh=function(o){var s=t" + "his,l=s.wd.location,h=o.href?o.href:'',i,j,k,p;i=h.indexOf(':');j=h.indexOf('?');k=h.indexOf('/');if(h&&(i<0||(j>=0&&i>j)||(k>=0&&i>k))){p=o.protocol&&o.protocol.length>1?o.protocol:(l.protocol?l.p" + "rotocol:'');i=l.pathname.lastIndexOf('/');h=(p?p+'//':'')+(o.host?o.host:(l.host?l.host:''))+(h.substring(0,1)!='/'?l.pathname.substring(0,i<0?0:i)+'/':'')+h}return h};s.ot=function(o){var t=o.tagN" + "ame;if(o.tagUrn||(o.scopeName&&o.scopeName.toUpperCase()!='HTML'))return '';t=t&&t.toUpperCase?t.toUpperCase():'';if(t=='SHAPE')t='';if(t){if((t=='INPUT'||t=='BUTTON')&&o.type&&o.type.toUpperCase)t" + "=o.type.toUpperCase();else if(!t&&o.href)t='A';}return t};s.oid=function(o){var s=this,t=s.ot(o),p,c,n='',x=0;if(t&&!o.s_oid){p=o.protocol;c=o.onclick;if(o.href&&(t=='A'||t=='AREA')&&(!c||!p||p.toL" + "owerCase().indexOf('javascript')<0))n=s.oh(o);else if(c){n=s.rep(s.rep(s.rep(s.rep(''+c,\"\\r\",''),\"\\n\",''),\"\\t\",''),' ','');x=2}else if(t=='INPUT'||t=='SUBMIT'){if(o.value)n=o.value;else if" + "(o.innerText)n=o.innerText;else if(o.textContent)n=o.textContent;x=3}else if(o.src&&t=='IMAGE')n=o.src;if(n){o.s_oid=s.fl(n,100);o.s_oidt=x}}return o.s_oid};s.rqf=function(t,un){var s=this,e=t.inde" + "xOf('='),u=e>=0?t.substring(0,e):'',q=e>=0?s.epa(t.substring(e+1)):'';if(u&&q&&(','+u+',').indexOf(','+un+',')>=0){if(u!=s.un&&s.un.indexOf(',')>=0)q='&u='+u+q+'&u=0';return q}return ''};s.rq=funct" + "ion(un){if(!un)un=this.un;var s=this,c=un.indexOf(','),v=s.c_r('s_sq'),q='';if(c<0)return s.pt(v,'&','rqf',un);return s.pt(un,',','rq',0)};s.sqp=function(t,a){var s=this,e=t.indexOf('='),q=e<0?'':s" + ".epa(t.substring(e+1));s.sqq[q]='';if(e>=0)s.pt(t.substring(0,e),',','sqs',q);return 0};s.sqs=function(un,q){var s=this;s.squ[un]=q;return 0};s.sq=function(q){var s=this,k='s_sq',v=s.c_r(k),x,c=0;s" + ".sqq=new Object;s.squ=new Object;s.sqq[q]='';s.pt(v,'&','sqp',0);s.pt(s.un,',','sqs',q);v='';for(x in s.squ)if(x&&(!Object||!Object.prototype||!Object.prototype[x]))s.sqq[s.squ[x]]+=(s.sqq[s.squ[x]" + "]?',':'')+x;for(x in s.sqq)if(x&&(!Object||!Object.prototype||!Object.prototype[x])&&s.sqq[x]&&(x==q||c<2)){v+=(v?'&':'')+s.sqq[x]+'='+s.ape(x);c++}return s.c_w(k,v,0)};s.wdl=new Function('e','var " + "s=s_c_il['+s._in+'],r=true,b=s.eh(s.wd,\"onload\"),i,o,oc;if(b)r=this[b](e);for(i=0;i<s.d.links.length;i++){o=s.d.links[i];oc=o.onclick?\"\"+o.onclick:\"\";if((oc.indexOf(\"s_gs(\")<0||oc.indexOf(" + "\".s_oc(\")>=0)&&oc.indexOf(\".tl(\")<0)s.eh(o,\"onclick\",0,s.lc);}return r');s.wds=function(){var s=this;if(s.apv>3&&(!s.isie||!s.ismac||s.apv>=5)){if(s.b&&s.b.attachEvent)s.b.attachEvent('onclic" + "k',s.bc);else if(s.b&&s.b.addEventListener){if(s.n&&((s.n.userAgent.indexOf('WebKit')>=0&&s.d.createEvent)||(s.n.userAgent.indexOf('Firefox/2')>=0&&s.wd.MouseEvent))){s.bbc=1;s.useForcedLinkTrackin" + "g=1;s.b.addEventListener('click',s.bc,true)}s.b.addEventListener('click',s.bc,false)}else s.eh(s.wd,'onload',0,s.wdl)}};s.vs=function(x){var s=this,v=s.visitorSampling,g=s.visitorSamplingGroup,k='s" + "_vsn_'+s.un+(g?'_'+g:''),n=s.c_r(k),e=new Date,y=e.getYear();e.setYear(y+10+(y<1900?1900:0));if(v){v*=100;if(!n){if(!s.c_w(k,x,e))return 0;n=x}if(n%10000>v)return 0}return 1};s.dyasmf=function(t,m)" + "{if(t&&m&&m.indexOf(t)>=0)return 1;return 0};s.dyasf=function(t,m){var s=this,i=t?t.indexOf('='):-1,n,x;if(i>=0&&m){var n=t.substring(0,i),x=t.substring(i+1);if(s.pt(x,',','dyasmf',m))return n}retu" + "rn 0};s.uns=function(){var s=this,x=s.dynamicAccountSelection,l=s.dynamicAccountList,m=s.dynamicAccountMatch,n,i;s.un=s.un.toLowerCase();if(x&&l){if(!m)m=s.wd.location.host;if(!m.toLowerCase)m=''+m" + ";l=l.toLowerCase();m=m.toLowerCase();n=s.pt(l,';','dyasf',m);if(n)s.un=n}i=s.un.indexOf(',');s.fun=i<0?s.un:s.un.substring(0,i)};s.sa=function(un){var s=this;if(s.un&&s.mpc('sa',arguments))return;s" + ".un=un;if(!s.oun)s.oun=un;else if((','+s.oun+',').indexOf(','+un+',')<0)s.oun+=','+un;s.uns()};s.m_i=function(n,a){var s=this,m,f=n.substring(0,1),r,l,i;if(!s.m_l)s.m_l=new Object;if(!s.m_nl)s.m_nl" + "=new Array;m=s.m_l[n];if(!a&&m&&m._e&&!m._i)s.m_a(n);if(!m){m=new Object,m._c='s_m';m._in=s.wd.s_c_in;m._il=s._il;m._il[m._in]=m;s.wd.s_c_in++;m.s=s;m._n=n;m._l=new Array('_c','_in','_il','_i','_e'" + ",'_d','_dl','s','n','_r','_g','_g1','_t','_t1','_x','_x1','_rs','_rr','_l');s.m_l[n]=m;s.m_nl[s.m_nl.length]=n}else if(m._r&&!m._m){r=m._r;r._m=m;l=m._l;for(i=0;i<l.length;i++)if(m[l[i]])r[l[i]]=m[" + "l[i]];r._il[r._in]=r;m=s.m_l[n]=r}if(f==f.toUpperCase())s[n]=m;return m};s.m_a=new Function('n','g','e','if(!g)g=\"m_\"+n;var s=s_c_il['+s._in+'],c=s[g+\"_c\"],m,x,f=0;if(s.mpc(\"m_a\",arguments))r" + "eturn;if(!c)c=s.wd[\"s_\"+g+\"_c\"];if(c&&s_d)s[g]=new Function(\"s\",s_ft(s_d(c)));x=s[g];if(!x)x=s.wd[\\'s_\\'+g];if(!x)x=s.wd[g];m=s.m_i(n,1);if(x&&(!m._i||g!=\"m_\"+n)){m._i=f=1;if((\"\"+x).ind" + "exOf(\"function\")>=0)x(s);else s.m_m(\"x\",n,x,e)}m=s.m_i(n,1);if(m._dl)m._dl=m._d=0;s.dlt();return f');s.m_m=function(t,n,d,e){t='_'+t;var s=this,i,x,m,f='_'+t,r=0,u;if(s.m_l&&s.m_nl)for(i=0;i<s." + "m_nl.length;i++){x=s.m_nl[i];if(!n||x==n){m=s.m_i(x);u=m[t];if(u){if((''+u).indexOf('function')>=0){if(d&&e)u=m[t](d,e);else if(d)u=m[t](d);else u=m[t]()}}if(u)r=1;u=m[t+1];if(u&&!m[f]){if((''+u).i" + "ndexOf('function')>=0){if(d&&e)u=m[t+1](d,e);else if(d)u=m[t+1](d);else u=m[t+1]()}}m[f]=1;if(u)r=1}}return r};s.m_ll=function(){var s=this,g=s.m_dl,i,o;if(g)for(i=0;i<g.length;i++){o=g[i];if(o)s.l" + "oadModule(o.n,o.u,o.d,o.l,o.e,1);g[i]=0}};s.loadModule=function(n,u,d,l,e,ln){var s=this,m=0,i,g,o=0,f1,f2,c=s.h?s.h:s.b,b,tcf;if(n){i=n.indexOf(':');if(i>=0){g=n.substring(i+1);n=n.substring(0,i)}" + "else g=\"m_\"+n;m=s.m_i(n)}if((l||(n&&!s.m_a(n,g)))&&u&&s.d&&c&&s.d.createElement){if(d){m._d=1;m._dl=1}if(ln){if(s.ssl)u=s.rep(u,'http:','https:');i='s_s:'+s._in+':'+n+':'+g;b='var s=s_c_il['+s._i" + "n+'],o=s.d.getElementById(\"'+i+'\");if(s&&o){if(!o.l&&s.wd.'+g+'){o.l=1;if(o.i)clearTimeout(o.i);o.i=0;s.m_a(\"'+n+'\",\"'+g+'\"'+(e?',\"'+e+'\"':'')+')}';f2=b+'o.c++;if(!s.maxDelay)s.maxDelay=250" + ";if(!o.l&&o.c<(s.maxDelay*2)/100)o.i=setTimeout(o.f2,100)}';f1=new Function('e',b+'}');tcf=new Function('s','c','i','u','f1','f2','var e,o=0;try{o=s.d.createElement(\"script\");if(o){o.type=\"text/" + "javascript\";'+(n?'o.id=i;o.defer=true;o.onload=o.onreadystatechange=f1;o.f2=f2;o.l=0;':'')+'o.src=u;c.appendChild(o);'+(n?'o.c=0;o.i=setTimeout(f2,100)':'')+'}}catch(e){o=0}return o');o=tcf(s,c,i," + "u,f1,f2)}else{o=new Object;o.n=n+':'+g;o.u=u;o.d=d;o.l=l;o.e=e;g=s.m_dl;if(!g)g=s.m_dl=new Array;i=0;while(i<g.length&&g[i])i++;g[i]=o}}else if(n){m=s.m_i(n);m._e=1}return m};s.voa=function(vo,r){v" + "ar s=this,l=s.va_g,i,k,v,x;for(i=0;i<l.length;i++){k=l[i];v=vo[k];if(v||vo['!'+k]){if(!r&&(k==\"contextData\"||k==\"retrieveLightData\")&&s[k])for(x in s[k])if(!v[x])v[x]=s[k][x];s[k]=v}}};s.vob=fu" + "nction(vo,onlySet){var s=this,l=s.va_g,i,k;for(i=0;i<l.length;i++){k=l[i];vo[k]=s[k];if(!onlySet&&!vo[k])vo['!'+k]=1}};s.dlt=new Function('var s=s_c_il['+s._in+'],d=new Date,i,vo,f=0;if(s.dll)for(i" + "=0;i<s.dll.length;i++){vo=s.dll[i];if(vo){if(!s.m_m(\"d\")||d.getTime()-vo._t>=s.maxDelay){s.dll[i]=0;s.t(vo)}else f=1}}if(s.dli)clearTimeout(s.dli);s.dli=0;if(f){if(!s.dli)s.dli=setTimeout(s.dlt,s" + ".maxDelay)}else s.dll=0');s.dl=function(vo){var s=this,d=new Date;if(!vo)vo=new Object;s.vob(vo);vo._t=d.getTime();if(!s.dll)s.dll=new Array;s.dll[s.dll.length]=vo;if(!s.maxDelay)s.maxDelay=250;s.d" + "lt()};s._waitingForMarketingCloudVisitorID = false;s._doneWaitingForMarketingCloudVisitorID = false;s._marketingCloudVisitorIDCallback=function(marketingCloudVisitorID) {var s=this;s.marketingCloud" + "VisitorID = marketingCloudVisitorID;s._doneWaitingForMarketingCloudVisitorID = true;s._callbackWhenReadyToTrackCheck();};s._waitingForAnalyticsVisitorID = false;s._doneWaitingForAnalyticsVisitorID " + "= false;s._analyticsVisitorIDCallback=function(analyticsVisitorID) {var s=this;s.analyticsVisitorID = analyticsVisitorID;s._doneWaitingForAnalyticsVisitorID = true;s._callbackWhenReadyToTrackCheck(" + ");};s._waitingForAudienceManagerLocationHint = false;s._doneWaitingForAudienceManagerLocationHint = false;s._audienceManagerLocationHintCallback=function(audienceManagerLocationHint) {var s=this;s." + "audienceManagerLocationHint = audienceManagerLocationHint;s._doneWaitingForAudienceManagerLocationHint = true;s._callbackWhenReadyToTrackCheck();};s._waitingForAudienceManagerBlob = false;s._doneWa" + "itingForAudienceManagerBlob = false;s._audienceManagerBlobCallback=function(audienceManagerBlob) {var s=this;s.audienceManagerBlob = audienceManagerBlob;s._doneWaitingForAudienceManagerBlob = true;" + "s._callbackWhenReadyToTrackCheck();};s.isReadyToTrack=function() {var s=this,readyToTrack = true,visitor = s.visitor;if ((visitor) && (visitor.isAllowed())) {if ((!s._waitingForMarketingCloudVisito" + "rID) && (!s.marketingCloudVisitorID) && (visitor.getMarketingCloudVisitorID)) {s._waitingForMarketingCloudVisitorID = true;s.marketingCloudVisitorID = visitor.getMarketingCloudVisitorID([s,s._marke" + "tingCloudVisitorIDCallback]);if (s.marketingCloudVisitorID) {s._doneWaitingForMarketingCloudVisitorID = true;}}if ((!s._waitingForAnalyticsVisitorID) && (!s.analyticsVisitorID) && (visitor.getAnaly" + "ticsVisitorID)) {s._waitingForAnalyticsVisitorID = true;s.analyticsVisitorID = visitor.getAnalyticsVisitorID([s,s._analyticsVisitorIDCallback]);if (s.analyticsVisitorID) {s._doneWaitingForAnalytics" + "VisitorID = true;}}if ((!s._waitingForAudienceManagerLocationHint) && (!s.audienceManagerLocationHint) && (visitor.getAudienceManagerLocationHint)) {s._waitingForAudienceManagerLocationHint = true;" + "s.audienceManagerLocationHint = visitor.getAudienceManagerLocationHint([s,s._audienceManagerLocationHintCallback]);if (s.audienceManagerLocationHint) {s._doneWaitingForAudienceManagerLocationHint =" + " true;}}if ((!s._waitingForAudienceManagerBlob) && (!s.audienceManagerBlob) && (visitor.getAudienceManagerBlob)) {s._waitingForAudienceManagerBlob = true;s.audienceManagerBlob = visitor.getAudience" + "ManagerBlob([s,s._audienceManagerBlobCallback]);if (s.audienceManagerBlob) {s._doneWaitingForAudienceManagerBlob = true;}}if (((s._waitingForMarketingCloudVisitorID)     && (!s._doneWaitingForMarke" + "tingCloudVisitorID)     && (!s.marketingCloudVisitorID)) ||((s._waitingForAnalyticsVisitorID)          && (!s._doneWaitingForAnalyticsVisitorID)          && (!s.analyticsVisitorID)) ||((s._waitingF" + "orAudienceManagerLocationHint) && (!s._doneWaitingForAudienceManagerLocationHint) && (!s.audienceManagerLocationHint)) ||((s._waitingForAudienceManagerBlob)         && (!s._doneWaitingForAudienceMa" + "nagerBlob)         && (!s.audienceManagerBlob))) {readyToTrack = false;}}return readyToTrack;};s._callbackWhenReadyToTrackQueue = null;s._callbackWhenReadyToTrackInterval = 0;s.callbackWhenReadyToT" + "rack=function(callbackThis,callback,args) {var s=this,callbackInfo;callbackInfo = {};callbackInfo.callbackThis = callbackThis;callbackInfo.callback     = callback;callbackInfo.args         = args;i" + "f (s._callbackWhenReadyToTrackQueue == null) {s._callbackWhenReadyToTrackQueue = [];}s._callbackWhenReadyToTrackQueue.push(callbackInfo);if (s._callbackWhenReadyToTrackInterval == 0) {s._callbackWh" + "enReadyToTrackInterval = setInterval(s._callbackWhenReadyToTrackCheck,100);}};s._callbackWhenReadyToTrackCheck=new Function('var s=s_c_il['+s._in+'],callbackNum,callbackInfo;if (s.isReadyToTrack())" + " {if (s._callbackWhenReadyToTrackInterval) {clearInterval(s._callbackWhenReadyToTrackInterval);s._callbackWhenReadyToTrackInterval = 0;}if (s._callbackWhenReadyToTrackQueue != null) {while (s._call" + "backWhenReadyToTrackQueue.length > 0) {callbackInfo = s._callbackWhenReadyToTrackQueue.shift();callbackInfo.callback.apply(callbackInfo.callbackThis,callbackInfo.args);}}}');s._handleNotReadyToTrac" + "k=function(variableOverrides) {var s=this,args,varKey,variableOverridesCopy = null,setVariables = null;if (!s.isReadyToTrack()) {args = [];if (variableOverrides != null) {variableOverridesCopy = {}" + ";for (varKey in variableOverrides) {variableOverridesCopy[varKey] = variableOverrides[varKey];}}setVariables = {};s.vob(setVariables,true);args.push(variableOverridesCopy);args.push(setVariables);s" + ".callbackWhenReadyToTrack(s,s.track,args);return true;}return false;};s.gfid=function(){var s=this,d='0123456789ABCDEF',k='s_fid',fid=s.c_r(k),h='',l='',i,j,m=8,n=4,e=new Date,y;if(!fid||fid.indexO" + "f('-')<0){for(i=0;i<16;i++){j=Math.floor(Math.random()*m);h+=d.substring(j,j+1);j=Math.floor(Math.random()*n);l+=d.substring(j,j+1);m=n=16}fid=h+'-'+l;}y=e.getYear();e.setYear(y+2+(y<1900?1900:0));" + "if(!s.c_w(k,fid,e))fid=0;return fid};s.track=s.t=function(vo,setVariables){var s=this,notReadyToTrack,trk=1,tm=new Date,sed=Math&&Math.random?Math.floor(Math.random()*10000000000000):tm.getTime(),s" + "ess='s'+Math.floor(tm.getTime()/10800000)%10+sed,y=tm.getYear(),vt=tm.getDate()+'/'+tm.getMonth()+'/'+(y<1900?y+1900:y)+' '+tm.getHours()+':'+tm.getMinutes()+':'+tm.getSeconds()+' '+tm.getDay()+' '" + "+tm.getTimezoneOffset(),tcf,tfs=s.gtfs(),ta=-1,q='',qs='',code='',vb=new Object;if (s.visitor) {if (s.visitor.getAuthState) {s.authState = s.visitor.getAuthState();}if ((!s.supplementalDataID) && (" + "s.visitor.getSupplementalDataID)) {s.supplementalDataID = s.visitor.getSupplementalDataID(\"AppMeasurement:\" + s._in,(s.expectSupplementalData ? false : true));}}if(s.mpc('t',arguments))return;s.g" + "l(s.vl_g);s.uns();s.m_ll();notReadyToTrack = s._handleNotReadyToTrack(vo);if (!notReadyToTrack) {if (setVariables) {s.voa(setVariables);}if(!s.td){var tl=tfs.location,a,o,i,x='',c='',v='',p='',bw='" + "',bh='',j='1.0',k=s.c_w('s_cc','true',0)?'Y':'N',hp='',ct='',pn=0,ps;if(String&&String.prototype){j='1.1';if(j.match){j='1.2';if(tm.setUTCDate){j='1.3';if(s.isie&&s.ismac&&s.apv>=5)j='1.4';if(pn.to" + "Precision){j='1.5';a=new Array;if(a.forEach){j='1.6';i=0;o=new Object;tcf=new Function('o','var e,i=0;try{i=new Iterator(o)}catch(e){}return i');i=tcf(o);if(i&&i.next){j='1.7';if(a.reduce){j='1.8';" + "if(j.trim){j='1.8.1';if(Date.parse){j='1.8.2';if(Object.create)j='1.8.5'}}}}}}}}}if(s.apv>=4)x=screen.width+'x'+screen.height;if(s.isns||s.isopera){if(s.apv>=3){v=s.n.javaEnabled()?'Y':'N';if(s.apv" + ">=4){c=screen.pixelDepth;bw=s.wd.innerWidth;bh=s.wd.innerHeight}}s.pl=s.n.plugins}else if(s.isie){if(s.apv>=4){v=s.n.javaEnabled()?'Y':'N';c=screen.colorDepth;if(s.apv>=5){bw=s.d.documentElement.of" + "fsetWidth;bh=s.d.documentElement.offsetHeight;if(!s.ismac&&s.b){tcf=new Function('s','tl','var e,hp=0;try{s.b.addBehavior(\"#default#homePage\");hp=s.b.isHomePage(tl)?\"Y\":\"N\"}catch(e){}return h" + "p');hp=tcf(s,tl);tcf=new Function('s','var e,ct=0;try{s.b.addBehavior(\"#default#clientCaps\");ct=s.b.connectionType}catch(e){}return ct');ct=tcf(s)}}}else r=''}if(s.pl)while(pn<s.pl.length&&pn<30)" + "{ps=s.fl(s.pl[pn].name,100)+';';if(p.indexOf(ps)<0)p+=ps;pn++}s.resolution=x;s.colorDepth=c;s.javascriptVersion=j;s.javaEnabled=v;s.cookiesEnabled=k;s.browserWidth=bw;s.browserHeight=bh;s.connectio" + "nType=ct;s.homepage=hp;s.plugins=p;s.td=1}if(vo){s.vob(vb);s.voa(vo)}if(!s.analyticsVisitorID&&!s.marketingCloudVisitorID)s.fid=s.gfid();if((vo&&vo._t)||!s.m_m('d')){if(s.usePlugins)s.doPlugins(s);" + "if(!s.abort){var l=s.wd.location,r=tfs.document.referrer;if(!s.pageURL)s.pageURL=l.href?l.href:l;if(!s.referrer&&!s._1_referrer)s.referrer=r;s._1_referrer=1;s.m_m('g');if(s.lnk||s.eo){var o=s.eo?s." + "eo:s.lnk,p=s.pageName,w=1,t=s.ot(o),n=s.oid(o),x=o.s_oidt,h,l,i,oc;if(s.eo&&o==s.eo){while(o&&!n&&t!='BODY'){o=o.parentElement?o.parentElement:o.parentNode;if(o){t=s.ot(o);n=s.oid(o);x=o.s_oidt}}if" + "(!n||t=='BODY')o='';if(o){oc=o.onclick?''+o.onclick:'';if((oc.indexOf('s_gs(')>=0&&oc.indexOf('.s_oc(')<0)||oc.indexOf('.tl(')>=0)o=0}}if(o){if(n)ta=o.target;h=s.oh(o);i=h.indexOf('?');h=s.linkLeav" + "eQueryString||i<0?h:h.substring(0,i);l=s.linkName;t=s.linkType?s.linkType.toLowerCase():s.lt(h);if(t&&(h||l)){s.pe='lnk_'+(t=='d'||t=='e'?t:'o');s.pev1=(h?s.ape(h):'');s.pev2=(l?s.ape(l):'')}else t" + "rk=0;if(s.trackInlineStats){if(!p){p=s.pageURL;w=0}t=s.ot(o);i=o.sourceIndex;if(o.dataset&&o.dataset.sObjectId){s.wd.s_objectID=o.dataset.sObjectId;}else if(o.getAttribute&&o.getAttribute('data-s-o" + "bject-id')){s.wd.s_objectID=o.getAttribute('data-s-object-id');}else if(s.useForcedLinkTracking){s.wd.s_objectID='';oc=o.onclick?''+o.onclick:'';if(oc){var ocb=oc.indexOf('s_objectID'),oce,ocq,ocx;" + "if(ocb>=0){ocb+=10;while(ocb<oc.length&&(\"= \\t\\r\\n\").indexOf(oc.charAt(ocb))>=0)ocb++;if(ocb<oc.length){oce=ocb;ocq=ocx=0;while(oce<oc.length&&(oc.charAt(oce)!=';'||ocq)){if(ocq){if(oc.charAt(" + "oce)==ocq&&!ocx)ocq=0;else if(oc.charAt(oce)==\"\\\\\")ocx=!ocx;else ocx=0;}else{ocq=oc.charAt(oce);if(ocq!='\"'&&ocq!=\"'\")ocq=0}oce++;}oc=oc.substring(ocb,oce);if(oc){o.s_soid=new Function('s','" + "var e;try{s.wd.s_objectID='+oc+'}catch(e){}');o.s_soid(s)}}}}}if(s.gg('objectID')){n=s.gg('objectID');x=1;i=1}if(p&&n&&t)qs='&pid='+s.ape(s.fl(p,255))+(w?'&pidt='+w:'')+'&oid='+s.ape(s.fl(n,100))+(" + "x?'&oidt='+x:'')+'&ot='+s.ape(t)+(i?'&oi='+i:'')}}else trk=0}if(trk||qs){s.sampled=s.vs(sed);if(trk){if(s.sampled)code=s.mr(sess,(vt?'&t='+s.ape(vt):'')+s.hav()+q+(qs?qs:s.rq()),0,ta);qs='';s.m_m('" + "t');if(s.p_r)s.p_r();s.referrer=s.lightProfileID=s.retrieveLightProfiles=s.deleteLightProfiles=''}s.sq(qs)}}}else s.dl(vo);if(vo)s.voa(vb,1);}s.abort=0;s.supplementalDataID=s.pageURLRest=s.lnk=s.eo" + "=s.linkName=s.linkType=s.wd.s_objectID=s.ppu=s.pe=s.pev1=s.pev2=s.pev3='';if(s.pg)s.wd.s_lnk=s.wd.s_eo=s.wd.s_linkName=s.wd.s_linkType='';return code};s.trackLink=s.tl=function(o,t,n,vo,f){var s=th" + "is;s.lnk=o;s.linkType=t;s.linkName=n;if(f){s.bct=o;s.bcf=f}s.t(vo)};s.trackLight=function(p,ss,i,vo){var s=this;s.lightProfileID=p;s.lightStoreForSeconds=ss;s.lightIncrementBy=i;s.t(vo)};s.setTagCo" + "ntainer=function(n){var s=this,l=s.wd.s_c_il,i,t,x,y;s.tcn=n;if(l)for(i=0;i<l.length;i++){t=l[i];if(t&&t._c=='s_l'&&t.tagContainerName==n){s.voa(t);if(t.lmq)for(i=0;i<t.lmq.length;i++){x=t.lmq[i];y" + "='m_'+x.n;if(!s[y]&&!s[y+'_c']){s[y]=t[y];s[y+'_c']=t[y+'_c']}s.loadModule(x.n,x.u,x.d)}if(t.ml)for(x in t.ml)if(s[x]){y=s[x];x=t.ml[x];for(i in x)if(!Object.prototype[i]){if(typeof(x[i])!='functio" + "n'||(''+x[i]).indexOf('s_c_il')<0)y[i]=x[i]}}if(t.mmq)for(i=0;i<t.mmq.length;i++){x=t.mmq[i];if(s[x.m]){y=s[x.m];if(y[x.f]&&typeof(y[x.f])=='function'){if(x.a)y[x.f].apply(y,x.a);else y[x.f].apply(" + "y)}}}if(t.tq)for(i=0;i<t.tq.length;i++)s.t(t.tq[i]);t.s=s;return}}};s.wd=window;s.ssl=(s.wd.location.protocol.toLowerCase().indexOf('https')>=0);s.d=document;s.b=s.d.body;if(s.d.getElementsByTagNam" + "e){s.h=s.d.getElementsByTagName('HEAD');if(s.h)s.h=s.h[0]}s.n=navigator;s.u=s.n.userAgent;s.ns6=s.u.indexOf('Netscape6/');var apn=s.n.appName,v=s.n.appVersion,ie=v.indexOf('MSIE '),o=s.u.indexOf('O" + "pera '),i;if(v.indexOf('Opera')>=0||o>0)apn='Opera';s.isie=(apn=='Microsoft Internet Explorer');s.isns=(apn=='Netscape');s.isopera=(apn=='Opera');s.ismac=(s.u.indexOf('Mac')>=0);if(o>0)s.apv=parseF" + "loat(s.u.substring(o+6));else if(ie>0){s.apv=parseInt(i=v.substring(ie+5));if(s.apv>3)s.apv=parseFloat(i)}else if(s.ns6>0)s.apv=parseFloat(s.u.substring(s.ns6+10));else s.apv=parseFloat(v);s.em=0;i" + "f(s.em.toPrecision)s.em=3;else if(String.fromCharCode){i=escape(String.fromCharCode(256)).toUpperCase();s.em=(i=='%C4%80'?2:(i=='%U0100'?1:0))}if(s.oun)s.sa(s.oun);s.sa(un);s.vl_l='supplementalData" + "ID,timestamp,dynamicVariablePrefix,visitorID,marketingCloudVisitorID,analyticsVisitorID,audienceManagerLocationHint,fid,vmk,visitorMigrationKey,visitorMigrationServer,visitorMigrationServerSecure,p" + "pu,charSet,visitorNamespace,cookieDomainPeriods,cookieLifetime,pageName,pageURL,referrer,contextData,currencyCode,lightProfileID,lightStoreForSeconds,lightIncrementBy,retrieveLightProfiles,deleteLi" + "ghtProfiles,retrieveLightData';s.va_l=s.sp(s.vl_l,',');s.vl_mr=s.vl_m='timestamp,charSet,visitorNamespace,cookieDomainPeriods,cookieLifetime,contextData,lightProfileID,lightStoreForSeconds,lightInc" + "rementBy';s.vl_t=s.vl_l+',variableProvider,channel,server,pageType,transactionID,purchaseID,campaign,state,zip,events,events2,products,audienceManagerBlob,authState,linkName,linkType';var n;for(n=1" + ";n<=75;n++){s.vl_t+=',prop'+n+',eVar'+n;s.vl_m+=',prop'+n+',eVar'+n}for(n=1;n<=5;n++)s.vl_t+=',hier'+n;for(n=1;n<=3;n++)s.vl_t+=',list'+n;s.va_m=s.sp(s.vl_m,',');s.vl_l2=',tnt,pe,pev1,pev2,pev3,res" + "olution,colorDepth,javascriptVersion,javaEnabled,cookiesEnabled,browserWidth,browserHeight,connectionType,homepage,pageURLRest,plugins';s.vl_t+=s.vl_l2;s.va_t=s.sp(s.vl_t,',');s.vl_g=s.vl_t+',track" + "ingServer,trackingServerSecure,trackingServerBase,fpCookieDomainPeriods,disableBufferedRequests,mobile,visitorSampling,visitorSamplingGroup,dynamicAccountSelection,dynamicAccountList,dynamicAccount" + "Match,trackDownloadLinks,trackExternalLinks,trackInlineStats,linkLeaveQueryString,linkDownloadFileTypes,linkExternalFilters,linkInternalFilters,linkTrackVars,linkTrackEvents,linkNames,lnk,eo,lightT" + "rackVars,_1_referrer,un';s.va_g=s.sp(s.vl_g,',');s.pg=pg;s.gl(s.vl_g);s.contextData=new Object;s.retrieveLightData=new Object;if(!ss)s.wds();if(pg){s.wd.s_co=function(o){return o};s.wd.s_gs=functio" + "n(un){s_gi(un,1,1).t()};s.wd.s_dc=function(un){s_gi(un,1).t()}}",
		    w =
		    window,
		    l = w.s_c_il,
		    n =
		    navigator,
		    u = n.userAgent,
		    v = n.appVersion,
		    e = v.indexOf('MSIE '),
		    m = u.indexOf('Netscape6/'),
		    a,
		    i,
		    j,
		    x,
		    s;
        if (un) {
            un = un.toLowerCase();
            if (l)
                for (j = 0; j < 2; j++)
                    for (i = 0; i < l.length; i++) {
                        s = l[i];
                        x = s._c;
                        if ((!x || x == 's_c' || (j > 0 && x == 's_l')) && (s.oun == un || (s.fs && s.sa && s.fs(s.oun, un)))) {
                            if (s.sa)
                                s.sa(un);
                            if (x == 's_c')
                                return s;
                        } else
                            s = 0;
                    }
        }
        w.s_an = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
        w.s_sp = new Function("x", "d", "var a=new Array,i=0,j;if(x){if(x.split)a=x.split(d);else if(!d)for(i=0;i<x.length;i++)a[a.length]=x.substring(i,i+1);else while(i>=0){j=x.indexOf(d,i);a[a.length]=x.subst" + "ring(i,j<0?x.length:j);i=j;if(i>=0)i+=d.length}}return a");
        w.s_jn = new Function("a", "d", "var x='',i,j=a.length;if(a&&j>0){x=a[0];if(j>1){if(a.join)x=a.join(d);else for(i=1;i<j;i++)x+=d+a[i]}}return x");
        w.s_rep = new Function("x", "o", "n", "return s_jn(s_sp(x,o),n)");
        w.s_d = new Function("x", "var t='`^@$#',l=s_an,l2=new Object,x2,d,b=0,k,i=x.lastIndexOf('~~'),j,v,w;if(i>0){d=x.substring(0,i);x=x.substring(i+2);l=s_sp(l,'');for(i=0;i<62;i++)l2[l[i]]=i;t=s_sp(t,'');d" + "=s_sp(d,'~');i=0;while(i<5){v=0;if(x.indexOf(t[i])>=0) {x2=s_sp(x,t[i]);for(j=1;j<x2.length;j++){k=x2[j].substring(0,1);w=t[i]+k;if(k!=' '){v=1;w=d[b+l2[k]]}x2[j]=w+x2[j].substring(1)}}if(v)x=s_jn(" + "x2,'');else{w=t[i]+' ';if(x.indexOf(w)>=0)x=s_rep(x,w,t[i]);i++;b+=62}}}return x");
        w.s_fe = new Function("c", "return s_rep(s_rep(s_rep(c,'\\\\','\\\\\\\\'),'\"','\\\\\"'),\"\\n\",\"\\\\n\")");
        w.s_fa = new Function("f", "var s=f.indexOf('(')+1,e=f.indexOf(')'),a='',c;while(s>=0&&s<e){c=f.substring(s,s+1);if(c==',')a+='\",\"';else if((\"\\n\\r\\t \").indexOf(c)<0)a+=c;s++}return a?'\"'+a+'\"':" + "a");
        w.s_ft = new Function("c", "c+='';var s,e,o,a,d,q,f,h,x;s=c.indexOf('=function(');while(s>=0){s++;d=1;q='';x=0;f=c.substring(s);a=s_fa(f);e=o=c.indexOf('{',s);e++;while(d>0){h=c.substring(e,e+1);if(q){i" + "f(h==q&&!x)q='';if(h=='\\\\')x=x?0:1;else x=0}else{if(h=='\"'||h==\"'\")q=h;if(h=='{')d++;if(h=='}')d--}if(d>0)e++}c=c.substring(0,s)+'new Function('+(a?a+',':'')+'\"'+s_fe(c.substring(o+1,e))+'\")" + "'+c.substring(e+1);s=c.indexOf('=function(')}return c;");
        c = s_d(c);
        if (e > 0) {
            a = parseInt(i = v.substring(e + 5));
            if (a > 3)
                a = parseFloat(i);
        } else if (m > 0)
            a = parseFloat(u.substring(m + 10));
        else
            a = parseFloat(v);
        if (a < 5 || v.indexOf('Opera') >= 0 || u.indexOf('Opera') >= 0)
            c = s_ft(c);
        if (!s) {
            s = new Object;
            if (!w.s_c_in) {
                w.s_c_il = new Array;
                w.s_c_in = 0;
            }
            s._il = w.s_c_il;
            s._in = w.s_c_in;
            s._il[s._in] = s;
            w.s_c_in++;
        }
        s._c = 's_c';
        (new Function("s", "un", "pg", "ss", c))(s, un, pg, ss);
        return s;
    }
}