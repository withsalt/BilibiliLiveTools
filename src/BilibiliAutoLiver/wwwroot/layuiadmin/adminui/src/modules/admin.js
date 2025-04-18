/** The Web UI Theme-v2.4.0 */ ;
layui.define("view", function(e) {
	function t(e) {
		var a = e.attr("lay-id"),
			t = (e.attr("lay-attr"), e.index());
		T.tabsBodyChange(t, {
			url: a,
			title: e.children("span").text()
		})
	}
	var d = layui.jquery,
		s = layui.laytpl,
		a = layui.table,
		i = layui.element,
		l = layui.util,
		n = layui.upload,
		r = (layui.form, layui.setter),
		o = layui.view,
		u = layui.device(),
		c = d(window),
		y = d(document),
		m = d("body"),
		f = d("#" + r.container),
		h = "layui-show",
		p = "layui-this",
		b = "layui-disabled",
		v = "#LAY_app_body",
		g = "LAY_app_flexible",
		x = "layadmin-layout-tabs",
		C = "layadmin-side-spread-sm",
		k = "layadmin-tabsbody-item",
		P = "layui-icon-shrink-right",
		F = "layui-icon-spread-left",
		A = "layadmin-side-shrink",
		T = {
			v: "2.4.0",
			mode: "iframe",
			req: o.req,
			exit: o.exit,
			escape: l.escape,
			on: function(e, a) {
				return layui.onevent.call(this, r.MOD_NAME, e, a)
			},
			sendAuthCode: function(i) {
				function l(e) {
					--n < 0 ? (t.removeClass(b).html("\u83b7\u53d6\u9a8c\u8bc1\u7801"), n = i.seconds,
						clearInterval(a)) : t.addClass(b).html(n + "\u79d2\u540e\u91cd\u83b7"), e || (
						a = setInterval(function() {
							l(!0)
						}, 1e3))
				}
				var a, n = (i = d.extend({
						seconds: 60,
						elemPhone: "#LAY_phone",
						elemVercode: "#LAY_vercode"
					}, i)).seconds,
					t = d(i.elem);
				i.elemPhone = d(i.elemPhone), i.elemVercode = d(i.elemVercode), t.on("click", function() {
					var a, e = i.elemPhone,
						t = e.val();
					if (n === i.seconds && !d(this).hasClass(b)) {
						if (!/^1\d{10}$/.test(t)) return e.focus(), layer.msg(
							"\u8bf7\u8f93\u5165\u6b63\u786e\u7684\u624b\u673a\u53f7");
						"object" == typeof i.ajax && (a = i.ajax.success, delete i.ajax.success), T
							.req(d.extend(!0, {
								url: "/auth/code",
								type: "get",
								data: {
									phone: t
								},
								success: function(e) {
									layer.msg(
										"\u9a8c\u8bc1\u7801\u5df2\u53d1\u9001\u81f3\u4f60\u7684\u624b\u673a\uff0c\u8bf7\u6ce8\u610f\u67e5\u6536", {
											icon: 1,
											shade: 0
										}), i.elemVercode.focus(), l(), a && a(e)
								}
							}, i.ajax))
					}
				})
			},
			screen: function() {
				var e = c.width();
				return 1200 < e ? 3 : 992 < e ? 2 : 768 < e ? 1 : 0
			},
			sideFlexible: function(e) {
				var a = f,
					t = d("#" + g),
					i = T.screen();
				"spread" === e ? (t.removeClass(F).addClass(P), i < 2 ? a.addClass(C) : a.removeClass(C), a
					.removeClass(A)) : (t.removeClass(P).addClass(F), i < 2 ? a.removeClass(A) : a
					.addClass(A), a.removeClass(C)), layui.event.call(this, r.MOD_NAME, "side({*})", {
					status: e
				})
			},
			popup: o.popup,
			popupRight: function(e) {
				return T.popup.index = layer.open(d.extend({
					type: 1,
					id: "LAY_adminPopupR",
					anim: -1,
					title: !1,
					closeBtn: !1,
					offset: "r",
					shade: .1,
					shadeClose: !0,
					skin: "layui-anim layui-anim-rl layui-layer-adminRight",
					area: "300px"
				}, e))
			},
			theme: function(e) {
				r.theme;
				var t = layui.data(r.tableName),
					a = "LAY_layadmin_theme",
					i = document.getElementById(a),
					l = document.createElement("style");
				if (e.CLEAR) return d(i).remove(), layui.data(r.tableName, {
					key: "theme",
					remove: !0
				});
				var n = s([".layui-side-menu,", ".layui-layer-admin .layui-layer-title,",
					".layadmin-side-shrink .layui-side-menu .layui-nav>.layui-nav-item>.layui-nav-child",
					"{background-color:{{d.color.main}} !important;}",
					".layadmin-pagetabs .layui-tab-title li:after,",
					".layadmin-pagetabs .layui-tab-title li.layui-this:after,",
					".layui-nav-tree .layui-this,", ".layui-nav-tree .layui-this>a,",
					".layui-nav-tree .layui-nav-child dd.layui-this,",
					".layui-nav-tree .layui-nav-child dd.layui-this a,",
					".layui-nav-tree .layui-nav-bar",
					"{background-color:{{d.color.selected}} !important;}",
					".layadmin-pagetabs .layui-tab-title li:hover,",
					".layadmin-pagetabs .layui-tab-title li.layui-this",
					"{color: {{d.color.selected}} !important;}",
					".layui-layout-admin .layui-logo{background-color:{{d.color.logo || d.color.main}} !important;}",
					"{{# if(d.color.header){ }}",
					".layui-layout-admin .layui-header{background-color:{{ d.color.header }};}",
					".layui-layout-admin .layui-header a,",
					".layui-layout-admin .layui-header a cite{color: #f8f8f8;}",
					".layui-layout-admin .layui-header a:hover{color: #fff;}",
					".layui-layout-admin .layui-header .layui-nav .layui-nav-more{border-top-color: #fbfbfb;}",
					".layui-layout-admin .layui-header .layui-nav .layui-nav-mored{border-color: transparent; border-bottom-color: #fbfbfb;}",
					".layui-layout-admin .layui-header .layui-nav .layui-this:after, .layui-layout-admin .layui-header .layui-nav-bar{background-color: #fff; background-color: rgba(255,255,255,.5);}",
					".layadmin-pagetabs .layui-tab-title li:after{display: none;}", "{{# } }}"
				].join("")).render(e = d.extend({}, t.theme, e));
				"styleSheet" in l ? (l.setAttribute("type", "text/css"), l.styleSheet.cssText = n) : l
					.innerHTML = n, l.id = a, i && m[0].removeChild(i), m[0].appendChild(l), e.color && m
					.attr("layadmin-themealias", e.color.alias), t.theme = t.theme || {}, layui.each(e,
						function(e, a) {
							t.theme[e] = a
						}), layui.data(r.tableName, {
						key: "theme",
						value: t.theme
					})
			},
			initTheme: function(e) {
				var a = r.theme;
				a.color[e = e || 0] && (a.color[e].index = e, T.theme({
					color: a.color[e]
				}))
			},
			tabsPage: {},
			tabsBody: function(e) {
				return d(v).find("." + k).eq(e || 0)
			},
			tabsBodyChange: function(e, a) {
				a = a || {}, T.tabsBody(e).addClass(h).siblings().removeClass(h), _.rollPage("auto", e), T
					.recordURL(a), layui.event.call(this, r.MOD_NAME, "tabsPage({*})", a)
			},
			recordURL: function(e) {
				var a;
				(r.record || {}).url && e.url && (/^(\w*:)*\/\/.+/.test(e.url) && (e.url = ""), location
					.hash = T.correctRouter(e.url), e.url && e.title && ((a = {})[e.url] = e.title,
						layui.data(r.tableName, {
							key: "record",
							value: a
						})))
			},
			resize: function(e) {
				var a = layui.router().path.join("-");
				T.resizeFn[a] && (c.off("resize", T.resizeFn[a]), delete T.resizeFn[a]), "off" !== e && (
				e(), T.resizeFn[a] = e, c.on("resize", T.resizeFn[a]))
			},
			resizeFn: {},
			runResize: function() {
				var e = layui.router().path.join("-");
				T.resizeFn[e] && T.resizeFn[e]()
			},
			delResize: function() {
				this.resize("off")
			},
			closeThisTabs: function() {
				T.tabsPage.index && d(L).eq(T.tabsPage.index).find(".layui-tab-close").trigger("click")
			},
			fullScreen: function() {
				var e = document.documentElement,
					a = e.requestFullscreen || e.webkitRequestFullScreen || e.mozRequestFullScreen || e
					.msRequestFullscreen;
				void 0 !== a && a && a.call(e)
			},
			exitScreen: function() {
				document.documentElement;
				document.exitFullscreen ? document.exitFullscreen() : document.mozCancelFullScreen ?
					document.mozCancelFullScreen() : document.webkitCancelFullScreen ? document
					.webkitCancelFullScreen() : document.msExitFullscreen && document.msExitFullscreen()
			},
			correctRouter: function(e) {
				return (e = /^\//.test(e) ? e : "/" + e).replace(/^(\/+)/, "/").replace(new RegExp("/" + r
					.entry + "$"), "/")
			}
		},
		_ = T.events = {
			flexible: function(e) {
				e = e.find("#" + g).hasClass(F);
				T.sideFlexible(e ? "spread" : null)
			},
			refresh: function() {
				var e = d("." + k).length;
				T.tabsPage.index >= e && (T.tabsPage.index = e - 1), T.tabsBody(T.tabsPage.index).find(
					".layadmin-iframe")[0].contentWindow.location.reload(!0)
			},
			serach: function(t) {
				t.off("keypress").on("keypress", function(e) {
					var a;
					this.value.replace(/\s/g, "") && 13 === e.keyCode && (e = t.attr("lay-action"),
						a = t.attr("lay-title") || t.attr("lay-text") || "\u641c\u7d22", e +=
						this.value, a = a + ": " + T.escape(this.value), T.openTabsPage({
							url: e,
							title: a,
							highlight: "color: #FF5722;"
						}), _.serach.keys || (_.serach.keys = {}), _.serach.keys[T.tabsPage
							.index] = this.value, this.value === _.serach.keys[T.tabsPage
						.index] && _.refresh(t), this.value = "")
				})
			},
			message: function(e) {
				e.find(".layui-badge-dot").remove()
			},
			theme: function() {
				T.popupRight({
					id: "LAY_adminPopupTheme",
					success: function() {
						o(this.id).render("system/theme")
					}
				})
			},
			note: function(e) {
				var a = T.screen() < 2,
					t = layui.data(r.tableName).note;
				_.note.index = T.popup({
					title: "\u672c\u5730\u4fbf\u7b7e",
					shade: 0,
					offset: ["41px", a ? null : e.offset().left - 250 + "px"],
					anim: -1,
					id: "LAY_adminNote",
					skin: "layadmin-note layui-anim layui-anim-upbit",
					content: '<textarea placeholder="\u5185\u5bb9"></textarea>',
					resize: !1,
					success: function(e, a) {
						e.find("textarea").val(void 0 === t ?
							"\u4fbf\u7b7e\u4e2d\u7684\u5185\u5bb9\u4f1a\u5b58\u50a8\u5728\u672c\u5730\uff0c\u8fd9\u6837\u5373\u4fbf\u4f60\u5173\u6389\u4e86\u6d4f\u89c8\u5668\uff0c\u5728\u4e0b\u6b21\u6253\u5f00\u65f6\uff0c\u4f9d\u7136\u4f1a\u8bfb\u53d6\u5230\u4e0a\u4e00\u6b21\u7684\u8bb0\u5f55\u3002\u662f\u4e2a\u975e\u5e38\u5c0f\u5de7\u5b9e\u7528\u7684\u672c\u5730\u5907\u5fd8\u5f55" :
							t).focus().on("keyup", function() {
							layui.data(r.tableName, {
								key: "note",
								value: this.value
							})
						})
					}
				})
			},
			fullscreen: function(e, a) {
				function t(e) {
					e ? n.addClass(l).removeClass(i) : n.addClass(i).removeClass(l)
				}
				var i = "layui-icon-screen-full",
					l = "layui-icon-screen-restore",
					n = e.children("i"),
					e = n.hasClass(i);
				if (a) return t(a.status);
				t(e), e ? T.fullScreen() : T.exitScreen()
			},
			about: function() {
				T.popupRight({
					id: "LAY_adminPopupAbout",
					success: function() {
						o(this.id).render("system/about")
					}
				})
			},
			more: function() {
				T.popupRight({
					id: "LAY_adminPopupMore",
					success: function() {
						o(this.id).render("system/more")
					}
				})
			},
			back: function() {
				history.back()
			},
			setTheme: function(e) {
				var a = e.data("index");
				e.siblings(".layui-this").data("index");
				e.hasClass(p) || (e.addClass(p).siblings(".layui-this").removeClass(p), T.initTheme(a), o(
					"LAY_adminPopupTheme").render("system/theme"))
			},
			rollPage: function(e, a) {
				var t, i = d("#LAY_app_tabsheader"),
					l = i.children("li"),
					n = (i.prop("scrollWidth"), i.outerWidth()),
					s = parseFloat(i.css("left"));
				if ("left" === e) !s && s <= 0 || (t = -s - n, l.each(function(e, a) {
					a = d(a).position().left;
					if (t <= a) return i.css("left", -a), !1
				}));
				else if ("auto" === e) {
					var r, e = l.eq(a);
					if (e[0]) {
						if ((a = e.position().left) < -s) return void i.css("left", -a);
						a + e.outerWidth() >= n - s && (r = a + e.outerWidth() - (n - s), l.each(function(e,
							a) {
							a = d(a).position().left;
							if (0 < a + s && r < a - s) return i.css("left", -a), !1
						}))
					}
				} else l.each(function(e, a) {
					var a = d(a),
						t = a.position().left;
					if (t + a.outerWidth() >= n - s) return i.css("left", -t), !1
				})
			},
			leftPage: function() {
				_.rollPage("left")
			},
			rightPage: function() {
				_.rollPage()
			},
			closeThisTabs: function() {
				(parent === self ? T : parent.layui.admin).closeThisTabs()
			},
			closeOtherTabs: function(e) {
				var t = "LAY-system-pagetabs-remove";
				"all" === e ? (d(L + ":gt(0)").remove(), d(v).find("." + k + ":gt(0)").remove(), d(L).eq(0)
					.trigger("click")) : (d(L).each(function(e, a) {
					e && e != T.tabsPage.index && (d(a).addClass(t), T.tabsBody(e).addClass(t))
				}), d("." + t).remove())
			},
			closeAllTabs: function() {
				_.closeOtherTabs("all")
			},
			shade: function() {
				T.sideFlexible()
			},
			im: function() {
				T.popup({
					id: "LAY-popup-layim-demo",
					shade: 0,
					area: ["800px", "300px"],
					title: "\u9762\u677f\u5916\u7684\u64cd\u4f5c\u793a\u4f8b",
					offset: "lb",
					success: function() {
						layui.view(this.id).render("layim/demo").then(function() {
							layui.use("im")
						})
					}
				})
			}
		},
		L = ("pageTabs" in layui.setter || (layui.setter.pageTabs = !0), r.pageTabs || (d("#LAY_app_tabs")
			.addClass("layui-hide"), f.addClass("layadmin-tabspage-none")), u.ie && u.ie < 10 && o.error(
			"IE" + u.ie +
			"\u4e0b\u8bbf\u95ee\u53ef\u80fd\u4e0d\u4f73\uff0c\u63a8\u8350\u4f7f\u7528\uff1aChrome / Firefox / Edge \u7b49\u9ad8\u7ea7\u6d4f\u89c8\u5668", {
				offset: "auto",
				id: "LAY_errorIE"
			}), (l = layui.data(r.tableName)).theme ? T.theme(l.theme) : r.theme && T.initTheme(r.theme
			.initColorIndex), i.on("tab(" + x + ")", function(e) {
			T.tabsPage.index = e.index
		}), T.on("tabsPage(setMenustatus)", function(e) {
			function s(e) {
				return {
					list: e.children(".layui-nav-child"),
					a: e.children("*[lay-href]"),
					name: e.data("name")
				}
			}
			var r = e.url,
				o = r.split("/"),
				e = d("#LAY-system-side-menu"),
				u = "layui-nav-itemed";
			e.find("." + p).removeClass(p), T.screen() < 2 && T.sideFlexible(), e.children("li").each(
				function(e, a) {
					var a = d(a),
						n = s(a),
						t = n.list.children("dd"),
						i = o[0] == n.name || r === n.a.attr("lay-href");
					if (t.each(function(e, a) {
							var a = d(a),
								t = s(a),
								i = t.list.children("dd"),
								l = o[0] == n.name && o[1] == t.name || r === t.a.attr(
									"lay-href");
							if (i.each(function(e, a) {
									var a = d(a),
										t = s(a);
									if (r === t.a.attr("lay-href")) return t = t.list[0] ?
										u : p, a.addClass(t).siblings().removeClass(t),
										!1
								}), l) return i = t.list[0] ? u : p, a.addClass(i).siblings()
								.removeClass(i), !1
						}), i) return t = n.list[0] ? u : p, a.addClass(t).siblings().removeClass(
						t), !1
				})
		}), i.on("nav(layadmin-system-side-menu)", function(e) {
			e.siblings(".layui-nav-child")[0] && f.hasClass(A) && (T.sideFlexible("spread"), layer
				.close(e.data("index"))), T.tabsPage.type = "nav"
		}), i.on("nav(layadmin-pagetabs-nav)", function(e) {
			e = e.parent();
			e.removeClass(p), e.parent().removeClass(h)
		}), "#LAY_app_tabsheader>li"),
		z = (m.on("click", L, function() {
			var e = d(this),
				a = e.index();
			T.tabsPage.type = "tab", T.tabsPage.index = a, t(e)
		}), i.on("tabDelete(" + x + ")", function(e) {
			var a = d(L + ".layui-this");
			e.index && T.tabsBody(e.index).remove(), t(a), T.delResize()
		}), m.on("click", "*[lay-href]", function() {
			var e = d(this),
				a = e.attr("lay-href"),
				t = e.attr("lay-title") || e.attr("lay-text") || e.text();
			T.tabsPage.elem = e, (parent === self ? layui : r.parentLayui || top.layui).admin
				.openTabsPage({
					url: a,
					title: t
				}), r.pageTabs && r.refreshCurrPage && a === T.tabsBody(T.tabsPage.index).find("iframe")
				.attr("src") && T.events.refresh()
		}), m.on("click", "*[layadmin-event]", function() {
			var e = d(this),
				a = e.attr("layadmin-event");
			_[a] && _[a].call(this, e)
		}), m.on("mouseenter", "*[lay-tips]", function() {
			var t, e, a, i = d(this);
			i.parent().hasClass("layui-nav-item") && !f.hasClass(A) || (a = i.attr("lay-tips"), t = i
				.attr("lay-offset"), e = i.attr("lay-direction"), a = layer.tips(a, this, {
					tips: e || 1,
					time: -1,
					success: function(e, a) {
						t && e.css("margin-left", t + "px")
					}
				}), i.data("index", a))
		}).on("mouseleave", "*[lay-tips]", function() {
			layer.close(d(this).data("index"))
		}), layui.data.resizeSystem = function() {
			layer.closeAll("tips"), z.lock || setTimeout(function() {
				T.sideFlexible(T.screen() < 2 ? "" : "spread"), delete z.lock
			}, 100), z.lock = !0
		});
	c.on("resize", layui.data.resizeSystem), y.on("fullscreenchange", function() {
		_.fullscreen(d('[layadmin-event="fullscreen"]'), {
			status: document.fullscreenElement
		})
	}), (u = r.request).tokenName && ((l = {})[u.tokenName] = layui.data(r.tableName)[u.tokenName] || "", a
		.set({
			headers: l,
			where: l
		}), n.set({
			headers: l,
			data: l
		})), e("admin", T)
});