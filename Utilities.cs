// ~Beginning Of File
using UnityEngine;
using System.IO;

namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~Utilities
        // Class for utilities not commonly used as well as unused but potentially useful utilities. More commonly used utilities can be found in ModSettings.

        private static class Utilities
        {
            public static void CopyParticleSystem(ParticleSystem pastePS, ParticleSystem copyPS)
            {
                ParticleSystem.CollisionModule pasteC = pastePS.collision;
                ParticleSystem.CollisionModule copyC = copyPS.collision;
                pasteC.bounce = copyC.bounce;
                pasteC.bounceMultiplier = copyC.bounceMultiplier;
                pasteC.collidesWith = copyC.collidesWith;
                pasteC.dampen = copyC.dampen;
                pasteC.dampenMultiplier = copyC.dampenMultiplier;
                pasteC.enabled = copyC.enabled;
                pasteC.enableDynamicColliders = copyC.enableDynamicColliders;
                pasteC.enableInteriorCollisions = copyC.enableInteriorCollisions;
                pasteC.lifetimeLoss = copyC.lifetimeLoss;
                pasteC.lifetimeLossMultiplier = copyC.lifetimeLossMultiplier;
                pasteC.maxCollisionShapes = copyC.maxCollisionShapes;
                pasteC.maxKillSpeed = copyC.maxKillSpeed;
                pasteC.minKillSpeed = copyC.minKillSpeed;
                pasteC.mode = copyC.mode;
                pasteC.quality = copyC.quality;
                pasteC.radiusScale = copyC.radiusScale;
                pasteC.sendCollisionMessages = copyC.sendCollisionMessages;
                pasteC.type = copyC.type;
                pasteC.voxelSize = copyC.voxelSize;

                ParticleSystem.ColorBySpeedModule pasteCBS = pastePS.colorBySpeed;
                ParticleSystem.ColorBySpeedModule copyCBS = copyPS.colorBySpeed;
                pasteCBS.color = copyCBS.color;
                pasteCBS.enabled = copyCBS.enabled;
                pasteCBS.range = copyCBS.range;

                ParticleSystem.ColorOverLifetimeModule pasteCOL = pastePS.colorOverLifetime;
                ParticleSystem.ColorOverLifetimeModule copyCOL = copyPS.colorOverLifetime;
                pasteCOL.color = copyCOL.color;
                pasteCOL.enabled = copyCOL.enabled;

                ParticleSystem.EmissionModule pasteE = pastePS.emission;
                ParticleSystem.EmissionModule copyE = copyPS.emission;
                pasteE.enabled = copyE.enabled;
                pasteE.rateOverDistance = copyE.rateOverDistance;
                pasteE.rateOverDistanceMultiplier = copyE.rateOverDistanceMultiplier;
                pasteE.rateOverTime = copyE.rateOverTime;
                pasteE.rateOverTimeMultiplier = copyE.rateOverTimeMultiplier;

                ParticleSystem.ExternalForcesModule pasteEF = pastePS.externalForces;
                ParticleSystem.ExternalForcesModule copyEF = copyPS.externalForces;
                pasteEF.enabled = copyEF.enabled;
                pasteEF.multiplier = copyEF.multiplier;

                ParticleSystem.ForceOverLifetimeModule pasteFOL = pastePS.forceOverLifetime;
                ParticleSystem.ForceOverLifetimeModule copyFOL = copyPS.forceOverLifetime;
                pasteFOL.enabled = copyFOL.enabled;
                pasteFOL.randomized = copyFOL.randomized;
                pasteFOL.space = copyFOL.space;
                pasteFOL.x = copyFOL.x;
                pasteFOL.xMultiplier = copyFOL.xMultiplier;
                pasteFOL.y = copyFOL.y;
                pasteFOL.yMultiplier = copyFOL.yMultiplier;
                pasteFOL.z = copyFOL.z;
                pasteFOL.zMultiplier = copyFOL.zMultiplier;

                pastePS.hideFlags = copyPS.hideFlags;

                ParticleSystem.LightsModule pasteL = pastePS.lights;
                ParticleSystem.LightsModule copyL = copyPS.lights;
                pasteL.alphaAffectsIntensity = copyL.alphaAffectsIntensity;
                pasteL.enabled = copyL.enabled;
                pasteL.intensity = copyL.intensity;
                pasteL.intensityMultiplier = copyL.intensityMultiplier;
                pasteL.light = copyL.light;
                pasteL.maxLights = copyL.maxLights;
                pasteL.range = copyL.range;
                pasteL.rangeMultiplier = copyL.rangeMultiplier;
                pasteL.ratio = copyL.ratio;
                pasteL.sizeAffectsRange = copyL.sizeAffectsRange;
                pasteL.useParticleColor = copyL.useParticleColor;
                pasteL.useRandomDistribution = copyL.useRandomDistribution;

                ParticleSystem.LimitVelocityOverLifetimeModule pasteLVOL = pastePS.limitVelocityOverLifetime;
                ParticleSystem.LimitVelocityOverLifetimeModule copyLVOL = copyPS.limitVelocityOverLifetime;
                pasteLVOL.dampen = copyLVOL.dampen;
                pasteLVOL.enabled = copyLVOL.enabled;
                pasteLVOL.limit = copyLVOL.limit;
                pasteLVOL.limitMultiplier = copyLVOL.limitMultiplier;
                pasteLVOL.limitX = copyLVOL.limitX;
                pasteLVOL.limitXMultiplier = copyLVOL.limitXMultiplier;
                pasteLVOL.limitY = copyLVOL.limitY;
                pasteLVOL.limitYMultiplier = copyLVOL.limitYMultiplier;
                pasteLVOL.limitZ = copyLVOL.limitZ;
                pasteLVOL.limitZMultiplier = copyLVOL.limitZMultiplier;
                pasteLVOL.separateAxes = copyLVOL.separateAxes;
                pasteLVOL.space = copyLVOL.space;

                ParticleSystem.MainModule pasteM = pastePS.main;
                ParticleSystem.MainModule copyM = copyPS.main;
                pasteM.customSimulationSpace = copyM.customSimulationSpace;
                pasteM.duration = copyM.duration;
                pasteM.gravityModifier = copyM.gravityModifier;
                pasteM.gravityModifierMultiplier = copyM.gravityModifierMultiplier;
                pasteM.loop = copyM.loop;
                pasteM.maxParticles = copyM.maxParticles;
                pasteM.playOnAwake = copyM.playOnAwake;
                pasteM.prewarm = copyM.prewarm;
                pasteM.randomizeRotationDirection = copyM.randomizeRotationDirection;
                pasteM.scalingMode = copyM.scalingMode;
                pasteM.simulationSpace = copyM.simulationSpace;
                pasteM.simulationSpeed = copyM.simulationSpeed;
                pasteM.startColor = copyM.startColor;
                pasteM.startDelay = copyM.startDelay;
                pasteM.startDelayMultiplier = copyM.startDelayMultiplier;
                pasteM.startLifetime = copyM.startLifetime;
                pasteM.startLifetimeMultiplier = copyM.startLifetimeMultiplier;
                pasteM.startRotation = copyM.startRotation;
                pasteM.startRotation3D = copyM.startRotation3D;
                pasteM.startRotationMultiplier = copyM.startRotationMultiplier;
                pasteM.startRotationX = copyM.startRotationX;
                pasteM.startRotationXMultiplier = copyM.startRotationXMultiplier;
                pasteM.startRotationY = copyM.startRotationY;
                pasteM.startRotationYMultiplier = copyM.startRotationYMultiplier;
                pasteM.startRotationZ = copyM.startRotationZ;
                pasteM.startRotationZMultiplier = copyM.startRotationZMultiplier;
                pasteM.startSize = copyM.startSize;
                pasteM.startSize3D = copyM.startSize3D;
                pasteM.startSizeMultiplier = copyM.startSizeMultiplier;
                pasteM.startSizeX = copyM.startSizeX;
                pasteM.startSizeXMultiplier = copyM.startSizeXMultiplier;
                pasteM.startSizeY = copyM.startSizeY;
                pasteM.startSizeYMultiplier = copyM.startSizeYMultiplier;
                pasteM.startSizeZ = copyM.startSizeZ;
                pasteM.startSizeZMultiplier = copyM.startSizeZMultiplier;
                pasteM.startSpeed = copyM.startSpeed;
                pasteM.startSpeedMultiplier = copyM.startSpeedMultiplier;

                ParticleSystem.NoiseModule pasteN = pastePS.noise;
                ParticleSystem.NoiseModule copyN = copyPS.noise;
                pasteN.damping = copyN.damping;
                pasteN.enabled = copyN.enabled;
                pasteN.frequency = copyN.frequency;
                pasteN.octaveCount = copyN.octaveCount;
                pasteN.octaveMultiplier = copyN.octaveMultiplier;
                pasteN.octaveScale = copyN.octaveScale;
                pasteN.quality = copyN.quality;
                pasteN.remap = copyN.remap;
                pasteN.remapEnabled = copyN.remapEnabled;
                pasteN.remapMultiplier = copyN.remapMultiplier;
                pasteN.remapX = copyN.remapX;
                pasteN.remapXMultiplier = copyN.remapXMultiplier;
                pasteN.remapY = copyN.remapY;
                pasteN.remapYMultiplier = copyN.remapYMultiplier;
                pasteN.remapZ = copyN.remapZ;
                pasteN.remapZMultiplier = copyN.remapZMultiplier;
                pasteN.scrollSpeed = copyN.scrollSpeed;
                pasteN.scrollSpeedMultiplier = copyN.scrollSpeedMultiplier;
                pasteN.separateAxes = copyN.separateAxes;
                pasteN.strength = copyN.strength;
                pasteN.strengthMultiplier = copyN.strengthMultiplier;
                pasteN.strengthX = copyN.strengthX;
                pasteN.strengthXMultiplier = copyN.strengthXMultiplier;
                pasteN.strengthY = copyN.strengthY;
                pasteN.strengthYMultiplier = copyN.strengthYMultiplier;
                pasteN.strengthZ = copyN.strengthZ;
                pasteN.strengthZMultiplier = copyN.strengthZMultiplier;

                pastePS.randomSeed = copyPS.randomSeed;

                ParticleSystem.RotationBySpeedModule pasteRBS = pastePS.rotationBySpeed;
                ParticleSystem.RotationBySpeedModule copyRBS = copyPS.rotationBySpeed;
                pasteRBS.enabled = copyRBS.enabled;
                pasteRBS.range = copyRBS.range;
                pasteRBS.separateAxes = copyRBS.separateAxes;
                pasteRBS.x = copyRBS.x;
                pasteRBS.xMultiplier = copyRBS.xMultiplier;
                pasteRBS.y = copyRBS.y;
                pasteRBS.yMultiplier = copyRBS.yMultiplier;
                pasteRBS.z = copyRBS.z;
                pasteRBS.zMultiplier = copyRBS.zMultiplier;

                ParticleSystem.RotationOverLifetimeModule pasteROL = pastePS.rotationOverLifetime;
                ParticleSystem.RotationOverLifetimeModule copyROL = copyPS.rotationOverLifetime;
                pasteROL.enabled = copyROL.enabled;
                pasteROL.separateAxes = copyROL.separateAxes;
                pasteROL.x = copyROL.x;
                pasteROL.xMultiplier = copyROL.xMultiplier;
                pasteROL.y = copyROL.y;
                pasteROL.yMultiplier = copyROL.yMultiplier;
                pasteROL.z = copyROL.z;
                pasteROL.zMultiplier = copyROL.zMultiplier;

                ParticleSystem.ShapeModule pasteS = pastePS.shape;
                ParticleSystem.ShapeModule copyS = copyPS.shape;
                pasteS.alignToDirection = copyS.alignToDirection;
                pasteS.angle = copyS.angle;
                pasteS.arc = copyS.arc;
                pasteS.box = copyS.box;
                pasteS.enabled = copyS.enabled;
                pasteS.length = copyS.length;
                pasteS.mesh = copyS.mesh;
                pasteS.meshMaterialIndex = copyS.meshMaterialIndex;
                pasteS.meshRenderer = copyS.meshRenderer;
                pasteS.meshScale = copyS.meshScale;
                pasteS.meshShapeType = copyS.meshShapeType;
                pasteS.normalOffset = copyS.normalOffset;
                pasteS.radius = copyS.radius;
                pasteS.randomDirectionAmount = copyS.randomDirectionAmount;
                pasteS.shapeType = copyS.shapeType;
                pasteS.skinnedMeshRenderer = copyS.skinnedMeshRenderer;
                pasteS.sphericalDirectionAmount = copyS.sphericalDirectionAmount;
                pasteS.useMeshColors = copyS.useMeshColors;
                pasteS.useMeshMaterialIndex = copyS.useMeshMaterialIndex;

                ParticleSystem.SizeBySpeedModule pasteSBS = pastePS.sizeBySpeed;
                ParticleSystem.SizeBySpeedModule copySBS = copyPS.sizeBySpeed;
                pasteSBS.enabled = copySBS.enabled;
                pasteSBS.range = copySBS.range;
                pasteSBS.separateAxes = copySBS.separateAxes;
                pasteSBS.size = copySBS.size;
                pasteSBS.sizeMultiplier = copySBS.sizeMultiplier;
                pasteSBS.x = copySBS.x;
                pasteSBS.xMultiplier = copySBS.xMultiplier;
                pasteSBS.y = copySBS.y;
                pasteSBS.yMultiplier = copySBS.yMultiplier;
                pasteSBS.z = copySBS.z;
                pasteSBS.zMultiplier = copySBS.zMultiplier;

                ParticleSystem.SizeOverLifetimeModule pasteSOL = pastePS.sizeOverLifetime;
                ParticleSystem.SizeOverLifetimeModule copySOL = copyPS.sizeOverLifetime;
                pasteSOL.enabled = copySOL.enabled;
                pasteSOL.separateAxes = copySOL.separateAxes;
                pasteSOL.size = copySOL.size;
                pasteSOL.sizeMultiplier = copySOL.sizeMultiplier;
                pasteSOL.x = copySOL.x;
                pasteSOL.xMultiplier = copySOL.xMultiplier;
                pasteSOL.y = copySOL.y;
                pasteSOL.yMultiplier = copySOL.yMultiplier;
                pasteSOL.z = copySOL.z;
                pasteSOL.zMultiplier = copySOL.zMultiplier;

                ParticleSystem.SubEmittersModule pasteSE = pastePS.subEmitters;
                ParticleSystem.SubEmittersModule copySE = copyPS.subEmitters;
                for (int i = 0; i < copySE.subEmittersCount; i++)
                {
                    pasteSE.AddSubEmitter(copySE.GetSubEmitterSystem(i), copySE.GetSubEmitterType(i), copySE.GetSubEmitterProperties(i));
                }

                ParticleSystem.TextureSheetAnimationModule pasteTSA = pastePS.textureSheetAnimation;
                ParticleSystem.TextureSheetAnimationModule copyTSA = copyPS.textureSheetAnimation;
                pasteTSA.animation = copyTSA.animation;
                pasteTSA.cycleCount = copyTSA.cycleCount;
                pasteTSA.enabled = copyTSA.enabled;
                pasteTSA.flipU = copyTSA.flipU;
                pasteTSA.flipV = copyTSA.flipV;
                pasteTSA.frameOverTime = copyTSA.frameOverTime;
                pasteTSA.frameOverTimeMultiplier = copyTSA.frameOverTimeMultiplier;
                pasteTSA.numTilesX = copyTSA.numTilesX;
                pasteTSA.numTilesY = copyTSA.numTilesY;
                pasteTSA.rowIndex = copyTSA.rowIndex;
                pasteTSA.startFrame = copyTSA.startFrame;
                pasteTSA.startFrameMultiplier = copyTSA.startFrameMultiplier;
                pasteTSA.useRandomRow = copyTSA.useRandomRow;
                pasteTSA.uvChannelMask = copyTSA.uvChannelMask;

                pastePS.time = copyPS.time;

                ParticleSystem.TrailModule pasteTra = pastePS.trails;
                ParticleSystem.TrailModule copyTra = copyPS.trails;
                pasteTra.colorOverLifetime = copyTra.colorOverLifetime;
                pasteTra.colorOverTrail = copyTra.colorOverTrail;
                pasteTra.dieWithParticles = copyTra.dieWithParticles;
                pasteTra.enabled = copyTra.enabled;
                pasteTra.inheritParticleColor = copyTra.inheritParticleColor;
                pasteTra.lifetime = copyTra.lifetime;
                pasteTra.lifetimeMultiplier = copyTra.lifetimeMultiplier;
                pasteTra.minVertexDistance = copyTra.minVertexDistance;
                pasteTra.ratio = copyTra.ratio;
                pasteTra.sizeAffectsLifetime = copyTra.sizeAffectsLifetime;
                pasteTra.sizeAffectsWidth = copyTra.sizeAffectsWidth;
                pasteTra.textureMode = copyTra.textureMode;
                pasteTra.widthOverTrail = copyTra.widthOverTrail;
                pasteTra.widthOverTrailMultiplier = copyTra.widthOverTrailMultiplier;
                pasteTra.worldSpace = copyTra.worldSpace;

                ParticleSystem.TriggerModule pasteTri = pastePS.trigger;
                ParticleSystem.TriggerModule copyTri = copyPS.trigger;
                pasteTri.enabled = copyTri.enabled;
                pasteTri.enter = copyTri.enter;
                pasteTri.exit = copyTri.exit;
                pasteTri.inside = copyTri.inside;
                pasteTri.outside = copyTri.outside;
                pasteTri.radiusScale = copyTri.radiusScale;

                pastePS.useAutoRandomSeed = copyPS.useAutoRandomSeed;

                ParticleSystem.VelocityOverLifetimeModule pasteVOL = pastePS.velocityOverLifetime;
                ParticleSystem.VelocityOverLifetimeModule copyVOL = copyPS.velocityOverLifetime;
                pasteVOL.enabled = copyVOL.enabled;
                pasteVOL.space = copyVOL.space;
                pasteVOL.x = copyVOL.x;
                pasteVOL.xMultiplier = copyVOL.xMultiplier;
                pasteVOL.y = copyVOL.y;
                pasteVOL.yMultiplier = copyVOL.yMultiplier;
                pasteVOL.z = copyVOL.z;
                pasteVOL.zMultiplier = copyVOL.zMultiplier;
            }

            public static UnityEngine.Object[] LoadAssetBundle(string assetBundleName)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Mods/MESMAssetBundles/" + assetBundleName);
                return AssetBundle.LoadFromFile(fullPath).LoadAllAssets();
            }

            public static Transform RecursiveTransformSearch(Transform transform, string targetTransformName)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    //Debug.Log("Name of transform found is " + transform.GetChild(i).name + " and target name is " + targetTransformName);
                    if (transform.GetChild(i).name.Equals(targetTransformName))
                    {
                        return transform.GetChild(i);
                    }
                    Transform recursiveTransform = RecursiveTransformSearch(transform.GetChild(i), targetTransformName);
                    if (recursiveTransform != null)
                    {
                        return recursiveTransform;
                    }
                }
                return null;
            }

            /* Not currently used utilities.
            public static IEnumerator ActivateGameObjectAfterTime(GameObject gameObject, float timeToWait)
            {
                yield return new WaitForSeconds(timeToWait);
                gameObject.SetActive(true);
            }
            
            private static void RecursiveTransformCheck(Transform transform, int layer)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Debug.Log("Transform found in layer " + layer + ": " + transform.GetChild(i));
                    RecursiveTransformCheck(transform.GetChild(i), layer + 1);
                }
            }

            // @CopyComponent
            public static T CopyComponent<T>(T original, GameObject destination) where T : Component // Copy a component at runtime - Shaffe - https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html - Accessed 14.07.2021
            {
                System.Type type = original.GetType();
                Component copy = destination.AddComponent(type);
                System.Reflection.FieldInfo[] fields = type.GetFields();
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    field.SetValue(copy, field.GetValue(original));
                }
                return copy as T;
            }


            // Copy a component at runtime - turbanov - https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html - Accessed 19.08.2022
            public static T CopyComponent<T>(T original, GameObject destination) where T : Component
            {
                System.Type type = original.GetType();
                var dst = destination.GetComponent(type) as T;
                if (!dst) dst = destination.AddComponent(type) as T;
                var fields = type.GetFields();
                foreach (var field in fields)
                {
                    if (field.IsStatic) continue;
                    field.SetValue(dst, field.GetValue(original));
                }
                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                    prop.SetValue(dst, prop.GetValue(original, null), null);
                }
                return dst as T;
            }
            */

            /*
            foreach (var comp in playerObjectivesToAdd.gameObject.GetComponents<Component>()) // How do I remove ALL components from a gameObject? - jgodfrey - https://answers.unity.com/questions/1173095/how-do-i-remove-all-components-from-a-gameobject.html - Accessed 14.07.2021
            {
                if (!(comp is PlayerObjectives))
                {
                    Debug.Log("Destroying " + comp.name);
                    Destroy(comp);
                }
            }
            */
        }
    }
}