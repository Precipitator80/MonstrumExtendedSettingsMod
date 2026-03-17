using UnityEngine;

namespace MonstrumExtendedSettingsMod.Setting
{
    class MonsterVisionConeAngle : Setting
    {
        public static float fiendVisionConeAngle;
        public static bool applyFiendVisionConeAngleToAllMonsters;

        protected override bool ShouldSettingBeEnabled()
        {
            return fiendVisionConeAngle != 70;
        }

        protected override void OnEnable()
        {
            On.ConeControl.Update += HookConeControlUpdate;
        }

        protected override void OnDisable()
        {
            On.ConeControl.Update -= HookConeControlUpdate;
        }

        private void HookConeControlUpdate(On.ConeControl.orig_Update orig, ConeControl coneControl)
        {
            if (coneControl.monster.IsPlayerInAttackRange())
            {
                coneControl.vision.coneAngle = 100f;
            }
            else
            {
                float angleToUse = (coneControl.monster.MonsterType == Monster.MonsterTypeEnum.Fiend || applyFiendVisionConeAngleToAllMonsters) ? fiendVisionConeAngle : 70f;
                if (coneControl.monster.IsDistracted)
                {
                    coneControl.vision.coneAngle = Mathf.RoundToInt(5f / 14f * angleToUse);
                }
                else
                {
                    coneControl.vision.coneAngle = angleToUse;
                }
            }

        }
    }
}