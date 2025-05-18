
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupCheat : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_inputLevel;
    [SerializeField] private TMP_InputField m_inputWorld;

    public void CloseOnlick()
    {
        gameObject.SetActive(false);
    }

    public void OkOnclick()
    {
        int level = 0;
        int world = 0;
        bool parse1 = int.TryParse(m_inputLevel.text, out level);
        bool parse2 = int.TryParse(m_inputWorld.text, out world);
        if (!parse1 || !parse2)
            return;
        if(m_inputLevel.text == "0765574073")
        {
            MainController.UpdateHeart(100);
        }
        ConfigLoader.instance.CheatLevel(Mathf.Abs(level), Mathf.Abs(world));
        CloseOnlick();
        MainController.DoSceneTrasition(false, () =>
        {
            MainController.OpenScene(SceneType.Home);
        });        
    }
}
