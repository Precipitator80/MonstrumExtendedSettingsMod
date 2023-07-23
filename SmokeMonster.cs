// ~Beginning Of File
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~SmokeMonster
        private static class SmokeMonster
        {
            //public static List<GameObject> smokeMonsterList;

            // #SmokeMonsterAfterGenerationInitialisation
            /*
            public static void SmokeMonsterAfterGenerationInitialisation()
            {
                if (smokeMonsterList.Count > 0)
                {
                    ModSettings.smokyShip = true;
                }
            }
            */

            // #CreateSmokeMonster
            public static GameObject CreateSmokeMonster(MonsterSelection monsterSelection)
            {
                GameObject monsterGameObject = UnityEngine.Object.Instantiate<GameObject>(monsterSelection.NameToObject("Fiend"));
                monsterGameObject.name = "Smoke Monster";
                monsterGameObject.GetComponent<Monster>().monsterType = "Smoke Monster";
                if (!ModSettings.giveAllMonstersASmokeShroud)
                {
                    monsterGameObject.AddComponent<SmokeShroud>();
                }
                return monsterGameObject;
            }

            // #InitialiseSmokeMonster
            public static void InitialiseSmokeMonster()
            {
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SmokeMonsterActiveFeatures

            public static void SmokeMonsterActiveFeatures()
            {
            }

            public static GameObject CreateSmokeEmitter(Room room)
            {
                float boxVolume = room.RoomBounds.size.x * room.RoomBounds.size.y * room.RoomBounds.size.z;
                return CreateSmokeEmitter(room, new Vector3(1.5f, 1.5f, 1.5f), boxVolume / 25f);
            }

            public static GameObject CreateSmokeEmitter(Room room, Vector3 size, float rate)
            {
                GameObject smokeGameObject = CreateSmokeEmitter(room.transform, room.RoomBounds.center, room.RoomBounds.size * 0.75f, size, rate);
                TimeScaleManager.Instance.StartCoroutine(WaitForSpawnTimer(room, smokeGameObject));
                return smokeGameObject;
            }

            private static IEnumerator WaitForSpawnTimer(Room room, GameObject smokeGameObject)
            {
                // Disable emission at start.
                ParticleSystem particleSystem = smokeGameObject.GetComponent<ParticleSystem>();
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = false;

                // Wait for player to leave starter room.
                while (!References.Monster.GetComponent<Monster>().Starter.spawning)
                {
                    emission.enabled = false;
                    yield return null;
                }

                // Wait a random time to turn on the smoke.
                float time = 0f;
                float timeToWait = UnityEngine.Random.Range(ModSettings.maxSpawnTime * 2f, ModSettings.maxSpawnTime * 4f);
                while (time < timeToWait)
                {
                    emission.enabled = false;
                    time += Time.deltaTime;
                    yield return null;
                }

                // Check whether the room is unpowered.
                if (!FuseBoxManager.Instance.fuseboxes[room.PrimaryRegion].powered)
                {
                    emission.enabled = true;
                }
                yield break;
            }

            private static Material smokeMaterial;

            // Volumetric Fog in Unity using Particles (Any Rendering Pipeline) - Etredal - https://www.youtube.com/watch?v=UllkvfMR96s - Accessed 21.05.2022
            // Try out the settings in the UnityEditor!
            public static GameObject CreateSmokeEmitter(Transform parentTransform, Vector3 position, Vector3 box, Vector3 size, float rate)
            {
                GameObject smokeGameObject = new GameObject();
                smokeGameObject.transform.SetParent(parentTransform, false);
                smokeGameObject.transform.position = position;

                ParticleSystemRenderer particleSystemRenderer = smokeGameObject.AddComponent<ParticleSystemRenderer>();

                if (smokeMaterial == null)
                {
                    ParticleSystemRenderer[] pss = FindObjectsOfType<ParticleSystemRenderer>();
                    foreach (ParticleSystemRenderer ps in pss)
                    {
                        if (ps.material != null && ps.material.name.Contains("Smoke"))
                        {
                            smokeMaterial = ps.material;
                            smokeMaterial.color = Color.white;
                            break;
                        }
                    }
                }
                particleSystemRenderer.material = smokeMaterial;

                ParticleSystem particleSystem = smokeGameObject.AddComponent<ParticleSystem>();
                particleSystem.name = "SmokeEmitter";

                ParticleSystem.MainModule main = particleSystem.main;
                main.startSize3D = true;
                main.startSizeX = size.x;
                main.startSizeY = size.y;
                main.startSizeZ = size.z;
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.0f, 0.3f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 150;

                ParticleSystem.MinMaxCurve startRotationCurve = main.startRotation;
                startRotationCurve.mode = ParticleSystemCurveMode.Curve;
                startRotationCurve.curve = new AnimationCurve(new Keyframe(0f, -180f), new Keyframe(5f, 180f));

                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.rateOverTime = rate;

                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.box = new Vector3(box.x, box.y, box.z);
                shape.randomDirectionAmount = 1f;

                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 0.5f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(0.35f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
                );
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
                return smokeGameObject;
            }
        }

        private class FogDamageTracker : MonoBehaviour
        {
            NewPlayerClass newPlayerClass;
            MindAttackEffect playerMindAttackEffect;
            DetectRoom playerDetectRoom;
            public bool smokeDamageActive;
            float breathAmount;
            float breathRecovery;

            private void Update()
            {
                if (!smokeDamageActive && DrainFactor() != 0f)
                {
                    base.StartCoroutine(SmokeDamage());
                }
            }

            private void Start()
            {
                newPlayerClass = base.GetComponent<NewPlayerClass>();
                playerMindAttackEffect = newPlayerClass.GetComponentInChildren<MindAttackEffect>();
                playerDetectRoom = newPlayerClass.GetComponentInChildren<DetectRoom>();
                smokeDamageActive = false;
                breathAmount = ModSettings.breathAmount;
                breathRecovery = ModSettings.breathRecovery;
            }

            private float DrainFactor()
            {
                float drainFactor = 0f;
                foreach (SmokeShroud smokeShroud in SmokeShroud.allSmokeShrouds)
                {
                    /*
                    // Sphere calculation.
                    if (Vector3.Distance(smokeShroud.transform.position, newPlayerClass.transform.position) < (smokeShroud.smokeRadius * smokeShroud.smokeDangerRadiusFactor))
                    {
                        drainFactor += ModSettings.smokeShroudBreathDrain;
                    }
                    */
                    // Box calculation
                    if (Mathf.Abs(smokeShroud.transform.position.y - newPlayerClass.transform.position.y) < 2f && Vector3.Distance(smokeShroud.transform.position, newPlayerClass.transform.position) < (smokeShroud.smokeRadius * smokeShroud.smokeDangerRadiusFactor))
                    {
                        drainFactor += ModSettings.smokeShroudBreathDrain;
                    }
                }
                if ((ModSettings.useSmokeMonster || ModSettings.smokyShip) && playerDetectRoom.RoomType == RoomStructure.Corridor)
                {
                    Room room = playerDetectRoom.GetRoom;
                    if (room != null && room.RoomType == RoomStructure.Corridor && room.PowerState == RoomPowerState.Powerable)
                    {
                        foreach (ParticleSystem particleSystem in room.GetComponentsInChildren<ParticleSystem>())
                        {
                            if (particleSystem.name.Equals("SmokeEmitter"))
                            {
                                ParticleSystem.EmissionModule emission = particleSystem.emission;
                                if (emission.enabled)
                                {
                                    drainFactor += ModSettings.smokeCorridorBreathDrain;
                                }
                                break;
                            }
                        }
                    }
                }
                return drainFactor;
            }

            private IEnumerator SmokeDamage()
            {
                smokeDamageActive = true;
                float timeLeft = breathAmount;
                float lastStrength = 0f;
                while (timeLeft > 0f)
                {
                    playerMindAttackEffect.enabled = true;
                    float dangerFactor = DrainFactor();
                    if (dangerFactor != 0f)
                    {
                        timeLeft -= Time.deltaTime * dangerFactor;
                    }
                    else
                    {
                        timeLeft += Time.deltaTime * breathRecovery;
                        if (timeLeft >= breathAmount)
                        {
                            smokeDamageActive = false;
                            yield break;
                        }
                    }
                    float deathTimerCompletionRatio = (breathAmount - timeLeft) / breathAmount; // Set the mind attack strength to a ratio / percentage of the maximum timer.
                    if (playerMindAttackEffect.strength < deathTimerCompletionRatio || playerMindAttackEffect.strength == lastStrength) // Only update if the deathTimerCompletionRatio is greater in case the Fiend is attacking the player already.
                    {
                        playerMindAttackEffect.strength = deathTimerCompletionRatio;
                    }
                    lastStrength = playerMindAttackEffect.strength;
                    yield return null;
                }
                timeLeft = 0f;
                // newPlayerClass.playerMotor.pHealth.InstantKill(PlayerHealth.DamageTypes.MindAttack); // This kills even in multiplayer. Possibly even when you have extra lives left.
                newPlayerClass.playerMotor.pHealth.DoDamage(100f, false, PlayerHealth.DamageTypes.MindAttack, false);
                yield break;
            }
        }

        public class DustFog : MonoBehaviour
        {
            private static Material dustMaterial;
            private void Start()
            {
                float sphereSurfaceArea = 4f * Mathf.PI * (Mathf.Pow(ModSettings.fogFarDistance, 2f));
                GameObject smokeGameObject = SmokeMonster.CreateSmokeEmitter(base.transform, base.transform.position + Vector3.up, Vector3.zero, new Vector3(10f, 10f, 10f), sphereSurfaceArea / 10f);

                ParticleSystemRenderer particleSystemRenderer = smokeGameObject.GetComponent<ParticleSystemRenderer>();
                if (dustMaterial == null)
                {
                    ParticleSystemRenderer[] pss = FindObjectsOfType<ParticleSystemRenderer>();
                    foreach (ParticleSystemRenderer ps in pss)
                    {
                        if (ps.material != null && ps.material.name.Contains("Dust"))
                        {
                            dustMaterial = ps.material;
                            dustMaterial.color = Color.white;
                            break;
                        }
                    }
                }
                particleSystemRenderer.material = dustMaterial;

                ParticleSystem particleSystem = smokeGameObject.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particleSystem.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f);

                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.SphereShell;
                shape.radius = ModSettings.fogFarDistance;

                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 0.5f), new GradientColorKey(Color.black, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
                );
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            }
        }

        public class SmokeShroud : MonoBehaviour
        {
            public static List<SmokeShroud> allSmokeShrouds;
            public float smokeRadius;
            public float smokeDangerRadiusFactor;

            private void Start()
            {
                smokeRadius = ModSettings.smokeShroudRadius;
                smokeDangerRadiusFactor = ModSettings.smokeShroudDangerRadiusFactor;
                float smokeDiameter = 2f * smokeRadius;
                SmokeShroud.allSmokeShrouds.Add(this);

                //float sphereVolume = (4f / 3f) * Mathf.PI * (Mathf.Pow(smokeRadius, 3f));
                float boxVolume = smokeDiameter * 2f * smokeDiameter; // Need to use different damage calculation with box than simply distance.
                GameObject smokeGameObject = SmokeMonster.CreateSmokeEmitter(base.transform, base.transform.position + Vector3.up, new Vector3(smokeDiameter, 2f, smokeDiameter), new Vector3(1.5f, 1.5f, 1.5f), boxVolume / 25f);

                /*
                ParticleSystem particleSystem = smokeGameObject.GetComponent<ParticleSystem>();

                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = smokeRadius;
                */
            }
        }

        public class FireShroud : MonoBehaviour
        {
            private float fireShroudRadius;
            private float fireBlastRadius;

            private void Start()
            {
                // Set the radii.
                fireShroudRadius = ModSettings.fireShroudRadius;
                fireBlastRadius = ModSettings.fireBlastRadius;
                float fireShroudDiameter = fireShroudRadius * 2f;

                // Clone a FuelParticle. (Is this cloning only the FuelDecal or the FuelParticle too and whatever is in its parent? I think it may be cloning the parent, which might make the transform updates easier.) 
                FuelDecal fuelDecal = Instantiate<GameObject>(FindObjectOfType<FuelParticles>().fuelDecal, base.transform.position + Vector3.up, Quaternion.identity).GetComponent<FuelDecal>();

                // Disable the decal material so that it cannot be seen. Possibly also prevents error from instantiation due to DecalManager.
                fuelDecal.fuelDecalMaterial.enabled = false;

                // Make the fire follow the monster.
                fuelDecal.transform.SetParent(base.transform, true);
                fuelDecal.transform.localRotation = Quaternion.identity;
                fuelDecal.flammable.transform.SetParent(fuelDecal.transform, true);
                fuelDecal.flammable.transform.localRotation = Quaternion.identity;
                fuelDecal.flammable.fire.transform.SetParent(fuelDecal.flammable.transform, true);
                fuelDecal.flammable.fire.transform.localRotation = Quaternion.identity;

                // Set the fire to burn forever.
                fuelDecal.flammable.fireFuel = float.MaxValue;
                fuelDecal.flammable.lowFuel = float.MaxValue;
                fuelDecal.maxFuel = float.MaxValue;

                // Change the size of the PlayerDamage BoxCollider.
                fuelDecal.flammable.fire.GetComponentInChildren<BoxCollider>().size = new Vector3(fireShroudDiameter, 0.25f, fireShroudDiameter);

                // Change the properties of the fire ParticleSystem.
                ParticleSystem particleSystem = fuelDecal.flammable.fire.gameObject.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particleSystem.main;
                main.startSize = new ParticleSystem.MinMaxCurve(1.25f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(3f);
                main.maxParticles = 150;
                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.box = new Vector3(fireShroudDiameter, 0.25f, fireShroudDiameter);
                shape.randomDirectionAmount = 1f;
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(30f);

                // Ignite the fire.
                fuelDecal.flammable.StartFire();
            }

            public void FireBlast()
            {
                GameObject fuelDecalGameObject = FindObjectOfType<FuelParticles>().fuelDecal;
                Collider[] colliders = Physics.OverlapBox(base.transform.position + Vector3.up, new Vector3(fireBlastRadius, 0.75f, fireBlastRadius));
                for (int i = 0; i < colliders.Length; i++)
                {
                    AmbushPoint ambushPoint = colliders[i].GetComponentInChildren<AmbushPoint>();
                    if (ambushPoint != null && (ambushPoint.trapType == AmbushPoint.TrapType.Ceiling || ambushPoint.trapType == AmbushPoint.TrapType.Vent))
                    {
                        Bounds roomBounds = ((MonoBehaviour)ambushPoint).transform.GetParentOfType<Room>().RoomBounds;
                        Instantiate<GameObject>(fuelDecalGameObject, roomBounds.center - new Vector3(0f, roomBounds.extents.y, 0f), Quaternion.identity).GetComponent<FuelDecal>().flammable.StartFire();
                    }
                }
            }
        }
    }
}
// ~End Of File