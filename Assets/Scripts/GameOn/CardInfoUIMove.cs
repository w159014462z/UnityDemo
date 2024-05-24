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
        // ��¼��ʼ��קʱ��ԭʼλ��
        prePosition = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ������Ʒ��λ�ã�ʹ���������ƶ�        
        dragOffset = Input.mousePosition - prePosition;
        transform.position += dragOffset;
        prePosition = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ������קʱ������ѡ������Ʒ�ص�ԭʼλ�ã����߷������µ�λ��
        
    }

}
