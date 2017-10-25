// --------------------------------------------------------------------------------------------------------------------
// <copyright file="he-IL.js" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Hebre - Israel script for address management control.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

var ctrlPressed = false;
var activeLanguage = '';
var activeLanguageDetected = false;
// On load function
$(function () {
    // Control key management.
    $(window).keydown(function (evt) {
        if (evt.which == 17) {
            ctrlPressed = true;
        }
    }).keyup(function (evt) {
        if (evt.which == 17) {
            ctrlPressed = false;
        }
    });

    // Pick up name text box CO1.
    $('.inputPickupName').keypress(function (event) {
        CheckControl(this);
        OnKeyPress(event, this);
    });
    $('.inputPickupName').blur(function (event) {
        CheckControl(this);
        OnKeyPress(event, this);
    });
});

// Initialize key press, for CO1 screen.
function CheckControl(control) {
    // Since is the only control that needs filtering, reset when is empty.
    if ($(control).val().trim().length == 0) {
        ResetLanguageSettings();
    }
}

// Check the validation group, to restart the filter.
function CheckControls(selector) {
    var reset = true;
    $.each($(selector), function () {
        if (this.value.trim().length != 0) {
            reset = false;
        }
    });

    // If all the fields are empty and user start to typing again.
    if (reset && $(selector).length > 0) {
        ResetLanguageSettings();
    }

    // Check the nick name field exisitence.
    if ($('.addressNicknameControl').length > 0) {
        // If it does not have already the events.
        if (!$('.addressNicknameControl').keyPressHandlerExist('OnKeyPress')) {
            $('.addressNicknameControl').addClass('hValidationGroup');
            $('.addressNicknameControl').keypress(function (event) {
                OnKeyPress(event, this);
            });
            $('.addressNicknameControl').blur(function (event) {
                OnKeyPress(event, this);
            });
        }
    }
}

// Reset language settings.
function ResetLanguageSettings() {
    activeLanguage = '';
    activeLanguageDetected = false;
}

// On key/On blur event.
function OnKeyPress(event, control) {
    // Reset restriction message.
    ResetRestrictoinMessage();

    // Check the validation group content.
    CheckControls('.hValidationGroup');

    // Variables, used to manage IE and FF cases.
    var Key = '';
    var KeyCode = 0;

    // If browser is FireFox.
    if (navigator.appName == "Netscape") {
        Key = String.fromCharCode(event.charCode);
        KeyCode = event.charCode;
    }
    else {
        Key = String.fromCharCode(event.keyCode);
        KeyCode = event.keyCode;
    }

    // If Control is empty.
    if ($(control).val().trim().length == 0 && KeyCode != 0 && !activeLanguageDetected) {
        activeLanguage = DetectLanguage(Key);
        activeLanguageDetected = true;
    }
    else if ($(control).val().trim().length > 0 && !activeLanguageDetected) {
        activeLanguage = DetectLanguage($(control).val().trim().substring(0, 1));
        activeLanguageDetected = true;
    }

    /* Execute regex filters.
    * We are passing global variable activeLanguage as a parameter,
    * instead of use it as global-inside, for future reusable purpouses.
    */
    SetActiveLanguage(Key, event, control, activeLanguage);
}

// Restrict to active language characters.
function SetActiveLanguage(Key, event, control, language) {
    if (!ctrlPressed || Key.toLowerCase() != 'v') {
        // Validating actual key pressed.
        var valid = true;
        var filter = GetNegativeLanguageRegex(language);
        if (filter && filter.test(Key)) {
            valid = false;
            SetRestrictionMessage(language);
        }

        // When user uses paste functionallity, onkeypress event is not fired so replace not allowed characters.
        var oLength = $(control).val().length;
        $(control).val($(control).val().replace(GetNegativeLanguageRegex(language), ''));
        // If something was replaced, show the error message.
        if (oLength != $(control).val().length) {
            SetRestrictionMessage(language);
        }

        // If the Key is invalid cancel the flow.
        if (event && !valid) {
            if (event.preventDefault) {
                event.preventDefault();
            }
            else {
                event.returnValue = false;
            }
        }
    }
}

// Get the language negative regular expression.
// Let's work with negative regex, so it's more easy to restrict the rest of the characters.
function GetNegativeLanguageRegex(language) {
    var regex;
    switch (language) {
        case 'hebrew':
            regex = /[a-zA-Z\u0400-\u04ff]+/;
            break;

        case 'english':
            regex = /[\u0590-\u05FF\u0400-\u04ff]+/;
            break;

        case 'rusian':
            regex = /[a-zA-Z\u0590-\u05FF]+/;
            break;
    }
    return regex;
}

// Get the language regular expression.
function GetLanguageRegex(language) {
    var regex;
    switch (language) {
        case 'hebrew':
            regex = /[\u0590-\u05FF]+/;
            break;

        case 'english':
            regex = /[a-zA-Z]+/;
            break;

        case 'rusian':
            regex = /[\u0400-\u04ff]+/;
            break;
    }
    return regex;
}

// Detect language.
function DetectLanguage(Key) {
    return GetLanguageRegex('rusian').test(Key) ? 'rusian' : GetLanguageRegex('hebrew').test(Key) ? 'hebrew' : 'english';
}

// Reset the restriction message.
function ResetRestrictoinMessage() {
    if ($('.languageRestrictionError').length > 0) {
        $('.languageRestrictionError').html('');
    }
}

// Set the restriction message
function SetRestrictionMessage(language) {
    if ($('.languageRestrictionError').length > 0) {
        var message;
        switch (language) {
            case '':
                message = language;
                break;
            case 'hebrew':
                message = $('.hebrewErrorMessage').html();
                break;
            case 'english':
                message = $('.englishErrorMessage').html();
                break;
            case 'rusian':
                message = $('.rusianErrorMessage').html();
                break;
        }
        $('.languageRestrictionError').html(message);
    }
}