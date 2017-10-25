function dynamicDividerLine_() {
    var tabsbox = findControl2('3tabsbox');
    var tab1col1 = findControl2('tab1-col1');
    var tab1divider = findControl2('tab1-divider');
    var tab1col2 = findControl2('tab1-col2');
    if (tabsbox != null && tab1col1 != null) {
        var tab1_col1_height = tab1col1.offsetHeight;
        var tab1_divider_height = tab1divider.offsetHeight;
        var tab1_col2_height = tab1col2.offsetHeight;

        if ((tab1_col1_height > 130) || (tab1_col2_height > 130)) {
            var tab1_tallest;
            if (tab1_col1_height > tab1_col2_height) {
                tab1_tallest = tab1_col1_height;
                tab1divider.style.height = tab1_tallest + 'px';
            }
            else if (tab1_col2_height > tab1_col1_height) {
                tab1_tallest = tab1_col2_height;
                tab1divider.style.height = tab1_tallest + 'px';
            }
        }
    }
}

function adjustDividerLineTab2_() {
    var tabsbox = findControl2('3tabsbox');
    var tab2col1 = findControl2('tab2-col1');
    var tab2divider = findControl2('tab2-divider');
    var tab2col2 = findControl2('tab2-col2');

    if (tabsbox && tab2col1 && tab2col2) {
        var tab2_col1_height = tab2col1.offsetHeight;
        var tab2_divider_height = tab2divider.offsetHeight;
        var tab2_col2_height = tab2col2.offsetHeight;

        if ((tab2_col1_height > 130) || (tab2_col2_height > 130)) {
            var tab2_tallest;
            if (tab2_col1_height > tab2_col2_height) {
                tab2_tallest = tab2_col1_height;
                tab2divider.style.height = tab2_tallest + 'px';
            }
            else if (tab2_col2_height > tab2_col1_height) {
                tab2_tallest = tab2_col2_height;
                tab2divider.style.height = tab2_tallest + 'px';
            }
        }
    }

    //Fix per defect 208190
    if ($('#tab2-col1 [id$="pDetails"]').html().trim()) {
        //Do nothing
    } else {
        $('#tab2-col1 [id$="lbDetails"]').css("display", "none");
        $('#tab2-col2').css("width", "auto");
    }

}

function adjustDividerLineTab3_() {
    var tabsbox = findControl2('3tabsbox');
    var tab3col1 = findControl2('tab3-col1');
    var tab3divider = findControl2('tab3-divider');
    var tab3col2 = findControl2('tab3-col2');

    if (tabsbox && tab3col1 && tab3col2) {
        var tab3_col1_height = tab3col1.offsetHeight;
        var tab3_divider_height = tab3divider.offsetHeight;
        var tab3_col2_height = tab3col2.offsetHeight;

        if ((tab3_col1_height > 130) || (tab3_col2_height > 130)) {
            var tab3_tallest;
            if (tab3_col1_height > tab3_col2_height) {
                tab3_tallest = tab3_col1_height;
                tab3divider.style.height = tab3_tallest + 'px';
            }
            else if (tab3_col2_height > tab3_col1_height) {
                tab3_tallest = tab3_col2_height;
                tab3divider.style.height = tab3_tallest + 'px';
            }
        }
    }
}

function ShowWrap(id) {
    var elem = findControl2('dynamic_wrap1');
    if (elem != null) {
        if ('dynamic_wrap1' != id) {
            elem.style.display = 'none';
        }
        else {
            elem.style.display = 'inline';
        }
    }

    elem = findControl2('dynamic_wrap2');
    if (elem != null) {
        if ('dynamic_wrap2' != id) {
            elem.style.display = 'none';
        }
        else {
            elem.style.display = 'inline';
        }
    }

    elem = findControl2('dynamic_wrap3');
    if (elem != null) {
        if ('dynamic_wrap3' != id) {
            elem.style.display = 'none';
        }
        else {
            elem.style.display = 'inline';
        }
    }
}

function findControl(id) {
    return document.getElementById(id);
}

var controlFound = null;
function findControl2(id) {
    controlFound = null;
    var all = document.getElementsByTagName("div");
    var l = all.length;
    var i;
    for (i = 0; i < l; i++) {
        if (all[i].id == id) {
            controlFound = all[i];
        }
    }
    return controlFound;
}

function switchingTab(id) {
    hideAllWraps();
    ShowWrap('dynamic_wrap' + id);

    if (id == '1') {
        $('#vis2').addClass('unFocusTab');
        $('#vis3').addClass('unFocusTab');
        $('#vis1').addClass('focusTab');
        $('#vis2').removeClass('focusTab');
        $('#vis3').removeClass('focusTab');
        $('#vis1').removeClass('unFocusTab');
    }
    else if (id == '2') {
        $('#vis1').addClass('unFocusTab');
        $('#vis3').addClass('unFocusTab');
        $('#vis2').addClass('focusTab');
        $('#vis1').removeClass('focusTab');
        $('#vis3').removeClass('focusTab');
        $('#vis2').removeClass('unFocusTab');
    }
    else if (id == '3') {
        $('#vis1').addClass('unFocusTab');
        $('#vis2').addClass('unFocusTab');
        $('#vis3').addClass('focusTab');
        $('#vis1').removeClass('focusTab');
        $('#vis2').removeClass('focusTab');
        $('#vis3').removeClass('unFocusTab');
    }
}