var inventoryDetails = {
    Init: function () {
        if ($('#inventoryGrid').length > 0) {
            var detailsModel = kendo.observable(new inventoryDetails.DetailsviewModel());
            kendo.bind($('#inventoryGrid'), detailsModel);
            detailsModel.loadInventory();
        }
        
        //detailsModel.loadErrorMessages();
    },
    
    DetailsviewModel: function () {
        this.Location = null;
        this.ListEmpty = false;
        this.details = [];
        this.isLoading = false;
        this.isLoaded = true;
        
        this.loadInventory = function() {
            var self = this;
            $("#principalContent").css('position', 'relative');
            kendo.ui.progress($("#principalContent"), true);
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                url: '/Ordering/api/BackOrderDetail/GetInventoryDetails',
                success: function(data) {
                    self.dataHandler(data);
                },
                error: function() {
                    self.error = true;
                },
                complete: function() {
                    $("#principalContent").css('position', 'static');
                    kendo.ui.progress($("#principalContent"), false);
                }
            });
        };

        this.dataHandler = function (data) {
            if (data) {

                //Let's generate the show data frame
                drawShowData(data.WhDetails, data.HasShipping);

                //Let's generate the table
                drawTable(data);

                if (this.details.length == 0)
                    this.set("ListEmpty", true);
                else
                    this.set("ListEmpty", false);
            }
        };

    }
        
};

$(function () {
    inventoryDetails.Init();
});

function drawShowData(dataWareHouses, hasShipping) {
    var wareHouse = $("<tr />");
    
    $("#showDataGrid").append(wareHouse);

    for (var x = 0; x < dataWareHouses.length; x++) {
        if (hasShipping == true) {
            if (dataWareHouses[x].Option == "Shipping") {
                wareHouse.append($("<td><input id='dwhouse" + x + "' type='checkbox' name='dwhouse' checked='checked' /> " + dataWareHouses[x].Name + " (" + dataWareHouses[x].OptionText + ")</input></td>"));
            } else {
                wareHouse.append($("<td><input id='dwhouse" + x + "' type='checkbox' name='dwhouse'/> " + dataWareHouses[x].Name + " (" + dataWareHouses[x].OptionText + ")</input></td>"));
            }
        } else {
            wareHouse.append($("<td><input id='dwhouse" + x + "' type='checkbox' name='dwhouse' checked='checked' /> " + dataWareHouses[x].Name + " (" + dataWareHouses[x].OptionText + ")</input></td>"));
        }
    }
}

function drawTable(data) {

    //Let's draw the header
    var rowHeader = $("<tr />");
    $("#inventoryGrid").append(rowHeader);

    rowHeader.append($("<th class='col-SKU'>" + SkuColumnTitleText + "</th>"));
    rowHeader.append($("<th class='col-Product'>" + DescriptionColumnText + "</th>"));
    
    for (var x = 0; x < data.WhDetails.length; x++) {
        if (data.HasShipping == true) {
            if (data.WhDetails[x].Option == "Shipping") {
                rowHeader.append($("<th reference=dwhouse" + x + ">" + data.WhDetails[x].Name + "</th>"));
            } else {
                rowHeader.append($("<th reference=dwhouse" + x + ">" + data.WhDetails[x].Name + "</th>").hide());
            }
        } else {
            rowHeader.append($("<th reference=dwhouse" + x + ">" + data.WhDetails[x].Name + "</th>"));
        }
    }

    //Let's draw the subheader
    var rowsubHeader = $("<tr />");
    $("#inventoryGrid").append(rowsubHeader);

    rowsubHeader.append($("<th class='invSubHeader'></th>"));
    rowsubHeader.append($("<th class='invSubHeader'></th>"));

    for (var x = 0; x < data.WhDetails.length; x++) {
        if (data.HasShipping == true) {
            if (data.WhDetails[x].Option == "Shipping") {
                rowsubHeader.append($("<th class='invSubHeader' reference=dwhouse" + x + ">" + data.WhDetails[x].OptionText + "</th>"));
            } else {
                rowsubHeader.append($("<th class='invSubHeader' reference=dwhouse" + x + ">" + data.WhDetails[x].OptionText + "</th>").hide());
            }
        //} else {
        //    //rowsubHeader.append($("<th class='invSubHeader' reference=dwhouse" + x + ">" + data.WhDetails[x].OptionText + "</th>"));
        }
    }


    //Let's draw the body
    for (var x = 0; x < data.FullCatalog.length; x++) {
        var row = $("<tr rowRef=" + x + " />");
        $("#inventoryGrid").append(row);

        row.append($("<td class='col-SKU'>" + data.FullCatalog[x].SKU + "</td>"));
        row.append($("<td class='col-Product'>" + data.FullCatalog[x].SKUDescription + "</td>"));

        //Let's compare the dataWareHouse position against the location on the Full Catalog array. If they match, we placed it in a new array with the corresponding position
        var skuInfo = new Array(data.WhDetails.length);
        var skuStatus;
        
        for (var y = 0; y < data.FullCatalog[x].InventoryDetails.length; y++) {    
            for (var z = 0; z < data.WhDetails.length; z++) {
                if ((data.FullCatalog[x].InventoryDetails[y].WHCode == data.WhDetails[z].WHCode) && (data.FullCatalog[x].InventoryDetails[y].DeliveryType == data.WhDetails[z].Option) && (data.FullCatalog[x].InventoryDetails[y].Location == data.WhDetails[z].Name)) {
                    var img;
                    var link = true;
                    var skuStatus = false;

                    if (((data.FullCatalog[x].InventoryDetails[y].Status == "Unavailable") || (data.FullCatalog[x].InventoryDetails[y].Status == null)) && (data.FullCatalog[x].InventoryDetails[y].DeliveryType == data.WhDetails[z].Option)) {
                        img = "<img src='/Content/Global/Products/Img/circle_red.gif' />";
                    } else if ((data.FullCatalog[x].InventoryDetails[y].Status == "Available") && (data.FullCatalog[x].InventoryDetails[y].DeliveryType == data.WhDetails[z].Option)) {
                            img = "<img src='/Content/Global/Products/Img/circle_green.gif' />";
                            link = false;
                            skuStatus = true;
                    } else if ((data.FullCatalog[x].InventoryDetails[y].Status == "AllowBackOrder") && (data.FullCatalog[x].InventoryDetails[y].DeliveryType == data.WhDetails[z].Option)) {
                            img = "<img src='/Content/Global/Products/Img/circle_orange.gif' />";
                            link = false;
                            skuStatus = true;
                    } else if ((data.FullCatalog[x].InventoryDetails[y].Status == "UnavailableInPrimaryWh") && (data.FullCatalog[x].InventoryDetails[y].DeliveryType == data.WhDetails[z].Option)) {
                            img = "<img src='/Content/Global/Products/Img/circle_blue.gif' />";
                            link = false;
                            skuStatus = true;
                    } else {
                            img = data.FullCatalog[x].InventoryDetails[y].Status;
                            link = false;
                    }

                    if (link) {
                        skuInfo[z] = img + "<div class='ttip'><a href='#' location='" + data.FullCatalog[x].InventoryDetails[y].Location + "' dateAvailability='" + data.FullCatalog[x].InventoryDetails[y].AvailabilityDate + "' type='" + data.FullCatalog[x].InventoryDetails[y].Type + "' tooltipcomments='" + data.FullCatalog[x].InventoryDetails[y].ToolTipComments + "' comments='" + data.FullCatalog[x].InventoryDetails[y].Comments + "' x='" + x + "' z='" + z + "'>" + StatusText + "</a></div>";
                    } else {
                        skuInfo[z] = img;
                    }
                } 
                continue;
            }
        }

        //Let's draw the row with the position values correct
        for (var i = 0; i < skuInfo.length; i++) {
            if (typeof skuInfo[i] !== "undefined") {
                if (data.HasShipping == true) {
                    if (data.WhDetails[i].Option == 'Shipping') {
                        row.append($("<td reference=dwhouse" + i + ">" + skuInfo[i] + "</td>").css('background-color', '#f3f3f3'));
                    } else {
                        row.append($("<td reference=dwhouse" + i + ">" + skuInfo[i] + "</td>").hide());
                    }
                } else {
                    row.append($("<td reference=dwhouse" + i + ">" + skuInfo[i] + "</td>").css('background-color', '#f3f3f3'));
                }
            } else {
                row.append($("<td reference=dwhouse" + i + "><img src='/Content/Global/Products/Img/circle_red.gif' /></td>"));
            }
        }
    }
}

$(document).ready(function () {
    
    $("input[name='dwhouse']").live('click', function () {
        if ($(this).prop("checked") == true) {
            $('#inventoryGrid [reference="' + this.id + '"]').show();
        } else {
            $('#inventoryGrid [reference="' + this.id + '"]').hide();
        }
    });

    $(function () {
        $(".ttip a").live('click', function ( event ) {
            event.preventDefault();
            var location = $(this).attr('location');
            var dateAvailability = $(this).attr('dateAvailability');
            var type = $(this).attr('type');
            var tooltipcomments = $(this).attr('tooltipcomments');
            var comments = $(this).attr('comments');
            var row = $(this).attr('x');
            var cell = $(this).attr('z');

            $("#inventoryGrid tr[rowRef] td[reference] div.ttip > div").hide();
                        
            var tooltipContent = $("<div />");
            $("#inventoryGrid tr[rowRef=" + row + "] td[reference='dwhouse" + cell + "'] div.ttip").append(tooltipContent);
            tooltipContent.append($("<div class='tooltipContent'>" + location + "<br /> " + type + " <br /> " + dateAvailability + " <br /> " + comments + " <br /> " + tooltipcomments + " </div>"));

            $("#inventoryGrid tr[rowRef] td[reference] div.ttip > div").hide();
            $("#inventoryGrid tr[rowRef=" + row + "] td[reference='dwhouse" + cell + "'] div.ttip > div").show();
        });
    });
});


$(document).mouseup(function (e) {
    
    var container = $("#inventoryGrid tr[rowRef] td[reference] div.ttip > div");

    if (!container.is(e.target) // if the target of the click isn't the container...
        && container.has(e.target).length === 0) // ... nor a descendant of the container
    {
        if (container.is(':visible')) {
            container.hide();
        }
    }
});