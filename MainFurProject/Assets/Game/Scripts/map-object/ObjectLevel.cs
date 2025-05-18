
using TMPro;
using UnityEngine;

public class ObjectLevel : MonoBehaviour
{
    [SerializeField] private TextMeshPro m_text;

    void Start()
    {
        PlayMode playMode = MainModel.gameInfo.playMode;
        switch(playMode)
        {
            case PlayMode.Normal:
                string levelName = ConfigLoader.instance.GetLevel(MainModel.gameInfo.world, MainModel.gameInfo.level).levelPath;
                bool isBonus = ConfigLoader.IsBonusLevel(levelName);
                m_text.text = isBonus ? "Bonus" : ConfigLoader.GetLevelString(MainModel.gameInfo.world, MainModel.gameInfo.level);
                break;
            case PlayMode.Boss:
                m_text.text = "Boss";
                break;
        }
    }
}
