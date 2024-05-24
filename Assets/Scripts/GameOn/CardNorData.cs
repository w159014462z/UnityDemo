using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Security.Claims;

public class CardNorData : MonoBehaviour
{
    public PlayerAndEnemy playerAndEnemy;
    private Identity identity;//角色的身份，出身份模式时用
    public string sex;//角色性别
    public List<string> skills;
    public int roleNum;//角色编号
    public int maxHp;//角色最大体力
    public int nowHp;//角色当前体力
    public int atkNum;//出攻次数
    public int atkDis;//出攻的距离
    public bool outCarding;//正在出牌
    public bool isNow;//是否是自身回合
    public bool alive;//当前人物是否存活
    public bool fairyBound;//是否中捆仙索
    public bool monsterBound;//是否中捆妖索
    public bool singleSelect;//单选模式
    public bool multiSelect;//多选模式
    public byte race;//判断该角色的种族，0为仙，1为妖,2为乱，乱身份后面出
    public byte phase;//判断该角色现在所处的阶段，0-4分别表示回合开始前、摸牌、出牌、弃牌阶段、等待阶段
    public Sprite weapon;//该玩家所装备的武器牌
    public Sprite mount;//该玩家所装备的坐骑牌
    public Sprite defend;//该玩家所装备的防具牌
    public string needOutCard;//需要打出的牌
    public string intoWaitCard;//进入等待状态打出的牌
    public GameObject currentClickCard;//当前点击的牌
    public GameObject preCurrentClickCard;//当前点击的牌
    public List<GameObject> handCard = new List<GameObject>(); //当前角色所拥有的手牌列表
    public List<GameObject> allWeaponCard = new List<GameObject>();//所有武器牌列表
    public List<GameObject> allMountCard = new List<GameObject>();//所有坐骑牌列表
    public List<GameObject> allBaseCard = new List<GameObject>();//所有基础牌列表
    public List<GameObject> allSkillCard = new List<GameObject>();//所有技能牌列表    
    public List<GameObject> selectRoles=new List<GameObject>();//挑选的卡牌作用对象
    private GameObject tarPosGo;//删除卡牌动画中卡牌移动的最终位置

    private void Awake() {
        tarPosGo = GameObject.Find("TarPos");        
        atkDis = 1;
        phase = 0;
        atkNum = 0;
        nowHp = maxHp;
        alive = true;
        outCarding = false;
        fairyBound = false;
        monsterBound = false;
        singleSelect = false;
        multiSelect = false;
        weapon = null;
        mount = null;
        defend = null;
        needOutCard = "";
        intoWaitCard = "";
        selectRoles.Clear();
        currentClickCard = preCurrentClickCard = null;
    }
   
    //删除手牌区的手牌同时把底层的卡牌列表对应的牌从列表删除
    public void DestroyCard(GameObject card) {
        //要在handCard中枚举当前点击的牌，然后才能成功从handCard中remove掉。
        string cardName=card.name;
        for (int i = 0; i < cardName.Length; i++) {
            if (cardName[i] == '(') {
                cardName=cardName.Substring(0, i);
                break;
            }
        }  
        for(int i=0;i<handCard.Count;i++) {
            if (handCard[i].name == cardName ) {
                handCard.RemoveAt(i);
                break;
            }           
        }   
        if(this.tag=="Player")
            Destroy(card);
    }

    //游戏开始后的数据初始化函数
    public void DataInit()
    {                
        phase = 0;
        atkNum = 0;
        singleSelect = false;
        multiSelect = false;
        outCarding = false;
        selectRoles.Clear();
    }

    //当前角色死亡的执行代码
    public void isDead()
    {
        
    }

    //实现玩家的技能
    public void SkillEffect(string skillName)
    {
        
    }

    public int GetAtkDis()
    {
        if (mount == null && weapon == null) {
            return 1;
        }
        if (GetCardName(weapon) == "金刚圈")
            return 2;
        if (GetCardName(weapon) == "紫金铃铛")
            return 2;
        if (GetCardName(weapon) == "九瓣花花锤")
            return 2;
        if (GetCardName(mount) == "象王")
            return 2;
        if (GetCardName(mount) == "狮王")
            return 2;
        if (GetCardName(mount) == "仙鹤")
            return 2;
        if (GetCardName(mount) == "神龟")
            return 2;
        if (GetCardName(mount) == "避水金睛兽")
            return 2;
        if (GetCardName(mount) == "大鹏金翅鸟")
            return 2;
        
        if (GetCardName(weapon) == "短短狼牙棒")
            return 3;
        if (GetCardName(weapon) == "方方便便铲")
            return 3;
        if (GetCardName(mount) == "风火轮")
            return 3;
        if (GetCardName(mount) == "白龙马")
            return 3;

        if (GetCardName(weapon) == "八丈火尖枪")
            return 4;
        if (GetCardName(weapon) == "九齿钉钉耙")
            return 4;
        if (GetCardName(mount) == "筋斗云")
            return 4;
        if (GetCardName(weapon) == "红缨枪")
            return 4;

        if (GetCardName(weapon) == "金箍棒")
            return 5;
        return 1;
    }

    public string GetCardName(Sprite sprite)
    {
        string cardName = sprite.name;
        for (byte i = 0; i < cardName.Length; i++) {
            if ((int)cardName[i] >= 48 && (int)cardName[i] <= 57) {
                cardName = cardName.Substring(0, i);
                break;
            }
        }
        return cardName;
    }
}
