using DDD.Scripts.Game.rock_paper_scissors;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DDD.Scripts.Core
{
    public class DDDWelcomePopupComponent : DDDPopupComponentBase
    {
        [SerializeField]
        private CanvasGroup anim;
        
        [SerializeField]
        private TMP_Text message;
        

        public override void Init(DDDPopupData popupData)
        {
            base.Init(popupData);
            anim.DOFade(1, 0.4F).OnComplete((() =>
            {
                OnOpenPopup();
            }));
           
        }

        protected override void OnOpenPopup()
        {
            anim.interactable = true;
            anim.blocksRaycasts = true;
            if (popupData.GenericData != null)
            {
                message.text = popupData.GenericData.ToString();
            }

            base.OnOpenPopup();
        }
    }
}