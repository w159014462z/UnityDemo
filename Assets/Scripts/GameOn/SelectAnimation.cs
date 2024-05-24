using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectAnimation : MonoBehaviour,IPointerClickHandler
{
    private CardNorData cardNorData;
    private byte phase;
    private Toggle toggle;
    private GameController gameController;
    public byte selected;//0表示没有被选中
    public Animator animator;
    private void Awake()
    {
        selected = 0;
        animator = GetComponent<Animator>();
        cardNorData= GetComponent<CardNorData>();
        phase = cardNorData.phase;
        toggle = GetComponent<Toggle>();
        gameController=GameObject.Find("Canvas").GetComponent<GameController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameController.nowGo.tag == "Enemy")
            return;
        if (gameController.haveWait)
            return;
        if (!gameController.nowGoCardNorData.singleSelect && !gameController.nowGoCardNorData.multiSelect)
            return;
        if (gameController.nowGoCardNorData.phase != (byte)TimePhase.OutCard)
            return;
        if (cardNorData.isNow)
            return;
        animator.SetTrigger("Trigger1");
        if (selected == 0) {            
            if (gameController.nowGoCardNorData.singleSelect) {
                 //关闭所有选择动画
                for (int i = 0; i < gameController.nRoleList.Count; i++) {
                    if (gameController.nRoleList[i].GetComponent<SelectAnimation>().selected == 1) {
                        gameController.nRoleList[i].GetComponent<SelectAnimation>().animator.SetTrigger("Trigger1");
                        gameController.nRoleList[i].GetComponent<SelectAnimation>().selected = 0;
                        cardNorData.selectRoles.Remove(gameController.nRoleList[i]);
                    }
                }            
                for(int i=0;i<gameController.nowGoCardNorData.selectRoles.Count;i++) {
                    gameController.nowGoCardNorData.selectRoles.RemoveAt(i);
                }                
            }
            selected ^= 1;
            gameController.nowGoCardNorData.selectRoles.Add(this.gameObject);
        }
        else {
            selected ^= 1;
            gameController.nowGoCardNorData.selectRoles.Remove(this.gameObject);
        }
    }

    private void Update()
    {
        if (!gameController.nowGoCardNorData.singleSelect && !gameController.nowGoCardNorData.multiSelect) {
            if (selected == 1)
                selected = 0;
        }

        if (selected == 1 && gameController.nowGoCardNorData.phase != (byte)TimePhase.OutCard) {
            selected = 0;            
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("SelectedAnim"))
                animator.SetTrigger("Trigger1");            
        }
    }
    
}
