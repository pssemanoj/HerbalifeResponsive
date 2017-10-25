// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceListGenerator.js" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Used to provide price list generator client side functionality.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// On load function
$(document).ready(function () {

    // Getting calculations.
    $('#PriceListGeneratorInputs a').last().click(function (e) {
        ExecuteInputChanges();

        // Preventing a postback.
        if (e && e.preventDefault) {
            e.preventDefault();
        }
        return false;
    });

    // Validation for find-tax controls.
    $('.txtTaxCity, .txtTaxZipCode, .txtStreetAddress').focus(function (e) {
        ReviewDefaultText(
            $(this),
            $(this).hasClass('txtTaxCity') ? window.defaultCityText : $(this).hasClass('txtTaxZipCode') ? window.defaultZipCodeText : window.defaultStreetAddress,
            'focus');
    }).blur(function (e) {
        ReviewDefaultText(
            $(this),
            $(this).hasClass('txtTaxCity') ? window.defaultCityText : $(this).hasClass('txtTaxZipCode') ? window.defaultZipCodeText : window.defaultStreetAddress,
            'blur');
    });

    // Show/Hide find-tax controls.
    $('#PriceListGeneratorInputs .linkBtn').first().click(function (e) {
        HideNotificationMessage();

        if ($('#TaxControls').hasClass('hide')) {
            $('#TaxControls').removeClass('hide');
            $('.lblErrorMsgTaxlookup1').removeClass('hide');
            $('.lblErrorMsgTaxlookup2').addClass('hide');
        } else {
            $('#TaxControls').addClass('hide');
        }

        // Preventing a postback.
        if (e && e.preventDefault) {
            e.preventDefault();
        }
        return false;
    });

    // Find tax action.
    $('#TaxControls a').last().click(function (e) {
        $('.txtSalesTaxCalculated').val('');
        if ($('.txtTaxCity').val() != window.defaultCityText
            && $('.txtStreetAddress').val() != window.defaultStreetAddress
            && $('.txtTaxZipCode').val() != window.defaultZipCodeText
            && $('.txtTaxState').val() != window.defaultStateOption) {

            // Display loading panel.
            ShowOnProgressStatus(true);

            // Getting the new data, according the command.
            $.ajaxPost(
                "GetSalesTax",
                "{ " +
                    "city: '" + $('.txtTaxCity').val() + "' ," +
                    "state: '" + $('.txtTaxState').val() + "', " +
                    "streetAddress: '" + $('.txtStreetAddress').val() + "', " +
                    "zipCode: '" + $('.txtTaxZipCode').val() + "' " +
                    "} ",
                SetSalesTaxValue);

            $('#TaxControls').addClass('hide');
        } else {
            $('.lblErrorMsgTaxlookup2').removeClass('hide');
            $('.lblErrorMsgTaxlookup1').addClass('hide');
        }

        // Preventing a postback.
        if (e && e.preventDefault) {
            e.preventDefault();
        }
        return false;
    });

    // Print only customer price column.
    $('.cbCustomerPrice').click(function () {
        LoadGridSettings();
    });

    // Append help icons.
    $('.distributorCostColumn').append($('#imgDistributorDiscountHelpIcon'));
    $('.distributorloadedCostColumn').append($('#imgDistributorLoadedCostIcon'));
    $('.customerPriceColumn').append($('#imgCustomerPriceIcon'));

    $('#TaxControls').click(function (e) {
        $(this).removeClass('hide');
    });

    $(document).mouseup(function (e) {
        var container = $('#TaxControls');

        if (container.has(e.target).length === 0) {
            container.addClass('hide');
        }
    });

    // Set inputs validations.
    $('.txtSalesTax, .txtShippingAndHandling, .txtCustomerDiscount').blur(function () {
        // Validate input on blur.
        ValidateInput($(this));
    });

    // Set functionality when clicking the arrow of the estimate S&H Fragment.
    $('.selectRate').click(function () {
        var val = $(this).parent().parent().children('td').eq(1).html();

        if (val.indexOf('span') >= 0) {
            val = $(this).parent().parent().children('td').eq(0).html();
        }

        if (val) {
            $("#estimateModal").data('kendoWindow').close();
            $('.txtShippingAndHandling').val(val.substring(0, val.indexOf('%')));
        }
    });

    // Exporting button functionallity.
    $('#' + window.btnExportId).click(function (e) {
        var taxInformation = '';
        if (!$('.cbCustomerPrice').children()[0].checked) {
            if ($('.txtSalesTax').val() == $('.txtSalesTaxCalculated').val()) {
                if ($('.txtTaxCity').val() != window.defaultCityText
                    && $('.txtStreetAddress').val() != window.defaultStreetAddress
                    && $('.txtTaxZipCode').val() != window.defaultZipCodeText
                    && $('.txtTaxState').val() != window.defaultStateOption) {
                    taxInformation = $('.txtTaxCity').val() + ", " + $('.txtTaxState').val() + ", " + $('.txtTaxZipCode').val();
                }
            }
        }

        window.open('PriceListGeneratorPrintable.aspx?ExportWhenRender=false' +
            (taxInformation ? "&TaxGeoInformation=" + taxInformation : '') +
            ($('.cbCustomerPrice').children()[0].checked ?
                Number($('.txtCustomerDiscount').val()) > 0 ? "&ViewMode=CustomerWithDiscount" :
                    "&ViewMode=Customer" : "") +
            ($('.txtSalesTax').val() ? "&SalesTax=" + $('.txtSalesTax').val() : "") +
            ($('.txtShippingAndHandling').val() ? "&SHTax=" + $('.txtShippingAndHandling').val() : "") +
            (window.isMultipleRate == '1' ? "&MultipleTax=1" : "") + "&ExportXLS=false");

        // Preventing a postback.
        if (e && e.preventDefault) {
            e.preventDefault();
        }

        return false;
    });

    // Exporting To Excel
    $('#' + window.btnExportXLSId).click(function (e) {
        var taxInformation = '';
        if (!$('.cbCustomerPrice').children()[0].checked) {
            if ($('.txtSalesTax').val() == $('.txtSalesTaxCalculated').val()) {
                if ($('.txtTaxCity').val() != window.defaultCityText
                    && $('.txtStreetAddress').val() != window.defaultStreetAddress
                    && $('.txtTaxZipCode').val() != window.defaultZipCodeText
                    && $('.txtTaxState').val() != window.defaultStateOption) {
                    taxInformation = $('.txtTaxCity').val() + ", " + $('.txtTaxState').val() + ", " + $('.txtTaxZipCode').val();
                }
            }
        }

        window.open('PriceListGeneratorPrintable.aspx?ExportWhenRender=false' +
            (taxInformation ? "&TaxGeoInformation=" + taxInformation : '') +
            ($('.cbCustomerPrice').children()[0].checked ?
                Number($('.txtCustomerDiscount').val()) > 0 ? "&ViewMode=CustomerWithDiscount" :
                    "&ViewMode=Customer" : "") +
            ($('.txtSalesTax').val() ? "&SalesTax=" + $('.txtSalesTax').val() : "") +
            ($('.txtShippingAndHandling').val() ? "&SHTax=" + $('.txtShippingAndHandling').val() : "") +
            (window.isMultipleRate == '1' ? "&MultipleTax=1" : "") + "&ExportXLS=true");

        // Preventing a postback.
        if (e && e.preventDefault) {
            e.preventDefault();
        }

        return false;
    });
    
    // Close the popup message
    $('#close-message').click(function () {
        var wnd = $("#messageModal").data("kendoWindow");
        wnd.close();
        LoadGridSettings();
    });
});

// validate input percentage.
function ValidateInput(control) {
    HideNotificationMessage();

    var val = FormatPercentageInput(control.val());
    if (isNaN(val) || val > 1) {
        control.val((0).toFixed(2));
        ShowNotificationMessage(window.invalidInputMessage, false);
        return;
    }

    var sepIndex = control.val().indexOf('.');
    if (control.val().indexOf(',') > sepIndex) {
        sepIndex = control.val().indexOf(',');
    }

    // Number of decimals
    if (sepIndex >= 0 && control.val().length - sepIndex > 3) {
        control.val(control.val().substring(0, sepIndex + 3));
    }
}

function ExecuteInputChanges() {
    // hide notfification message.
    HideNotificationMessage();

    // Getting user intputs.
    var salesTax = FormatPercentageInput($('.txtSalesTax').val());
    var shippingHandling = FormatPercentageInput($('.txtShippingAndHandling').val());
    var distributorDiscount = FormatPercentageInput($('.txtDistributorCost').val());
    var customerDiscount = FormatPercentageInput($('.txtCustomerDiscount').val());

    if (isNaN(salesTax) || salesTax > 1
        || isNaN(shippingHandling) || shippingHandling > 1
        || isNaN(distributorDiscount) || distributorDiscount > 1
        || isNaN(customerDiscount) || customerDiscount > 1) {
        ShowNotificationMessage(window.invalidInputMessage, false);
    }

    var hasChange = false;
    // If an input change.
    if (($('.txtSalesTax').val() != Number(window.currentSalesTax)
        || $('.txtShippingAndHandling').val() != Number(window.currentShippingAndHandling)
        || $('.txtDistributorCost').val() != Number(window.currentDistributorCost)
        || $('.txtCustomerDiscount').val() != Number(window.currentCustomerDiscount)) || $('#' + window.ProductListGridID).hasClass('hide')) {
        hasChange = true;
    }

    if (hasChange) {
        if (window.isMultipleRate == '1' && $('.txtSalesTax').val() != Number(window.currentSalesTax) &&
            Number(window.currentSalesTax) == 0) {
            window.isMultipleRate = '';
            $('#Flag').addClass('hide');
        }

        // Setting current input values.        
        window.currentSalesTax = $('.txtSalesTax').val();
        window.currentShippingAndHandling = $('.txtShippingAndHandling').val();
        window.currentDistributorCost = $('.txtDistributorCost').val();
        window.currentCustomerDiscount = $('.txtCustomerDiscount').val();

        if (window.isMultipleRate == '1') {
            var multiMessage = window.multipleTaxRateText.replace("{0}", $('.txtShippingAndHandling').val());
            $('#pMessage').text(multiMessage);
        } else {
            var singleMessage = window.singleTaxRateText.replace("{0}", $('.txtSalesTax').val());
            singleMessage = singleMessage.replace("{1}", $('.txtShippingAndHandling').val());
            $('#pMessage').text(singleMessage);
        }

        var wnd = $("#messageModal").data("kendoWindow");
        wnd.center().open();
    } else {
        EnableExportToPDFButton(true);
    }
}

// Load grid settings.
function LoadGridSettings() {
    // Getting user intputs.
    var salesTax = FormatPercentageInput($('.txtSalesTax').val());
    var shippingHandling = FormatPercentageInput($('.txtShippingAndHandling').val());
    var distributorDiscount = FormatPercentageInput($('.txtDistributorCost').val());
    var customerDiscount = FormatPercentageInput($('.txtCustomerDiscount').val());

    // Validations.
    if (isNaN(salesTax)) {
        return;
    }
    if (isNaN(shippingHandling)) {
        return;
    }
    if (isNaN(distributorDiscount)) {
        return;
    }
    if (isNaN(customerDiscount)) {
        return;
    }

    if ($('body').css('cursor') != 'wait') {
        // Display loading panel.
        ShowOnProgressStatus(true);
    }

    // Getting the new data, according the command.
    $.ajaxPost(
        "GetData",
        "{ " +
            "salesTax: " + salesTax + " ," +
            "shippingAndHandling: " + shippingHandling + ", " +
            "distributorDiscount: " + distributorDiscount + ", " +
            "customerDiscount: " + customerDiscount + " " +
            "} ",
        updateGrid);
}

// Format percentage input.
function FormatPercentageInput(inputText) {
    return Number(inputText.replace(',', '.')) / 100;
}

// Setting the data source to the grid after the get data method successfully action.
function updateGrid(result) {
    result = result.d;

    // Find grid object.
    var tableView = window.$find(window.ProductListGridID).get_masterTableView();

    // Clareing all exsiting category names.
    $('.categoryName').parent().remove();

    // Fixing the width of the columns with a selector because telerik method does not work. (Just for FireFox)
    $('#' + window.ProductListGridID + '_ctl00_Header colgroup:first-child col:nth-child(2)').css('width',
        $('.cbCustomerPrice').children()[0].checked ? '100%' : '236px');
    $('#' + window.ProductListGridID + '_ctl00 colgroup:first-child col:nth-child(2)').css('width',
        $('.cbCustomerPrice').children()[0].checked ? '100%' : '236px');

    // Setting the data source.
    tableView.set_dataSource(result);
    tableView.dataBind();
    ReviewColumnsToShow();
    // Show the table and buttons.
    $('#' + window.ProductListGridID).removeClass('hide');
    $('#PriceListGeneratorButtons').removeClass('hide');
    
    if (window.isMultipleRate == '1') {
        $('#Flag').removeClass('hide');
    } else {
        $('.Flag').addClass('hide');
    }

    // Showing updated meessage.
    ShowNotificationMessage(window.priceListUpdatedText, true);

    // Hide the on progress status.
    ShowOnProgressStatus(false);
}

// Hide notification message.
function HideNotificationMessage() {
    $('#notificationMessage').addClass('hide');
}

// Show notification message.
function ShowNotificationMessage(msg, positive) {
    var noticeMsg = $('#notificationMessage');
    var noticeIcon = $('#msgIcon');

    //assign content
    noticeIcon.text(msg);

    //reset message styles 
    if (noticeMsg.hasClass('successPLGMsg') && !positive) {
        noticeMsg.removeClass('successPLGMsg');
        noticeIcon.removeClass('successIcon');
    }
        //if success, add success style
    else if (positive) {
        noticeMsg.addClass('successPLGMsg');
        noticeIcon.addClass('successIcon');
    }

    //show msg
    noticeMsg.removeClass('hide');
}

// On grid row data bound.
function GridRowDataBound(sender, args) {
    // Loading row data elements.
    var gridItem = args.get_item();
    var record = args.get_dataItem();

    // Adding a delimiter to each category.
    var templateCell = gridItem.get_cell("Sku");
    if (window.currentCategory != record.Category) {
        $("<tr><td class='categoryName' colspan='7'>" + record.Category + "</td></tr>").insertBefore($(templateCell).parent());
    }
    window.currentCategory = record.Category;
}

// On grid command function.
function GridCommand(sender, args) {
    try {
        if (args) {
            args.set_cancel(true);
        }

        if (window.isMultipleRate == '1' && $('.txtSalesTax').val() != Number(window.currentSalesTax) &&
            Number(window.currentSalesTax) == 0) {
            window.isMultipleRate = '';
            $('#Flag').addClass('hide');
        }

        if (window.isMultipleRate == '1') {
            var multiMessage = window.multipleTaxRateText.replace("{0}", $('.txtShippingAndHandling').val());
            $('#pMessage').text(multiMessage);
        } else {
            var singleMessage = window.singleTaxRateText.replace("{0}", $('.txtSalesTax').val());
            singleMessage = singleMessage.replace("{1}", $('.txtShippingAndHandling').val());
            $('#pMessage').text(singleMessage);
        }

        var wnd = $("#messageModal").data("kendoWindow");
        wnd.center().open();
    } catch (ex) {
        alert(ex);
    }
}

// Show on progress status method.
function ShowOnProgressStatus(onProgress) {
    var progressPanel = window.$find(window.LoadingPanelClientID);

    // If page is on progress, show loading panel.
    if (onProgress) {
        progressPanel.show(window.ProductListGridID);
        $('body').css('cursor', 'wait');
        $('input').readonly = true;
        $('input').css('background-color', 'lightgray');
        EnableExportToPDFButton(false);
        $('#ctl00_ctl00_ContentArea_ProductsContent_cbCustomerPrice').parent().hide();
    }
        // If page is not on progress, hide loading panel.
    else {
        progressPanel.hide(window.ProductListGridID);
        $('body').css('cursor', 'auto');
        $('input').readonly = false;
        $('input').css('background-color', 'white');
        EnableExportToPDFButton(true);
        $('#ctl00_ctl00_ContentArea_ProductsContent_cbCustomerPrice').parent().show();
    }
}

function EnableExportToPDFButton(enabled) {
    if (enabled) {
        $('#' + window.btnExportDisabledId).css('display', 'none');
        $('#' + window.btnExportId).css('display', 'inline-block');
    } else {
        $('#' + window.btnExportId).css('display', 'none');
        $('#' + window.btnExportDisabledId).css('display', 'inline-block');
    }
}

// Review the default text for inputs that has one.
function ReviewDefaultText(control, defaultText, event) {
    if (event === 'focus') {
        if (control.val() === defaultText) {
            control.val('');
        }
    }
    else if (event === 'blur') {
        if (!control.val()) {
            control.val(defaultText);
        }
    }
}

// Set the sales tax value for text box.
function SetSalesTaxValue(result) {
    if ('InvalidAddress' == (result.d)) {

        // Display loading panel.
        ShowOnProgressStatus(false);

        ShowNotificationMessage(window.invalidAddressMessage, false);
    } else {
        $('.lblErrorMsgTaxlookup1').removeClass('hide');
        $('.lblErrorMsgTaxlookup2').addClass('hide');

        var returnedValues = result.d.split('@');

        if (returnedValues[0] == 'Multiple') {
            $('.txtSalesTax').val("0.0");
            $('.txtSalesTax').next().val("0.0");
            $('.txtSalesTax').next().next().val("0.0");
            $('.txtSalesTaxCalculated').val("0.0");
            $('#Flag').removeClass('hide');
            window.isMultipleRate = '1';
            var multiMessage = window.multipleTaxRateText.replace("{0}", $('.txtShippingAndHandling').val());
            $('#pMessage').text(multiMessage);
        } else {
            $('.txtSalesTax').val(returnedValues[0]);
            $('.txtSalesTax').next().val(returnedValues[0]);
            $('.txtSalesTax').next().next().val(returnedValues[0]);
            $('.txtSalesTaxCalculated').val(returnedValues[0]);
            $('#Flag').addClass('hide');
            window.isMultipleRate = '';
            var singleMessage = window.singleTaxRateText.replace("{0}", $('.txtSalesTax').val());
            singleMessage = singleMessage.replace("{1}", $('.txtShippingAndHandling').val());
            $('#pMessage').text(singleMessage);
        }

        window.currentSalesTax = $('.txtSalesTax').val();
        window.currentShippingAndHandling = $('.txtShippingAndHandling').val();
        window.currentDistributorCost = $('.txtDistributorCost').val();
        window.currentCustomerDiscount = $('.txtCustomerDiscount').val();

        $('.txtTaxCity').val(returnedValues[1]);
        $('.txtTaxZipCode').val(returnedValues[2]);
        ShowOnProgressStatus(false);

        var wnd = $("#messageModal").data("kendoWindow");
        wnd.center().open();
    }
}

// Review columns to show.
function ReviewColumnsToShow() {
    var tableView = window.$find(window.ProductListGridID).get_masterTableView();
    if ($('.cbCustomerPrice').children()[0].checked) {
        var customerDiscount = $('.txtCustomerDiscount').val().trim();
        tableView.hideColumn(2);

        if (!isNaN(customerDiscount) && Number(customerDiscount) > 0) {
            tableView.showColumn(5);
        } else {
            tableView.hideColumn(5);
        }

        tableView.hideColumn(3);
        tableView.hideColumn(4);
        tableView.hideColumn(6);
        tableView.hideColumn(7);
    } else {
        tableView.showColumn(2);
        tableView.showColumn(3);
        tableView.showColumn(4);
        tableView.hideColumn(5);
        tableView.showColumn(6);
        tableView.showColumn(7);
    }
}

// Show help text.
function ShowHelpText(id) {
    var tooltip = $find(id);
    if (tooltip) {
        if (tooltip.show) {
            tooltip.show();
        }
    }
    if (event) {
        event.returnValue = false;
    }
}
