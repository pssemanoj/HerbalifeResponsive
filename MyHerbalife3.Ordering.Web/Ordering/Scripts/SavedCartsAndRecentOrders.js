// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedCartsAndRecentOrders.js" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Handle the event of the saved carts and recent orders page.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// Load function.
$(function () {
    // Load the grid after the page render event.
    // Avoiding an error, executing something when the grid has not been rendered.
    setTimeout(LoadCartGridSettings, 2000);

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
    if (locale == "zh-CN") {
        $('#tabRecentOrders').hide();
    }

});

// Load carts grid function.
function LoadCartGridSettings() {
    try {
        // Search saved carts.
        var grid = $find(CartsGridClientID);
        if (grid) {
            // Show on progress status.
            ShowOnProgressStatus(true);
            // Load data from server.
            $.ajaxPost("GetData",
                    "{ startIndex : 0, maximumRows : " + grid.get_masterTableView().get_pageSize() + "," +
                        " sortExpressions : '" + orderBy + "', filterExpressions : '', " +
                        " copyOrderMode : " + CopyOrderMode + ", copyOrderIndex : " + CopyOrderIndex + "," +
                        " copyOrderMaxLength : " + CopyOrderMaxLength + " }",
                    updateGrid);
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
        $('#tabRecentOrders').css('cursor', 'pointer');
        $('#tabSavedCarts').css('cursor', 'pointer');
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
    //if (event) {
    //    event.returnValue = false;
    //}
}

// Setting the data source to the grid after the get data method successfully action.
function updateGrid(result) {
    var result = result.d;

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
    $('.hiddenClass').addClass('hide');

    // Show the grid after the data has been rendered.
    $(".RadGrid").show();

    return false;
}

// Carts grid command function .
function CartsGrid_Command(sender, args) {
    try {
        if (args) {
            args.set_cancel(true);
        }

        // Getting actual properties from the rad grid.
        var pageSize = sender.get_masterTableView().get_pageSize();
        var filterExpressions = $('#' + txtSearchID).val();
        var currentPageIndex = sender.get_masterTableView().get_currentPageIndex();
        var filterExpressionsAsSQL = filterExpressions.toString();
        var startIndex = currentPageIndex * pageSize;
        var fliter = filterExpressions == SearchText ? "" : filterExpressions;

        ShowOnProgressStatus(true);
        // Getting the new data, according the command.
        $.ajaxPost("GetData",
                    "{ startIndex : " + startIndex + ", maximumRows : " + pageSize + "," +
                        " sortExpressions : '" + orderBy + "', filterExpressions : '" + fliter + "', " +
                        " copyOrderMode : " + CopyOrderMode + ", copyOrderIndex : " + CopyOrderIndex + "," +
                        " copyOrderMaxLength : " + CopyOrderMaxLength + " }",
                    updateGrid);
    }
    catch (ex) {
        alert(ex);
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
    var lblAddressText = $(pAddress).children('[id$="lblAddressText"]')[0];

    $(pAddress).hide();
    $(lnkCartName).removeClass('expanded');

    // Ensure the elements exists.
    if (lnkCartName) {
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
            lnkCartName.innerHTML = record.DraftName;
        }

        $(lnkCartName).unbind('click');
        $(lnkCartName).click(function () {
            $(lnkCartName).toggleClass('expanded');
            $(pAddress).toggle(10);
            return false;
        });

        // Bounding elements.
        lblDate.innerHTML = record.Date;
        lblAddress.innerHTML = theAddress = record.Address ? record.Address : '';
        lblRecipient.innerHTML = record.Recipient ? record.Recipient : '';

        if (theAddress == '') {
            lblAddressText.remove();
        }

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
    var divDelete = $(templateCell).children('.deleteCartBtn')[0];
    var divCopyOrder = $(templateCell).children('.copyOrderBtn')[0];
    $(divCheckOut).unbind('click');
    $(divCheckOut).click(function () {
        SetCurrentShoppingCartID(record.ID);
        ShowOnProgressStatus(true);
        $.ajaxPost("ShoppingCartIsNotEmpty", "{ }", CheckCheckOutConfirmation);
        return false;
    });
    $(divDelete).unbind('click');
    $(divDelete).click(function () {
        $('#' + pnlDeleteCartID).removeClass("hide");
        $find(mdlDeleteCartID).show();
        SetCurrentShoppingCartID(record.ID);
        return false;
    });
    $(divCopyOrder).unbind('click');
    $(divCopyOrder).click(function () {
        SetCurrentShoppingCartID(record.ID);
        ShowOnProgressStatus(true);
        $.ajaxPost("CopyOrder", "{ shoppingCartID : " + record.ID + ", ignoreCartNotEmpty : false }", CheckAndCopy);
        return false;
    });

    // Show or Hide labels and buttons according selected mode.
    if (CopyOrderMode) {
        $(divCopyOrder).removeClass('hide');
        $(divDelete).addClass('hide');
        $(divCheckOut).addClass('hide');
    }
    else {
        $(divCopyOrder).addClass('hide');
        $(divDelete).removeClass('hide');
        $(divCheckOut).removeClass('hide');
    }
    
    
}

function IsDiscSku() {
    $.ajaxPost("IsDiscSku", "{ cartId : " + GetCurrentShoppingCartID() + "}", ConfirmCheckOutChina);
}
function SetDiscSkuMessage(d) {
    if (d === 1) {
        $('#DiscSku').show();
    }
    if (d === 2) {
        $('#DiscSkuP').show();
    }
    if (d === 3 ) {
        $('#DiscSkus').show();
    }
    ShowOnProgressStatus(false);
}

// Confirm CheckOut action.
function ConfirmCheckOut() {
    window.location = 'ShoppingCart.aspx?CartID=' + GetCurrentShoppingCartID();
}


// Confirm CheckOut action. China
function ConfirmCheckOutChina(result) {
    if (result.d !== 0) {
        SetDiscSkuMessage(result.d);
        
    } else {
        window.location = 'ShoppingCart.aspx?CartID=' + GetCurrentShoppingCartID();
    }
    
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
        $find(mdlClearCartID).hide();
        $.ajaxPost("CartRetrieved", "{ cartId : " + result.d.ShoppingCartID + " }", CheckNonResidentMessage(result.d.ShoppingCartID) );
        //window.location = 'ShoppingCart.aspx?CartID=' + result.d.ShoppingCartID;
    }
    // If current shopping cart is not empty and has not been saved.
    else if (result.d.IsShoppingCartNotEmpty) {
        SetConfirmationPopUp('Copy', true);
        ShowOnProgressStatus(false);
        $find(mdlClearCartID).show();
    }
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
    $.ajaxPost("DeleteCart", "{ cartID : " + GetCurrentShoppingCartID() + " }", RefreshCartsGridWithDelay);
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
    $('#DiscSku').hide();
    $('#DiscSkuP').hide();
    $('#DiscSkus').hide();
    // If current shopping cart is not empty.
    if (result.d) {
        SetConfirmationPopUp('CheckOut', true);
        ShowOnProgressStatus(false);
        $find(mdlClearCartID).show();
    }
    // Go to the check out page.
    else {
        if (IsChina != null && IsChina.toLowerCase().trim() === 'true') {
            $.ajaxPost("CartRetrieved", "{ cartId : " + GetCurrentShoppingCartID() + " }", IsDiscSku);
        } else {
            $.ajaxPost("CartRetrieved", "{ cartId : " + GetCurrentShoppingCartID() + " }", CheckNonResidentMessage(null));
        }
        
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

// Set the visibility of the button on the pop up for action confirmation.
var localAction;
function SetConfirmationPopUp(action, hideErrors) {
    localAction = action;
    $('#' + hidActionID).val(action);
    if (hideErrors) {
        $('.lblErrors').addClass('hide');
    }
    $('#' + pnlClearCartID).removeClass("hide");
    switch (action) {
        case 'CheckOut':
            $('#ContinueCo').hide();
            $('#ContinueDo').hide();
            $('#ContinueCh').show();
            break;
        case 'New':
            $('#ContinueDo').show();
            $('#ContinueCo').hide();
            $('#ContinueCh').hide();
            break;
        case 'Copy':
            $('#ContinueDo').hide();
            $('#ContinueCh').hide();
            $('#ContinueCo').show();
            break;
    }
}

function CheckNonResidentMessage(id) {
    if (id != null) {
        SetCurrentShoppingCartID(id);
    }
    $.ajaxPost("DisplayNonResidentMessage", "{ shoppingCartID : " + GetCurrentShoppingCartID() + " }",
        function (result) {
            if (result.d) {
                // Show Popup for NonResidents
                $('#' + pnlNonResidentMessageID).removeClass("hide");
                ShowOnProgressStatus(false);
                $find(mdlNonResidentMessageID).show();
            } else {
                ConfirmCheckOut();
            }
        });
}