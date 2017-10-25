// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChinaRecentOrders.js" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Handle the event of the saved carts and recent orders page.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// Load function.
$(function () {
    setTimeout(fixRadMonthYearPicker, 900);
    // Load the grid after the page render event.
    // Avoiding an error, executing something when the grid has not been rendered.
    setTimeout(LoadCartGridSettings, 1000);

    // Handle search textbox focus event.
    $('#' + txtSearchID).focus(function () {
        if ($('#' + txtSearchID).val() == SearchText) {
            $('#' + txtSearchID).val('');
        }
    });

    // Handle search textbox blur event.
    $('#' + txtSearchID).blur(function () {
        if (!$('#' + txtSearchID).val()) {
            $('#' + txtSearchID).val(SearchText);
        }
    });

    // Handle search button click event.
    $('#' + btnSearchID).click(function () {
        // Reload carts grid data.
        RefreshCartsGrid();
        return false;
    });

    // Handle tabs click event.
    $('#tabSavedCarts').click(function () {
        EnableCopyOrderMode(false);
    });
    $('#tabRecentOrders').click(function () {
        EnableCopyOrderMode(true);
    });
});

// Load carts grid function.
function LoadCartGridSettings() {
    try {
        // Search saved carts.
        var grid = $find(CartsGridClientID);
        if (grid) {
            var startYearMonth = GetYearMonth(uiStartYearMonthID);
            var endYearMonth = GetYearMonth(uiEndYearMonthID);

            var orderStatus = $find(ddlOrderStatusID).get_selectedItem().get_value();

            CartsGridGetData(0, grid.get_masterTableView().get_pageSize(), "", startYearMonth, endYearMonth, orderStatus);
        }
    }
    catch (ex) {
        alert(ex);
    }
}

// Show on progress status method.
function ShowOnProgressStatus(onProgress) {
    // If page is on progress, show loading panel.
    if (onProgress) {
        $find(RadAjaxLoadingPanelClientID).show(CartsGridClientID);
        $('body').css('cursor', 'wait');
        $('#tabSavedCarts').css('cursor', 'wait');
        $('#tabRecentOrders').css('cursor', 'wait');
    }
        // If page is not on progress, hide loading panel.
    else {
        $find(RadAjaxLoadingPanelClientID).hide(CartsGridClientID);
        $('body').css('cursor', 'auto');
        $('#tabRecentOrders').css('cursor', 'auto');
        $('#tabSavedCarts').css('cursor', 'auto');
    }
}

// Add more rows to the results (For recent orders only, since is getting 8 more per click.)
function AddMoreRows() {
    CopyOrderIndex += CopyOrderMaxLength;
    // Reload carts grid data.
    RefreshCartsGrid();
}

// To enable or disable the copy order grid mode.
function EnableCopyOrderMode(enabled) {
    CopyOrderMode = enabled;

    // Set page index to 0.
    var tableView = $find(CartsGridClientID).get_masterTableView();
    if (tableView) {
        tableView.set_currentPageIndex(0);
    }

    // Reseting search criteria to default.
    $('#' + txtSearchID).val(SearchText);

    // Show 'showMoreRows' button
    if (CopyOrderMode) {
        $('#ShowMoreRows').removeClass('hide');
        $('#tabSavedCarts').switchClasses('unFocusedTab', 'focusedTab');
        $('#tabRecentOrders').switchClasses('focusedTab', 'unFocusedTab');
    }
    else {
        $('#ShowMoreRows').addClass('hide');
        $('#tabSavedCarts').switchClasses('focusedTab', 'unFocusedTab');
        $('#tabRecentOrders').switchClasses('unFocusedTab', 'focusedTab');
    }

    // Reload carts grid data.
    RefreshCartsGrid();

    // Preventing default.
    if (event) {
        event.returnValue = false;
    }
}

// Setting the data source to the grid after the get data method successfully action.
function updateGrid(result) {
    var result = result.d;

    if (result == null) {
        result = {
            TotalRows: 0,
            ResultList: [],
        };
    }

    // Find grid object.
    var tableView = $find(CartsGridClientID).get_masterTableView();

    // Setting the data source.
    tableView.set_dataSource(result.ResultList);
    tableView.dataBind();

    // Setting the total value.
    tableView.set_virtualItemCount(result.TotalRows);

    // Hide the on progress status.
    // Avoiding on progress never ends issue.
    ShowOnProgressStatus(false);
    ShowOnProgressStatus(false);

    // Hide show more rows link according the filter copy order mode status.
    var fliter = $('#' + txtSearchID).val() == SearchText ? "" : $('#' + txtSearchID).val();
    if (CopyOrderMode && !fliter) {
        $('#ShowMoreRows').show();
    }
    else {
        $('#ShowMoreRows').hide();
    }

    // Hiding adress elements.
    if (responsiveMode != undefined && responsiveMode !== true) {
        $('.hiddenClass').addClass('hide');
    }

    // Show the grid after the data has been rendered.
    $(".RadGrid").show();

    $('table thead tr th:nth-child(1)').addClass('headcol');

    if (undefined != responsiveMode && responsiveMode == true) {

        $('#divGrid').css('width', ($(window).outerWidth() - $('.headcol').outerWidth()) - 10);

        $.each($(".headcol"), function(key, headcol) {
            $(headcol).css("height", $(".productsCont").parent().outerHeight())
        });

        $.each($($(".productsCont").parent()), function (key, productsCont) {
            $(productsCont)
                .parent()
                .find('.headcol')
                .css("height", $(productsCont).outerHeight());
            $(productsCont)
                .parent()
                .find('[id$="lblStoreInfo"]')
                .css({"display" : "block", "min-width": "84px"})
                .parent()
                .css({ "height": $(productsCont).outerHeight(), "border-right": 0, "border-top": 0 });
        });

        $("table thead tr th:nth-child(2)").css({
            "height": $("table thead tr th:nth-child(1)").outerHeight() + "px",
            "min-width": "118px"
        });

    }

    return false;
}

// Carts grid command function .
function CartsGrid_Command(sender, args) {
    try {
        //if(sender)
        if (args) {
            args.set_cancel(true);
        }

        // Getting actual properties from the rad grid.
        var pageSize = sender.get_masterTableView().get_pageSize();
        var filterExpressions = $('#' + txtSearchID).val();
        var currentPageIndex = sender.get_masterTableView().get_currentPageIndex();
        var filterExpressionsAsSQL = filterExpressions.toString();
        var startIndex = currentPageIndex * pageSize;
        var filter = filterExpressions == SearchText ? "" : filterExpressions;

        var startYearMonth = GetYearMonth(uiStartYearMonthID);
        var endYearMonth = GetYearMonth(uiEndYearMonthID);
        
        var orderStatus = $find(ddlOrderStatusID).get_selectedItem().get_value();

        CartsGridGetData(startIndex, pageSize, filter, startYearMonth, endYearMonth, orderStatus);
    }
    catch (ex) {
        alert(ex);
    }
}

function CartsGridGetData(startIndex, pageSize, filter, startYearMonth, endYearMonth, orderStatus) {
    // Show on progress status.
    ShowOnProgressStatus(true);

    // Getting the new data, according the command.
    $.ajaxPost("GetData",
                "{ startIndex : " + startIndex + ", maximumRows : " + pageSize + "," +
                    " sortExpressions : '" + orderBy + "', filterExpressions : '" + filter + "', " +
                    " copyOrderMode : " + CopyOrderMode + ", copyOrderIndex : " + CopyOrderIndex + "," +
                    " copyOrderMaxLength : " + CopyOrderMaxLength +
                    ", startYearMonth : '" + startYearMonth + "'" +
                    ", endYearMonth : '" + endYearMonth + "'" +
                    ", orderStatus : '" + orderStatus + "'" +
                    " }",
                updateGrid);
    if (responsiveMode != undefined && responsiveMode == true) {
        /* Recent Orders */
        startYearMonth = $("[id$='uiStartYearMonth_fix']");
        txtSearch = $("[id$='txtSearch']");

        $('.X-icon').css('margin-left', startYearMonth.outerWidth() -20);
        txtSearch.css('width', (($(window).outerWidth() -startYearMonth.parent().parent().next().outerWidth()) - 52));
    }
}

// On carts grid row data bound.
function CartsGrid_RowDataBound(sender, args) {
    // Loading row data elements.
    var gridItem = args.get_item();
    var record = args.get_dataItem();

    //Cart Summary column template
    var templateCell = gridItem.get_cell("CartSummary");
    var lnkCartName = $(templateCell).children('.lnkCartName')[0];
    var lblDate = $(templateCell).children('.lblDate')[0];
    var pAddress = $(templateCell).children('.pAddress')[0];
    var lblRecipient = $(pAddress).children('.lblRecipient')[0];
    var lblAddress = $(pAddress).children('.lblAddress')[0];

    var totalCol = args.get_item().findElement("lblTotalColumn");
    var volCol = args.get_item().findElement("lblVolumeCoulmn");
    //var channelInfo = args.get_item().findElement("lblChannelInfo");
    var storeInfo = args.get_item().findElement("lblStoreInfo");
    var orderMonth = args.get_item().findElement("lblOrderMonth");
    var orderStatus = args.get_item().findElement("lblOrderStatus");



    $(pAddress).hide();
    $(lnkCartName).removeClass('expanded');

    // Ensure the elements exists.
    if (lnkCartName) {

        $(lnkCartName).parent().addClass('headcol');

        // Cart name link configuration.
        if (CopyOrderMode) {
            if (record.OrderNumber) {
                lnkCartName.innerHTML = record.OrderNumber;
            }
            else {
                lnkCartName.innerHTML = '#';
            }
        }
        else {
            lnkCartName.innerHTML = record.OrderNumber;
        }

        $(lnkCartName).unbind('click');
        $(lnkCartName).click(function () {
            $(lnkCartName).toggleClass('expanded');
            $(pAddress).toggle(10);
            return false;
        });

        // Bounding elements.
        lblDate.innerHTML = record.Date;
        lblAddress.innerHTML = record.Address ? record.Address : '';
        lblRecipient.innerHTML = record.Recipient ? record.Recipient : '';

        totalCol.innerHTML = record.TotalAmount ? record.TotalAmount : '';
        if (volCol != null) {
            volCol.innerHTML = record.VolumePoints ? record.VolumePoints: '';
        }
        //channelInfo.innerHTML = record.ChannelInfo ? record.ChannelInfo : '';
        storeInfo.innerHTML = record.StoreInfo ? record.StoreInfo : '';
        orderMonth.innerHTML = record.OrderMonth ? record.OrderMonth : '';
        orderStatus.innerHTML = record.OrderStatus ? record.OrderStatus : '';

        //Products column template
        templateCell = gridItem.get_cell('Products');
        var divProducts = $(templateCell).children('.productsCont')[0];
        var lnkTotalProducts = $(divProducts).children('.lnkTotalProducts')[0];

        // Link show-more configuration.
        $(lnkTotalProducts).unbind('click');
        $(lnkTotalProducts).click(function () {
            $('.moreRowsItem' + record.ID).toggleClass('hide', 10);

            lnkTotalProducts.innerHTML = lnkTotalProducts.innerHTML == lnkShowLessProducts ?
                lnkShowMoreProducts.replace('{0}', record.CartItems.length) :
                lnkShowLessProducts;

            return false;
        });

        //Loading products details grid view
        if (record.CartItems.length > 0) {
            $($(divProducts).children(0).children(0)).html('<tr>' + $($(divProducts).children(0).children(0).children(0)).html() + '</tr>');
            var r = new Array();
            var j = 0;
            for (var i = 0; i < record.CartItems.length; i++) {
                var cart = record.CartItems[i];
                r[++j] = AddTableRow([cart.Quantity, cart.SKU, cart.Description], i, record.ID);
            }
            $($(divProducts).children(0).children(0)).append(r.join(''));
        }
        lnkTotalProducts.innerHTML = lnkShowMoreProducts.replace('{0}', record.CartItems.length);
        if (!record.CartItems || record.CartItems.length <= 3) {
            $(lnkTotalProducts).hide();
        }
        else {
            $(lnkTotalProducts).show();
        }
        if (record.CartItems.length == 0) {
            $(divProducts).addClass('hide');
        }
        else {
            $(divProducts).removeClass('hide');
        }

    }

    // Buttons column template
    templateCell = gridItem.get_cell('Actions');
    var divCheckOut = $(templateCell).children('.checkOutBtn')[0];
    $(divCheckOut).unbind('click');
    $(divCheckOut).click(function () {
        SetCurrentShoppingCartID(record.ID);
        var cartInfo = "OrderNumber=" + record.OrderNumber + ";PendingOrderEmail=" + record.PendingOrderEmail + ";PendingOrderRecipient=" + record.PendingOrderRecipient
            + ";PendingOrderPhone=" + record.PendingOrderPhone + ";PendingOrderInstruction=" + record.PendingOrderInstruction + ";PendingOrderAltPhone=" + record.PendingOrderAltPhone
            + ";PendingOrderRGNumber=" + record.PendingOrderRGNumber;
        SetCurrentShoppingCartInfo(cartInfo);
        ShowOnProgressStatus(true);
        //$.ajaxPost("ShoppingCartIsNotEmpty", "{ }", CheckCheckOutConfirmation);
    });

    var divCopyOrder = $(templateCell).children('.copyOrderBtn')[0];
    $(divCopyOrder).unbind('click');
    $(divCopyOrder).click(function (event) {
        event.preventDefault();
        SetCurrentShoppingCartID(record.ID);
        ShowOnProgressStatus(true);
        $.ajaxPost("CopyOrder", "{ shoppingCartID : " + record.ID + ", ignoreCartNotEmpty : false }", CheckAndCopy);
    });

    var divDelete = $(templateCell).children('.deleteCartBtn')[0];
    $(divDelete).unbind('click');
    $(divDelete).click(function () {
        SetCurrentShoppingCartInfo(record.OrderNumber);
        $('#' + pnlDeleteCartID).removeClass("hide");
        $find(mdlDeleteCartID).show();
        SetCurrentShoppingCartID(record.ID);
        return false;
    });

    var divFeedBack = $(templateCell).children('.FeedBackBtn')[0];
    $(divFeedBack).unbind('click');
    $(divFeedBack).click(function (event) {
        event.preventDefault();
        ShowOnProgressStatus(true);
        window.location = 'OrderFeedBack.aspx?OrderHeaderId=' + record.OrderHeaderId;
    });

    var divExpressTrack = $(templateCell).children('.ExpressTrackBtn')[0];
    $(divExpressTrack).unbind('click');
    $(divExpressTrack).click(function (event) {
        event.preventDefault();
        ShowOnProgressStatus(true);
        window.location = 'ExpressTrack.aspx?OrderId=' + record.OrderNumber;
    });

    // Show or Hide labels and buttons according selected mode.
    if (CopyOrderMode) {
        $(divCheckOut).addClass('hide');
    }
    else {
        $(divCheckOut).removeClass('hide');
    }

    if (record.ID > 0) {
        $(divCopyOrder).removeClass('hide');
        $(divExpressTrack).removeClass('hide');
    } else {
        $(divCopyOrder).addClass('hide');
        $(divExpressTrack).addClass('hide');
    }

    if (record.IsPaymentPending) {
        $(divCheckOut).removeClass('hide');
        $(divDelete).removeClass('hide');
        $(divCopyOrder).addClass('hide');
        $(divFeedBack).addClass('hide');
        $(divExpressTrack).addClass('hide');
    } else {
        $(divCheckOut).addClass('hide');
        $(divDelete).addClass('hide');
        $(divCopyOrder).removeClass('hide');
        $(divExpressTrack).removeClass('hide');
    }

    if (record.IsCopyEnabled) {
        $(divCopyOrder).removeClass('hide');
        $(divExpressTrack).removeClass('hide');
    } else {
        $(divCopyOrder).addClass('hide');
        $(divExpressTrack).addClass('hide');
    }

    if ((record.OrderStatus == OrderStatus_Description_CN_Complete) || (record.OrderStatus == OrderStatus_Description_CN_Complete2))
        $(divFeedBack).addClass('hide');
    else
        $(divFeedBack).removeClass('hide');

    if ((record.OrderStatus == OrderStatus_Description_CN_Payment_Pending) || (record.OrderStatus == OrderStatus_Description_CN_Status_Unknown))
        $(divExpressTrack).addClass('hide');
    else
        $(divExpressTrack).removeClass('hide');

    var divHistoricalData = $(templateCell).children('.HistoricalData')[0];
    if (record.IsHistoricalData) {
        $(divCheckOut).addClass('hide');
        $(divCopyOrder).addClass('hide');
        $(divDelete).addClass('hide');
        $(divExpressTrack).addClass('hide');
        $(divFeedBack).addClass('hide');
        $(divHistoricalData).removeClass('hide');
    }
    else {
        $(divHistoricalData).addClass('hide');

        if (record.HasEventItem) {
            $(divCopyOrder).addClass('hide');
        }
    }
}

// Confirm CheckOut action.
function ConfirmCheckOut() {
    window.location = 'ShoppingCart.aspx?CartID=' + GetCurrentShoppingCartID();
}

// Confirm copy order action.
function ConfirmCopyOrder() {
    ShowOnProgressStatus(true);
    $.ajaxPost("CopyOrder", "{ shoppingCartID : " + GetCurrentShoppingCartID() + ", ignoreCartNotEmpty : true }", CheckAndCopy);
}

// Check if user can copy an order, according the current shopping cart status.
function CheckAndCopy(result) {
    // If order has been copied successfully.
    if (result.d.Copied) {
       // $find(mdlClearCartID).hide();
        //$.ajaxPost("CartRetrieved", "{ cartId : " + result.d.ShoppingCartID + " }");
        copyOrderRedirect(result);
        //setTimeout(function() { alert('hello');}, 3000);
    }
        // If current shopping cart is not empty and has not been saved.
    else if (result.d.IsShoppingCartNotEmpty) {
        SetConfirmationPopUp('Copy', true);
        ShowOnProgressStatus(false);
        $find(mdlClearCartID).show();
    }
    else {
        SetConfirmationPopUp('Error', false);
        ShowOnProgressStatus(false);
        $find(mdlMessageID).show();
    }
}

function CancelCopy() {
    ShowOnProgressStatus(false);
    $('#' + pnlMessageID).addClass("hide");
    $find(mdlMessageID).hide();
}

// Confirm copy order action.
function copyOrderRedirect(result) {
    window.location = 'ShoppingCart.aspx?CartID=' + result.d.ShoppingCartID;
}

// Cancel delete action.
function CancelDelete() {
    ShowOnProgressStatus(false);
    $('#' + pnlDeleteCartID).addClass("hide");
    $find(mdlDeleteCartID).hide();
}

// On delete cart action.
function OnDeleteCart() {
    ShowOnProgressStatus(true);
    $('#' + pnlDeleteCartID).addClass("hide");
    $find(mdlDeleteCartID).hide();
    var orderNum = $('#' + hidPendingCartInfo).val();
    $.ajaxPost("DeleteCart", "{ cartID : " + GetCurrentShoppingCartID() + ",  orderNumber : '" + orderNum + "' }");
    window.location = 'Pricelist.aspx?ETO=FALSE';
    return false;
}

// Refresh the carts grid data with delay.
function RefreshCartsGridWithDelay() {
    setTimeout(RefreshCartsGrid, 2500)
}

// Refresh carts grid data.
function RefreshCartsGrid() {
    CartsGrid_Command($find(CartsGridClientID), null);
}

// Refresh carts grid data after the order by selection change.
function OrderBySelectedChanged(sender, eventArgs) {
    var item = eventArgs.get_item();
    orderBy = item.get_value();
    RefreshCartsGrid();
}

// Refresh carts grid data after the Order Status selection change.
function OrderStatusSelectedChanged(sender, eventArgs) {
    RefreshCartsGrid();
}

// Adding a row to a table.
function AddTableRow(values, i, id) {
    var r = new Array();
    var j = 0;
    r[++j] = '<tr>';
    for (var v = 0; v < values.length; v++) {
        r[++j] = AddTableCell(values[v], i, id);
    }
    r[++j] = '</tr>';
    return r.join('');
}

// Adding a cell to a row.
function AddTableCell(value, i, id) {
    var r = new Array();
    var j = 0;
    r[++j] = '<td ';
    r[++j] = (i > 2 ? ' class="moreRowsItem' + id + ' hide" ' : ' ');
    r[++j] = '><span>';
    r[++j] = value;
    r[++j] = '</span></td>';
    return r.join('');
}

// Removing hide class for the save cart popup.
function RemoveHideClass() {
    SetConfirmationPopUp('New', true);
}

// Check if user can check out a shopping cart, according the current shopping cart status.
function CheckCheckOutConfirmation(result) {
    // If current shopping cart is not empty.
    if (result.d) {
        SetConfirmationPopUp('CheckOut', true);
        ShowOnProgressStatus(false);
        $find(mdlClearCartID).show();
    }
        // Go to the check out page.
    else {
        $.ajaxPost("CartRetrieved", "{ cartId : " + GetCurrentShoppingCartID() + " }", ConfirmCheckOut);
    }
}

// Get current shopping cart id.
function GetCurrentShoppingCartID() {
    return $('#' + hidCartID).val();
}

// Set the current shopping cart id.
function SetCurrentShoppingCartID(id) {
    $('#' + hidCartID).val(id);
}

// Set the current shopping cart id.
function SetCurrentShoppingCartInfo(info) {
    $('#' + hidPendingCartInfo).val(info);
}

// Set the visibility of the button on the pop up for action confirmation.
var localAction;
function SetConfirmationPopUp(action, hideErrors) {
    localAction = action;
    $('#' + hidActionID).val(action);
    if (hideErrors) {
        $('.lblErrors').addClass('hide');
    }
    switch (action) {
        case 'CheckOut':
            $('#' + pnlClearCartID).removeClass("hide");
            $('#ContinueCo').hide();
            $('#ContinueDo').hide();
            $('#ContinueCh').show();
            break;
        case 'New':
            $('#' + pnlClearCartID).removeClass("hide");
            $('#ContinueDo').show();
            $('#ContinueCo').hide();
            $('#ContinueCh').hide();
            break;
        case 'Copy':
            $('#' + pnlClearCartID).removeClass("hide");
            $('#ContinueDo').hide();
            $('#ContinueCh').hide();
            $('#ContinueCo').show();
            break;
        case 'Error':
            $('#' + pnlMessageID).removeClass("hide");
            break;
    }
}

function GetYearMonth(uiObjId) {
    var val = $find(uiObjId).get_selectedDate();
    if (val == null) return null;

    var ret = val.customFormat("#YYYY##MM#");
    return ret;
}

// workaround telerik month-year control issue #140667
function fixRadMonthYearPicker() {
    fixRadMonthYearPicker_2(uiStartYearMonthID);
    fixRadMonthYearPicker_2(uiEndYearMonthID);
}
function fixRadMonthYearPicker_2(uiId) {
    var uiI = $("#" + uiId + "_dateInput_text");
    if (uiI != null) {
        var newId = uiId + "_fix";
        var newInput = $("<input type='text' id='" + newId + "' disabled style='width:" + (uiI.width() - 20) + "px' />");

        var newXId = uiId + "_X_fix";
        var newX = $("<div id='" + newXId + "' class='X-icon' >&nbsp;&nbsp;x&nbsp;&nbsp;</div>");
        newX.attr("linkId", uiId);
        newX.attr("fixerId", newId);
        newX.click(fixRadMonthYearPicker_X);

        var newParent = $("<div />");

        uiI.before(newParent);
        uiI.appendTo(newParent);

        newParent.before(newInput);
        newParent.before(newX);
        newParent.hide();
    }
}

function RadMonthYearPicker_OnDateSelected(sender, eventArgs) {
    var dt = eventArgs.get_newValue();
    if (dt.length > 6) dt = dt.substr(0, 6);

    var fixer = $("#" + sender._element.id + "_fix");
    fixer.prop("value", dt);
}

function fixRadMonthYearPicker_X() {
    var obj = $(this);
    var linkObj = $find(obj.attr("linkId"));
    linkObj.set_selectedDate(null);

    var fixer = $("#" + obj.attr("fixerId"));
    fixer.prop("value", "");
}

// dateTime to custom format string
// http://stackoverflow.com/a/4673990/782132
Date.prototype.customFormat = function (formatString) {
    var YYYY, YY, MMMM, MMM, MM, M, DDDD, DDD, DD, D, hhh, hh, h, mm, m, ss, s, ampm, AMPM, dMod, th;
    var dateObject = this;
    YY = ((YYYY = dateObject.getFullYear()) + "").slice(-2);
    MM = (M = dateObject.getMonth() + 1) < 10 ? ('0' + M) : M;
    MMM = (MMMM = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"][M - 1]).substring(0, 3);
    DD = (D = dateObject.getDate()) < 10 ? ('0' + D) : D;
    DDD = (DDDD = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"][dateObject.getDay()]).substring(0, 3);
    th = (D >= 10 && D <= 20) ? 'th' : ((dMod = D % 10) == 1) ? 'st' : (dMod == 2) ? 'nd' : (dMod == 3) ? 'rd' : 'th';
    formatString = formatString.replace("#YYYY#", YYYY).replace("#YY#", YY).replace("#MMMM#", MMMM).replace("#MMM#", MMM).replace("#MM#", MM).replace("#M#", M).replace("#DDDD#", DDDD).replace("#DDD#", DDD).replace("#DD#", DD).replace("#D#", D).replace("#th#", th);

    h = (hhh = dateObject.getHours());
    if (h == 0) h = 24;
    if (h > 12) h -= 12;
    hh = h < 10 ? ('0' + h) : h;
    AMPM = (ampm = hhh < 12 ? 'am' : 'pm').toUpperCase();
    mm = (m = dateObject.getMinutes()) < 10 ? ('0' + m) : m;
    ss = (s = dateObject.getSeconds()) < 10 ? ('0' + s) : s;
    return formatString.replace("#hhh#", hhh).replace("#hh#", hh).replace("#h#", h).replace("#mm#", mm).replace("#m#", m).replace("#ss#", ss).replace("#s#", s).replace("#ampm#", ampm).replace("#AMPM#", AMPM);
}