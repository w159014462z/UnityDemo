using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CardButton : MonoBehaviour
{
    // Start is called before the first frame update
    public Toggle toggle;
    public CardNorData cardNorData;
    public GameObject playerCardGo;
    public GameController gameController;
    private void Awake() {
        toggle = GetComponent<Toggle>();
        playerCardGo = GameObject.Find("PlayerCard");
        gameController=GameObject.Find("Canvas").GetComponent<GameController>();
    }

    private void Update()
    {
        
    }

    private void LateUpdate() {
        cardNorData = FindNowPlayerCardNorDataByGoTag("Player");
        if (toggle.group == null) {
            toggle.group = transform.parent.GetComponent<ToggleGroup>();
        }
        
        if (cardNorData!=null && toggle.isOn && !cardNorData.outCarding && cardNorData.currentClickCard != this.gameObject) {
            cardNorData.currentClickCard = this.gameObject;
        }
    }

    //通过标签找到对应标签的正处于自身回合的游戏物体的CardNorData脚本文件
    public CardNorData FindNowPlayerCardNorDataByGoTag(string tag)
    {
        GameObject[] goArr = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < goArr.Length; i++) {
            //第i个player的角色存在CardNorData脚本的同时现在也是该角色的回合
            if (goArr[i].GetComponent<CardNorData>() != null && goArr[i].GetComponent<CardNorData>().isNow) {
                return goArr[i].GetComponent<CardNorData>();
            }
        }
        return null;
    }

    //监听卡牌上的toggle组件的isOn属性，根据isOn的值来控制卡牌动画
    public void SelToggle(bool selected)
    {
        if (toggle.group == null)
            return;
        if (transform.tag == "Weapon" || transform.tag == "Mount" || transform.tag=="Defend") return;
        if (selected)
        {
            Tween tween=gameObject.transform.DOMove(transform.position+new Vector3(0,40,0),0.05f);            
            tween.SetAutoKill(false);
            if (cardNorData != null) {
                if(!cardNorData.outCarding)
                    cardNorData.currentClickCard = this.gameObject;            
                switch (GetCardName(this.gameObject)) {
                    case "攻":
                        cardNorData.singleSelect = true;
                        break;
                }            
            }

        }
        else
        {            
            if (cardNorData != null) {
                bool haveIsonTrue = false;
                for (int i = 0; i < playerCardGo.transform.childCount; i++) {
                    if (playerCardGo.transform.GetChild(i).GetComponent<Toggle>().isOn) {
                        haveIsonTrue = true;
                        break;
                    }
                }
                switch (GetCardName(this.gameObject)) {
                    case "攻": {
                        //如果是通过点击其它牌导致的SelToggle的else调用则进行前后点击牌名判断
                        if (haveIsonTrue) {
                            if (GetCardName(cardNorData.currentClickCard) != GetCardName(cardNorData.preCurrentClickCard)) {
                                //关闭所有选择动画
                                for (int i = 0; i < gameController.nRoleList.Count; i++) {
                                    if (gameController.nRoleList[i].GetComponent<SelectAnimation>().selected == 1) {
                                        gameController.nRoleList[i].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                                        gameController.nRoleList[i].GetComponent<SelectAnimation>().selected = 0;
                                        cardNorData.selectRoles.Remove(gameController.nRoleList[i]);

                                    }
                                }
                            }
                        }
                        else {
                            //关闭所有选择动画
                            for (int i = 0; i < gameController.nRoleList.Count; i++) {
                                if (gameController.nRoleList[i].GetComponent<SelectAnimation>().selected == 1) {
                                    gameController.nRoleList[i].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                                    gameController.nRoleList[i].GetComponent<SelectAnimation>().selected = 0;
                                    cardNorData.selectRoles.Remove(gameController.nRoleList[i]);

                                }
                            }
                        }
                        cardNorData.singleSelect = false;
                        break;
                    }
                }
                cardNorData.preCurrentClickCard = this.gameObject;
            }            
            gameObject.transform.DOPlayBackwards();
        }
    }

    

    public string GetCardName(GameObject card)
    {
        if (card == null) {
            Debug.Log("card is null");
            return "null";
        }
        string cardName = card.name;
        for (byte i = 0; i < cardName.Length; i++) {
            if ((int)cardName[i] >= 48 && (int)cardName[i] <= 57) {
                cardName = cardName.Substring(0, i);
                break;
            }
        }
        return cardName;
    }

}
