document.addEventListener("DOMContentLoaded",()=>{let t="user-info-popup",e=document.querySelector("#"+t),n=document.querySelector("#nick-name-field"),o=document.querySelector("#first-name-field"),r=document.querySelector("#last-name-field"),u=document.querySelector("#country-field"),d=document.querySelector("#state-field"),l=document.querySelector("#city-field"),a=document.querySelector("#address-field"),c=document.querySelector("#phone-field"),i=document.querySelector("#zip-code-field");var s=document.querySelector("#save-user-details-btn"),p=document.querySelector("#close-user-details-btn");checkFormValidity(e,s),String.prototype.toPhoneFormat=function(){return this.replace(/[^0-9]/g,"").substr(0,12).split("").reduce(function(e,t,n){return e+(!n||n%4?"":" ")+t},"")},i.addEventListener("keyup",function(e){e=e.target.value.replace(/\D/g,"").match(/(\d{0,5})/),i.value=e[1]||""}),c.addEventListener("keyup",function(e){e=e.target.value.replace(/\D/g,"").match(/(\d{0,3})(\d{0,3})(\d{0,3})(\d{0,3})/),c.value=e[2]?` +${e[1]} ${e[2]}-${e[3]}-`+(e[4]||""):e[1]}),s.addEventListener("click",function(){var e=[n,o,r,u,d,l,a,c,i];e.forEach(e=>{""===e.value&&(e.style.border="1px solid red")}),(e=[...e].every(e=>!!e.value))&&(e=`{
            'phone': '${c.value}',
            'userName': '${n.value}',
            'firstName': '${o.value}',
            'lastName': '${r.value}',
            'gender': 'none',
            'address': '${a.value}',
            'zipCode': '${i.value}',
            'city': '${l.value}',
            'state': '${d.value}',
            'country': '${u.value}'
        } `,console.log("js: "+e),hidePopup(t),window.unityInstance.SendMessage("Application","NotifyUserInfoHTMLPopupClose",e),clearCreditCardInfoOnPopup())}),p.addEventListener("click",function(){clearModalValues(e),hidePopup(t),window.unityInstance.SendMessage("Application","NotifyUserInfoHTMLPopupClose","")})});