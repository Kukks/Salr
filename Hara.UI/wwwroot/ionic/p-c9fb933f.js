import{b as t,c as n}from"./p-275c7570.js";import{a as o,b as e,g as a}from"./p-a48a5ea6.js";import{OVERLAY_BACK_BUTTON_PRIORITY as s}from"./p-28891211.js";let i=0;const r=new WeakMap,c=t=>({create:n=>v(t,n),dismiss:(n,o,e)=>g(document,n,o,t,e),getTop:async()=>k(document,t)}),d=c("ion-alert"),u=c("ion-action-sheet"),p=c("ion-loading"),l=c("ion-modal"),m=c("ion-picker"),f=c("ion-popover"),y=c("ion-toast"),b=t=>{"undefined"!=typeof document&&w(document);const n=i++;t.overlayIndex=n,t.hasAttribute("id")||(t.id="ion-overlay-"+n)},v=(t,n)=>"undefined"!=typeof customElements?customElements.whenDefined(t).then((()=>{const o=document.createElement(t);return o.classList.add("overlay-hidden"),Object.assign(o,n),E(document).appendChild(o),o.componentOnReady()})):Promise.resolve(),x='[tabindex]:not([tabindex^="-"]), input:not([type=hidden]):not([tabindex^="-"]), textarea:not([tabindex^="-"]), button:not([tabindex^="-"]), select:not([tabindex^="-"]), .ion-focusable:not([tabindex^="-"])',h="input:not([type=hidden]), textarea, button, select",w=t=>{0===i&&(i=1,t.addEventListener("focus",(n=>((t,n)=>{const o=k(n),e=t.target;if(o&&e)if(o===e)o.lastFocus=void 0;else{const t=a(o);if(!t.contains(e))return;const s=t.querySelector(".ion-overlay-wrapper");if(!s)return;if(s.contains(e))o.lastFocus=e;else{const t=o.lastFocus;((t,n)=>{let o=t.querySelector(x);const e=o&&o.shadowRoot;e&&(o=e.querySelector(h)||o),o?o.focus():n.focus()})(s,o),t===n.activeElement&&((t,n)=>{const o=Array.from(t.querySelectorAll(x));let e=o.length>0?o[o.length-1]:null;const a=e&&e.shadowRoot;a&&(e=a.querySelector(h)||e),e?e.focus():n.focus()})(s,o),o.lastFocus=n.activeElement}}})(n,t)),!0),t.addEventListener("ionBackButton",(n=>{const o=k(t);o&&o.backdropDismiss&&n.detail.register(s,(()=>o.dismiss(void 0,M)))})),t.addEventListener("keyup",(n=>{if("Escape"===n.key){const n=k(t);n&&n.backdropDismiss&&n.dismiss(void 0,M)}})))},g=(t,n,o,e,a)=>{const s=k(t,e,a);return s?s.dismiss(n,o):Promise.reject("overlay does not exist")},k=(t,n,o)=>{const e=((t,n)=>(void 0===n&&(n="ion-alert,ion-action-sheet,ion-loading,ion-modal,ion-picker,ion-popover,ion-toast"),Array.from(t.querySelectorAll(n)).filter((t=>t.overlayIndex>0))))(t,n);return void 0===o?e[e.length-1]:e.find((t=>t.id===o))},j=async(o,e,a,s,i)=>{if(o.presented)return;o.presented=!0,o.willPresent.emit();const r=t(o),c=o.enterAnimation?o.enterAnimation:n.get(e,"ios"===r?a:s);await O(o,c,o.el,i)&&o.didPresent.emit(),"ION-TOAST"!==o.el.tagName&&A(o.el),o.keyboardClose&&o.el.focus()},A=async t=>{let n=document.activeElement;if(!n)return;const o=n&&n.shadowRoot;o&&(n=o.querySelector(h)||n),await t.onDidDismiss(),n.focus()},B=async(o,e,a,s,i,c,d)=>{if(!o.presented)return!1;o.presented=!1;try{o.el.style.setProperty("pointer-events","none"),o.willDismiss.emit({data:e,role:a});const u=t(o),p=o.leaveAnimation?o.leaveAnimation:n.get(s,"ios"===u?i:c);"gesture"!==a&&await O(o,p,o.el,d),o.didDismiss.emit({data:e,role:a}),r.delete(o)}catch(u){console.error(u)}return o.el.remove(),!0},E=t=>t.querySelector("ion-app")||t.body,O=async(t,o,e,a)=>{e.classList.remove("overlay-hidden");const s=o(e.shadowRoot||t.el,a);t.animated&&n.getBoolean("animated",!0)||s.duration(0),t.keyboardClose&&s.beforeAddWrite((()=>{const t=e.ownerDocument.activeElement;t&&t.matches("input, ion-input, ion-textarea")&&t.blur()}));const i=r.get(t)||[];return r.set(t,[...i,s]),await s.play(),!0},P=(t,n)=>{let o;const e=new Promise((t=>o=t));return T(t,n,(t=>{o(t.detail)})),e},T=(t,n,a)=>{const s=o=>{e(t,n,s),a(o)};o(t,n,s)},z=t=>"cancel"===t||t===M,G=t=>t(),I=(t,o)=>{if("function"==typeof t)return n.get("_zoneGate",G)((()=>{try{return t(o)}catch(n){console.error(n)}}))},M="backdrop";export{M as B,d as a,u as b,f as c,j as d,b as e,B as f,P as g,r as h,z as i,p as l,l as m,m as p,I as s,y as t}