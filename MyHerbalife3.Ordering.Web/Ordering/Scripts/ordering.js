$(document).ready(function (event) {
    // Add viewport content tag, to fix Zoom in and Zoom out when click on button
    $('meta[name="viewport"]').attr('content', 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0');
});

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

    // Trigger cancel behavior when click outside modal window
    $('[id$="backgroundElement"]').live('click', function () {
        if ($('.gdo-popup [id$="btnCancel"]:visible')[0] != undefined) {
            $('.gdo-popup [id$="btnCancel"]:visible')[0].click();
        }
    });
    document.onkeydown = function (evt) {
        evt = evt || window.event;
        if (evt.keyCode == 27) { // ESC
            if ($('.gdo-popup [id$="btnCancel"]:visible')[0] != undefined) {
                $('.gdo-popup [id$="btnCancel"]:visible')[0].click();
            }
        }
    };

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
            width: 560,
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
                //if (responsiveMode != true) {
                $(window).bind('scroll', function () {
                    freeze_header_cols();
                });
                //}
            }
        });
        // === /Freeze Pricelist Header Cols


    }

    $('.br select[id$="ddlCardType"]').live('change', function () {
        var _this = this, src = "";
        if ($(_this).val() == "Selecione") {
            $("img[id$='ccBranding']").hide();
            return;
        }
        
        src = "Images/payment/" + $(_this).val() + ".jpg";
        $("img[id$='ccBranding']").attr("src", src);
        $("img[id$='ccBranding']").show();
    });

    $('#showSizeChartLink').live("click", function(e) {
        e.preventDefault();

        var _this = $(this);
        var _chartTable = $('[id$="sizeChartTable"]');

        if (_chartTable.hasClass('opened')) {

            _this.find('.arrow').toggleClass('up');
            _chartTable.slideUp().removeClass('opened');
            if (_this.hasClass('grandpa')) { eraseCookie('showcat'); }

        } else {

            _this.find('.arrow').toggleClass('up');
            _chartTable.slideDown().addClass('opened');

        }
    });

    // Workaround to add footer-links class
    $('#hrblSiteWrapper > footer > nav').attr('class', 'footer-links');

    $('.grandpa').click(function (e) {
        
        if (is_page("productdetail") || is_page("catalog")) { return false; }

        var _this = this;
        var ck = $(_this).attr('ck');

        if (ck == "showcat")
        {
            eraseCookie('showapp');
            createCookie(ck, 'true', 7)
        }
        else if (ck == "showapp")
        {
            eraseCookie('showcat');
            createCookie(ck, 'true', 7)
        }
    });

    // Dirty fix to hide top Menu on responsive mode
    if ($.cookie("RENDERING_LOCALE") == "he_IL") {
        $("#mobile-main-nav").remove();
    }

    $(".toggler-abbr").live("click", function () {

        if ($(".togglable-abbr").is(":visible")) {
            $(".togglable-abbr").slideUp(400, "swing", function () {
                $("[id$='txtStreet']").focus();
            });
        } else {
            $(".togglable-abbr").slideDown();
        }
        
    });

    //*** Placeholders for I8 and IE9
    var testElement = document.createElement('input');
    if (!('placeholder' in testElement)) {
        $("input[placeholder], textarea[placeholder]").each(function () {
            if ($(this).attr("type") == "password")
                $(this).before('<label for="' + $(this).attr("id") + '" class="placeholder">' + $(this).attr('placeholder') + '</label>');
            else
                $(this).val($(this).attr('placeholder'));
        });

        $("input[placeholder], textarea[placeholder]").focusin(function () {
            if ($(this).attr("type") == "password") {
                if ($(this).val() == "") $('label[for="' + $(this).attr("id") + '"]').hide();
            }
            else
                if ($(this).val() == $(this).attr('placeholder')) $(this).val('');
        });

        $("input[placeholder], textarea[placeholder]").blur(function () {
            if ($(this).val() == '') {
                if ($(this).attr("type") == "password")
                    $('label[for="' + $(this).attr("id") + '"]').show();
                else
                    $(this).val($(this).attr('placeholder'));
            }
        });

        $("[type=password]").bind('propertychange change', function () {
            if ($("[type=password]").val() == '')
                $('label[for="pin"]').show();
            else
                $('label[for="pin"]').hide();
        });
    }

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
    var help_date;

    if ($("#ctl00_ctl00_PageHeaderArea_hTargetDate").length > 0) {
        help_date = document.getElementById("ctl00_ctl00_PageHeaderArea_hTargetDate").value;
    } else {

        if ($("#PageHeaderArea_hTargetDate").length > 0) {
            help_date = document.getElementById("PageHeaderArea_hTargetDate").value;
        }

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
    eraseCookie('BNES_DUPCHECKWITHREFID_STATUS'); /* Fix per defects 175887, 175969 */

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
    var pathname = window.location.pathname;

    if (is_page("productdetail") || is_page("catalog")) {

        $('.toggle-child').hide();
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
        if (document.cookie.indexOf("showapp") >= 0) {

            $('[id$="toggleMeApparel"]')
                .show()
                .addClass('opened');

            $('[id$="toggleMeApparel"]')
                .prev()
                .find(".arrow")
                .toggleClass("up icon-minus-ln-1");


        }
        // Keep sub cat opened
        setTimeout(keepSubcatOpened, 250);

        $('.toggle-parent.grandpa').click(function (e) {

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
        $('.toggle-parent .arrow').click(function (e) {

            var _this = $(this);
            var _child = $(_this).parent().next('.toggle-child');
            var _opened = _this.parent().parent().find('.opened');

            if (_this.parent().hasClass('grandpa')) { return; }

            // Close opened list
            if (_opened.length && !_child.is(_opened) && !_this.parent().hasClass('grandpa')) {

                _opened.prev().find('.arrow').toggleClass('up icon-minus-ln-1');
                _opened.removeClass('opened').slideUp();

    }

            if (_child.hasClass('opened')) {

                _this.parent().find('.arrow').toggleClass('up icon-minus-ln-1');
                _child.slideUp().removeClass('opened');

                if (_this.parent().hasClass('grandpa') && _this.parent().hasClass('catalog')) { eraseCookie('showcat'); }
                else if (_this.parent().hasClass('grandpa') && _this.parent().hasClass('apparel')) { eraseCookie('showapp'); }

            } else {

                _this.parent().find('.arrow').toggleClass('up icon-minus-ln-1');
                _child.slideDown().addClass('opened');

                if (_this.parent().hasClass('grandpa') && _this.parent().hasClass('catalog')) { createCookie('showcat', 'true', 7); }
                else if (_this.parent().hasClass('grandpa') && _this.parent().hasClass('apparel')) { createCookie('showapp', 'true', 7); }
}

        });
    }
}
function keepSubcatOpened() {
    if ($('li.toggle-parent.active').find('.icon.arrow').length > 0) {
        $('li.toggle-parent.active:not(.grandpa)').find('.icon.arrow').click();
}
    if ($('li.active:not(.toggle-parent)').length > 0 && $('li.active:not(.toggle-parent)').parent().hasClass("opened") == false) {
        $('li.active').parent().prev().find('.icon.arrow').click();
    }
}
function RemovePopup(event) {
    $('#ctl00_ctl00_ContentArea_uchrblMessageBoxControl2_popup_MessageBox_backgroundElement').hide();
    $('#ctl00_ctl00_ContentArea_uchrblMessageBoxControl2_pnlMessageBox').hide();
    $("div[id$='MessageBox_backgroundElement']").hide();
    $("div[id$='pnlMessageBox']").hide();
}

$(document).mouseup(function (e) {
    var container = $("div[id$='pnlMessageBox']");

    if (!container.is(e.target) // if the target of the click isn't the container...
        && container.has(e.target).length === 0) // ... nor a descendant of the container
    {
        container.hide();
        $("div[id$='_popup_MessageBox_backgroundElement']").hide();
    }
});
/* Fix per defect 129848 */
$(document).ready(function () {
    //only numbers for inputs use class "onlyNumbers"
    $('body').on('input keydown keyup', '.onlyNumbers', function (e) {
        regex = /[^0-9]/g;
        if (this.value.match(regex)) {
            this.value = this.value.replace(regex, '');
        }
    });
});

$(document).ready(function () {
    cronometro();

    var cookieLocal = $.cookie("RENDERING_LOCALE");
    if (cookieLocal == "zh_CN") {
        $("a[name='checkoutButton']").live('click', function () {
            $("div[id$='updateProgressDiv']").css({ "background-color": "gray", "opacity": "0.75" });
        });
    } else if ((cookieLocal == "ru_BY") || (cookieLocal == "ru_KZ")) {
        $('input[type=radio]').live('click', function () {
            if ($(this).val() == 2) {
                $("div.checkout-logos").css({ "display": "inherit" });
            }
            else {
                $("div.checkout-logos").css({ "display": "none" });
            }
        });

        /* Task 205859 */
        if (cookieLocal == "ru_BY") {
            setInterval(function () {
                $("input[id$='chkAcknowledgeTransaction']").attr('disabled', true);
                $("div[id$='pnlAcknowledCheckContent']").css({ "display": "none" });
            }, 100);

        }

    }
});

/* Fix per defect 170995 */
function cleaningTable() {
    if ($.browser.msie && $.browser.version == '9.0') {
        tableHtml = $('.gdo-edit-summary table.gdo-order-tbl:not(.childSKUTable)').html();
        var expr = new RegExp('>[ \t\r\n\v\f]*<', 'g');
        if (typeof (tableHtml) != 'undefined') {
            tableHtml = tableHtml.replace(expr, '><');
            $('.gdo-edit-summary table.gdo-order-tbl:not(.childSKUTable)').html(tableHtml);
        }
    }
}

/* Fix per defect 117926 */
function hidePromoPopup() {
    $(document).ready(function () {
        var cookieLocal = $.cookie("RENDERING_LOCALE");

        if ((cookieLocal == "en_US") || (cookieLocal == "es_US") || (cookieLocal == "es_PR") || (cookieLocal == "en_CA") || (cookieLocal == "fr_CA")) {
            if (document.cookie.indexOf("valPopup") >= 0) {
                // Cookie still exist    
                $("div[id$='MessageBoxControl2_pnlMessageBox']").css({ "display": "none" });
                $("div[id$='uchrblMessageBoxControl2_popup_MessageBox_backgroundElement']").css({ "display": "none" });
                eraseCookie('valPopup');
            }
        }
    });
}


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
                qty = (skus[1] != "") ? skus[1] : 0; // QTY

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
function freeze_header_cols() {
    var scrollTop = $(window).scrollTop();
    // === Freeze Pricelist Header Cols
    var colProdW = $('.freeze-header').next().next().find('.col-Product').outerWidth();
    $('.freeze-header .col-Product').css('width', colProdW);

    /* === VP === */
    if (responsiveMode != true) {
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

        if (responsiveMode == true) {
            $actions
                .addClass('topSticky')
                .css({ 'top': 50 });

            $actions
                .children()
                .first()
                .css({ 'padding': 0, 'background-color': '#fff' });
        }

    } else if (distanceActionsNext > actionsH) {

        $actions.removeClass('topSticky');

    }

    if (responsiveMode == true) { return; }

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
function is_page(pageName) {
    return (window.location.pathname.toLowerCase().indexOf(pageName) > -1) ? true : false;
}
function repositionModal(element, atTop) {

    atTop = typeof atTop !== 'undefined' && atTop != false ? true : false;

    if (element.length < 0) { return false; }

    // Set Top Position
    if (atTop == true) {
        element.css('top', $(window).scrollTop() + 20)
    } else {
        var top = (($(window).outerHeight() / 2) - (element.outerHeight() / 2) + $(window).scrollTop());

        top = (top > 20) ? top: 20;
        element.css('top', top);
    }

    //Set Center Position
    var left = (($(window).outerWidth() - element.outerWidth()) / 2) + $(window).scrollLeft();
    element.css('left', left);

}
function repositionProductDetailModal(element) {

    if (element.length < 0) { return false; }

    // Set Top Position
    element.css('top', $(window).scrollTop() + 20);

    //Set Center Position
    var left = ((($(window).outerWidth() - element.outerWidth()) / 2) + $(window).scrollLeft());

    // Check if element it's greater than main container
    if (($(window).scrollLeft() + element.outerWidth()) > $(".modalBackground:visible").outerWidth()) {
        left = $(".modalBackground:visible").outerWidth() -element.outerWidth();
    }
    // Avoid that modal has negative position, in order to be fully visible
    if (left < 0 && responsiveMode != true) { left = 0; }
    element.css('left', left);

    element.parent().parent().css('position', 'static');

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
                qty = (skus[1] != "") ? skus[1] : 0; // QTY

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
function freeze_header_cols() {
    var scrollTop = $(window).scrollTop();
    // === Freeze Pricelist Header Cols
    var colProdW = $('.freeze-header').next().next().find('.col-Product').outerWidth();
    $('.freeze-header .col-Product').css('width', colProdW);

    /* === VP === */
    if (responsiveMode != true) {
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

        if (responsiveMode == true) {
            $actions
                .addClass('topSticky')
                .css({ 'top': 50 });

            $actions
                .children()
                .first()
                .css({ 'padding': 0, 'background-color': '#fff' });
        }

    } else if (distanceActionsNext > actionsH) {

        $actions.removeClass('topSticky');

    }

    if (responsiveMode == true) { return; }

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
function is_page(pageName) {
    return (window.location.pathname.toLowerCase().indexOf(pageName) > -1) ? true : false;
}

/* Task 206456 */
function getSKUBackOrder(element, e) {
    // Get the link id and the sku value
    var linkClicked = element.id;
    var sku = $(element).attr('sku');
    var UID = $(element).attr('uid');
    var skuCategory = $(element).attr('ctg');

    if ($("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").hasClass('hide')) {
        var cookieLocal = $.cookie("RENDERING_LOCALE").replace("_", "-");

        $.ajax({
            type: 'POST',
            url: "PriceList.aspx/GetSKUBackOrderDetails",
            data: "{'sku':'" + sku + "', 'locale':'" + cookieLocal + "'}",
            cache: true,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
                onSuccess(data, sku, UID, skuCategory, linkClicked);
            },
            error: function (data, success, error) {
                console.log("Error : " + error)
            }
        });

        $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").slideDown("fast", function () {
            $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").removeClass("hide");
            $("#" + linkClicked).text(hideAvailabilityText);
        });
    } else {

        $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").slideUp("fast", function () {
            $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").addClass("hide");
            if (showAvailabilityText == "") showAvailabilityText = "Show Availability";
            $("#" + linkClicked).text(showAvailabilityText);
        });
    }

    return false;
}

function onSuccess(data, sku, UID, skuCategory, linkClicked) {

    var skuBackOrder = JSON.parse(data.d);

    // Draw the table
    $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\"] tr:not('.tr-head')").remove();

    for (var i = 0; i < skuBackOrder.InventoryDetails.length; i++) {
        drawRow(skuBackOrder.InventoryDetails[i], sku, skuCategory, UID);
    }

    //Footer
    var footer = $("<tr class='inventLink' />");
    $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").append(footer);
    footer.append($("<td colspan='5'>For a full list of product availability, please <a href='/Ordering/inventory'>click here</a></td>"));
    footer.append($("<td><a onclick='javascrip:hideMe(\"" + sku + "\",\"" + UID + "\",\"" + skuCategory + "\",\"" + linkClicked + "\")'>" + hideAvailabilityText + "</a></td>"));
}

function drawRow(rowData, sku, skuCategory, UID) {
    var row = $("<tr />");
    $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").append(row);

    row.append($("<td colspan='2'>" + rowData.Location + " </td>"));
    row.append($("<td>" + rowData.Type + "</td>"));
    if (rowData.Status == "Unavailable") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_red.gif' /></td>"));
    } else if (rowData.Status == "Available") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_green.gif' /></td>"));
    } else if (rowData.Status == "AllowBackOrder") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_yellow.gif' /></td>"));
    } else if (rowData.Status == "UnavailableInPrimaryWh") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_blue.gif' /></td>"));
    } else {
        row.append($("<td>" + rowData.Status + "</td>"));
    }
    row.append($("<td>" + rowData.AvailabilityDate + "</td>"));
    row.append($("<td colspan='2'>" + rowData.Comments + "</td>"));
}

function hideMe(sku, UID, skuCategory, linkClicked) {

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

        if (responsiveMode == true) {
            $actions
                .addClass('topSticky')
                .css({ 'top': 50 });

            $actions
                .children()
                .first()
                .css({ 'padding': 0, 'background-color': '#fff' });
        }

    } else if (distanceActionsNext > actionsH) {

        $actions.removeClass('topSticky');

    }

    if (responsiveMode == true) { return; }

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
function is_page(pageName) {
    return (window.location.pathname.toLowerCase().indexOf(pageName) > -1) ? true : false;
}

/* Task 206456 */
function getSKUBackOrder(element, e) {
    // Get the link id and the sku value
    var linkClicked = element.id;
    var sku = $(element).attr('sku');
    var UID = $(element).attr('uid');
    var skuCategory = $(element).attr('ctg');

    if ($("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").hasClass('hide')) {
        var cookieLocal = $.cookie("RENDERING_LOCALE").replace("_", "-");

        $.ajax({
            type: 'POST',
            url: "PriceList.aspx/GetSKUBackOrderDetails",
            data: "{'sku':'" + sku + "', 'locale':'" + cookieLocal + "'}",
            cache: true,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
                onSuccess(data, sku, UID, skuCategory, linkClicked);
            },
            error: function (data, success, error) {
                console.log("Error : " + error)
            }
        });

        $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").slideDown("fast", function () {
            $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").removeClass("hide");
            $("#" + linkClicked).text(hideAvailabilityText);
        });
    } else {

        $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").slideUp("fast", function () {
            $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").addClass("hide");
            $("#" + linkClicked).text(showAvailabilityText);
        });
    }

    return false;
}

function onSuccess(data, sku, UID, skuCategory, linkClicked) {

    var skuBackOrder = JSON.parse(data.d);

    // Draw the table
    $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\"] tr:not('.tr-head')").remove();

    for (var i = 0; i < skuBackOrder.InventoryDetails.length; i++) {
        drawRow(skuBackOrder.InventoryDetails[i], sku, skuCategory, UID);
    }

    //Footer
    var footer = $("<tr class='inventLink' />");
    $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").append(footer);
    footer.append($("<td colspan='5'>" + linkInstructionsText + " <a href='/Ordering/inventory' target='_blank'>" + anchorText  + "</a></td>"));
    footer.append($("<td><a onclick='javascrip:hideMe(\"" + sku + "\",\"" + UID + "\",\"" + skuCategory + "\",\"" + linkClicked + "\")'>" + hideAvailabilityText + "</a></td>"));
}

function drawRow(rowData, sku, skuCategory, UID) {
    var row = $("<tr />");
    $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").append(row);

    row.append($("<td colspan='2'>" + rowData.Location + " </td>"));
    row.append($("<td>" + rowData.Type + "</td>"));
    if (rowData.Status == "Unavailable") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_red.gif' /></td>"));
    } else if (rowData.Status == "Available") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_green.gif' /></td>"));
    } else if (rowData.Status == "AllowBackOrder") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_orange.gif' /></td>"));
    } else if (rowData.Status == "UnavailableInPrimaryWh") {
        row.append($("<td><img src='/Content/Global/Products/Img/circle_blue.gif' /></td>"));
    } else {
        row.append($("<td>" + rowData.Status + "</td>"));
    }
    row.append($("<td>" + rowData.AvailabilityDate + "</td>"));
    row.append($("<td colspan='2'>" + rowData.Comments + "</td>"));
}

function hideMe(sku, UID, skuCategory, linkClicked) {

    if (!$("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").hasClass('hide')) {
        $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").slideUp("fast", function () {
            $("#BackOrderTable" + sku + "[uid=\"" + UID + "\"][ctg=\"" + skuCategory + "\" ]").addClass("hide");
            $("#" + linkClicked).text(showAvailabilityText);
        });
    }
}
function scroll_to_element(elementSelector, duration) {
    $('html, body').animate({
        scrollTop: $(elementSelector).offset().top
    }, duration);
}

function targetPageParams() {
    return AdobeTarget;
}

function foldDaumPostcode() {
    kendo.jQuery('#wrapDaum').css('display', 'none');
}

function execDaumPostcode() {
    
    var currentScroll = Math.max(document.body.scrollTop, document.documentElement.scrollTop);
    var element_wrap = document.getElementById('wrapDaum');
    new daum.Postcode({
        oncomplete: function(data) {
            // Popup in part to write code to execute when you click on a search result item.

            // The combination of the address in accordance with the exposure rules for each address.
            // If the variable is not coming down the value of yen because different spaces ( '') values, and branch to see it.
            var fullAddr = ''; // Final address parameters
            var extraAddr = ''; //Combination address variable

            // User brings the address value according to the selected address type.
            if (data.userSelectedType === 'R') { // If the user selects the street address
                fullAddr = data.roadAddress;

            } else { // If the user selects the address, parcel number (J)
                fullAddr = data.jibunAddress;
            }

            // The address type when the user selects Street and combinations.
            if(data.userSelectedType === 'R'){
                // If you add beopjeongdong people.
                if(data.bname !== ''){
                    extraAddr += data.bname;
                }
                // If there is a building name is added.
                if(data.buildingName !== ''){
                    extraAddr += (extraAddr !== '' ? ', ' + data.buildingName : data.buildingName);
                }
                // Make a final address by adding the brackets on each side, depending on the presence or absence of a combined address.
                fullAddr += (extraAddr !== '' ? ' ('+ extraAddr +')' : '');
            }

            // Put your zip code and address information into the appropriate fields.
            kendo.jQuery("input[id$='txtAddress1']").val("" + data.sido); // try

            kendo.jQuery("input[id$='txtAddress2']").val("" + data.sigungu + " " + data.roadAddress); // Ward County Town + hibernate	
               
            kendo.jQuery("input[id$='txtPostalCode1']").val("" +data.zonecode); // Use new 5-digit ZIP Code			

            // Move the cursor to the address field detail.
            kendo.jQuery("input[id$='txtAddress3']").focus();

            element_wrap.style.display = 'none';

            document.body.scrollTop = currentScroll;

        },
        onresize: function (size) {
            
            element_wrap.style.height = size.height + 'px';
        },
        width: '100%',
        height: '100%'
    }).embed(element_wrap);
    kendo.jQuery('#wrapDaum').css('display', 'block');
    
}

function formatPostalCode(postcode) {
    const MaxPostCodeLength = 7;
    const FirstPartLength = 3;
    var zeroPreseed;
    var postCode = postcode.value.replace("-","");
    var currentPostCodeLength = postCode.toString().length;
    if (currentPostCodeLength < MaxPostCodeLength) {
        var diff = MaxPostCodeLength - currentPostCodeLength;

        for (var i = 0; i < diff - 1; i++) {
            if (zeroPreseed == null) zeroPreseed = '0';
            else zeroPreseed += '0';
        }
    }

    if (zeroPreseed != null) zeroPreseed += postCode;
    else zeroPreseed = postCode;

    postCode = zeroPreseed;

    if ((MaxPostCodeLength - postCode.toString().length) > 0) {
        for (var i = 0; i < (MaxPostCodeLength - postCode.toString().length) ; i++) {
            postCode = '0' + postCode.toString();
        }
    }
    postcode.value = postCode.toString().substring(0, FirstPartLength) + '-' + postCode.toString().substring(FirstPartLength);
}
function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode != 46 && charCode > 31
      && (charCode < 48 || charCode > 57))
        return false;

    return true;
}
