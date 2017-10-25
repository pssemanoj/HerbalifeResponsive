function OnFRZipCodeKeyPress(e, ctrl) {
    var keynum;
    var keychar;
    //alert(ctrl.value.length);
    if (window.event) // IE
    {
        keynum = e.keyCode;
    }
    else if (e.which) // Netscape/Firefox/Opera
    {
        keynum = e.which;
    }
    keychar = String.fromCharCode(keynum);
    // take only numbers
    if (!/^ *[0-9]+ *$/.test(keychar)) {
        e.cancelBubble = true;
        if (window.event) // IE
        {
            e.keyCode = 0;
        }
        else if (e.which) {
            e.which = 0;
        }
    }
}
