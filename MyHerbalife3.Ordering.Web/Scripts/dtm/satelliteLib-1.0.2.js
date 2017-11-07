/*
 ============== DO NOT ALTER ANYTHING BELOW THIS LINE ! ============

 Adobe Visitor API for JavaScript version: 1.6.0
 Copyright 1996-2015 Adobe, Inc. All Rights Reserved
 More info available at http://www.omniture.com
*/
function Visitor(t, e) {
    if (!t) throw "Visitor requires Adobe Marketing Cloud Org ID";
    var n = this;
    n.version = "1.6.0";
    var i = window,
        a = i.Visitor;
    a.version = n.version, i.s_c_in || (i.s_c_il = [], i.s_c_in = 0), n._c = "Visitor", n._il = i.s_c_il, n._in = i.s_c_in, n._il[n._in] = n, i.s_c_in++, n.ia = {
        Ca: []
    };
    var r = i.document,
        o = a.pb;
    o || (o = null);
    var s = a.qb;
    s || (s = void 0);
    var c = a.Ia;
    c || (c = A);
    var u = a.Ga;
    u || (u = B), n.ea = function (t) {
        var e, n, i = 0;
        if (t)
            for (e = 0; e < t.length; e++) n = t.charCodeAt(e), i = (i << 5) - i + n, i &= i;
        return i
    }, n.v = function (t) {
        var e, n = "0123456789",
            i = "",
            a = "",
            r = 8,
            o = 10,
            s = 10;
        if (1 == t) {
            for (n += "ABCDEF", t = 0; 16 > t; t++) e = Math.floor(Math.random() * r), i += n.substring(e, e + 1), e = Math.floor(Math.random() * r), a += n.substring(e, e + 1), r = 16;
            return i + "-" + a
        }
        for (t = 0; 19 > t; t++) e = Math.floor(Math.random() * o), i += n.substring(e, e + 1), 0 == t && 9 == e ? o = 3 : (1 == t || 2 == t) && 10 != o && 2 > e ? o = 10 : t > 2 && (o = 10), e = Math.floor(Math.random() * s), a += n.substring(e, e + 1), 0 == t && 9 == e ? s = 3 : (1 == t || 2 == t) && 10 != s && 2 > e ? s = 10 : t > 2 && (s = 10);
        return i + a
    }, n.Ka = function () {
        var t;
        if (!t && i.location && (t = i.location.hostname), t)
            if (/^[0-9.]+$/.test(t)) t = "";
            else {
                var e = t.split("."),
                    n = e.length - 1,
                    a = n - 1;
                if (n > 1 && 2 >= e[n].length && (2 == e[n - 1].length || 0 > ",ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,ax,az,ba,bb,be,bf,bg,bh,bi,bj,bm,bo,br,bs,bt,bv,bw,by,bz,ca,cc,cd,cf,cg,ch,ci,cl,cm,cn,co,cr,cu,cv,cw,cx,cz,de,dj,dk,dm,do,dz,ec,ee,eg,es,et,eu,fi,fm,fo,fr,ga,gb,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gw,gy,hk,hm,hn,hr,ht,hu,id,ie,im,in,io,iq,ir,is,it,je,jo,jp,kg,ki,km,kn,kp,kr,ky,kz,la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly,ma,mc,md,me,mg,mh,mk,ml,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,na,nc,ne,nf,ng,nl,no,nr,nu,nz,om,pa,pe,pf,ph,pk,pl,pm,pn,pr,ps,pt,pw,py,qa,re,ro,rs,ru,rw,sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sx,sy,sz,tc,td,tf,tg,th,tj,tk,tl,tm,tn,to,tp,tr,tt,tv,tw,tz,ua,ug,uk,us,uy,uz,va,vc,ve,vg,vi,vn,vu,wf,ws,yt,".indexOf("," + e[n] + ",")) && a--, a > 0)
                    for (t = ""; n >= a;) t = e[n] + (t ? "." : "") + t, n--
            }
        return t
    }, n.cookieRead = function (t) {
        var t = encodeURIComponent(t),
            e = (";" + r.cookie).split(" ").join(";"),
            n = e.indexOf(";" + t + "="),
            i = 0 > n ? n : e.indexOf(";", n + 1);
        return 0 > n ? "" : decodeURIComponent(e.substring(n + 2 + t.length, 0 > i ? e.length : i))
    }, n.cookieWrite = function (t, e, i) {
        var a, o = n.cookieLifetime,
            e = "" + e,
            o = o ? ("" + o).toUpperCase() : "";
        return i && "SESSION" != o && "NONE" != o ? (a = "" != e ? parseInt(o ? o : 0, 10) : -60) ? (i = new Date, i.setTime(i.getTime() + 1e3 * a)) : 1 == i && (i = new Date, a = i.getYear(), i.setYear(a + 2 + (1900 > a ? 1900 : 0))) : i = 0, t && "NONE" != o ? (r.cookie = encodeURIComponent(t) + "=" + encodeURIComponent(e) + "; path=/;" + (i ? " expires=" + i.toGMTString() + ";" : "") + (n.cookieDomain ? " domain=" + n.cookieDomain + ";" : ""), n.cookieRead(t) == e) : 0
    }, n.g = o, n.K = function (t, e) {
        try {
            "function" == typeof t ? t.apply(i, e) : t[1].apply(t[0], e)
        } catch (n) { }
    }, n.Oa = function (t, e) {
        e && (n.g == o && (n.g = {}), n.g[t] == s && (n.g[t] = []), n.g[t].push(e))
    }, n.q = function (t, e) {
        if (n.g != o) {
            var i = n.g[t];
            if (i)
                for (; 0 < i.length;) n.K(i.shift(), e)
        }
    }, n.J = function (t, e) {
        var n, i = encodeURIComponent("d_fieldgroup") + "=" + encodeURIComponent(e);
        if (-1 === t.indexOf("?")) return t + "?" + i;
        var a = t.split("?"),
            r = a[0] + "?",
            a = a[1].split("&");
        return a.splice(null != n ? n : 1, 0, i), r + a.join("&")
    }, n.j = o, n.Ma = function (t, e, i, a) {
        e = n.J(e, t), a.url = n.J(a.url, t), a.k = n.J(a.k, t), a === Object(a) && a.k && "XMLHttpRequest" === n.ka.C.D ? n.ka.$a(a, i, t) : n.useCORSOnly || n.ha(t, e, i)
    }, n.ha = function (t, e, i) {
        var a, s = 0,
            c = 0;
        if (e && r) {
            for (a = 0; !s && 2 > a;) {
                try {
                    s = (s = r.getElementsByTagName(a > 0 ? "HEAD" : "head")) && 0 < s.length ? s[0] : 0
                } catch (u) {
                    s = 0
                }
                a++
            }
            if (!s) try {
                r.body && (s = r.body)
            } catch (l) {
                s = 0
            }
            if (s)
                for (a = 0; !c && 2 > a;) {
                    try {
                        c = r.createElement(a > 0 ? "SCRIPT" : "script")
                    } catch (d) {
                        c = 0
                    }
                    a++
                }
        }
        e && s && c ? (c.type = "text/javascript", c.src = e, s.firstChild ? s.insertBefore(c, s.firstChild) : s.appendChild(c), s = n.loadTimeout, P.e[t] = {
            requestStart: P.o(),
            url: e,
            sa: s,
            qa: P.va(),
            ra: 0
        }, i && (n.j == o && (n.j = {}), n.j[t] = setTimeout(function () {
            i(A)
        }, s)), n.ia.Ca.push(e)) : i && i()
    }, n.Ja = function (t) {
        n.j != o && n.j[t] && (clearTimeout(n.j[t]), n.j[t] = 0)
    }, n.fa = u, n.ga = u, n.isAllowed = function () {
        return !n.fa && (n.fa = c, n.cookieRead(n.cookieName) || n.cookieWrite(n.cookieName, "T", 1)) && (n.ga = c), n.ga
    }, n.b = o, n.d = o;
    var l = a.Hb;
    l || (l = "MC");
    var d = a.Mb;
    d || (d = "MCMID");
    var h = a.Ib;
    h || (h = "MCCIDH");
    var f = a.Lb;
    f || (f = "MCSYNCS");
    var g = a.Jb;
    g || (g = "MCIDTS");
    var p = a.Kb;
    p || (p = "MCOPTOUT");
    var v = a.Fb;
    v || (v = "A");
    var m = a.Cb;
    m || (m = "MCAID");
    var y = a.Gb;
    y || (y = "AAM");
    var b = a.Eb;
    b || (b = "MCAAMLH");
    var k = a.Db;
    k || (k = "MCAAMB");
    var S = a.Nb;
    S || (S = "NONE"), n.M = 0, n.da = function () {
        if (!n.M) {
            var t = n.version;
            n.audienceManagerServer && (t += "|" + n.audienceManagerServer), n.audienceManagerServerSecure && (t += "|" + n.audienceManagerServerSecure), n.M = n.ea(t)
        }
        return n.M
    }, n.ja = u, n.f = function () {
        if (!n.ja) {
            n.ja = c;
            var t, e, i, a, r = n.da(),
                s = u,
                l = n.cookieRead(n.cookieName),
                d = new Date;
            if (n.b == o && (n.b = {}), l && "T" != l)
                for (l = l.split("|"), l[0].match(/^[\-0-9]+$/) && (parseInt(l[0], 10) != r && (s = c), l.shift()), 1 == l.length % 2 && l.pop(), r = 0; r < l.length; r += 2) t = l[r].split("-"), e = t[0], i = l[r + 1], 1 < t.length ? (a = parseInt(t[1], 10), t = 0 < t[1].indexOf("s")) : (a = 0, t = u), s && (e == h && (i = ""), a > 0 && (a = d.getTime() / 1e3 - 60)), e && i && (n.c(e, i, 1), a > 0 && (n.b["expire" + e] = a + (t ? "s" : ""), d.getTime() >= 1e3 * a || t && !n.cookieRead(n.sessionCookieName))) && (n.d || (n.d = {}), n.d[e] = c);
            !n.a(m) && (l = n.cookieRead("s_vi")) && (l = l.split("|"), 1 < l.length && 0 <= l[0].indexOf("v1") && (i = l[1], r = i.indexOf("["), r >= 0 && (i = i.substring(0, r)), i && i.match(/^[0-9a-fA-F\-]+$/) && n.c(m, i)))
        }
    }, n.Qa = function () {
        var t, e, i = n.da();
        for (t in n.b) !Object.prototype[t] && n.b[t] && "expire" != t.substring(0, 6) && (e = n.b[t], i += (i ? "|" : "") + t + (n.b["expire" + t] ? "-" + n.b["expire" + t] : "") + "|" + e);
        n.cookieWrite(n.cookieName, i, 1)
    }, n.a = function (t, e) {
        return n.b == o || !e && n.d && n.d[t] ? o : n.b[t]
    }, n.c = function (t, e, i) {
        n.b == o && (n.b = {}), n.b[t] = e, i || n.Qa()
    }, n.La = function (t, e) {
        var i = n.a(t, e);
        return i ? i.split("*") : o
    }, n.Pa = function (t, e, i) {
        n.c(t, e ? e.join("*") : "", i)
    }, n.wb = function (t, e) {
        var i = n.La(t, e);
        if (i) {
            var a, r = {};
            for (a = 0; a < i.length; a += 2) r[i[a]] = i[a + 1];
            return r
        }
        return o
    }, n.yb = function (t, e, i) {
        var a, r = o;
        if (e)
            for (a in r = [], e) Object.prototype[a] || (r.push(a), r.push(e[a]));
        n.Pa(t, r, i)
    }, n.m = function (t, e, i) {
        var a = new Date;
        a.setTime(a.getTime() + 1e3 * e), n.b == o && (n.b = {}), n.b["expire" + t] = Math.floor(a.getTime() / 1e3) + (i ? "s" : ""), 0 > e ? (n.d || (n.d = {}), n.d[t] = c) : n.d && (n.d[t] = u), i && (n.cookieRead(n.sessionCookieName) || n.cookieWrite(n.sessionCookieName, "1"))
    }, n.ca = function (t) {
        return t && ("object" == typeof t && (t = t.d_mid ? t.d_mid : t.visitorID ? t.visitorID : t.id ? t.id : t.uuid ? t.uuid : "" + t), t && (t = t.toUpperCase(), "NOTARGET" == t && (t = S)), !t || t != S && !t.match(/^[0-9a-fA-F\-]+$/)) && (t = ""), t
    }, n.i = function (t, e) {
        if (n.Ja(t), n.h != o && (n.h[t] = u), P.e[t] && (P.e[t].nb = P.o(), P.I(t)), t == l) {
            var i = n.a(d);
            if (!i) {
                if (i = "object" == typeof e && e.mid ? e.mid : n.ca(e), !i) {
                    if (n.B) return void n.getAnalyticsVisitorID(o, u, c);
                    i = n.v()
                }
                n.c(d, i)
            }
            i && i != S || (i = ""), "object" == typeof e && ((e.d_region || e.dcs_region || e.d_blob || e.blob) && n.i(y, e), n.B && e.mid && n.i(v, {
                id: e.id
            })), n.q(d, [i])
        }
        if (t == y && "object" == typeof e) {
            i = 604800, e.id_sync_ttl != s && e.id_sync_ttl && (i = parseInt(e.id_sync_ttl, 10));
            var a = n.a(b);
            a || ((a = e.d_region) || (a = e.dcs_region), a && (n.m(b, i), n.c(b, a))), a || (a = ""), n.q(b, [a]), a = n.a(k), (e.d_blob || e.blob) && ((a = e.d_blob) || (a = e.blob), n.m(k, i), n.c(k, a)), a || (a = ""), n.q(k, [a]), !e.error_msg && n.z && n.c(h, n.z)
        }
        if (t == v && (i = n.a(m), i || ((i = n.ca(e)) ? i !== S && n.m(k, -1) : i = S, n.c(m, i)), i && i != S || (i = ""), n.q(m, [i])), n.idSyncDisableSyncs ? T.wa = c : (T.wa = u, i = {}, i.ibs = e.ibs, i.subdomain = e.subdomain, T.kb(i)), e === Object(e)) {
            var r;
            n.isAllowed() && (r = n.a(p)), r || (r = S, e.d_optout && e.d_optout instanceof Array && (r = e.d_optout.join(",")), i = parseInt(e.d_ottl, 10), isNaN(i) && (i = 7200), n.m(p, i, A), n.c(p, r)), n.q(p, [r])
        }
    }, n.h = o, n.r = function (t, e, i, a, r) {
        var s, u = "";
        return n.isAllowed() && (n.f(), u = n.a(t), !u && (t == d || t == p ? s = l : t == b || t == k ? s = y : t == m && (s = v), s)) ? (!e || n.h != o && n.h[s] || (n.h == o && (n.h = {}), n.h[s] = c, n.Ma(s, e, function (e, i) {
            if (!n.a(t))
                if (P.e[s] && (P.e[s].timeout = P.o(), P.e[s].eb = !!e, P.I(s)), i !== Object(i) || n.useCORSOnly) {
                    var a = "";
                    t == d ? a = n.v() : s == y && (a = {
                        error_msg: "timeout"
                    }), n.i(s, a)
                } else n.ha(s, i.url, i.G)
        }, r)), n.Oa(t, i), e || n.i(s, {
            id: S
        }), "") : (t != d && t != m || u != S || (u = "", a = c), i && a && n.K(i, [u]), u)
    }, n._setMarketingCloudFields = function (t) {
        n.f(), n.i(l, t)
    }, n.setMarketingCloudVisitorID = function (t) {
        n._setMarketingCloudFields(t)
    }, n.B = u, n.getMarketingCloudVisitorID = function (t, e) {
        if (n.isAllowed()) {
            n.marketingCloudServer && 0 > n.marketingCloudServer.indexOf(".demdex.net") && (n.B = c);
            var i = n.w("_setMarketingCloudFields");
            return n.r(d, i.url, t, e, i)
        }
        return ""
    }, n.Na = function () {
        n.getAudienceManagerBlob()
    }, a.AuthState = {
        UNKNOWN: 0,
        AUTHENTICATED: 1,
        LOGGED_OUT: 2
    }, n.u = {}, n.ba = u, n.z = "", n.setCustomerIDs = function (t) {
        if (n.isAllowed() && t) {
            n.f();
            var e, i;
            for (e in t)
                if (!Object.prototype[e] && (i = t[e]))
                    if ("object" == typeof i) {
                        var a = {};
                        i.id && (a.id = i.id), i.authState != s && (a.authState = i.authState), n.u[e] = a
                    } else n.u[e] = {
                        id: i
                    };
            var t = n.getCustomerIDs(),
                a = n.a(h),
                r = "";
            a || (a = 0);
            for (e in t) Object.prototype[e] || (i = t[e], r += (r ? "|" : "") + e + "|" + (i.id ? i.id : "") + (i.authState ? i.authState : ""));
            n.z = n.ea(r), n.z != a && (n.ba = c, n.Na())
        }
    }, n.getCustomerIDs = function () {
        n.f();
        var t, e, i = {};
        for (t in n.u) Object.prototype[t] || (e = n.u[t], i[t] || (i[t] = {}), e.id && (i[t].id = e.id), i[t].authState = e.authState != s ? e.authState : a.AuthState.UNKNOWN);
        return i
    }, n._setAnalyticsFields = function (t) {
        n.f(), n.i(v, t)
    }, n.setAnalyticsVisitorID = function (t) {
        n._setAnalyticsFields(t)
    }, n.getAnalyticsVisitorID = function (t, e, i) {
        if (n.isAllowed()) {
            var a = "";
            if (i || (a = n.getMarketingCloudVisitorID(function () {
                    n.getAnalyticsVisitorID(t, c)
            })), a || i) {
                var r = i ? n.marketingCloudServer : n.trackingServer,
                    o = "";
                n.loadSSL && (i ? n.marketingCloudServerSecure && (r = n.marketingCloudServerSecure) : n.trackingServerSecure && (r = n.trackingServerSecure));
                var s = {};
                if (r) {
                    var r = "http" + (n.loadSSL ? "s" : "") + "://" + r + "/id",
                        a = "d_visid_ver=" + n.version + "&mcorgid=" + encodeURIComponent(n.marketingCloudOrgID) + (a ? "&mid=" + encodeURIComponent(a) : "") + (n.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : ""),
                        u = ["s_c_il", n._in, "_set" + (i ? "MarketingCloud" : "Analytics") + "Fields"],
                        o = r + "?" + a + "&callback=s_c_il%5B" + n._in + "%5D._set" + (i ? "MarketingCloud" : "Analytics") + "Fields";
                    s.k = r + "?" + a, s.oa = u
                }
                return s.url = o, n.r(i ? d : m, o, t, e, s)
            }
        }
        return ""
    }, n._setAudienceManagerFields = function (t) {
        n.f(), n.i(y, t)
    }, n.w = function (t) {
        var e = n.audienceManagerServer,
            i = "",
            a = n.a(d),
            r = n.a(k, c),
            o = n.a(m),
            o = o && o != S ? "&d_cid_ic=AVID%01" + encodeURIComponent(o) : "";
        if (n.loadSSL && n.audienceManagerServerSecure && (e = n.audienceManagerServerSecure), e) {
            var s, u, i = n.getCustomerIDs();
            if (i)
                for (s in i) Object.prototype[s] || (u = i[s], o += "&d_cid_ic=" + encodeURIComponent(s) + "%01" + encodeURIComponent(u.id ? u.id : "") + (u.authState ? "%01" + u.authState : ""));
            return t || (t = "_setAudienceManagerFields"), e = "http" + (n.loadSSL ? "s" : "") + "://" + e + "/id", a = "d_visid_ver=" + n.version + "&d_rtbd=json&d_ver=2" + (!a && n.B ? "&d_verify=1" : "") + "&d_orgid=" + encodeURIComponent(n.marketingCloudOrgID) + "&d_nsid=" + (n.idSyncContainerID || 0) + (a ? "&d_mid=" + encodeURIComponent(a) : "") + (n.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : "") + (r ? "&d_blob=" + encodeURIComponent(r) : "") + o, r = ["s_c_il", n._in, t], i = e + "?" + a + "&d_cb=s_c_il%5B" + n._in + "%5D." + t, {
                url: i,
                k: e + "?" + a,
                oa: r
            }
        }
        return {
            url: i
        }
    }, n.getAudienceManagerLocationHint = function (t, e) {
        if (n.isAllowed() && n.getMarketingCloudVisitorID(function () {
                n.getAudienceManagerLocationHint(t, c)
        })) {
            var i = n.a(m);
            if (i || (i = n.getAnalyticsVisitorID(function () {
                    n.getAudienceManagerLocationHint(t, c)
            })), i) return i = n.w(), n.r(b, i.url, t, e, i)
        }
        return ""
    }, n.getAudienceManagerBlob = function (t, e) {
        if (n.isAllowed() && n.getMarketingCloudVisitorID(function () {
                n.getAudienceManagerBlob(t, c)
        })) {
            var i = n.a(m);
            if (i || (i = n.getAnalyticsVisitorID(function () {
                    n.getAudienceManagerBlob(t, c)
            })), i) {
                var i = n.w(),
                    a = i.url;
                return n.ba && n.m(k, -1), n.r(k, a, t, e, i)
            }
        }
        return ""
    }, n.s = "", n.A = {}, n.N = "", n.O = {}, n.getSupplementalDataID = function (t, e) {
        !n.s && !e && (n.s = n.v(1));
        var i = n.s;
        return n.N && !n.O[t] ? (i = n.N, n.O[t] = c) : i && (n.A[t] && (n.N = n.s, n.O = n.A, n.s = i = e ? "" : n.v(1), n.A = {}), i && (n.A[t] = c)), i
    }, a.OptOut = {
        GLOBAL: "global"
    }, n.getOptOut = function (t, e) {
        if (n.isAllowed()) {
            var i = n.w("_setMarketingCloudFields");
            return n.r(p, i.url, t, e, i)
        }
        return ""
    }, n.isOptedOut = function (t, e, i) {
        return n.isAllowed() ? (e || (e = a.OptOut.GLOBAL), (i = n.getOptOut(function (i) {
            n.K(t, [i == a.OptOut.GLOBAL || 0 <= i.indexOf(e)])
        }, i)) ? i == a.OptOut.GLOBAL || 0 <= i.indexOf(e) : o) : u
    };
    var E = {
        p: !!i.postMessage,
        Fa: 1,
        aa: 864e5
    };
    n.rb = E, n.ma = {
        postMessage: function (t, e, n) {
            var i = 1;
            e && (E.p ? n.postMessage(t, e.replace(/([^:]+:\/\/[^\/]+).*/, "$1")) : e && (n.location = e.replace(/#.*$/, "") + "#" + +new Date + i++ + "&" + t))
        },
        V: function (t, e) {
            var n;
            try {
                E.p && (t && (n = function (n) {
                    return "string" == typeof e && n.origin !== e || "[object Function]" === Object.prototype.toString.call(e) && e(n.origin) === B ? B : void t(n)
                }), window.addEventListener ? window[t ? "addEventListener" : "removeEventListener"]("message", n, B) : window[t ? "attachEvent" : "detachEvent"]("onmessage", n))
            } catch (i) { }
        }
    };
    var w = {
        na: function () {
            return r.addEventListener ? function (t, e, n) {
                t.addEventListener(e, function (t) {
                    "function" == typeof n && n(t)
                }, u)
            } : r.attachEvent ? function (t, e, n) {
                t.attachEvent("on" + e, function (t) {
                    "function" == typeof n && n(t)
                })
            } : void 0
        }(),
        map: function (t, e) {
            if (Array.prototype.map) return t.map(e);
            if (void 0 === t || t === o) throw new TypeError;
            var n = Object(t),
                i = n.length >>> 0;
            if ("function" != typeof e) throw new TypeError;
            for (var a = Array(i), r = 0; i > r; r++) r in n && (a[r] = e.call(e, n[r], r, n));
            return a
        },
        Za: function (t, e) {
            return this.map(t, function (t) {
                return encodeURIComponent(t)
            }).join(e)
        }
    };
    n.xb = w;
    var C = {
        C: function () {
            var t = "none",
                e = c;
            return "undefined" != typeof XMLHttpRequest && XMLHttpRequest === Object(XMLHttpRequest) && ("withCredentials" in new XMLHttpRequest ? t = "XMLHttpRequest" : new Function("/*@cc_on return /^10/.test(@_jscript_version) @*/")() ? t = "XMLHttpRequest" : "undefined" != typeof XDomainRequest && XDomainRequest === Object(XDomainRequest) && (e = u), 0 < Object.prototype.toString.call(window.ob).indexOf("Constructor") && (e = u)), {
                D: t,
                Ab: e
            }
        }(),
        ab: function () {
            return "none" === this.C.D ? o : new window[this.C.D]
        },
        $a: function (t, e, i) {
            var a = this;
            e && (t.G = e);
            try {
                var r = this.ab();
                r.open("get", t.k + "&ts=" + (new Date).getTime(), A), "XMLHttpRequest" === this.C.D && (r.withCredentials = c, r.timeout = n.loadTimeout, r.setRequestHeader("Content-Type", "application/x-www-form-urlencoded"), r.onreadystatechange = function () {
                    if (4 === this.readyState && 200 === this.status) t: {
                        var e;
                        try {
                            if (e = JSON.parse(this.responseText), e !== Object(e)) {
                                a.n(t, null, "Response is not JSON");
                                break t
                            }
                        } catch (n) {
                            a.n(t, n, "Error parsing response as JSON");
                            break t
                        }
                        try {
                            for (var i = t.oa, r = window, o = 0; o < i.length; o++) r = r[i[o]];
                            r(e)
                        } catch (s) {
                            a.n(t, s, "Error forming callback function")
                        }
                    }
                }), r.onerror = function (e) {
                    a.n(t, e, "onerror")
                }, r.ontimeout = function (e) {
                    a.n(t, e, "ontimeout")
                }, r.send(), P.e[i] = {
                    requestStart: P.o(),
                    url: t.k,
                    sa: r.timeout,
                    qa: P.va(),
                    ra: 1
                }, n.ia.Ca.push(t.k)
            } catch (o) {
                this.n(t, o, "try-catch")
            }
        },
        n: function (t, e, i) {
            n.CORSErrors.push({
                Bb: t,
                error: e,
                description: i
            }), t.G && ("ontimeout" === i ? t.G(A) : t.G(B, t))
        }
    };
    n.ka = C;
    var T = {
        Ha: 3e4,
        $: 649,
        Ea: u,
        id: o,
        S: o,
        ua: function (t) {
            return "string" == typeof t ? (t = t.split("/"), t[0] + "//" + t[2]) : void 0
        },
        l: o,
        url: o,
        bb: function () {
            var t = "http://fast.",
                e = "?d_nsid=" + n.idSyncContainerID + "#" + encodeURIComponent(r.location.href);
            return this.l || (this.l = "nosubdomainreturned"), n.loadSSL && (t = n.idSyncSSLUseAkamai ? "https://fast." : "https://"), t = t + this.l + ".demdex.net/dest5.html" + e, this.S = this.ua(t), this.id = "destination_publishing_iframe_" + this.l + "_" + n.idSyncContainerID, t
        },
        Ta: function () {
            var t = "?d_nsid=" + n.idSyncContainerID + "#" + encodeURIComponent(r.location.href);
            "string" == typeof n.L && n.L.length && (this.id = "destination_publishing_iframe_" + (new Date).getTime() + "_" + n.idSyncContainerID, this.S = this.ua(n.L), this.url = n.L + t)
        },
        wa: o,
        ta: u,
        X: u,
        F: o,
        Ob: o,
        jb: o,
        Pb: o,
        W: u,
        H: [],
        hb: [],
        ib: [],
        ya: E.p ? 15 : 100,
        T: [],
        fb: [],
        pa: c,
        Ba: u,
        Aa: function () {
            return !n.idSyncDisable3rdPartySyncing && (this.ta || n.tb) && this.l && "nosubdomainreturned" !== this.l && this.url && !this.X
        },
        Q: function () {
            function t() {
                i = document.createElement("iframe"), i.sandbox = "allow-scripts allow-same-origin", i.title = "Adobe ID Syncing iFrame", i.id = n.id, i.style.cssText = "display: none; width: 0; height: 0;", i.src = n.url, n.jb = c, e(), document.body.appendChild(i)
            }

            function e() {
                w.na(i, "load", function () {
                    i.className = "aamIframeLoaded", n.F = c, n.t()
                })
            }
            this.X = c;
            var n = this,
                i = document.getElementById(this.id);
            i ? "IFRAME" !== i.nodeName ? (this.id += "_2", t()) : "aamIframeLoaded" !== i.className ? e() : (this.F = c, this.xa = i, this.t()) : t(), this.xa = i
        },
        t: function (t) {
            var e = this;
            t === Object(t) && this.T.push(t), (this.Ba || !E.p || this.F) && this.T.length && (this.I(this.T.shift()), this.t()), !n.idSyncDisableSyncs && this.F && this.H.length && !this.W && (this.Ea || (this.Ea = c, setTimeout(function () {
                e.ya = E.p ? 15 : 150
            }, this.Ha)), this.W = c, this.Da())
        },
        I: function (t) {
            var e, n, i, a, r, o = encodeURIComponent;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (i = 0; n > i; i++) a = e[i], r = [o("ibs"), o(a.id || ""), o(a.tag || ""), w.Za(a.url || [], ","), o(a.ttl || ""), "", "", a.fireURLSync ? "true" : "false"], this.pa ? this.P(r.join("|")) : a.fireURLSync && this.Ua(a, r.join("|"));
            this.fb.push(t)
        },
        Ua: function (t, e) {
            n.f();
            var i = n.a(f),
                a = u,
                r = u,
                o = Math.ceil((new Date).getTime() / E.aa);
            i ? (i = i.split("*"), r = this.lb(i, t.id, o), a = r.Xa, r = r.Ya, a && r || (this.P(e), i.push(t.id + "-" + (o + Math.ceil(t.ttl / 60 / 24))), this.gb(i), n.c(f, i.join("*")))) : (this.P(e), n.c(f, t.id + "-" + (o + Math.ceil(t.ttl / 60 / 24))))
        },
        lb: function (t, e, n) {
            var i, a, r, o = u,
                s = u;
            for (a = 0; a < t.length; a++) i = t[a], r = parseInt(i.split("-")[1], 10), i.match("^" + e + "-") ? (o = c, r > n ? s = c : (t.splice(a, 1), a--)) : n >= r && (t.splice(a, 1), a--);
            return {
                Xa: o,
                Ya: s
            }
        },
        gb: function (t) {
            if (t.join("*").length > this.$)
                for (t.sort(function (t, e) {
                        return parseInt(t.split("-")[1], 10) - parseInt(e.split("-")[1], 10)
                }) ; t.join("*").length > this.$;) t.shift()
        },
        P: function (t) {
            var e = encodeURIComponent;
            this.H.push(e(n.ub ? "---destpub-debug---" : "---destpub---") + t)
        },
        Da: function () {
            var t, e = this;
            this.H.length ? (t = this.H.shift(), n.ma.postMessage(t, this.url, this.xa.contentWindow), this.hb.push(t), setTimeout(function () {
                e.Da()
            }, this.ya)) : this.W = u
        },
        V: function (t) {
            var e = /^---destpub-to-parent---/;
            "string" == typeof t && e.test(t) && (e = t.replace(e, "").split("|"), "canSetThirdPartyCookies" === e[0] && (this.pa = "true" === e[1] ? c : u, this.Ba = c, this.t()), this.ib.push(t))
        },
        kb: function (t) {
            this.url === o && (this.l = "string" == typeof n.la && n.la.length ? n.la : t.subdomain || "", this.url = this.bb()), t.ibs instanceof Array && t.ibs.length && (this.ta = c), this.Aa() && (n.idSyncAttachIframeASAP ? this.Ra() : (a.Z || "complete" === r.readyState || "loaded" === r.readyState) && this.Q()), "function" == typeof n.idSyncIDCallResult ? n.idSyncIDCallResult(t) : this.t(t), "function" == typeof n.idSyncAfterIDCallResult && n.idSyncAfterIDCallResult(t)
        },
        Sa: function (t, e) {
            return n.vb || !t || e - t > E.Fa
        },
        Ra: function () {
            function t() {
                e.X || (document.body ? e.Q() : setTimeout(t, 30))
            }
            var e = this;
            t()
        }
    };
    n.sb = T, n.timeoutMetricsLog = [];
    var P = {
        Wa: window.performance && window.performance.timing ? 1 : 0,
        za: window.performance && window.performance.timing ? window.performance.timing : o,
        Y: o,
        R: o,
        e: {},
        U: [],
        send: function (t) {
            if (n.takeTimeoutMetrics && t === Object(t)) {
                var e, i = [],
                    a = encodeURIComponent;
                for (e in t) t.hasOwnProperty(e) && i.push(a(e) + "=" + a(t[e]));
                t = "http" + (n.loadSSL ? "s" : "") + "://dpm.demdex.net/event?d_visid_ver=" + n.version + "&d_visid_stg_timeout=" + n.loadTimeout + "&" + i.join("&") + "&d_orgid=" + a(n.marketingCloudOrgID) + "&d_timingapi=" + this.Wa + "&d_winload=" + this.cb() + "&d_ld=" + this.o(), (new Image).src = t, n.timeoutMetricsLog.push(t)
            }
        },
        cb: function () {
            return this.R === o && (this.R = this.za ? this.Y - this.za.navigationStart : this.Y - a.Va), this.R
        },
        o: function () {
            return (new Date).getTime()
        },
        I: function (t) {
            var e = this.e[t],
                n = {};
            n.d_visid_stg_timeout_captured = e.sa, n.d_visid_cors = e.ra, n.d_fieldgroup = t, n.d_settimeout_overriden = e.qa, e.timeout ? e.eb ? (n.d_visid_timedout = 1, n.d_visid_timeout = e.timeout - e.requestStart, n.d_visid_response = -1) : (n.d_visid_timedout = "n/a", n.d_visid_timeout = "n/a", n.d_visid_response = "n/a") : (n.d_visid_timedout = 0, n.d_visid_timeout = -1, n.d_visid_response = e.nb - e.requestStart), n.d_visid_url = e.url, a.Z ? this.send(n) : this.U.push(n), delete this.e[t]
        },
        mb: function () {
            for (var t = 0, e = this.U.length; e > t; t++) this.send(this.U[t])
        },
        va: function () {
            return "function" == typeof setTimeout.toString ? -1 < setTimeout.toString().indexOf("[native code]") ? 0 : 1 : -1
        }
    };
    if (n.zb = P, 0 > t.indexOf("@") && (t += "@AdobeOrg"), n.marketingCloudOrgID = t, n.cookieName = "AMCV_" + t, n.sessionCookieName = "AMCVS_" + t, n.cookieDomain = n.Ka(), n.cookieDomain == i.location.hostname && (n.cookieDomain = ""), n.loadSSL = 0 <= i.location.protocol.toLowerCase().indexOf("https"), n.loadTimeout = 3e4, n.CORSErrors = [], n.marketingCloudServer = n.audienceManagerServer = "dpm.demdex.net", e && "object" == typeof e) {
        for (var L in e) !Object.prototype[L] && (n[L] = e[L]);
        n.idSyncContainerID = n.idSyncContainerID || 0, n.f(), C = n.a(g), L = Math.ceil((new Date).getTime() / E.aa), !n.idSyncDisableSyncs && T.Sa(C, L) && (n.m(k, -1), n.c(g, L)), n.getMarketingCloudVisitorID(), n.getAudienceManagerLocationHint(), n.getAudienceManagerBlob()
    }
    if (!n.idSyncDisableSyncs) {
        T.Ta(), w.na(window, "load", function () {
            a.Z = c, P.Y = P.o(), P.mb();
            var t = T;
            t.Aa() && t.Q()
        });
        try {
            n.ma.V(function (t) {
                T.V(t.data)
            }, T.S)
        } catch (I) { }
    }
}
var A = !0,
    B = !1;
Visitor.getInstance = function (t, e) {
    var n, i, a = window.s_c_il;
    if (0 > t.indexOf("@") && (t += "@AdobeOrg"), a)
        for (i = 0; i < a.length; i++)
            if ((n = a[i]) && "Visitor" == n._c && n.marketingCloudOrgID == t) return n;
    return new Visitor(t, e)
},
    function () {
        function t() {
            e.Z = n
        }
        var e = window.Visitor,
            n = e.Ia,
            i = e.Ga;
        n || (n = A), i || (i = B), window.addEventListener ? window.addEventListener("load", t) : window.attachEvent && window.attachEvent("onload", t), e.Va = (new Date).getTime()
    }(),
    // All code and conventions are protected by copyright
    function (t, e, n) {
        function i() {
            var t = C.filter(C.rules, function (t) {
                return 0 === t.event.indexOf("dataelementchange")
            });
            this.dataElementsNames = C.map(t, function (t) {
                var e = t.event.match(/dataelementchange\((.*)\)/i);
                return e[1]
            }, this), this.initPolling()
        }

        function a() {
            this.rules = C.filter(C.rules, function (t) {
                return "elementexists" === t.event
            })
        }

        function r(t) {
            this.delay = 250, this.FB = t, C.domReady(C.bind(function () {
                C.poll(C.bind(this.initialize, this), this.delay, 8)
            }, this))
        }

        function o() {
            var t = this.eventRegex = /^hover\(([0-9]+)\)$/,
                e = this.rules = [];
            C.each(C.rules, function (n) {
                var i = n.event.match(t);
                i && e.push([Number(n.event.match(t)[1]), n.selector])
            })
        }

        function s(e) {
            e = e || C.rules, this.rules = C.filter(e, function (t) {
                return "inview" === t.event
            }), this.elements = [], this.eventHandler = C.bind(this.track, this), C.addEventHandler(t, "scroll", this.eventHandler), C.addEventHandler(t, "load", this.eventHandler)
        }

        function c() {
            C.getToolsByType("nielsen").length > 0 && C.domReady(C.bind(this.initialize, this))
        }

        function u() {
            this.lastURL = C.URL(), this._fireIfURIChanged = C.bind(this.fireIfURIChanged, this), this._onPopState = C.bind(this.onPopState, this), this._onHashChange = C.bind(this.onHashChange, this), this._pushState = C.bind(this.pushState, this), this._replaceState = C.bind(this.replaceState, this), this.initialize()
        }

        function l() {
            C.addEventHandler(t, "orientationchange", l.orientationChange)
        }

        function d(e) {
            C.domReady(C.bind(function () {
                this.twttr = e || t.twttr, this.initialize()
            }, this))
        }

        function h() {
            this.rules = C.filter(C.rules, function (t) {
                return "videoplayed" === t.event.substring(0, 11)
            }), this.eventHandler = C.bind(this.onUpdateTime, this)
        }

        function f() {
            this.defineEvents(), this.visibilityApiHasPriority = !0, e.addEventListener ? this.setVisibilityApiPriority(!1) : this.attachDetachOlderEventListeners(!0, e, "focusout");
            C.bindEvent("aftertoolinit", function () {
                C.fireEvent(C.visibility.isHidden() ? "tabblur" : "tabfocus")
            })
        }

        function g(t) {
            C.BaseTool.call(this, t), this.name = t.name || "Basic"
        }

        function p() {
            C.BaseTool.call(this), this.asyncScriptCallbackQueue = [], this.argsForBlockingScripts = []
        }

        function v(t) {
            C.BaseTool.call(this, t)
        }

        function m(t) {
            C.BaseTool.call(this, t)
        }

        function y(t) {
            C.BaseTool.call(this, t), this.defineListeners(), this.beaconMethod = "plainBeacon", this.adapt = new y.DataAdapters, this.dataProvider = new y.DataProvider.Aggregate
        }

        function b(t) {
            C.BaseTool.call(this, t), this.varBindings = {}, this.events = [], this.products = [], this.customSetupFuns = []
        }

        function k(t) {
            C.BaseTool.call(this, t), this.styleElements = {}, this.targetPageParamsStore = {}
        }

        function S(t) {
            C.BaseTool.call(this, t), this.name = t.name || "VisitorID", this.initialize()
        }
        var E = Object.prototype.toString,
            w = t._satellite && t._satellite.override,
            C = {
                initialized: !1,
                $data: function (t, e, i) {
                    if (t) {
                        var a = "__satellite__",
                            r = C.dataCache,
                            o = t[a];
                        o || (o = t[a] = C.uuid++);
                        var s = r[o];
                        return s || (s = r[o] = {}), i === n ? s[e] : void (s[e] = i)
                    }
                },
                uuid: 1,
                dataCache: {},
                keys: function (t) {
                    var e = [];
                    for (var n in t) t.hasOwnProperty(n) && e.push(n);
                    return e
                },
                values: function (t) {
                    var e = [];
                    for (var n in t) t.hasOwnProperty(n) && e.push(t[n]);
                    return e
                },
                isArray: Array.isArray || function (t) {
                    return "[object Array]" === E.apply(t)
                },
                isObject: function (t) {
                    return null != t && !C.isArray(t) && "object" == typeof t
                },
                isString: function (t) {
                    return "string" == typeof t
                },
                isNumber: function (t) {
                    return "[object Number]" === E.apply(t) && !C.isNaN(t)
                },
                isNaN: function (t) {
                    return t !== t
                },
                isRegex: function (t) {
                    return t instanceof RegExp
                },
                isLinkTag: function (t) {
                    return !(!t || !t.nodeName || "a" !== t.nodeName.toLowerCase())
                },
                each: function (t, e, n) {
                    for (var i = 0, a = t.length; a > i; i++) e.call(n, t[i], i, t)
                },
                map: function (t, e, n) {
                    for (var i = [], a = 0, r = t.length; r > a; a++) i.push(e.call(n, t[a], a, t));
                    return i
                },
                filter: function (t, e, n) {
                    for (var i = [], a = 0, r = t.length; r > a; a++) {
                        var o = t[a];
                        e.call(n, o, a, t) && i.push(o)
                    }
                    return i
                },
                any: function (t, e, n) {
                    for (var i = 0, a = t.length; a > i; i++) {
                        var r = t[i];
                        if (e.call(n, r, i, t)) return !0
                    }
                    return !1
                },
                every: function (t, e, n) {
                    for (var i = !0, a = 0, r = t.length; r > a; a++) {
                        var o = t[a];
                        i = i && e.call(n, o, a, t)
                    }
                    return i
                },
                contains: function (t, e) {
                    return -1 !== C.indexOf(t, e)
                },
                indexOf: function (t, e) {
                    if (t.indexOf) return t.indexOf(e);
                    for (var n = t.length; n--;)
                        if (e === t[n]) return n;
                    return -1
                },
                find: function (t, e, n) {
                    if (!t) return null;
                    for (var i = 0, a = t.length; a > i; i++) {
                        var r = t[i];
                        if (e.call(n, r, i, t)) return r
                    }
                    return null
                },
                textMatch: function (t, e) {
                    if (null == e) throw new Error("Illegal Argument: Pattern is not present");
                    return null == t ? !1 : "string" == typeof e ? t === e : e instanceof RegExp ? e.test(t) : !1
                },
                stringify: function (t, e) {
                    if (e = e || [], C.isObject(t)) {
                        if (C.contains(e, t)) return "<Cycle>";
                        e.push(t)
                    }
                    if (C.isArray(t)) return "[" + C.map(t, function (t) {
                        return C.stringify(t, e)
                    }).join(",") + "]";
                    if (C.isString(t)) return '"' + String(t) + '"';
                    if (C.isObject(t)) {
                        var n = [];
                        for (var i in t) t.hasOwnProperty(i) && n.push(i + ": " + C.stringify(t[i], e));
                        return "{" + n.join(", ") + "}"
                    }
                    return String(t)
                },
                trim: function (t) {
                    return null == t ? null : t.trim ? t.trim() : t.replace(/^ */, "").replace(/ *$/, "")
                },
                bind: function (t, e) {
                    return function () {
                        return t.apply(e, arguments)
                    }
                },
                throttle: function (t, e) {
                    var n = null;
                    return function () {
                        var i = this,
                            a = arguments;
                        clearTimeout(n), n = setTimeout(function () {
                            t.apply(i, a)
                        }, e)
                    }
                },
                domReady: function (t) {
                    function n(t) {
                        for (h = 1; t = a.shift() ;) t()
                    }
                    var i, a = [],
                        r = !1,
                        o = e,
                        s = o.documentElement,
                        c = s.doScroll,
                        u = "DOMContentLoaded",
                        l = "addEventListener",
                        d = "onreadystatechange",
                        h = /^loade|^c/.test(o.readyState);
                    return o[l] && o[l](u, i = function () {
                        o.removeEventListener(u, i, r), n()
                    }, r), c && o.attachEvent(d, i = function () {
                        /^c/.test(o.readyState) && (o.detachEvent(d, i), n())
                    }), t = c ? function (e) {
                        self != top ? h ? e() : a.push(e) : function () {
                            try {
                                s.doScroll("left")
                            } catch (n) {
                                return setTimeout(function () {
                                    t(e)
                                }, 50)
                            }
                            e()
                        }()
                    } : function (t) {
                        h ? t() : a.push(t)
                    }
                }(),
                loadScript: function (t, n) {
                    var i = e.createElement("script");
                    C.scriptOnLoad(t, i, n), i.src = t, e.getElementsByTagName("head")[0].appendChild(i)
                },
                scriptOnLoad: function (t, e, n) {
                    function i(t) {
                        t && C.logError(t), n && n(t)
                    }
                    "onload" in e ? (e.onload = function () {
                        i()
                    }, e.onerror = function () {
                        i(new Error("Failed to load script " + t))
                    }) : "readyState" in e && (e.onreadystatechange = function () {
                        var t = e.readyState;
                        ("loaded" === t || "complete" === t) && (e.onreadystatechange = null, i())
                    })
                },
                loadScriptOnce: function (t, e) {
                    C.loadedScriptRegistry[t] || C.loadScript(t, function (n) {
                        n || (C.loadedScriptRegistry[t] = !0), e && e(n)
                    })
                },
                loadedScriptRegistry: {},
                loadScriptSync: function (t) {
                    return e.write ? C.domReadyFired ? void C.notify('Cannot load sync the "' + t + '" script after DOM Ready.', 1) : (t.indexOf('"') > -1 && (t = encodeURI(t)), void e.write('<script src="' + t + '"></script>')) : void C.notify('Cannot load sync the "' + t + '" script because "document.write" is not available', 1)
                },
                pushAsyncScript: function (t) {
                    C.tools["default"].pushAsyncScript(t)
                },
                pushBlockingScript: function (t) {
                    C.tools["default"].pushBlockingScript(t)
                },
                addEventHandler: t.addEventListener ? function (t, e, n) {
                    t.addEventListener(e, n, !1)
                } : function (t, e, n) {
                    t.attachEvent("on" + e, n)
                },
                removeEventHandler: t.removeEventListener ? function (t, e, n) {
                    t.removeEventListener(e, n, !1)
                } : function (t, e, n) {
                    t.detachEvent("on" + e, n)
                },
                preventDefault: t.addEventListener ? function (t) {
                    t.preventDefault()
                } : function (t) {
                    t.returnValue = !1
                },
                stopPropagation: function (t) {
                    t.cancelBubble = !0, t.stopPropagation && t.stopPropagation()
                },
                containsElement: function (t, e) {
                    return t.contains ? t.contains(e) : !!(16 & t.compareDocumentPosition(e))
                },
                matchesCss: function (n) {
                    function i(t, e) {
                        var n = e.tagName;
                        return n ? t.toLowerCase() === n.toLowerCase() : !1
                    }
                    var a = n.matchesSelector || n.mozMatchesSelector || n.webkitMatchesSelector || n.oMatchesSelector || n.msMatchesSelector;
                    return a ? function (n, i) {
                        if (i === e || i === t) return !1;
                        try {
                            return a.call(i, n)
                        } catch (r) {
                            return !1
                        }
                    } : n.querySelectorAll ? function (t, e) {
                        var n = e.parentNode;
                        if (!n) return !1;
                        if (t.match(/^[a-z]+$/i)) return i(t, e);
                        try {
                            for (var a = e.parentNode.querySelectorAll(t), r = a.length; r--;)
                                if (a[r] === e) return !0
                        } catch (o) { }
                        return !1
                    } : function (t, e) {
                        if (t.match(/^[a-z]+$/i)) return i(t, e);
                        try {
                            return C.Sizzle.matches(t, [e]).length > 0
                        } catch (n) {
                            return !1
                        }
                    }
                }(e.documentElement),
                cssQuery: function (t) {
                    return t.querySelectorAll ? function (e, n) {
                        var i;
                        try {
                            i = t.querySelectorAll(e)
                        } catch (a) {
                            i = []
                        }
                        n(i)
                    } : function (t, e) {
                        if (C.Sizzle) {
                            var n;
                            try {
                                n = C.Sizzle(t)
                            } catch (i) {
                                n = []
                            }
                            e(n)
                        } else C.sizzleQueue.push([t, e])
                    }
                }(e),
                hasAttr: function (t, e) {
                    return t.hasAttribute ? t.hasAttribute(e) : t[e] !== n
                },
                inherit: function (t, e) {
                    var n = function () { };
                    n.prototype = e.prototype, t.prototype = new n, t.prototype.constructor = t
                },
                extend: function (t, e) {
                    for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n])
                },
                toArray: function () {
                    try {
                        var t = Array.prototype.slice;
                        return t.call(e.documentElement.childNodes, 0)[0].nodeType,
                            function (e) {
                                return t.call(e, 0)
                            }
                    } catch (n) {
                        return function (t) {
                            for (var e = [], n = 0, i = t.length; i > n; n++) e.push(t[n]);
                            return e
                        }
                    }
                }(),
                equalsIgnoreCase: function (t, e) {
                    return null == t ? null == e : null == e ? !1 : String(t).toLowerCase() === String(e).toLowerCase()
                },
                poll: function (t, e, n) {
                    function i() {
                        C.isNumber(n) && a++ >= n || t() || setTimeout(i, e)
                    }
                    var a = 0;
                    e = e || 1e3, i()
                },
                escapeForHtml: function (t) {
                    return t ? String(t).replace(/\&/g, "&amp;").replace(/\</g, "&lt;").replace(/\>/g, "&gt;").replace(/\"/g, "&quot;").replace(/\'/g, "&#x27;").replace(/\//g, "&#x2F;") : t
                }
            };
        C.availableTools = {}, C.availableEventEmitters = [], C.fireOnceEvents = ["condition", "elementexists"], C.initEventEmitters = function () {
            C.eventEmitters = C.map(C.availableEventEmitters, function (t) {
                return new t
            })
        }, C.eventEmitterBackgroundTasks = function () {
            C.each(C.eventEmitters, function (t) {
                "backgroundTasks" in t && t.backgroundTasks()
            })
        }, C.initTools = function (t) {
            var e = {
                "default": new p
            },
                n = C.settings.euCookieName || "sat_track";
            for (var i in t)
                if (t.hasOwnProperty(i)) {
                    var a, r, o;
                    if (a = t[i], a.euCookie) {
                        var s = "true" !== C.readCookie(n);
                        if (s) continue
                    }
                    if (r = C.availableTools[a.engine], !r) {
                        var c = [];
                        for (var u in C.availableTools) C.availableTools.hasOwnProperty(u) && c.push(u);
                        throw new Error("No tool engine named " + a.engine + ", available: " + c.join(",") + ".")
                    }
                    o = new r(a), o.id = i, e[i] = o
                }
            return e
        }, C.preprocessArguments = function (t, e, n, i, a) {
            function r(t) {
                return i && C.isString(t) ? t.toLowerCase() : t
            }

            function o(t) {
                var c = {};
                for (var u in t)
                    if (t.hasOwnProperty(u)) {
                        var l = t[u];
                        C.isObject(l) ? c[u] = o(l) : C.isArray(l) ? c[u] = s(l, i) : c[u] = r(C.replace(l, e, n, a))
                    }
                return c
            }

            function s(t, i) {
                for (var a = [], s = 0, c = t.length; c > s; s++) {
                    var u = t[s];
                    C.isString(u) ? u = r(C.replace(u, e, n)) : u && u.constructor === Object && (u = o(u)), a.push(u)
                }
                return a
            }
            return t ? s(t, i) : t
        }, C.execute = function (t, e, n, i) {
            function a(a) {
                var r = i[a || "default"];
                if (r) try {
                    r.triggerCommand(t, e, n)
                } catch (o) {
                    C.logError(o)
                }
            }
            if (!_satellite.settings.hideActivity)
                if (i = i || C.tools, t.engine) {
                    var r = t.engine;
                    for (var o in i)
                        if (i.hasOwnProperty(o)) {
                            var s = i[o];
                            s.settings && s.settings.engine === r && a(o)
                        }
                } else t.tool instanceof Array ? C.each(t.tool, function (t) {
                    a(t)
                }) : a(t.tool)
        }, C.Logger = {
            outputEnabled: !1,
            messages: [],
            keepLimit: 100,
            flushed: !1,
            LEVELS: [null, null, "log", "info", "warn", "error"],
            message: function (t, e) {
                var n = this.LEVELS[e] || "log";
                this.messages.push([n, t]), this.messages.length > this.keepLimit && this.messages.shift(), this.outputEnabled && this.echo(n, t)
            },
            getHistory: function () {
                return this.messages
            },
            clearHistory: function () {
                this.messages = []
            },
            setOutputState: function (t) {
                this.outputEnabled != t && (this.outputEnabled = t, t ? this.flush() : this.flushed = !1)
            },
            echo: function (e, n) {
                t.console && t.console[e]("SATELLITE: " + n)
            },
            flush: function () {
                this.flushed || (C.each(this.messages, function (t) {
                    t[2] !== !0 && (this.echo(t[0], t[1]), t[2] = !0)
                }, this), this.flushed = !0)
            }
        }, C.notify = C.bind(C.Logger.message, C.Logger), C.cleanText = function (t) {
            return null == t ? null : C.trim(t).replace(/\s+/g, " ")
        }, C.cleanText.legacy = function (t) {
            return null == t ? null : C.trim(t).replace(/\s{2,}/g, " ").replace(/[^\000-\177]*/g, "")
        }, C.text = function (t) {
            return t.textContent || t.innerText
        }, C.specialProperties = {
            text: C.text,
            cleanText: function (t) {
                return C.cleanText(C.text(t))
            }
        }, C.getObjectProperty = function (t, e, i) {
            for (var a, r = e.split("."), o = t, s = C.specialProperties, c = 0, u = r.length; u > c; c++) {
                if (null == o) return n;
                var l = r[c];
                if (i && "@" === l.charAt(0)) {
                    var d = l.slice(1);
                    o = s[d](o)
                } else if (o.getAttribute && (a = l.match(/^getAttribute\((.+)\)$/))) {
                    var h = a[1];
                    o = o.getAttribute(h)
                } else o = o[l]
            }
            return o
        }, C.getToolsByType = function (t) {
            if (!t) throw new Error("Tool type is missing");
            var e = [];
            for (var n in C.tools)
                if (C.tools.hasOwnProperty(n)) {
                    var i = C.tools[n];
                    i.settings && i.settings.engine === t && e.push(i)
                }
            return e
        }, C.setVar = function () {
            var t = C.data.customVars;
            if (null == t && (C.data.customVars = {}, t = C.data.customVars), "string" == typeof arguments[0]) {
                var e = arguments[0];
                t[e] = arguments[1]
            } else if (arguments[0]) {
                var n = arguments[0];
                for (var i in n) n.hasOwnProperty(i) && (t[i] = n[i])
            }
        }, C.dataElementSafe = function (t, e) {
            if (arguments.length > 2) {
                var n = arguments[2];
                "pageview" === e ? C.dataElementSafe.pageviewCache[t] = n : "session" === e ? C.setCookie("_sdsat_" + t, n) : "visitor" === e && C.setCookie("_sdsat_" + t, n, 730)
            } else {
                if ("pageview" === e) return C.dataElementSafe.pageviewCache[t];
                if ("session" === e || "visitor" === e) return C.readCookie("_sdsat_" + t)
            }
        }, C.dataElementSafe.pageviewCache = {}, C.realGetDataElement = function (e) {
            var n;
            return e.selector ? C.hasSelector && C.cssQuery(e.selector, function (t) {
                if (t.length > 0) {
                    var i = t[0];
                    "text" === e.property ? n = i.innerText || i.textContent : e.property in i ? n = i[e.property] : C.hasAttr(i, e.property) && (n = i.getAttribute(e.property))
                }
            }) : e.queryParam ? n = e.ignoreCase ? C.getQueryParamCaseInsensitive(e.queryParam) : C.getQueryParam(e.queryParam) : e.cookie ? n = C.readCookie(e.cookie) : e.jsVariable ? n = C.getObjectProperty(t, e.jsVariable) : e.customJS ? n = e.customJS() : e.contextHub && (n = e.contextHub()), C.isString(n) && e.cleanText && (n = C.cleanText(n)), n
        }, C.getDataElement = function (t, e, i) {
            if (i = i || C.dataElements[t], null == i) return C.settings.undefinedVarsReturnEmpty ? "" : null;
            var a = C.realGetDataElement(i);
            return a === n && i.storeLength ? a = C.dataElementSafe(t, i.storeLength) : a !== n && i.storeLength && C.dataElementSafe(t, i.storeLength, a), a || e || (a = i["default"] || ""), C.isString(a) && i.forceLowerCase && (a = a.toLowerCase()), a
        }, C.getVar = function (i, a, r) {
            var o, s, c = C.data.customVars,
                u = r ? r.target || r.srcElement : null,
                l = {
                    uri: C.URI(),
                    protocol: e.location.protocol,
                    hostname: e.location.hostname
                };
            if (C.dataElements && i in C.dataElements) return C.getDataElement(i);
            if (s = l[i.toLowerCase()], s === n)
                if ("this." === i.substring(0, 5)) i = i.slice(5), s = C.getObjectProperty(a, i, !0);
                else if ("event." === i.substring(0, 6)) i = i.slice(6), s = C.getObjectProperty(r, i);
                else if ("target." === i.substring(0, 7)) i = i.slice(7), s = C.getObjectProperty(u, i);
                else if ("window." === i.substring(0, 7)) i = i.slice(7), s = C.getObjectProperty(t, i);
                else if ("param." === i.substring(0, 6)) i = i.slice(6), s = C.getQueryParam(i);
                else if (o = i.match(/^rand([0-9]+)$/)) {
                    var d = Number(o[1]),
                        h = (Math.random() * (Math.pow(10, d) - 1)).toFixed(0);
                    s = Array(d - h.length + 1).join("0") + h
                } else s = C.getObjectProperty(c, i);
            return s
        }, C.getVars = function (t, e, n) {
            var i = {};
            return C.each(t, function (t) {
                i[t] = C.getVar(t, e, n)
            }), i
        }, C.replace = function (t, e, n, i) {
            return "string" != typeof t ? t : t.replace(/%(.*?)%/g, function (t, a) {
                var r = C.getVar(a, e, n);
                return null == r ? C.settings.undefinedVarsReturnEmpty ? "" : t : i ? C.escapeForHtml(r) : r
            })
        }, C.escapeHtmlParams = function (t) {
            return t.escapeHtml = !0, t
        }, C.searchVariables = function (t, e, n) {
            if (!t || 0 === t.length) return "";
            for (var i = [], a = 0, r = t.length; r > a; a++) {
                var o = t[a],
                    s = C.getVar(o, e, n);
                i.push(o + "=" + escape(s))
            }
            return "?" + i.join("&")
        }, C.fireRule = function (t, e, n) {
            var i = t.trigger;
            if (i) {
                for (var a = 0, r = i.length; r > a; a++) {
                    var o = i[a];
                    C.execute(o, e, n)
                }
                C.contains(C.fireOnceEvents, t.event) && (t.expired = !0)
            }
        }, C.isLinked = function (t) {
            for (var e = t; e; e = e.parentNode)
                if (C.isLinkTag(e)) return !0;
            return !1
        }, C.firePageLoadEvent = function (t) {
            for (var n = e.location, i = {
                type: t,
                target: n
            }, a = C.pageLoadRules, r = C.evtHandlers[i.type], o = a.length; o--;) {
                var s = a[o];
                C.ruleMatches(s, i, n) && (C.notify('Rule "' + s.name + '" fired.', 1), C.fireRule(s, n, i))
            }
            for (var c in C.tools)
                if (C.tools.hasOwnProperty(c)) {
                    var u = C.tools[c];
                    u.endPLPhase && u.endPLPhase(t)
                }
            r && C.each(r, function (t) {
                t(i)
            })
        }, C.track = function (t) {
            t = t.replace(/^\s*/, "").replace(/\s*$/, "");
            for (var e = 0; e < C.directCallRules.length; e++) {
                var n = C.directCallRules[e];
                if (n.name === t) return C.notify('Direct call Rule "' + t + '" fired.', 1), void C.fireRule(n, location, {
                    type: t
                })
            }
            C.notify('Direct call Rule "' + t + '" not found.', 1)
        }, C.basePath = function () {
            return C.data.host ? ("https:" === e.location.protocol ? "https://" + C.data.host.https : "http://" + C.data.host.http) + "/" : this.settings.basePath
        }, C.setLocation = function (e) {
            t.location = e
        }, C.parseQueryParams = function (t) {
            var e = function (t) {
                var e = t;
                try {
                    e = decodeURIComponent(t)
                } catch (n) { }
                return e
            };
            if ("" === t || C.isString(t) === !1) return {};
            0 === t.indexOf("?") && (t = t.substring(1));
            var n = {},
                i = t.split("&");
            return C.each(i, function (t) {
                t = t.split("="), t[1] && (n[e(t[0])] = e(t[1]))
            }), n
        }, C.getCaseSensitivityQueryParamsMap = function (t) {
            var e = C.parseQueryParams(t),
                n = {};
            for (var i in e) e.hasOwnProperty(i) && (n[i.toLowerCase()] = e[i]);
            return {
                normal: e,
                caseInsensitive: n
            }
        }, C.updateQueryParams = function () {
            C.QueryParams = C.getCaseSensitivityQueryParamsMap(t.location.search)
        }, C.updateQueryParams(), C.getQueryParam = function (t) {
            return C.QueryParams.normal[t]
        }, C.getQueryParamCaseInsensitive = function (t) {
            return C.QueryParams.caseInsensitive[t.toLowerCase()]
        }, C.encodeObjectToURI = function (t) {
            if (C.isObject(t) === !1) return "";
            var e = [];
            for (var n in t) t.hasOwnProperty(n) && e.push(encodeURIComponent(n) + "=" + encodeURIComponent(t[n]));
            return e.join("&")
        }, C.readCookie = function (t) {
            for (var i = t + "=", a = e.cookie.split(";"), r = 0; r < a.length; r++) {
                for (var o = a[r];
                    " " == o.charAt(0) ;) o = o.substring(1, o.length);
                if (0 === o.indexOf(i)) return o.substring(i.length, o.length)
            }
            return n
        }, C.setCookie = function (t, n, i) {
            var a;
            if (i) {
                var r = new Date;
                r.setTime(r.getTime() + 24 * i * 60 * 60 * 1e3), a = "; expires=" + r.toGMTString()
            } else a = "";
            e.cookie = t + "=" + n + a + "; path=/"
        }, C.removeCookie = function (t) {
            C.setCookie(t, "", -1)
        }, C.getElementProperty = function (t, e) {
            if ("@" === e.charAt(0)) {
                var i = C.specialProperties[e.substring(1)];
                if (i) return i(t)
            }
            return "innerText" === e ? C.text(t) : e in t ? t[e] : t.getAttribute ? t.getAttribute(e) : n
        }, C.propertiesMatch = function (t, e) {
            if (t)
                for (var n in t)
                    if (t.hasOwnProperty(n)) {
                        var i = t[n],
                            a = C.getElementProperty(e, n);
                        if ("string" == typeof i && i !== a) return !1;
                        if (i instanceof RegExp && !i.test(a)) return !1
                    }
            return !0
        }, C.isRightClick = function (t) {
            var e;
            return t.which ? e = 3 == t.which : t.button && (e = 2 == t.button), e
        }, C.ruleMatches = function (t, e, n, i) {
            var a = t.condition,
                r = t.conditions,
                o = t.property,
                s = e.type,
                c = t.value,
                u = e.target || e.srcElement,
                l = n === u;
            if (t.event !== s && ("custom" !== t.event || t.customEvent !== s)) return !1;
            if (!C.ruleInScope(t)) return !1;
            if ("click" === t.event && C.isRightClick(e)) return !1;
            if (t.isDefault && i > 0) return !1;
            if (t.expired) return !1;
            if ("inview" === s && e.inviewDelay !== t.inviewDelay) return !1;
            if (!l && (t.bubbleFireIfParent === !1 || 0 !== i && t.bubbleFireIfChildFired === !1)) return !1;
            if (t.selector && !C.matchesCss(t.selector, n)) return !1;
            if (!C.propertiesMatch(o, n)) return !1;
            if (null != c)
                if ("string" == typeof c) {
                    if (c !== n.value) return !1
                } else if (!c.test(n.value)) return !1;
            if (a) try {
                if (!a.call(n, e, u)) return C.notify('Condition for rule "' + t.name + '" not met.', 1), !1
            } catch (d) {
                return C.notify('Condition for rule "' + t.name + '" not met. Error: ' + d.message, 1), !1
            }
            if (r) {
                var h = C.find(r, function (i) {
                    try {
                        return !i.call(n, e, u)
                    } catch (a) {
                        return C.notify('Condition for rule "' + t.name + '" not met. Error: ' + a.message, 1), !0
                    }
                });
                if (h) return C.notify("Condition " + h.toString() + ' for rule "' + t.name + '" not met.', 1), !1
            }
            return !0
        }, C.evtHandlers = {}, C.bindEvent = function (t, e) {
            var n = C.evtHandlers;
            n[t] || (n[t] = []), n[t].push(e)
        }, C.whenEvent = C.bindEvent, C.unbindEvent = function (t, e) {
            var n = C.evtHandlers;
            if (n[t]) {
                var i = C.indexOf(n[t], e);
                n[t].splice(i, 1)
            }
        }, C.bindEventOnce = function (t, e) {
            var n = function () {
                C.unbindEvent(t, n), e.apply(null, arguments)
            };
            C.bindEvent(t, n)
        }, C.isVMLPoisoned = function (t) {
            if (!t) return !1;
            try {
                t.nodeName
            } catch (e) {
                if ("Attribute only valid on v:image" === e.message) return !0
            }
            return !1
        }, C.handleEvent = function (t) {
            if (!C.$data(t, "eventProcessed")) {
                var e = t.type.toLowerCase(),
                    n = t.target || t.srcElement,
                    i = 0,
                    a = C.rules,
                    r = (C.tools, C.evtHandlers[t.type]);
                if (C.isVMLPoisoned(n)) return void C.notify("detected " + e + " on poisoned VML element, skipping.", 1);
                r && C.each(r, function (e) {
                    e(t)
                });
                var o = n && n.nodeName;
                o ? C.notify("detected " + e + " on " + n.nodeName, 1) : C.notify("detected " + e, 1);
                for (var s = n; s; s = s.parentNode) {
                    var c = !1;
                    if (C.each(a, function (e) {
                            C.ruleMatches(e, t, s, i) && (C.notify('Rule "' + e.name + '" fired.', 1), C.fireRule(e, s, t), i++, e.bubbleStop && (c = !0))
                    }), c) break
                }
                C.$data(t, "eventProcessed", !0)
            }
        }, C.onEvent = e.querySelectorAll ? function (t) {
            C.handleEvent(t)
        } : function () {
            var t = [],
                e = function (e) {
                    e.selector ? t.push(e) : C.handleEvent(e)
                };
            return e.pendingEvents = t, e
        }(), C.fireEvent = function (t, e) {
            C.onEvent({
                type: t,
                target: e
            })
        }, C.registerEvents = function (t, e) {
            for (var n = e.length - 1; n >= 0; n--) {
                var i = e[n];
                C.$data(t, i + ".tracked") || (C.addEventHandler(t, i, C.onEvent), C.$data(t, i + ".tracked", !0))
            }
        }, C.registerEventsForTags = function (t, n) {
            for (var i = t.length - 1; i >= 0; i--)
                for (var a = t[i], r = e.getElementsByTagName(a), o = r.length - 1; o >= 0; o--) C.registerEvents(r[o], n)
        }, C.setListeners = function () {
            var t = ["click", "submit"];
            C.each(C.rules, function (e) {
                "custom" === e.event && e.hasOwnProperty("customEvent") && !C.contains(t, e.customEvent) && t.push(e.customEvent)
            }), C.registerEvents(e, t)
        }, C.getUniqueRuleEvents = function () {
            return C._uniqueRuleEvents || (C._uniqueRuleEvents = [], C.each(C.rules, function (t) {
                -1 === C.indexOf(C._uniqueRuleEvents, t.event) && C._uniqueRuleEvents.push(t.event)
            })), C._uniqueRuleEvents
        }, C.setFormListeners = function () {
            if (!C._relevantFormEvents) {
                var t = ["change", "focus", "blur", "keypress"];
                C._relevantFormEvents = C.filter(C.getUniqueRuleEvents(), function (e) {
                    return -1 !== C.indexOf(t, e)
                })
            }
            C._relevantFormEvents.length && C.registerEventsForTags(["input", "select", "textarea", "button"], C._relevantFormEvents)
        }, C.setVideoListeners = function () {
            if (!C._relevantVideoEvents) {
                var t = ["play", "pause", "ended", "volumechange", "stalled", "loadeddata"];
                C._relevantVideoEvents = C.filter(C.getUniqueRuleEvents(), function (e) {
                    return -1 !== C.indexOf(t, e)
                })
            }
            C._relevantVideoEvents.length && C.registerEventsForTags(["video"], C._relevantVideoEvents)
        }, C.readStoredSetting = function (e) {
            try {
                return e = "sdsat_" + e, t.localStorage.getItem(e)
            } catch (n) {
                return C.notify("Cannot read stored setting from localStorage: " + n.message, 2), null
            }
        }, C.loadStoredSettings = function () {
            var t = C.readStoredSetting("debug"),
                e = C.readStoredSetting("hide_activity");
            t && (C.settings.notifications = "true" === t), e && (C.settings.hideActivity = "true" === e)
        }, C.isRuleActive = function (t, e) {
            function n(t, e) {
                return e = a(e, {
                    hour: t[f](),
                    minute: t[g]()
                }), Math.floor(Math.abs((t.getTime() - e.getTime()) / 864e5))
            }

            function i(t, e) {
                function n(t) {
                    return 12 * t[d]() + t[h]()
                }
                return Math.abs(n(t) - n(e))
            }

            function a(t, e) {
                var n = new Date(t.getTime());
                for (var i in e)
                    if (e.hasOwnProperty(i)) {
                        var a = e[i];
                        switch (i) {
                            case "hour":
                                n[p](a);
                                break;
                            case "minute":
                                n[v](a);
                                break;
                            case "date":
                                n[m](a)
                        }
                    }
                return n
            }

            function r(t, e) {
                var n = t[f](),
                    i = t[g](),
                    a = e[f](),
                    r = e[g]();
                return 60 * n + i > 60 * a + r
            }

            function o(t, e) {
                var n = t[f](),
                    i = t[g](),
                    a = e[f](),
                    r = e[g]();
                return 60 * a + r > 60 * n + i
            }
            var s = t.schedule;
            if (!s) return !0;
            var c = s.utc,
                u = c ? "getUTCDate" : "getDate",
                l = c ? "getUTCDay" : "getDay",
                d = c ? "getUTCFullYear" : "getFullYear",
                h = c ? "getUTCMonth" : "getMonth",
                f = c ? "getUTCHours" : "getHours",
                g = c ? "getUTCMinutes" : "getMinutes",
                p = c ? "setUTCHours" : "setHours",
                v = c ? "setUTCMinutes" : "setMinutes",
                m = c ? "setUTCDate" : "setDate";
            if (e = e || new Date, s.repeat) {
                if (r(s.start, e)) return !1;
                if (o(s.end, e)) return !1;
                if (e < s.start) return !1;
                if (s.endRepeat && e >= s.endRepeat) return !1;
                if ("daily" === s.repeat) {
                    if (s.repeatEvery) {
                        var y = n(s.start, e);
                        if (y % s.repeatEvery !== 0) return !1
                    }
                } else if ("weekly" === s.repeat) {
                    if (s.days) {
                        if (!C.contains(s.days, e[l]())) return !1
                    } else if (s.start[l]() !== e[l]()) return !1;
                    if (s.repeatEvery) {
                        var b = n(s.start, e);
                        if (b % (7 * s.repeatEvery) !== 0) return !1
                    }
                } else if ("monthly" === s.repeat) {
                    if (s.repeatEvery) {
                        var k = i(s.start, e);
                        if (k % s.repeatEvery !== 0) return !1
                    }
                    if (s.nthWeek && s.mthDay) {
                        if (s.mthDay !== e[l]()) return !1;
                        var S = Math.floor((e[u]() - e[l]() + 1) / 7);
                        if (s.nthWeek !== S) return !1
                    } else if (s.start[u]() !== e[u]()) return !1
                } else if ("yearly" === s.repeat) {
                    if (s.start[h]() !== e[h]()) return !1;
                    if (s.start[u]() !== e[u]()) return !1;
                    if (s.repeatEvery) {
                        var b = Math.abs(s.start[d]() - e[d]());
                        if (b % s.repeatEvery !== 0) return !1
                    }
                }
            } else {
                if (s.start > e) return !1;
                if (s.end < e) return !1
            }
            return !0
        }, C.isOutboundLink = function (t) {
            if (!t.getAttribute("href")) return !1;
            var e = t.hostname,
                n = (t.href, t.protocol);
            if ("http:" !== n && "https:" !== n) return !1;
            var i = C.any(C.settings.domainList, function (t) {
                return C.isSubdomainOf(e, t)
            });
            return i ? !1 : e !== location.hostname
        }, C.isLinkerLink = function (t) {
            return t.getAttribute && t.getAttribute("href") ? C.hasMultipleDomains() && t.hostname != location.hostname && !t.href.match(/^javascript/i) && !C.isOutboundLink(t) : !1
        }, C.isSubdomainOf = function (t, e) {
            if (t === e) return !0;
            var n = t.length - e.length;
            return n > 0 ? C.equalsIgnoreCase(t.substring(n), e) : !1
        }, C.getVisitorId = function () {
            var t = C.getToolsByType("visitor_id");
            return 0 === t.length ? null : t[0].getInstance()
        }, C.URI = function () {
            var t = e.location.pathname + e.location.search;
            return C.settings.forceLowerCase && (t = t.toLowerCase()), t
        }, C.URL = function () {
            var t = e.location.href;
            return C.settings.forceLowerCase && (t = t.toLowerCase()), t
        }, C.filterRules = function () {
            function t(t) {
                return C.isRuleActive(t) ? !0 : !1
            }
            C.rules = C.filter(C.rules, t), C.pageLoadRules = C.filter(C.pageLoadRules, t)
        }, C.ruleInScope = function (t, n) {
            function i(t, e) {
                function n(t) {
                    return e.match(t)
                }
                var i = t.include,
                    r = t.exclude;
                if (i && a(i, e)) return !0;
                if (r) {
                    if (C.isString(r) && r === e) return !0;
                    if (C.isArray(r) && C.any(r, n)) return !0;
                    if (C.isRegex(r) && n(r)) return !0
                }
                return !1
            }

            function a(t, e) {
                function n(t) {
                    return e.match(t)
                }
                return C.isString(t) && t !== e ? !0 : C.isArray(t) && !C.any(t, n) ? !0 : C.isRegex(t) && !n(t) ? !0 : !1
            }
            n = n || e.location;
            var r = t.scope;
            if (!r) return !0;
            var o = r.URI,
                s = r.subdomains,
                c = r.domains,
                u = r.protocols,
                l = r.hashes;
            return o && i(o, n.pathname + n.search) ? !1 : s && i(s, n.hostname) ? !1 : c && a(c, n.hostname) ? !1 : u && a(u, n.protocol) ? !1 : l && i(l, n.hash) ? !1 : !0
        }, C.backgroundTasks = function () {
            +new Date;
            C.setFormListeners(), C.setVideoListeners(), C.loadStoredSettings(), C.registerNewElementsForDynamicRules(), C.eventEmitterBackgroundTasks(); +new Date
        }, C.registerNewElementsForDynamicRules = function () {
            function t(e, n) {
                var i = t.cache[e];
                return i ? n(i) : void C.cssQuery(e, function (i) {
                    t.cache[e] = i, n(i)
                })
            }
            t.cache = {}, C.each(C.dynamicRules, function (e) {
                t(e.selector, function (t) {
                    C.each(t, function (t) {
                        var n = "custom" === e.event ? e.customEvent : e.event;
                        C.$data(t, "dynamicRules.seen." + n) || (C.$data(t, "dynamicRules.seen." + n, !0), C.propertiesMatch(e.property, t) && C.registerEvents(t, [n]))
                    })
                })
            })
        }, C.ensureCSSSelector = function () {
            return e.querySelectorAll ? void (C.hasSelector = !0) : (C.loadingSizzle = !0, C.sizzleQueue = [], void C.loadScript(C.basePath() + "selector.js", function () {
                if (!C.Sizzle) return void C.logError(new Error("Failed to load selector.js"));
                var t = C.onEvent.pendingEvents;
                C.each(t, function (t) {
                    C.handleEvent(t)
                }, this), C.onEvent = C.handleEvent, C.hasSelector = !0, delete C.loadingSizzle, C.each(C.sizzleQueue, function (t) {
                    C.cssQuery(t[0], t[1])
                }), delete C.sizzleQueue
            }))
        }, C.errors = [], C.logError = function (t) {
            C.errors.push(t), C.notify(t.name + " - " + t.message, 5)
        }, C.pageBottom = function () {
            C.initialized && (C.pageBottomFired = !0, C.firePageLoadEvent("pagebottom"))
        }, C.stagingLibraryOverride = function () {
            var t = "true" === C.readStoredSetting("stagingLibrary");
            if (t) {
                for (var n, i, a, r = e.getElementsByTagName("script"), o = /^(.*)satelliteLib-([a-f0-9]{40})\.js$/, s = /^(.*)satelliteLib-([a-f0-9]{40})-staging\.js$/, c = 0, u = r.length; u > c && (a = r[c].getAttribute("src"), !a || (n || (n = a.match(o)), i || (i = a.match(s)), !i)) ; c++);
                if (n && !i) {
                    var l = n[1] + "satelliteLib-" + n[2] + "-staging.js";
                    if (e.write) e.write('<script src="' + l + '"></script>');
                    else {
                        var d = e.createElement("script");
                        d.src = l, e.head.appendChild(d)
                    }
                    return !0
                }
            }
            return !1
        }, C.checkAsyncInclude = function () {
            t.satellite_asyncLoad && C.notify('You may be using the async installation of Satellite. In-page HTML and the "pagebottom" event will not work. Please update your Satellite installation for these features.', 5)
        }, C.hasMultipleDomains = function () {
            return !!C.settings.domainList && C.settings.domainList.length > 1
        }, C.handleOverrides = function () {
            if (w)
                for (var t in w) w.hasOwnProperty(t) && (C.data[t] = w[t])
        }, C.privacyManagerParams = function () {
            var t = {};
            C.extend(t, C.settings.privacyManagement);
            var e = [];
            for (var n in C.tools)
                if (C.tools.hasOwnProperty(n)) {
                    var i = C.tools[n],
                        a = i.settings;
                    if (!a) continue;
                    "sc" === a.engine && e.push(i)
                }
            var r = C.filter(C.map(e, function (t) {
                return t.getTrackingServer()
            }), function (t) {
                return null != t
            });
            t.adobeAnalyticsTrackingServers = r;
            for (var o = ["bannerText", "headline", "introductoryText", "customCSS"], s = 0; s < o.length; s++) {
                var c = o[s],
                    u = t[c];
                if (u)
                    if ("text" === u.type) t[c] = u.value;
                    else {
                        if ("data" !== u.type) throw new Error("Invalid type: " + u.type);
                        t[c] = C.getVar(u.value)
                    }
            }
            return t
        }, C.prepareLoadPrivacyManager = function () {
            function e(t) {
                function e() {
                    r++, r === a.length && (n(), clearTimeout(o), t())
                }

                function n() {
                    C.each(a, function (t) {
                        C.unbindEvent(t.id + ".load", e)
                    })
                }

                function i() {
                    n(), t()
                }
                var a = C.filter(C.values(C.tools), function (t) {
                    return t.settings && "sc" === t.settings.engine
                });
                if (0 === a.length) return t();
                var r = 0;
                C.each(a, function (t) {
                    C.bindEvent(t.id + ".load", e)
                });
                var o = setTimeout(i, 5e3)
            }
            C.addEventHandler(t, "load", function () {
                e(C.loadPrivacyManager)
            })
        }, C.loadPrivacyManager = function () {
            var t = C.basePath() + "privacy_manager.js";
            C.loadScript(t, function () {
                var t = C.privacyManager;
                t.configure(C.privacyManagerParams()), t.openIfRequired()
            })
        }, C.init = function (e) {
            if (!C.stagingLibraryOverride()) {
                C.configurationSettings = e;
                var i = e.tools;
                delete e.tools;
                for (var a in e) e.hasOwnProperty(a) && (C[a] = e[a]);
                C.data.customVars === n && (C.data.customVars = {}), C.data.queryParams = C.QueryParams.normal, C.handleOverrides(), C.detectBrowserInfo(), C.trackVisitorInfo && C.trackVisitorInfo(), C.loadStoredSettings(), C.Logger.setOutputState(C.settings.notifications), C.checkAsyncInclude(), C.ensureCSSSelector(), C.filterRules(), C.dynamicRules = C.filter(C.rules, function (t) {
                    return t.eventHandlerOnElement
                }), C.tools = C.initTools(i), C.initEventEmitters(), C.firePageLoadEvent("aftertoolinit"), C.settings.privacyManagement && C.prepareLoadPrivacyManager(), C.hasSelector && C.domReady(C.eventEmitterBackgroundTasks), C.setListeners(), C.domReady(function () {
                    C.poll(function () {
                        C.backgroundTasks()
                    }, C.settings.recheckEvery || 3e3)
                }), C.domReady(function () {
                    C.domReadyFired = !0, C.pageBottomFired || C.pageBottom(), C.firePageLoadEvent("domready")
                }), C.addEventHandler(t, "load", function () {
                    C.firePageLoadEvent("windowload")
                }), C.firePageLoadEvent("pagetop"), C.initialized = !0
            }
        }, C.pageLoadPhases = ["aftertoolinit", "pagetop", "pagebottom", "domready", "windowload"], C.loadEventBefore = function (t, e) {
            return C.indexOf(C.pageLoadPhases, t) <= C.indexOf(C.pageLoadPhases, e)
        }, C.flushPendingCalls = function (t) {
            t.pending && (C.each(t.pending, function (e) {
                var n = e[0],
                    i = e[1],
                    a = e[2],
                    r = e[3];
                n in t ? t[n].apply(t, [i, a].concat(r)) : t.emit ? t.emit(n, i, a, r) : C.notify("Failed to trigger " + n + " for tool " + t.id, 1)
            }), delete t.pending)
        }, C.setDebug = function (e) {
            try {
                t.localStorage.setItem("sdsat_debug", e)
            } catch (n) {
                C.notify("Cannot set debug mode: " + n.message, 2)
            }
        }, C.getUserAgent = function () {
            return navigator.userAgent
        }, C.detectBrowserInfo = function () {
            function t(t) {
                return function (e) {
                    for (var n in t)
                        if (t.hasOwnProperty(n)) {
                            var i = t[n],
                                a = i.test(e);
                            if (a) return n
                        }
                    return "Unknown"
                }
            }
            var e = t({
                "IE Edge Mobile": /Windows Phone.*Edge/,
                "IE Edge": /Edge/,
                OmniWeb: /OmniWeb/,
                "Opera Mini": /Opera Mini/,
                "Opera Mobile": /Opera Mobi/,
                Opera: /Opera/,
                Chrome: /Chrome|CriOS|CrMo/,
                Firefox: /Firefox|FxiOS/,
                "IE Mobile": /IEMobile/,
                IE: /MSIE|Trident/,
                "Mobile Safari": /Mobile(\/[0-9A-z]+)? Safari/,
                Safari: /Safari/
            }),
                n = t({
                    Blackberry: /BlackBerry|BB10/,
                    "Symbian OS": /Symbian|SymbOS/,
                    Maemo: /Maemo/,
                    Android: /Android/,
                    Linux: / Linux /,
                    Unix: /FreeBSD|OpenBSD|CrOS/,
                    Windows: /[\( ]Windows /,
                    iOS: /iPhone|iPad|iPod/,
                    MacOS: /Macintosh;/
                }),
                i = t({
                    Nokia: /Symbian|SymbOS|Maemo/,
                    "Windows Phone": /Windows Phone/,
                    Blackberry: /BlackBerry|BB10/,
                    Android: /Android/,
                    iPad: /iPad/,
                    iPod: /iPod/,
                    iPhone: /iPhone/,
                    Desktop: /.*/
                }),
                a = C.getUserAgent();
            C.browserInfo = {
                browser: e(a),
                os: n(a),
                deviceType: i(a)
            }
        }, C.isHttps = function () {
            return "https:" == e.location.protocol
        }, C.BaseTool = function (t) {
            this.settings = t || {}, this.forceLowerCase = C.settings.forceLowerCase, "forceLowerCase" in this.settings && (this.forceLowerCase = this.settings.forceLowerCase)
        }, C.BaseTool.prototype = {
            triggerCommand: function (t, e, n) {
                var i = this.settings || {};
                if (this.initialize && this.isQueueAvailable() && this.isQueueable(t) && n && C.loadEventBefore(n.type, i.loadOn)) return void this.queueCommand(t, e, n);
                var a = t.command,
                    r = this["$" + a],
                    o = r ? r.escapeHtml : !1,
                    s = C.preprocessArguments(t.arguments, e, n, this.forceLowerCase, o);
                r ? r.apply(this, [e, n].concat(s)) : this.$missing$ ? this.$missing$(a, e, n, s) : C.notify("Failed to trigger " + a + " for tool " + this.id, 1)
            },
            endPLPhase: function (t) { },
            isQueueable: function (t) {
                return "cancelToolInit" !== t.command
            },
            isQueueAvailable: function () {
                return !this.initialized && !this.initializing
            },
            flushQueue: function () {
                this.pending && (C.each(this.pending, function (t) {
                    this.triggerCommand.apply(this, t)
                }, this), this.pending = [])
            },
            queueCommand: function (t, e, n) {
                this.pending || (this.pending = []), this.pending.push([t, e, n])
            },
            $cancelToolInit: function () {
                this._cancelToolInit = !0
            }
        }, t._satellite = C, i.prototype.initPolling = function () {
            0 !== this.dataElementsNames.length && (this.dataElementsStore = this.getDataElementsValues(), C.poll(C.bind(this.checkDataElementValues, this), 1e3))
        }, i.prototype.getDataElementsValues = function () {
            var t = {};
            return C.each(this.dataElementsNames, function (e) {
                t[e] = C.getVar(e);
            }), t
        }, i.prototype.checkDataElementValues = function () {
            C.each(this.dataElementsNames, C.bind(function (t) {
                var e = C.getVar(t),
                    n = this.dataElementsStore[t];
                e !== n && (this.dataElementsStore[t] = e, C.onEvent({
                    type: "dataelementchange(" + t + ")",
                    target: {
                        value: e,
                        previousValue: n
                    }
                }))
            }, this))
        }, C.availableEventEmitters.push(i), a.prototype.backgroundTasks = function () {
            C.each(this.rules, function (t) {
                C.cssQuery(t.selector, function (t) {
                    if (t.length > 0) {
                        var e = t[0];
                        if (C.$data(e, "elementexists.seen")) return;
                        C.$data(e, "elementexists.seen", !0), C.onEvent({
                            type: "elementexists",
                            target: e
                        })
                    }
                })
            })
        }, C.availableEventEmitters.push(a), r.prototype = {
            initialize: function () {
                return this.FB = this.FB || t.FB, this.FB && this.FB.Event && this.FB.Event.subscribe ? (this.bind(), !0) : void 0
            },
            bind: function () {
                this.FB.Event.subscribe("edge.create", function () {
                    C.notify("tracking a facebook like", 1), C.onEvent({
                        type: "facebook.like",
                        target: e
                    })
                }), this.FB.Event.subscribe("edge.remove", function () {
                    C.notify("tracking a facebook unlike", 1), C.onEvent({
                        type: "facebook.unlike",
                        target: e
                    })
                }), this.FB.Event.subscribe("message.send", function () {
                    C.notify("tracking a facebook share", 1), C.onEvent({
                        type: "facebook.send",
                        target: e
                    })
                })
            }
        }, C.availableEventEmitters.push(r), o.prototype = {
            backgroundTasks: function () {
                var t = this;
                C.each(this.rules, function (e) {
                    var n = e[1],
                        i = e[0];
                    C.cssQuery(n, function (e) {
                        C.each(e, function (e) {
                            t.trackElement(e, i)
                        })
                    })
                }, this)
            },
            trackElement: function (t, e) {
                var n = this,
                    i = C.$data(t, "hover.delays");
                i ? C.contains(i, e) || i.push(e) : (C.addEventHandler(t, "mouseover", function (e) {
                    n.onMouseOver(e, t)
                }), C.addEventHandler(t, "mouseout", function (e) {
                    n.onMouseOut(e, t)
                }), C.$data(t, "hover.delays", [e]))
            },
            onMouseOver: function (t, e) {
                var n = t.target || t.srcElement,
                    i = t.relatedTarget || t.fromElement,
                    a = (e === n || C.containsElement(e, n)) && !C.containsElement(e, i);
                a && this.onMouseEnter(e)
            },
            onMouseEnter: function (t) {
                var e = C.$data(t, "hover.delays"),
                    n = C.map(e, function (e) {
                        return setTimeout(function () {
                            C.onEvent({
                                type: "hover(" + e + ")",
                                target: t
                            })
                        }, e)
                    });
                C.$data(t, "hover.delayTimers", n)
            },
            onMouseOut: function (t, e) {
                var n = t.target || t.srcElement,
                    i = t.relatedTarget || t.toElement,
                    a = (e === n || C.containsElement(e, n)) && !C.containsElement(e, i);
                a && this.onMouseLeave(e)
            },
            onMouseLeave: function (t) {
                var e = C.$data(t, "hover.delayTimers");
                e && C.each(e, function (t) {
                    clearTimeout(t)
                })
            }
        }, C.availableEventEmitters.push(o), s.offset = function (n) {
            var i = null,
                a = null;
            try {
                var r = n.getBoundingClientRect(),
                    o = e,
                    s = o.documentElement,
                    c = o.body,
                    u = t,
                    l = s.clientTop || c.clientTop || 0,
                    d = s.clientLeft || c.clientLeft || 0,
                    h = u.pageYOffset || s.scrollTop || c.scrollTop,
                    f = u.pageXOffset || s.scrollLeft || c.scrollLeft;
                i = r.top + h - l, a = r.left + f - d
            } catch (g) { }
            return {
                top: i,
                left: a
            }
        }, s.getViewportHeight = function () {
            var n = t.innerHeight,
                i = e.compatMode;
            return i && (n = "CSS1Compat" == i ? e.documentElement.clientHeight : e.body.clientHeight), n
        }, s.getScrollTop = function () {
            return e.documentElement.scrollTop ? e.documentElement.scrollTop : e.body.scrollTop
        }, s.isElementInDocument = function (t) {
            return e.body.contains(t)
        }, s.isElementInView = function (t) {
            if (!s.isElementInDocument(t)) return !1;
            var e = s.getViewportHeight(),
                n = s.getScrollTop(),
                i = s.offset(t).top,
                a = t.offsetHeight;
            return null !== i ? !(n > i + a || i > n + e) : !1
        }, s.prototype = {
            backgroundTasks: function () {
                var t = this.elements;
                C.each(this.rules, function (e) {
                    C.cssQuery(e.selector, function (n) {
                        var i = 0;
                        C.each(n, function (e) {
                            C.contains(t, e) || (t.push(e), i++)
                        }), i && C.notify(e.selector + " added " + i + " elements.", 1)
                    })
                }), this.track()
            },
            checkInView: function (t, e, n) {
                var i = C.$data(t, "inview");
                if (s.isElementInView(t)) {
                    i || C.$data(t, "inview", !0);
                    var a = this;
                    this.processRules(t, function (n, i, r) {
                        if (e || !n.inviewDelay) C.$data(t, i, !0), C.onEvent({
                            type: "inview",
                            target: t,
                            inviewDelay: n.inviewDelay
                        });
                        else if (n.inviewDelay) {
                            var o = C.$data(t, r);
                            o || (o = setTimeout(function () {
                                a.checkInView(t, !0, n.inviewDelay)
                            }, n.inviewDelay), C.$data(t, r, o))
                        }
                    }, n)
                } else {
                    if (!s.isElementInDocument(t)) {
                        var r = C.indexOf(this.elements, t);
                        this.elements.splice(r, 1)
                    }
                    i && C.$data(t, "inview", !1), this.processRules(t, function (e, n, i) {
                        var a = C.$data(t, i);
                        a && clearTimeout(a)
                    }, n)
                }
            },
            track: function () {
                for (var t = this.elements.length - 1; t >= 0; t--) this.checkInView(this.elements[t])
            },
            processRules: function (t, e, n) {
                var i = this.rules;
                n && (i = C.filter(this.rules, function (t) {
                    return t.inviewDelay == n
                })), C.each(i, function (n, i) {
                    var a = n.inviewDelay ? "viewed_" + n.inviewDelay : "viewed",
                        r = "inview_timeout_id_" + i;
                    C.$data(t, a) || C.matchesCss(n.selector, t) && e(n, a, r)
                })
            }
        }, C.availableEventEmitters.push(s), c.prototype = {
            obue: !1,
            initialize: function () {
                this.attachCloseListeners()
            },
            obuePrevUnload: function () { },
            obuePrevBeforeUnload: function () { },
            newObueListener: function () {
                this.obue || (this.obue = !0, this.triggerBeacons())
            },
            attachCloseListeners: function () {
                this.prevUnload = t.onunload, this.prevBeforeUnload = t.onbeforeunload, t.onunload = C.bind(function (e) {
                    this.prevUnload && setTimeout(C.bind(function () {
                        this.prevUnload.call(t, e)
                    }, this), 1), this.newObueListener()
                }, this), t.onbeforeunload = C.bind(function (e) {
                    this.prevBeforeUnload && setTimeout(C.bind(function () {
                        this.prevBeforeUnload.call(t, e)
                    }, this), 1), this.newObueListener()
                }, this)
            },
            triggerBeacons: function () {
                C.fireEvent("leave", e)
            }
        }, C.availableEventEmitters.push(c), u.prototype = {
            initialize: function () {
                this.setupHistoryAPI(), this.setupHashChange()
            },
            fireIfURIChanged: function () {
                var t = C.URL();
                this.lastURL !== t && (this.fireEvent(), this.lastURL = t)
            },
            fireEvent: function () {
                C.updateQueryParams(), C.onEvent({
                    type: "locationchange",
                    target: e
                })
            },
            setupSPASupport: function () {
                this.setupHistoryAPI(), this.setupHashChange()
            },
            setupHistoryAPI: function () {
                var e = t.history;
                e && (e.pushState && (this.originalPushState = e.pushState, e.pushState = this._pushState), e.replaceState && (this.originalReplaceState = e.replaceState, e.replaceState = this._replaceState)), C.addEventHandler(t, "popstate", this._onPopState)
            },
            pushState: function () {
                var t = this.originalPushState.apply(history, arguments);
                return this.onPushState(), t
            },
            replaceState: function () {
                var t = this.originalReplaceState.apply(history, arguments);
                return this.onReplaceState(), t
            },
            setupHashChange: function () {
                C.addEventHandler(t, "hashchange", this._onHashChange)
            },
            onReplaceState: function () {
                setTimeout(this._fireIfURIChanged, 0)
            },
            onPushState: function () {
                setTimeout(this._fireIfURIChanged, 0)
            },
            onPopState: function () {
                setTimeout(this._fireIfURIChanged, 0)
            },
            onHashChange: function () {
                setTimeout(this._fireIfURIChanged, 0)
            },
            uninitialize: function () {
                this.cleanUpHistoryAPI(), this.cleanUpHashChange()
            },
            cleanUpHistoryAPI: function () {
                history.pushState === this._pushState && (history.pushState = this.originalPushState), history.replaceState === this._replaceState && (history.replaceState = this.originalReplaceState), C.removeEventHandler(t, "popstate", this._onPopState)
            },
            cleanUpHashChange: function () {
                C.removeEventHandler(t, "hashchange", this._onHashChange)
            }
        }, C.availableEventEmitters.push(u), l.orientationChange = function (e) {
            var n = 0 === t.orientation ? "portrait" : "landscape";
            e.orientation = n, C.onEvent(e)
        }, C.availableEventEmitters.push(l), d.prototype = {
            initialize: function () {
                var t = this.twttr;
                t && "function" == typeof t.ready && t.ready(C.bind(this.bind, this))
            },
            bind: function () {
                this.twttr.events.bind("tweet", function (t) {
                    t && (C.notify("tracking a tweet button", 1), C.onEvent({
                        type: "twitter.tweet",
                        target: e
                    }))
                })
            }
        }, C.availableEventEmitters.push(d), h.prototype = {
            backgroundTasks: function () {
                var t = this.eventHandler;
                C.each(this.rules, function (e) {
                    C.cssQuery(e.selector || "video", function (e) {
                        C.each(e, function (e) {
                            C.$data(e, "videoplayed.tracked") || (C.addEventHandler(e, "timeupdate", C.throttle(t, 100)), C.$data(e, "videoplayed.tracked", !0))
                        })
                    })
                })
            },
            evalRule: function (t, e) {
                var n = e.event,
                    i = t.seekable,
                    a = i.start(0),
                    r = i.end(0),
                    o = t.currentTime,
                    s = e.event.match(/^videoplayed\(([0-9]+)([s%])\)$/);
                if (s) {
                    var c = s[2],
                        u = Number(s[1]),
                        l = "%" === c ? function () {
                            return 100 * (o - a) / (r - a) >= u
                        } : function () {
                            return o - a >= u
                        };
                    !C.$data(t, n) && l() && (C.$data(t, n, !0), C.onEvent({
                        type: n,
                        target: t
                    }))
                }
            },
            onUpdateTime: function (t) {
                var e = this.rules,
                    n = t.target;
                if (n.seekable && 0 !== n.seekable.length)
                    for (var i = 0, a = e.length; a > i; i++) this.evalRule(n, e[i])
            }
        }, C.availableEventEmitters.push(h), f.prototype = {
            defineEvents: function () {
                this.oldBlurClosure = function () {
                    C.fireEvent("tabblur", e)
                }, this.oldFocusClosure = C.bind(function () {
                    this.visibilityApiHasPriority ? C.fireEvent("tabfocus", e) : null != C.visibility.getHiddenProperty() ? C.visibility.isHidden() || C.fireEvent("tabfocus", e) : C.fireEvent("tabfocus", e)
                }, this)
            },
            attachDetachModernEventListeners: function (t) {
                var n = 0 == t ? "removeEventHandler" : "addEventHandler";
                C[n](e, C.visibility.getVisibilityEvent(), this.handleVisibilityChange)
            },
            attachDetachOlderEventListeners: function (e, n, i) {
                var a = 0 == e ? "removeEventHandler" : "addEventHandler";
                C[a](n, i, this.oldBlurClosure), C[a](t, "focus", this.oldFocusClosure)
            },
            handleVisibilityChange: function () {
                C.visibility.isHidden() ? C.fireEvent("tabblur", e) : C.fireEvent("tabfocus", e)
            },
            setVisibilityApiPriority: function (e) {
                this.visibilityApiHasPriority = e, this.attachDetachOlderEventListeners(!1, t, "blur"), this.attachDetachModernEventListeners(!1), e ? null != C.visibility.getHiddenProperty() ? this.attachDetachModernEventListeners(!0) : this.attachDetachOlderEventListeners(!0, t, "blur") : (this.attachDetachOlderEventListeners(!0, t, "blur"), null != C.visibility.getHiddenProperty() && this.attachDetachModernEventListeners(!0))
            },
            oldBlurClosure: null,
            oldFocusClosure: null,
            visibilityApiHasPriority: !0
        }, C.availableEventEmitters.push(f), C.ecommerce = {
            addItem: function () {
                var t = [].slice.call(arguments);
                C.onEvent({
                    type: "ecommerce.additem",
                    target: t
                })
            },
            addTrans: function () {
                var t = [].slice.call(arguments);
                C.data.saleData.sale = {
                    orderId: t[0],
                    revenue: t[2]
                }, C.onEvent({
                    type: "ecommerce.addtrans",
                    target: t
                })
            },
            trackTrans: function () {
                C.onEvent({
                    type: "ecommerce.tracktrans",
                    target: []
                })
            }
        }, C.visibility = {
            isHidden: function () {
                var t = this.getHiddenProperty();
                return t ? e[t] : !1
            },
            isVisible: function () {
                return !this.isHidden()
            },
            getHiddenProperty: function () {
                var t = ["webkit", "moz", "ms", "o"];
                if ("hidden" in e) return "hidden";
                for (var n = 0; n < t.length; n++)
                    if (t[n] + "Hidden" in e) return t[n] + "Hidden";
                return null
            },
            getVisibilityEvent: function () {
                var t = this.getHiddenProperty();
                return t ? t.replace(/[H|h]idden/, "") + "visibilitychange" : null
            }
        }, C.inherit(g, C.BaseTool), C.extend(g.prototype, {
            initialize: function () {
                var t = this.settings;
                if (this.settings.initTool !== !1) {
                    var e = t.url;
                    e = "string" == typeof e ? C.basePath() + e : C.isHttps() ? e.https : e.http, C.loadScript(e, C.bind(this.onLoad, this)), this.initializing = !0
                } else this.initialized = !0
            },
            isQueueAvailable: function () {
                return !this.initialized
            },
            onLoad: function () {
                this.initialized = !0, this.initializing = !1, this.settings.initialBeacon && this.settings.initialBeacon(), this.flushQueue()
            },
            endPLPhase: function (t) {
                var e = this.settings.loadOn;
                t === e && (C.notify(this.name + ": Initializing at " + t, 1), this.initialize())
            },
            $fire: function (t, e, n) {
                return this.initializing ? void this.queueCommand({
                    command: "fire",
                    arguments: [n]
                }, t, e) : void n.call(this.settings, t, e)
            }
        }), C.availableTools.am = g, C.availableTools.adlens = g, C.availableTools.aem = g, C.availableTools.__basic = g, C.inherit(p, C.BaseTool), C.extend(p.prototype, {
            name: "Default",
            $loadIframe: function (e, n, i) {
                var a = i.pages,
                    r = i.loadOn,
                    o = C.bind(function () {
                        C.each(a, function (t) {
                            this.loadIframe(e, n, t)
                        }, this)
                    }, this);
                r || o(), "domready" === r && C.domReady(o), "load" === r && C.addEventHandler(t, "load", o)
            },
            loadIframe: function (t, n, i) {
                var a = e.createElement("iframe");
                a.style.display = "none";
                var r = C.data.host,
                    o = i.data,
                    s = this.scriptURL(i.src),
                    c = C.searchVariables(o, t, n);
                r && (s = C.basePath() + s), s += c, a.src = s;
                var u = e.getElementsByTagName("body")[0];
                u ? u.appendChild(a) : C.domReady(function () {
                    e.getElementsByTagName("body")[0].appendChild(a)
                })
            },
            scriptURL: function (t) {
                var e = C.settings.scriptDir || "";
                return e + t
            },
            $loadScript: function (e, n, i) {
                var a = i.scripts,
                    r = i.sequential,
                    o = i.loadOn,
                    s = C.bind(function () {
                        r ? this.loadScripts(e, n, a) : C.each(a, function (t) {
                            this.loadScripts(e, n, [t])
                        }, this)
                    }, this);
                o ? "domready" === o ? C.domReady(s) : "load" === o && C.addEventHandler(t, "load", s) : s()
            },
            loadScripts: function (t, e, n) {
                function i() {
                    if (r.length > 0 && a) {
                        var c = r.shift();
                        c.call(t, e, o)
                    }
                    var u = n.shift();
                    if (u) {
                        var l = C.data.host,
                            d = s.scriptURL(u.src);
                        l && (d = C.basePath() + d), a = u, C.loadScript(d, i)
                    }
                }
                try {
                    var a, n = n.slice(0),
                        r = this.asyncScriptCallbackQueue,
                        o = e.target || e.srcElement,
                        s = this
                } catch (c) {
                    console.error("scripts is", C.stringify(n))
                }
                i()
            },
            $loadBlockingScript: function (t, e, n) {
                var i = n.scripts,
                    a = (n.loadOn, C.bind(function () {
                        C.each(i, function (n) {
                            this.loadBlockingScript(t, e, n)
                        }, this)
                    }, this));
                a()
            },
            loadBlockingScript: function (t, e, n) {
                var i = this.scriptURL(n.src),
                    a = C.data.host,
                    r = e.target || e.srcElement;
                a && (i = C.basePath() + i), this.argsForBlockingScripts.push([t, e, r]), C.loadScriptSync(i)
            },
            pushAsyncScript: function (t) {
                this.asyncScriptCallbackQueue.push(t)
            },
            pushBlockingScript: function (t) {
                var e = this.argsForBlockingScripts.shift(),
                    n = e[0];
                t.apply(n, e.slice(1))
            },
            $writeHTML: C.escapeHtmlParams(function (t, n) {
                if (C.domReadyFired || !e.write) return void C.notify("Command writeHTML failed. You should try appending HTML using the async option.", 1);
                if ("pagebottom" !== n.type && "pagetop" !== n.type) return void C.notify("You can only use writeHTML on the `pagetop` and `pagebottom` events.", 1);
                for (var i = 2, a = arguments.length; a > i; i++) {
                    var r = arguments[i].html;
                    r = C.replace(r, t, n), e.write(r)
                }
            }),
            linkNeedsDelayActivate: function (e, n) {
                n = n || t;
                var i = e.tagName,
                    a = e.getAttribute("target"),
                    r = e.getAttribute("href");
                return i && "a" !== i.toLowerCase() ? !1 : r ? a ? "_blank" === a ? !1 : "_top" === a ? n.top === n : "_parent" === a ? !1 : "_self" === a ? !0 : n.name ? a === n.name : !0 : !0 : !1
            },
            $delayActivateLink: function (t, e) {
                if (this.linkNeedsDelayActivate(t)) {
                    C.preventDefault(e);
                    var n = C.settings.linkDelay || 100;
                    setTimeout(function () {
                        C.setLocation(t.href)
                    }, n)
                }
            },
            isQueueable: function (t) {
                return "writeHTML" !== t.command
            }
        }), C.availableTools["default"] = p, C.inherit(v, C.BaseTool), C.extend(v.prototype, {
            name: "GA",
            initialize: function () {
                var e = this.settings,
                    n = t._gaq,
                    i = e.initCommands || [],
                    a = e.customInit;
                if (n || (_gaq = []), this.isSuppressed()) C.notify("GA: page code not loaded(suppressed).", 1);
                else {
                    if (!n && !v.scriptLoaded) {
                        var r = C.isHttps(),
                            o = (r ? "https://ssl" : "http://www") + ".google-analytics.com/ga.js";
                        e.url && (o = r ? e.url.https : e.url.http), C.loadScript(o), v.scriptLoaded = !0, C.notify("GA: page code loaded.", 1)
                    }
                    var s = (e.domain, e.trackerName),
                        c = T.allowLinker(),
                        u = C.replace(e.account, location);
                    C.settings.domainList || [];
                    _gaq.push([this.cmd("setAccount"), u]), c && _gaq.push([this.cmd("setAllowLinker"), c]), _gaq.push([this.cmd("setDomainName"), T.cookieDomain()]), C.each(i, function (t) {
                        var e = [this.cmd(t[0])].concat(C.preprocessArguments(t.slice(1), location, null, this.forceLowerCase));
                        _gaq.push(e)
                    }, this), a && (this.suppressInitialPageView = !1 === a(_gaq, s)), e.pageName && this.$overrideInitialPageView(null, null, e.pageName)
                }
                this.initialized = !0, C.fireEvent(this.id + ".configure", _gaq, s)
            },
            isSuppressed: function () {
                return this._cancelToolInit || this.settings.initTool === !1
            },
            tracker: function () {
                return this.settings.trackerName
            },
            cmd: function (t) {
                var e = this.tracker();
                return e ? e + "._" + t : "_" + t
            },
            $overrideInitialPageView: function (t, e, n) {
                this.urlOverride = n
            },
            trackInitialPageView: function () {
                if (!this.isSuppressed() && !this.suppressInitialPageView)
                    if (this.urlOverride) {
                        var t = C.preprocessArguments([this.urlOverride], location, null, this.forceLowerCase);
                        this.$missing$("trackPageview", null, null, t)
                    } else this.$missing$("trackPageview")
            },
            endPLPhase: function (t) {
                var e = this.settings.loadOn;
                t === e && (C.notify("GA: Initializing at " + t, 1), this.initialize(), this.flushQueue(), this.trackInitialPageView())
            },
            call: function (t, e, n, i) {
                if (!this._cancelToolInit) {
                    var a = (this.settings, this.tracker()),
                        r = this.cmd(t),
                        i = i ? [r].concat(i) : [r];
                    _gaq.push(i), a ? C.notify("GA: sent command " + t + " to tracker " + a + (i.length > 1 ? " with parameters [" + i.slice(1).join(", ") + "]" : "") + ".", 1) : C.notify("GA: sent command " + t + (i.length > 1 ? " with parameters [" + i.slice(1).join(", ") + "]" : "") + ".", 1)
                }
            },
            $missing$: function (t, e, n, i) {
                this.call(t, e, n, i)
            },
            $postTransaction: function (e, n, i) {
                var a = C.data.customVars.transaction = t[i];
                this.call("addTrans", e, n, [a.orderID, a.affiliation, a.total, a.tax, a.shipping, a.city, a.state, a.country]), C.each(a.items, function (t) {
                    this.call("addItem", e, n, [t.orderID, t.sku, t.product, t.category, t.unitPrice, t.quantity])
                }, this), this.call("trackTrans", e, n)
            },
            delayLink: function (t, e) {
                var n = this;
                if (T.allowLinker() && t.hostname.match(this.settings.linkerDomains) && !C.isSubdomainOf(t.hostname, location.hostname)) {
                    C.preventDefault(e);
                    var i = C.settings.linkDelay || 100;
                    setTimeout(function () {
                        n.call("link", t, e, [t.href])
                    }, i)
                }
            },
            popupLink: function (e, n) {
                if (t._gat) {
                    C.preventDefault(n);
                    var i = this.settings.account,
                        a = t._gat._createTracker(i),
                        r = a._getLinkerUrl(e.href);
                    t.open(r)
                }
            },
            $link: function (t, e) {
                "_blank" === t.getAttribute("target") ? this.popupLink(t, e) : this.delayLink(t, e)
            },
            $trackEvent: function (t, e) {
                var n = Array.prototype.slice.call(arguments, 2);
                if (n.length >= 4 && null != n[3]) {
                    var i = parseInt(n[3], 10);
                    C.isNaN(i) && (i = 1), n[3] = i
                }
                this.call("trackEvent", t, e, n)
            }
        }), C.availableTools.ga = v, C.inherit(m, C.BaseTool), C.extend(m.prototype, {
            name: "GAUniversal",
            endPLPhase: function (t) {
                var e = this.settings,
                    n = e.loadOn;
                t === n && (C.notify("GAU: Initializing at " + t, 1), this.initialize(), this.flushQueue(), this.trackInitialPageView())
            },
            getTrackerName: function () {
                return this.settings.trackerSettings.name || ""
            },
            isPageCodeLoadSuppressed: function () {
                return this.settings.initTool === !1 || this._cancelToolInit === !0
            },
            initialize: function () {
                if (this.isPageCodeLoadSuppressed()) return this.initialized = !0, void C.notify("GAU: Page code not loaded (suppressed).", 1);
                var e = "ga";
                t[e] = t[e] || this.createGAObject(), t.GoogleAnalyticsObject = e, C.notify("GAU: Page code loaded.", 1), C.loadScriptOnce(this.getToolUrl());
                var n = this.settings;
                if (T.allowLinker() && n.allowLinker !== !1 ? this.createAccountForLinker() : this.createAccount(), this.executeInitCommands(), n.customInit) {
                    var i = n.customInit,
                        a = i(t[e], this.getTrackerName());
                    a === !1 && (this.suppressInitialPageView = !0)
                }
                this.initialized = !0
            },
            createGAObject: function () {
                var t = function () {
                    t.q.push(arguments)
                };
                return t.q = [], t.l = 1 * new Date, t
            },
            createAccount: function () {
                this.create()
            },
            createAccountForLinker: function () {
                var t = {};
                T.allowLinker() && (t.allowLinker = !0), this.create(t), this.call("require", "linker"), this.call("linker:autoLink", this.autoLinkDomains(), !1, !0)
            },
            create: function (t) {
                var e = this.settings.trackerSettings;
                e = C.preprocessArguments([e], location, null, this.forceLowerCase)[0], e.trackingId = C.replace(this.settings.trackerSettings.trackingId, location), e.cookieDomain || (e.cookieDomain = T.cookieDomain()), C.extend(e, t || {}), this.call("create", e)
            },
            autoLinkDomains: function () {
                var t = location.hostname;
                return C.filter(C.settings.domainList, function (e) {
                    return e !== t
                })
            },
            executeInitCommands: function () {
                var t = this.settings;
                t.initCommands && C.each(t.initCommands, function (t) {
                    var e = t.splice(2, t.length - 2);
                    t = t.concat(C.preprocessArguments(e, location, null, this.forceLowerCase)), this.call.apply(this, t)
                }, this)
            },
            trackInitialPageView: function () {
                this.suppressInitialPageView || this.isPageCodeLoadSuppressed() || this.call("send", "pageview")
            },
            call: function () {
                return "function" != typeof ga ? void C.notify("GA Universal function not found!", 4) : void (this.isCallSuppressed() || (arguments[0] = this.cmd(arguments[0]), this.log(C.toArray(arguments)), ga.apply(t, arguments)))
            },
            isCallSuppressed: function () {
                return this._cancelToolInit === !0
            },
            $missing$: function (t, e, n, i) {
                i = i || [], i = [t].concat(i), this.call.apply(this, i)
            },
            getToolUrl: function () {
                var t = this.settings,
                    e = C.isHttps();
                return t.url ? e ? t.url.https : t.url.http : (e ? "https://ssl" : "http://www") + ".google-analytics.com/analytics.js"
            },
            cmd: function (t) {
                var e = ["send", "set", "get"],
                    n = this.getTrackerName();
                return n && -1 !== C.indexOf(e, t) ? n + "." + t : t
            },
            log: function (t) {
                var e = t[0],
                    n = this.getTrackerName() || "default",
                    i = "GA Universal: sent command " + e + " to tracker " + n;
                if (t.length > 1) {
                    C.stringify(t.slice(1));
                    i += " with parameters " + C.stringify(t.slice(1))
                }
                i += ".", C.notify(i, 1)
            }
        }), C.availableTools.ga_universal = m;
        var T = {
            allowLinker: function () {
                return C.hasMultipleDomains()
            },
            cookieDomain: function () {
                var e = C.settings.domainList,
                    n = C.find(e, function (e) {
                        var n = t.location.hostname;
                        return C.equalsIgnoreCase(n.slice(n.length - e.length), e)
                    }),
                    i = n ? "." + n : "auto";
                return i
            }
        };
        C.inherit(y, C.BaseTool), C.extend(y.prototype, {
            name: "Nielsen",
            endPLPhase: function (t) {
                switch (t) {
                    case "pagetop":
                        this.initialize();
                        break;
                    case "pagebottom":
                        this.enableTracking && (this.queueCommand({
                            command: "sendFirstBeacon",
                            arguments: []
                        }), this.flushQueueWhenReady())
                }
            },
            defineListeners: function () {
                this.onTabFocus = C.bind(function () {
                    this.notify("Tab visible, sending view beacon when ready", 1), this.tabEverVisible = !0, this.flushQueueWhenReady()
                }, this), this.onPageLeave = C.bind(function () {
                    this.notify("isHuman? : " + this.isHuman(), 1), this.isHuman() && this.sendDurationBeacon()
                }, this), this.onHumanDetectionChange = C.bind(function (t) {
                    this == t.target.target && (this.human = t.target.isHuman)
                }, this)
            },
            initialize: function () {
                this.initializeTracking(), this.initializeDataProviders(), this.initializeNonHumanDetection(), this.tabEverVisible = C.visibility.isVisible(), this.tabEverVisible ? this.notify("Tab visible, sending view beacon when ready", 1) : C.bindEventOnce("tabfocus", this.onTabFocus), this.initialized = !0
            },
            initializeTracking: function () {
                this.initialized || (this.notify("Initializing tracking", 1), this.addRemovePageLeaveEvent(this.enableTracking), this.addRemoveHumanDetectionChangeEvent(this.enableTracking), this.initialized = !0)
            },
            initializeDataProviders: function () {
                var t, e = this.getAnalyticsTool();
                this.dataProvider.register(new y.DataProvider.VisitorID(C.getVisitorId())), e ? (t = new y.DataProvider.Generic("rsid", function () {
                    return e.settings.account
                }), this.dataProvider.register(t)) : this.notify("Missing integration with Analytics: rsid will not be sent.")
            },
            initializeNonHumanDetection: function () {
                C.nonhumandetection ? (C.nonhumandetection.init(), this.setEnableNonHumanDetection(0 == this.settings.enableNonHumanDetection ? !1 : !0), this.settings.nonHumanDetectionDelay > 0 && this.setNonHumanDetectionDelay(1e3 * parseInt(this.settings.nonHumanDetectionDelay))) : this.notify("NHDM is not available.")
            },
            getAnalyticsTool: function () {
                return this.settings.integratesWith ? C.tools[this.settings.integratesWith] : void 0
            },
            flushQueueWhenReady: function () {
                this.enableTracking && this.tabEverVisible && C.poll(C.bind(function () {
                    return this.isReadyToTrack() ? (this.flushQueue(), !0) : void 0
                }, this), 100, 20)
            },
            isReadyToTrack: function () {
                return this.tabEverVisible && this.dataProvider.isReady()
            },
            $setVars: function (t, e, n) {
                for (var i in n) {
                    var a = n[i];
                    "function" == typeof a && (a = a()), this.settings[i] = a
                }
                this.notify("Set variables done", 2), this.prepareContextData()
            },
            $setEnableTracking: function (t, e, n) {
                this.notify("Will" + (n ? "" : " not") + " track time on page", 1), this.enableTracking != n && (this.addRemovePageLeaveEvent(n), this.addRemoveHumanDetectionChangeEvent(n), this.enableTracking = n)
            },
            $sendFirstBeacon: function (t, e, n) {
                this.sendViewBeacon()
            },
            setEnableNonHumanDetection: function (t) {
                t ? C.nonhumandetection.register(this) : C.nonhumandetection.unregister(this)
            },
            setNonHumanDetectionDelay: function (t) {
                C.nonhumandetection.register(this, t)
            },
            addRemovePageLeaveEvent: function (t) {
                this.notify((t ? "Attach onto" : "Detach from") + " page leave event", 1);
                var e = 0 == t ? "unbindEvent" : "bindEvent";
                C[e]("leave", this.onPageLeave)
            },
            addRemoveHumanDetectionChangeEvent: function (t) {
                this.notify((t ? "Attach onto" : "Detach from") + " human detection change event", 1);
                var e = 0 == t ? "unbindEvent" : "bindEvent";
                C[e]("humandetection.change", this.onHumanDetectionChange)
            },
            sendViewBeacon: function () {
                this.notify("Tracked page view.", 1), this.sendBeaconWith()
            },
            sendDurationBeacon: function () {
                if (!C.timetracking || "function" != typeof C.timetracking.timeOnPage || null == C.timetracking.timeOnPage()) return void this.notify("Could not track close due missing time on page", 5);
                this.notify("Tracked close", 1), this.sendBeaconWith({
                    timeOnPage: Math.round(C.timetracking.timeOnPage() / 1e3),
                    duration: "D",
                    timer: "timer"
                });
                var t, e = "";
                for (t = 0; t < this.magicConst; t++) e += "0"
            },
            sendBeaconWith: function (t) {
                this.enableTracking && this[this.beaconMethod].call(this, this.prepareUrl(t))
            },
            plainBeacon: function (t) {
                var e = new Image;
                e.src = t, e.width = 1, e.height = 1, e.alt = ""
            },
            navigatorSendBeacon: function (t) {
                navigator.sendBeacon(t)
            },
            prepareUrl: function (t) {
                var e = this.settings;
                return C.extend(e, this.dataProvider.provide()), C.extend(e, t), this.preparePrefix(this.settings.collectionServer) + this.adapt.convertToURI(this.adapt.toNielsen(this.substituteVariables(e)))
            },
            preparePrefix: function (t) {
                return "//" + encodeURIComponent(t) + ".imrworldwide.com/cgi-bin/gn?"
            },
            substituteVariables: function (t) {
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[n] = C.replace(t[n]));
                return e
            },
            prepareContextData: function () {
                if (!this.getAnalyticsTool()) return void this.notify("Adobe Analytics missing.");
                var t = this.settings;
                t.sdkVersion = _satellite.publishDate, this.getAnalyticsTool().$setVars(null, null, {
                    contextData: this.adapt.toAnalytics(this.substituteVariables(t))
                })
            },
            isHuman: function () {
                return this.human
            },
            onTabFocus: function () { },
            onPageLeave: function () { },
            onHumanDetectionChange: function () { },
            notify: function (t, e) {
                C.notify(this.logPrefix + t, e)
            },
            beaconMethod: "plainBeacon",
            adapt: null,
            enableTracking: !1,
            logPrefix: "Nielsen: ",
            tabEverVisible: !1,
            human: !0,
            magicConst: 2e6
        }), y.DataProvider = {}, y.DataProvider.Generic = function (t, e) {
            this.key = t, this.valueFn = e
        }, C.extend(y.DataProvider.Generic.prototype, {
            isReady: function () {
                return !0
            },
            getValue: function () {
                return this.valueFn()
            },
            provide: function () {
                this.isReady() || y.prototype.notify("Not yet ready to provide value for: " + this.key, 5);
                var t = {};
                return t[this.key] = this.getValue(), t
            }
        }), y.DataProvider.VisitorID = function (t, e, n) {
            this.key = e || "uuid", this.visitorInstance = t, this.visitorInstance && (this.visitorId = t.getMarketingCloudVisitorID([this, this._visitorIdCallback])), this.fallbackProvider = n || new y.UUID
        }, C.inherit(y.DataProvider.VisitorID, y.DataProvider.Generic), C.extend(y.DataProvider.VisitorID.prototype, {
            isReady: function () {
                return null === this.visitorInstance ? !0 : !!this.visitorId
            },
            getValue: function () {
                return this.visitorId || this.fallbackProvider.get()
            },
            _visitorIdCallback: function (t) {
                this.visitorId = t
            }
        }), y.DataProvider.Aggregate = function () {
            this.providers = [];
            for (var t = 0; t < arguments.length; t++) this.register(arguments[t])
        }, C.extend(y.DataProvider.Aggregate.prototype, {
            register: function (t) {
                this.providers.push(t)
            },
            isReady: function () {
                return C.every(this.providers, function (t) {
                    return t.isReady()
                })
            },
            provide: function () {
                var t = {};
                return C.each(this.providers, function (e) {
                    C.extend(t, e.provide())
                }), t
            }
        }), y.UUID = function () { }, C.extend(y.UUID.prototype, {
            generate: function () {
                return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (t) {
                    var e = 16 * Math.random() | 0,
                        n = "x" == t ? e : 3 & e | 8;
                    return n.toString(16)
                })
            },
            get: function () {
                var t = C.readCookie(this.key("uuid"));
                return t ? t : (t = this.generate(), C.setCookie(this.key("uuid"), t), t)
            },
            key: function (t) {
                return "_dtm_nielsen_" + t
            }
        }), y.DataAdapters = function () { }, C.extend(y.DataAdapters.prototype, {
            toNielsen: function (t) {
                var e = (new Date).getTime(),
                    i = {
                        c6: "vc,",
                        c13: "asid,",
                        c15: "apn,",
                        c27: "cln,",
                        c32: "segA,",
                        c33: "segB,",
                        c34: "segC,",
                        c35: "adrsid,",
                        c29: "plid,",
                        c30: "bldv,",
                        c40: "adbid,"
                    },
                    a = {
                        ci: t.clientId,
                        c6: t.vcid,
                        c13: t.appId,
                        c15: t.appName,
                        prv: 1,
                        forward: 0,
                        ad: 0,
                        cr: t.duration || "V",
                        rt: "text",
                        st: "dcr",
                        prd: "dcr",
                        r: e,
                        at: t.timer || "view",
                        c16: t.sdkVersion,
                        c27: t.timeOnPage || 0,
                        c40: t.uuid,
                        c35: t.rsid,
                        ti: e,
                        sup: 0,
                        c32: t.segmentA,
                        c33: t.segmentB,
                        c34: t.segmentC,
                        asn: t.assetName,
                        c29: t.playerID,
                        c30: t.buildVersion
                    };
                for (key in a)
                    if (a[key] !== n && null != a[key] && a[key] !== n && null != a && "" != a) {
                        var r = encodeURIComponent(a[key]);
                        i.hasOwnProperty(key) && r && (r = i[key] + r), a[key] = r
                    }
                return this.filterObject(a)
            },
            toAnalytics: function (t) {
                return this.filterObject({
                    "a.nielsen.clientid": t.clientId,
                    "a.nielsen.vcid": t.vcid,
                    "a.nielsen.appid": t.appId,
                    "a.nielsen.appname": t.appName,
                    "a.nielsen.accmethod": "0",
                    "a.nielsen.ctype": "text",
                    "a.nielsen.sega": t.segmentA,
                    "a.nielsen.segb": t.segmentB,
                    "a.nielsen.segc": t.segmentC,
                    "a.nielsen.asset": t.assetName
                })
            },
            convertToURI: function (t) {
                if (C.isObject(t) === !1) return "";
                var e = [];
                for (var n in t) t.hasOwnProperty(n) && e.push(n + "=" + t[n]);
                return e.join("&")
            },
            filterObject: function (t) {
                for (var e in t) !t.hasOwnProperty(e) || null != t[e] && t[e] !== n || delete t[e];
                return t
            }
        }), C.availableTools.nielsen = y, C.inherit(b, C.BaseTool), C.extend(b.prototype, {
            name: "SC",
            endPLPhase: function (t) {
                var e = this.settings.loadOn;
                t === e && this.initialize(t)
            },
            initialize: function (e) {
                if (!this._cancelToolInit)
                    if (this.settings.initVars = this.substituteVariables(this.settings.initVars, {
                        type: e
                    }), this.settings.initTool !== !1) {
                        var n = this.settings.sCodeURL || C.basePath() + "s_code.js";
                        "object" == typeof n && (n = "https:" === t.location.protocol ? n.https : n.http), n.match(/^https?:/) || (n = C.basePath() + n), this.settings.initVars && this.$setVars(null, null, this.settings.initVars), C.loadScript(n, C.bind(this.onSCodeLoaded, this)), this.initializing = !0
                    } else this.initializing = !0, this.pollForSC()
            },
            getS: function (e, n) {
                var i = n && n.hostname || t.location.hostname,
                    a = this.concatWithToolVarBindings(n && n.setVars || this.varBindings),
                    r = n && n.addEvent || this.events,
                    o = this.getAccount(i),
                    s = t.s_gi;
                if (!s) return null;
                if (this.isValidSCInstance(e) || (e = null), !o && !e) return C.notify("Adobe Analytics: tracker not initialized because account was not found", 1), null;
                var e = e || s(o),
                    c = "D" + C.appVersion;
                "undefined" != typeof e.tagContainerMarker ? e.tagContainerMarker = c : "string" == typeof e.version && e.version.substring(e.version.length - 5) !== "-" + c && (e.version += "-" + c), e.sa && this.settings.skipSetAccount !== !0 && this.settings.initTool !== !1 && e.sa(this.settings.account), this.applyVarBindingsOnTracker(e, a), r.length > 0 && (e.events = r.join(","));
                var u = C.getVisitorId();
                return u && (e.visitor = C.getVisitorId()), e
            },
            onSCodeLoaded: function (t) {
                this.initialized = !0, this.initializing = !1;
                var e = ["Adobe Analytics: loaded", t ? " (manual)" : "", "."];
                C.notify(e.join(""), 1), C.fireEvent(this.id + ".load", this.getS()), t || (this.flushQueueExceptTrackLink(), this.sendBeacon()), this.flushQueue()
            },
            getAccount: function (e) {
                return t.s_account ? t.s_account : e && this.settings.accountByHost ? this.settings.accountByHost[e] || this.settings.account : this.settings.account
            },
            getTrackingServer: function () {
                var e = this,
                    n = e.getS();
                if (n) {
                    if (n.ssl && n.trackingServerSecure) return n.trackingServerSecure;
                    if (n.trackingServer) return n.trackingServer
                }
                var i = e.getAccount(t.location.hostname);
                if (!i) return null;
                var a, r, o, s = "",
                    c = n && n.dc;
                return a = i, r = a.indexOf(","), r >= 0 && (a = a.gb(0, r)), a = a.replace(/[^A-Za-z0-9]/g, ""), s || (s = "2o7.net"), c = c ? ("" + c).toLowerCase() : "d1", "2o7.net" == s && ("d1" == c ? c = "112" : "d2" == c && (c = "122"), o = ""), r = a + "." + c + "." + o + s
            },
            sendBeacon: function () {
                var e = this.getS(t[this.settings.renameS || "s"]);
                return e ? this.settings.customInit && this.settings.customInit(e) === !1 ? void C.notify("Adobe Analytics: custom init suppressed beacon", 1) : (this.settings.executeCustomPageCodeFirst && this.applyVarBindingsOnTracker(e, this.varBindings), this.executeCustomSetupFuns(e), e.t(), this.clearVarBindings(), this.clearCustomSetup(), void C.notify("Adobe Analytics: tracked page view", 1)) : void C.notify("Adobe Analytics: page code not loaded", 1)
            },
            pollForSC: function () {
                C.poll(C.bind(function () {
                    return "function" == typeof t.s_gi ? (this.onSCodeLoaded(!0), !0) : void 0
                }, this))
            },
            flushQueueExceptTrackLink: function () {
                if (this.pending) {
                    for (var t = [], e = 0; e < this.pending.length; e++) {
                        var n = this.pending[e],
                            i = n[0];
                        "trackLink" === i.command ? t.push(n) : this.triggerCommand.apply(this, n)
                    }
                    this.pending = t
                }
            },
            isQueueAvailable: function () {
                return !this.initialized
            },
            substituteVariables: function (t, e) {
                var n = {};
                for (var i in t)
                    if (t.hasOwnProperty(i)) {
                        var a = t[i];
                        n[i] = C.replace(a, location, e)
                    }
                return n
            },
            $setVars: function (t, e, n) {
                for (var i in n)
                    if (n.hasOwnProperty(i)) {
                        var a = n[i];
                        "function" == typeof a && (a = a()), this.varBindings[i] = a
                    }
                C.notify("Adobe Analytics: set variables.", 2)
            },
            $customSetup: function (t, e, n) {
                this.customSetupFuns.push(function (i) {
                    n.call(t, e, i)
                })
            },
            isValidSCInstance: function (t) {
                return !!t && "function" == typeof t.t && "function" == typeof t.tl
            },
            concatWithToolVarBindings: function (t) {
                var e = this.settings.initVars || {};
                return C.map(["trackingServer", "trackingServerSecure"], function (n) {
                    e[n] && !t[n] && (t[n] = e[n])
                }), t
            },
            applyVarBindingsOnTracker: function (t, e) {
                for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n])
            },
            clearVarBindings: function () {
                this.varBindings = {}
            },
            clearCustomSetup: function () {
                this.customSetupFuns = []
            },
            executeCustomSetupFuns: function (e) {
                C.each(this.customSetupFuns, function (n) {
                    n.call(t, e)
                })
            },
            $trackLink: function (t, e, n) {
                n = n || {};
                var i = n.type,
                    a = n.linkName;
                !a && t && t.nodeName && "a" === t.nodeName.toLowerCase() && (a = t.innerHTML), a || (a = "link clicked");
                var r = n && n.setVars,
                    o = n && n.addEvent || [],
                    s = this.getS(null, {
                        setVars: r,
                        addEvent: o
                    });
                if (!s) return void C.notify("Adobe Analytics: page code not loaded", 1);
                var c = s.linkTrackVars,
                    u = s.linkTrackEvents,
                    l = this.definedVarNames(r);
                n && n.customSetup && n.customSetup.call(t, e, s), o.length > 0 && l.push("events"), s.products && l.push("products"), l = this.mergeTrackLinkVars(s.linkTrackVars, l), o = this.mergeTrackLinkVars(s.linkTrackEvents, o), s.linkTrackVars = this.getCustomLinkVarsList(l);
                var d = C.map(o, function (t) {
                    return t.split(":")[0]
                });
                s.linkTrackEvents = this.getCustomLinkVarsList(d), s.tl(!0, i || "o", a), C.notify(["Adobe Analytics: tracked link ", "using: linkTrackVars=", C.stringify(s.linkTrackVars), "; linkTrackEvents=", C.stringify(s.linkTrackEvents)].join(""), 1), s.linkTrackVars = c, s.linkTrackEvents = u
            },
            mergeTrackLinkVars: function (t, e) {
                return t && (e = t.split(",").concat(e)),
                    e
            },
            getCustomLinkVarsList: function (t) {
                var e = C.indexOf(t, "None");
                return e > -1 && t.length > 1 && t.splice(e, 1), t.join(",")
            },
            definedVarNames: function (t) {
                t = t || this.varBindings;
                var e = [];
                for (var n in t) t.hasOwnProperty(n) && /^(eVar[0-9]+)|(prop[0-9]+)|(hier[0-9]+)|campaign|purchaseID|channel|server|state|zip|pageType$/.test(n) && e.push(n);
                return e
            },
            $trackPageView: function (t, e, n) {
                var i = n && n.setVars,
                    a = n && n.addEvent || [],
                    r = this.getS(null, {
                        setVars: i,
                        addEvent: a
                    });
                return r ? (r.linkTrackVars = "", r.linkTrackEvents = "", this.executeCustomSetupFuns(r), n && n.customSetup && n.customSetup.call(t, e, r), r.t(), this.clearVarBindings(), this.clearCustomSetup(), void C.notify("Adobe Analytics: tracked page view", 1)) : void C.notify("Adobe Analytics: page code not loaded", 1)
            },
            $postTransaction: function (e, n, i) {
                var a = C.data.transaction = t[i],
                    r = this.varBindings,
                    o = this.settings.fieldVarMapping;
                if (C.each(a.items, function (t) {
                        this.products.push(t)
                }, this), r.products = C.map(this.products, function (t) {
                        var e = [];
                        if (o && o.item)
                            for (var n in o.item)
                                if (o.item.hasOwnProperty(n)) {
                                    var i = o.item[n];
                                    e.push(i + "=" + t[n]), "event" === i.substring(0, 5) && this.events.push(i)
                }
                        var a = ["", t.product, t.quantity, t.unitPrice * t.quantity];
                        return e.length > 0 && a.push(e.join("|")), a.join(";")
                }, this).join(","), o && o.transaction) {
                    var s = [];
                    for (var c in o.transaction)
                        if (o.transaction.hasOwnProperty(c)) {
                            var i = o.transaction[c];
                            s.push(i + "=" + a[c]), "event" === i.substring(0, 5) && this.events.push(i)
                        }
                    r.products.length > 0 && (r.products += ","), r.products += ";;;;" + s.join("|")
                }
            },
            $addEvent: function (t, e) {
                for (var n = 2, i = arguments.length; i > n; n++) this.events.push(arguments[n])
            },
            $addProduct: function (t, e) {
                for (var n = 2, i = arguments.length; i > n; n++) this.products.push(arguments[n])
            }
        }), C.availableTools.sc = b, C.inherit(k, C.BaseTool), C.extend(k.prototype, {
            name: "tnt",
            endPLPhase: function (t) {
                "aftertoolinit" === t && this.initialize()
            },
            initialize: function () {
                C.notify("Test & Target: Initializing", 1), this.initializeTargetPageParams(), this.load()
            },
            initializeTargetPageParams: function () {
                t.targetPageParams && this.updateTargetPageParams(this.parseTargetPageParamsResult(t.targetPageParams())), this.updateTargetPageParams(this.settings.pageParams), this.setTargetPageParamsFunction()
            },
            load: function () {
                var t = this.getMboxURL(this.settings.mboxURL);
                this.settings.initTool !== !1 ? this.settings.loadSync ? (C.loadScriptSync(t), this.onScriptLoaded()) : (C.loadScript(t, C.bind(this.onScriptLoaded, this)), this.initializing = !0) : this.initialized = !0
            },
            getMboxURL: function (e) {
                var n = e;
                return C.isObject(e) && (n = "https:" === t.location.protocol ? e.https : e.http), n.match(/^https?:/) ? n : C.basePath() + n
            },
            onScriptLoaded: function () {
                C.notify("Test & Target: loaded.", 1), this.flushQueue(), this.initialized = !0, this.initializing = !1
            },
            $addMbox: function (t, e, n) {
                var i = n.mboxGoesAround,
                    a = i + "{visibility: hidden;}",
                    r = this.appendStyle(a);
                i in this.styleElements || (this.styleElements[i] = r), this.initialized ? this.$addMBoxStep2(null, null, n) : this.initializing && this.queueCommand({
                    command: "addMBoxStep2",
                    arguments: [n]
                }, t, e)
            },
            $addMBoxStep2: function (n, i, a) {
                var r = this.generateID(),
                    o = this;
                C.addEventHandler(t, "load", C.bind(function () {
                    C.cssQuery(a.mboxGoesAround, function (n) {
                        var i = n[0];
                        if (i) {
                            var s = e.createElement("div");
                            s.id = r, i.parentNode.replaceChild(s, i), s.appendChild(i), t.mboxDefine(r, a.mboxName);
                            var c = [a.mboxName];
                            a.arguments && (c = c.concat(a.arguments)), t.mboxUpdate.apply(null, c), o.reappearWhenCallComesBack(i, r, a.timeout, a)
                        }
                    })
                }, this)), this.lastMboxID = r
            },
            $addTargetPageParams: function (t, e, n) {
                this.updateTargetPageParams(n)
            },
            generateID: function () {
                var t = "_sdsat_mbox_" + String(Math.random()).substring(2) + "_";
                return t
            },
            appendStyle: function (t) {
                var n = e.getElementsByTagName("head")[0],
                    i = e.createElement("style");
                return i.type = "text/css", i.styleSheet ? i.styleSheet.cssText = t : i.appendChild(e.createTextNode(t)), n.appendChild(i), i
            },
            reappearWhenCallComesBack: function (t, e, n, i) {
                function a() {
                    var t = r.styleElements[i.mboxGoesAround];
                    t && (t.parentNode.removeChild(t), delete r.styleElements[i.mboxGoesAround])
                }
                var r = this;
                C.cssQuery('script[src*="omtrdc.net"]', function (t) {
                    var e = t[0];
                    if (e) {
                        C.scriptOnLoad(e.src, e, function () {
                            C.notify("Test & Target: request complete", 1), a(), clearTimeout(i)
                        });
                        var i = setTimeout(function () {
                            C.notify("Test & Target: bailing after " + n + "ms", 1), a()
                        }, n)
                    } else C.notify("Test & Target: failed to find T&T ajax call, bailing", 1), a()
                })
            },
            updateTargetPageParams: function (t) {
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[C.replace(n)] = C.replace(t[n]));
                C.extend(this.targetPageParamsStore, e)
            },
            getTargetPageParams: function () {
                return this.targetPageParamsStore
            },
            setTargetPageParamsFunction: function () {
                t.targetPageParams = C.bind(this.getTargetPageParams, this)
            },
            parseTargetPageParamsResult: function (t) {
                var e = t;
                return C.isArray(t) && (t = t.join("&")), C.isString(t) && (e = C.parseQueryParams(t)), e
            }
        }), C.availableTools.tnt = k, C.extend(S.prototype, {
            getInstance: function () {
                return this.instance
            },
            initialize: function () {
                var t, e = this.settings;
                C.notify("Visitor ID: Initializing tool", 1), t = this.createInstance(e.mcOrgId, e.initVars), null !== t && (e.customerIDs && this.applyCustomerIDs(t, e.customerIDs), e.autoRequest && t.getMarketingCloudVisitorID(), this.instance = t)
            },
            createInstance: function (t, e) {
                if (!C.isString(t)) return C.notify('Visitor ID: Cannot create instance using mcOrgId: "' + t + '"', 4), null;
                t = C.replace(t), C.notify('Visitor ID: Create instance using mcOrgId: "' + t + '"', 1), e = this.parseValues(e);
                var n = Visitor.getInstance(t, e);
                return C.notify("Visitor ID: Set variables: " + C.stringify(e), 1), n
            },
            applyCustomerIDs: function (t, e) {
                var n = this.parseIds(e);
                t.setCustomerIDs(n), C.notify("Visitor ID: Set Customer IDs: " + C.stringify(n), 1)
            },
            parseValues: function (t) {
                if (C.isObject(t) === !1) return {};
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[n] = C.replace(t[n]));
                return e
            },
            parseIds: function (t) {
                var e = {};
                if (C.isObject(t) === !1) return {};
                for (var n in t)
                    if (t.hasOwnProperty(n)) {
                        var i = C.replace(t[n].id);
                        i !== t[n].id && i && (e[n] = {}, e[n].id = i, e[n].authState = Visitor.AuthState[t[n].authState])
                    }
                return e
            }
        }), C.availableTools.visitor_id = S, _satellite.init({
            tools: {
                "7a6e5ffce8293a2f9ddf48fa88932347": {
                    engine: "sc",
                    loadOn: "pagebottom",
                    euCookie: !1,
                    sCodeURL: "omniture-scode-2.0.1.js",
                    renameS: "s_omntr",
                    initVars: {
                        trackInlineStats: !0,
                        trackDownloadLinks: !1,
                        trackExternalLinks: !1,
                        linkLeaveQueryString: !1,
                        dynamicVariablePrefix: "D="
                    },
                    skipSetAccount: !0
                },
                "915a71a14948aad5318b7120c6657500ebfe57b2": {
                    engine: "tnt",
                    mboxURL: "mbox-2.0.2.js",
                    loadSync: !0,
                    pageParams: {}
                },
                c56732f673c39bcf7bd835d7fe94e877a94bd7aa: {
                    engine: "visitor_id",
                    loadOn: "pagetop",
                    name: "VisitorID",
                    mcOrgId: "E1DC1042548EFE0F0A4C98A4@AdobeOrg",
                    autoRequest: !1,
                    initVars: {
                        trackingServer: "metrics.herbalife.com",
                        trackingServerSecure: "smetrics.herbalife.com",
                        marketingCloudServer: "metrics.herbalife.com",
                        marketingCloudServerSecure: "smetrics.herbalife.com",
                        loadTimeout: "1200"
                    }
                }
            },
            pageLoadRules: [{
                name: "Matt - Temp",
                trigger: [{
                    command: "writeHTML",
                    arguments: [{
                        html: '<!-- Temp Page load rule --> \n  <script>\n    console.log("TEMP - Adobe: Page load rule is working");\n</script>'
                    }]
                }],
                scope: {
                    protocols: [/https:/i]
                },
                event: "pagebottom"
            }],
            rules: [{
                name: "content:download link",
                trigger: [{
                    engine: "sc",
                    command: "trackLink",
                    arguments: [{
                        type: "d",
                        linkName: "File Download",
                        customSetup: function (t, e) {
                            var n = this.href,
                                i = n.replace(/\?.*$/, "").replace(/.*\//, "");
                            e.prop60 = i, e.eVar7 = "D=c60", e.events = "event4", e.linkTrackVars = "prop60,eVar7,events", e.linkTrackEvents = "event4"
                        }
                    }]
                }, {
                    command: "delayActivateLink"
                }],
                selector: "a",
                property: {
                    href: /\.(?:avi|css|csv|doc|docx|eps|exe|fla|gif|jpg|js|m4v|mov|mp3|mpg|pdf|png|pps|ppt|pptx|rar|tab|txt|vsd|vxd|wav|wma|wmv||xls|xlsx|xml|zip)($|\&|\?)/i
                },
                event: "click",
                bubbleFireIfParent: !0,
                bubbleFireIfChildFired: !0,
                bubbleStop: !1
            }, {
                name: "content:exit link",
                trigger: [{
                    engine: "sc",
                    command: "trackLink",
                    arguments: [{
                        type: "e",
                        linkName: "Exit Link",
                        customSetup: function (t, e) {
                            var n = this.href;
                            e.prop56 = n, e.linkTrackVars = "prop54,prop55,prop56";
                            var i = e.getPreviousValue(e.pageName, "s_ppn");
                            e.prop55 = "", i && (e.prop55 = e.getPercentPageViewed())
                        }
                    }]
                }, {
                    command: "delayActivateLink"
                }],
                conditions: [function (t, e) {
                    return _satellite.isOutboundLink(this)
                }],
                selector: "a",
                event: "click",
                bubbleFireIfParent: !0,
                bubbleFireIfChildFired: !0,
                bubbleStop: !1
            }],
            directCallRules: [],
            settings: {
                trackInternalLinks: !0,
                libraryName: "satelliteLib",
                isStaging: !1,
                allowGATTcalls: !1,
                downloadExtensions: /\.(?:doc|docx|eps|jpg|png|svg|xls|ppt|pptx|pdf|xlsx|tab|csv|zip|txt|vsd|vxd|xml|js|css|rar|exe|wma|mov|avi|wmv|mp3|wav|m4v)($|\&|\?)/i,
                notifications: !1,
                utilVisible: !1,
                domainList: ["myherbalife.com"],
                scriptDir: "/Scripts/dtm/",
                tagTimeout: 3e3
            },
            data: {
                URI: e.location.pathname + e.location.search,
                browser: {},
                cartItems: [],
                revenue: "",
                host: {
                    http: t.location.host + "/Scripts/dtm",
                    https: t.location.host + "/Scripts/dtm"
                }
            },
            dataElements: {
                Omntr_EncId: {
                    jsVariable: "_AnalyticsFacts_.EncId",
                    storeLength: "pageview"
                },
                Omntr_Id: {
                    jsVariable: "_AnalyticsFacts_.Id",
                    storeLength: "pageview"
                },
                Omntr_IsLoggedIn: {
                    jsVariable: "_AnalyticsFacts_.IsLoggedIn",
                    storeLength: "pageview"
                },
                Omntr_OmnitureSiteId: {
                    jsVariable: "_AnalyticsFacts_.OmnitureSiteId",
                    storeLength: "pageview"
                }
            },
            appVersion: "6I2",
            buildDate: "2016-08-02 16:23:34 UTC",
            publishDate: "2016-08-02 16:23:34 UTC"
        })
    }(window, document);