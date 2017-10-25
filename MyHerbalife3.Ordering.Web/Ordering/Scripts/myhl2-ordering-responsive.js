$(document).ready(function (event) {

    // Add viewport content tag, to fix Zoom in and Zoom out when click on button
    $('meta[name="viewport"]').attr('content', 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0');

    // === Account Icon
    $('#accountIcon').live("click", function () {
        toggleUserInfo(this);
    });

    // === Toggle Minicart/Left menu
    $('#cartOverlay, .cart-icon.clone').live("click", function () {
        var shown = $('.shown').attr('id');

        if (typeof (shown) == 'undefined') { return false;}

        if (shown.indexOf('minicart') > -1) {
            toggleMiniCart($('#CartIcon'));
        } else {
            toggleLeftMenu($('#leftMenu'));
        }
    });
    // === User Information
    createUserInformation();
    if (
        ( isPage("pricelist") || isPage("catalog") || isPage("productdetail") ||
          isPage("searchproducts") || isPage("productsku") || isPage("savedpaymentinformation")
        ))
    {

        $('#CartIcon:not(.clone), a.cart-items').live("click", function () {
            toggleMiniCart(this);
        });
        $('#leftMenu:not(.clone)').live("click", function () {
            toggleLeftMenu(this);
        });

        // === Hide Minicart
        var minicart = $('#gdo-right-column-minicart');
        minicart.css({
            right: parseInt(minicart.css('right'), 10) == 0 ?
              -minicart.outerWidth() :
              0
        }).show();

    }

    /* Left Nav Only */
    if (isPage("orderpreferences") || isPage("savedcarts") || isPage("savedshippingaddress") ||
        isPage("shoppingcart") || isPage('orderlistview') || isPage('checkout') || isPage("createorderforpc") || 
        isPage("freightsimulation") || isPage("survey") || isPage("savedpickupcourierlocation") || isPage("confirm") || 
        isPage("donation"))
    {
        // === Left Menu
        $('#leftMenu').live("click", function () {
            toggleLeftMenu(this);
        });

    }

    $(window).resize(function () {
        $('#TB_ajaxContent .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));

        if ($("#LoginView").css("display") != "none") {

            /* Cart Icon */
            var shoppingcart = $('#gdo-right-column-minicart');
            shoppingcart.attr('style', '').removeClass('shown');
            $('#CartIcon').attr('style', 'right: 0px; z-index: 0;');

            if ($('#cartOverlay').length > 0) {
                removeElement('#cartOverlay');
            }

            /* Left Nav */
            var leftnav = $('[id$="divleft"]');
            leftnav.attr('style', '').removeClass('shown');
            $('#leftMenu').attr('style', 'left: 0px; z-index: 0;');

            if ($('#cartOverlay').length > 0) {
                removeElement('#cartOverlay');
            }
        }

        /* Recent Orders */
        $('#divGrid').css('width', ($(window).outerWidth() - $('.headcol').outerWidth()) - 10);
        startYearMonth = $("[id$='uiStartYearMonth_fix']");
        endYearMonth = $("[id$='uiEndYearMonth_fix']");
        txtSearch = $("[id$='txtSearch']");

        startYearMonth.css('width', (($(window).outerWidth() - startYearMonth.parent().parent().next().outerWidth()) - 40));
        endYearMonth.css('width', (($(window).outerWidth() - startYearMonth.parent().parent().next().outerWidth()) - 40));
        txtSearch.css('width', (($(window).outerWidth() - startYearMonth.parent().parent().next().outerWidth()) - 80));

        $('.X-icon').css('margin-left', startYearMonth.outerWidth() - 20);
    });

    /* Append Close Button Go Classic Site */
    var linkbox = $("#classicLinkBox");
    linkbox.append('<div><a class="close whiteBtn">Close</a></div>');
    linkbox.find('div .close').click(function () {
        linkbox.fadeOut("slow", function () {
            linkbox.remove();
        });
    });

});
function toggleMiniCart(_this) {
    var minicart = $('#gdo-right-column-minicart');

    if (parseInt(minicart.css('right')) >= 0 && ! minicart.hasClass('shown') ) {
        minicart.css({
            right: parseInt(minicart.css('right'), 10) == 0 ?
              -minicart.outerWidth() :
              0
        }).show();
    }

    minicart.animate({
        right: parseInt(minicart.css('right'), 10) == 0 ?
          -minicart.outerWidth() :
          0
    }, {
        start: function () {
            minicart.toggleClass('shown');
            $('#CartIcon .arrow-left').toggleClass('arrow-right');
            var cartIcon = $('#CartIcon');
            var miniCartIcon = (!$(".cart-icon.clone").length > 0) ? cartIcon.clone().addClass('clone').appendTo('body') : $(".cart-icon.clone");

            if (minicart.hasClass('shown')) {
                createOverlay();
                $(_this).animate({ right: minicart.outerWidth() });
                $(miniCartIcon).animate({ right: minicart.outerWidth() });

                minicart.css('z-index', 10001);
                cartIcon.css('z-index', 10001);
                miniCartIcon.css('z-index', 10001);
            } else {
                cartIcon.animate({ right: 0 });
                miniCartIcon.animate({ right: 0 });
            }
        },
        complete: function () {
            if (!minicart.hasClass('shown')) {
                var cartIcon = $('#CartIcon');
                var miniCartIcon = $(".cart-icon.clone");

                removeElement('#cartOverlay');
                cartIcon.css('z-index', 0);
                minicart.css('z-index', 0);
                if ($(miniCartIcon).hasClass('clone')) { $(miniCartIcon).remove() }
            }
        }
    });
}
function toggleLeftMenu(_this) {
    var leftmenu = $('[id$="divleft"]');

    if (parseInt(leftmenu.css('left')) >= 0 && !leftmenu.hasClass('shown')) {
        leftmenu.css({
            left: parseInt(leftmenu.css('left'), 10) == 0 ?
              -leftmenu.outerWidth() :
              0
        }).show();
    }

    leftmenu.animate({
        left: parseInt(leftmenu.css('left'), 10) == 0 ?
          -leftmenu.outerWidth() :
          0
    }, {
        start: function () {
            leftmenu.toggleClass('shown');

            $('#leftMenu .arrow').toggleClass('arrow-left');
            var leftMenuIcon = (!$(".left-menu.clone").length > 0) ? $("#leftMenu").clone().addClass('clone').appendTo('body') : $(".left-menu.clone");

            if (leftmenu.hasClass('shown')) {
                createOverlay();
                $(_this).animate({ left: leftmenu.outerWidth() });
                $(leftMenuIcon).animate({ left: leftmenu.outerWidth() });
                leftmenu.css('z-index', 10001);
                $(_this).css('z-index', 10001);
                leftMenuIcon.css('z-index', 10001);
                
            } else {
                $(_this).animate({ left: 0 });
                $(leftMenuIcon).animate({ left: 0 });
            }
        },
        complete: function () {
            if (!leftmenu.hasClass('shown')) {
                var leftMenuIcon = $(".left-menu.clone");

                removeElement('#cartOverlay')
                leftmenu.css('z-index', 0);
                $(_this).css('z-index', 0);
                if ($(leftMenuIcon).hasClass('clone')) { $(leftMenuIcon).remove() }
            }
        }
    });
}
function toggleUserInfo(_this) {
    var UserInfo = $('#UserInfo');

    if (parseInt(UserInfo.css('top')) >= 0 && !UserInfo.hasClass('shown')) {
        UserInfo.css({
            top: parseInt(UserInfo.css('top'), 10) == 0 ? (-UserInfo.outerHeight() -50) : 50
        }).show();
    }

    UserInfo.animate({
        top: parseInt(UserInfo.css('top'), 10) == 0 ? (-UserInfo.outerHeight() -50): 50
    }, {
        start: function () {
            UserInfo.toggleClass('shown');
            if (!UserInfo.hasClass('shown')) {
                UserInfo.animate({ top: (-UserInfo.outerHeight() - (50 * 2))
            });
            }
        }
    });
}
function createOverlay() {
    var options = {
        elementType: "div",
        id:          "cartOverlay",
        className:   "overlay",
        html:        "",
        appendTo:    '[id$="body"]'
    };
    createElement(options);
}
function createElement(options) {
    element = document.createElement(options.elementType);
    element.id = options.id;
    element.className = options.className;
    element.innerHTML = options.html;

    appendElement(element, options.appendTo);
}
function removeElement(selector) {
    $(selector).remove();
}
function appendElement(element, parentSelector) {
    var parentElement = document.querySelector(parentSelector)
    parentElement.appendChild(element);
}
function isPage(pageName) {
    return (window.location.pathname.toLowerCase().indexOf(pageName) > -1 ) ? true : false;
}
function createUserInformation() {
    
    var options = {
        elementType: "div",
        id: "UserInfo",
        className: "visible-xs",
        html:
            "<div class='FullName'></div>" +
            "<div class='TabTeam'></div>" +
            "<div class='Volume'></div>" +
            "<div class='LogoutLink col-xs-5 col-xs-offset-7'></div>",
        appendTo: '[id$="body"]'
    };
    createElement(options);
    // === Load Information
    $.get('/api/Volume/', VolumeInfoHandler);
    $.get('/api/DistributorProfile/', profileDataHandler);

    $("#DistributorInfoTable .LogoutLink").clone().appendTo('#UserInfo .LogoutLink').addClass('neutral');
    // === Hide User Info
    var userInfo = $('#UserInfo');
    userInfo.css({
        top: parseInt(userInfo.css('top'), 10) == 0 ? 0 : -userInfo.outerHeight() - ( 46 * 2)
    }).show();
}
function VolumeInfoHandler(data) {
    if ($.isArray(data)) {
        for (var i = 0; i < data.length; i++) {
            if (data[i].IsCurrentMonth) {
                data[i] = cleanHeaderText(data[i]);
                $("#UserInfo .Volume").text(data[i].HeaderText);
            }
        }
    }
}
function cleanHeaderText(volumeObject) {
    if (volumeObject && volumeObject.HeaderText) {
        volumeObject.PlainHeaderText = volumeObject.HeaderText.replace(/\<[^>]+\>/, ' ');
    }
    return volumeObject;
}
function profileDataHandler(data) {
    if (data) {
        $("#UserInfo .FullName").text(data.FullNamePattern);
        $("#UserInfo .TabTeam").text(data.Salutation);
    }
}