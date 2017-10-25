var ExpressTrackingJSON = null;

function GetExpressTrackInfo(expressCode, expressNo) {
    $("#EDIdetail").show();

    ShowOnProgressStatus(true);

    $.ajax({
        type: "POST",
        url: '/Ordering/ExpressTrack.aspx/GetExpressTrackInfo',
        data: "{ expressCode : '" + expressCode + "', expressNo : '" + expressNo + "' }",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: updateResult,
        error: function showError(xhr, status, exc) { location.reload(true); }
    });
}

function ShowOnProgressStatus(onProgress) {
    var uiTgt = $("#EDIdetail");
    // If page is on progress, show loading panel.
    if (onProgress) {
        $find(RadAjaxLoadingPanelClientID).show(uiTgt);
        $('body').css('cursor', 'wait');
    }
        // If page is not on progress, hide loading panel.
    else {
        $find(RadAjaxLoadingPanelClientID).hide(uiTgt);
        $('body').css('cursor', 'auto');
    }
}

function updateResult(result) {
    ShowOnProgressStatus(false);

    var rslt = result.d;

    if ((rslt != null) && (rslt.Result != "false")) {
        ExpressTrackingJSON = JSON.parse(rslt.Result);

        RenderTracking(true, rslt.ExpressCode, rslt.ExpressNo, "More");
        return;
    }

    RenderTracking2("");
}

function RenderTracking(queryEDI, expressCode, expressNo, type) {

    if (!queryEDI) {
        $("#EDIdetail").hide();
        return;
    }

    $("#EDIdetail").show();

    var obj = ExpressTrackingJSON;

    var html = "";
    try {
        var steps = "";
        var length = 0;
        if (expressCode.toLocaleLowerCase() == "yunda") {
            //韵达
            if (obj.result.toLocaleLowerCase() != "false") {
                steps = obj.steps;
                length = steps.length;
                if (length == 0) {
                    html = Html_NoExpressTrackingDetails();
                    return html;
                }
                if (length > 3) {
                    if (type == "Shrink") {
                        length = 3;
                    }
                }
                for (var i = 0; i < length; i++) {
                    if (i == steps.length - 1) {
                        html += "<span style='color:green;'>" + steps[i].time + "  " + steps[i].address + "：  " + steps[i].remark + "</span>";
                    } else {
                        html += steps[i].time + "  " + steps[i].address + "：  " + steps[i].remark + "</br>";
                    }
                }
            } else {
                html = Html_ExpressTrackingError(obj.remark);
            }
        } else if (expressCode.toLocaleLowerCase() == "fedex") {
            //联邦
            if (!obj.tracking || !obj.tracking.detail) {
                html = Html_NoExpressTrackingDetails();
                return html;
            }
            steps = obj.tracking.detail.activities.activity;
            length = steps.length;
            if (length > 3) {
                if (type == "Shrink") {
                    length = 3;
                }
            }
            for (var i = 0; i < length; i++) {
                if (i == 0) {
                    html += "<span style='color:green;'>" + steps[i].datetime + "  " + steps[i].location + "：  " + steps[i].scan + "</span></br>";
                } else {
                    html += steps[i].datetime + "  " + steps[i].location + "：  " + steps[i].scan + "</br>";
                }
            }
        } else if (expressCode.toLocaleLowerCase() == "sf") {
            //顺丰
            if (obj.Response.Body == null || !obj.Response.Body.RouteResponse) {
                html = Html_NoExpressTrackingDetails();
                return html;
            }
            steps = obj.Response.Body.RouteResponse.Route;
            length = steps.length;
            if (length > 3) {
                if (type == "Shrink") {
                    length = 3;
                }
            }
            for (var i = 0; i < length; i++) {
                if (i == steps.length - 1) {
                    html += "<span style='color:green;'>" + steps[i].accept_time + "  " + steps[i].accept_address + "：  " + steps[i].remark + "</span>";
                } else {
                    html += steps[i].accept_time + "  " + steps[i].accept_address + "：  " + steps[i].remark + "</br>";
                }
            }
        } else if (expressCode.toLocaleLowerCase() == "bestway") {
            //百世汇通
            //判断obj.success是否存在，若在则说明查询出错：!obj.success == 'true'表明不存在
            if (!!obj.success) {
                html = Html_ExpressTrackingError(obj.reason);
                return html;
            }
            if (!obj.traceLogs[0] || !obj.traceLogs[0].traces) {
                html = Html_NoExpressTrackingDetails();
                return html;
            }
            steps = obj.traceLogs[0].traces;
            length = steps.length;
            if (length > 3) {
                if (type == "Shrink") {
                    length = 3;
                }
            }
            for (var i = 0; i < length; i++) {
                if (i == steps.length - 1) {
                    html += "<span style='color:green;'>" + steps[i].acceptTime + "  " + steps[i].acceptAddress + "：  " + steps[i].remark + "</span>";
                } else {
                    html += steps[i].acceptTime + "  " + steps[i].acceptAddress + "：  " + steps[i].remark + "</br>";
                }
            }
        }
        if (steps.length > 3) {
            if (type == "Shrink") {
                html += "<div class='edimore' onclick='RenderTracking(" + queryEDI + ", \"" + expressCode + "\", \"" + expressNo + "\",\"More\")'>" + Txt_MoreDetails + "</div>";
            } else {
                html += "<div class='edimore' onclick='RenderTracking(" + queryEDI + ", \"" + expressCode + "\", \"" + expressNo + "\",\"Shrink\")'>" + Txt_LessDetails + "</div>";
            }
        }
    } catch (err) {
    }

    RenderTracking2(html);
}

function Html_NoExpressTrackingDetails() {
    return "<label style='color:red;'>" + Txt_NoExpressTrackingDetails + "</label>";
}
function Html_ExpressTrackingError(txt) {
    var html = "<label style='color:red;'>" + Txt_ExpressTrackingError + "</label>";
    return html.format(txt);
}

function RenderTracking2(html) {
    if (html == "") html = Html_NoExpressTrackingDetails();

    var uiTgt = $("#EDIContent");
    uiTgt.html(html);

    var h = "<div class='detailLogo'>"
         + "<img src='/Ordering/Images/China/1_03.png' width='11' height='15' alt='提示' title='提示' style='vertical-align:middle;margin-right:10px;' />"
         + Txt_ContactUsForExpressIssue + "</div>";
    $(h).appendTo(uiTgt);
}