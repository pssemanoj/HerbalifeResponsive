$(function () {

    var lnkCartName = $('.lnkCartName');
    var pAddress = $('.pAddress');

    $(lnkCartName).click(function () {
        var _this = this;

        $(_this)
          .toggleClass('expanded')
          .parent()
          .find(pAddress)
          .toggle(10);

        var i = $(_this)
          .find('i')
          .toggleClass('icon-arrow-triangle-right');
    });

    var lnkTotalProducts = $('.lnkTotalProducts');
    $(lnkTotalProducts).click(function () {
        var _this = this;

        var itemsToShow = $(_this).attr('show');
        $(_this).parent().find('.' + itemsToShow).toggleClass('hide');
        
    });

    if (undefined != responsiveMode && responsiveMode == true) {

        $('#divGrid').css('width', ($(window).outerWidth() - $('.headcol').outerWidth()) - 10);

        $.each($(".headcol"), function (key, headcol) {
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
                .css({ "display": "block", "min-width": "84px" })
                .parent()
                .css({ "height": $(productsCont).outerHeight(), "border-right": 0, "border-top": 0 });
        });

        $("table thead tr th:nth-child(2)").css({
            "height": $("table thead tr th:nth-child(1)").outerHeight() + "px",
            "min-width": "118px"
        });

    }
});