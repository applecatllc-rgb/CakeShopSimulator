using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Level1_Preparation";

    public void GoToNextScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError(
                $"请先把场景 {nextSceneName} 添加到构建场景列表。");
            return;
        }

        Debug.Log(
            $"进入 {nextSceneName}，分数：{GameSession.Score}，" +
            $"已接订单：{GameSession.Orders.Count}");

        SceneManager.LoadScene(nextSceneName);
    }
}
