var payoptionlink = null;
var tokenSuccess = false;
var tokenizedRegex = '^[0-9]{1}[a-z]|[A-Z]{1}[0-9]{6,17}$';
var tokenTimer;
var TokenTimerEnabled = false;

function SetQHPayOptions(settings) 
{
    var options = "LumpSum";
    var rowPrefix = payoptionlink.id.replace("lnkPaymentOptions", "");
    var lumpsum = document.getElementById("LumpSumQH");
    var revolving = document.getElementById("RevolvingQH");
    var installments = document.getElementById("InstallmentsQH");
    var numInstallments = document.getElementById("NumberInstallmentsQH");
    var bonus1 = document.getElementById("Bonus1");
    var bonus1Month = document.getElementById(pgPrefix + "Bonus1Month");
    var bonus2 = document.getElementById("Bonus2");
    var bonus2Month1 = document.getElementById(pgPrefix + "Bonus2Month1");
    var bonus2Month2 = document.getElementById(pgPrefix + "Bonus2Month2");
    var payOption = document.getElementById(rowPrefix + "txtOption");
    var payChoice1 = document.getElementById(rowPrefix + "txtChoice1");
    var payChoice2 = document.getElementById(rowPrefix + "txtChoice2");
    var lumpsumText = GetTextValue(document.getElementById(pgPrefix + "lbLumpSum"));
    var revolvingText = GetTextValue(document.getElementById(pgPrefix + "lbRevolving"));
    var installmentsText = GetTextValue(document.getElementById(pgPrefix + "lbInstallments"));
    var bonus1Text = GetTextValue(document.getElementById(pgPrefix + "lbBonus1"));
    var bonus2Text = GetTextValue(document.getElementById(pgPrefix + "lbBonus2"));

    if (lumpsum.checked) {
        options = lumpsumText;
        payOption.value = 1;
    }
    else if (revolving.checked) {
        options = revolvingText;
        payOption.value = 2;
    }
    else if (installments.checked) {
        options = installmentsText + ": " + numInstallments.value;
        payOption.value = 3;
        payChoice1.value = numInstallments.value;
    }
    else if (bonus1.checked) {
        options = bonus1Text + ": " + bonus1Month.value;
        payOption.value = 4;
        payChoice1.value = bonus1Month.value;
    }
    else if (bonus2.checked) {
        options = bonus2Text + ": " + bonus2Month1.value + " - " + bonus2Month2.value;
        payOption.value = 5;
        payChoice1.value = bonus2Month1.value;
        payChoice2.value = bonus2Month2.value;
    }

    SetTextValue(payoptionlink, options);
    payoptionlink = null;
    payOption = null;
    payChoice1 = null;
    payChoice2 = null;
}

function SetQHSelections()
{
    var options = "LumpSum";
    var rowPrefix = payoptionlink.id.replace("lnkPaymentOptions", "");
    var lumpsum = document.getElementById("LumpSumQH");
    var revolving = document.getElementById("RevolvingQH");
    var installments = document.getElementById("InstallmentsQH");
    var numInstallments = document.getElementById("NumberInstallmentsQH");
    var bonus1 = document.getElementById("Bonus1");
    var bonus1Month = document.getElementById(pgPrefix + "Bonus1Month");
    var bonus2 = document.getElementById("Bonus2");
    var bonus2Month1 = document.getElementById(pgPrefix + "Bonus2Month1");
    var bonus2Month2 = document.getElementById(pgPrefix + "Bonus2Month2");
    var payOption = document.getElementById(rowPrefix + "txtOption");
    var payChoice1 = document.getElementById(rowPrefix + "txtChoice1");
    var payChoice2 = document.getElementById(rowPrefix + "txtChoice2");

    switch (payOption.value) 
    {
        case "":
        case "0":
        case "1":
            {
                lumpsum.checked = true;
                break;
            }
        case "2":
            {
                revolving.checked = true;
                break;
            }
        case "3":
            {
                installments.checked = true;
                SetSelection(numInstallments, payChoice1.value);
                break;
            }
        case "4":
            {
                bonus1.checked = true;
                SetSelection(bonus1Month, payChoice1.value);
                break;
            }
        case "5":
            {
                bonus2.checked = true;
                SetSelection(bonus2Month1, payChoice1.value);
                SetSelection(bonus2Month2, payChoice2.value);
                break;
            }
    }
}

function SetALPayOptions(settings) {
    var options = "LumpSum";
    var rowPrefix = payoptionlink.id.replace("lnkPaymentOptions", "");
    var lumpsum = document.getElementById("LumpSumAL");
    var revolving = document.getElementById("RevolvingAL");
    var installments = document.getElementById("InstallmentsAL");
    var numInstallments = document.getElementById("NumberInstallmentsAL");
    var payOption = document.getElementById(rowPrefix + "txtOption");
    var payChoice1 = document.getElementById(rowPrefix + "txtChoice1");
    var payChoice2 = document.getElementById(rowPrefix + "txtChoice2");
    var lumpsumText = GetTextValue(document.getElementById(pgPrefix + "lbLumpSum"));
    var revolvingText = GetTextValue(document.getElementById(pgPrefix + "lbRevolving"));
    var installmentsText = GetTextValue(document.getElementById(pgPrefix + "lbInstallments"));

    if (lumpsum.checked) {
        options = lumpsumText;
        payOption.value = 1;
    }
    else if (revolving.checked) {
        options = revolvingText;
        payOption.value = 2;
    }
    else if (installments.checked) {
        options = installmentsText + ": " + numInstallments.value;
        payOption.value = 3;
        payChoice1.value = numInstallments.value;
    }

    SetTextValue(payoptionlink, options);
    payoptionlink = null;
    payOption = null;
    payChoice1 = null;
    payChoice2 = null;
}

function SetALSelections() {
    var options = "LumpSum";
    var rowPrefix = payoptionlink.id.replace("lnkPaymentOptions", "");
    var lumpsum = document.getElementById("LumpSumAL");
    var revolving = document.getElementById("RevolvingAL");
    var installments = document.getElementById("InstallmentsAL");
    var numInstallments = document.getElementById("NumberInstallmentsAL");
    var payOption = document.getElementById(rowPrefix + "txtOption");
    var payChoice1 = document.getElementById(rowPrefix + "txtChoice1");
    var payChoice2 = document.getElementById(rowPrefix + "txtChoice2");
    switch (payOption.value)
    {
        case "":
        case "0":
    case "1":
        {
            lumpsum.checked = true;
            break;
        }
        case "2":
        {
            revolving.checked = true;
            break;
        }
        case "3":
        {
            installments.checked = true;
            SetSelection(numInstallments, payChoice1.value);
            break;
        }
    }
}

function ShowPopup(parent)
{
    if (null != parent)
    {
        payoptionlink = GetEventSource(parent);
        var popupBehavior = $find("modPopPayOptions");

        if (null != popupBehavior)
        {
            popupBehavior.set_X(GetLeft(payoptionlink));
            popupBehavior.set_Y(GetTop(payoptionlink));
            popupBehavior.show();
        }
    }
}

function GetLeft(o)
{
    var l = 0;
    var p = o;
    for (; p != null; p = p.offsetParent)
    {
        l += p.offsetLeft;
    }
    return l;
}
function GetTop(o)
{
    var t = o.offsetHeight;
    var p = o;
    for (; p != null; p = p.offsetParent)
    {
        t += p.offsetTop + 1;
    }
    return t;
}

function ValidateRemove(item) 
{
    return window.confirm(DeleteCardQuestion);
}

function CheckNickNameForNumbers(nickName) {
    var nickNameNum = nickName.replace(/[^\d.]/g, '');
    if (nickNameNum != null && nickNameNum.length > 10) {
        return !luhn10(nickNameNum);
    }
    return true;
}

function ValidateNickName(e, item) {
    var isValid = true;
    var msg = '';
    var message = document.getElementById(prefix + "lblMessage");
    var nickName = document.getElementById(prefix + "txtNickName");
    isValid = CheckNickNameForNumbers(nickName.value);
    if (!isValid) {
        msg = validNickNameRequired;
        nickName.focus();
    }
    if (!isValid) {
        SetTextValue(message, msg);
    }
    return isValid;
}

function ValidateNewCard(e, item) 
{
    var msg = '';
    var cardList = document.getElementById(prefix + "ddlCardType");
    var cardType = cardList[cardList.selectedIndex].value;
    var cardNumber = document.getElementById(prefix + "txtCardNumber");
    var nickName = document.getElementById(prefix + "txtNickName");
    var cardName = document.getElementById(prefix + "txtCardHolderName");
    var streetAddress = document.getElementById(prefix + "txtStreetAddress");
    var city = document.getElementById(prefix + "txtCity");
    var state = document.getElementById(prefix + "txtState");
    var zip = document.getElementById(prefix + "txtZip");
    var editMode = document.getElementById(prefix + "EditMode");
    var message = document.getElementById(prefix + "lblMessage");
    var isracardlocale = document.getElementById(prefix + "hdLocale");
    var tin = document.getElementById(prefix + "txtTIN");
    TokenTimerEnabled = document.getElementById(prefix + "hdTokenTimerEnabled");
    var isTokenized = false;
    var isValid = true;
    
    //Go
    if (isValid) { 
        if (null != nickName.value && nickName.value.length > 0) {
            var nickVal = escapeRegExp(nickName.value);
            isValid = ValidateUniqueNickName(nickVal);
            if (!isValid) {
                msg = validNonDupeNickNameRequired;
                nickName.focus();
            }
            isValid = CheckNickNameForNumbers(nickName.value);
            if (!isValid) {
                msg = validNickNameRequired;
                nickName.focus();
            }
        }
    }

    if (isValid)
    {
        if (null == cardName.value || cardName.value.length == 0) 
        {
            isValid = false;
            msg = CardNameRequired;
            cardName.focus();
        }
    }

    if (isValid) 
    {
        if (null == cardList || cardList.selectedIndex <= 0) 
        {
            isValid = false;
            msg = CardTypeRequired;
            cardList.focus();
        }
    }

    if (isValid) 
    {
        if (null == cardNumber.value || cardNumber.value.length == 0) 
        {
            isValid = false;
            msg = CardNumberRequired;
            cardNumber.focus();
        }
    }

    if (isValid)
    {
        if (cardNumber.value.indexOf('XXXX') == -1)
        {
            var valRegex = GetValidationRegex(cardType);
            if (!cardNumber.value.match(valRegex) || !ValidateLuhn(cardType, cardNumber.value))
            {
                isTokenized = cardNumber.value.match(tokenizedRegex);
                if (!isTokenized)
                {
                    isValid = false;
                    msg = CardNumberInvalid;
                    cardNumber.focus();
                    cardNumber.select();
                }
            }
        }
        else
        {
            isTokenized = true;
        }
    }

    if ((isracardlocale.value == 'he-IL' || isracardlocale.value == 'ru-IL') && (cardType == 'MC') && ((cardNumber.value.length == 8) || (cardNumber.value.length == 9)))
    {
        if (!ValidateIsracard(cardType, cardNumber.value, isracardlocale.value))
        {
            isValid = false;
            msg = CardNumberInvalid;
            cardNumber.focus();
            cardNumber.select();
        }
        else
        {
            isValid = true;
        }

    }

    if (isValid && isracardlocale.value == 'ko-KR')
    {
        if (null == tin.value || tin.value.length != 6 || !(/^\d+$/.test(tin.value)))
        {
            isValid = false;
            msg = "카드소유자주민번호앞자리6자리";
            tin.focus();
            tin.select();
        }

    }

    if (isValid) 
    {
        if (!ValidateExpirationDate(cardList, prefix)) 
        {
            isValid = false;
            msg = ExpDateRequired;
        }
    }

    if (isValid) 
    {
        if (!ValidateExpirationDateIsCurrent(cardList, prefix)) 
        {
            isValid = false;
            msg = CardHasExpired;
        }
    }

    if (isValid) 
    {
        if (null != streetAddress) 
        {
            if (streetAddress.value.length == 0) 
            {
                isValid = false;
                msg = StreetAddressRequired;
                streetAddress.focus();
            }

            if (isValid) 
            {
                if (null != city && city.value.length == 0) 
                {
                    isValid = false;
                    msg = CityRequired;
                    city.focus();
                }
            }

            if (isValid)
            {

                if (null != state && state.value.length == 0) 
                {
                    isValid = false;
                    msg = StateRequired;
                    state.focus();
                }
            }

            if (isValid) 
            {
                if (null != zip && zip.value.length == 0) 
                {
                    isValid = false;
                    msg = ValidZipRequired;
                    zip.focus();
                }
            }
        }
    }

    //Tokenization
    if (isValid && !tokenizationDisabled && !isTokenized)
    {
        try
        {
            Tokenize(cardNumber.value);
        }
        catch (e)
        {
            tokenSuccess = false;
        }

        isValid = tokenSuccess;
        if (!isValid) {
            msg = TokenizationFailed;
        } else {
            if (TokenTimerEnabled) {
                tokenTimer = setTimeout(function () { TokenTimerFunction(); }, 10000);
            }
        }
    }

    if (isValid && !tokenizationDisabled)
    {
        TestNotTokenized();
    }
    
    if (!isValid) 
    {
        SetTextValue(message, msg);
    }

    return isValid;
}

function TokenTimerFunction() {
    clearTimeout(tokenTimer);
    var modalPopup = $find('ppPaymentInfoPopupBehavior');
    if(modalPopup){
        var continueButton = document.getElementById(prefix + "hdButton");
        if (continueButton) {
            continueButton.click();
        }
    }
}

function Tokenize(cardNumber)
{
    $.ajax({
        type: "POST",
        url: "/cde/tokenizer.svc/gettoken",
        data: JSON.stringify({ request: { AuthToken: authToken, CardNumber: cardNumber } }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processdata: true,
        async: false,
        success: function (msg)
        {
            ServiceSucceeded(msg);
        },
        error: ServiceFailed
    });
}

function ServiceSucceeded(result)
{
    try 
    {
        var token = result.GetTokenResult.Token;
        var failed = result.GetTokenResult.FailureReason;
        tokenSuccess = (token.length > 0 && null == failed);
        if (tokenSuccess)
        {
            maskToken();
            document.getElementById(prefix + "txtCardNumber").value = token;
        }
    }
    catch (e)
    {
        tokenSuccess = false;
    }
}

function ServiceFailed(xhr, ajaxOptions, thrownError)
{
    tokenSuccess = false;
}

function TestNotTokenized()
{
    var cardNumber = document.getElementById(prefix + "txtCardNumber");
    notTokenized = !cardNumber.value.match(tokenizedRegex);
    if (notTokenized)
    {
        isValid = false;
        msg = TokenizationFailed;
        tokenSuccess = false;
    }
}

function maskToken()
{
    try
    {
        var strCardNo = document.getElementById(prefix + "txtCardNumber").value;
        var lastChars = strCardNo.substr(strCardNo.length - 4);
        var firstChars = strCardNo.substr(0, 1);
        var newCardNo = (firstChars + "XXXXXXXXXXX" + lastChars);

        var cloned = document.getElementById(prefix + "txtCardNumber").cloneNode(false);
        cloned.id = "gerry";
        cloned.name = "gerry"
        cloned.value = newCardNo;
        document.getElementById(prefix + 'txtCardNumber').parentNode.appendChild(cloned);
        document.getElementById(prefix + 'txtCardNumber').style.display = "none"; //.style.visibility = "hidden";
    }
    catch (e)
    { }
}


function ValidateExpirationDate(card, prefix)
{
    var isValid = (card[card.selectedIndex].getAttribute('Value') == 'QH');
    if (!isValid)
    {
        var month = document.getElementById(prefix + "ddlExpMonth");
        var year = document.getElementById(prefix + "ddlExpYear");
        isValid = (month.value > '0' && year.value > '0');
        if (!isValid)
        {
            if (!month.value > '0')
            {
                month.selectedIndex = 0;
                month.focus();
            }
            if (month.value > '0' && !year.value > '0')
            {
                year.selectedIndex = 0;
                year.focus();
            }
        }
    }

    return isValid;
}

function ValidateExpirationDateIsCurrent(card, prefix)
{
    var isValid = (card[card.selectedIndex].getAttribute('Value') == 'QH');
    if (!isValid) 
    {
        var month = document.getElementById(prefix + "ddlExpMonth");
        var year = document.getElementById(prefix + "ddlExpYear");
        isValid = (new Date(2000 + parseInt(year.value, 10), parseInt(month.value, 10), 1) > new Date(new Date().getFullYear(), new Date().getMonth(), 1));
    }

    return isValid;
}

function ValidateLuhn(card, num)
{
    if ('AX!DI!JC!VI!MC'.indexOf(card) == -1)
    {
        return true;
    }

    num = (num + '').replace(/\D+/g, '').split('').reverse();
    if (!num.length) 
    {
        return false;
    }
    var total = 0, i;
    for (i = 0; i < num.length; i++) 
    {
        num[i] = parseInt(num[i], 10)
        total += i % 2 ? 2 * num[i] - (num[i] > 4 ? 9 : 0) : num[i];
    }

    return (total % 10) == 0;
}


function ValidateIsracard(card, num, isracardlocale)
{
    if (('!MC'.indexOf(card) != -1) && ((num.length == 8) || (num.length == 9)) && ((isracardlocale == 'he-IL') || (isracardlocale == 'ru-IL')))
    {      
        var total = 0;
        for (var i = 1, l = num.length; i <= l; ++i)
        {
                total += (i * parseInt(num.charAt(l - i)));
        }
        return (total % 11) == 0;        
    }

    else
    {
        return false;
    } 
}


function GetEventSource(e) 
{
    var node = null;
    if (null != window.event) 
    {
        node = window.event.srcElement;
    }
    else
    {
        if (null != e) 
        {
            node = e.target;
            while (node.nodeType != node.ELEMENT_NODE) 
            {
                node = node.parentNode;
            }
        }
    }

    return node;
}

function SetSelection(ctl, value) 
{
    if (null != ctl && null != ctl[0]) 
    {
        for (var i = 0; i < ctl.length; i++) 
        {
            if (ctl[i].value == value) 
            {
                ctl[i].selected = 'selected';
                return;
            }
        }
    }
}

function GetTextValue(node) 
{
    var ret = null;
    if (null != node) 
    {
        if (null != node.innerText) 
        {
            ret = node.innerText;
        }
        else if (null != node.textContent) 
        {
            ret = node.textContent;
        }
        if (null == ret) 
        {
            if (null != node.text && node.text.length > 0) 
            {
                ret = node.text;
            }
            else 
            {
                ret = node;
            }
        }
    }

    return ret;
}

function SetTextValue(node, value) 
{
    var ret = false;
    if (null != node.innerText) 
    {
        node.innerText = value;
        ret = true;
    }
    else 
    {
        node.textContent = value;
        ret = true;
    }

    return ret;
}

function EnableControl(bEnable, ctl, bRecursive) 
{
    if (null != ctl.Attributes) 
    {
        var val = ctl.getAttribute("disabled");
        if (null == val) 
        {
            ctl.setAttribute("disabled", "");
            val = ctl.getAttribute("disabled");
        }
        if (null != val) 
        {
            if (null != ctl.id) ctl.setAttribute("disabled", (bEnable) ? "" : "true");
            if (bRecursive) 
            {
                var cn = ctl.childNodes;
                if (null != cn) 
                {
                    for (var i = 0; i < cn.length; i++) 
                    {
                        EnableControl(bEnable, cn[i], bRecursive);
                    }
                }
            }
        }
    }
}

function resolveSingleCardRowSelection(item)
{
    /*var row = item;
    while (null != row && row.tagName != "TR" && row.tagName != "TH" && row.tagName != "TBODY" && row.tagName != "TABLE")
    {
    row = GetParentElement(row);
    }
    if (null != row) 
    {
    var allRows = GetParentElement(row).childNodes;
    for (var i = 0; i < ((registeredCards) ? allRows.length : allRows.length -1); i++) 
    {
    if (allRows[i].childNodes[0].tagName != 'TH') 
    {
    for (var j = 1; j < ((registeredCards) ? allRows[i].childNodes.length : allRows[i].childNodes.length -1); j++) 
    {
    EnableControl(row == allRows[i], allRows[i].childNodes[j], true)
    //allRows[i].runtimeStyle.fontBold = (row == allRows[i])
    var ctls = allRows[i].getElementsByTagName("INPUT");

    if (null != ctls && ctls.length > 1) 
    {
    ctls[1].checked = (row == allRows[i]);
    ctls[ctls.length - 1].value = (row == allRows[i]) ? findBalanceField().value : '';
    if (ctls[ctls.length - 2].id.indexOf('CVV') > 0 && row != allRows[i]) 
    {
    ctls[ctls.length - 2].value = '';
    }
    }
    }
    }
    }
    }*/

    $("tr.cardPaymentRow").each(function () 
    {
        var isSelected = false;
        var inputRadio = $(this).find("input[type=radio]").first().get();
        if (inputRadio.length > 0) 
        {
            isSelected = (item == inputRadio[0]);
            $(inputRadio[0])
                .attr("checked", isSelected);
        }
        $(this)
            .find("td")
            .css("fontColor", (isSelected ? "#000" : ""))
            .css("fontWeight", (isSelected ? "bold" : "normal"));
        $(this)
            .find("input[type=text]")
            .attr("disabled", !isSelected)
            .val("");
        $(this)
            .find(".cardPaymentRowAmount")
            .val((isSelected ? findBalanceField().value : ''));

        var i = 0;
        $(this)
            .find("td")
            .each(function () 
            {
                if (i++ > 0) 
                {
                    if (isSelected) 
                    {
                        $(this).removeAttr("disabled");
                    }
                    else 
                    {
                        $(this)
                            .attr("disabled", true);
                    }
                }
            })
            .last()
            .removeAttr("disabled");
    });
}

function GetValidationRegex(cardType) 
{
    var validation = "//";
    var validations = document.getElementById(prefix + "Validations").value;
    var listValidations = validations.split(";");
    for (var i = 0; i < listValidations.length; i++) 
    {
        if (listValidations[i].substring(0, listValidations[i].indexOf("=")) == cardType)
        {
            validation = listValidations[i].substring(listValidations[i].indexOf("=") + 1);
            break;
        }
    }

    return validation;
}

function ValidateUniqueNickName(nickName)
{
    var onick = (typeof originalNick !== "undefined") ? ((null !== originalNick) ? originalNick : '') : '';
    
    if (nickName != onick)
    {
        var hdNickNameList = document.getElementById(prefix + "hdNickNameList").value;
        return (null != hdNickNameList) ? (hdNickNameList.search(nickName + ";") < 0) : false;
    }
    else
    {
        return true;
    }
}

function GetParentElement(node) 
{
    var ret = null;
    if (null != node) 
    {
        if (null != node.parentElement) 
        {
            ret = node.parentElement;
        }
        else if (null != node.parentNode) 
        {
            ret = node.parentNode;
        }
    }

    return ret;
}

function CheckSaveBoxSettings(e) 
{
    var chkSave = GetEventSource(e);
    if (null != chkSave) 
    {
        var chkPrimary = document.getElementById(prefix + "chkMakePrimaryCreditCard");
        if(null != chkPrimary)
        {
            if (chkSave.checked) 
            {
                chkPrimary.disabled = false;
                // ie fix
                $(chkPrimary).closest('span').removeAttr('disabled');
            }
            else 
            {
                chkPrimary.checked = false;
                chkPrimary.disabled = true;
            }
        }
    }
}

function CheckPrimaryBoxSettings(e) 
{
    var chkPrimary = GetEventSource(e);
    if (null != chkPrimary) 
    {
        var chkSave = document.getElementById(prefix + "chkSaveCreditCard");
        if (null != chkSave) 
        {
            if (chkPrimary.checked) 
            {
                if (!chkSave.checked) 
                {
                    chkPrimary.checked = false;
                }
            }
        }
    }
}


function ValidateNewCardBR(e, item)
{
    var msg = '';
    var cardList = document.getElementById(prefix + "ddlCardType");
    var cardType = cardList[cardList.selectedIndex].value;
    var nickName = document.getElementById(prefix + "txtNickName");
    var cardNumber = document.getElementById(prefix + "txtCardNumber");
    var cardName = document.getElementById(prefix + "txtCardHolderName");
    var streetAddress = document.getElementById(prefix + "txtStreetAddress");
    var streetNumber = document.getElementById(prefix + "txtNumber");
    var neighborhood = document.getElementById(prefix + "txtNeighborhood");
    var city = document.getElementById(prefix + "txtCity");
    var state = document.getElementById(prefix + "txtState");
    var zip = document.getElementById(prefix + "txtZip");
    var editMode = document.getElementById(prefix + "EditMode");
    var message = document.getElementById(prefix + "lblMessage");
    TokenTimerEnabled = document.getElementById(prefix + "hdTokenTimerEnabled");
    var isTokenized = false;
    var isValid = true;

   //Go
    if (isValid) 
    {
        if (null != nickName.value && nickName.value.length > 0)
    {
        isValid = ValidateUniqueNickName(nickName.value);
            if (!isValid)
        {
            msg = validNonDupeNickNameRequired;
            nickName.focus();
            }
            
            isValid = CheckNickNameForNumbers(nickName.value);
            if (!isValid) {
                msg = validNickNameRequired;
                nickName.focus();
            }
    }
    }

    if (isValid) 
    {
        if (null == cardName.value || cardName.value.length == 0) 
        {
            isValid = false;
            msg = CardNameRequired;
            cardName.focus();
        }
    }

    if (isValid)
    {
        if (null == cardList || cardList.selectedIndex <= 0)
        {
            isValid = false;
            msg = CardTypeRequired;
            cardList.focus();
        }
    }

    if (isValid)
    {
        if (null == cardNumber.value || cardNumber.value.length == 0)
        {
            isValid = false;
            msg = CardNumberRequired;
            cardNumber.focus();
        }
    }

    if (isValid)
    {
        if (cardNumber.value.indexOf('XXXX') == -1)
        {
            var valRegex = GetValidationRegex(cardType);
            if (!cardNumber.value.match(valRegex) || !ValidateLuhn(cardType, cardNumber.value))
            {
                isTokenized = cardNumber.value.match(tokenizedRegex);
                if (!isTokenized)
                {
                    isValid = false;
                    msg = CardNumberInvalid;
                    cardNumber.focus();
                    cardNumber.select();
                }
            }
        }
        else
        {
            isTokenized = true;
        }
    }

    if (isValid)
    {
        if (!ValidateExpirationDate(cardList, prefix))
        {
            isValid = false;
            msg = ExpDateRequired;
        }
    }

    if (isValid)
    {
        if (!ValidateExpirationDateIsCurrent(cardList, prefix))
        {
            isValid = false;
            msg = CardHasExpired;
        }
    }

    if (isValid)
    {
        if (null != streetAddress)
        {
            if (streetAddress.value.length == 0)
            {
                isValid = false;
                msg = StreetAddressRequired;
                streetAddress.focus();
            }

            if (isValid)
            {
                if (null != streetNumber && streetNumber.value.length == 0)
                {
                    isValid = false;
                    msg = NumberRequired;
                    streetNumber.focus();
                }
            }

            if (isValid)
            {
                if (null != neighborhood && neighborhood.value.length == 0)
                {
                    isValid = false;
                    msg = NeighborhoodRequired;
                    neighborhood.focus();
                }
            }

            if (isValid)
            {
                if (null != city && city.value.length == 0)
                {
                    isValid = false;
                    msg = CityRequired;
                    city.focus();
                }
            }

            if (isValid)
            {

                if (null != state && state.value.length == 0)
                {
                    isValid = false;
                    msg = StateRequired;
                    state.focus();
                }
            }

            if (isValid)
            {
                if (null != zip && zip.value.length == 0)
                {
                    isValid = false;
                    msg = ValidZipRequired;
                    zip.focus();
                }
            }
        }
    }
    
    //Tokenization
    if (isValid && !tokenizationDisabled && !isTokenized)
    {
        try
        {
            Tokenize(cardNumber.value);
        }
        catch (e)
        {
            tokenSuccess = false;
        }

        isValid = tokenSuccess;
        if (!isValid) {
            msg = TokenizationFailed;
        } else {
            if (TokenTimerEnabled) {
                tokenTimer = setTimeout(function() { TokenTimerFunction(); }, 10000);
            }
        }
    }

    if (isValid && !tokenizationDisabled)
    {
        TestNotTokenized();
    }
    
    if (!isValid) 
    {
        SetTextValue(message, msg);
    }

    return isValid;
}
function escapeRegExp(str) {
    return str.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
}
var luhn10 = function (a, b, c, d, e) {
    for (d = +a[b = a.length - 1], e = 0; b--;)
        c = +a[b], d += ++e % 2 ? 2 * c % 10 + (c > 4) : c;
    return !(d % 10);
};