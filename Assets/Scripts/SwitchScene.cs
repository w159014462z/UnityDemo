using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    // �������·���������ָ���ĳ���
    public void LoadMainPage()
    {
        SceneManager.LoadScene(0);
    }
    public void LoadGameOn()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void LoadDeveloper()
    {
        SceneManager.LoadScene(2);
    }
    public void LoadSetting()
    {
        SceneManager.LoadScene(3);
    }

    // ���ô˷������첽����ָ���ĳ���
    public void LoadSceneAsync(int sceneBuildIndex)
    {
        SceneManager.LoadSceneAsync(sceneBuildIndex);
    }

    // ���ô˷������첽����ָ���ĳ��������ü���ģʽ
    public void LoadSceneAsyncWithMode(int sceneBuildIndex, LoadSceneMode loadSceneMode)
    {
        SceneManager.LoadSceneAsync(sceneBuildIndex, loadSceneMode);
    }
}
