var _AT = _AT || {}; (function () { _AT.config = o({ atServer: "cdn.tt.omtrdc.net", atLocation: "/cdn", checkAllowedOrigin: true }, _AT.config); var c = { AT_CONNECT: "at-connect", TARGET_HANDSHAKE_ACK: "targetjsHandShakeAck", TARGET_HANDSHAKE: "targetjsHandShake" }; function o() { var t = {}, r; for (r = 0; r < arguments.length; r++) { var s = arguments[r]; for (var u in s) { t[u] = s[u] } } return t } function b(u, t, s) { var r = s; if (u.attachEvent) { r = function () { s.call(u, window.event) }; u.attachEvent("on" + t, s) } else { if (u.addEventListener) { u.addEventListener(t, r, false) } } return r } function i(t, s, r) { if (t.detachEvent) { t.detachEvent("on" + s, r) } else { t.removeEventListener(s, r, false) } } function a(t, s) { var r = document.createElement("script"); r.type = "text/javascript"; r.src = t; if (typeof s === "function") { r.onload = s } q(r) } function f(s, t) { var r = document.createElement("link"); r.rel = "stylesheet"; r.type = "text/css"; r.href = s; if (typeof t === "function") { r.onload = t } q(r) } function q(s) { var r = (document.getElementsByTagName("head") || document.getElementsByTagName("body"))[0]; r.appendChild(s) } function n(s) { var u = /^http[s]?\:\/\/(.+)\.(marketing[-]?.+com)$/gi, r = ["http://marketing-qa11.qa.testandtarget.omniture.com", "https://marketing-qa12.corp.adobe.com"], x = u.exec(s), v, t, w = ["marketing-dev.corp.adobe.com", "marketing-qa.corp.adobe.com", "marketing-qa12.corp.adobe.com", "marketing-stage.adobe.com", "marketing-beta.adobe.com", "marketing.adobe.com"]; if (r.indexOf(s) !== -1) { return true } if (x === null || x.length < 3) { return false } v = x[1]; t = x[2]; return !(w.indexOf(t) === -1 || !(v.match(/[\.|\/]/ig) === null)) } function m(r) { if (r.updatePageURL && r.pageURL.indexOf("javascript:") === -1) { window.location.href = r.pageURL } } function e() { window.cdq.host.enableLogger(_AT.cdqConfig.isLoggerEnabled) } function l() { var r = window._AT; e(); window.cdq.host.log("debug", "Dependencies loaded!"); window.cdq.host.log("debug", "trying to connect"); window.cdq.host.connect(r.connectEvent) } function g() { k("/adobetarget/admin"); j("/target-vec-helper", l) } function k(r, s) { f("//" + _AT.config.atServer + _AT.config.atLocation + r + ".css", s) } function j(r, s) { a("//" + _AT.config.atServer + _AT.config.atLocation + r + ".js", s) } function p(w) { var r = w.origin || "", v, u = window._AT; if (u.config.checkAllowedOrigin && !n(r)) { return } if (typeof w.data !== "string") { return } try { v = (u.JSON || JSON).parse(w.data) } catch (t) { console.error("Invalid JSON in message data, ignored", t); return } if (v.action === c.TARGET_HANDSHAKE_ACK) { m(v.config) } if (v.action === c.AT_CONNECT) { u.connectEvent = {}; u.cdqConfig = {}; for (var s in w) { if (w[s]) { u.connectEvent[s] = w[s] } } for (var s in v.config) { if ({}.hasOwnProperty.call(v.config, s)) { u.cdqConfig[s] = v.config[s] } } g(); i(window, "message", p) } } var d = document.createElement("iframe"); b(d, "load", function () { if (!_AT.JSON) { _AT.JSON = d.contentWindow.JSON } document.head.removeChild(d) }); d.style.display = "none"; if (document.domain !== window.location.host) { d.src = "javascript:'<script>window.onload=function(){document.write(\\'<script>document.domain=\\\"" + document.domain + "\\\";<\\\\/script>\\');document.close();};<\/script>'" } document.head.appendChild(d); if (typeof _AT.eventListenerAdded == "undefined") { b(window, "message", p); _AT.eventListenerAdded = true } if (window != top) { var h = _AT.JSON || JSON; if (typeof h !== "undefined") { window.parent.postMessage(h.stringify({ action: c.TARGET_HANDSHAKE, pageURL: window.location.toString(), isAdmin: window.location.toString().indexOf("mboxEdit=1") !== -1 }), "*") } } })();