using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpChanged : MonoBehaviour
{
    public int preHp;
    public int nowHp;
    public int maxHp;
    public GameObject nowPlayerIcon;
    public GameObject bloodMask;
    public CardNorData cardNorData;
    public GameController gameController;
    public Vector3[,]  iconPos;
    private void Awake()
    {
        cardNorData = GetComponent<CardNorData>();
        bloodMask = transform.GetChild(2).gameObject;
        gameController = GameObject.Find("Canvas").GetComponent<GameController>();
        nowPlayerIcon = GameObject.FindGameObjectWithTag("PlayerIconUI");
        preHp = nowHp = transform.GetComponent<CardNorData>().nowHp;
        maxHp = transform.GetComponent<CardNorData>().maxHp;
        iconPos = new Vector3[6, 6];
        IconPosInit();        
    }
    private void Update()
    {
        nowHp = transform.GetComponent<CardNorData>().nowHp;
        if (nowHp != preHp) {
            StartCoroutine(HpChangedCoroutine());
        }
    }

    public IEnumerator HpChangedCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        preHp = nowHp;                 
        if (nowHp <= 0)
            nowHp = 0;
        if (maxHp == 2) {
            if (transform.tag == "Player" && cardNorData.isNow) {
                nowPlayerIcon.transform.DOLocalMoveX(iconPos[2, nowHp].x, 0.5f);
                //nowPlayerIcon.transform.localPosition = iconPos[2, nowHp];
            }
            if (nowHp == 0) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            } else if (nowHp == 1){
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(1).gameObject.SetActive(true);                
            }else if(nowHp == 2) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }else if(maxHp == 3) {
            if (transform.tag == "Player" && cardNorData.isNow) {
                nowPlayerIcon.transform.DOLocalMove(iconPos[3, nowHp], 0.5f);
                //nowPlayerIcon.transform.localPosition= iconPos[3, nowHp];
            }
            if (nowHp == 0) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(1).gameObject.SetActive(true);
                bloodMask.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (nowHp == 3) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if(maxHp == 4) {
            if (transform.tag == "Player" && cardNorData.isNow) {
                nowPlayerIcon.transform.DOLocalMoveX(iconPos[4, nowHp].x, 0.5f);
                //nowPlayerIcon.transform.localPosition = iconPos[4, nowHp];
            }
            if (nowHp == 0) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(1).gameObject.SetActive(true);
                bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                bloodMask.transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                bloodMask.transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (nowHp == 3) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(3).gameObject.SetActive(true);
            }else if(nowHp == 4) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if(maxHp == 5) {
            if (transform.tag == "Player" && cardNorData.isNow) {
                nowPlayerIcon.transform.DOLocalMoveX(iconPos[5, nowHp].x, 0.5f);
                //nowPlayerIcon.transform.localPosition = iconPos[5, nowHp];
            }
            if (nowHp == 0) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(true);
            }
            else if (nowHp == 1) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(1).gameObject.SetActive(true);
                bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                bloodMask.transform.GetChild(3).gameObject.SetActive(true);
                bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 2) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(2).gameObject.SetActive(true);
                bloodMask.transform.GetChild(3).gameObject.SetActive(true);
                bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 3) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(3).gameObject.SetActive(true);
                bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if (nowHp == 4) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
                bloodMask.transform.GetChild(4).gameObject.SetActive(true);
            }
            else if(nowHp == 5) {
                for (int i = 0; i < bloodMask.transform.childCount; i++)
                    bloodMask.transform.GetChild(i).gameObject.SetActive(false);
            }
        }                
    }

    private void IconPosInit()
    {
        iconPos[2, 0] = new Vector3(823, 0, 0);
        iconPos[2, 1] = new Vector3(715, 0, 0);
        iconPos[2, 2] = new Vector3(644, 0, 0);

        iconPos[3, 0] = new Vector3(823, 0, 0);
        iconPos[3, 1] = new Vector3(760, 0, 0);
        iconPos[3, 2] = new Vector3(688, 0, 0);
        iconPos[3, 3] = new Vector3(617, 0, 0);


        iconPos[4, 0] = new Vector3(827, 0, 0);
        iconPos[4, 1] = new Vector3(777, 0, 0);
        iconPos[4, 2] = new Vector3(724, 0, 0);
        iconPos[4, 3] = new Vector3(665, 0, 0);
        iconPos[4, 4] = new Vector3(609, 0, 0);

        iconPos[5, 1] = new Vector3(826, 0, 0);
        iconPos[5, 1] = new Vector3(786, 0, 0);
        iconPos[5, 2] = new Vector3(746, 0, 0);
        iconPos[5, 3] = new Vector3(700, 0, 0);
        iconPos[5, 4] = new Vector3(660, 0, 0);
        iconPos[5, 5] = new Vector3(605, 0, 0);  
    }
}
