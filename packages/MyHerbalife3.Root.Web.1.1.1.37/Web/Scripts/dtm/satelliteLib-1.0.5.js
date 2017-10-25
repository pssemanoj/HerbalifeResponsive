/*
 ============== DO NOT ALTER ANYTHING BELOW THIS LINE ! ============

 Adobe Visitor API for JavaScript version: 1.8.0
 Copyright 1996-2015 Adobe, Inc. All Rights Reserved
 More info available at http://www.omniture.com
*/
function Visitor(t, e) {
    if (!t) throw "Visitor requires Adobe Marketing Cloud Org ID";
    var n = this;
    n.version = "1.8.0";
    var i = window,
        a = i.Visitor;
    a.version = n.version, i.s_c_in || (i.s_c_il = [], i.s_c_in = 0), n._c = "Visitor", n._il = i.s_c_il, n._in = i.s_c_in, n._il[n._in] = n, i.s_c_in++, n.la = {
        Ha: []
    };
    var r = i.document,
        o = a.Db;
    o || (o = null);
    var s = a.Eb;
    s || (s = void 0);
    var c = a.Pa;
    c || (c = !0);
    var u = a.Na;
    u || (u = !1), n.ha = function (t) {
        var e, n, i = 0;
        if (t)
            for (e = 0; e < t.length; e++) n = t.charCodeAt(e), i = (i << 5) - i + n, i &= i;
        return i
    }, n.r = function (t, e) {
        var n, i, a = "0123456789",
            r = "",
            o = "",
            s = 8,
            u = 10,
            l = 10;
        if (e === d && (D.isClientSideMarketingCloudVisitorID = c), 1 == t) {
            for (a += "ABCDEF", n = 0; 16 > n; n++) i = Math.floor(Math.random() * s), r += a.substring(i, i + 1), i = Math.floor(Math.random() * s), o += a.substring(i, i + 1), s = 16;
            return r + "-" + o
        }
        for (n = 0; 19 > n; n++) i = Math.floor(Math.random() * u), r += a.substring(i, i + 1), 0 == n && 9 == i ? u = 3 : (1 == n || 2 == n) && 10 != u && 2 > i ? u = 10 : n > 2 && (u = 10), i = Math.floor(Math.random() * l), o += a.substring(i, i + 1), 0 == n && 9 == i ? l = 3 : (1 == n || 2 == n) && 10 != l && 2 > i ? l = 10 : n > 2 && (l = 10);
        return r + o
    }, n.Ta = function () {
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
    }, n.h = o, n.L = function (t, e) {
        try {
            "function" == typeof t ? t.apply(i, e) : t[1].apply(t[0], e)
        } catch (n) { }
    }, n.Ya = function (t, e) {
        e && (n.h == o && (n.h = {}), n.h[t] == s && (n.h[t] = []), n.h[t].push(e))
    }, n.q = function (t, e) {
        if (n.h != o) {
            var i = n.h[t];
            if (i)
                for (; 0 < i.length;) n.L(i.shift(), e)
        }
    }, n.w = function (t, e, n, i) {
        if (n = encodeURIComponent(e) + "=" + encodeURIComponent(n), e = w.wb(t), t = w.ob(t), -1 === t.indexOf("?")) return t + "?" + n + e;
        var a = t.split("?"),
            t = a[0] + "?",
            i = w.ab(a[1], n, i);
        return t + i + e
    }, n.Sa = function (t, e) {
        var n = RegExp("[\\?&#]" + e + "=([^&#]*)").exec(t);
        return n && n.length ? decodeURIComponent(n[1]) : void 0
    }, n.Xa = function () {
        var t = o,
            e = i.location.href;
        try {
            var a = n.Sa(e, C.ba);
            if (a)
                for (var t = {}, r = a.split("|"), e = 0, s = r.length; s > e; e++) {
                    var c = r[e].split("=");
                    t[c[0]] = decodeURIComponent(c[1])
                }
            return t
        } catch (u) { }
    }, n.Qa = function () {
        var t = n.Xa();
        if (t) {
            var e = t[d],
                i = n.setMarketingCloudVisitorID;
            e && e.match(C.v) && i(e), n.j(S, -1), t = t[y], e = n.setAnalyticsVisitorID, t && t.match(C.v) && e(t)
        }
    }, n.l = o, n.Va = function (t, e, i, a) {
        e = n.w(e, "d_fieldgroup", t, 1), a.url = n.w(a.url, "d_fieldgroup", t, 1), a.m = n.w(a.m, "d_fieldgroup", t, 1), D.c[t] = c, a === Object(a) && a.m && "XMLHttpRequest" === n.na.F.G ? n.na.kb(a, i, t) : n.useCORSOnly || n.ka(t, e, i)
    }, n.ka = function (t, e, i) {
        var a, s = 0,
            u = 0;
        if (e && r) {
            for (a = 0; !s && 2 > a;) {
                try {
                    s = (s = r.getElementsByTagName(a > 0 ? "HEAD" : "head")) && 0 < s.length ? s[0] : 0
                } catch (l) {
                    s = 0
                }
                a++
            }
            if (!s) try {
                r.body && (s = r.body)
            } catch (d) {
                s = 0
            }
            if (s)
                for (a = 0; !u && 2 > a;) {
                    try {
                        u = r.createElement(a > 0 ? "SCRIPT" : "script")
                    } catch (h) {
                        u = 0
                    }
                    a++
                }
        }
        e && s && u ? (u.type = "text/javascript", u.src = e, s.firstChild ? s.insertBefore(u, s.firstChild) : s.appendChild(u), s = n.loadTimeout, P.c[t] = {
            requestStart: P.o(),
            url: e,
            wa: s,
            ua: P.Aa(),
            va: 0
        }, i && (n.l == o && (n.l = {}), n.l[t] = setTimeout(function () {
            i(c)
        }, s)), n.la.Ha.push(e)) : i && i()
    }, n.Ra = function (t) {
        n.l != o && n.l[t] && (clearTimeout(n.l[t]), n.l[t] = 0)
    }, n.ia = u, n.ja = u, n.isAllowed = function () {
        return !n.ia && (n.ia = c, n.cookieRead(n.cookieName) || n.cookieWrite(n.cookieName, "T", 1)) && (n.ja = c), n.ja
    }, n.b = o, n.e = o;
    var l = a.Vb;
    l || (l = "MC");
    var d = a.ac;
    d || (d = "MCMID");
    var h = a.Wb;
    h || (h = "MCCIDH");
    var f = a.Zb;
    f || (f = "MCSYNCS");
    var g = a.$b;
    g || (g = "MCSYNCSOP");
    var p = a.Xb;
    p || (p = "MCIDTS");
    var v = a.Yb;
    v || (v = "MCOPTOUT");
    var m = a.Tb;
    m || (m = "A");
    var y = a.Qb;
    y || (y = "MCAID");
    var b = a.Ub;
    b || (b = "AAM");
    var k = a.Sb;
    k || (k = "MCAAMLH");
    var S = a.Rb;
    S || (S = "MCAAMB");
    var E = a.bc;
    E || (E = "NONE"), n.N = 0, n.ga = function () {
        if (!n.N) {
            var t = n.version;
            n.audienceManagerServer && (t += "|" + n.audienceManagerServer), n.audienceManagerServerSecure && (t += "|" + n.audienceManagerServerSecure), n.N = n.ha(t)
        }
        return n.N
    }, n.ma = u, n.f = function () {
        if (!n.ma) {
            n.ma = c;
            var t, e, i, a, r = n.ga(),
                s = u,
                l = n.cookieRead(n.cookieName),
                d = new Date;
            if (n.b == o && (n.b = {}), l && "T" != l)
                for (l = l.split("|"), l[0].match(/^[\-0-9]+$/) && (parseInt(l[0], 10) != r && (s = c), l.shift()), 1 == l.length % 2 && l.pop(), r = 0; r < l.length; r += 2) t = l[r].split("-"), e = t[0], i = l[r + 1], 1 < t.length ? (a = parseInt(t[1], 10), t = 0 < t[1].indexOf("s")) : (a = 0, t = u), s && (e == h && (i = ""), a > 0 && (a = d.getTime() / 1e3 - 60)), e && i && (n.d(e, i, 1), a > 0 && (n.b["expire" + e] = a + (t ? "s" : ""), d.getTime() >= 1e3 * a || t && !n.cookieRead(n.sessionCookieName))) && (n.e || (n.e = {}), n.e[e] = c);
            !n.a(y) && (l = n.cookieRead("s_vi")) && (l = l.split("|"), 1 < l.length && 0 <= l[0].indexOf("v1") && (i = l[1], r = i.indexOf("["), r >= 0 && (i = i.substring(0, r)), i && i.match(C.v) && n.d(y, i)))
        }
    }, n.$a = function () {
        var t, e, i = n.ga();
        for (t in n.b) !Object.prototype[t] && n.b[t] && "expire" != t.substring(0, 6) && (e = n.b[t], i += (i ? "|" : "") + t + (n.b["expire" + t] ? "-" + n.b["expire" + t] : "") + "|" + e);
        n.cookieWrite(n.cookieName, i, 1)
    }, n.a = function (t, e) {
        return n.b == o || !e && n.e && n.e[t] ? o : n.b[t]
    }, n.d = function (t, e, i) {
        n.b == o && (n.b = {}), n.b[t] = e, i || n.$a()
    }, n.Ua = function (t, e) {
        var i = n.a(t, e);
        return i ? i.split("*") : o
    }, n.Za = function (t, e, i) {
        n.d(t, e ? e.join("*") : "", i)
    }, n.Kb = function (t, e) {
        var i = n.Ua(t, e);
        if (i) {
            var a, r = {};
            for (a = 0; a < i.length; a += 2) r[i[a]] = i[a + 1];
            return r
        }
        return o
    }, n.Mb = function (t, e, i) {
        var a, r = o;
        if (e)
            for (a in r = [], e) Object.prototype[a] || (r.push(a), r.push(e[a]));
        n.Za(t, r, i)
    }, n.j = function (t, e, i) {
        var a = new Date;
        a.setTime(a.getTime() + 1e3 * e), n.b == o && (n.b = {}), n.b["expire" + t] = Math.floor(a.getTime() / 1e3) + (i ? "s" : ""), 0 > e ? (n.e || (n.e = {}), n.e[t] = c) : n.e && (n.e[t] = u), i && (n.cookieRead(n.sessionCookieName) || n.cookieWrite(n.sessionCookieName, "1"))
    }, n.fa = function (t) {
        return t && ("object" == typeof t && (t = t.d_mid ? t.d_mid : t.visitorID ? t.visitorID : t.id ? t.id : t.uuid ? t.uuid : "" + t), t && (t = t.toUpperCase(), "NOTARGET" == t && (t = E)), !t || t != E && !t.match(C.v)) && (t = ""), t
    }, n.k = function (t, e) {
        if (n.Ra(t), n.i != o && (n.i[t] = u), P.c[t] && (P.c[t].Bb = P.o(), P.K(t)), D.c[t] && D.Ja(t, u), t == l) {
            D.isClientSideMarketingCloudVisitorID !== c && (D.isClientSideMarketingCloudVisitorID = u);
            var i = n.a(d);
            if (!i) {
                if (i = "object" == typeof e && e.mid ? e.mid : n.fa(e), !i) {
                    if (n.D) return void n.getAnalyticsVisitorID(o, u, c);
                    i = n.r(0, d)
                }
                n.d(d, i)
            }
            i && i != E || (i = ""), "object" == typeof e && ((e.d_region || e.dcs_region || e.d_blob || e.blob) && n.k(b, e), n.D && e.mid && n.k(m, {
                id: e.id
            })), n.q(d, [i])
        }
        if (t == b && "object" == typeof e) {
            i = 604800, e.id_sync_ttl != s && e.id_sync_ttl && (i = parseInt(e.id_sync_ttl, 10));
            var a = n.a(k);
            a || ((a = e.d_region) || (a = e.dcs_region), a && (n.j(k, i), n.d(k, a))), a || (a = ""), n.q(k, [a]), a = n.a(S), (e.d_blob || e.blob) && ((a = e.d_blob) || (a = e.blob), n.j(S, i), n.d(S, a)), a || (a = ""), n.q(S, [a]), !e.error_msg && n.B && n.d(h, n.B)
        }
        if (t == m && (i = n.a(y), i || ((i = n.fa(e)) ? i !== E && n.j(S, -1) : i = E, n.d(y, i)), i && i != E || (i = ""), n.q(y, [i])), n.idSyncDisableSyncs ? I.Ba = c : (I.Ba = u, i = {}, i.ibs = e.ibs, i.subdomain = e.subdomain, I.xb(i)), e === Object(e)) {
            var r;
            n.isAllowed() && (r = n.a(v)), r || (r = E, e.d_optout && e.d_optout instanceof Array && (r = e.d_optout.join(",")), i = parseInt(e.d_ottl, 10), isNaN(i) && (i = 7200), n.j(v, i, c), n.d(v, r)), n.q(v, [r])
        }
    }, n.i = o, n.s = function (t, e, i, a, r) {
        var s, u = "",
            h = w.qb(t);
        return !n.isAllowed() || (n.f(), u = n.a(t), n.disableThirdPartyCalls && !u && (t === d ? (u = n.r(0, d), n.setMarketingCloudVisitorID(u)) : t === y && !h && (u = "", n.setAnalyticsVisitorID(u))), u || n.disableThirdPartyCalls && !h) || (t == d || t == v ? s = l : t == k || t == S ? s = b : t == y && (s = m), !s) ? (t != d && t != y || u != E || (u = "", a = c), i && a && n.L(i, [u]), u) : (!e || n.i != o && n.i[s] || (n.i == o && (n.i = {}), n.i[s] = c, n.Va(s, e, function (e, i) {
            if (!n.a(t))
                if (P.c[s] && (P.c[s].timeout = P.o(), P.c[s].pb = !!e, P.K(s)), i !== Object(i) || n.useCORSOnly) {
                    e && D.Ja(s, c);
                    var a = "";
                    t == d ? a = n.r(0, d) : s == b && (a = {
                        error_msg: "timeout"
                    }), n.k(s, a)
                } else n.ka(s, i.url, i.I)
        }, r)), n.Ya(t, i), e || n.k(s, {
            id: E
        }), "")
    }, n._setMarketingCloudFields = function (t) {
        n.f(), n.k(l, t)
    }, n.setMarketingCloudVisitorID = function (t) {
        n._setMarketingCloudFields(t)
    }, n.D = u, n.getMarketingCloudVisitorID = function (t, e) {
        if (n.isAllowed()) {
            n.marketingCloudServer && 0 > n.marketingCloudServer.indexOf(".demdex.net") && (n.D = c);
            var i = n.A("_setMarketingCloudFields");
            return n.s(d, i.url, t, e, i)
        }
        return ""
    }, n.Wa = function () {
        n.getAudienceManagerBlob()
    }, a.AuthState = {
        UNKNOWN: 0,
        AUTHENTICATED: 1,
        LOGGED_OUT: 2
    }, n.z = {}, n.ea = u, n.B = "", n.setCustomerIDs = function (t) {
        if (n.isAllowed() && t) {
            n.f();
            var e, i;
            for (e in t)
                if (!Object.prototype[e] && (i = t[e]))
                    if ("object" == typeof i) {
                        var a = {};
                        i.id && (a.id = i.id), i.authState != s && (a.authState = i.authState), n.z[e] = a
                    } else n.z[e] = {
                        id: i
                    };
            var t = n.getCustomerIDs(),
                a = n.a(h),
                r = "";
            a || (a = 0);
            for (e in t) Object.prototype[e] || (i = t[e], r += (r ? "|" : "") + e + "|" + (i.id ? i.id : "") + (i.authState ? i.authState : ""));
            n.B = n.ha(r), n.B != a && (n.ea = c, n.Wa())
        }
    }, n.getCustomerIDs = function () {
        n.f();
        var t, e, i = {};
        for (t in n.z) Object.prototype[t] || (e = n.z[t], i[t] || (i[t] = {}), e.id && (i[t].id = e.id), i[t].authState = e.authState != s ? e.authState : a.AuthState.UNKNOWN);
        return i
    }, n._setAnalyticsFields = function (t) {
        n.f(), n.k(m, t)
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
                    s.m = r + "?" + a, s.ra = u
                }
                return s.url = o, n.s(i ? d : y, o, t, e, s)
            }
        }
        return ""
    }, n._setAudienceManagerFields = function (t) {
        n.f(), n.k(b, t)
    }, n.A = function (t) {
        var e = n.audienceManagerServer,
            i = "",
            a = n.a(d),
            r = n.a(S, c),
            o = n.a(y),
            o = o && o != E ? "&d_cid_ic=AVID%01" + encodeURIComponent(o) : "";
        if (n.loadSSL && n.audienceManagerServerSecure && (e = n.audienceManagerServerSecure), e) {
            var s, u, i = n.getCustomerIDs();
            if (i)
                for (s in i) Object.prototype[s] || (u = i[s], o += "&d_cid_ic=" + encodeURIComponent(s) + "%01" + encodeURIComponent(u.id ? u.id : "") + (u.authState ? "%01" + u.authState : ""));
            return t || (t = "_setAudienceManagerFields"), e = "http" + (n.loadSSL ? "s" : "") + "://" + e + "/id", a = "d_visid_ver=" + n.version + "&d_rtbd=json&d_ver=2" + (!a && n.D ? "&d_verify=1" : "") + "&d_orgid=" + encodeURIComponent(n.marketingCloudOrgID) + "&d_nsid=" + (n.idSyncContainerID || 0) + (a ? "&d_mid=" + encodeURIComponent(a) : "") + (n.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : "") + (r ? "&d_blob=" + encodeURIComponent(r) : "") + o, r = ["s_c_il", n._in, t], i = e + "?" + a + "&d_cb=s_c_il%5B" + n._in + "%5D." + t, {
                url: i,
                m: e + "?" + a,
                ra: r
            }
        }
        return {
            url: i
        }
    }, n.getAudienceManagerLocationHint = function (t, e) {
        if (n.isAllowed() && n.getMarketingCloudVisitorID(function () {
                n.getAudienceManagerLocationHint(t, c)
        })) {
            var i = n.a(y);
            if (i || (i = n.getAnalyticsVisitorID(function () {
                    n.getAudienceManagerLocationHint(t, c)
            })), i) return i = n.A(), n.s(k, i.url, t, e, i)
        }
        return ""
    }, n.getAudienceManagerBlob = function (t, e) {
        if (n.isAllowed() && n.getMarketingCloudVisitorID(function () {
                n.getAudienceManagerBlob(t, c)
        })) {
            var i = n.a(y);
            if (i || (i = n.getAnalyticsVisitorID(function () {
                    n.getAudienceManagerBlob(t, c)
            })), i) {
                var i = n.A(),
                    a = i.url;
                return n.ea && n.j(S, -1), n.s(S, a, t, e, i)
            }
        }
        return ""
    }, n.t = "", n.C = {}, n.O = "", n.P = {}, n.getSupplementalDataID = function (t, e) {
        !n.t && !e && (n.t = n.r(1));
        var i = n.t;
        return n.O && !n.P[t] ? (i = n.O, n.P[t] = c) : i && (n.C[t] && (n.O = n.t, n.P = n.C, n.t = i = e ? "" : n.r(1), n.C = {}), i && (n.C[t] = c)), i
    }, a.OptOut = {
        GLOBAL: "global"
    }, n.getOptOut = function (t, e) {
        if (n.isAllowed()) {
            var i = n.A("_setMarketingCloudFields");
            return n.s(v, i.url, t, e, i)
        }
        return ""
    }, n.isOptedOut = function (t, e, i) {
        return n.isAllowed() ? (e || (e = a.OptOut.GLOBAL), (i = n.getOptOut(function (i) {
            n.L(t, [i == a.OptOut.GLOBAL || 0 <= i.indexOf(e)])
        }, i)) ? i == a.OptOut.GLOBAL || 0 <= i.indexOf(e) : o) : u
    }, n.appendVisitorIDsTo = function (t) {
        for (var e = C.ba, i = [
                [d, n.a(d)],
                [y, n.a(y)]
        ], a = "", r = 0, s = i.length; s > r; r++) {
            var c = i[r],
                u = c[0],
                c = c[1];
            c != o && c !== E && (a = a ? a += "|" : a, a += u + "=" + encodeURIComponent(c))
        }
        try {
            return n.w(t, e, a)
        } catch (l) {
            return t
        }
    };
    var C = {
        p: !!i.postMessage,
        Ma: 1,
        da: 864e5,
        ba: "adobe_mc",
        v: /^[0-9a-fA-F\-]+$/
    };
    n.Fb = C, n.pa = {
        postMessage: function (t, e, n) {
            var i = 1;
            e && (C.p ? n.postMessage(t, e.replace(/([^:]+:\/\/[^\/]+).*/, "$1")) : e && (n.location = e.replace(/#.*$/, "") + "#" + +new Date + i++ + "&" + t))
        },
        X: function (t, e) {
            var n;
            try {
                C.p && (t && (n = function (n) {
                    return "string" == typeof e && n.origin !== e || "[object Function]" === Object.prototype.toString.call(e) && !1 === e(n.origin) ? !1 : void t(n)
                }), window.addEventListener ? window[t ? "addEventListener" : "removeEventListener"]("message", n, !1) : window[t ? "attachEvent" : "detachEvent"]("onmessage", n))
            } catch (i) { }
        }
    };
    var w = {
        Q: function () {
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
        jb: function (t, e) {
            return this.map(t, function (t) {
                return encodeURIComponent(t)
            }).join(e)
        },
        wb: function (t) {
            var e = t.indexOf("#");
            return e > 0 ? t.substr(e) : ""
        },
        ob: function (t) {
            var e = t.indexOf("#");
            return e > 0 ? t.substr(0, e) : t
        },
        ab: function (t, e, n) {
            return t = t.split("&"), n = n != o ? n : t.length, t.splice(n, 0, e), t.join("&")
        },
        qb: function (t, e, i) {
            return t !== y ? u : (e || (e = n.trackingServer), i || (i = n.trackingServerSecure), t = n.loadSSL ? i : e, "string" == typeof t && t.length ? 0 > t.indexOf("2o7.net") && 0 > t.indexOf("omtrdc.net") : u)
        }
    };
    n.Lb = w;
    var T = {
        F: function () {
            var t = "none",
                e = c;
            return "undefined" != typeof XMLHttpRequest && XMLHttpRequest === Object(XMLHttpRequest) && ("withCredentials" in new XMLHttpRequest ? t = "XMLHttpRequest" : new Function("/*@cc_on return /^10/.test(@_jscript_version) @*/")() ? t = "XMLHttpRequest" : "undefined" != typeof XDomainRequest && XDomainRequest === Object(XDomainRequest) && (e = u), 0 < Object.prototype.toString.call(window.Cb).indexOf("Constructor") && (e = u)), {
                G: t,
                Ob: e
            }
        }(),
        lb: function () {
            return "none" === this.F.G ? o : new window[this.F.G]
        },
        kb: function (t, e, i) {
            var a = this;
            e && (t.I = e);
            try {
                var r = this.lb();
                r.open("get", t.m + "&ts=" + (new Date).getTime(), c), "XMLHttpRequest" === this.F.G && (r.withCredentials = c, r.timeout = n.loadTimeout, r.setRequestHeader("Content-Type", "application/x-www-form-urlencoded"), r.onreadystatechange = function () {
                    if (4 === this.readyState && 200 === this.status) t: {
                        var e;
                        try {
                            if (e = JSON.parse(this.responseText), e !== Object(e)) {
                                a.n(t, o, "Response is not JSON");
                                break t
                            }
                        } catch (n) {
                            a.n(t, n, "Error parsing response as JSON");
                            break t
                        }
                        try {
                            for (var i = t.ra, r = window, s = 0; s < i.length; s++) r = r[i[s]];
                            r(e)
                        } catch (c) {
                            a.n(t, c, "Error forming callback function")
                        }
                    }
                }), r.onerror = function (e) {
                    a.n(t, e, "onerror")
                }, r.ontimeout = function (e) {
                    a.n(t, e, "ontimeout")
                }, r.send(), P.c[i] = {
                    requestStart: P.o(),
                    url: t.m,
                    wa: r.timeout,
                    ua: P.Aa(),
                    va: 1
                }, n.la.Ha.push(t.m)
            } catch (s) {
                this.n(t, s, "try-catch")
            }
        },
        n: function (t, e, i) {
            n.CORSErrors.push({
                Pb: t,
                error: e,
                description: i
            }), t.I && ("ontimeout" === i ? t.I(c) : t.I(u, t))
        }
    };
    n.na = T;
    var I = {
        Oa: 3e4,
        ca: 649,
        La: u,
        id: o,
        W: [],
        T: o,
        za: function (t) {
            return "string" == typeof t ? (t = t.split("/"), t[0] + "//" + t[2]) : void 0
        },
        g: o,
        url: o,
        mb: function () {
            var t = "http://fast.",
                e = "?d_nsid=" + n.idSyncContainerID + "#" + encodeURIComponent(r.location.href);
            return this.g || (this.g = "nosubdomainreturned"), n.loadSSL && (t = n.idSyncSSLUseAkamai ? "https://fast." : "https://"), t = t + this.g + ".demdex.net/dest5.html" + e, this.T = this.za(t), this.id = "destination_publishing_iframe_" + this.g + "_" + n.idSyncContainerID, t
        },
        eb: function () {
            var t = "?d_nsid=" + n.idSyncContainerID + "#" + encodeURIComponent(r.location.href);
            "string" == typeof n.M && n.M.length && (this.id = "destination_publishing_iframe_" + (new Date).getTime() + "_" + n.idSyncContainerID, this.T = this.za(n.M), this.url = n.M + t)
        },
        Ba: o,
        xa: u,
        Z: u,
        H: o,
        cc: o,
        vb: o,
        dc: o,
        Y: u,
        J: [],
        tb: [],
        ub: [],
        Da: C.p ? 15 : 100,
        U: [],
        rb: [],
        sa: c,
        Ga: u,
        Fa: function () {
            return !n.idSyncDisable3rdPartySyncing && (this.xa || n.Hb) && this.g && "nosubdomainreturned" !== this.g && this.url && !this.Z
        },
        R: function () {
            function t() {
                i = document.createElement("iframe"), i.sandbox = "allow-scripts allow-same-origin", i.title = "Adobe ID Syncing iFrame", i.id = n.id, i.style.cssText = "display: none; width: 0; height: 0;", i.src = n.url, n.vb = c, e(), document.body.appendChild(i)
            }

            function e() {
                w.Q(i, "load", function () {
                    i.className = "aamIframeLoaded", n.H = c, n.u()
                })
            }
            this.Z = c;
            var n = this,
                i = document.getElementById(this.id);
            i ? "IFRAME" !== i.nodeName ? (this.id += "_2", t()) : "aamIframeLoaded" !== i.className ? e() : (this.H = c, this.Ca = i, this.u()) : t(), this.Ca = i
        },
        u: function (t) {
            var e = this;
            t === Object(t) && (this.U.push(t), this.yb(t)), (this.Ga || !C.p || this.H) && this.U.length && (this.K(this.U.shift()), this.u()), !n.idSyncDisableSyncs && this.H && this.J.length && !this.Y && (this.La || (this.La = c, setTimeout(function () {
                e.Da = C.p ? 15 : 150
            }, this.Oa)), this.Y = c, this.Ia())
        },
        yb: function (t) {
            var e, n, i;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (t = 0; n > t; t++) i = e[t], i.syncOnPage && this.ta(i, "", "syncOnPage")
        },
        K: function (t) {
            var e, n, i, a, r, o = encodeURIComponent;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (i = 0; n > i; i++) a = e[i], r = [o("ibs"), o(a.id || ""), o(a.tag || ""), w.jb(a.url || [], ","), o(a.ttl || ""), "", "", a.fireURLSync ? "true" : "false"], a.syncOnPage || (this.sa ? this.qa(r.join("|")) : a.fireURLSync && this.ta(a, r.join("|")));
            this.rb.push(t)
        },
        ta: function (t, e, i) {
            var a = (i = "syncOnPage" === i ? c : u) ? g : f;
            n.f();
            var r = n.a(a),
                o = u,
                s = u,
                l = Math.ceil((new Date).getTime() / C.da);
            r ? (r = r.split("*"), s = this.zb(r, t.id, l), o = s.hb, s = s.ib, (!o || !s) && this.ya(i, t, e, r, a, l)) : (r = [], this.ya(i, t, e, r, a, l))
        },
        zb: function (t, e, n) {
            var i, a, r, o = u,
                s = u;
            for (a = 0; a < t.length; a++) i = t[a], r = parseInt(i.split("-")[1], 10), i.match("^" + e + "-") ? (o = c, r > n ? s = c : (t.splice(a, 1), a--)) : n >= r && (t.splice(a, 1), a--);
            return {
                hb: o,
                ib: s
            }
        },
        sb: function (t) {
            if (t.join("*").length > this.ca)
                for (t.sort(function (t, e) {
                        return parseInt(t.split("-")[1], 10) - parseInt(e.split("-")[1], 10)
                }) ; t.join("*").length > this.ca;) t.shift()
        },
        ya: function (t, e, i, a, r, s) {
            var c = this;
            if (t) {
                if ("img" === e.tag) {
                    var u, l, d, t = e.url,
                        i = n.loadSSL ? "https:" : "http:";
                    for (a = 0, u = t.length; u > a; a++) {
                        l = t[a], d = /^\/\//.test(l);
                        var h = new Image;
                        w.Q(h, "load", function (t, e, i, a) {
                            return function () {
                                c.W[t] = o, n.f();
                                var s = n.a(r),
                                    u = [];
                                if (s) {
                                    var l, d, h, s = s.split("*");
                                    for (l = 0, d = s.length; d > l; l++) h = s[l], h.match("^" + e.id + "-") || u.push(h)
                                }
                                c.Ka(u, e, i, a)
                            }
                        }(this.W.length, e, r, s)), h.src = (d ? i : "") + l, this.W.push(h)
                    }
                }
            } else this.qa(i), this.Ka(a, e, r, s)
        },
        qa: function (t) {
            var e = encodeURIComponent;
            this.J.push(e(n.Ib ? "---destpub-debug---" : "---destpub---") + t)
        },
        Ka: function (t, e, i, a) {
            t.push(e.id + "-" + (a + Math.ceil(e.ttl / 60 / 24))), this.sb(t), n.d(i, t.join("*"))
        },
        Ia: function () {
            var t, e = this;
            this.J.length ? (t = this.J.shift(), n.pa.postMessage(t, this.url, this.Ca.contentWindow), this.tb.push(t), setTimeout(function () {
                e.Ia()
            }, this.Da)) : this.Y = u
        },
        X: function (t) {
            var e = /^---destpub-to-parent---/;
            "string" == typeof t && e.test(t) && (e = t.replace(e, "").split("|"), "canSetThirdPartyCookies" === e[0] && (this.sa = "true" === e[1] ? c : u, this.Ga = c, this.u()), this.ub.push(t))
        },
        xb: function (t) {
            (this.url === o || t.subdomain && "nosubdomainreturned" === this.g) && (this.g = "string" == typeof n.oa && n.oa.length ? n.oa : t.subdomain || "", this.url = this.mb()), t.ibs instanceof Array && t.ibs.length && (this.xa = c), this.Fa() && (n.idSyncAttachIframeOnWindowLoad ? (a.aa || "complete" === r.readyState || "loaded" === r.readyState) && this.R() : this.bb()), "function" == typeof n.idSyncIDCallResult ? n.idSyncIDCallResult(t) : this.u(t), "function" == typeof n.idSyncAfterIDCallResult && n.idSyncAfterIDCallResult(t)
        },
        cb: function (t, e) {
            return n.Jb || !t || e - t > C.Ma
        },
        bb: function () {
            function t() {
                e.Z || (document.body ? e.R() : setTimeout(t, 30))
            }
            var e = this;
            t()
        }
    };
    n.Gb = I, n.timeoutMetricsLog = [];
    var P = {
        gb: window.performance && window.performance.timing ? 1 : 0,
        Ea: window.performance && window.performance.timing ? window.performance.timing : o,
        $: o,
        S: o,
        c: {},
        V: [],
        send: function (t) {
            if (n.takeTimeoutMetrics && t === Object(t)) {
                var e, i = [],
                    a = encodeURIComponent;
                for (e in t) t.hasOwnProperty(e) && i.push(a(e) + "=" + a(t[e]));
                t = "http" + (n.loadSSL ? "s" : "") + "://dpm.demdex.net/event?d_visid_ver=" + n.version + "&d_visid_stg_timeout=" + n.loadTimeout + "&" + i.join("&") + "&d_orgid=" + a(n.marketingCloudOrgID) + "&d_timingapi=" + this.gb + "&d_winload=" + this.nb() + "&d_ld=" + this.o(), (new Image).src = t, n.timeoutMetricsLog.push(t)
            }
        },
        nb: function () {
            return this.S === o && (this.S = this.Ea ? this.$ - this.Ea.navigationStart : this.$ - a.fb), this.S
        },
        o: function () {
            return (new Date).getTime()
        },
        K: function (t) {
            var e = this.c[t],
                n = {};
            n.d_visid_stg_timeout_captured = e.wa, n.d_visid_cors = e.va, n.d_fieldgroup = t, n.d_settimeout_overriden = e.ua, e.timeout ? e.pb ? (n.d_visid_timedout = 1, n.d_visid_timeout = e.timeout - e.requestStart, n.d_visid_response = -1) : (n.d_visid_timedout = "n/a", n.d_visid_timeout = "n/a", n.d_visid_response = "n/a") : (n.d_visid_timedout = 0, n.d_visid_timeout = -1, n.d_visid_response = e.Bb - e.requestStart), n.d_visid_url = e.url, a.aa ? this.send(n) : this.V.push(n), delete this.c[t]
        },
        Ab: function () {
            for (var t = 0, e = this.V.length; e > t; t++) this.send(this.V[t])
        },
        Aa: function () {
            return "function" == typeof setTimeout.toString ? -1 < setTimeout.toString().indexOf("[native code]") ? 0 : 1 : -1
        }
    };
    n.Nb = P;
    var D = {
        isClientSideMarketingCloudVisitorID: o,
        MCIDCallTimedOut: o,
        AnalyticsIDCallTimedOut: o,
        AAMIDCallTimedOut: o,
        c: {},
        Ja: function (t, e) {
            switch (t) {
                case l:
                    e === u ? this.MCIDCallTimedOut !== c && (this.MCIDCallTimedOut = u) : this.MCIDCallTimedOut = e;
                    break;
                case m:
                    e === u ? this.AnalyticsIDCallTimedOut !== c && (this.AnalyticsIDCallTimedOut = u) : this.AnalyticsIDCallTimedOut = e;
                    break;
                case b:
                    e === u ? this.AAMIDCallTimedOut !== c && (this.AAMIDCallTimedOut = u) : this.AAMIDCallTimedOut = e
            }
        }
    };
    if (n.isClientSideMarketingCloudVisitorID = function () {
            return D.isClientSideMarketingCloudVisitorID
    }, n.MCIDCallTimedOut = function () {
            return D.MCIDCallTimedOut
    }, n.AnalyticsIDCallTimedOut = function () {
            return D.AnalyticsIDCallTimedOut
    }, n.AAMIDCallTimedOut = function () {
            return D.AAMIDCallTimedOut
    }, n.idSyncGetOnPageSyncInfo = function () {
            return n.f(), n.a(g)
    }, 0 > t.indexOf("@") && (t += "@AdobeOrg"), n.marketingCloudOrgID = t, n.cookieName = "AMCV_" + t, n.sessionCookieName = "AMCVS_" + t, n.cookieDomain = n.Ta(), n.cookieDomain == i.location.hostname && (n.cookieDomain = ""), n.loadSSL = 0 <= i.location.protocol.toLowerCase().indexOf("https"), n.loadTimeout = 3e4, n.CORSErrors = [], n.marketingCloudServer = n.audienceManagerServer = "dpm.demdex.net", n.Qa(), e && "object" == typeof e) {
        for (var L in e) !Object.prototype[L] && (n[L] = e[L]);
        n.idSyncContainerID = n.idSyncContainerID || 0, n.f(), T = n.a(p), L = Math.ceil((new Date).getTime() / C.da), !n.idSyncDisableSyncs && I.cb(T, L) && (n.j(S, -1), n.d(p, L)), n.getMarketingCloudVisitorID(), n.getAudienceManagerLocationHint(), n.getAudienceManagerBlob()
    }
    if (!n.idSyncDisableSyncs) {
        I.eb(), w.Q(window, "load", function () {
            a.aa = c, P.$ = P.o(), P.Ab();
            var t = I;
            t.Fa() && t.R()
        });
        try {
            n.pa.X(function (t) {
                I.X(t.data)
            }, I.T)
        } catch (O) { }
    }
}
Visitor.getInstance = function (t, e) {
    var n, i, a = window.s_c_il;
    if (0 > t.indexOf("@") && (t += "@AdobeOrg"), a)
        for (i = 0; i < a.length; i++)
            if ((n = a[i]) && "Visitor" == n._c && n.marketingCloudOrgID == t) return n;
    return new Visitor(t, e)
},
    function () {
        function t() {
            e.aa = n
        }
        var e = window.Visitor,
            n = e.Pa,
            i = e.Na;
        n || (n = !0), i || (i = !1), window.addEventListener ? window.addEventListener("load", t) : window.attachEvent && window.attachEvent("onload", t), e.fb = (new Date).getTime()
    }(),
    // All code and conventions are protected by copyright
    function (t, e, n) {
        function i() {
            w.addEventHandler(t, "orientationchange", i.orientationChange)
        }

        function a(t) {
            this.delay = 250, this.FB = t, w.domReady(w.bind(function () {
                w.poll(w.bind(this.initialize, this), this.delay, 8)
            }, this))
        }

        function r() {
            this.rules = w.filter(w.rules, function (t) {
                return "videoplayed" === t.event.substring(0, 11)
            }), this.eventHandler = w.bind(this.onUpdateTime, this)
        }

        function o() {
            this.rules = w.filter(w.rules, function (t) {
                return "elementexists" === t.event
            })
        }

        function s() {
            w.getToolsByType("nielsen").length > 0 && w.domReady(w.bind(this.initialize, this))
        }

        function c() {
            this.lastURL = w.URL(), this._fireIfURIChanged = w.bind(this.fireIfURIChanged, this), this._onPopState = w.bind(this.onPopState, this), this._onHashChange = w.bind(this.onHashChange, this), this._pushState = w.bind(this.pushState, this), this._replaceState = w.bind(this.replaceState, this), this.initialize()
        }

        function u() {
            var t = w.filter(w.rules, function (t) {
                return 0 === t.event.indexOf("dataelementchange")
            });
            this.dataElementsNames = w.map(t, function (t) {
                var e = t.event.match(/dataelementchange\((.*)\)/i);
                return e[1]
            }, this), this.initPolling()
        }

        function l() {
            var t = this.eventRegex = /^hover\(([0-9]+)\)$/,
                e = this.rules = [];
            w.each(w.rules, function (n) {
                var i = n.event.match(t);
                i && e.push([Number(n.event.match(t)[1]), n.selector])
            })
        }

        function d() {
            this.defineEvents(), this.visibilityApiHasPriority = !0, e.addEventListener ? this.setVisibilityApiPriority(!1) : this.attachDetachOlderEventListeners(!0, e, "focusout");
            w.bindEvent("aftertoolinit", function () {
                w.fireEvent(w.visibility.isHidden() ? "tabblur" : "tabfocus")
            })
        }

        function h(e) {
            w.domReady(w.bind(function () {
                this.twttr = e || t.twttr, this.initialize()
            }, this))
        }

        function f(e) {
            e = e || w.rules, this.rules = w.filter(e, function (t) {
                return "inview" === t.event
            }), this.elements = [], this.eventHandler = w.bind(this.track, this), w.addEventHandler(t, "scroll", this.eventHandler), w.addEventHandler(t, "load", this.eventHandler)
        }

        function g(t) {
            w.BaseTool.call(this, t), this.name = t.name || "Basic"
        }

        function p(t) {
            w.BaseTool.call(this, t), this.name = t.name || "VisitorID", this.initialize()
        }

        function v() {
            w.BaseTool.call(this), this.asyncScriptCallbackQueue = [], this.argsForBlockingScripts = []
        }

        function m(t) {
            w.BaseTool.call(this, t), this.defineListeners(), this.beaconMethod = "plainBeacon", this.adapt = new m.DataAdapters, this.dataProvider = new m.DataProvider.Aggregate
        }

        function y(t) {
            w.BaseTool.call(this, t), this.styleElements = {}, this.targetPageParamsStore = {}
        }

        function b(t) {
            w.BaseTool.call(this, t), this.varBindings = {}, this.events = [], this.products = [], this.customSetupFuns = []
        }

        function k(t) {
            w.BaseTool.call(this, t)
        }

        function S(t) {
            w.BaseTool.call(this, t)
        }
        var E = Object.prototype.toString,
            C = t._satellite && t._satellite.override,
            w = {
                initialized: !1,
                $data: function (t, e, i) {
                    if (t) {
                        var a = "__satellite__",
                            r = w.dataCache,
                            o = t[a];
                        o || (o = t[a] = w.uuid++);
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
                    return null != t && !w.isArray(t) && "object" == typeof t
                },
                isString: function (t) {
                    return "string" == typeof t
                },
                isNumber: function (t) {
                    return "[object Number]" === E.apply(t) && !w.isNaN(t)
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
                    return -1 !== w.indexOf(t, e)
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
                    if (e = e || [], w.isObject(t)) {
                        if (w.contains(e, t)) return "<Cycle>";
                        e.push(t)
                    }
                    if (w.isArray(t)) return "[" + w.map(t, function (t) {
                        return w.stringify(t, e)
                    }).join(",") + "]";
                    if (w.isString(t)) return '"' + String(t) + '"';
                    if (w.isObject(t)) {
                        var n = [];
                        for (var i in t) t.hasOwnProperty(i) && n.push(i + ": " + w.stringify(t[i], e));
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
                    w.scriptOnLoad(t, i, n), i.src = t, e.getElementsByTagName("head")[0].appendChild(i)
                },
                scriptOnLoad: function (t, e, n) {
                    function i(t) {
                        t && w.logError(t), n && n(t)
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
                    w.loadedScriptRegistry[t] || w.loadScript(t, function (n) {
                        n || (w.loadedScriptRegistry[t] = !0), e && e(n)
                    })
                },
                loadedScriptRegistry: {},
                loadScriptSync: function (t) {
                    return e.write ? w.domReadyFired ? void w.notify('Cannot load sync the "' + t + '" script after DOM Ready.', 1) : (t.indexOf('"') > -1 && (t = encodeURI(t)), void e.write('<script src="' + t + '"></script>')) : void w.notify('Cannot load sync the "' + t + '" script because "document.write" is not available', 1)
                },
                pushAsyncScript: function (t) {
                    w.tools["default"].pushAsyncScript(t)
                },
                pushBlockingScript: function (t) {
                    w.tools["default"].pushBlockingScript(t)
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
                            return w.Sizzle.matches(t, [e]).length > 0
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
                        if (w.Sizzle) {
                            var n;
                            try {
                                n = w.Sizzle(t)
                            } catch (i) {
                                n = []
                            }
                            e(n)
                        } else w.sizzleQueue.push([t, e])
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
                        w.isNumber(n) && a++ >= n || t() || setTimeout(i, e)
                    }
                    var a = 0;
                    e = e || 1e3, i()
                },
                escapeForHtml: function (t) {
                    return t ? String(t).replace(/\&/g, "&amp;").replace(/\</g, "&lt;").replace(/\>/g, "&gt;").replace(/\"/g, "&quot;").replace(/\'/g, "&#x27;").replace(/\//g, "&#x2F;") : t
                }
            };
        w.availableTools = {}, w.availableEventEmitters = [], w.fireOnceEvents = ["condition", "elementexists"], w.initEventEmitters = function () {
            w.eventEmitters = w.map(w.availableEventEmitters, function (t) {
                return new t
            })
        }, w.eventEmitterBackgroundTasks = function () {
            w.each(w.eventEmitters, function (t) {
                "backgroundTasks" in t && t.backgroundTasks()
            })
        }, w.initTools = function (t) {
            var e = {
                "default": new v
            },
                n = w.settings.euCookieName || "sat_track";
            for (var i in t)
                if (t.hasOwnProperty(i)) {
                    var a, r, o;
                    if (a = t[i], a.euCookie) {
                        var s = "true" !== w.readCookie(n);
                        if (s) continue
                    }
                    if (r = w.availableTools[a.engine], !r) {
                        var c = [];
                        for (var u in w.availableTools) w.availableTools.hasOwnProperty(u) && c.push(u);
                        throw new Error("No tool engine named " + a.engine + ", available: " + c.join(",") + ".")
                    }
                    o = new r(a), o.id = i, e[i] = o
                }
            return e
        }, w.preprocessArguments = function (t, e, n, i, a) {
            function r(t) {
                return i && w.isString(t) ? t.toLowerCase() : t
            }

            function o(t) {
                var c = {};
                for (var u in t)
                    if (t.hasOwnProperty(u)) {
                        var l = t[u];
                        w.isObject(l) ? c[u] = o(l) : w.isArray(l) ? c[u] = s(l, i) : c[u] = r(w.replace(l, e, n, a))
                    }
                return c
            }

            function s(t, i) {
                for (var a = [], s = 0, c = t.length; c > s; s++) {
                    var u = t[s];
                    w.isString(u) ? u = r(w.replace(u, e, n)) : u && u.constructor === Object && (u = o(u)), a.push(u)
                }
                return a
            }
            return t ? s(t, i) : t
        }, w.execute = function (t, e, n, i) {
            function a(a) {
                var r = i[a || "default"];
                if (r) try {
                    r.triggerCommand(t, e, n)
                } catch (o) {
                    w.logError(o)
                }
            }
            if (!_satellite.settings.hideActivity)
                if (i = i || w.tools, t.engine) {
                    var r = t.engine;
                    for (var o in i)
                        if (i.hasOwnProperty(o)) {
                            var s = i[o];
                            s.settings && s.settings.engine === r && a(o)
                        }
                } else t.tool instanceof Array ? w.each(t.tool, function (t) {
                    a(t)
                }) : a(t.tool)
        }, w.Logger = {
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
                this.flushed || (w.each(this.messages, function (t) {
                    t[2] !== !0 && (this.echo(t[0], t[1]), t[2] = !0)
                }, this), this.flushed = !0)
            }
        }, w.notify = w.bind(w.Logger.message, w.Logger), w.cleanText = function (t) {
            return null == t ? null : w.trim(t).replace(/\s+/g, " ")
        }, w.cleanText.legacy = function (t) {
            return null == t ? null : w.trim(t).replace(/\s{2,}/g, " ").replace(/[^\000-\177]*/g, "")
        }, w.text = function (t) {
            return t.textContent || t.innerText
        }, w.specialProperties = {
            text: w.text,
            cleanText: function (t) {
                return w.cleanText(w.text(t))
            }
        }, w.getObjectProperty = function (t, e, i) {
            for (var a, r = e.split("."), o = t, s = w.specialProperties, c = 0, u = r.length; u > c; c++) {
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
        }, w.getToolsByType = function (t) {
            if (!t) throw new Error("Tool type is missing");
            var e = [];
            for (var n in w.tools)
                if (w.tools.hasOwnProperty(n)) {
                    var i = w.tools[n];
                    i.settings && i.settings.engine === t && e.push(i)
                }
            return e
        }, w.setVar = function () {
            var t = w.data.customVars;
            if (null == t && (w.data.customVars = {}, t = w.data.customVars), "string" == typeof arguments[0]) {
                var e = arguments[0];
                t[e] = arguments[1]
            } else if (arguments[0]) {
                var n = arguments[0];
                for (var i in n) n.hasOwnProperty(i) && (t[i] = n[i])
            }
        }, w.dataElementSafe = function (t, e) {
            if (arguments.length > 2) {
                var n = arguments[2];
                "pageview" === e ? w.dataElementSafe.pageviewCache[t] = n : "session" === e ? w.setCookie("_sdsat_" + t, n) : "visitor" === e && w.setCookie("_sdsat_" + t, n, 730)
            } else {
                if ("pageview" === e) return w.dataElementSafe.pageviewCache[t];
                if ("session" === e || "visitor" === e) return w.readCookie("_sdsat_" + t)
            }
        }, w.dataElementSafe.pageviewCache = {}, w.realGetDataElement = function (e) {
            var n;
            return e.selector ? w.hasSelector && w.cssQuery(e.selector, function (t) {
                if (t.length > 0) {
                    var i = t[0];
                    "text" === e.property ? n = i.innerText || i.textContent : e.property in i ? n = i[e.property] : w.hasAttr(i, e.property) && (n = i.getAttribute(e.property))
                }
            }) : e.queryParam ? n = e.ignoreCase ? w.getQueryParamCaseInsensitive(e.queryParam) : w.getQueryParam(e.queryParam) : e.cookie ? n = w.readCookie(e.cookie) : e.jsVariable ? n = w.getObjectProperty(t, e.jsVariable) : e.customJS ? n = e.customJS() : e.contextHub && (n = e.contextHub()), w.isString(n) && e.cleanText && (n = w.cleanText(n)), n
        }, w.getDataElement = function (t, e, i) {
            if (i = i || w.dataElements[t], null == i) return w.settings.undefinedVarsReturnEmpty ? "" : null;
            var a = w.realGetDataElement(i);
            return a === n && i.storeLength ? a = w.dataElementSafe(t, i.storeLength) : a !== n && i.storeLength && w.dataElementSafe(t, i.storeLength, a), a || e || (a = i["default"] || ""), w.isString(a) && i.forceLowerCase && (a = a.toLowerCase()), a
        }, w.getVar = function (i, a, r) {
            var o, s, c = w.data.customVars,
                u = r ? r.target || r.srcElement : null,
                l = {
                    uri: w.URI(),
                    protocol: e.location.protocol,
                    hostname: e.location.hostname
                };
            if (w.dataElements && i in w.dataElements) return w.getDataElement(i);
            if (s = l[i.toLowerCase()], s === n)
                if ("this." === i.substring(0, 5)) i = i.slice(5), s = w.getObjectProperty(a, i, !0);
                else if ("event." === i.substring(0, 6)) i = i.slice(6), s = w.getObjectProperty(r, i);
                else if ("target." === i.substring(0, 7)) i = i.slice(7), s = w.getObjectProperty(u, i);
                else if ("window." === i.substring(0, 7)) i = i.slice(7), s = w.getObjectProperty(t, i);
                else if ("param." === i.substring(0, 6)) i = i.slice(6), s = w.getQueryParam(i);
                else if (o = i.match(/^rand([0-9]+)$/)) {
                    var d = Number(o[1]),
                        h = (Math.random() * (Math.pow(10, d) - 1)).toFixed(0);
                    s = Array(d - h.length + 1).join("0") + h
                } else s = w.getObjectProperty(c, i);
            return s
        }, w.getVars = function (t, e, n) {
            var i = {};
            return w.each(t, function (t) {
                i[t] = w.getVar(t, e, n)
            }), i
        }, w.replace = function (t, e, n, i) {
            return "string" != typeof t ? t : t.replace(/%(.*?)%/g, function (t, a) {
                var r = w.getVar(a, e, n);
                return null == r ? w.settings.undefinedVarsReturnEmpty ? "" : t : i ? w.escapeForHtml(r) : r
            })
        }, w.escapeHtmlParams = function (t) {
            return t.escapeHtml = !0, t
        }, w.searchVariables = function (t, e, n) {
            if (!t || 0 === t.length) return "";
            for (var i = [], a = 0, r = t.length; r > a; a++) {
                var o = t[a],
                    s = w.getVar(o, e, n);
                i.push(o + "=" + escape(s))
            }
            return "?" + i.join("&")
        }, w.fireRule = function (t, e, n) {
            var i = t.trigger;
            if (i) {
                for (var a = 0, r = i.length; r > a; a++) {
                    var o = i[a];
                    w.execute(o, e, n)
                }
                w.contains(w.fireOnceEvents, t.event) && (t.expired = !0)
            }
        }, w.isLinked = function (t) {
            for (var e = t; e; e = e.parentNode)
                if (w.isLinkTag(e)) return !0;
            return !1
        }, w.firePageLoadEvent = function (t) {
            for (var n = e.location, i = {
                type: t,
                target: n
            }, a = w.pageLoadRules, r = w.evtHandlers[i.type], o = a.length; o--;) {
                var s = a[o];
                w.ruleMatches(s, i, n) && (w.notify('Rule "' + s.name + '" fired.', 1), w.fireRule(s, n, i))
            }
            for (var c in w.tools)
                if (w.tools.hasOwnProperty(c)) {
                    var u = w.tools[c];
                    u.endPLPhase && u.endPLPhase(t)
                }
            r && w.each(r, function (t) {
                t(i)
            })
        }, w.track = function (t) {
            t = t.replace(/^\s*/, "").replace(/\s*$/, "");
            for (var e = 0; e < w.directCallRules.length; e++) {
                var n = w.directCallRules[e];
                if (n.name === t) return w.notify('Direct call Rule "' + t + '" fired.', 1), void w.fireRule(n, location, {
                    type: t
                })
            }
            w.notify('Direct call Rule "' + t + '" not found.', 1)
        }, w.basePath = function () {
            return w.data.host ? ("https:" === e.location.protocol ? "https://" + w.data.host.https : "http://" + w.data.host.http) + "/" : this.settings.basePath
        }, w.setLocation = function (e) {
            t.location = e
        }, w.parseQueryParams = function (t) {
            var e = function (t) {
                var e = t;
                try {
                    e = decodeURIComponent(t)
                } catch (n) { }
                return e
            };
            if ("" === t || w.isString(t) === !1) return {};
            0 === t.indexOf("?") && (t = t.substring(1));
            var n = {},
                i = t.split("&");
            return w.each(i, function (t) {
                t = t.split("="), t[1] && (n[e(t[0])] = e(t[1]))
            }), n
        }, w.getCaseSensitivityQueryParamsMap = function (t) {
            var e = w.parseQueryParams(t),
                n = {};
            for (var i in e) e.hasOwnProperty(i) && (n[i.toLowerCase()] = e[i]);
            return {
                normal: e,
                caseInsensitive: n
            }
        }, w.updateQueryParams = function () {
            w.QueryParams = w.getCaseSensitivityQueryParamsMap(t.location.search)
        }, w.updateQueryParams(), w.getQueryParam = function (t) {
            return w.QueryParams.normal[t]
        }, w.getQueryParamCaseInsensitive = function (t) {
            return w.QueryParams.caseInsensitive[t.toLowerCase()]
        }, w.encodeObjectToURI = function (t) {
            if (w.isObject(t) === !1) return "";
            var e = [];
            for (var n in t) t.hasOwnProperty(n) && e.push(encodeURIComponent(n) + "=" + encodeURIComponent(t[n]));
            return e.join("&")
        }, w.readCookie = function (t) {
            for (var i = t + "=", a = e.cookie.split(";"), r = 0; r < a.length; r++) {
                for (var o = a[r];
                    " " == o.charAt(0) ;) o = o.substring(1, o.length);
                if (0 === o.indexOf(i)) return o.substring(i.length, o.length)
            }
            return n
        }, w.setCookie = function (t, n, i) {
            var a;
            if (i) {
                var r = new Date;
                r.setTime(r.getTime() + 24 * i * 60 * 60 * 1e3), a = "; expires=" + r.toGMTString()
            } else a = "";
            e.cookie = t + "=" + n + a + "; path=/"
        }, w.removeCookie = function (t) {
            w.setCookie(t, "", -1)
        }, w.getElementProperty = function (t, e) {
            if ("@" === e.charAt(0)) {
                var i = w.specialProperties[e.substring(1)];
                if (i) return i(t)
            }
            return "innerText" === e ? w.text(t) : e in t ? t[e] : t.getAttribute ? t.getAttribute(e) : n
        }, w.propertiesMatch = function (t, e) {
            if (t)
                for (var n in t)
                    if (t.hasOwnProperty(n)) {
                        var i = t[n],
                            a = w.getElementProperty(e, n);
                        if ("string" == typeof i && i !== a) return !1;
                        if (i instanceof RegExp && !i.test(a)) return !1
                    }
            return !0
        }, w.isRightClick = function (t) {
            var e;
            return t.which ? e = 3 == t.which : t.button && (e = 2 == t.button), e
        }, w.ruleMatches = function (t, e, n, i) {
            var a = t.condition,
                r = t.conditions,
                o = t.property,
                s = e.type,
                c = t.value,
                u = e.target || e.srcElement,
                l = n === u;
            if (t.event !== s && ("custom" !== t.event || t.customEvent !== s)) return !1;
            if (!w.ruleInScope(t)) return !1;
            if ("click" === t.event && w.isRightClick(e)) return !1;
            if (t.isDefault && i > 0) return !1;
            if (t.expired) return !1;
            if ("inview" === s && e.inviewDelay !== t.inviewDelay) return !1;
            if (!l && (t.bubbleFireIfParent === !1 || 0 !== i && t.bubbleFireIfChildFired === !1)) return !1;
            if (t.selector && !w.matchesCss(t.selector, n)) return !1;
            if (!w.propertiesMatch(o, n)) return !1;
            if (null != c)
                if ("string" == typeof c) {
                    if (c !== n.value) return !1
                } else if (!c.test(n.value)) return !1;
            if (a) try {
                if (!a.call(n, e, u)) return w.notify('Condition for rule "' + t.name + '" not met.', 1), !1
            } catch (d) {
                return w.notify('Condition for rule "' + t.name + '" not met. Error: ' + d.message, 1), !1
            }
            if (r) {
                var h = w.find(r, function (i) {
                    try {
                        return !i.call(n, e, u)
                    } catch (a) {
                        return w.notify('Condition for rule "' + t.name + '" not met. Error: ' + a.message, 1), !0
                    }
                });
                if (h) return w.notify("Condition " + h.toString() + ' for rule "' + t.name + '" not met.', 1), !1
            }
            return !0
        }, w.evtHandlers = {}, w.bindEvent = function (t, e) {
            var n = w.evtHandlers;
            n[t] || (n[t] = []), n[t].push(e)
        }, w.whenEvent = w.bindEvent, w.unbindEvent = function (t, e) {
            var n = w.evtHandlers;
            if (n[t]) {
                var i = w.indexOf(n[t], e);
                n[t].splice(i, 1)
            }
        }, w.bindEventOnce = function (t, e) {
            var n = function () {
                w.unbindEvent(t, n), e.apply(null, arguments)
            };
            w.bindEvent(t, n)
        }, w.isVMLPoisoned = function (t) {
            if (!t) return !1;
            try {
                t.nodeName
            } catch (e) {
                if ("Attribute only valid on v:image" === e.message) return !0
            }
            return !1
        }, w.handleEvent = function (t) {
            if (!w.$data(t, "eventProcessed")) {
                var e = t.type.toLowerCase(),
                    n = t.target || t.srcElement,
                    i = 0,
                    a = w.rules,
                    r = (w.tools, w.evtHandlers[t.type]);
                if (w.isVMLPoisoned(n)) return void w.notify("detected " + e + " on poisoned VML element, skipping.", 1);
                r && w.each(r, function (e) {
                    e(t)
                });
                var o = n && n.nodeName;
                o ? w.notify("detected " + e + " on " + n.nodeName, 1) : w.notify("detected " + e, 1);
                for (var s = n; s; s = s.parentNode) {
                    var c = !1;
                    if (w.each(a, function (e) {
                            w.ruleMatches(e, t, s, i) && (w.notify('Rule "' + e.name + '" fired.', 1), w.fireRule(e, s, t), i++, e.bubbleStop && (c = !0))
                    }), c) break
                }
                w.$data(t, "eventProcessed", !0)
            }
        }, w.onEvent = e.querySelectorAll ? function (t) {
            w.handleEvent(t)
        } : function () {
            var t = [],
                e = function (e) {
                    e.selector ? t.push(e) : w.handleEvent(e)
                };
            return e.pendingEvents = t, e
        }(), w.fireEvent = function (t, e) {
            w.onEvent({
                type: t,
                target: e
            })
        }, w.registerEvents = function (t, e) {
            for (var n = e.length - 1; n >= 0; n--) {
                var i = e[n];
                w.$data(t, i + ".tracked") || (w.addEventHandler(t, i, w.onEvent), w.$data(t, i + ".tracked", !0))
            }
        }, w.registerEventsForTags = function (t, n) {
            for (var i = t.length - 1; i >= 0; i--)
                for (var a = t[i], r = e.getElementsByTagName(a), o = r.length - 1; o >= 0; o--) w.registerEvents(r[o], n)
        }, w.setListeners = function () {
            var t = ["click", "submit"];
            w.each(w.rules, function (e) {
                "custom" === e.event && e.hasOwnProperty("customEvent") && !w.contains(t, e.customEvent) && t.push(e.customEvent)
            }), w.registerEvents(e, t)
        }, w.getUniqueRuleEvents = function () {
            return w._uniqueRuleEvents || (w._uniqueRuleEvents = [], w.each(w.rules, function (t) {
                -1 === w.indexOf(w._uniqueRuleEvents, t.event) && w._uniqueRuleEvents.push(t.event)
            })), w._uniqueRuleEvents
        }, w.setFormListeners = function () {
            if (!w._relevantFormEvents) {
                var t = ["change", "focus", "blur", "keypress"];
                w._relevantFormEvents = w.filter(w.getUniqueRuleEvents(), function (e) {
                    return -1 !== w.indexOf(t, e)
                })
            }
            w._relevantFormEvents.length && w.registerEventsForTags(["input", "select", "textarea", "button"], w._relevantFormEvents)
        }, w.setVideoListeners = function () {
            if (!w._relevantVideoEvents) {
                var t = ["play", "pause", "ended", "volumechange", "stalled", "loadeddata"];
                w._relevantVideoEvents = w.filter(w.getUniqueRuleEvents(), function (e) {
                    return -1 !== w.indexOf(t, e)
                })
            }
            w._relevantVideoEvents.length && w.registerEventsForTags(["video"], w._relevantVideoEvents)
        }, w.readStoredSetting = function (e) {
            try {
                return e = "sdsat_" + e, t.localStorage.getItem(e)
            } catch (n) {
                return w.notify("Cannot read stored setting from localStorage: " + n.message, 2), null
            }
        }, w.loadStoredSettings = function () {
            var t = w.readStoredSetting("debug"),
                e = w.readStoredSetting("hide_activity");
            t && (w.settings.notifications = "true" === t), e && (w.settings.hideActivity = "true" === e)
        }, w.isRuleActive = function (t, e) {
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
                        if (!w.contains(s.days, e[l]())) return !1
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
        }, w.isOutboundLink = function (t) {
            if (!t.getAttribute("href")) return !1;
            var e = t.hostname,
                n = (t.href, t.protocol);
            if ("http:" !== n && "https:" !== n) return !1;
            var i = w.any(w.settings.domainList, function (t) {
                return w.isSubdomainOf(e, t)
            });
            return i ? !1 : e !== location.hostname
        }, w.isLinkerLink = function (t) {
            return t.getAttribute && t.getAttribute("href") ? w.hasMultipleDomains() && t.hostname != location.hostname && !t.href.match(/^javascript/i) && !w.isOutboundLink(t) : !1
        }, w.isSubdomainOf = function (t, e) {
            if (t === e) return !0;
            var n = t.length - e.length;
            return n > 0 ? w.equalsIgnoreCase(t.substring(n), e) : !1
        }, w.getVisitorId = function () {
            var t = w.getToolsByType("visitor_id");
            return 0 === t.length ? null : t[0].getInstance()
        }, w.URI = function () {
            var t = e.location.pathname + e.location.search;
            return w.settings.forceLowerCase && (t = t.toLowerCase()), t
        }, w.URL = function () {
            var t = e.location.href;
            return w.settings.forceLowerCase && (t = t.toLowerCase()), t
        }, w.filterRules = function () {
            function t(t) {
                return w.isRuleActive(t) ? !0 : !1
            }
            w.rules = w.filter(w.rules, t), w.pageLoadRules = w.filter(w.pageLoadRules, t)
        }, w.ruleInScope = function (t, n) {
            function i(t, e) {
                function n(t) {
                    return e.match(t)
                }
                var i = t.include,
                    r = t.exclude;
                if (i && a(i, e)) return !0;
                if (r) {
                    if (w.isString(r) && r === e) return !0;
                    if (w.isArray(r) && w.any(r, n)) return !0;
                    if (w.isRegex(r) && n(r)) return !0
                }
                return !1
            }

            function a(t, e) {
                function n(t) {
                    return e.match(t)
                }
                return w.isString(t) && t !== e ? !0 : w.isArray(t) && !w.any(t, n) ? !0 : w.isRegex(t) && !n(t) ? !0 : !1
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
        }, w.backgroundTasks = function () {
            +new Date;
            w.setFormListeners(), w.setVideoListeners(), w.loadStoredSettings(), w.registerNewElementsForDynamicRules(), w.eventEmitterBackgroundTasks(); +new Date
        }, w.registerNewElementsForDynamicRules = function () {
            function t(e, n) {
                var i = t.cache[e];
                return i ? n(i) : void w.cssQuery(e, function (i) {
                    t.cache[e] = i, n(i)
                })
            }
            t.cache = {}, w.each(w.dynamicRules, function (e) {
                t(e.selector, function (t) {
                    w.each(t, function (t) {
                        var n = "custom" === e.event ? e.customEvent : e.event;
                        w.$data(t, "dynamicRules.seen." + n) || (w.$data(t, "dynamicRules.seen." + n, !0), w.propertiesMatch(e.property, t) && w.registerEvents(t, [n]))
                    })
                })
            })
        }, w.ensureCSSSelector = function () {
            return e.querySelectorAll ? void (w.hasSelector = !0) : (w.loadingSizzle = !0, w.sizzleQueue = [], void w.loadScript(w.basePath() + "selector.js", function () {
                if (!w.Sizzle) return void w.logError(new Error("Failed to load selector.js"));
                var t = w.onEvent.pendingEvents;
                w.each(t, function (t) {
                    w.handleEvent(t)
                }, this), w.onEvent = w.handleEvent, w.hasSelector = !0, delete w.loadingSizzle, w.each(w.sizzleQueue, function (t) {
                    w.cssQuery(t[0], t[1])
                }), delete w.sizzleQueue
            }))
        }, w.errors = [], w.logError = function (t) {
            w.errors.push(t), w.notify(t.name + " - " + t.message, 5)
        }, w.pageBottom = function () {
            w.initialized && (w.pageBottomFired = !0, w.firePageLoadEvent("pagebottom"))
        }, w.stagingLibraryOverride = function () {
            var t = "true" === w.readStoredSetting("stagingLibrary");
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
        }, w.checkAsyncInclude = function () {
            t.satellite_asyncLoad && w.notify('You may be using the async installation of Satellite. In-page HTML and the "pagebottom" event will not work. Please update your Satellite installation for these features.', 5)
        }, w.hasMultipleDomains = function () {
            return !!w.settings.domainList && w.settings.domainList.length > 1
        }, w.handleOverrides = function () {
            if (C)
                for (var t in C) C.hasOwnProperty(t) && (w.data[t] = C[t])
        }, w.privacyManagerParams = function () {
            var t = {};
            w.extend(t, w.settings.privacyManagement);
            var e = [];
            for (var n in w.tools)
                if (w.tools.hasOwnProperty(n)) {
                    var i = w.tools[n],
                        a = i.settings;
                    if (!a) continue;
                    "sc" === a.engine && e.push(i)
                }
            var r = w.filter(w.map(e, function (t) {
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
                        t[c] = w.getVar(u.value)
                    }
            }
            return t
        }, w.prepareLoadPrivacyManager = function () {
            function e(t) {
                function e() {
                    r++, r === a.length && (n(), clearTimeout(o), t())
                }

                function n() {
                    w.each(a, function (t) {
                        w.unbindEvent(t.id + ".load", e)
                    })
                }

                function i() {
                    n(), t()
                }
                var a = w.filter(w.values(w.tools), function (t) {
                    return t.settings && "sc" === t.settings.engine
                });
                if (0 === a.length) return t();
                var r = 0;
                w.each(a, function (t) {
                    w.bindEvent(t.id + ".load", e)
                });
                var o = setTimeout(i, 5e3)
            }
            w.addEventHandler(t, "load", function () {
                e(w.loadPrivacyManager)
            })
        }, w.loadPrivacyManager = function () {
            var t = w.basePath() + "privacy_manager.js";
            w.loadScript(t, function () {
                var t = w.privacyManager;
                t.configure(w.privacyManagerParams()), t.openIfRequired()
            })
        }, w.init = function (e) {
            if (!w.stagingLibraryOverride()) {
                w.configurationSettings = e;
                var i = e.tools;
                delete e.tools;
                for (var a in e) e.hasOwnProperty(a) && (w[a] = e[a]);
                w.data.customVars === n && (w.data.customVars = {}), w.data.queryParams = w.QueryParams.normal, w.handleOverrides(), w.detectBrowserInfo(), w.trackVisitorInfo && w.trackVisitorInfo(), w.loadStoredSettings(), w.Logger.setOutputState(w.settings.notifications), w.checkAsyncInclude(), w.ensureCSSSelector(), w.filterRules(), w.dynamicRules = w.filter(w.rules, function (t) {
                    return t.eventHandlerOnElement
                }), w.tools = w.initTools(i), w.initEventEmitters(), w.firePageLoadEvent("aftertoolinit"), w.settings.privacyManagement && w.prepareLoadPrivacyManager(), w.hasSelector && w.domReady(w.eventEmitterBackgroundTasks), w.setListeners(), w.domReady(function () {
                    w.poll(function () {
                        w.backgroundTasks()
                    }, w.settings.recheckEvery || 3e3)
                }), w.domReady(function () {
                    w.domReadyFired = !0, w.pageBottomFired || w.pageBottom(), w.firePageLoadEvent("domready")
                }), w.addEventHandler(t, "load", function () {
                    w.firePageLoadEvent("windowload")
                }), w.firePageLoadEvent("pagetop"), w.initialized = !0
            }
        }, w.pageLoadPhases = ["aftertoolinit", "pagetop", "pagebottom", "domready", "windowload"], w.loadEventBefore = function (t, e) {
            return w.indexOf(w.pageLoadPhases, t) <= w.indexOf(w.pageLoadPhases, e)
        }, w.flushPendingCalls = function (t) {
            t.pending && (w.each(t.pending, function (e) {
                var n = e[0],
                    i = e[1],
                    a = e[2],
                    r = e[3];
                n in t ? t[n].apply(t, [i, a].concat(r)) : t.emit ? t.emit(n, i, a, r) : w.notify("Failed to trigger " + n + " for tool " + t.id, 1)
            }), delete t.pending)
        }, w.setDebug = function (e) {
            try {
                t.localStorage.setItem("sdsat_debug", e)
            } catch (n) {
                w.notify("Cannot set debug mode: " + n.message, 2)
            }
        }, w.getUserAgent = function () {
            return navigator.userAgent
        }, w.detectBrowserInfo = function () {
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
                a = w.getUserAgent();
            w.browserInfo = {
                browser: e(a),
                os: n(a),
                deviceType: i(a)
            }
        }, w.isHttps = function () {
            return "https:" == e.location.protocol
        }, w.BaseTool = function (t) {
            this.settings = t || {}, this.forceLowerCase = w.settings.forceLowerCase, "forceLowerCase" in this.settings && (this.forceLowerCase = this.settings.forceLowerCase)
        }, w.BaseTool.prototype = {
            triggerCommand: function (t, e, n) {
                var i = this.settings || {};
                if (this.initialize && this.isQueueAvailable() && this.isQueueable(t) && n && w.loadEventBefore(n.type, i.loadOn)) return void this.queueCommand(t, e, n);
                var a = t.command,
                    r = this["$" + a],
                    o = r ? r.escapeHtml : !1,
                    s = w.preprocessArguments(t.arguments, e, n, this.forceLowerCase, o);
                r ? r.apply(this, [e, n].concat(s)) : this.$missing$ ? this.$missing$(a, e, n, s) : w.notify("Failed to trigger " + a + " for tool " + this.id, 1)
            },
            endPLPhase: function (t) { },
            isQueueable: function (t) {
                return "cancelToolInit" !== t.command
            },
            isQueueAvailable: function () {
                return !this.initialized && !this.initializing
            },
            flushQueue: function () {
                this.pending && (w.each(this.pending, function (t) {
                    this.triggerCommand.apply(this, t)
                }, this), this.pending = [])
            },
            queueCommand: function (t, e, n) {
                this.pending || (this.pending = []), this.pending.push([t, e, n])
            },
            $cancelToolInit: function () {
                this._cancelToolInit = !0
            }
        }, t._satellite = w, i.orientationChange = function (e) {
            var n = 0 === t.orientation ? "portrait" : "landscape";
            e.orientation = n, w.onEvent(e)
        }, w.availableEventEmitters.push(i), a.prototype = {
            initialize: function () {
                return this.FB = this.FB || t.FB, this.FB && this.FB.Event && this.FB.Event.subscribe ? (this.bind(), !0) : void 0
            },
            bind: function () {
                this.FB.Event.subscribe("edge.create", function () {
                    w.notify("tracking a facebook like", 1), w.onEvent({
                        type: "facebook.like",
                        target: e
                    })
                }), this.FB.Event.subscribe("edge.remove", function () {
                    w.notify("tracking a facebook unlike", 1), w.onEvent({
                        type: "facebook.unlike",
                        target: e
                    })
                }), this.FB.Event.subscribe("message.send", function () {
                    w.notify("tracking a facebook share", 1), w.onEvent({
                        type: "facebook.send",
                        target: e
                    })
                })
            }
        }, w.availableEventEmitters.push(a), r.prototype = {
            backgroundTasks: function () {
                var t = this.eventHandler;
                w.each(this.rules, function (e) {
                    w.cssQuery(e.selector || "video", function (e) {
                        w.each(e, function (e) {
                            w.$data(e, "videoplayed.tracked") || (w.addEventHandler(e, "timeupdate", w.throttle(t, 100)), w.$data(e, "videoplayed.tracked", !0))
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
                    !w.$data(t, n) && l() && (w.$data(t, n, !0), w.onEvent({
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
        }, w.availableEventEmitters.push(r), o.prototype.backgroundTasks = function () {
            w.each(this.rules, function (t) {
                w.cssQuery(t.selector, function (t) {
                    if (t.length > 0) {
                        var e = t[0];
                        if (w.$data(e, "elementexists.seen")) return;
                        w.$data(e, "elementexists.seen", !0), w.onEvent({
                            type: "elementexists",
                            target: e
                        })
                    }
                })
            })
        }, w.availableEventEmitters.push(o), s.prototype = {
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
                this.prevUnload = t.onunload, this.prevBeforeUnload = t.onbeforeunload, t.onunload = w.bind(function (e) {
                    this.prevUnload && setTimeout(w.bind(function () {
                        this.prevUnload.call(t, e)
                    }, this), 1), this.newObueListener()
                }, this), t.onbeforeunload = w.bind(function (e) {
                    this.prevBeforeUnload && setTimeout(w.bind(function () {
                        this.prevBeforeUnload.call(t, e)
                    }, this), 1), this.newObueListener()
                }, this)
            },
            triggerBeacons: function () {
                w.fireEvent("leave", e)
            }
        }, w.availableEventEmitters.push(s), c.prototype = {
            initialize: function () {
                this.setupHistoryAPI(), this.setupHashChange()
            },
            fireIfURIChanged: function () {
                var t = w.URL();
                this.lastURL !== t && (this.fireEvent(), this.lastURL = t)
            },
            fireEvent: function () {
                w.updateQueryParams(), w.onEvent({
                    type: "locationchange",
                    target: e
                })
            },
            setupSPASupport: function () {
                this.setupHistoryAPI(), this.setupHashChange()
            },
            setupHistoryAPI: function () {
                var e = t.history;
                e && (e.pushState && (this.originalPushState = e.pushState, e.pushState = this._pushState), e.replaceState && (this.originalReplaceState = e.replaceState, e.replaceState = this._replaceState)), w.addEventHandler(t, "popstate", this._onPopState)
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
                w.addEventHandler(t, "hashchange", this._onHashChange)
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
                history.pushState === this._pushState && (history.pushState = this.originalPushState), history.replaceState === this._replaceState && (history.replaceState = this.originalReplaceState), w.removeEventHandler(t, "popstate", this._onPopState)
            },
            cleanUpHashChange: function () {
                w.removeEventHandler(t, "hashchange", this._onHashChange)
            }
        }, w.availableEventEmitters.push(c), u.prototype.initPolling = function () {
            0 !== this.dataElementsNames.length && (this.dataElementsStore = this.getDataElementsValues(), w.poll(w.bind(this.checkDataElementValues, this), 1e3))
        }, u.prototype.getDataElementsValues = function () {
            var t = {};
            return w.each(this.dataElementsNames, function (e) {
                t[e] = w.getVar(e)
            }), t
        }, u.prototype.checkDataElementValues = function () {
            w.each(this.dataElementsNames, w.bind(function (t) {
                var e = w.getVar(t),
                    n = this.dataElementsStore[t];
                e !== n && (this.dataElementsStore[t] = e, w.onEvent({
                    type: "dataelementchange(" + t + ")",
                    target: {
                        value: e,
                        previousValue: n
                    }
                }))
            }, this))
        }, w.availableEventEmitters.push(u), l.prototype = {
            backgroundTasks: function () {
                var t = this;
                w.each(this.rules, function (e) {
                    var n = e[1],
                        i = e[0];
                    w.cssQuery(n, function (e) {
                        w.each(e, function (e) {
                            t.trackElement(e, i)
                        })
                    })
                }, this)
            },
            trackElement: function (t, e) {
                var n = this,
                    i = w.$data(t, "hover.delays");
                i ? w.contains(i, e) || i.push(e) : (w.addEventHandler(t, "mouseover", function (e) {
                    n.onMouseOver(e, t)
                }), w.addEventHandler(t, "mouseout", function (e) {
                    n.onMouseOut(e, t)
                }), w.$data(t, "hover.delays", [e]))
            },
            onMouseOver: function (t, e) {
                var n = t.target || t.srcElement,
                    i = t.relatedTarget || t.fromElement,
                    a = (e === n || w.containsElement(e, n)) && !w.containsElement(e, i);
                a && this.onMouseEnter(e)
            },
            onMouseEnter: function (t) {
                var e = w.$data(t, "hover.delays"),
                    n = w.map(e, function (e) {
                        return setTimeout(function () {
                            w.onEvent({
                                type: "hover(" + e + ")",
                                target: t
                            })
                        }, e)
                    });
                w.$data(t, "hover.delayTimers", n)
            },
            onMouseOut: function (t, e) {
                var n = t.target || t.srcElement,
                    i = t.relatedTarget || t.toElement,
                    a = (e === n || w.containsElement(e, n)) && !w.containsElement(e, i);
                a && this.onMouseLeave(e)
            },
            onMouseLeave: function (t) {
                var e = w.$data(t, "hover.delayTimers");
                e && w.each(e, function (t) {
                    clearTimeout(t)
                })
            }
        }, w.availableEventEmitters.push(l), d.prototype = {
            defineEvents: function () {
                this.oldBlurClosure = function () {
                    w.fireEvent("tabblur", e)
                }, this.oldFocusClosure = w.bind(function () {
                    this.visibilityApiHasPriority ? w.fireEvent("tabfocus", e) : null != w.visibility.getHiddenProperty() ? w.visibility.isHidden() || w.fireEvent("tabfocus", e) : w.fireEvent("tabfocus", e)
                }, this)
            },
            attachDetachModernEventListeners: function (t) {
                var n = 0 == t ? "removeEventHandler" : "addEventHandler";
                w[n](e, w.visibility.getVisibilityEvent(), this.handleVisibilityChange)
            },
            attachDetachOlderEventListeners: function (e, n, i) {
                var a = 0 == e ? "removeEventHandler" : "addEventHandler";
                w[a](n, i, this.oldBlurClosure), w[a](t, "focus", this.oldFocusClosure)
            },
            handleVisibilityChange: function () {
                w.visibility.isHidden() ? w.fireEvent("tabblur", e) : w.fireEvent("tabfocus", e)
            },
            setVisibilityApiPriority: function (e) {
                this.visibilityApiHasPriority = e, this.attachDetachOlderEventListeners(!1, t, "blur"), this.attachDetachModernEventListeners(!1), e ? null != w.visibility.getHiddenProperty() ? this.attachDetachModernEventListeners(!0) : this.attachDetachOlderEventListeners(!0, t, "blur") : (this.attachDetachOlderEventListeners(!0, t, "blur"), null != w.visibility.getHiddenProperty() && this.attachDetachModernEventListeners(!0))
            },
            oldBlurClosure: null,
            oldFocusClosure: null,
            visibilityApiHasPriority: !0
        }, w.availableEventEmitters.push(d), h.prototype = {
            initialize: function () {
                var t = this.twttr;
                t && "function" == typeof t.ready && t.ready(w.bind(this.bind, this))
            },
            bind: function () {
                this.twttr.events.bind("tweet", function (t) {
                    t && (w.notify("tracking a tweet button", 1), w.onEvent({
                        type: "twitter.tweet",
                        target: e
                    }))
                })
            }
        }, w.availableEventEmitters.push(h), f.offset = function (n) {
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
        }, f.getViewportHeight = function () {
            var n = t.innerHeight,
                i = e.compatMode;
            return i && (n = "CSS1Compat" == i ? e.documentElement.clientHeight : e.body.clientHeight), n
        }, f.getScrollTop = function () {
            return e.documentElement.scrollTop ? e.documentElement.scrollTop : e.body.scrollTop
        }, f.isElementInDocument = function (t) {
            return e.body.contains(t)
        }, f.isElementInView = function (t) {
            if (!f.isElementInDocument(t)) return !1;
            var e = f.getViewportHeight(),
                n = f.getScrollTop(),
                i = f.offset(t).top,
                a = t.offsetHeight;
            return null !== i ? !(n > i + a || i > n + e) : !1
        }, f.prototype = {
            backgroundTasks: function () {
                var t = this.elements;
                w.each(this.rules, function (e) {
                    w.cssQuery(e.selector, function (n) {
                        var i = 0;
                        w.each(n, function (e) {
                            w.contains(t, e) || (t.push(e), i++)
                        }), i && w.notify(e.selector + " added " + i + " elements.", 1)
                    })
                }), this.track()
            },
            checkInView: function (t, e, n) {
                var i = w.$data(t, "inview");
                if (f.isElementInView(t)) {
                    i || w.$data(t, "inview", !0);
                    var a = this;
                    this.processRules(t, function (n, i, r) {
                        if (e || !n.inviewDelay) w.$data(t, i, !0), w.onEvent({
                            type: "inview",
                            target: t,
                            inviewDelay: n.inviewDelay
                        });
                        else if (n.inviewDelay) {
                            var o = w.$data(t, r);
                            o || (o = setTimeout(function () {
                                a.checkInView(t, !0, n.inviewDelay)
                            }, n.inviewDelay), w.$data(t, r, o))
                        }
                    }, n)
                } else {
                    if (!f.isElementInDocument(t)) {
                        var r = w.indexOf(this.elements, t);
                        this.elements.splice(r, 1)
                    }
                    i && w.$data(t, "inview", !1), this.processRules(t, function (e, n, i) {
                        var a = w.$data(t, i);
                        a && clearTimeout(a)
                    }, n)
                }
            },
            track: function () {
                for (var t = this.elements.length - 1; t >= 0; t--) this.checkInView(this.elements[t])
            },
            processRules: function (t, e, n) {
                var i = this.rules;
                n && (i = w.filter(this.rules, function (t) {
                    return t.inviewDelay == n
                })), w.each(i, function (n, i) {
                    var a = n.inviewDelay ? "viewed_" + n.inviewDelay : "viewed",
                        r = "inview_timeout_id_" + i;
                    w.$data(t, a) || w.matchesCss(n.selector, t) && e(n, a, r)
                })
            }
        }, w.availableEventEmitters.push(f), w.inherit(g, w.BaseTool), w.extend(g.prototype, {
            initialize: function () {
                var t = this.settings;
                if (this.settings.initTool !== !1) {
                    var e = t.url;
                    e = "string" == typeof e ? w.basePath() + e : w.isHttps() ? e.https : e.http, w.loadScript(e, w.bind(this.onLoad, this)), this.initializing = !0
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
                t === e && (w.notify(this.name + ": Initializing at " + t, 1), this.initialize())
            },
            $fire: function (t, e, n) {
                return this.initializing ? void this.queueCommand({
                    command: "fire",
                    arguments: [n]
                }, t, e) : void n.call(this.settings, t, e)
            }
        }), w.availableTools.am = g, w.availableTools.adlens = g, w.availableTools.aem = g, w.availableTools.__basic = g, w.extend(p.prototype, {
            getInstance: function () {
                return this.instance
            },
            initialize: function () {
                var t, e = this.settings;
                w.notify("Visitor ID: Initializing tool", 1), t = this.createInstance(e.mcOrgId, e.initVars), null !== t && (e.customerIDs && this.applyCustomerIDs(t, e.customerIDs), e.autoRequest && t.getMarketingCloudVisitorID(), this.instance = t)
            },
            createInstance: function (t, e) {
                if (!w.isString(t)) return w.notify('Visitor ID: Cannot create instance using mcOrgId: "' + t + '"', 4), null;
                t = w.replace(t), w.notify('Visitor ID: Create instance using mcOrgId: "' + t + '"', 1), e = this.parseValues(e);
                var n = Visitor.getInstance(t, e);
                return w.notify("Visitor ID: Set variables: " + w.stringify(e), 1), n
            },
            applyCustomerIDs: function (t, e) {
                var n = this.parseIds(e);
                t.setCustomerIDs(n), w.notify("Visitor ID: Set Customer IDs: " + w.stringify(n), 1)
            },
            parseValues: function (t) {
                if (w.isObject(t) === !1) return {};
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[n] = w.replace(t[n]));
                return e
            },
            parseIds: function (t) {
                var e = {};
                if (w.isObject(t) === !1) return {};
                for (var n in t)
                    if (t.hasOwnProperty(n)) {
                        var i = w.replace(t[n].id);
                        i !== t[n].id && i && (e[n] = {}, e[n].id = i, e[n].authState = Visitor.AuthState[t[n].authState])
                    }
                return e
            }
        }), w.availableTools.visitor_id = p, w.inherit(v, w.BaseTool), w.extend(v.prototype, {
            name: "Default",
            $loadIframe: function (e, n, i) {
                var a = i.pages,
                    r = i.loadOn,
                    o = w.bind(function () {
                        w.each(a, function (t) {
                            this.loadIframe(e, n, t)
                        }, this)
                    }, this);
                r || o(), "domready" === r && w.domReady(o), "load" === r && w.addEventHandler(t, "load", o)
            },
            loadIframe: function (t, n, i) {
                var a = e.createElement("iframe");
                a.style.display = "none";
                var r = w.data.host,
                    o = i.data,
                    s = this.scriptURL(i.src),
                    c = w.searchVariables(o, t, n);
                r && (s = w.basePath() + s), s += c, a.src = s;
                var u = e.getElementsByTagName("body")[0];
                u ? u.appendChild(a) : w.domReady(function () {
                    e.getElementsByTagName("body")[0].appendChild(a)
                })
            },
            scriptURL: function (t) {
                var e = w.settings.scriptDir || "";
                return e + t
            },
            $loadScript: function (e, n, i) {
                var a = i.scripts,
                    r = i.sequential,
                    o = i.loadOn,
                    s = w.bind(function () {
                        r ? this.loadScripts(e, n, a) : w.each(a, function (t) {
                            this.loadScripts(e, n, [t])
                        }, this)
                    }, this);
                o ? "domready" === o ? w.domReady(s) : "load" === o && w.addEventHandler(t, "load", s) : s()
            },
            loadScripts: function (t, e, n) {
                function i() {
                    if (r.length > 0 && a) {
                        var c = r.shift();
                        c.call(t, e, o)
                    }
                    var u = n.shift();
                    if (u) {
                        var l = w.data.host,
                            d = s.scriptURL(u.src);
                        l && (d = w.basePath() + d), a = u, w.loadScript(d, i)
                    }
                }
                try {
                    var a, n = n.slice(0),
                        r = this.asyncScriptCallbackQueue,
                        o = e.target || e.srcElement,
                        s = this
                } catch (c) {
                    console.error("scripts is", w.stringify(n))
                }
                i()
            },
            $loadBlockingScript: function (t, e, n) {
                var i = n.scripts,
                    a = (n.loadOn, w.bind(function () {
                        w.each(i, function (n) {
                            this.loadBlockingScript(t, e, n)
                        }, this)
                    }, this));
                a()
            },
            loadBlockingScript: function (t, e, n) {
                var i = this.scriptURL(n.src),
                    a = w.data.host,
                    r = e.target || e.srcElement;
                a && (i = w.basePath() + i), this.argsForBlockingScripts.push([t, e, r]), w.loadScriptSync(i)
            },
            pushAsyncScript: function (t) {
                this.asyncScriptCallbackQueue.push(t)
            },
            pushBlockingScript: function (t) {
                var e = this.argsForBlockingScripts.shift(),
                    n = e[0];
                t.apply(n, e.slice(1))
            },
            $writeHTML: w.escapeHtmlParams(function (t, n) {
                if (w.domReadyFired || !e.write) return void w.notify("Command writeHTML failed. You should try appending HTML using the async option.", 1);
                if ("pagebottom" !== n.type && "pagetop" !== n.type) return void w.notify("You can only use writeHTML on the `pagetop` and `pagebottom` events.", 1);
                for (var i = 2, a = arguments.length; a > i; i++) {
                    var r = arguments[i].html;
                    r = w.replace(r, t, n), e.write(r)
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
                    w.preventDefault(e);
                    var n = w.settings.linkDelay || 100;
                    setTimeout(function () {
                        w.setLocation(t.href)
                    }, n)
                }
            },
            isQueueable: function (t) {
                return "writeHTML" !== t.command
            }
        }), w.availableTools["default"] = v, w.inherit(m, w.BaseTool), w.extend(m.prototype, {
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
                this.onTabFocus = w.bind(function () {
                    this.notify("Tab visible, sending view beacon when ready", 1), this.tabEverVisible = !0, this.flushQueueWhenReady()
                }, this), this.onPageLeave = w.bind(function () {
                    this.notify("isHuman? : " + this.isHuman(), 1), this.isHuman() && this.sendDurationBeacon()
                }, this), this.onHumanDetectionChange = w.bind(function (t) {
                    this == t.target.target && (this.human = t.target.isHuman)
                }, this)
            },
            initialize: function () {
                this.initializeTracking(), this.initializeDataProviders(), this.initializeNonHumanDetection(), this.tabEverVisible = w.visibility.isVisible(), this.tabEverVisible ? this.notify("Tab visible, sending view beacon when ready", 1) : w.bindEventOnce("tabfocus", this.onTabFocus), this.initialized = !0
            },
            initializeTracking: function () {
                this.initialized || (this.notify("Initializing tracking", 1), this.addRemovePageLeaveEvent(this.enableTracking), this.addRemoveHumanDetectionChangeEvent(this.enableTracking), this.initialized = !0)
            },
            initializeDataProviders: function () {
                var t, e = this.getAnalyticsTool();
                this.dataProvider.register(new m.DataProvider.VisitorID(w.getVisitorId())), e ? (t = new m.DataProvider.Generic("rsid", function () {
                    return e.settings.account
                }), this.dataProvider.register(t)) : this.notify("Missing integration with Analytics: rsid will not be sent.")
            },
            initializeNonHumanDetection: function () {
                w.nonhumandetection ? (w.nonhumandetection.init(), this.setEnableNonHumanDetection(0 == this.settings.enableNonHumanDetection ? !1 : !0), this.settings.nonHumanDetectionDelay > 0 && this.setNonHumanDetectionDelay(1e3 * parseInt(this.settings.nonHumanDetectionDelay))) : this.notify("NHDM is not available.")
            },
            getAnalyticsTool: function () {
                return this.settings.integratesWith ? w.tools[this.settings.integratesWith] : void 0
            },
            flushQueueWhenReady: function () {
                this.enableTracking && this.tabEverVisible && w.poll(w.bind(function () {
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
                t ? w.nonhumandetection.register(this) : w.nonhumandetection.unregister(this)
            },
            setNonHumanDetectionDelay: function (t) {
                w.nonhumandetection.register(this, t)
            },
            addRemovePageLeaveEvent: function (t) {
                this.notify((t ? "Attach onto" : "Detach from") + " page leave event", 1);
                var e = 0 == t ? "unbindEvent" : "bindEvent";
                w[e]("leave", this.onPageLeave)
            },
            addRemoveHumanDetectionChangeEvent: function (t) {
                this.notify((t ? "Attach onto" : "Detach from") + " human detection change event", 1);
                var e = 0 == t ? "unbindEvent" : "bindEvent";
                w[e]("humandetection.change", this.onHumanDetectionChange)
            },
            sendViewBeacon: function () {
                this.notify("Tracked page view.", 1), this.sendBeaconWith()
            },
            sendDurationBeacon: function () {
                if (!w.timetracking || "function" != typeof w.timetracking.timeOnPage || null == w.timetracking.timeOnPage()) return void this.notify("Could not track close due missing time on page", 5);
                this.notify("Tracked close", 1), this.sendBeaconWith({
                    timeOnPage: Math.round(w.timetracking.timeOnPage() / 1e3),
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
                return w.extend(e, this.dataProvider.provide()), w.extend(e, t), this.preparePrefix(this.settings.collectionServer) + this.adapt.convertToURI(this.adapt.toNielsen(this.substituteVariables(e)))
            },
            preparePrefix: function (t) {
                return "//" + encodeURIComponent(t) + ".imrworldwide.com/cgi-bin/gn?"
            },
            substituteVariables: function (t) {
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[n] = w.replace(t[n]));
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
                w.notify(this.logPrefix + t, e)
            },
            beaconMethod: "plainBeacon",
            adapt: null,
            enableTracking: !1,
            logPrefix: "Nielsen: ",
            tabEverVisible: !1,
            human: !0,
            magicConst: 2e6
        }), m.DataProvider = {}, m.DataProvider.Generic = function (t, e) {
            this.key = t, this.valueFn = e
        }, w.extend(m.DataProvider.Generic.prototype, {
            isReady: function () {
                return !0
            },
            getValue: function () {
                return this.valueFn()
            },
            provide: function () {
                this.isReady() || m.prototype.notify("Not yet ready to provide value for: " + this.key, 5);
                var t = {};
                return t[this.key] = this.getValue(), t
            }
        }), m.DataProvider.VisitorID = function (t, e, n) {
            this.key = e || "uuid", this.visitorInstance = t, this.visitorInstance && (this.visitorId = t.getMarketingCloudVisitorID([this, this._visitorIdCallback])), this.fallbackProvider = n || new m.UUID
        }, w.inherit(m.DataProvider.VisitorID, m.DataProvider.Generic), w.extend(m.DataProvider.VisitorID.prototype, {
            isReady: function () {
                return null === this.visitorInstance ? !0 : !!this.visitorId
            },
            getValue: function () {
                return this.visitorId || this.fallbackProvider.get()
            },
            _visitorIdCallback: function (t) {
                this.visitorId = t
            }
        }), m.DataProvider.Aggregate = function () {
            this.providers = [];
            for (var t = 0; t < arguments.length; t++) this.register(arguments[t])
        }, w.extend(m.DataProvider.Aggregate.prototype, {
            register: function (t) {
                this.providers.push(t)
            },
            isReady: function () {
                return w.every(this.providers, function (t) {
                    return t.isReady()
                })
            },
            provide: function () {
                var t = {};
                return w.each(this.providers, function (e) {
                    w.extend(t, e.provide())
                }), t
            }
        }), m.UUID = function () { }, w.extend(m.UUID.prototype, {
            generate: function () {
                return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (t) {
                    var e = 16 * Math.random() | 0,
                        n = "x" == t ? e : 3 & e | 8;
                    return n.toString(16)
                })
            },
            get: function () {
                var t = w.readCookie(this.key("uuid"));
                return t ? t : (t = this.generate(), w.setCookie(this.key("uuid"), t), t)
            },
            key: function (t) {
                return "_dtm_nielsen_" + t
            }
        }), m.DataAdapters = function () { }, w.extend(m.DataAdapters.prototype, {
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
                if (w.isObject(t) === !1) return "";
                var e = [];
                for (var n in t) t.hasOwnProperty(n) && e.push(n + "=" + t[n]);
                return e.join("&")
            },
            filterObject: function (t) {
                for (var e in t) !t.hasOwnProperty(e) || null != t[e] && t[e] !== n || delete t[e];
                return t
            }
        }), w.availableTools.nielsen = m, w.inherit(y, w.BaseTool), w.extend(y.prototype, {
            name: "tnt",
            endPLPhase: function (t) {
                "aftertoolinit" === t && this.initialize()
            },
            initialize: function () {
                w.notify("Test & Target: Initializing", 1), this.initializeTargetPageParams(), this.load()
            },
            initializeTargetPageParams: function () {
                t.targetPageParams && this.updateTargetPageParams(this.parseTargetPageParamsResult(t.targetPageParams())), this.updateTargetPageParams(this.settings.pageParams), this.setTargetPageParamsFunction()
            },
            load: function () {
                var t = this.getMboxURL(this.settings.mboxURL);
                this.settings.initTool !== !1 ? this.settings.loadSync ? (w.loadScriptSync(t), this.onScriptLoaded()) : (w.loadScript(t, w.bind(this.onScriptLoaded, this)), this.initializing = !0) : this.initialized = !0
            },
            getMboxURL: function (e) {
                var n = e;
                return w.isObject(e) && (n = "https:" === t.location.protocol ? e.https : e.http), n.match(/^https?:/) ? n : w.basePath() + n
            },
            onScriptLoaded: function () {
                w.notify("Test & Target: loaded.", 1), this.flushQueue(), this.initialized = !0, this.initializing = !1
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
                w.addEventHandler(t, "load", w.bind(function () {
                    w.cssQuery(a.mboxGoesAround, function (n) {
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
                w.cssQuery('script[src*="omtrdc.net"]', function (t) {
                    var e = t[0];
                    if (e) {
                        w.scriptOnLoad(e.src, e, function () {
                            w.notify("Test & Target: request complete", 1), a(), clearTimeout(i)
                        });
                        var i = setTimeout(function () {
                            w.notify("Test & Target: bailing after " + n + "ms", 1), a()
                        }, n)
                    } else w.notify("Test & Target: failed to find T&T ajax call, bailing", 1), a()
                })
            },
            updateTargetPageParams: function (t) {
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[w.replace(n)] = w.replace(t[n]));
                w.extend(this.targetPageParamsStore, e)
            },
            getTargetPageParams: function () {
                return this.targetPageParamsStore
            },
            setTargetPageParamsFunction: function () {
                t.targetPageParams = w.bind(this.getTargetPageParams, this)
            },
            parseTargetPageParamsResult: function (t) {
                var e = t;
                return w.isArray(t) && (t = t.join("&")), w.isString(t) && (e = w.parseQueryParams(t)), e
            }
        }), w.availableTools.tnt = y, w.inherit(b, w.BaseTool), w.extend(b.prototype, {
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
                        var n = this.settings.sCodeURL || w.basePath() + "s_code.js";
                        "object" == typeof n && (n = "https:" === t.location.protocol ? n.https : n.http), n.match(/^https?:/) || (n = w.basePath() + n), this.settings.initVars && this.$setVars(null, null, this.settings.initVars), w.loadScript(n, w.bind(this.onSCodeLoaded, this)), this.initializing = !0
                    } else this.initializing = !0, this.pollForSC()
            },
            getS: function (e, n) {
                var i = n && n.hostname || t.location.hostname,
                    a = this.concatWithToolVarBindings(n && n.setVars || this.varBindings),
                    r = n && n.addEvent || this.events,
                    o = this.getAccount(i),
                    s = t.s_gi;
                if (!s) return null;
                if (this.isValidSCInstance(e) || (e = null), !o && !e) return w.notify("Adobe Analytics: tracker not initialized because account was not found", 1), null;
                var e = e || s(o),
                    c = "D" + w.appVersion;
                "undefined" != typeof e.tagContainerMarker ? e.tagContainerMarker = c : "string" == typeof e.version && e.version.substring(e.version.length - 5) !== "-" + c && (e.version += "-" + c), e.sa && this.settings.skipSetAccount !== !0 && this.settings.initTool !== !1 && e.sa(this.settings.account), this.applyVarBindingsOnTracker(e, a), r.length > 0 && (e.events = r.join(","));
                var u = w.getVisitorId();
                return u && (e.visitor = w.getVisitorId()), e
            },
            onSCodeLoaded: function (t) {
                this.initialized = !0, this.initializing = !1;
                var e = ["Adobe Analytics: loaded", t ? " (manual)" : "", "."];
                w.notify(e.join(""), 1), w.fireEvent(this.id + ".load", this.getS()), t || (this.flushQueueExceptTrackLink(), this.sendBeacon()), this.flushQueue()
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
                return e ? this.settings.customInit && this.settings.customInit(e) === !1 ? void w.notify("Adobe Analytics: custom init suppressed beacon", 1) : (this.settings.executeCustomPageCodeFirst && this.applyVarBindingsOnTracker(e, this.varBindings), this.executeCustomSetupFuns(e), e.t(), this.clearVarBindings(), this.clearCustomSetup(), void w.notify("Adobe Analytics: tracked page view", 1)) : void w.notify("Adobe Analytics: page code not loaded", 1)
            },
            pollForSC: function () {
                w.poll(w.bind(function () {
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
                        n[i] = w.replace(a, location, e)
                    }
                return n
            },
            $setVars: function (t, e, n) {
                for (var i in n)
                    if (n.hasOwnProperty(i)) {
                        var a = n[i];
                        "function" == typeof a && (a = a()), this.varBindings[i] = a
                    }
                w.notify("Adobe Analytics: set variables.", 2)
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
                return w.map(["trackingServer", "trackingServerSecure"], function (n) {
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
                w.each(this.customSetupFuns, function (n) {
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
                if (!s) return void w.notify("Adobe Analytics: page code not loaded", 1);
                var c = s.linkTrackVars,
                    u = s.linkTrackEvents,
                    l = this.definedVarNames(r);
                n && n.customSetup && n.customSetup.call(t, e, s), o.length > 0 && l.push("events"), s.products && l.push("products"), l = this.mergeTrackLinkVars(s.linkTrackVars, l), o = this.mergeTrackLinkVars(s.linkTrackEvents, o), s.linkTrackVars = this.getCustomLinkVarsList(l);
                var d = w.map(o, function (t) {
                    return t.split(":")[0]
                });
                s.linkTrackEvents = this.getCustomLinkVarsList(d), s.tl(!0, i || "o", a), w.notify(["Adobe Analytics: tracked link ", "using: linkTrackVars=", w.stringify(s.linkTrackVars), "; linkTrackEvents=", w.stringify(s.linkTrackEvents)].join(""), 1), s.linkTrackVars = c, s.linkTrackEvents = u
            },
            mergeTrackLinkVars: function (t, e) {
                return t && (e = t.split(",").concat(e)), e
            },
            getCustomLinkVarsList: function (t) {
                var e = w.indexOf(t, "None");
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
                return r ? (r.linkTrackVars = "", r.linkTrackEvents = "", this.executeCustomSetupFuns(r), n && n.customSetup && n.customSetup.call(t, e, r), r.t(), this.clearVarBindings(), this.clearCustomSetup(), void w.notify("Adobe Analytics: tracked page view", 1)) : void w.notify("Adobe Analytics: page code not loaded", 1)
            },
            $postTransaction: function (e, n, i) {
                var a = w.data.transaction = t[i],
                    r = this.varBindings,
                    o = this.settings.fieldVarMapping;
                if (w.each(a.items, function (t) {
                        this.products.push(t)
                }, this), r.products = w.map(this.products, function (t) {
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
        }), w.availableTools.sc = b, w.inherit(k, w.BaseTool), w.extend(k.prototype, {
            name: "GA",
            initialize: function () {
                var e = this.settings,
                    n = t._gaq,
                    i = e.initCommands || [],
                    a = e.customInit;
                if (n || (_gaq = []), this.isSuppressed()) w.notify("GA: page code not loaded(suppressed).", 1);
                else {
                    if (!n && !k.scriptLoaded) {
                        var r = w.isHttps(),
                            o = (r ? "https://ssl" : "http://www") + ".google-analytics.com/ga.js";
                        e.url && (o = r ? e.url.https : e.url.http), w.loadScript(o), k.scriptLoaded = !0, w.notify("GA: page code loaded.", 1)
                    }
                    var s = (e.domain, e.trackerName),
                        c = T.allowLinker(),
                        u = w.replace(e.account, location);
                    w.settings.domainList || [];
                    _gaq.push([this.cmd("setAccount"), u]), c && _gaq.push([this.cmd("setAllowLinker"), c]), _gaq.push([this.cmd("setDomainName"), T.cookieDomain()]), w.each(i, function (t) {
                        var e = [this.cmd(t[0])].concat(w.preprocessArguments(t.slice(1), location, null, this.forceLowerCase));
                        _gaq.push(e)
                    }, this), a && (this.suppressInitialPageView = !1 === a(_gaq, s)), e.pageName && this.$overrideInitialPageView(null, null, e.pageName)
                }
                this.initialized = !0, w.fireEvent(this.id + ".configure", _gaq, s)
            },
            isSuppressed: function () {
                return this._cancelToolInit || this.settings.initTool === !1
            },
            tracker: function () {
                return this.settings.trackerName;
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
                        var t = w.preprocessArguments([this.urlOverride], location, null, this.forceLowerCase);
                        this.$missing$("trackPageview", null, null, t)
                    } else this.$missing$("trackPageview")
            },
            endPLPhase: function (t) {
                var e = this.settings.loadOn;
                t === e && (w.notify("GA: Initializing at " + t, 1), this.initialize(), this.flushQueue(), this.trackInitialPageView())
            },
            call: function (t, e, n, i) {
                if (!this._cancelToolInit) {
                    var a = (this.settings, this.tracker()),
                        r = this.cmd(t),
                        i = i ? [r].concat(i) : [r];
                    _gaq.push(i), a ? w.notify("GA: sent command " + t + " to tracker " + a + (i.length > 1 ? " with parameters [" + i.slice(1).join(", ") + "]" : "") + ".", 1) : w.notify("GA: sent command " + t + (i.length > 1 ? " with parameters [" + i.slice(1).join(", ") + "]" : "") + ".", 1)
                }
            },
            $missing$: function (t, e, n, i) {
                this.call(t, e, n, i)
            },
            $postTransaction: function (e, n, i) {
                var a = w.data.customVars.transaction = t[i];
                this.call("addTrans", e, n, [a.orderID, a.affiliation, a.total, a.tax, a.shipping, a.city, a.state, a.country]), w.each(a.items, function (t) {
                    this.call("addItem", e, n, [t.orderID, t.sku, t.product, t.category, t.unitPrice, t.quantity])
                }, this), this.call("trackTrans", e, n)
            },
            delayLink: function (t, e) {
                var n = this;
                if (T.allowLinker() && t.hostname.match(this.settings.linkerDomains) && !w.isSubdomainOf(t.hostname, location.hostname)) {
                    w.preventDefault(e);
                    var i = w.settings.linkDelay || 100;
                    setTimeout(function () {
                        n.call("link", t, e, [t.href])
                    }, i)
                }
            },
            popupLink: function (e, n) {
                if (t._gat) {
                    w.preventDefault(n);
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
                    w.isNaN(i) && (i = 1), n[3] = i
                }
                this.call("trackEvent", t, e, n)
            }
        }), w.availableTools.ga = k, w.inherit(S, w.BaseTool), w.extend(S.prototype, {
            name: "GAUniversal",
            endPLPhase: function (t) {
                var e = this.settings,
                    n = e.loadOn;
                t === n && (w.notify("GAU: Initializing at " + t, 1), this.initialize(), this.flushQueue(), this.trackInitialPageView())
            },
            getTrackerName: function () {
                return this.settings.trackerSettings.name || ""
            },
            isPageCodeLoadSuppressed: function () {
                return this.settings.initTool === !1 || this._cancelToolInit === !0
            },
            initialize: function () {
                if (this.isPageCodeLoadSuppressed()) return this.initialized = !0, void w.notify("GAU: Page code not loaded (suppressed).", 1);
                var e = "ga";
                t[e] = t[e] || this.createGAObject(), t.GoogleAnalyticsObject = e, w.notify("GAU: Page code loaded.", 1), w.loadScriptOnce(this.getToolUrl());
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
                e = w.preprocessArguments([e], location, null, this.forceLowerCase)[0], e.trackingId = w.replace(this.settings.trackerSettings.trackingId, location), e.cookieDomain || (e.cookieDomain = T.cookieDomain()), w.extend(e, t || {}), this.call("create", e)
            },
            autoLinkDomains: function () {
                var t = location.hostname;
                return w.filter(w.settings.domainList, function (e) {
                    return e !== t
                })
            },
            executeInitCommands: function () {
                var t = this.settings;
                t.initCommands && w.each(t.initCommands, function (t) {
                    var e = t.splice(2, t.length - 2);
                    t = t.concat(w.preprocessArguments(e, location, null, this.forceLowerCase)), this.call.apply(this, t)
                }, this)
            },
            trackInitialPageView: function () {
                this.suppressInitialPageView || this.isPageCodeLoadSuppressed() || this.call("send", "pageview")
            },
            call: function () {
                return "function" != typeof ga ? void w.notify("GA Universal function not found!", 4) : void (this.isCallSuppressed() || (arguments[0] = this.cmd(arguments[0]), this.log(w.toArray(arguments)), ga.apply(t, arguments)))
            },
            isCallSuppressed: function () {
                return this._cancelToolInit === !0
            },
            $missing$: function (t, e, n, i) {
                i = i || [], i = [t].concat(i), this.call.apply(this, i)
            },
            getToolUrl: function () {
                var t = this.settings,
                    e = w.isHttps();
                return t.url ? e ? t.url.https : t.url.http : (e ? "https://ssl" : "http://www") + ".google-analytics.com/analytics.js"
            },
            cmd: function (t) {
                var e = ["send", "set", "get"],
                    n = this.getTrackerName();
                return n && -1 !== w.indexOf(e, t) ? n + "." + t : t
            },
            log: function (t) {
                var e = t[0],
                    n = this.getTrackerName() || "default",
                    i = "GA Universal: sent command " + e + " to tracker " + n;
                if (t.length > 1) {
                    w.stringify(t.slice(1));
                    i += " with parameters " + w.stringify(t.slice(1))
                }
                i += ".", w.notify(i, 1)
            }
        }), w.availableTools.ga_universal = S;
        var T = {
            allowLinker: function () {
                return w.hasMultipleDomains()
            },
            cookieDomain: function () {
                var e = w.settings.domainList,
                    n = w.find(e, function (e) {
                        var n = t.location.hostname;
                        return w.equalsIgnoreCase(n.slice(n.length - e.length), e)
                    }),
                    i = n ? "." + n : "auto";
                return i
            }
        };
        w.ecommerce = {
            addItem: function () {
                var t = [].slice.call(arguments);
                w.onEvent({
                    type: "ecommerce.additem",
                    target: t
                })
            },
            addTrans: function () {
                var t = [].slice.call(arguments);
                w.data.saleData.sale = {
                    orderId: t[0],
                    revenue: t[2]
                }, w.onEvent({
                    type: "ecommerce.addtrans",
                    target: t
                })
            },
            trackTrans: function () {
                w.onEvent({
                    type: "ecommerce.tracktrans",
                    target: []
                })
            }
        }, w.visibility = {
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
        }, _satellite.init({
            tools: {
                "7a6e5ffce8293a2f9ddf48fa88932347": {
                    engine: "sc",
                    loadOn: "pagebottom",
                    euCookie: !1,
                    sCodeURL: "omniture-scode-2.0.4.js",
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
                    mboxURL: "mbox-2.0.4.js",
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
                        html: '<!-- Temp Page load rule --> \n  <script>\n    _satellite.notify("Temp - Adobe | Page load rule",1)\n</script>'
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
                libraryName: "satelliteLib-1.0.5",
                isStaging: !0,
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
                "account: charSet": {
                    jsVariable: "accountData.charSet",
                    storeLength: "pageview"
                },
                "account: currencyCode": {
                    jsVariable: "accountData.currencyCode",
                    storeLength: "pageview"
                },
                "account: suiteId": {
                    jsVariable: "accountData.suiteId",
                    storeLength: "pageview"
                }
            },
            appVersion: "6I2",
            buildDate: "2016-09-28 23:43:09 UTC",
            publishDate: "2016-09-21 23:31:41 UTC"
        })
    }(window, document);