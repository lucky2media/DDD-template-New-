using System.Collections.Generic;

namespace DDD.Scripts.Core
{
    public class DDDPopupComponentBase : DDDMonoBehaviour
    {
        protected DDDPopupData popupData;
        
        public virtual void Init(DDDPopupData popupData)
        {
            this.popupData = popupData;
            OnOpenPopup();
        }
        
        protected virtual void OnOpenPopup()
        {
            var data = new Dictionary<DDDDataKeys, object>();
            data.Add(DDDDataKeys.popup_type, popupData.PopupType.ToString());
            
            popupData.OnPopupOpen?.Invoke();
        }
        
        public virtual void ClosePopup()
        {
            OnClosePopup();
        }

        protected virtual void OnClosePopup()
        {
            var data = new Dictionary<DDDDataKeys, object>();
            data.Add(DDDDataKeys.popup_type, popupData.PopupType.ToString());
            
            popupData.OnPopupClose?.Invoke(this);
        }
    }
    
    public enum DDDDataKeys
    {
        popup_type,
    }
}