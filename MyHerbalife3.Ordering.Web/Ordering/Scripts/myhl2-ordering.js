$(document).ready(function () {

    var oldTxtCP1 = '';
    var oldTxtCP2 = '';
    var searchingText = '';

    $(document).keydown(function (e) {
        var element = (e != null) ? e.target.nodeName.toLowerCase() : 'null';
        var elementType = (e != null) ? e.target.type : 'null';

        if (element != 'input' && element != 'textarea' && elementType != 'text') {
            if (e.keyCode === 8) {
                return false;
            }
        }
    });

    if ($("ul.skin2").length != 0) ("ul.skin2").tabs("div.skin2 > div");

    // HFF Modal =========================================
    $HFFModal = {
        message: null,
        init: function () {

            $("#modal-container").modal({
                closeHTML: "<a href='#' title='Close' class='modal-close'>x</a>",
                position: ["15%", ],
                overlayId: 'modal-overlay',
                containerId: 'modal-container',
                onOpen: $HFFModal.open,
                onShow: $HFFModal.show,
                onClose: $HFFModal.close
            });

        },
        open: function (dialog) {

            var title = $('#modal-container .modal-title').html();
            $('#modal-container .modal-title').html('Loading...');

            dialog.overlay.fadeIn(200, function () {
                dialog.container.fadeIn(200, function () {
                    dialog.data.fadeIn(200, function () {

                        var h = $('#modal-container .modal-innerContent').height();
                        ;

                        $('#modal-container .modal-content').animate(
                            {
                                height: h
                            }, function () {
                                $('#modal-container .modal-title').html(title);
                                $('#modal-container .modal-innerContent').fadeIn(200, function () {

                                }); // $('#contact-container form').fadeIn(200, function () 
                            });
                    });
                });
            });
        },
        show: function (dialog) {
        },
        close: function (dialog) {
            $('#modal-container .modal-innerContent').fadeOut();
            $('#modal-container .modal-title').html('Thank you...');
            $('#modal-container .modal-content').animate(
                {
                    height: 40
                }, function () {
                    dialog.data.fadeOut(200, function () {
                        dialog.container.fadeOut(200, function () {
                            dialog.overlay.fadeOut(200, function () {
                                $.modal.close();
                            }); // dialog.overlay.fadeOut(200, function () 
                        }); // dialog.container.fadeOut(200, function () 
                    }); // dialog.data.fadeOut(200, function () 
                });
        }, // Close =============
        error: function (xhr) {
            alert(xhr.statusText);
        },
        showError: function () {
            $('#modal-container .error-message')
                .html($('<div class="contact-error"></div>').append(contact.message))
                .fadeIn(200);
        }
    };
    // End HFF Modal =========================================      

    if ($("#zoomIntoProduct").length > 0) {
        $("#zoomIntoProduct").kendoWindow();
        var dialog = $("#zoomIntoProduct").data("kendoWindow");
        dialog.setOptions({
            width: 500,
        });
    }
    if ($("#printThisPage").length > 0) {
        $("#printThisPage").kendoWindow();
        var dialog = $("#printThisPage").data("kendoWindow");
        dialog.setOptions({
            height: ($(window).outerHeight() - ($("#printThisPage").prev().outerHeight() + 50)),
            width: 700
        });
    }

    $(".btnPrintThisPage").click(function (e) {
        e.preventDefault();
        if ($("#printThisPage").length > 0) {
            $("#printThisPage").kendoWindow().data("kendoWindow").close();
        }
    });

    if ($.cookie("RENDERING_LOCALE") == "zh_CN") {

        // === China fix, Simulate Yes click if user clicks on overlay
        $('[id$="UpdatePanelTermCondition"] [id$="backgroundElement"]').live("click", function () {
            $('div[id$="PnlTermCondition"] a[id$="btnTermConditionYes"]')[0].click();
        });

        // === Freeze Pricelist Header Cols
        $(window).bind('scroll', function () {
            if (is_page("pricelist")) {
                if (responsiveMode != true) {
                    $(window).bind('scroll', function () {
                        freeze_header_cols();
                    });
                }
            }
        });
        // === /Freeze Pricelist Header Cols

        if (responsiveMode != true) {

            $(".tabs-SKU input[id$='Quantity']").live('keyup', function () {

                if ($('[id$="lbVolumePoint"].totalVP').length > 0) {
                 $('[id$="lbVolumePoint"].totalVP').text(ProdDetailVPCalculation.get_total_vp());
                }
                if ($('[id$="lbTotalAmount"].totalAmount').length > 0) {
                    $('[id$="lbTotalAmount"].totalAmount').text(ProdDetailVPCalculation.get_total_ammount());
                }

            });
        }

    }


    /*Since all the tabs are hidden with css we are displaying the tab with class .active_tab using fadeIn()
    function. If you just want it to show with no effect, just put show() instead of fadeIn() */
    //$('.active_tab').fadeIn();

    $('[name$="rblPaymentOptions"]').live("change", function () {

        var _this_options = this;

        //$('.tab_link').live('click', function (event) {
        var _parent = $(_this_options).parent();

        /* ...remove the tab_link_selected class from current active link... */
        $('.tab_link_selected').removeClass('tab_link_selected');

        /* ...and add it to the new active link */
        $(_parent).addClass('tab_link_selected');

        /*...get the title attribute (which corensponds to the id of the needed text container),
        but you can use any attribute you want*/
        var container_id = $(_parent).attr('href');

        $('.active_tab').removeClass('active_tab').hide();

        $('[id$="' + container_id + '"]').addClass('active_tab').show();

        //});

    });



});

function CheckNumeric(e, ctrl) {
    var keynum;
    if (!e)
        var e = window.event
    if (e.keyCode) keynum = e.keyCode;
    else if (e.which) keynum = e.which;

    // take only numbers
    if (keynum > 57 || keynum < 48) {
        e.cancelBubble = true;
        if (e.keyCode) // IE
        {
            e.keyCode = 2;
        }
        else if (e.which) {
            if (e.preventDefault) e.preventDefault();
            if (e.stopPropagation) e.stopPropagation();

        }
    }
}

function CheckAreaPlusPhone(e, ctrlArea, ctrlPhone, totalLength) {
    var elementAreaCode = document.getElementById(ctrlArea);
    var elementPhone = document.getElementById(ctrlPhone);
    if (elementAreaCode != null && elementPhone != null) {
        if (elementAreaCode.value.length + elementPhone.value.length >= totalLength) {
            e.cancelBubble = true;
            if (window.event) {
                e.keyCode = 0;
            }
            else if (e.which) {
                e.which = 0;
            }
        }
    }
}

function PickupSelected(ctrl) {
    var items = document.getElementsByTagName('input');
    for (i = 0; i < items.length; i++) {
        if (items[i].type == "radio") {
            if (items[i].name.indexOf("rbSelected") != -1) {
                if (items[i] != ctrl) {
                    items[i].checked = false;
                }
            }
        }
    }

}

function CheckChanges(textbox, selector) {
    var changed = false;

    if (selector === '.txtPostCode2') {
        oldTxtCP2 = $(selector).val();
        if (oldTxtCP1 != $(textbox).val()) {
            changed = true;
        }
        oldTxtCP1 = $(textbox).val();
    }
    else {
        oldTxtCP1 = $(selector).val();
        if (oldTxtCP2 != $(textbox).val()) {
            changed = true;
        }
        oldTxtCP2 = $(textbox).val();
    }


    if (($(selector).val() && $(textbox).val() && changed) ||
        (!$(selector).val() && !$(textbox).val())) {
        javascript: __doPostBack(textbox.name, '');
    }
}

function AllowOnlyNumbers2(control) {
    $(control).val($(control).val().replace(/[^\d]/g, ''));
};

function AllowOnlyNumbers(event) {
    // Allow only backspace and delete
    if (event.keyCode == 46 || event.keyCode == 8) {
        // let it happen, don't do anything
    }
    else {
        // Ensure that it is a number and stop the keypress
        if (event.keyCode < 48 || event.keyCode > 57) {
            event.preventDefault();
        }
    }
};

function EnableMasterScrolling() {
    $("body").css("overflow", "auto");
};

function DisableMasterScrolling() {
    $("body").css("overflow", "hidden");
};

$.fn.switchClasses = function (toAdd, toRemove) {
    this.each(function () {
        $(this).removeClass(toRemove).addClass(toAdd);
    });
},

// If a handler exist on the keypress event.
$.fn.keyPressHandlerExist = function (handlerName) {
    var exist = false;
    this.each(function () {
        if (handlerName && $(this).data('events') && $(this).data('events').keypress) {
            $.each($(this).data('events').keypress, function (i, handler) {
                if (handler.handler.toString().indexOf(handlerName) >= 0) {
                    exist = true;
                }
            });
        }
    });
    return exist;
}

// String.Format equivalent from c#.
String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
}

// Allow only numeric characters.
$.fn.setNumeric2 = function (event) {
    this.each(function () {
        $(this).keypress(function (event) {
            var key = '';
            // If browser is FireFox.
            if (navigator.appName == "Netscape") {
                key = String.fromCharCode(event.charCode);
            }
            else {
                key = String.fromCharCode(event.keyCode);
            }

            var valid = false;
            var filter = /[0-9]+/;
            if (filter.test(key)) {
                valid = true;
            }

            var filteredValue = $(this).val();
            for (var i = $(this).val().length - 1; i >= 0; i--) {
                var ch = $(this).val().charAt(i);
                if (!filter.test(ch)) {
                    filteredValue = filteredValue.substring(0, i) + filteredValue.substring(i + 1);
                }
            }
            $(this).val(filteredValue);

            if (event && !valid) {
                if (event.preventDefault) {
                    event.preventDefault();
                }
                else {
                    event.returnValue = false;
                }
            }
        });
        $(this).blur(function (event) {
            var filter = /[0-9]+/;
            var filteredValue = $(this).val();
            for (var i = $(this).val().length - 1; i >= 0; i--) {
                var ch = $(this).val().charAt(i);
                if (!filter.test(ch)) {
                    filteredValue = filteredValue.substring(0, i) + filteredValue.substring(i + 1);
                }
            }
            $(this).val(filteredValue);
        });
    });
},

// Set numeric textbox.
$.fn.setAsNumeric = function (nDecimals) {
    this.each(function () {
        $(this).keypress(function (event) {
            var key = '';

            // If browser is FireFox.
            if (navigator.appName == "Netscape") {
                key = String.fromCharCode(event.charCode);
            } else {
                key = String.fromCharCode(event.keyCode);
            }
            var valid = true;

            var hasPoint = $(this).val().indexOf('.') >= 0;
            var hasComa = $(this).val().indexOf(',') >= 0;

            // If it has already a decimals separator.
            if ((key === '.' || key === ',') && (hasPoint || hasComa)) {
                valid = false;
            }

            if (event && !valid) {
                if (event.preventDefault) {
                    event.preventDefault();
                } else {
                    event.returnValue = false;
                }
            }

        });
    });
},

// Allow only hebrew characters.
$.fn.disableEnglish = function (event) {
    this.each(function () {
        $(this).keypress(function (event) {
            var Key = '';
            // If browser is FireFox.
            if (navigator.appName == "Netscape") {
                Key = String.fromCharCode(event.charCode);
            }
            else {
                Key = String.fromCharCode(event.keyCode);
            }

            var valid = true;
            var filter = /[a-zA-Z]+/;
            if (filter.test(Key)) {
                valid = false;
            }
            $(control).val($(control).val().replace(/[a-zA-Z]+/, ''));
            if (event && !valid) {
                if (event.preventDefault) {
                    event.preventDefault();
                }
                else {
                    event.returnValue = false;
                }
            }
        });
        $(this).blur(function (event) {
            var Key = '';
            // If browser is FireFox.
            if (navigator.appName == "Netscape") {
                Key = String.fromCharCode(event.charCode);
            }
            else {
                Key = String.fromCharCode(event.keyCode);
            }

            var valid = true;
            var filter = /[a-zA-Z]+/;
            if (filter.test(Key)) {
                valid = false;
            }
            $(control).val($(control).val().replace(/[a-zA-Z]+/, ''));
            if (event && !valid) {
                if (event.preventDefault) {
                    event.preventDefault();
                }
                else {
                    event.returnValue = false;
                }
            }
        });
    });
}

// Legacy javascript code.
function checkSKUChar(e, ctrl) {
    var keychar;

    if (!e) {
        e = window.event;
    }

    keychar = String.fromCharCode(e.keyCode || e.which);

    if (!/^ *[0-9a-zA-Z]+ *$/.test(keychar)) {
        e.cancelBubble = true;
        if (e.keyCode) // IE
        {
            e.keyCode = 2;
        }
        else if (e.which) {
            if (e.preventDefault) e.preventDefault();
            if (e.stopPropagation) e.stopPropagation();

        }
    }
}

//Timer
function cronometro() {
    var today = new Date();
    if ($("#ctl00_ctl00_PageHeaderArea_hTargetDate").length > 0) {
        var help_date = document.getElementById("ctl00_ctl00_PageHeaderArea_hTargetDate").value;
    } else {
        var help_date = document.getElementById("PageHeaderArea_hTargetDate").value;
    }


    if (help_date) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(cronometro);
        // Split help_date to obtain target_date parameters
        var YearEOM = help_date.split(' ')[0];
        var MonthEOM = help_date.split(' ')[1];
        var DayEOM = help_date.split(' ')[2];
        var HourEOM = help_date.split(' ')[3];
        var MinEOM = help_date.split(' ')[4];
        var SecEOM = help_date.split(' ')[5];

        // get tag element
        var target_date = new Date(YearEOM, MonthEOM - 1, DayEOM, HourEOM, MinEOM, SecEOM);
        //var target_date = new Date(today.getFullYear(), today.getMonth() + 1, 0, 23, 59);

        // variables for time units
        var days, hours, minutes, seconds;
        var timer;
        var counterlbl = document.getElementById("ctl00_ctl00_PageHeaderArea_counter");
        var countdown_sec = document.getElementById("countdown_sec");
        var countdown_min = document.getElementById("countdown_min");
        var countdown_hr = document.getElementById("countdown_hr");
        var countdown_dy = document.getElementById("countdown_dy");

        // Split timtest to obtain serverTime parameters        
        if ($("#ctl00_ctl00_PageHeaderArea_hcurrentDate").length > 0) {
            var timtest = document.getElementById("ctl00_ctl00_PageHeaderArea_hcurrentDate").value;
        } else {
            var timtest = document.getElementById("PageHeaderArea_hcurrentDate").value;
        }
        var YearServer = timtest.split(' ')[0];
        var MonthServer = timtest.split(' ')[1];
        var DayServer = timtest.split(' ')[2];
        var HourServer = timtest.split(' ')[3];
        var MinServer = timtest.split(' ')[4];
        var SecServer = timtest.split(' ')[5];

        var serverTime = new Date(YearServer, MonthServer - 1, DayServer, HourServer, MinServer, SecServer);
        var clientNow = new Date().getTime();
        var endTime = target_date - serverTime + clientNow;

        // update the tags with id "countdown_" every 1 second
        timer = setInterval(function () {

            // find the amount of "seconds" between now and target
            var current_date = new Date().getTime();
            var seconds_left = (endTime - new Date()) / 1000;

            // do some time calculations
            days = parseInt(seconds_left / 86400);
            seconds_left = seconds_left % 86400;

            hours = parseInt(seconds_left / 3600);
            seconds_left = seconds_left % 3600;

            minutes = parseInt(seconds_left / 60);
            seconds = parseInt(seconds_left % 60);

            if (days <= 0 && hours <= 0 && minutes <= 0 && seconds <= 0) {
                //countdown_sec.innerHTML = "00";
                countdown_min.innerHTML = "00";
                countdown_hr.innerHTML = "00";
                countdown_dy.innerHTML = "00";
                counterlbl.className = counterlbl.className + " hide";

                clearInterval(timer);
            }
            else {
                // format countdown string + set tag value

                //if (seconds < 10) {
                //    countdown_sec.innerHTML = "0" + seconds;
                //}
                //else {
                //    countdown_sec.innerHTML = seconds;
                //}
                if (minutes < 10) {
                    countdown_min.innerHTML = "0" + minutes;
                }
                else {
                    countdown_min.innerHTML = minutes;
                }
                if (hours < 10) {
                    countdown_hr.innerHTML = "0" + hours;
                }
                else {
                    countdown_hr.innerHTML = hours;
                }
                if (days < 10) {
                    countdown_dy.innerHTML = "0" + days;
                }
                else {
                    countdown_dy.innerHTML = days;
                }
            }
        }, 1000);
    }

}

/* Fix per defect 117491 */
function cookieEater() {
    eraseCookie('DUPCHECKWITHREFID_STATUS');

    if (document.cookie.indexOf("DUPCHECKWITHREFID_STATUS") >= 0) {
        // Cookie still exist    
        $(document).ready(function () {
            $.cookie('DUPCHECKWITHREFID_STATUS', null);
        });
    }

}

function createCookie(name, value, days) {
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + "; path=/";
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}

/* Hide Product Catalog*/
function toggleCatalog() {

    if (is_page("productdetail") || is_page("catalog")) {

        $('.toggle-child').hide();
        $('.toggle-child:not(.grandson)').show();
        $('li.toggle-parent:not(.grandpa) i').remove();

        $('.grandpa')
            .addClass('toggle-parent')
            .find('.arrow')
            .removeClass('hide');

        if (document.cookie.indexOf("showcat") >= 0) {

            $('[id$="toggleMe"]')
                .show()
                .addClass('opened');

            $('[id$="toggleMe"]')
                .prev()
                .find(".arrow")
                .toggleClass("up icon-minus-ln-1");

        }

        $('.toggle-parent.grandpa').click(function (e) {
            e.preventDefault();
            var _this = $(this);
            var _child = $(_this).next('.toggle-child');
            var _opened = _this.parent().find('.opened');

            // Close opened list
            if (_opened.length && !_child.is(_opened) && !_this.hasClass('grandpa')) {

                _opened.prev().find('.arrow').toggleClass('up icon-minus-ln-1');
                _opened.removeClass('opened').slideUp();

            }

            if (_child.hasClass('opened')) {

                _this.find('.arrow').toggleClass('up icon-minus-ln-1');
                _child.slideUp().removeClass('opened');

                if (_this.hasClass('grandpa') && _this.hasClass('catalog')) { eraseCookie('showcat'); }
                else if (_this.hasClass('grandpa') && _this.hasClass('apparel')) { eraseCookie('showapp'); }

            } else {

                _this.find('.arrow').toggleClass('up icon-minus-ln-1');
                _child.slideDown().addClass('opened');

                if (_this.hasClass('grandpa') && _this.hasClass('catalog')) { createCookie('showcat', 'true', 7); }
                else if (_this.hasClass('grandpa') && _this.hasClass('apparel')) { createCookie('showapp', 'true', 7); }
            }

        });
    }
}

function RemovePopup(event) {
    $('#ctl00_ctl00_ContentArea_uchrblMessageBoxControl2_popup_MessageBox_backgroundElement').hide();
    $('#ctl00_ctl00_ContentArea_uchrblMessageBoxControl2_pnlMessageBox').hide();
}

$(document).mouseup(function (e) {
    var container = $('#ctl00_ctl00_ContentArea_uchrblMessageBoxControl2_pnlMessageBox');

    if (!container.is(e.target) // if the target of the click isn't the container...
        && container.has(e.target).length === 0) // ... nor a descendant of the container
    {
        container.hide();
        $('#ctl00_ctl00_ContentArea_uchrblMessageBoxControl2_popup_MessageBox_backgroundElement').hide();
    }
});
function scroll_to_element(elementSelector, duration) {
    $('html, body').animate({
        scrollTop: $(elementSelector).offset().top
    }, duration);
}

$(document).ready(function () {
    var cookieLocal = $.cookie("RENDERING_LOCALE");
    if (cookieLocal == "zh_CN") {
        $("a[name='checkoutButton']").live('click', function () {
            $("div[id$='updateProgressDiv']").css({ "background-color": "white", "opacity": "0.75", "background-image": "url('/Ordering/Images/China/hrblpreloader.gif')" });
        });
    } else if ((cookieLocal == "ru_BY") || (cookieLocal == "ru_KZ")) {
        $('input[type=radio]').live('click', function () {
            if ($(this).val() == 3) {
                $("div.checkout-logos").css({ "display": "none" });
            }
            else {
                $("div.checkout-logos").css({ "display": "inherit" });
            }
        });
    }
});

/* Fix per defect 117926 */
function hidePromoPopup() {
    $(document).ready(function () {
        var cookieLocal = $.cookie("RENDERING_LOCALE");

        if ((cookieLocal == "en_CA") || (cookieLocal == "fr_CA")) {
            if (document.cookie.indexOf("valPopup") >= 0) {
                // Cookie still exist    
                $("div[id$='MessageBoxControl2_pnlMessageBox']").css({ "display": "none" });
                $("div[id$='uchrblMessageBoxControl2_popup_MessageBox_backgroundElement']").css({ "display": "none" });

                setTimeout(function () {
                    $("div[id$='uchrblMessageBoxControl2_popup_MessageBox_backgroundElement']").hide();
                    $("div[id$='MessageBoxControl2_pnlMessageBox']").hide();
                }, 1);

                eraseCookie('valPopup');
            }
        }
    });
}

function freeze_header_cols() {
    var scrollTop = $(window).scrollTop();
    // === Freeze Pricelist Header Cols
    var colProdW = $('.freeze-header').next().next().find('.col-Product').outerWidth();
    $('.freeze-header .col-Product').css('width', colProdW);

    /* === VP === */
    var $vp = $('.freeze-vp'),
        $vpNext = $('.freeze-vp + tr'),
        vpW = $vp.next().outerWidth(),
        vpH = $vp.next().outerHeight(),
        vpOffset = $vp.offset().top,
        vpNextOffset = $vpNext.offset().top,
        distanceVP = (vpOffset - scrollTop),
        distanceVPNext = (vpNextOffset - scrollTop);

    if (distanceVP < 0) {

        $vp.addClass('topSticky').css({ 'width': vpW, 'background-color': '#FFF' });

    } else if (distanceVPNext > vpH) {

        $vp.removeClass('topSticky');

    }

    /* === Actions === */
    var $actions = $('.freeze-actions'),
        $actionsNext = $('.freeze-actions + tr'),
        actionsW = $actions.next().outerWidth(),
        actionsH = $actions.next().outerHeight(),
        actionsOffset = $actions.offset().top,
        actionsNextOffset = $actionsNext.offset().top,
        distanceActions = (actionsOffset - scrollTop),
        distanceActionsNext = (actionsNextOffset - scrollTop);

    if (distanceActions < 0) {

        $actions
            .addClass('topSticky')
            .css({ 'width': actionsW });
        $actions
            .children()
            .first()
            .css('width', actionsW);

    } else if (distanceActionsNext > actionsH) {

        $actions.removeClass('topSticky');

    }

    /*=== Pricelist Table */
    var $header = $('.freeze-header'),
        $headerNext = $('.freeze-header + tr');
    var headerW = $header.next().outerWidth(),
        headerH = $header.next().outerHeight(),
        headerOffset = $header.offset().top,
        headerNextOffset = $headerNext.offset().top,
        distanceH = (headerOffset - scrollTop),
        distanceHNext = (headerNextOffset - scrollTop);

    if (distanceH < 0) {

        $header
            .addClass('topSticky')
            .css({ 'width': headerW, 'top': $vp.outerHeight() });

    } else if (distanceHNext > headerH) {

        $header.removeClass('topSticky');

    }
}

var VPCalculation = {
    selected_skus: [],
    get_skus: function () {

        var products = $('[id$="pricelistGridInfo"]').val();

        if (products == "") { return false; }

        var prods = '{';
        products = products.split(",");

        $.each(products, function (key, value) {

            if (value != "") {

                skus = value.split("|");
                sku = skus[0].replace("txt_", ""); // SKU Id
                qty = (skus[1] != "") ? parseInt(skus[1], 10) : 0; // QTY

                if (isNaN(qty)) { qty = 0; }

                psku = ($("tr[product-sku='" + sku + "'] .col-VolumePoint").attr('item-vp') != undefined) ?
                    '"vp" : ' + $("tr[product-sku='" + sku + "'] .col-VolumePoint").attr('item-vp') + '' :
                    "";
                comma = (psku == "") ? "" : ",";

                prods +=

                  '"' + sku + '"' + ': {' +

                    '"price" : ' + $("tr[product-sku='" + sku + "'] .col-RetailPrice").attr('list-price') + ',' +
                    '"qty" : ' + qty + comma +
                    psku +

                  '},';

            }

        });
        prods = prods.substring(0, prods.length - 1);
        prods += '}';
        this.selected_skus = JSON.parse(prods);
    },
    get_total_vp: function () {
        var totalVP = 0;
        $.each(VPCalculation.selected_skus, function (sku, item) {

            totalVP += item.vp * item.qty;

        });

        return this.format_totals(totalVP);
    },
    get_total_ammount: function () {
        var totalAmmount = 0;
        $.each(VPCalculation.selected_skus, function (sku, item) {

            totalAmmount += item.price * item.qty;

        });

        return this.format_totals(totalAmmount);
    },
    format_totals: function (total) {
        var neg = false;
        if (total < 0) {
            neg = true;
            total = Math.abs(total);
        }
        return parseFloat(total, 10).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
    }

};

var ProdDetailVPCalculation = {
    total_vp: 0,
    total_ammount: 0,

    get_total_vp: function () {
        var qty_inputs = $(".tabs-SKU input[id$='Quantity']");
        var VPs = $(".tabs-SKU span[id$='VolumePoints']");
        var qty = 0, vp = 0, totalVP = 0;

        $.each(qty_inputs, function (key, qty_input) {

            qty = ($(qty_input).val() != "") ? parseInt($(qty_input).val(), 10) : 0; // QTY
            
            if (isNaN(qty)) { qty = 0; }

            if (qty > 0) {
                vp = $(VPs[key]).text();
                totalVP += qty * vp;
            }

        });

        return this.format_totals(totalVP);
    },
    get_total_ammount: function () {
        var qty_inputs = $(".tabs-SKU input[id$='Quantity']");
        var prices = $(".tabs-SKU span[id$='Retail']");
        var qty = 0, price = 0, totalAmmount = 0;

        $.each(qty_inputs, function (key, qty_input) {

            qty = ($(qty_input).val() != "") ? parseInt($(qty_input).val(), 10) : 0; // QTY

            if (isNaN(qty)) { qty = 0; }

            if (qty > 0) {

                price = $(prices[key]).parent().attr('list-price');
                totalAmmount += qty * price;

            }

        });

        return this.format_totals(totalAmmount);
    },
    format_totals: function (total) {
        var neg = false;
        if (total < 0) {
            neg = true;
            total = Math.abs(total);
        }
        return parseFloat(total, 10).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
    }

};

function is_page(pageName) {
    return (window.location.pathname.toLowerCase().indexOf(pageName) > -1) ? true : false;
}

(function ($) {
    $.support.placeholder = ('placeholder' in document.createElement('input'));
})(jQuery);


//fix for IE7 and IE8 for Placeholders
$(function () {
    if (!$.support.placeholder) {
        $("[placeholder]").focus(function () {
            if ($(this).val() == $(this).attr("placeholder")) $(this).val("");
        }).blur(function () {
            if ($(this).val() == "") $(this).val($(this).attr("placeholder"));
        }).blur();

        $("[placeholder]").parents("form").submit(function () {
            $(this).find('[placeholder]').each(function () {
                if ($(this).val() == $(this).attr("placeholder")) {
                    $(this).val("");
                }
            });
        });
    }
});

function visibleAreaHeight() {
    jQuery('#TB_ajaxContent .gdo-popup').css(
                'max-height',
                $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.20))
            );

    jQuery(window).resize(function () {
        jQuery('#TB_ajaxContent .gdo-popup').css(
          'max-height',
          $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.20))
        );
    });

}