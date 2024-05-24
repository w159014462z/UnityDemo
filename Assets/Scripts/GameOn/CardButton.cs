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

    //ͨ����ǩ�ҵ���Ӧ��ǩ������������غϵ���Ϸ�����CardNorData�ű��ļ�
    public CardNorData FindNowPlayerCardNorDataByGoTag(string tag)
    {
        GameObject[] goArr = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < goArr.Length; i++) {
            //��i��player�Ľ�ɫ����CardNorData�ű���ͬʱ����Ҳ�Ǹý�ɫ�Ļغ�
            if (goArr[i].GetComponent<CardNorData>() != null && goArr[i].GetComponent<CardNorData>().isNow) {
                return goArr[i].GetComponent<CardNorData>();
            }
        }
        return null;
    }

    //���������ϵ�toggle�����isOn���ԣ�����isOn��ֵ�����ƿ��ƶ���
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
                    case "��":
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
                    case "��": {
                        //�����ͨ����������Ƶ��µ�SelToggle��else���������ǰ���������ж�
                        if (haveIsonTrue) {
                            if (GetCardName(cardNorData.currentClickCard) != GetCardName(cardNorData.preCurrentClickCard)) {
                                //�ر�����ѡ�񶯻�
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
                            //�ر�����ѡ�񶯻�
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
