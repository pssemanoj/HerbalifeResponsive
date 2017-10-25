function OnAddressKeyPress(e, ctrl) {
    var elementSrc = e.srcElement;
    if (elementSrc != null) {
        if (elementSrc.value.length >= 40) {
            e.ancelBubble = true;
            if (window.event) // IE
            {
                e.keyCode = 0;
            }
            else if (e.which) {
                e.which = 0;
            }
        }
    }
}
function OnTWZipCodeKeyPress(e, ctrl) {
    var keynum;
    var keychar;

    if (window.event) // IE
    {
        keynum = e.keyCode;
    }
    else if (e.which) // Netscape/Firefox/Opera
    {
        keynum = e.which;
    }
    keychar = String.fromCharCode(keynum);

    // take only numbers and -
    if (!/^ *[0-9-]+ *$/.test(keychar)) {
        e.ancelBubble = true;
        if (window.event) // IE
        {
            e.keyCode = 0;
        }
        else if (e.which) {
            e.which = 0;
        }
    }
    var elementSrc = e.srcElement;
    if (elementSrc != null) {
        if (elementSrc.value.length >= 3) {
            e.ancelBubble = true;
            if (window.event) // IE
            {
                e.keyCode = 0;
            }
            else if (e.which) {
                e.which = 0;
            }
        }
    }
}

