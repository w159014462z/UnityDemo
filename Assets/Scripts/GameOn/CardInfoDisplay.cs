using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System;
using System.Security.Cryptography.X509Certificates;

//设置当鼠标点击手牌和人物牌以及装备牌时的放大显示，方便看效果
public class CardInfoDisplay : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    private bool pointerDown;
    private static bool activeDisp;
    private float pointerDownTime;
    private Toggle toggle;
    private Sprite cardImageSp;
    private GameObject cardInfoDisp;
    private Button exitButton;
    List<CardInfo> cardInfoList = new List<CardInfo>();
    
    private void Awake()
    {
        pointerDown = false;
        activeDisp = false;
        pointerDownTime = 0;
        toggle = this.GetComponent<Toggle>();
        cardInfoDisp = GameObject.Find("CardInfo");
        exitButton=cardInfoDisp.transform.GetChild(1).GetComponent<Button>(); 
        exitButton.onClick.AddListener(OnExitButtonClick);
        AddCardInfo();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (transform.GetComponent<Image>().sprite == null) return;
        if (cardImageSp == null)
            GetCardImageSp();        
        if (transform.tag == "HandCard") {
            if (transform.GetComponent<Toggle>().isOn) {
                cardInfoDisp.transform.GetChild(0).GetComponent<Image>().sprite = cardImageSp;
                if(!activeDisp)
                    cardInfoDisp.transform.localPosition = new Vector3(0, 50, 0);                
            }
            
        }
        if(transform.tag == "Weapon" || transform.tag == "Mount" || transform.tag=="Defend") {
            cardInfoDisp.transform.GetChild(0).GetComponent<Image>().sprite = cardImageSp;
            if (!activeDisp)
                cardInfoDisp.transform.localPosition = new Vector3(0, 50, 0);
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = 0;
        pointerDown = true;
        if (activeDisp) {
            //if (transform.tag == "Player" || transform.tag == "Enemy"){
            //备用
            //}
            if (cardImageSp == null)
                return;            
            cardInfoDisp.transform.GetChild(0).GetComponent<Image>().sprite = cardImageSp;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;        
    }

    public void OnExitButtonClick()
    {
        activeDisp = false;
        pointerDownTime = 0;             
        cardInfoDisp.transform.localPosition = new Vector2(3000, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeDisp)
            return;
        if (transform.tag == "HandCard" || transform.tag == "Weapon" || transform.tag == "Mount" || transform.tag=="Defend") {
            cardInfoDisp.transform.localPosition = new Vector3(3000, 0 , 0);
        }
    }       

    //获取当前卡牌图片对应预制体的sprite到CardImageSp游戏物体
    private void GetCardImageSp() {
        this.name = transform.GetComponent<Image>().sprite.name;//修改卡牌名字为绑定的sprite名字，即初始化cardGo的名字
        int leftKHidx = this.name.IndexOf('(');
        if (leftKHidx != -1)
        {
            string belong_prefab_name;
            if (this.name[leftKHidx - 1] != ' ')
                belong_prefab_name = this.name.Substring(0, leftKHidx);
            else
                belong_prefab_name = this.name.Substring(0, leftKHidx - 1);
            cardImageSp = Resources.Load<Sprite>(belong_prefab_name);
        }
        else
        {
            cardImageSp = Resources.Load<Sprite>(this.name);
        }
    }

    private void Update()
    {
        //指向非人物牌或者指向人物牌但没有长按鼠标左键时返回
        if (transform.tag == "HandCard" || !pointerDown || activeDisp)
            return;
        //实现长按显示人物信息
        if (pointerDownTime < 0.4f)
            pointerDownTime += Time.deltaTime;
        if (pointerDownTime > 0.4f) {
            if (transform.tag == "Player" || transform.tag == "Enemy") {
                cardInfoDisp.transform.GetChild(0).GetComponent<Image>().sprite = cardImageSp;
                if (cardInfoDisp.transform.localPosition != new Vector3(0,50,0))
                    cardInfoDisp.transform.localPosition = new Vector3(0, 50, 0);
                activeDisp = true;
            }
        }
    }

    //添加卡牌信息，方便后面写新版ui显示卡牌信息
    public void AddCardInfo() {
        cardInfoList.Add(new CardInfo("攻", "使用攻给对方造成1点伤害"));
        //...
    }

}

public class CardInfo
{
    private string name;
    private string description;
    public string Description {  get { return description; } }
    public string Name { get { return name; } }
    public CardInfo(string name, string description){
        this.name = name;   
        this.description = description;
    }

   
}

