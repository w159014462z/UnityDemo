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
    private Identity identity;//��ɫ����ݣ������ģʽʱ��
    public string sex;//��ɫ�Ա�
    public List<string> skills;
    public int roleNum;//��ɫ���
    public int maxHp;//��ɫ�������
    public int nowHp;//��ɫ��ǰ����
    public int atkNum;//��������
    public int atkDis;//�����ľ���
    public bool outCarding;//���ڳ���
    public bool isNow;//�Ƿ�������غ�
    public bool alive;//��ǰ�����Ƿ���
    public bool fairyBound;//�Ƿ���������
    public bool monsterBound;//�Ƿ���������
    public bool singleSelect;//��ѡģʽ
    public bool multiSelect;//��ѡģʽ
    public byte race;//�жϸý�ɫ�����壬0Ϊ�ɣ�1Ϊ��,2Ϊ�ң�����ݺ����
    public byte phase;//�жϸý�ɫ���������Ľ׶Σ�0-4�ֱ��ʾ�غϿ�ʼǰ�����ơ����ơ����ƽ׶Ρ��ȴ��׶�
    public Sprite weapon;//�������װ����������
    public Sprite mount;//�������װ����������
    public Sprite defend;//�������װ���ķ�����
    public string needOutCard;//��Ҫ�������
    public string intoWaitCard;//����ȴ�״̬�������
    public GameObject currentClickCard;//��ǰ�������
    public GameObject preCurrentClickCard;//��ǰ�������
    public List<GameObject> handCard = new List<GameObject>(); //��ǰ��ɫ��ӵ�е������б�
    public List<GameObject> allWeaponCard = new List<GameObject>();//�����������б�
    public List<GameObject> allMountCard = new List<GameObject>();//�����������б�
    public List<GameObject> allBaseCard = new List<GameObject>();//���л������б�
    public List<GameObject> allSkillCard = new List<GameObject>();//���м������б�    
    public List<GameObject> selectRoles=new List<GameObject>();//��ѡ�Ŀ������ö���
    private GameObject tarPosGo;//ɾ�����ƶ����п����ƶ�������λ��

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
   
    //ɾ��������������ͬʱ�ѵײ�Ŀ����б��Ӧ���ƴ��б�ɾ��
    public void DestroyCard(GameObject card) {
        //Ҫ��handCard��ö�ٵ�ǰ������ƣ�Ȼ����ܳɹ���handCard��remove����
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

    //��Ϸ��ʼ������ݳ�ʼ������
    public void DataInit()
    {                
        phase = 0;
        atkNum = 0;
        singleSelect = false;
        multiSelect = false;
        outCarding = false;
        selectRoles.Clear();
    }

    //��ǰ��ɫ������ִ�д���
    public void isDead()
    {
        
    }

    //ʵ����ҵļ���
    public void SkillEffect(string skillName)
    {
        
    }

    public int GetAtkDis()
    {
        if (mount == null && weapon == null) {
            return 1;
        }
        if (GetCardName(weapon) == "���Ȧ")
            return 2;
        if (GetCardName(weapon) == "�Ͻ�����")
            return 2;
        if (GetCardName(weapon) == "�Ű껨����")
            return 2;
        if (GetCardName(mount) == "����")
            return 2;
        if (GetCardName(mount) == "ʨ��")
            return 2;
        if (GetCardName(mount) == "�ɺ�")
            return 2;
        if (GetCardName(mount) == "���")
            return 2;
        if (GetCardName(mount) == "��ˮ����")
            return 2;
        if (GetCardName(mount) == "���������")
            return 2;
        
        if (GetCardName(weapon) == "�̶�������")
            return 3;
        if (GetCardName(weapon) == "��������")
            return 3;
        if (GetCardName(mount) == "�����")
            return 3;
        if (GetCardName(mount) == "������")
            return 3;

        if (GetCardName(weapon) == "���ɻ��ǹ")
            return 4;
        if (GetCardName(weapon) == "�ųݶ�����")
            return 4;
        if (GetCardName(mount) == "���")
            return 4;
        if (GetCardName(weapon) == "��ӧǹ")
            return 4;

        if (GetCardName(weapon) == "�𹿰�")
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
