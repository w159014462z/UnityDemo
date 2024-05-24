using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;


public class GameController : MonoBehaviour
{
    public bool haveWait;//�Ƿ����˴��ڵȴ�״̬
    private bool gameStart;//������Ϸ��ʼ���ƶ���������ȫˢ���ڵ�ǰPlayerCard��
    private bool gameInitDealCardFinish;//��Ϸ��ʼ���ƽ������
    private bool playerSwitchPlayer;//����л������ʱ��ֹ������UI��ǰ�л��������߼�    
    private TimePhase prePhase;//��ҽ���ȴ�״̬֮ǰ��״̬
    public int timeNums;//��¼��Ϸ�ӿ�ʼ���е��������ܻغ���
    public int nowRoleNum;//��ǰ�غϵĽ�ɫ�ı��
    public int preRoleNum;//��ǰ�ȴ��Ľ�ɫ�ı��
    public int roleCount;//��ɫ�ܸ���
    public int nowCardPileCode;//��ǰ�������������������������ж�Ӧ�ı�ţ���Ϸ�Ӵ�����С������ƣ����ñ���ΪĬ�Ͽ���������
    public int nowPlayerRoleNum;//��ǰUI����ʾ����ұ��
    private float nowTime;//ͳ����һغϿ�ʼʱ���Ѿ��ۼƵ�ʱ��
    private float preNowTime;//�洢��ǰ��ҳ����ƺ���Ҫ������ҳ���ʱnowTime��ֵ
    public float[] maxTime;//ÿ���˻غϿ�ʼ��������ÿ���׶ε�ʱ��
    public Slider slider;//�����������Ϸ����
    public GameObject nowRoleNumGo;//��ʾ��ǰ��ұ�ŵ���Ϸ����
    public GameObject timeNumsGo;//��ʾ��ǰ�ۼƻغ�������Ϸ����
    public GameObject teammateGp;//�����ѡ��ɫ��
    public GameObject enemyGp;//������ѡ��ɫ��
    public GameObject nowGo;//��ǰ�غϵĽ�ɫ����Ϸ����
    public GameObject playerHandCardInfo;//����Լ���ǰ���������/�����������������ʾ����Ϸ����
    public GameObject playerCardGo;//��ȡPlayerCard��Ϸ���壬���Ǵ�ŵ�ǰ��ҵĿ�����Ϸ����ĸ�����
    public GameObject skillButtonGo;//��ȡSkillButton��Ϸ���壬��ż��ܰ�ť
    private GameObject GamePhase;//��Ϸ�غϽ׶���ʾ����Ϸ����ĸ�����
    private GameObject tarPosGo;//ɾ�����ƶ����п����ƶ�������λ��
    
    public CardNorData nowGoCardNorData;//��ǰ�غϽ�ɫ���󶨵�CardNorData�ű��ļ�
    public List<GameObject> allWeaponCard = new List<GameObject>();//�����������б�
    public List<GameObject> allMountCard = new List<GameObject>();//�����������б�
    public List<GameObject> allBaseCard = new List<GameObject>();//���л������б�
    public List<GameObject> allSkillCard = new List<GameObject>();//���м������б�
    public List<GameObject> roleList = new List<GameObject>();//��ʼ��ұ���б�
    public List<GameObject> nRoleList = new List<GameObject>();//��������ұ���б�
    private List<GameObject> initCardPile = new List<GameObject>();//Ĭ�Ͽ���
    private List<GameObject> cardPile = new List<GameObject>();//��Ϸ�еĿ���
    private CardPileController cardPileCtrl=new CardPileController("Normal",120);//����һ������ΪNormal�ĺ����������120�ֿ��ƵĿ���

    private void Awake() {
        timeNums = 1;
        nowRoleNum = 0;
        preRoleNum = 0;
        gameStart = true;
        gameInitDealCardFinish = false;
        playerSwitchPlayer = false;
        //maxTime = new float[5] { 1.5f,1.5f,30f,8f,15f};
        maxTime = new float[5] { 1.5f,1.5f,30f,2f,30f};//����
        nowRoleNumGo = GameObject.Find("NowRoleNum");
        timeNumsGo = GameObject.Find("TimeNums");
        skillButtonGo = GameObject.Find("SkillButton");
        slider = GameObject.Find("PlayerSlider").GetComponent<Slider>();
        teammateGp = GameObject.Find("Teammate");
        enemyGp = GameObject.Find("Enemy");
        playerCardGo = GameObject.Find("PlayerCard");
        tarPosGo = GameObject.Find("TarPos");
        playerHandCardInfo = GameObject.Find("PlayerHandCardInfo");
        GamePhase = GameObject.Find("GamePhase");
        //CardPileInit();
        //nowCardPileCode = cardPileCtrl.cntSum;
        //���������Ϸ��ʼ����������Start���治��Awake������Խ��BuildExe�ļ���һ��ʼ�����Ƶ�һ���������û�а�UIˢ�µ�bug
        GameInit();
    }

    void Start()
    {               
        //ˢ�½�ɫ������ŵ�UI
        for (int i = 0;i < roleCount; i++) {
            CardNorData cardNorDatatmp = nRoleList[i].GetComponent<CardNorData>();
            nRoleList[i].transform.GetChild(1).GetComponent<Text>().text = cardNorDatatmp.roleNum + 1 + "��λ";            
        }
        nRoleList[0].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
        nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum+1).ToString();
        timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"��ǰ�غ�����{timeNums}";
    }

    private void GameInit() {
        CardPileInit();
        nowCardPileCode = cardPileCtrl.cntSum;

        RoleNumInit();
        nowGo = nRoleList[nowRoleNum];
        nowGoCardNorData = nRoleList[0].GetComponent<CardNorData>();
        nowGoCardNorData.isNow= true;
        //for (int i = 0; i < roleCount; i++) {
        //    tarPosGo.transform.GetChild(0).gameObject.SetActive(true);
        //    Vector3 tmp = tarPosGo.transform.GetChild(0).position;
        //    Vector3 target = nRoleList[i].transform.position;
        //    if (nRoleList[i].tag == "Player") {
        //        target = playerCardGo.transform.position;
        //    }
        //    tarPosGo.transform.GetChild(0).DOMove(target, 0.6f).OnComplete(() => {
        //        tarPosGo.transform.GetChild(0).gameObject.SetActive(false);
        //        tarPosGo.transform.GetChild(0).position = tmp;
        //    });
        //    DealCards(nRoleList[i], 4);//����
        //}
   
    #region �����ƶ�������ʱ,����ֱ��
        StartCoroutine(GameInitCoroutine());
    #endregion

        RoleUIInit();
    }

    IEnumerator GameInitCoroutine()
    {
        int ft=0;
        for (ft = 0; ft < roleCount; ft++) {
            if (nRoleList[ft].tag == "Player") {                
                break;
            }
        }

        Vector3 tmp = tarPosGo.transform.GetChild(2).position;        
        for (int i = 0; i < roleCount; i++) {
            tarPosGo.transform.GetChild(2).gameObject.SetActive(true);            
            Vector3 target = nRoleList[i].transform.position;
            if (i == ft) {
                target = playerCardGo.transform.position;
            }
            tarPosGo.transform.GetChild(2).DOMove(target, 0.6f).OnComplete(() => {
                tarPosGo.transform.GetChild(2).gameObject.SetActive(false);
                tarPosGo.transform.GetChild(2).position = tmp;
            });            
            yield return new WaitForSeconds(0.8f);            
            DealCards(nRoleList[i], 4);
            if (i == ft)
                gameStart = false;
        }

        gameInitDealCardFinish = true;
    }

    private void RoleNumInit()
    {
        for (int i = 0; i < teammateGp.transform.childCount; i++) {
            roleList.Add(teammateGp.transform.GetChild(i).gameObject);
        }
        roleCount += teammateGp.transform.childCount;
        for (int i = 0; i < enemyGp.transform.childCount; i++) {
            roleList.Add(enemyGp.transform.GetChild(i).gameObject);
        }
        roleCount += enemyGp.transform.childCount;

        int[] randSort = new int[roleCount];//�������ÿ����ɫ��λ�õ����飬Ŀ���ǿ�����Ϸ�غϽ��е�˳��
        for (int i = 0; i < roleCount; i++) {
            randSort[i] = i;
        }
        Shuffle(ref randSort);

        for (int i = 0; i < roleCount; i++) {
            for (int j = 0; j < roleCount; j++) {
                if (randSort[j] == i) {
                    nRoleList.Add(roleList[j]);
                    break;
                }
            }
        }
        for (int i = 0; i < roleCount; i++) {
            nRoleList[i].transform.GetComponent<CardNorData>().roleNum = i;
        }
    }

    //����һ�������������ֵ��λ��
    private static void Shuffle(ref int[] arr)
    {
        for (int i = 0; i < arr.Length; i++) {
            int idx = UnityEngine.Random.Range(i, arr.Length);
            int tmp = arr[idx];
            arr[idx] = arr[i];
            arr[i] = tmp;
        }
    }

    private void RoleUIInit()
    {
        GameObject nowPlayerIcon = GameObject.FindGameObjectWithTag("PlayerIconUI");
        int ft;//��һ����ҽ�ɫ��roleNum    
        for (ft = 0; ft < roleCount; ft++) {
            if (nRoleList[ft].tag == "Player") {
                nowPlayerIcon.GetComponent<Image>().sprite = nRoleList[ft].transform.GetComponent<Image>().sprite;
                nowPlayerIcon.name = nRoleList[ft].name;
                nowPlayerRoleNum = ft;
                break;
            }
        }

        if (nowGo.tag == "Player")
            //����һ��ˢ�¸����ӵ�еĿ���(���ƺ�װ����)��������
            CardAreaRefresh();
        else {
            //ɾ����ǰ����UI�µ��������ƣ���ʱ����Щ����Ϊ����UI����ʱ����
            for (int i = 0; i < playerCardGo.transform.childCount; i++) {
                Destroy(playerCardGo.transform.GetChild(i).gameObject);
            }

            CardNorData playerCardNorData = nRoleList[ft].transform.GetComponent<CardNorData>();

            //ˢ�¼�����
            for (int i = 0; i < skillButtonGo.transform.childCount; i++) {
                if (skillButtonGo.transform.GetChild(i).name != "TextBG")
                    skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = "��";
            }
            for (int i = 0; i < playerCardNorData.skills.Count; i++) {
                if (i > 2)
                    break;//��ʱֻ����������������������������λ��ֹ��������
                skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = playerCardNorData.skills[i];
            }

            //ˢ�µ�ǰ��ɫ��װ�������ڵĿ�
            GameObject MountGo = GameObject.Find("EquipAreaTotal").transform.GetChild(1).transform.GetChild(0).gameObject;
            GameObject WeaponGo = GameObject.Find("EquipAreaTotal").transform.GetChild(1).transform.GetChild(1).gameObject;
            GameObject DefendGo = GameObject.Find("EquipAreaTotal").transform.GetChild(1).transform.GetChild(2).gameObject;
            if (playerCardNorData.mount == null) {
                MountGo.GetComponent<Image>().sprite = null;
                MountGo.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            }
            else {
                MountGo.GetComponent<Image>().sprite = playerCardNorData.mount;
                MountGo.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            }

            if (playerCardNorData.weapon == null) {
                WeaponGo.GetComponent<Image>().sprite = null;
                WeaponGo.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            }
            else {
                WeaponGo.GetComponent<Image>().sprite = playerCardNorData.weapon;
                WeaponGo.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            }
            
            if (playerCardNorData.defend == null) {
                DefendGo.GetComponent<Image>().sprite = null;
                DefendGo.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            }
            else {
                DefendGo.GetComponent<Image>().sprite = playerCardNorData.defend;
                DefendGo.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            }

            //Debug.Log("test: "+nowGoCardNorData.handCard.Count);//��ʾ��ǰ��ɫ��ӵ�е���������
            for (int i = 0; i < playerCardNorData.handCard.Count; i++) {
                GameObject go = Instantiate(playerCardNorData.handCard[i]);
                //go.GetComponent<CardInfoDisplay>().AddCardInfo();//��������󶨵Ŀ��ƿ�����ı�
                go.transform.SetParent(playerCardGo.transform, false);
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 2) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 3) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 4) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 5) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
            }
        }

    }

    //�ı䵱ǰ��ҿ��ƵĽ�ɫ�Ļغ�ʱ����������ȣ�return true ��ʾʱ�䵽�ˣ�false���ʾʱ��û�е�
    private bool ChangeSlider(byte phase)
    {
        if (nowGo.tag == "Player") {
            slider.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255);
        }
        if (nowGo.tag == "Enemy") {
            slider.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(255, 0, 0);
        }
        nowTime += Time.deltaTime;        
        if (nowTime > maxTime[phase] || (phase == (byte)TimePhase.QuitCard && nowGoCardNorData.handCard.Count <= nowGoCardNorData.nowHp && nowTime >= 0.8f)
            || (phase==(byte)TimePhase.Wait && nowGoCardNorData.needOutCard == null)) {
            nowTime = 0;
            slider.value = 0;
            return true;
        }
        //�غϿ�ʼ�׶κ����ƽ׶ν���������
        if (phase != 0 && phase != 1)
            slider.value = (maxTime[phase] - nowTime) / maxTime[phase];
        return false;
    }

    void Update()
    {
        //��Ϸ��ʼ���ƶ�����û���������к����غϴ���
        if (!gameInitDealCardFinish)
            return;
        //��ʱˢ�µ�ǰ�����ӵ�еĿ����������Ż��ɵ������޸�ʱ���޸���Ŀ��
        if (!playerSwitchPlayer && nowGo != null && nowGo.tag == "Player")
            playerHandCardInfo.GetComponent<Text>().text = $"������ {nowGoCardNorData.handCard.Count}/{nowGoCardNorData.maxHp}";

        //�ȴ��׶� #4��һ�������Լ��غ�ʹ�ý�����Ҫ�Է������������ʱ����
        if (nRoleList[preRoleNum].GetComponent<CardNorData>().phase == (byte)TimePhase.Wait) {
            if (ChangeSlider((byte)TimePhase.Wait)) {
                if (nRoleList[nowRoleNum].GetComponent<CardNorData>().needOutCard != "") {                    
                    bool haveNeedOutCard=false;
                    for (int i = 0; i < nowGoCardNorData.handCard.Count; i++) {
                        if (GetCardName(nowGoCardNorData.handCard[i]) == nowGoCardNorData.needOutCard){
                            if (nowGo.tag == "Player") {
                                for (int j = 0; j < playerCardGo.transform.childCount; j++) {
                                    if (GetCardName1(playerCardGo.transform.GetChild(j).gameObject) == nowGoCardNorData.handCard[i].name) {
                                        nowGoCardNorData.handCard.RemoveAt(i);
                                        playerCardGo.transform.GetChild(j).DOMove(tarPosGo.transform.GetChild(0).position, 0.6f).OnComplete(() => {
                                            Destroy(playerCardGo.transform.GetChild(j).gameObject);
                                        });
                                        break;
                                    }
                                }
                            }
                            else if (nowGo.tag == "Enemy") {
                                Debug.Log("test: "+nowGoCardNorData.handCard[i].name);
                                GameObject card = Instantiate(nowGoCardNorData.handCard[i]);
                                card.transform.SetParent(nowGo.transform, false);
                                card.transform.localPosition=Vector3.zero;
                                card.AddComponent<Canvas>();                                
                                card.GetComponent<Canvas>().overrideSorting = true;
                                card.GetComponent<Canvas>().sortingOrder = 1;
                                card.transform.DOMove(tarPosGo.transform.GetChild(0).position, 0.6f).OnComplete(() => {
                                    Destroy(card);
                                });
                                nowGoCardNorData.DestroyCard(nowGoCardNorData.handCard[i]);
                            }                            
                            haveNeedOutCard = true;
                            break;
                        }                            
                    }
                    if(!haveNeedOutCard) {
                        //������Ҵ�������ж�û�д����Ҫ������ƽ��е�Ч��
                        switch (nRoleList[preRoleNum].GetComponent<CardNorData>().intoWaitCard) {
                            case "��":
                                nowGoCardNorData.nowHp -= 1;
                                break;
                            default:
                                break;
                        }
                        
                    }
                    nowGoCardNorData.needOutCard = "";
                }                
                SwitchRole();
            }                                   
            return;
        }

        //�غϿ�ʼ�׶�#0
        if (nowGoCardNorData.phase == (byte)TimePhase.Begin) {            
            //����ý׶δ�������
            if(nowTime==0) {
                slider.value = 1;
                GamePhase.transform.GetChild(0).gameObject.SetActive(true);          
                //��ǰ��ɫΪ�ɣ��������ұ�ĳ����ʹ�������ɣ�������
                if (nowGoCardNorData.race == 0 && nowGoCardNorData.fairyBound) {
                    //д�ж��������Ƿ���Ч�Ĵ���
                    GamePhase.transform.GetChild(0).gameObject.SetActive(false);
                    GamePhase.transform.GetChild(1).gameObject.SetActive(true) ;

                }
                else if (nowGoCardNorData.race == 1 && nowGoCardNorData.monsterBound) {
                    //д�ж��������Ƿ���Ч�Ĵ���
                    GamePhase.transform.GetChild(0).gameObject.SetActive(false);
                    GamePhase.transform.GetChild(1).gameObject.SetActive(true);

                }
            }
            if (ChangeSlider(0)) {                
                nowGoCardNorData.phase = (byte)TimePhase.DealCard;
                
            }
            return;
        }

        //���ƽ׶�#1
        if (nowGoCardNorData.phase == (byte)TimePhase.DealCard) {
            if(nowTime == 0) {
                slider.value = 1;
                GamePhase.transform.GetChild(0).gameObject.SetActive(false);
                GamePhase.transform.GetChild(1).gameObject.SetActive(false);
                GamePhase.transform.GetChild(2).gameObject.SetActive(true);

                tarPosGo.transform.GetChild(3).gameObject.SetActive(true);
                Vector3 tmp = tarPosGo.transform.GetChild(3).position;
                Vector3 target=nowGo.transform.position;
                if (nowGo.tag == "Player") {
                    target = playerCardGo.transform.position;
                }
                tarPosGo.transform.GetChild(3).DOMove(target, 0.6f).OnComplete(() => {
                    tarPosGo.transform.GetChild(3).gameObject.SetActive(false);
                    tarPosGo.transform.GetChild(3).position = tmp;                    
                });
                DealCards(nowGo, 2);                               
            }
            if (ChangeSlider(1)) {
                nowGoCardNorData.phase = (byte)TimePhase.OutCard;                
            }
            return;
        }

        //���ƽ׶�#2
        if(nowGoCardNorData.phase == (byte)TimePhase.OutCard) {
            if (nowTime == 0) {
                slider.value = 1;
                GamePhase.transform.GetChild(2).gameObject.SetActive(false);
                GamePhase.transform.GetChild(3).gameObject.SetActive(true);
            }
            if (nowGo.tag == "Enemy") {                              
                nowGoCardNorData.phase = (byte)TimePhase.QuitCard;                    
                Debug.Log($"���˵�{nowRoleNum + 1}�ŵ��˽�ɫ�ĳ���,��ֱ������");
                return;                
            }
            if (nowGo.tag == "Player") {
                if (nowGoCardNorData.currentClickCard != null && nowGoCardNorData.currentClickCard.GetComponent<Toggle>().isOn) {                                     
                
                }
            }
            if (ChangeSlider(2)) {
                nowGoCardNorData.phase = (byte)TimePhase.QuitCard;               
            }
            return;
        }

        //���ƽ׶�#3
        if (nowGoCardNorData.phase == (int)TimePhase.QuitCard) {
            if (nowTime == 0) {
                slider.value = 1;
                GamePhase.transform.GetChild(3).gameObject.SetActive(false);
                GamePhase.transform.GetChild(4).gameObject.SetActive(true);            
                
            }
            QuitCard();
        }
    }    

    //����,��go�����ɫ��num����
    private void DealCards(GameObject go,int num) {
        int ft;
        for(ft=0;ft<roleCount;ft++) {
            if (nRoleList[ft].tag == "Player") {
                break;
            }
        }
        CardNorData goCardNorData = go.GetComponent<CardNorData>();
        //���ڿ��ѵ��ƻ�����go��Ϸ���巢num����
        if (num < nowCardPileCode) {
            for (int t = 0; t < num; t++) {
                nowCardPileCode -= 1;
                //����������ʾ����Ϸ�ոտ�ʼ�����ڳ��Ƶ�����ҵĵ�һ������
                goCardNorData.handCard.Add(cardPile[nowCardPileCode]);
                if ((go.tag == "Player" && goCardNorData.roleNum == nowRoleNum) || (gameStart && goCardNorData.roleNum==ft)) {
                    GameObject newCard = Instantiate(cardPile[nowCardPileCode]);
                    newCard.transform.SetParent(playerCardGo.transform, false);                
                }
            }
        }
        //���Ѹպù������������¿���
        else if (num == nowCardPileCode) {
            for (int t = 0; t < num; t++) {
                nowCardPileCode -= 1;
                go.GetComponent<CardNorData>().handCard.Add(cardPile[nowCardPileCode]);
                if (nowGo.tag == "Player") {
                    GameObject newCard = Instantiate(cardPile[nowCardPileCode]);
                    newCard.transform.SetParent(playerCardGo.transform, false);
                }
            }
            RefreshCardPile(3);
            nowCardPileCode = cardPileCtrl.cntSum;
        }
        //���Ѳ���
        else if (num > nowCardPileCode) {
            for (int t = 0; t < nowCardPileCode; t++) {
                go.GetComponent<CardNorData>().handCard.Add(cardPile[t]);
                if (nowGo.tag == "Player") {
                    GameObject newCard = Instantiate(cardPile[nowCardPileCode]);
                    newCard.transform.SetParent(playerCardGo.transform, false);
                }
            }
            num -= nowCardPileCode;
            RefreshCardPile(3);
            nowCardPileCode = cardPileCtrl.cntSum;
            for (int t = 0; t < num; t++) {
                nowCardPileCode -= 1;
                go.GetComponent<CardNorData>().handCard.Add(cardPile[nowCardPileCode]);
                if (nowGo.tag == "Player") {
                    GameObject newCard = Instantiate(cardPile[nowCardPileCode]);
                    newCard.transform.SetParent(playerCardGo.transform, false);
                }
            }
        }
    }

    //����
    private void QuitCard()
    {
        if (nowGo.tag == "Player") {

        }
        if (nowGo.tag == "Enemy") {

        }
        if (ChangeSlider(3)) {
            if (nowGoCardNorData.handCard.Count > nowGoCardNorData.nowHp) {
                int quitCardNum = nowGoCardNorData.handCard.Count - nowGoCardNorData.nowHp;
                int idx = 0;
                if (nowGo.tag == "Player") {
                    for (int i = 0; i < quitCardNum; i++) {
                        idx = UnityEngine.Random.Range(0, playerCardGo.transform.childCount);
                        nowGoCardNorData.DestroyCard(playerCardGo.transform.GetChild(idx).gameObject);
                    }
                    nowGoCardNorData.currentClickCard = null;
                    playerHandCardInfo.GetComponent<Text>().text = $"������ {nowGoCardNorData.handCard.Count}/{nowGoCardNorData.maxHp}";
                }
                if (nowGo.tag == "Enemy") {
                    for (int i = 0; i < quitCardNum; i++) {
                        idx = UnityEngine.Random.Range(0, nowGoCardNorData.handCard.Count);
                        nowGoCardNorData.DestroyCard(nowGoCardNorData.handCard[idx]);
                    }
                }
            }

            SwitchRole();
            GamePhase.transform.GetChild(4).gameObject.SetActive(false);
        }
    }

    private void SwitchRole()
    {
        //�ر�����ѡ�񶯻�
        for (int i = 0; i < nRoleList.Count; i++) {
            if (nRoleList[i].GetComponent<SelectAnimation>().selected == 1) {
                if (nRoleList[i].GetComponent<SelectAnimation>().animator.GetCurrentAnimatorStateInfo(0).IsName("SelectedAnim"))
                    nRoleList[i].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                nRoleList[i].GetComponent<SelectAnimation>().selected = 0;
                nowGoCardNorData.selectRoles.Remove(nRoleList[i]);
            }
        }

        //���˴��ڵȴ�״̬˵�����ڵ��õ�Ϊ�ȴ�״̬ʱ�Ľ�ɫ�л�        
        byte pre = (byte)nowRoleNum;             
        if (haveWait) {            
            nowGoCardNorData.isNow = false;
            nowRoleNum += 1;
            nowRoleNum %= roleCount;
            while (nowRoleNum != preRoleNum) {                            
                if (nRoleList[nowRoleNum].GetComponent<CardNorData>().needOutCard != "") {
                    nowGo = nRoleList[nowRoleNum];
                    nowGoCardNorData = nowGo.GetComponent<CardNorData>();
                    nowGoCardNorData.isNow = true;
                    if (nowGo.tag == "Enemy") {
                        maxTime[(byte)TimePhase.Wait] = 5f;
                    }
                    else if (nowGo.tag == "Player") {
                        maxTime[(byte)TimePhase.Wait] = 15f;
                    }
                    break;
                }
                nowRoleNum += 1;
                nowRoleNum %= roleCount;
            }
            
            //������������Ҫ���Ƶ��������е�ǰ���ڵȴ�״̬������״̬�Ļָ�
            if (nowRoleNum == preRoleNum) {
                nowGo = nRoleList[nowRoleNum];
                nowGoCardNorData = nowGo.GetComponent<CardNorData>();
                nowGoCardNorData.isNow = true;
                haveWait = false;
                nowTime = preNowTime;
                nowGoCardNorData.intoWaitCard = "";
                nowGoCardNorData.phase = (byte)prePhase;
            }
        }
        else {
            nowGoCardNorData.DataInit();
            nowGoCardNorData.isNow = false;
            nowRoleNum += 1;
            nowRoleNum %= roleCount;
            nowGo = nRoleList[nowRoleNum];
            nowGoCardNorData = nowGo.GetComponent<CardNorData>();
            nowGoCardNorData.isNow = true;        
            timeNums++;
        }
        if (nowGo.tag == "Player") {
            #region �Լ���һ�������л�����һ���������ʱЭ�̣�Ŀ����ʹ�������ɸ�����
            if (nRoleList[pre].tag == "Player") {
                StartCoroutine(DelayExecute(pre));
            }
            #endregion
            else {            
                NowPlayerUIRefresh();
                nRoleList[pre].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 255, 255);
                nowGo.transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
                nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum + 1).ToString();
                timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"��ǰ�غ�����{timeNums}";
            }          
        }
        else {
            nRoleList[pre].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 255, 255);
            nowGo.transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
            nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum + 1).ToString();
            timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"��ǰ�غ�����{timeNums}";
        }
    }

    IEnumerator DelayExecute(int pre)
    {
        playerSwitchPlayer = true;
        yield return new WaitForSeconds(1f);
        NowPlayerUIRefresh();
        nRoleList[pre].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 255, 255);
        nowGo.transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
        nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum + 1).ToString();
        timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"��ǰ�غ�����{timeNums}";
        playerSwitchPlayer = false;
    }   
    
    private void NowPlayerUIRefresh() {
        nowPlayerRoleNum = nowGoCardNorData.roleNum;
        GameObject.FindGameObjectWithTag("PlayerIconUI").GetComponent<Image>().sprite = nowGo.GetComponent<Image>().sprite;
        GameObject.FindGameObjectWithTag("PlayerIconUI").name = nowGo.name;
        CardAreaRefresh();        
        HpUIChange();
    }

    //ˢ������ӽ��µ�UI����
    private void CardAreaRefresh() {
        //ɾ����ǰ����UI�µ���������
        for (int i = 0; i < playerCardGo.transform.childCount; i++){
            Destroy(playerCardGo.transform.GetChild(i).gameObject);
        }

        //ˢ�¼�����
        for (int i = 0; i < skillButtonGo.transform.childCount; i++) {
            if(skillButtonGo.transform.GetChild(i).name!="TextBG")
                skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = "��";
        }
        for (int i = 0; i < nowGoCardNorData.skills.Count; i++) {
            if(i>2) break;//��ʱֻ����������������������������λ��ֹ��������
            skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = nowGoCardNorData.skills[i];
        }

        //ˢ�µ�ǰ���װ���������
        EquipAreaRefresh();

        //Debug.Log("test: "+nowGoCardNorData.handCard.Count);//��ʾ��ǰ��ɫ��ӵ�е���������
        for (int i = 0; i < nowGoCardNorData.handCard.Count; i++){
            GameObject go = Instantiate(nowGoCardNorData.handCard[i]);
            //go.GetComponent<CardInfoDisplay>().AddCardInfo();//��������󶨵Ŀ��ƿ�����ı�
            go.transform.SetParent(playerCardGo.transform, false);
        }

        if (nowGo.GetComponent<CardNorData>().maxHp == 2) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
        }
        if (nowGo.GetComponent<CardNorData>().maxHp == 3) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
        }
        if (nowGo.GetComponent<CardNorData>().maxHp == 4) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
        }
        if (nowGo.GetComponent<CardNorData>().maxHp == 5) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("��Ѫ");
        }
    }

    public void EquipAreaRefresh()
    {
        //ˢ�µ�ǰ��ɫ��װ�������ڵĿ�
        GameObject MountGo = GameObject.Find("EquipAreaTotal").transform.GetChild(1).transform.GetChild(0).gameObject;
        GameObject WeaponGo = GameObject.Find("EquipAreaTotal").transform.GetChild(1).transform.GetChild(1).gameObject;
        GameObject DefendGo = GameObject.Find("EquipAreaTotal").transform.GetChild(1).transform.GetChild(2).gameObject;
        if (nowGoCardNorData.mount == null) {
            MountGo.GetComponent<Image>().sprite = null;
            MountGo.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
        else {
            MountGo.GetComponent<Image>().sprite = nowGoCardNorData.mount;
            MountGo.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }

        if (nowGoCardNorData.weapon == null) {
            WeaponGo.GetComponent<Image>().sprite = null;
            WeaponGo.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
        else {
            WeaponGo.GetComponent<Image>().sprite = nowGoCardNorData.weapon;
            WeaponGo.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }

        if (nowGoCardNorData.defend == null) {
            DefendGo.GetComponent<Image>().sprite = null;
            DefendGo.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
        else {
            DefendGo.GetComponent<Image>().sprite = nowGoCardNorData.defend;
            DefendGo.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
    }   

    public void HpUIChange()
    {        
        int nowHp = nowGoCardNorData.nowHp;
        int maxHp = nowGoCardNorData.maxHp;
        HpChanged hpChanged=nowGo.GetComponent<HpChanged>();        
        if (nowHp <= 0)
            nowHp = 0;
        if (maxHp == 2) {
            hpChanged.nowPlayerIcon.transform.localPosition = hpChanged.iconPos[2, nowHp];            
            if (nowHp == 0) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(1).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if (maxHp == 3) {
            hpChanged.nowPlayerIcon.transform.localPosition = hpChanged.iconPos[3, nowHp];            
            if (nowHp == 0) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(1).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (nowHp == 3) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if (maxHp == 4) {
            hpChanged.nowPlayerIcon.transform.localPosition = hpChanged.iconPos[4, nowHp];
            if (nowHp == 0) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(1).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (nowHp == 3) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (nowHp == 4) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if (maxHp == 5) {

            hpChanged.nowPlayerIcon.transform.localPosition = hpChanged.iconPos[5, nowHp];
            
            if (nowHp == 0) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(1).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(3).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(3).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 3) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(3).gameObject.SetActive(true);
                hpChanged.bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 4) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                hpChanged.bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 5) {
                for (int i = 0; i < hpChanged.bloodMask.transform.childCount; i++)
                    hpChanged.bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }


#region ���Ѵ���
    //���ѳ�ʼ��
    private void CardPileInit() {        
        CardIntoPile(1, "��", "������",22, 11, 22, 13);
        CardIntoPile(2, "��", "������", 6, 13, 5, 12);
        CardIntoPile(3, "���", "������", 2, 6, 3, 7);
        CardIntoPile(4, "�������", "������", 0, 1, 1, 0);
        CardIntoPile(5, "�������", "������", 1, 0, 0, 0);
        CardIntoPile(6, "�Ͻ��«", "������", 1, 2, 2, 2);
        CardIntoPile(7, "������", "������", 0, 1, 0, 0);
        CardIntoPile(8, "����", "������", 2, 2, 0, 0);
        CardIntoPile(9, "��������", "������", 0, 0, 2, 2);
        CardIntoPile(10, "�����", "������", 1, 3, 3, 2);
        CardIntoPile(11, "����ɽ", "������", 2, 0, 0, 0);
        CardIntoPile(12, "������", "������", 1, 0, 0, 0);
        CardIntoPile(13, "��������", "������", 0, 0, 3, 3);
        CardIntoPile(14, "������ë", "������", 2, 0, 2, 2);
        CardIntoPile(15, "��Ҵ��", "������", 0, 1, 0, 0);
        CardIntoPile(16, "������", "������", 0, 2, 0, 1);
        CardIntoPile(17, "������", "������", 1, 2, 0, 1);
        CardIntoPile(18, "����", "������", 0, 0, 0, 2);
        CardIntoPile(19, "�Ž���", "������", 0, 0, 2, 1);
        CardIntoPile(20, "����", "������", 0, 0, 1, 0);
        CardIntoPile(21, "����", "������", 0, 0, 1, 0);
        CardIntoPile(22, "С��С��", "������", 1, 0, 0, 1);        
        CardIntoPile(23, "͵�컻��", "������", 0, 0, 1, 1);
        CardIntoPile(24, "�������", "������", 1, 3, 0, 0);
        CardIntoPile(25, "���Ǳ���", "������", 0, 0, 0, 1);
        CardIntoPile(26, "���Ȧ", "������", 0, 0, 0, 1);
        CardIntoPile(27, "�Ͻ�����", "������", 0, 0, 1, 0);
        CardIntoPile(28, "�Ű껨����", "������", 0, 0, 0, 1);
        CardIntoPile(29, "���ɻ��ǹ", "������", 0, 1, 0, 0);
        CardIntoPile(30, "�ųݶ�����", "������", 0, 1, 0,0);
        CardIntoPile(31, "��������", "������", 0, 0, 1, 0);
        CardIntoPile(32, "�̶�������", "������", 0, 0, 1, 0);
        CardIntoPile(33, "��ӧǹ", "������", 1, 0, 0, 0);
        CardIntoPile(34, "�𹿰�", "������", 1, 0, 0, 0);
        CardIntoPile(35, "����", "������", 0, 0, 0, 1);
        CardIntoPile(36, "ʨ��", "������", 1, 0, 0, 0);
        CardIntoPile(37, "�ɺ�", "������", 0, 1, 0, 0);
        CardIntoPile(38, "���", "������", 0, 0, 1, 0);
        CardIntoPile(39, "���", "������", 1, 0, 0, 0);
        CardIntoPile(40, "��ˮ����", "������", 1, 0, 0, 0);
        CardIntoPile(41, "�����", "������", 0, 1, 0, 0);
        CardIntoPile(42, "������", "������", 0, 0, 0, 1);
        CardIntoPile(43, "���������", "������", 0, 1, 0, 0);
        
        //�����Ϸ�������cardPileModeͬ��initCardPile
        for (int i = 1; i <= cardPileCtrl.kindCount; i++) {
            if (cardPileCtrl.kind[i] == "������") {
                for (int j = 1; j <= 4; j++) {
                    if (cardPileCtrl.count[i,j] == 0) 
                        continue;
                    for (int k = 0; k < allBaseCard.Count; k++) {
                        if (allBaseCard[k].name == cardPileCtrl.cardsName[i] + j.ToString()) {
                            for (int t = 0; t < cardPileCtrl.count[i, j]; t++) {
                                initCardPile.Add(allBaseCard[k]);
                            }
                            break;
                        }
                    }
                }
            } else if (cardPileCtrl.kind[i] == "������") {
                for (int j = 1; j <= 4; j++) {
                    if (cardPileCtrl.count[i, j] == 0)
                        continue;
                    for (int k = 0; k < allSkillCard.Count; k++) {
                        if (allSkillCard[k].name == cardPileCtrl.cardsName[i] + j.ToString()) {
                            for (int t = 0; t < cardPileCtrl.count[i, j]; t++) {
                                initCardPile.Add(allSkillCard[k]);
                            }
                            break;
                        }
                    }
                }
            } else if (cardPileCtrl.kind[i] == "������") {
                for (int j = 1; j <= 4; j++) {
                    if (cardPileCtrl.count[i, j] == 0)
                        continue;
                    for (int k = 0; k < allWeaponCard.Count; k++) {
                        if (allWeaponCard[k].name == cardPileCtrl.cardsName[i] + j.ToString()) {
                            for (int t = 0; t < cardPileCtrl.count[i, j]; t++) {
                                initCardPile.Add(allWeaponCard[k]);
                            }
                            break;
                        }
                    }
                }
            } else if (cardPileCtrl.kind[i] == "������"){
                for (int j = 1; j <= 4; j++) {
                    if (cardPileCtrl.count[i, j] == 0)
                        continue;
                    for (int k = 0; k < allMountCard.Count; k++) {
                        if (allMountCard[k].name == cardPileCtrl.cardsName[i] + j.ToString()) {
                            for (int t = 0; t < cardPileCtrl.count[i, j]; t++) {
                                initCardPile.Add(allMountCard[k]);
                            }
                            break;
                        }
                    }
                }
            }
        }
        cardPile.AddRange(initCardPile);
        RefreshCardPile(3);
    }

    //���ƶѽ������ϴ��t��
    private void RefreshCardPile(int t)
    {
        for (int i = 0; i < t; i++) {
            for(int j=0;j<cardPile.Count;j++) {
                GameObject tmp = cardPile[j];                
                int randomIdx=UnityEngine.Random.Range(0,cardPile.Count);
                cardPile[j] = cardPile[randomIdx];
                cardPile[randomIdx] = tmp;
            }
        }   
    }

    //���º�������Ӧ���ƴ���cardPileMode�У����������ֱ�Ϊ���Ʊ�ţ����������������ͣ��ÿ��ƺ��ҡ����ġ�÷������Ƭ�������͵�����
    private void CardIntoPile(int id, string name,string kd, int num1, int num2, int num3, int num4)
    {
        cardPileCtrl.cardsName[id] = name;
        cardPileCtrl.kind[id] = kd;
        cardPileCtrl.count[id, 1] = num1;
        cardPileCtrl.count[id, 2] = num2;
        cardPileCtrl.count[id, 3] = num3;
        cardPileCtrl.count[id, 4] = num4;
        cardPileCtrl.cntSum +=  num1 + num2 + num3 + num4;
        cardPileCtrl.kindCount++;
    }
#endregion

    //ȷ�ϰ�ť�󶨵��¼�
    public void EnterButton()
    {
        Debug.Log("���ڵ�����EnterButton������ ");
        //����ڱ��˵ĵȴ��׶δ����Ҫ������ƵĴ���
        if (nowGo.tag == "Player" && nowGoCardNorData.needOutCard != "" && GetCardName(nowGoCardNorData.currentClickCard) == nowGoCardNorData.needOutCard
            && nowGoCardNorData.currentClickCard.GetComponent<Toggle>().isOn) {
            Debug.Log(nowGo.name + "��"+nRoleList[preRoleNum].name+"�ĵȴ��׶δ����" + nowGoCardNorData.currentClickCard.name);
            nowGoCardNorData.currentClickCard.AddComponent<Canvas>();
            nowGoCardNorData.currentClickCard.GetComponent<Canvas>().overrideSorting = true;
            nowGoCardNorData.currentClickCard.GetComponent<Canvas>().sortingOrder = 1;
            //�洢��ǰneedOutCard�ǿյ������cardNorData����Ϊ�ڶ����������ˢ��nowCardNorData��
            CardNorData tmpCardNorData= nowGoCardNorData;
            nowGoCardNorData.currentClickCard.transform.DOMove(tarPosGo.transform.GetChild(0).position, 0.6f).OnComplete(() =>
            {
                tmpCardNorData.DestroyCard(tmpCardNorData.currentClickCard);
            });
            nowGoCardNorData.needOutCard = "";
            nowTime = 0;
            SwitchRole();
            return;
        }        
        //�����������ҽ�ɫ�Ļغϵĳ��ƽ׶���ѡ�������Ʋ�������������ƶ�������ɾ��������                
        if (nowGoCardNorData.phase==(byte)TimePhase.OutCard) {
            if (nowGo.tag == "Player" && nowGoCardNorData.currentClickCard != null && nowGoCardNorData.currentClickCard.GetComponent<Toggle>().isOn) {                
                string cardName = GetCardName(nowGoCardNorData.currentClickCard);                
                if (CanOut(cardName)) {
                    Debug.Log(nowGo.name + "�����" + cardName);
                    nowGoCardNorData.outCarding = true;
                    nowGoCardNorData.currentClickCard.AddComponent<Canvas>();
                    nowGoCardNorData.currentClickCard.GetComponent<Canvas>().overrideSorting = true;
                    nowGoCardNorData.currentClickCard.GetComponent<Canvas>().sortingOrder = 1;
                    nowGoCardNorData.currentClickCard.GetComponent<Toggle>().group = GameObject.Find("BG").GetComponent<ToggleGroup>();
                    Sprite sprite = nowGoCardNorData.currentClickCard.GetComponent<Image>().sprite;
                    nowGoCardNorData.currentClickCard.transform.DOMove(tarPosGo.transform.GetChild(0).position, 0.6f).OnComplete(() =>
                    {
                        nowGoCardNorData.DestroyCard(nowGoCardNorData.currentClickCard);
                        nowGoCardNorData.outCarding = false;
                        switch (cardName) {
                            #region ������
                            case "��":
                                nowGoCardNorData.selectRoles[0].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                                nowGoCardNorData.singleSelect = false;
                                nowGoCardNorData.atkNum++;
                                nowGoCardNorData.selectRoles[0].GetComponent<CardNorData>().needOutCard = "��";
                                //����ȴ��׶ε��йش���
                                prePhase = (TimePhase)nowGoCardNorData.phase;
                                preNowTime = nowTime;
                                nowTime = 0;
                                preRoleNum = nowRoleNum;
                                nowGoCardNorData.intoWaitCard = "��";
                                haveWait = true;
                                nowGoCardNorData.phase = (byte)TimePhase.Wait;
                                SwitchRole();
                                break;
                            case "���":
                                nowGoCardNorData.nowHp += 1;
                                break;
                            #endregion

                            #region ������

                            #endregion 

                            #region ������
                            case "���Ǳ���":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "���Ȧ":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�Ͻ�����":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�Ű껨����":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�ųݶ�����":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "��������":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�̶�������":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "��ӧǹ":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�𹿰�":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            #endregion

                            #region ������
                            case "����":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "ʨ��":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�ɺ�":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "���":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "���":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "��ˮ����":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "�����":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "������":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "���������":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                             #endregion
                        }
                    });                    
                }
                else {
                    Debug.Log(cardName + "�����ò�����");
                }
            }
        }

        //��ǰ�������ƽ׶�
        if (nowGoCardNorData.phase == (byte)TimePhase.QuitCard) {
            if (nowGoCardNorData.handCard.Count <= nowGoCardNorData.nowHp)
                return;
            if (nowGo.tag == "Player" && nowGoCardNorData.currentClickCard != null && nowGoCardNorData.currentClickCard.GetComponent<Toggle>().isOn) {
                string cardName = GetCardName(nowGoCardNorData.currentClickCard);                
                nowGoCardNorData.currentClickCard.AddComponent<Canvas>();
                nowGoCardNorData.currentClickCard.GetComponent<Canvas>().overrideSorting = true;
                nowGoCardNorData.currentClickCard.GetComponent<Canvas>().sortingOrder = 1;
                nowGoCardNorData.currentClickCard.transform.DOMove(tarPosGo.transform.GetChild(0).position, 0.6f).OnComplete(() =>
                {                    
                    nowGoCardNorData.DestroyCard(nowGoCardNorData.currentClickCard);
                });
            }
        }
    }

    //�õ�ȥ�������ֵĿ�����
    public string GetCardName(GameObject card)
    {
        string cardName = card.name;
        for (byte i = 0; i < cardName.Length; i++) {
            if ((int)cardName[i] >= 48 && (int)cardName[i] <= 57) {
                cardName = cardName.Substring(0, i);
                break;
            }
        }
        return cardName; 
    }

    public string GetCardName1(GameObject card)
    {
        if (card.name.IndexOf('(') == -1)
            return card.name;
        string cardName = card.name;
        for (int i = 0; i < cardName.Length; i++) {
            if (cardName[i] == '(') {
                cardName = cardName.Substring(0, i);
                break;
            }
        }
        return cardName;
    }

    //�ж����ڵ�ǰ����ܷ��������cardName������
    private bool CanOut(string cardName)
    {
        if (cardName == "��") {
            if (nowGoCardNorData.selectRoles.Count == 0)
                return false;
            if (nowGoCardNorData.atkNum == 0)
                return true;
            if (nowGoCardNorData.atkNum > 0) {
                if (HaveEquip("���Ǳ���"))
                    return true;
                if (nowGo.name == "ţħ��")
                    return true;
                return false;
            }
        }
        if (cardName == "���") {
            if (nowGoCardNorData.nowHp == nowGoCardNorData.maxHp)
                return false;
            return true;
        }
        if (cardName == "��") {
            return false;
        }
        return true;
    }

    //����ǰ�����ǲ���û�и������ֵ�װ��(���������,������򷵻�true��û���򷵻�false
    private bool HaveEquip(string cardName)
    {
        string[] cardNames = new string[5] { " ", cardName + "1", cardName + "2", cardName + "3", cardName + "4" };
        //��������
        for (byte i = 1; i <= 4; i++) {
            if (nowGoCardNorData.weapon != null && nowGoCardNorData.weapon.name == cardNames[i]) {
                return true;
            }
        }

        //��������
        for (byte i = 1; i <= 4; i++) {
            if (nowGoCardNorData.mount != null && nowGoCardNorData.mount.name == cardNames[i]) {
                return true;
            }
        }

        return false;
    }

    //ȡ����ť�󶨵��¼�
    public void QuitButton()
    {
        Debug.Log("���ڵ�����QuitButton������ ");
        for (byte i = 0; i < playerCardGo.transform.childCount; i++) {
            if (playerCardGo.transform.GetChild(i).GetComponent<Toggle>().isOn) {
                playerCardGo.transform.GetChild(i).GetComponent<Toggle>().isOn = false;

            }
        }
            
    }

    //�غϽ�����ť�󶨵��¼�
    public void TimeEndButton()
    {
        Debug.Log("���ڵ�����TimeEndButton������ ");
        if(nowGoCardNorData.phase == (byte)TimePhase.Begin && nowGoCardNorData.phase == (byte)TimePhase.DealCard) {
            return;
        }
        if (nowGo.tag == "Player" && nowGoCardNorData.phase != (byte)TimePhase.QuitCard && gameInitDealCardFinish) {
            if (haveWait) {
                nowTime = maxTime[(byte)TimePhase.Wait];                
                return;
            }
            nowTime = 0;
            nowGoCardNorData.phase = (byte)TimePhase.QuitCard;
        }
    }
}

//���ѿ����࣬�������ɿ��ѵ����࣬�������������֣�������ɫ����������
public class CardPileController
{
    public int cntSum;
    public int kindCount;
    public string pileName;
    public string [] cardsName;
    public string [] kind;
    public int[,] count;
    public CardPileController(string name,int maxCardKind)
    {
        pileName = name;
        kindCount = 0;
        cardsName = new string[maxCardKind+1];
        kind = new string[maxCardKind+1];
        count = new int[maxCardKind + 1,5];
        cntSum = 0; 
    }

}