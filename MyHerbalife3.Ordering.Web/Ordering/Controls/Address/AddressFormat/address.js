function OnZipCodeKeyPress(e, ctrl) {
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

function OnDoubleByteCharacterCheck(e, ctrl)
{
    var code;
    var finalLength;
  
    var length = ctrl.maxLength;
    finalLength = length;
    for (i = 0; i <= length-1; i++)
    {
        code = ctrl.value.charCodeAt(i);
        // check for double byte
        if (code > 255)
        {
            length -= 1;
            finalLength = length;
        }

    }
    ctrl.value = ctrl.value.substr(0, finalLength);
}

    function OnWeightStringValueCheck(e, ctrl) {
        var weightString;
        ctrl.validity.valid = true;
        weightString = encodeURI(ctrl.value).split(/%..|./).length - 1;
        if (weightString > 60)
            ctrl.validity.valid = false;
        else
            ctrl.validity.valid = true;
        //return false;//alert("string too long");
    }


function OnFieldKeyPress(e, control) {
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

    if (keynum != 0) {
        keychar = String.fromCharCode(keynum);
        if (/^ *[<]+ *$/.test(keychar) || /^ *[>]+ *$/.test(keychar)) {
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
    else if ($(control).val().trim().length > 0) {
        $(control).val($(control).val().trim().replace(/</g, "").replace(/>/g, ""));
    }
}

