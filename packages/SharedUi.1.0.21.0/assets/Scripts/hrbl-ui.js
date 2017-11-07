// --------------------------------------------------------------------------------------------------------------------
// <copyright file="hrbl-ui.js" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Global UI Javascript modules, used in MyHerbalife.com and Herbalife.com
//   Framwork: Require JS, jQuery JS and Kendo UI
// </summary>
// <authors>
// DTS - Content DEV UI | DTS - Integration 
// </authors>
// --------------------------------------------------------------------------------------------------------------------


//********* Init Require () 
requirejs.config({
    baseUrl: "/SharedUI/Scripts",
    paths: {
        rootJS: "../../Scripts",
        asWidgetManager: "//video.herbalife.com/ws/embed/video_player/",
    },
    shim: {
        "homePage": ["kendo.custom.min", "data-view-models-1.0.0"],
        "data-view-models-1.0.0": ["kendo.custom.min"],
        "../../BizWorks/Scripts/bizworks": ["kendo.custom.min", "reveal"],
        "video": ["asWidgetManager"]
    },
    waitSeconds: 60
});

require([
    "kendo.custom.min",
    "data-view-models-1.0.0",
    "omniture-scode"
], start);

//Other JS that doesn't need dependencies
require([
    "Myherbalife",
    "TealeafSDKConfig",
    "video"
]);


//************ splash login   

function splash() {

    flowplayer.conf = {
        swf: "../Media/flowplayer.commercial-3.2.14.swf"
    };

    $('#_PinManagmentLink').click(function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        window.open(url, "", "height=470,width=500,left=160,top=100,resizable=yes,toolbar=no,scrollbars=yes,menubar=yes,location=no,directories=no,status=yes");
    });
}

//************ splash login


//********* Init Start ()
// Init() all modules dependent on jQuery and KendoUI
// Any and all jQuery and KendoUI modules MUST be placed in start() Scope

function start() {

    $(document).ready(function () {

        //*** data api view models
        var dataViewModels = new DataViewModels();
        dataViewModels.Init();

        //*** Omniture
        var omniture = new OmnitureModule();
        omniture.Init();

        //*** splash page
        if ($(".splash").length != 0) {
            require([
                "flowplayer.min"
            ], splash);
        }

        //*** Left Menu fixes
        if ($("div[id$='LeftNavMenuPanel']").children().length == 0) $("#LeftNavMenuCell").remove();

        try {
            var leftMenuVar = ".leftMenuNew";
            var leftNav = $(leftMenuVar);

            //Getting page name
            var pathname = decodeURI($(location).attr('pathname') + $(location).attr('search')).toLowerCase();

            //Searching and assigning active class
            var item = $(leftMenuVar + " a").filter(function () {
                link = $(this).attr("href").toLowerCase();
                if ($("#ProductMenu").length > 0) link = "/ordering/" + link;
                return link == pathname;
            });
            if (leftNav && item) item.parents('li').addClass("active");
        } catch (err) {

        }

        //*** Login form rules
        $("#username").keypress(function (e) {
            if (e.keyCode == 8 || e.keyCode == 46) return;
            var input = String.fromCharCode(e.which);
            var regex = /[a-zA-Z0-9_]/;
            if (!regex.test(input)) {
                if (!e.shiftKey && input != ".") {
                    switch (e.keyCode) {
                        case 8://backspace
                        case 9://tab
                        case 35://end
                        case 36://home
                        case 37://left arrow
                        case 38://up arrow
                        case 39://right arrow
                        case 40://down arrow
                        case 46://delete
                            return;
                    }
                }
                e.preventDefault();
            }
        });

        //*** Placeholders for I8 and IE9
        var testElement = document.createElement('input');
        if (!('placeholder' in testElement)) {
            $("input[placeholder]").each(function () {
                if ($(this).attr("type") == "password")
                    $(this).before('<label for="' + $(this).attr("id") + '" class="placeholder">' + $(this).attr('placeholder') + '</label>');
                else
                    $(this).val($(this).attr('placeholder'));
            });

            $("input[placeholder]").focusin(function () {
                if ($(this).attr("type") == "password") {
                    if ($(this).val() == "") $('label[for="' + $(this).attr("id") + '"]').hide();
                }
                else
                    if ($(this).val() == $(this).attr('placeholder')) $(this).val('');
            });

            $("input[placeholder]").blur(function () {
                if ($(this).val() == '') {
                    if ($(this).attr("type") == "password")
                        $('label[for="' + $(this).attr("id") + '"]').show();
                    else
                        $(this).val($(this).attr('placeholder'));
                }
            });
        }

        //*** Search box
        $("#search_txt").keypress(function (event) {

            if (event.keyCode == 13) {
                if ($("#search_txt").val() == "") return false;
                else {
                    event.preventDefault();
                    window.location.href = './SiteSearch.aspx?sp_q=' + $("#search_txt").val();
                }
            }
        });

        //*** Top Nav
        var openTopNav = false;
        var itemTopNav = "";
        var topNav = $("#topNav").kendoMenu({
            animation: {
                open: { effects: "fadeIn", duration: 0 },
                close: { effects: "fadeout", duration: 200 }
            },
            openOnClick: true,
            hoverDelay: 0,
            open: function (e) {
                itemTopNav = e.item;
                openTopNav = true;
                topNav.clicked = false;
            },
            close: function (e) {
                if (openTopNav && e.item == itemTopNav) e.preventDefault();
            }
        }).data("kendoMenu");

        $("#topNav > li").click(function (e) {
            if (openTopNav == true && e.currentTarget != itemTopNav)
                itemTopNav = e.currentTarget; topNav.open();
            if ($(e.target).closest("#topNav ul").length <= 0)
                e.preventDefault();
        });

        //*** Right Top Nav
        var openRightTopNav = false;
        var itemRightTopNav = "";
        var RightTopNav = $("#rightTopNav > ul").kendoMenu({
            animation: {
                open: { effects: "fadeIn", duration: 0 },
                close: { effects: "fadeout", duration: 200 }
            },
            openOnClick: true,
            hoverDelay: 0,
            open: function (e) {
                itemRightTopNav = e.item;
                openRightTopNav = true;
                RightTopNav.clicked = false;
                if (e.item && e.item.id == "cartInfoDropDown") {
                    dataViewModels.cartViewModel.load();
                }
            },
            close: function (e) {
                if (openRightTopNav && e.item == itemRightTopNav) e.preventDefault();
                else if (e.item && e.item.id == "cartInfoDropDown") {
                    dataViewModels.cartViewModel.reset();
                }
            }
        }).data("kendoMenu");

        $("#rightTopNav > ul > li").click(function (e) {
            if (openRightTopNav == true && e.currentTarget != itemRightTopNav)
                itemRightTopNav = e.currentTarget; topNav.open();
            if ($(e.target).closest("#rightTopNav > ul ul").length <= 0)
                e.preventDefault();
        });

        $("html").click(function (e) {
            //Close Top Nav
            if (openTopNav == true && $(e.target).closest("#topNav").length <= 0) {
                itemTopNav = "";
                openTopNav = false;
                topNav.close();
            }

            //Close Right Top Nav
            if (openRightTopNav == true && $(e.target).closest("#rightTopNav > ul").length <= 0) {
                itemRightTopNav = "";
                openRightTopNav = false;
                RightTopNav.close();
            }
        });

        adaptMenu();
        $(window).resize(function () {
            adaptMenu();
        });

        //*** Local Selector
        $("#closeLocalSel").click(function () {
            $("#localeSel").click();
        });

        $("#localeSel").click(function (e) {
            $(this).toggleClass("selected");
            if ($(".icon-chevron-down", this).length) $(".icon-chevron-down", this).toggleClass("icon-chevron-up");
            else $(".icon-chevron-up", this).toggleClass("icon-chevron-down");

            if ($("#locales").is(":visible")) $("#locales").slideUp(400);
            else $("#locales").slideDown(400);

            e.stopImmediatePropagation();
        });

        var localeSelectorChangeHandler = function (e) {
            var sender = e.target;
            var locale = sender.options[sender.selectedIndex].value;

            if (locale && locale.length == 5) {
                window.location.href = '/LocaleChange.ashx?locale=' + locale;
            }
        };

        $("select.localeSelector").kendoDropDownList({
            height: 500
        });
        $("select.localeSelector").change(localeSelectorChangeHandler);

        //*** Nav position
        if ($(".topNav").length != 0) {
            var navPos = $(".topNav").offset();
            $(window).scroll(function () {
                //code for fixed navigation        
                if (($(this).scrollTop() > navPos.top) && !$(".topNav").hasClass('fixed')) {
                    $(".topNav").addClass('fixed');
                } else if ($(this).scrollTop() <= navPos.top && $(".topNav").hasClass('fixed')) {
                    $(".topNav").removeClass('fixed');
                }
            });
        }

        //*** Buttons for K-grid
        if ($(".k-grid").length) {
            //Add record buttons on k-grid
            $(".k-grid-add").removeClass("k-button");
            $(".k-grid-add").addClass("btn"); //styling of "Add Shipping Address" button
        }

        //*** modal window
        $(".hrblModal").each(function () {
            var w = $(this).width(),
            title = $("h4", $(this)).text();

            $(this).kendoWindow({
                modal: true,
                resizable: false,
                visible: false,
                width: w,
                title: title
            });
        });

        $(".k-window .k-icon").addClass("icon-x").text("");

        $('.modalBtn[title]').live("mouseenter", function () {
            $(this).data("title", $(this).attr("title")).removeAttr("title");
        });

        $('.modalBtn').live("click", function (e) {
            e.preventDefault();
            $("#" + $(this).data("title") + ".hrblModal").data("kendoWindow").open().center();
        });

        //*** Alert Messages
        if ($("#systemAlert")) {

            var numPar = $('#systemAlert div').length;
            var actualDiv = $('#systemAlert div:first-child');
            var currentPar = 1;

            $('#systemAlert span b').html(currentPar + ' of ' + numPar);

            $('#systemAlert .next').click(function () {
                if (currentPar >= numPar) {
                    currentPar = numPar;
                } else {
                    currentPar++;
                    var nextDiv = actualDiv.next();
                    actualDiv.fadeOut(200, function () { nextDiv.fadeIn(200); });
                    actualDiv = nextDiv;
                    $('#systemAlert span b').html(currentPar + ' of ' + numPar);
                }
            });

            $('#systemAlert .prev').click(function () {
                if (currentPar <= 1) {
                    currentPar = 1;
                } else {
                    currentPar--;
                    var nextDiv = actualDiv.prev();
                    actualDiv.fadeOut(200, function () { nextDiv.fadeIn(400); });
                    actualDiv = nextDiv;
                    $('#systemAlert span b').html(currentPar + ' of ' + numPar);
                }
            });

            $('#systemAlert button').click(function () {
                $("#systemAlert p").slideToggle(600);
            });

        }

        //*** Video
        if ($(".videoWrapper").length != 0) {
            $('.videoWrapper iframe').each(function () {
                var separator = "?";
                var url = $(this).attr("src");

                if (url.indexOf("?") != -1) {
                    separator = "&";
                }
                $(this).attr("src", url + separator + "wmode=transparent");
            });
        }

        if (window && window._AnalyticsFacts_ && window._AnalyticsFacts_.Id) {
            //*** idle timeout handling
            var idleWarningWindow = '#idleLogout';
            var idleWarningContentUrl = '/ed/' + _AnalyticsFacts_.LanguageCode + '-' + _AnalyticsFacts_.CountryCode + '/pages/IdleTimeoutWarning.html';

            var logoutUrl = '/Authentication/Logout';

            try {
                var handle = $(idleWarningWindow).data("kendoWindow");
                handle.refresh({
                    url: idleWarningContentUrl
                });
                $(idleWarningWindow + " .btnForward").live("click", function () { handle.close(); });
            }catch (err) {  };

            IdleLogout.configure({
                Warn: {
                    Action: function () { handle && handle.open().center(); }
                },
                Act: {
                    Action: function() { document.location.href = logoutUrl; }
                }
            });
            // wire up resetting actions
            $(window).scroll(IdleLogout.reset);
            $(window).click(IdleLogout.reset);
            // start the timer
            IdleLogout.start();
        }
    });
}

function adaptMenu() {
    var submenu = $("#topNav .k-item .k-group");
    var val = (($("body").width() - 960) / 2) + "px";
    submenu.css("padding-left", val);
    submenu.css("padding-right", val);
}


var IdleLogout = (function () {
    var config = {
        Act: { Handle: null, Action: null, Interval: 15 * 60 * 1000 },
        Warn: { Handle: null, Action: null, Interval: 10 * 60 * 1000 }
    };

    var configure = function (cfg) {
        setIfDefined(config.Act, 'Interval', cfg.Act);
        setIfDefined(config.Act, 'Action', cfg.Act);

        setIfDefined(config.Warn, 'Interval', cfg.Warn);
        setIfDefined(config.Warn, 'Action', cfg.Warn);
    };

    var setIfDefined = function (target, prop, value) {
        if (target && value && value[prop]) {
            target[prop] = value[prop];
        }
    };

    var reset = function () {
        tryReset(config.Warn);
        tryReset(config.Act);

        // We need to review this line, is causing a print of <P> in every scroll action.
        //$('body').append('<p>reset by something ' + new Date() + '</p>');

        start();
    };

    var tryReset = function (cfg) {
        if (cfg && cfg.Handle && window) { window.clearTimeout(cfg.Handle); }

    };

    var start = function () {
        tryStart(config.Warn);
        tryStart(config.Act);
    };

    var tryStart = function (cfg) {
        if (cfg && cfg.Action && cfg.Interval) {
            cfg.Handle = window.setTimeout(cfg.Action, cfg.Interval);
        }
    };


    return {
        reset: reset,

        start: start,

        configure: function (configuration) {
            reset();
            configure(configuration);
        }
    };
})();

