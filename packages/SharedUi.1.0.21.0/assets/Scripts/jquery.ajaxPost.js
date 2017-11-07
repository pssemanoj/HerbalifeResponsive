// --------------------------------------------------------------------------------------------------------------------
// <copyright file="jquery.ajaxPost.js" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Handle the json ajax posts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

$.ajaxPost = function (method, data, success, error) {
    $.ajax({
        type: "POST",
        url: window.location.href.replace('#', '') + "/" + method,
        data: data,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: success,
        error: function showError(xhr, status, exc) { location.reload(true); }
    });
};