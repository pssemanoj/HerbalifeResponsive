$(document).ready(function (event) {

    // Add viewport content tag, to fix Zoom in and Zoom out when click on button
    $('meta[name="viewport"]').attr('content', 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0');

    $('.menu-responsive-toggle').click(function (e) {
        e.preventDefault();
        $(this).parent().find('#ProductMenu').slideToggle();
    });

    // === Account Icon
    $('#accountIcon').live("click", function () {
        toggleUserInfo(this);
    });

    // === Toggle Minicart
    $('#cartOverlay, .cart-icon.clone').live("click", function () {
        var shown = $('.shown').attr('id');

        if (typeof (shown) == 'undefined') { return false;}

        if (shown.indexOf('minicart') > -1) {
            toggleMiniCart($('#CartIcon'));
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

        // === Hide Minicart
        var minicart = $('#gdo-right-column-minicart');
        minicart.css({
            right: parseInt(minicart.css('right'), 10) == 0 ? "-75%" : 0
        }).show();

    }


    $(window).resize(function () {
        $('#TB_ajaxContent .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));
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
                $(_this).animate({ right: '75%' });
                $(miniCartIcon).animate({ right: '75%' });

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

    $("#myProfileDropDown a[href='/Authentication/Logout']").clone().appendTo('#UserInfo .LogoutLink').addClass('neutral');
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