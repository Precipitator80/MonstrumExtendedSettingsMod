using UnityEngine;

namespace MonstrumExtendedSettingsMod
{

    public partial class ExtendedSettingsModScript
    {
        public class MESMMonster : MonoBehaviour
        {
            public Monster monster;

            public static MESMMonster CreateMESMMonster(MonsterSelection monsterSelection, Monster.MonsterTypeEnum MonsterType)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(monsterSelection.NameToObject(MonsterType.ToString()));
                if (gameObject == null)
                {
                    Debug.Log("Error creating MESMMonster 1!");
                }
                MESMMonster mesmMonster = gameObject.AddComponent<MESMMonster>();
                if (mesmMonster.gameObject == null)
                {
                    Debug.Log("Error creating MESMMonster 2!");
                }
                else
                {
                    Debug.Log("Created MESM monster");
                }
                return mesmMonster;
            }

            protected virtual void Awake()
            {
                monster = GetComponent<Monster>();
            }
        }
    }
}