// --------------------------------------------------------------------------------------------------------------------
// <copyright file="jquery.hPlugins.js" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Herbalife jquery plugins.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

(function ($) {
    // Switch two classes for each element.
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
    },

    // String.Format equivalent from c#.
    String.prototype.format = String.prototype.f = function () {
        var s = this,
            i = arguments.length;

        while (i--) {
            s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
        }
        return s;
    },
    
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
    };
})(jQuery);