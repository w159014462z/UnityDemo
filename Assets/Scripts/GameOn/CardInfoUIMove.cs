using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInfoUIMove : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler
{    
    private Vector3 prePosition;
    private Vector3 dragOffset; 
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 记录开始拖拽时的原始位置
        prePosition = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 更新物品的位置，使其跟随鼠标移动        
        dragOffset = Input.mousePosition - prePosition;
        transform.position += dragOffset;
        prePosition = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 结束拖拽时，可以选择让物品回到原始位置，或者放置在新的位置
        
    }

}
