using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    // 调用以下方法来加载指定的场景
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

    // 调用此方法来异步加载指定的场景
    public void LoadSceneAsync(int sceneBuildIndex)
    {
        SceneManager.LoadSceneAsync(sceneBuildIndex);
    }

    // 调用此方法来异步加载指定的场景并设置加载模式
    public void LoadSceneAsyncWithMode(int sceneBuildIndex, LoadSceneMode loadSceneMode)
    {
        SceneManager.LoadSceneAsync(sceneBuildIndex, loadSceneMode);
    }
}
