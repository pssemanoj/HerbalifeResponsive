<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CCTokenizer.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.CCTokenizer" %>
<asp:Label ID="lblControlMessage" runat="server" meta:resourcekey="lblControlMessageResource1"></asp:Label>
<asp:HiddenField ID="hdnValidations" runat="server" />

<script type="text/javascript">

    function ValidateNewCard(e, item)
    {
		var msg = '';
	    var isValid = true;
        
		var cardList = document.getElementById(ddlCardtypeControlName);
	    var cardType;
	    if (cardList.length > 0)
	    {
	        cardType = cardList[cardList.selectedIndex].value;
	    }
		var cardNumber = document.getElementById(txtCardNumberControlName);
		var cvv = document.getElementById(txtCVVControlName);
		var cardHolder = document.getElementById(txtCardHolderNameControlName);
		var cardName = document.getElementById(txtCardNameControlName);
		var streetAddress = document.getElementById(txtStreetAddressControlName);
		var city = document.getElementById(txtCityControlName);
		var state = document.getElementById(txtStateControlName);
		var zip = document.getElementById(txtZipControlName);
		var message = document.getElementById(lblMessageControlName);
		var tokenizedRegex = '^[0-9]{1}[a-z]{11}[0-9]{4}$';
		var isTokenized = false;
		var isValid = true;


		if (null != cardName && cardName.value.length == 0)
		{
			isValid = false;
			msg = CardNameRequired;
			cardName.focus();
		}

		if (isValid)
		{
		    if (null != cardList && cardList.selectedIndex < 0)
		    {
				isValid = false;
				msg = CardTypeRequired;
				cardList.focus();
			}
		}

		if (isValid)
		{
		    if (null != cardNumber && cardNumber.value.length == 0)
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
		}

		if (isValid)
		{
		    if (!ValidateExpirationDate(cardList))
		    {
				isValid = false;
				msg = ExpDateRequired;
			}
		}

		if (isValid)
		{
		    if (null != cvv && (null != CVVRequired && CVVRequired.length > 0))
		    {
		        if (null == cvv.value || cvv.value.length == 0)
		        {
		            isValid = false;
		            msg = CVVRequired;
		            cvv.focus();
		        }
		    }
		}

		if (isValid)
		{
		    if (!ValidateExpirationDateIsCurrent(cardList))
		    {
				isValid = false;
				msg = CardHasExpired;
			}
		}

		if (isValid)
		{
		    if (null != streetAddress)
		    {
		        if (null == streetAddress.value || streetAddress.value.length == 0)
		        {
					isValid = false;
					msg = StreetAddressRequired;
					streetAddress.focus();
				}

		        if (isValid)
		        {
		            if (null != city && (null != CityRequired && CityRequired.length > 0))
		            {
		                if (null == city.value || city.value.length == 0)
		                {
		                    isValid = false;
		                    msg = CityRequired;
		                    city.focus();
		                }
					}
				}

		        if (isValid)
		        {
		            if (null != state && (null != StateRequired && StateRequired.length > 0))
		            {
		                if (null == state.value || state.value.length == 0)
		                {
		                    isValid = false;
		                    msg = StateRequired;
		                    state.focus();
		                }
					}
				}

		        if (isValid)
		        {
		            if (null != zip && (null != ValidZipRequired && ValidZipRequired.length > 0))
		            {
		                if (null == zip.value || zip.value.length == 0)
		                {
		                    isValid = false;
		                    msg = ValidZipRequired;
		                    zip.focus();
		                }
					}
				}
			}
		}

		//Tokenization
		if (isValid && !tokenizationDisabled && !isTokenized)
		{
			Tokenize(cardNumber.value);
			isValid = tokenSuccess;
			if (!isValid)
			{
				msg = TokenizationFailed;
			}
		}

		if (!isValid)
		{
		    SetTextValue(message, msg);
		    return false;
		}
		else
		{
		    message.parentNode.style.display = 'none';
		}

		return isValid;
	}

	var tokenSuccess = false;

	function Tokenize(cardNumber)
	{
	    if (!DontTokenizeJustMaskAndUseSessionStorage)
	    {
	        $.ajax(
            {
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
	    else
	    {
	        try
	        {
	            var masked = cardNumber.charAt(0) + "XXXXXXXXXXX" + cardNumber.substring(cardNumber.length - 4);
	            document.getElementById(txtCardNumberControlName).value = masked;
	            window.name = cardNumber;
	        }
	        catch (e)
	        {
	            tokenSuccess = false;
	        }
	        tokenSuccess = true;
        }
	}

	function ServiceSucceeded(result)
	{
	    try
	    {
	        var token = result.GetTokenResult.Token;
	        var failed = result.GetTokenResult.FailureReason;
			tokenSuccess = (token.length > 0 && null == failed)
			if (tokenSuccess)
			{
			    document.getElementById(txtCardNumberControlName).value = token;
			    MaskToken();
			}
		}
	    catch (e)
	    {
			tokenSuccess = false;
		}
	}

    function ServiceFailed(xhr)
	{
		var token = 'tokenizedCard';
		document.getElementById(txtCardNumberControlName).value = token;
		tokenSuccess = false;
    }

    function MaskToken()
    {
        var strCardNo = document.getElementById(txtCardNumberControlName).value;
        var lastChars = strCardNo.substr(strCardNo.length - 4);
        var firstChars = strCardNo.substr(0, 1);
        var newCardNo = (firstChars + "XXXXXXXXXXX" + lastChars);

        var cloned = document.getElementById(txtCardNumberControlName).cloneNode(true);
        cloned.setAttribute('id', 'hdnCardNumber');
        cloned.value = newCardNo;
        document.getElementById(txtCardNumberControlName).parentNode.appendChild(cloned);
        document.getElementById(txtCardNumberControlName).style.display = "none"; 
    }

	function ValidateExpirationDate(card)
	{
		var isValid = (card[card.selectedIndex].getAttribute('Value') == 'QH');
		if (!isValid)
		{
		    var month = document.getElementById(ddlExpMonthControlName);
		    var year = document.getElementById(ddlExpYearControlName);
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

	function ValidateExpirationDateIsCurrent(card)
	{
		var isValid = (card[card.selectedIndex].getAttribute('Value') == 'QH');
		if (!isValid)
		{
		    var month = document.getElementById(ddlExpMonthControlName);
		    var year = document.getElementById(ddlExpYearControlName);
		    var base = (parseInt(year.value, 10) > 2000) ? 0 : 2000;
		    isValid = (new Date(base + parseInt(year.value, 10), parseInt(month.value, 10), 1) > new Date(new Date().getFullYear(), new Date().getMonth(), 1));
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
	    node.parentNode.style.display = 'block';
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

	function GetValidationRegex(cardType)
	{
		var validation = "//";

		var validations = document.getElementById(hdnValidationsControlName).value;
		var listValidations = validations.split(";");
		for (var i = 0; i < listValidations.length; i++)
		{
		    if (listValidations[i].substring(0, 2) == cardType)
		    {
				validation = listValidations[i].substring(3);
				break;
			}
		}

		return validation;
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

</script>
