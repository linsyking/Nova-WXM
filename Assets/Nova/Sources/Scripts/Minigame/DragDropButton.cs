using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nova
{
    public class DragDropButton : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private int startIndex;
        private int originalIndex;
        private int sibling;

        private float topPadding;
        private float height;
        private float spacing;

        private Camera cam;
        private float own_z;
        private Transform myparentTransform;

        private void Start()
        {
            cam = UICameraHelper.Active;
            myparentTransform = transform.parent;
            own_z = cam.WorldToScreenPoint(myparentTransform.position).z;

            topPadding = myparentTransform.parent.GetComponent<VerticalLayoutGroup>().padding.top;
            spacing = myparentTransform.parent.GetComponent<VerticalLayoutGroup>().spacing;
            height = myparentTransform.GetComponent<RectTransform>().sizeDelta.y;

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            myparentTransform.SetSiblingIndex(myparentTransform.parent.GetSiblingIndex() - 1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            
            var screenR = new Vector3(eventData.position.x, eventData.position.y, own_z);
            var changedPosition = cam.ScreenToWorldPoint(screenR);
            changedPosition.x = myparentTransform.position.x;
            myparentTransform.position = changedPosition;

            var rsib = 0;

            foreach(Transform child in myparentTransform.parent)
            {
                var thisy = child.localPosition.y;
                if (myparentTransform.localPosition.y < thisy)
                {
                    rsib++;
                }
            }
            sibling = rsib;

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ChangeSibling();
        }

        private void ChangeSibling()
        {
            myparentTransform.SetSiblingIndex(sibling);
            //myparentTransform.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            LayoutRebuilder.ForceRebuildLayoutImmediate(myparentTransform.parent.GetComponent<RectTransform>());
            
        }

    }

}
