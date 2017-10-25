// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductBySKU.js" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used provide the client side functionallity for order by sku page.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// Store the auto complete options.
var autoCompleteOptions = null;

// Store the sku's list.
var skuList = null;


function AddRemoveFavouriteClick(DisID, favID, idSKU, pID, id2, bolDEL) {

    var isDel = 0;
    var img = $('#' + favID).attr('src');

    if (img.indexOf('_on') > 0) isDel = 1;

    $.ajax({
        type: "POST",
        url: "Pricelist.aspx/SetFavouriteSKU",
        data: "{'DisID':'" + DisID + "','ProdID':" + pID + ",'ProdSKU':'" + idSKU + "', 'bolDEL':" + isDel + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccess(favID, img),
        failure: function (response) {
            alert(response.d);
        }
    });

    return false;
}

function OnSuccess(favID, img) {

    if (img.indexOf('_on') > 0)
        $('#' + favID).attr('src', $('#' + favID).attr('src').replace('_on', '_off'));
    else
        $('#' + favID).attr('src', $('#' + favID).attr('src').replace('_off', '_on'));

    var path = $('#' + favID).attr('src');
}
