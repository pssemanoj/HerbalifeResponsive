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

$(function () {
    setTimeout(InitializeControls, 1000);
});

// Initialize all products and quantity controls.
function InitializeControls() {
    if ($('#OrderBySkuMain .productInput').length > 0) {
        // Fix
        $('.onlyNumbers').addClass('qtyInput');

        // Set numeric inputs.
        $('.qtyInput').setNumeric2();

        // Get product information list.
        $.ajaxPost("GetAutoCompleteOptions", "{ }", SetAutoCompleteOptions);

        // Set the functionality to replace the text with the sku.
        $('.productInput').blur(function () {
            // Validate if the sku result is in the options list.
            if (this.value) {
                var isProduct = $.inArray(this.value, autoCompleteOptions) >= 0;
                var isSku = $.inArray(this.value, skuList) >= 0;
                if (isProduct || isSku) {
                    // Leave only the sku.
                    if (isProduct) {
                        // $(this).val(this.value.substring(0, this.value.indexOf(' ')));
                    }

                    // Removing error image.
                    $(this).parent().prev().find('img').addClass('hide');

                    // Adding complete class.
                    $(this).addClass('completeSKUInput');

                    // Send row for review.
                    IsRowComplete($(this), $(this).parent().parent().find('.qtyInput'), false);
                } else {
                    // If any autocomplete box is showed for peding user selection.
                    if ($('.ui-autocomplete:visible').length == 0) {

                        // SKU/Product does not exist.
                        $('.errorList').html(noSKUFoundText.format(this.value.toUpperCase()));
                        $(this).parent().prev().find('.gdo-error-message-icon').removeClass('hide');

                        // Removing complete class.
                        $(this).removeClass('completeSKUInput');

                        // Adding error image.
                        $(this).parent().prev().find('img').removeClass('hide');
                    }
                }
            }
        });

        // Removing complete class if focus.
        $('.productInput').focus(function () {
            $(this).removeClass('completeSKUInput');
        });

        // Send row for review.
        $('.qtyInput').blur(function () {
            IsRowComplete($(this).parent().parent().find('.productInput'), $(this), true);
        });

        // Find all inputs with errors and show them.
        $('.productInput').each(function () {
            if (this.value) {
                $(this).parent().parent().removeClass('hide');
                $(this).parent().prev().find('img').removeClass('hide');
            }
        });
    }
}

// Set auto complete options for the product inputs.
function SetAutoCompleteOptions(result) {
    if (result && result.d) {
        autoCompleteOptions = result.d.ProductsList;
        skuList = result.d.SkuList;
        InitializeAutocompleteOptions();
    }
}

// Initialize autocomplete options.
function InitializeAutocompleteOptions() {
    if (autoCompleteOptions) {
        $('.productInput').kendoAutoComplete({
            dataSource: autoCompleteOptions,
            filter: "contains",
            placeholder: autoPlaceHolderText
        });
    }

    // Offset for the current sku's (errored)
    var visibleIndex = 0;
    $.each($('.productInput'), function (i, v) {
        if (v.value) {
            //$('.productInput').eq(visibleIndex).val(v.value);
            //$('.productInput').eq(visibleIndex).parent().parent().removeClass('hide');
            //visibleIndex++;
            //v.value = '';
        }
    });
}

// Review if the row is complete.
function IsRowComplete(productSku, quantity, focusOnNext) {
    var sku = productSku.eq(2).val() ? productSku.eq(2).val() : productSku.eq(1).val();
    var qty = quantity.val();

    // Not empty.
    if (sku && qty) {
        productSku.parent().parent().next().removeClass('hide');

        // Focus on the next product input.
        if (focusOnNext) {
            productSku.parent().parent().next().find('.productInput').focus();
        }
    }
}