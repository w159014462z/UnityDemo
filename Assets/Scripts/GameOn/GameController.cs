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
    public bool haveWait;//是否有人处于等待状态
    private bool gameStart;//控制游戏开始发牌动画不至于全刷新在当前PlayerCard上
    private bool gameInitDealCardFinish;//游戏初始抽牌结束与否
    private bool playerSwitchPlayer;//玩家切换到玩家时防止卡牌数UI提前切换不符合逻辑    
    private TimePhase prePhase;//玩家进入等待状态之前的状态
    public int timeNums;//记录游戏从开始运行到结束的总回合数
    public int nowRoleNum;//当前回合的角色的编号
    public int preRoleNum;//当前等待的角色的编号
    public int roleCount;//角色总个数
    public int nowCardPileCode;//当前卡堆最上面那张在整个卡堆中对应的编号，游戏从大编号往小编号摸牌，即该变量为默认卡堆总张数
    public int nowPlayerRoleNum;//当前UI中显示的玩家编号
    private float nowTime;//统计玩家回合开始时的已经累计的时间
    private float preNowTime;//存储当前玩家出完牌后需要其他玩家出牌时nowTime的值
    public float[] maxTime;//每个人回合开始到结束中每个阶段的时间
    public Slider slider;//进度条组件游戏物体
    public GameObject nowRoleNumGo;//显示当前玩家编号的游戏物体
    public GameObject timeNumsGo;//显示当前累计回合数的游戏物体
    public GameObject teammateGp;//玩家所选角色组
    public GameObject enemyGp;//敌人所选角色组
    public GameObject nowGo;//当前回合的角色的游戏物体
    public GameObject playerHandCardInfo;//存放自己当前人物的手牌/最大容纳手牌数据显示的游戏物体
    public GameObject playerCardGo;//获取PlayerCard游戏物体，这是存放当前玩家的卡牌游戏物体的父物体
    public GameObject skillButtonGo;//获取SkillButton游戏物体，存放技能按钮
    private GameObject GamePhase;//游戏回合阶段显示的游戏物体的父物体
    private GameObject tarPosGo;//删除卡牌动画中卡牌移动的最终位置
    
    public CardNorData nowGoCardNorData;//当前回合角色所绑定的CardNorData脚本文件
    public List<GameObject> allWeaponCard = new List<GameObject>();//所有武器牌列表
    public List<GameObject> allMountCard = new List<GameObject>();//所有坐骑牌列表
    public List<GameObject> allBaseCard = new List<GameObject>();//所有基础牌列表
    public List<GameObject> allSkillCard = new List<GameObject>();//所有技能牌列表
    public List<GameObject> roleList = new List<GameObject>();//初始玩家编号列表
    public List<GameObject> nRoleList = new List<GameObject>();//随机后的玩家编号列表
    private List<GameObject> initCardPile = new List<GameObject>();//默认卡堆
    private List<GameObject> cardPile = new List<GameObject>();//游戏中的卡堆
    private CardPileController cardPileCtrl=new CardPileController("Normal",120);//定义一个名字为Normal的含有最多容纳120种卡牌的卡堆

    private void Awake() {
        timeNums = 1;
        nowRoleNum = 0;
        preRoleNum = 0;
        gameStart = true;
        gameInitDealCardFinish = false;
        playerSwitchPlayer = false;
        //maxTime = new float[5] { 1.5f,1.5f,30f,8f,15f};
        maxTime = new float[5] { 1.5f,1.5f,30f,2f,30f};//测试
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
        //下面这个游戏初始化函数个放Start里面不放Awake里面可以解决BuildExe文件后，一开始发完牌第一个玩家人物没有把UI刷新的bug
        GameInit();
    }

    void Start()
    {               
        //刷新角色所属编号的UI
        for (int i = 0;i < roleCount; i++) {
            CardNorData cardNorDatatmp = nRoleList[i].GetComponent<CardNorData>();
            nRoleList[i].transform.GetChild(1).GetComponent<Text>().text = cardNorDatatmp.roleNum + 1 + "号位";            
        }
        nRoleList[0].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
        nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum+1).ToString();
        timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"当前回合数：{timeNums}";
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
        //    DealCards(nRoleList[i], 4);//发牌
        //}
   
    #region 给发牌动画做延时,符合直观
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

        int[] randSort = new int[roleCount];//用来随机每个角色的位置的数组，目的是控制游戏回合进行的顺序
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

    //打乱一个给定的数组的值的位置
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
        int ft;//第一个玩家角色的roleNum    
        for (ft = 0; ft < roleCount; ft++) {
            if (nRoleList[ft].tag == "Player") {
                nowPlayerIcon.GetComponent<Image>().sprite = nRoleList[ft].transform.GetComponent<Image>().sprite;
                nowPlayerIcon.name = nRoleList[ft].name;
                nowPlayerRoleNum = ft;
                break;
            }
        }

        if (nowGo.tag == "Player")
            //下面一段刷新该玩家拥有的卡牌(手牌和装备牌)到场景中
            CardAreaRefresh();
        else {
            //删除当前人物UI下的所有手牌，此时的这些卡牌为测试UI布局时加入
            for (int i = 0; i < playerCardGo.transform.childCount; i++) {
                Destroy(playerCardGo.transform.GetChild(i).gameObject);
            }

            CardNorData playerCardNorData = nRoleList[ft].transform.GetComponent<CardNorData>();

            //刷新技能栏
            for (int i = 0; i < skillButtonGo.transform.childCount; i++) {
                if (skillButtonGo.transform.GetChild(i).name != "TextBG")
                    skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = "空";
            }
            for (int i = 0; i < playerCardNorData.skills.Count; i++) {
                if (i > 2)
                    break;//暂时只设置了三个技能栏，这里用来限位防止后续报错
                skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = playerCardNorData.skills[i];
            }

            //刷新当前角色的装备区域内的卡
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

            //Debug.Log("test: "+nowGoCardNorData.handCard.Count);//显示当前角色所拥有的手牌张数
            for (int i = 0; i < playerCardNorData.handCard.Count; i++) {
                GameObject go = Instantiate(playerCardNorData.handCard[i]);
                //go.GetComponent<CardInfoDisplay>().AddCardInfo();//给该人物绑定的卡牌库添加文本
                go.transform.SetParent(playerCardGo.transform, false);
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 2) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("二血");
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 3) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("三血");
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 4) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("四血");
            }
            if (nRoleList[ft].GetComponent<CardNorData>().maxHp == 5) {
                GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("五血");
            }
        }

    }

    //改变当前玩家控制的角色的回合时间进度条长度，return true 表示时间到了，false则表示时间没有到
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
        //回合开始阶段和摸牌阶段进度条不动
        if (phase != 0 && phase != 1)
            slider.value = (maxTime[phase] - nowTime) / maxTime[phase];
        return false;
    }

    void Update()
    {
        //游戏开始发牌动画还没结束不进行后续回合处理。
        if (!gameInitDealCardFinish)
            return;
        //随时刷新当前玩家所拥有的卡牌数，可优化成当卡牌修改时才修改数目。
        if (!playerSwitchPlayer && nowGo != null && nowGo.tag == "Player")
            playerHandCardInfo.GetComponent<Text>().text = $"手牌数 {nowGoCardNorData.handCard.Count}/{nowGoCardNorData.maxHp}";

        //等待阶段 #4，一般是在自己回合使用进入需要对方出牌情况的牌时进入
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
                        //根据玩家打出的牌判断没有打出需要打出的牌进行的效果
                        switch (nRoleList[preRoleNum].GetComponent<CardNorData>().intoWaitCard) {
                            case "攻":
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

        //回合开始阶段#0
        if (nowGoCardNorData.phase == (byte)TimePhase.Begin) {            
            //进入该阶段触发代码
            if(nowTime==0) {
                slider.value = 1;
                GamePhase.transform.GetChild(0).gameObject.SetActive(true);          
                //当前角色为仙（妖）族且被某别人使用了捆仙（妖）索
                if (nowGoCardNorData.race == 0 && nowGoCardNorData.fairyBound) {
                    //写判定捆仙索是否生效的代码
                    GamePhase.transform.GetChild(0).gameObject.SetActive(false);
                    GamePhase.transform.GetChild(1).gameObject.SetActive(true) ;

                }
                else if (nowGoCardNorData.race == 1 && nowGoCardNorData.monsterBound) {
                    //写判定捆妖索是否生效的代码
                    GamePhase.transform.GetChild(0).gameObject.SetActive(false);
                    GamePhase.transform.GetChild(1).gameObject.SetActive(true);

                }
            }
            if (ChangeSlider(0)) {                
                nowGoCardNorData.phase = (byte)TimePhase.DealCard;
                
            }
            return;
        }

        //摸牌阶段#1
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

        //出牌阶段#2
        if(nowGoCardNorData.phase == (byte)TimePhase.OutCard) {
            if (nowTime == 0) {
                slider.value = 1;
                GamePhase.transform.GetChild(2).gameObject.SetActive(false);
                GamePhase.transform.GetChild(3).gameObject.SetActive(true);
            }
            if (nowGo.tag == "Enemy") {                              
                nowGoCardNorData.phase = (byte)TimePhase.QuitCard;                    
                Debug.Log($"到了第{nowRoleNum + 1}号敌人角色的出牌,先直接跳过");
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

        //弃牌阶段#3
        if (nowGoCardNorData.phase == (int)TimePhase.QuitCard) {
            if (nowTime == 0) {
                slider.value = 1;
                GamePhase.transform.GetChild(3).gameObject.SetActive(false);
                GamePhase.transform.GetChild(4).gameObject.SetActive(true);            
                
            }
            QuitCard();
        }
    }    

    //发牌,给go这个角色发num张牌
    private void DealCards(GameObject go,int num) {
        int ft;
        for(ft=0;ft<roleCount;ft++) {
            if (nRoleList[ft].tag == "Player") {
                break;
            }
        }
        CardNorData goCardNorData = go.GetComponent<CardNorData>();
        //现在卡堆的牌还够给go游戏物体发num张牌
        if (num < nowCardPileCode) {
            for (int t = 0; t < num; t++) {
                nowCardPileCode -= 1;
                //或后面的语句表示在游戏刚刚开始后现在抽牌的是玩家的第一个人物
                goCardNorData.handCard.Add(cardPile[nowCardPileCode]);
                if ((go.tag == "Player" && goCardNorData.roleNum == nowRoleNum) || (gameStart && goCardNorData.roleNum==ft)) {
                    GameObject newCard = Instantiate(cardPile[nowCardPileCode]);
                    newCard.transform.SetParent(playerCardGo.transform, false);                
                }
            }
        }
        //卡堆刚好够，抽完生成新卡堆
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
        //卡堆不够
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

    //弃牌
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
                    playerHandCardInfo.GetComponent<Text>().text = $"手牌数 {nowGoCardNorData.handCard.Count}/{nowGoCardNorData.maxHp}";
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
        //关闭所有选择动画
        for (int i = 0; i < nRoleList.Count; i++) {
            if (nRoleList[i].GetComponent<SelectAnimation>().selected == 1) {
                if (nRoleList[i].GetComponent<SelectAnimation>().animator.GetCurrentAnimatorStateInfo(0).IsName("SelectedAnim"))
                    nRoleList[i].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                nRoleList[i].GetComponent<SelectAnimation>().selected = 0;
                nowGoCardNorData.selectRoles.Remove(nRoleList[i]);
            }
        }

        //有人处于等待状态说明现在调用的为等待状态时的角色切换        
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
            
            //处理完其他需要出牌的人物后进行当前处于等待状态的人物状态的恢复
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
            #region 自己的一个人物切换到另一个人物的延时协程，目的是使动画过渡更流畅
            if (nRoleList[pre].tag == "Player") {
                StartCoroutine(DelayExecute(pre));
            }
            #endregion
            else {            
                NowPlayerUIRefresh();
                nRoleList[pre].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 255, 255);
                nowGo.transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
                nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum + 1).ToString();
                timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"当前回合数：{timeNums}";
            }          
        }
        else {
            nRoleList[pre].transform.GetChild(1).GetComponent<Text>().color = new Color(255, 255, 255);
            nowGo.transform.GetChild(1).GetComponent<Text>().color = new Color(255, 0, 0);
            nowRoleNumGo.transform.GetChild(1).GetComponent<Text>().text = (nowRoleNum + 1).ToString();
            timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"当前回合数：{timeNums}";
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
        timeNumsGo.transform.GetChild(0).GetComponent<Text>().text = $"当前回合数：{timeNums}";
        playerSwitchPlayer = false;
    }   
    
    private void NowPlayerUIRefresh() {
        nowPlayerRoleNum = nowGoCardNorData.roleNum;
        GameObject.FindGameObjectWithTag("PlayerIconUI").GetComponent<Image>().sprite = nowGo.GetComponent<Image>().sprite;
        GameObject.FindGameObjectWithTag("PlayerIconUI").name = nowGo.name;
        CardAreaRefresh();        
        HpUIChange();
    }

    //刷新玩家视角下的UI界面
    private void CardAreaRefresh() {
        //删除当前人物UI下的所有手牌
        for (int i = 0; i < playerCardGo.transform.childCount; i++){
            Destroy(playerCardGo.transform.GetChild(i).gameObject);
        }

        //刷新技能栏
        for (int i = 0; i < skillButtonGo.transform.childCount; i++) {
            if(skillButtonGo.transform.GetChild(i).name!="TextBG")
                skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = "空";
        }
        for (int i = 0; i < nowGoCardNorData.skills.Count; i++) {
            if(i>2) break;//暂时只设置了三个技能栏，这里用来限位防止后续报错
            skillButtonGo.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text = nowGoCardNorData.skills[i];
        }

        //刷新当前玩家装备区域的牌
        EquipAreaRefresh();

        //Debug.Log("test: "+nowGoCardNorData.handCard.Count);//显示当前角色所拥有的手牌张数
        for (int i = 0; i < nowGoCardNorData.handCard.Count; i++){
            GameObject go = Instantiate(nowGoCardNorData.handCard[i]);
            //go.GetComponent<CardInfoDisplay>().AddCardInfo();//给该人物绑定的卡牌库添加文本
            go.transform.SetParent(playerCardGo.transform, false);
        }

        if (nowGo.GetComponent<CardNorData>().maxHp == 2) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("二血");
        }
        if (nowGo.GetComponent<CardNorData>().maxHp == 3) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("三血");
        }
        if (nowGo.GetComponent<CardNorData>().maxHp == 4) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("四血");
        }
        if (nowGo.GetComponent<CardNorData>().maxHp == 5) {
            GameObject.Find("PlayerHP").GetComponent<Image>().sprite = Resources.Load<Sprite>("五血");
        }
    }

    public void EquipAreaRefresh()
    {
        //刷新当前角色的装备区域内的卡
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


#region 卡堆处理
    //卡堆初始化
    private void CardPileInit() {        
        CardIntoPile(1, "攻", "基础牌",22, 11, 22, 13);
        CardIntoPile(2, "受", "基础牌", 6, 13, 5, 12);
        CardIntoPile(3, "蟠桃", "基础牌", 2, 6, 3, 7);
        CardIntoPile(4, "二昧真火", "技能牌", 0, 1, 1, 0);
        CardIntoPile(5, "三昧真火", "技能牌", 1, 0, 0, 0);
        CardIntoPile(6, "紫金葫芦", "技能牌", 1, 2, 2, 2);
        CardIntoPile(7, "生死簿", "技能牌", 0, 1, 0, 0);
        CardIntoPile(8, "单挑", "技能牌", 2, 2, 0, 0);
        CardIntoPile(9, "翻江倒海", "技能牌", 0, 0, 2, 2);
        CardIntoPile(10, "金刚镯", "技能牌", 1, 3, 3, 2);
        CardIntoPile(11, "火焰山", "技能牌", 2, 0, 0, 0);
        CardIntoPile(12, "金钟罩", "技能牌", 1, 0, 0, 0);
        CardIntoPile(13, "锦阑袈裟", "技能牌", 0, 0, 3, 3);
        CardIntoPile(14, "救命毫毛", "技能牌", 2, 0, 2, 2);
        CardIntoPile(15, "蟠桃大会", "技能牌", 0, 1, 0, 0);
        CardIntoPile(16, "捆妖索", "技能牌", 0, 2, 0, 1);
        CardIntoPile(17, "捆仙索", "技能牌", 1, 2, 0, 1);
        CardIntoPile(18, "护卫", "技能牌", 0, 0, 0, 2);
        CardIntoPile(19, "芭蕉扇", "技能牌", 0, 0, 2, 1);
        CardIntoPile(20, "嘲讽", "技能牌", 0, 0, 1, 0);
        CardIntoPile(21, "孕育", "技能牌", 0, 0, 1, 0);
        CardIntoPile(22, "小打小闹", "技能牌", 1, 0, 0, 1);        
        CardIntoPile(23, "偷天换日", "技能牌", 0, 0, 1, 1);
        CardIntoPile(24, "心灵控制", "技能牌", 1, 3, 0, 0);
        CardIntoPile(25, "七星宝剑", "武器牌", 0, 0, 0, 1);
        CardIntoPile(26, "金刚圈", "武器牌", 0, 0, 0, 1);
        CardIntoPile(27, "紫金铃铛", "武器牌", 0, 0, 1, 0);
        CardIntoPile(28, "九瓣花花锤", "武器牌", 0, 0, 0, 1);
        CardIntoPile(29, "八丈火尖枪", "武器牌", 0, 1, 0, 0);
        CardIntoPile(30, "九齿钉钉耙", "武器牌", 0, 1, 0,0);
        CardIntoPile(31, "方方便便铲", "武器牌", 0, 0, 1, 0);
        CardIntoPile(32, "短短狼牙棒", "武器牌", 0, 0, 1, 0);
        CardIntoPile(33, "红缨枪", "武器牌", 1, 0, 0, 0);
        CardIntoPile(34, "金箍棒", "武器牌", 1, 0, 0, 0);
        CardIntoPile(35, "象王", "坐骑牌", 0, 0, 0, 1);
        CardIntoPile(36, "狮王", "坐骑牌", 1, 0, 0, 0);
        CardIntoPile(37, "仙鹤", "坐骑牌", 0, 1, 0, 0);
        CardIntoPile(38, "神龟", "坐骑牌", 0, 0, 1, 0);
        CardIntoPile(39, "筋斗云", "坐骑牌", 1, 0, 0, 0);
        CardIntoPile(40, "避水金睛兽", "坐骑牌", 1, 0, 0, 0);
        CardIntoPile(41, "风火轮", "坐骑牌", 0, 1, 0, 0);
        CardIntoPile(42, "白龙马", "坐骑牌", 0, 0, 0, 1);
        CardIntoPile(43, "大鹏金翅鸟", "坐骑牌", 0, 1, 0, 0);
        
        //根据上方给定的cardPileMode同步initCardPile
        for (int i = 1; i <= cardPileCtrl.kindCount; i++) {
            if (cardPileCtrl.kind[i] == "基础牌") {
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
            } else if (cardPileCtrl.kind[i] == "技能牌") {
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
            } else if (cardPileCtrl.kind[i] == "武器牌") {
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
            } else if (cardPileCtrl.kind[i] == "坐骑牌"){
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

    //对牌堆进行随机洗牌t次
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

    //以下函数将对应卡牌存入cardPileMode中，函数变量分别为卡牌编号，卡牌名，卡牌类型，该卡牌黑桃、红心、梅花、方片四种类型的张数
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

    //确认按钮绑定的事件
    public void EnterButton()
    {
        Debug.Log("现在调用了EnterButton函数： ");
        //玩家在别人的等待阶段打出需要打出的牌的处理
        if (nowGo.tag == "Player" && nowGoCardNorData.needOutCard != "" && GetCardName(nowGoCardNorData.currentClickCard) == nowGoCardNorData.needOutCard
            && nowGoCardNorData.currentClickCard.GetComponent<Toggle>().isOn) {
            Debug.Log(nowGo.name + "在"+nRoleList[preRoleNum].name+"的等待阶段打出了" + nowGoCardNorData.currentClickCard.name);
            nowGoCardNorData.currentClickCard.AddComponent<Canvas>();
            nowGoCardNorData.currentClickCard.GetComponent<Canvas>().overrideSorting = true;
            nowGoCardNorData.currentClickCard.GetComponent<Canvas>().sortingOrder = 1;
            //存储当前needOutCard非空的人物的cardNorData，因为在动画结束后会刷新nowCardNorData。
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
        //如果现在是玩家角色的回合的出牌阶段且选中了手牌并打出，则处理完移动动画后删除该手牌                
        if (nowGoCardNorData.phase==(byte)TimePhase.OutCard) {
            if (nowGo.tag == "Player" && nowGoCardNorData.currentClickCard != null && nowGoCardNorData.currentClickCard.GetComponent<Toggle>().isOn) {                
                string cardName = GetCardName(nowGoCardNorData.currentClickCard);                
                if (CanOut(cardName)) {
                    Debug.Log(nowGo.name + "打出了" + cardName);
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
                            #region 基础牌
                            case "攻":
                                nowGoCardNorData.selectRoles[0].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                                nowGoCardNorData.singleSelect = false;
                                nowGoCardNorData.atkNum++;
                                nowGoCardNorData.selectRoles[0].GetComponent<CardNorData>().needOutCard = "受";
                                //进入等待阶段的有关处理
                                prePhase = (TimePhase)nowGoCardNorData.phase;
                                preNowTime = nowTime;
                                nowTime = 0;
                                preRoleNum = nowRoleNum;
                                nowGoCardNorData.intoWaitCard = "攻";
                                haveWait = true;
                                nowGoCardNorData.phase = (byte)TimePhase.Wait;
                                SwitchRole();
                                break;
                            case "蟠桃":
                                nowGoCardNorData.nowHp += 1;
                                break;
                            #endregion

                            #region 技能牌

                            #endregion 

                            #region 武器牌
                            case "七星宝剑":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "金刚圈":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "紫金铃铛":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "九瓣花花锤":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "九齿钉钉耙":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "方方便便铲":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "短短狼牙棒":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "红缨枪":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            case "金箍棒":
                                nowGoCardNorData.weapon = sprite;
                                EquipAreaRefresh();
                                break;
                            #endregion

                            #region 坐骑牌
                            case "象王":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "狮王":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "仙鹤":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "神龟":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "筋斗云":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "避水金睛兽":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "风火轮":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "白龙马":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                            case "大鹏金翅鸟":
                                nowGoCardNorData.mount = sprite;
                                EquipAreaRefresh();
                                break;
                             #endregion
                        }
                    });                    
                }
                else {
                    Debug.Log(cardName + "现在用不了了");
                }
            }
        }

        //当前处于弃牌阶段
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

    //得到去除掉数字的卡牌名
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

    //判断现在当前玩家能否出给定的cardName这张牌
    private bool CanOut(string cardName)
    {
        if (cardName == "攻") {
            if (nowGoCardNorData.selectRoles.Count == 0)
                return false;
            if (nowGoCardNorData.atkNum == 0)
                return true;
            if (nowGoCardNorData.atkNum > 0) {
                if (HaveEquip("七星宝剑"))
                    return true;
                if (nowGo.name == "牛魔王")
                    return true;
                return false;
            }
        }
        if (cardName == "蟠桃") {
            if (nowGoCardNorData.nowHp == nowGoCardNorData.maxHp)
                return false;
            return true;
        }
        if (cardName == "受") {
            return false;
        }
        return true;
    }

    //看当前人物是不是没有给定名字的装备(武器，坐骑）,如果有则返回true，没有则返回false
    private bool HaveEquip(string cardName)
    {
        string[] cardNames = new string[5] { " ", cardName + "1", cardName + "2", cardName + "3", cardName + "4" };
        //看武器牌
        for (byte i = 1; i <= 4; i++) {
            if (nowGoCardNorData.weapon != null && nowGoCardNorData.weapon.name == cardNames[i]) {
                return true;
            }
        }

        //看坐骑牌
        for (byte i = 1; i <= 4; i++) {
            if (nowGoCardNorData.mount != null && nowGoCardNorData.mount.name == cardNames[i]) {
                return true;
            }
        }

        return false;
    }

    //取消按钮绑定的事件
    public void QuitButton()
    {
        Debug.Log("现在调用了QuitButton函数： ");
        for (byte i = 0; i < playerCardGo.transform.childCount; i++) {
            if (playerCardGo.transform.GetChild(i).GetComponent<Toggle>().isOn) {
                playerCardGo.transform.GetChild(i).GetComponent<Toggle>().isOn = false;

            }
        }
            
    }

    //回合结束按钮绑定的事件
    public void TimeEndButton()
    {
        Debug.Log("现在调用了TimeEndButton函数： ");
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

//卡堆控制类，包括生成卡堆的种类，总数，卡堆名字，各个花色数量等数据
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