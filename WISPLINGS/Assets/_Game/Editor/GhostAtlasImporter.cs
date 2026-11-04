using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Rendering;
using GhostSurvival.Animation;
using GhostSurvival.Characters;
using GhostSurvival.Cosmetics;
using GhostSurvival.Creatures;
using GhostSurvival.Environment;
using GhostSurvival.Cameras;

namespace GhostSurvival.Editor
{
    /// <summary>
    /// Automated setup tool for Ghost Survival / W1SPL1NGS prototype.
    /// Runs automatically via [InitializeOnLoad] and via Unity Tools menu.
    /// Imports and slices sprite atlases, generates animation clips, Animator Controllers,
    /// prefabs, and the ArtSandbox test scene with exact 128x128 specs and custom pivots.
    /// </summary>
    [InitializeOnLoad]
    public static class GhostAtlasImporter
    {
        private const string SpritesDir = "Assets/_Game/Art/Sprites";
        private const string MaterialsDir = "Assets/_Game/Art/Materials";
        private const string AnimationsDir = "Assets/_Game/Animations";
        private const string PrefabsDir = "Assets/_Game/Prefabs";
        private const string ScenesDir = "Assets/_Game/Scenes";

        static GhostAtlasImporter()
        {
            EditorApplication.delayCall += AutoRunSetupOnce;
        }

        private static void AutoRunSetupOnce()
        {
            if (!SessionState.GetBool("W1SPL1NGS_AutoSetupExecuted_v3", false))
            {
                SessionState.SetBool("W1SPL1NGS_AutoSetupExecuted_v3", true);
                SetupAll();
            }
        }

        [MenuItem("Tools/W1SPL1NGS/1. Setup All (Slice, Clips, Prefabs, Scene)")]
        public static void SetupAll()
        {
            Debug.Log("[W1SPL1NGS] Starting complete prototype setup v3...");
            EnsureMaterials();
            SliceAllAtlases();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            CreateAnimationClips();
            CreateAnimatorControllers();
            CreatePrefabs();
            BuildSandboxScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[W1SPL1NGS] Complete prototype setup v3 finished successfully!");
        }

        public static Material GetOrCreateParticleMaterial()
        {
            EnsureDirectory(MaterialsDir);
            string matPath = $"{MaterialsDir}/M_URP_Particle.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");

                mat = new Material(shader);
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            return mat;
        }

        private static void EnsureMaterials()
        {
            GetOrCreateParticleMaterial();
        }

        [MenuItem("Tools/W1SPL1NGS/2. Slice Atlases Only")]
        public static void SliceAllAtlases()
        {
            SliceAtlas($"{SpritesDir}/ghost_main_8x4.png", 8, 4, 128, 128, new Vector2(0.5f, 0.18f), "Ghost", true);
            SliceAtlas($"{SpritesDir}/cosmetic_hat_witch_8x4.png", 8, 4, 128, 128, new Vector2(0.5f, 0.18f), "Hat_Witch", true);
            SliceAtlas($"{SpritesDir}/cosmetic_wings_moth_8x4.png", 8, 4, 128, 128, new Vector2(0.5f, 0.18f), "Wings_Moth", true);
            SliceAtlas($"{SpritesDir}/soul_collectible_6x1.png", 6, 1, 128, 128, new Vector2(0.5f, 0.5f), "Soul", false);
            SliceAtlas($"{SpritesDir}/shade_stalker_4x4.png", 4, 4, 128, 128, new Vector2(0.5f, 0.18f), "Stalker", false);

            Debug.Log("[W1SPL1NGS] All 5 atlases sliced with exact dimensions and pivots.");
        }

        private static void SliceAtlas(string assetPath, int cols, int rows, int cellW, int cellH, Vector2 pivot, string prefix, bool isGhostLayout)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[W1SPL1NGS] Failed to find TextureImporter at {assetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 128;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            int totalH = rows * cellH;
            string[] directions = { "Down", "Left", "Right", "Up" };

            List<SpriteMetaData> metas = new List<SpriteMetaData>();

            for (int r = 0; r < rows; r++)
            {
                int unityY = totalH - (r + 1) * cellH;
                string dir = (rows == 4) ? directions[r] : "Hover";

                for (int c = 0; c < cols; c++)
                {
                    int unityX = c * cellW;
                    string frameName;

                    if (isGhostLayout)
                    {
                        string animType = (c < 4) ? "Idle" : "Walk";
                        int frameIdx = (c < 4) ? c : (c - 4);
                        frameName = $"{prefix}_{dir}_{animType}_{frameIdx}";
                    }
                    else if (prefix == "Stalker")
                    {
                        frameName = $"{prefix}_{dir}_Chase_{c}";
                    }
                    else if (prefix == "Soul")
                    {
                        frameName = $"{prefix}_Hover_{c}";
                    }
                    else
                    {
                        frameName = $"{prefix}_{dir}_{c}";
                    }

                    SpriteMetaData meta = new SpriteMetaData
                    {
                        name = frameName,
                        rect = new Rect(unityX, unityY, cellW, cellH),
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = pivot
                    };
                    metas.Add(meta);
                }
            }

            importer.spritesheet = metas.ToArray();
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        [MenuItem("Tools/W1SPL1NGS/3. Create Animation Clips")]
        public static void CreateAnimationClips()
        {
            EnsureDirectory(AnimationsDir);

            // Frame rate reduced to 6 FPS for soft, calm and floaty ghost animation
            const float ghostFps = 6f;

            string[] directions = { "Down", "Left", "Right", "Up" };
            string ghostAtlasPath = $"{SpritesDir}/ghost_main_8x4.png";
            Dictionary<string, Sprite> ghostSprites = LoadSprites(ghostAtlasPath);

            foreach (var dir in directions)
            {
                Sprite[] idleFrames = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    ghostSprites.TryGetValue($"Ghost_{dir}_Idle_{i}", out idleFrames[i]);
                }
                BuildSpriteClip($"{AnimationsDir}/Ghost_Idle_{dir}.anim", idleFrames, ghostFps, true);

                Sprite[] walkFrames = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    ghostSprites.TryGetValue($"Ghost_{dir}_Walk_{i}", out walkFrames[i]);
                }
                BuildSpriteClip($"{AnimationsDir}/Ghost_Walk_{dir}.anim", walkFrames, ghostFps, true);
            }

            string stalkerAtlasPath = $"{SpritesDir}/shade_stalker_4x4.png";
            Dictionary<string, Sprite> stalkerSprites = LoadSprites(stalkerAtlasPath);

            foreach (var dir in directions)
            {
                Sprite[] chaseFrames = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    stalkerSprites.TryGetValue($"Stalker_{dir}_Chase_{i}", out chaseFrames[i]);
                }
                BuildSpriteClip($"{AnimationsDir}/Stalker_Chase_{dir}.anim", chaseFrames, 6f, true);
            }

            string soulAtlasPath = $"{SpritesDir}/soul_collectible_6x1.png";
            Dictionary<string, Sprite> soulSprites = LoadSprites(soulAtlasPath);
            Sprite[] hoverFrames = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                soulSprites.TryGetValue($"Soul_Hover_{i}", out hoverFrames[i]);
            }
            BuildSpriteClip($"{AnimationsDir}/Soul_Hover.anim", hoverFrames, 6f, true);

            Debug.Log("[W1SPL1NGS] Animation clips regenerated at relaxed frame rate (6 FPS).");
        }

        private static void BuildSpriteClip(string clipPath, Sprite[] sprites, float frameRate, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            clip.frameRate = frameRate;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "VisualRoot/BodyRenderer",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
        }

        [MenuItem("Tools/W1SPL1NGS/4. Create Animator Controllers")]
        public static void CreateAnimatorControllers()
        {
            EnsureDirectory(AnimationsDir);

            string ghostCtrlPath = $"{AnimationsDir}/GhostAnimatorController.controller";
            AnimatorController ghostCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ghostCtrlPath);
            if (ghostCtrl == null)
            {
                ghostCtrl = AnimatorController.CreateAnimatorControllerAtPath(ghostCtrlPath);
            }

            if (!HasParameter(ghostCtrl, "Direction"))
                ghostCtrl.AddParameter("Direction", AnimatorControllerParameterType.Int);
            if (!HasParameter(ghostCtrl, "IsMoving"))
                ghostCtrl.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

            var ghostRootSm = ghostCtrl.layers[0].stateMachine;
            string[] directions = { "Down", "Left", "Right", "Up" };

            Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();

            var childStates = ghostRootSm.states;
            foreach (var cs in childStates)
            {
                ghostRootSm.RemoveState(cs.state);
            }

            for (int d = 0; d < 4; d++)
            {
                string dir = directions[d];
                var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Ghost_Idle_{dir}.anim");
                var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Ghost_Walk_{dir}.anim");

                var idleState = ghostRootSm.AddState($"Idle_{dir}");
                idleState.motion = idleClip;
                states[$"Idle_{d}"] = idleState;

                var walkState = ghostRootSm.AddState($"Walk_{dir}");
                walkState.motion = walkClip;
                states[$"Walk_{d}"] = walkState;

                if (d == 0) ghostRootSm.defaultState = idleState;
            }

            for (int d = 0; d < 4; d++)
            {
                var idleTrans = ghostRootSm.AddAnyStateTransition(states[$"Idle_{d}"]);
                idleTrans.canTransitionToSelf = false;
                idleTrans.duration = 0f;
                idleTrans.hasExitTime = false;
                idleTrans.AddCondition(AnimatorConditionMode.Equals, d, "Direction");
                idleTrans.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");

                var walkTrans = ghostRootSm.AddAnyStateTransition(states[$"Walk_{d}"]);
                walkTrans.canTransitionToSelf = false;
                walkTrans.duration = 0f;
                walkTrans.hasExitTime = false;
                walkTrans.AddCondition(AnimatorConditionMode.Equals, d, "Direction");
                walkTrans.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
            }

            string stalkerCtrlPath = $"{AnimationsDir}/ShadeStalkerController.controller";
            AnimatorController stalkerCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(stalkerCtrlPath);
            if (stalkerCtrl == null)
            {
                stalkerCtrl = AnimatorController.CreateAnimatorControllerAtPath(stalkerCtrlPath);
            }
            if (!HasParameter(stalkerCtrl, "Direction"))
                stalkerCtrl.AddParameter("Direction", AnimatorControllerParameterType.Int);
            if (!HasParameter(stalkerCtrl, "IsChasing"))
                stalkerCtrl.AddParameter("IsChasing", AnimatorControllerParameterType.Bool);

            var stalkerSm = stalkerCtrl.layers[0].stateMachine;
            var stalkerChildStates = stalkerSm.states;
            foreach (var cs in stalkerChildStates)
            {
                stalkerSm.RemoveState(cs.state);
            }

            for (int d = 0; d < 4; d++)
            {
                string dir = directions[d];
                var chaseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Stalker_Chase_{dir}.anim");
                var chaseState = stalkerSm.AddState($"Chase_{dir}");
                chaseState.motion = chaseClip;

                if (d == 0) stalkerSm.defaultState = chaseState;

                var trans = stalkerSm.AddAnyStateTransition(chaseState);
                trans.canTransitionToSelf = false;
                trans.duration = 0f;
                trans.hasExitTime = false;
                trans.AddCondition(AnimatorConditionMode.Equals, d, "Direction");
            }

            string soulCtrlPath = $"{AnimationsDir}/SoulController.controller";
            AnimatorController soulCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(soulCtrlPath);
            if (soulCtrl == null)
            {
                soulCtrl = AnimatorController.CreateAnimatorControllerAtPath(soulCtrlPath);
            }
            var soulSm = soulCtrl.layers[0].stateMachine;
            var hoverClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Soul_Hover.anim");
            var soulChildStates = soulSm.states;
            foreach (var cs in soulChildStates)
            {
                soulSm.RemoveState(cs.state);
            }
            var hoverState = soulSm.AddState("Soul_Hover");
            hoverState.motion = hoverClip;
            soulSm.defaultState = hoverState;

            EditorUtility.SetDirty(ghostCtrl);
            EditorUtility.SetDirty(stalkerCtrl);
            EditorUtility.SetDirty(soulCtrl);
        }

        private static bool HasParameter(AnimatorController ctrl, string paramName)
        {
            foreach (var p in ctrl.parameters)
            {
                if (p.name == paramName) return true;
            }
            return false;
        }

        [MenuItem("Tools/W1SPL1NGS/5. Create Prefabs")]
        public static void CreatePrefabs()
        {
            EnsureDirectory(PrefabsDir);

            Material particleMat = GetOrCreateParticleMaterial();

            var ghostSprites = LoadSpritesList($"{SpritesDir}/ghost_main_8x4.png");
            var hatSprites = LoadSpritesList($"{SpritesDir}/cosmetic_hat_witch_8x4.png");
            var wingsSprites = LoadSpritesList($"{SpritesDir}/cosmetic_wings_moth_8x4.png");
            var stalkerSprites = LoadSpritesList($"{SpritesDir}/shade_stalker_4x4.png");
            var soulSprites = LoadSpritesList($"{SpritesDir}/soul_collectible_6x1.png");

            var ghostCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/GhostAnimatorController.controller");
            var stalkerCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/ShadeStalkerController.controller");
            var soulCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/SoulController.controller");

            // 1. GhostPlayer Prefab
            // GhostPlayer
            // └── VisualRoot (Bobs and breathes with hover effect, moving body, hat, wings in perfect unison)
            //     ├── Shadow
            //     ├── WingsRenderer
            //     ├── BodyRenderer
            //     ├── HatRenderer (Positioned higher, above the eyes)
            //     └── EffectRenderer (URP particle material)
            GameObject ghostRoot = new GameObject("GhostPlayer");
            ghostRoot.AddComponent<SortingGroup>();
            var controller = ghostRoot.AddComponent<GhostController>();
            var ghostAnim = ghostRoot.AddComponent<GhostAnimator>();
            var loadout = ghostRoot.AddComponent<CosmeticLoadout>();
            var syncer = ghostRoot.AddComponent<CosmeticSynchronizer>();
            var col2d = ghostRoot.AddComponent<CircleCollider2D>();
            col2d.radius = 0.35f;
            col2d.offset = new Vector2(0f, 0.15f);
            var ghostAnimatorComp = ghostRoot.AddComponent<Animator>();
            ghostAnimatorComp.runtimeAnimatorController = ghostCtrl;

            // VisualRoot
            GameObject visualRootGo = new GameObject("VisualRoot");
            visualRootGo.transform.SetParent(ghostRoot.transform, false);

            // Shadow
            GameObject shadowGo = new GameObject("Shadow");
            shadowGo.transform.SetParent(visualRootGo.transform, false);
            shadowGo.transform.localPosition = Vector3.zero;
            var shadowSr = shadowGo.AddComponent<SpriteRenderer>();
            shadowSr.sortingOrder = -20;
            shadowSr.color = new Color(0f, 0f, 0f, 0.45f);

            // WingsRenderer (Sorting Order -10)
            GameObject wingsGo = new GameObject("WingsRenderer");
            wingsGo.transform.SetParent(visualRootGo.transform, false);
            wingsGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var wingsSr = wingsGo.AddComponent<SpriteRenderer>();
            wingsSr.sortingOrder = -10;
            if (wingsSprites.Count > 0) wingsSr.sprite = wingsSprites[0];

            // BodyRenderer (Sorting Order 0)
            GameObject bodyGo = new GameObject("BodyRenderer");
            bodyGo.transform.SetParent(visualRootGo.transform, false);
            var bodySr = bodyGo.AddComponent<SpriteRenderer>();
            bodySr.sortingOrder = 0;
            if (ghostSprites.Count > 0) bodySr.sprite = ghostSprites[0];

            // HatRenderer (Sorting Order +10) - Positioned higher (y = 0.22)
            GameObject hatGo = new GameObject("HatRenderer");
            hatGo.transform.SetParent(visualRootGo.transform, false);
            hatGo.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            var hatSr = hatGo.AddComponent<SpriteRenderer>();
            hatSr.sortingOrder = 10;
            if (hatSprites.Count > 0) hatSr.sprite = hatSprites[0];

            // EffectRenderer (Sorting Order +15) with URP particle material
            GameObject effectGo = new GameObject("EffectRenderer");
            effectGo.transform.SetParent(visualRootGo.transform, false);
            effectGo.transform.localPosition = new Vector3(0.3f, 0.3f, 0f);
            CreateWispParticleSystem(effectGo, particleMat);

            // Wire components
            var soController = new SerializedObject(controller);
            soController.FindProperty("visualRoot").objectReferenceValue = visualRootGo.transform;
            soController.ApplyModifiedProperties();

            var soGhostAnim = new SerializedObject(ghostAnim);
            soGhostAnim.FindProperty("animator").objectReferenceValue = ghostAnimatorComp;
            soGhostAnim.FindProperty("bodyRenderer").objectReferenceValue = bodySr;
            soGhostAnim.ApplyModifiedProperties();

            var soLoadout = new SerializedObject(loadout);
            soLoadout.FindProperty("hatRenderer").objectReferenceValue = hatSr;
            soLoadout.FindProperty("wingsRenderer").objectReferenceValue = wingsSr;
            soLoadout.ApplyModifiedProperties();

            // Wire and serialize all 32 sprite lookups for Hat and Wings into CosmeticSynchronizer
            var soSyncer = new SerializedObject(syncer);
            soSyncer.FindProperty("bodyRenderer").objectReferenceValue = bodySr;
            soSyncer.FindProperty("hatRenderer").objectReferenceValue = hatSr;
            soSyncer.FindProperty("wingsRenderer").objectReferenceValue = wingsSr;
            soSyncer.FindProperty("hatOffset").vector3Value = new Vector3(0f, 0.22f, 0f);
            soSyncer.FindProperty("wingsOffset").vector3Value = new Vector3(0f, 0.05f, 0f);

            var propHats = soSyncer.FindProperty("hatSprites");
            propHats.arraySize = hatSprites.Count;
            for (int i = 0; i < hatSprites.Count; i++)
            {
                propHats.GetArrayElementAtIndex(i).objectReferenceValue = hatSprites[i];
            }

            var propWings = soSyncer.FindProperty("wingsSprites");
            propWings.arraySize = wingsSprites.Count;
            for (int i = 0; i < wingsSprites.Count; i++)
            {
                propWings.GetArrayElementAtIndex(i).objectReferenceValue = wingsSprites[i];
            }
            soSyncer.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(ghostRoot, $"{PrefabsDir}/GhostPlayer.prefab");
            GameObject.DestroyImmediate(ghostRoot);

            // 2. ShadeStalker Prefab
            GameObject stalkerGo = new GameObject("ShadeStalker");
            var stalkerRb = stalkerGo.AddComponent<Rigidbody2D>();
            stalkerRb.gravityScale = 0f;
            stalkerRb.freezeRotation = true;
            var stalkerCol = stalkerGo.AddComponent<CircleCollider2D>();
            stalkerCol.radius = 0.4f;
            stalkerCol.offset = new Vector2(0f, 0.15f);

            var stalkerSr = stalkerGo.AddComponent<SpriteRenderer>();
            stalkerSr.sortingOrder = 0;
            if (stalkerSprites.Count > 0) stalkerSr.sprite = stalkerSprites[0];

            var stalkerAnimator = stalkerGo.AddComponent<Animator>();
            stalkerAnimator.runtimeAnimatorController = stalkerCtrl;

            var stalkerComp = stalkerGo.AddComponent<ShadeStalker>();
            var soStalker = new SerializedObject(stalkerComp);
            soStalker.FindProperty("animator").objectReferenceValue = stalkerAnimator;
            soStalker.FindProperty("spriteRenderer").objectReferenceValue = stalkerSr;
            soStalker.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(stalkerGo, $"{PrefabsDir}/ShadeStalker.prefab");
            GameObject.DestroyImmediate(stalkerGo);

            // 3. SoulCollectible Prefab
            GameObject soulGo = new GameObject("SoulCollectible");
            var soulSr = soulGo.AddComponent<SpriteRenderer>();
            soulSr.sortingOrder = 5;
            if (soulSprites.Count > 0) soulSr.sprite = soulSprites[0];

            var soulAnimator = soulGo.AddComponent<Animator>();
            soulAnimator.runtimeAnimatorController = soulCtrl;

            var soulCol = soulGo.AddComponent<CircleCollider2D>();
            soulCol.isTrigger = true;
            soulCol.radius = 0.5f;

            soulGo.AddComponent<SoulCollectible>();

            GameObject soulAura = new GameObject("GlowAura");
            soulAura.transform.SetParent(soulGo.transform, false);
            CreateGlowParticleSystem(soulAura, new Color(0.75f, 0.91f, 1f, 0.4f), particleMat);

            PrefabUtility.SaveAsPrefabAsset(soulGo, $"{PrefabsDir}/SoulCollectible.prefab");
            GameObject.DestroyImmediate(soulGo);

            Debug.Log("[W1SPL1NGS] Prefabs regenerated with unified VisualRoot, synchronized 32 frames for hat/wings, and URP materials.");
        }

        private static ParticleSystem CreateWispParticleSystem(GameObject parent, Material particleMat)
        {
            var ps = parent.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 0.8f;
            main.startSpeed = 0.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.91f, 1f, 0.8f), new Color(0.78f, 0.76f, 1f, 0.6f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.75f, 0.91f, 1f), 0f), new GradientColorKey(new Color(0.78f, 0.76f, 1f), 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.8f;

            var psr = parent.GetComponent<ParticleSystemRenderer>();
            psr.sortingOrder = 15;
            if (particleMat != null) psr.material = particleMat;

            return ps;
        }

        private static ParticleSystem CreateGlowParticleSystem(GameObject parent, Color color, Material particleMat)
        {
            var ps = parent.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 1.0f;
            main.startSpeed = 0.1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = color;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.8f, 1f, 0f));

            var psr = parent.GetComponent<ParticleSystemRenderer>();
            psr.sortingOrder = 4;
            if (particleMat != null) psr.material = particleMat;

            return ps;
        }

        [MenuItem("Tools/W1SPL1NGS/6. Build ArtSandbox Scene")]
        public static void BuildSandboxScene()
        {
            EnsureDirectory(ScenesDir);
            string scenePath = $"{ScenesDir}/ArtSandbox.unity";

            Material particleMat = GetOrCreateParticleMaterial();

            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );

            // 1. Camera with smooth follow & Zoom support (Scrollwheel, I, O)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.0f;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
            var camFollow = camGo.AddComponent<CameraFollow>();

            // 2. Clean Environment Ground Marker (2D, no broken 3D Quad mesh)
            GameObject groundGo = new GameObject("Environment_Ground");
            groundGo.AddComponent<SandboxGround>();

            // 3. Subtle Fog System with valid URP particle material
            GameObject fogGo = new GameObject("Environment_Fog");
            var fogPs = fogGo.AddComponent<ParticleSystem>();
            var fMain = fogPs.main;
            fMain.loop = true;
            fMain.startLifetime = 6.0f;
            fMain.startSpeed = 0.15f;
            fMain.startSize = new ParticleSystem.MinMaxCurve(3.0f, 6.0f);
            fMain.startColor = new Color(0.2f, 0.2f, 0.25f, 0.08f);
            fMain.simulationSpace = ParticleSystemSimulationSpace.World;
            var fEmission = fogPs.emission;
            fEmission.rateOverTime = 15f;
            var fShape = fogPs.shape;
            fShape.shapeType = ParticleSystemShapeType.Box;
            fShape.scale = new Vector3(30f, 30f, 1f);
            var fNoise = fogPs.noise;
            fNoise.enabled = true;
            fNoise.strength = 0.3f;
            var fpsr = fogGo.GetComponent<ParticleSystemRenderer>();
            fpsr.sortingOrder = 20;
            if (particleMat != null) fpsr.material = particleMat;

            // 4. Instantiate Player
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/GhostPlayer.prefab");
            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerInstance.name = "GhostPlayer";
            playerInstance.transform.position = Vector3.zero;
            camFollow.SetTarget(playerInstance.transform);

            // 5. Instantiate Shade Stalker
            GameObject stalkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/ShadeStalker.prefab");
            GameObject stalkerInstance = (GameObject)PrefabUtility.InstantiatePrefab(stalkerPrefab);
            stalkerInstance.name = "ShadeStalker";
            stalkerInstance.transform.position = new Vector3(4.5f, 2.5f, 0f);

            // 6. Instantiate Collectible Souls (5 souls placed around the map)
            GameObject soulPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/SoulCollectible.prefab");
            Vector3[] soulPositions = new Vector3[]
            {
                new Vector3(-2.5f, 1.5f, 0f),
                new Vector3(2.0f, -2.0f, 0f),
                new Vector3(-3.5f, -2.5f, 0f),
                new Vector3(3.5f, 1.0f, 0f),
                new Vector3(0.0f, 3.5f, 0f)
            };

            for (int i = 0; i < soulPositions.Length; i++)
            {
                GameObject soulInstance = (GameObject)PrefabUtility.InstantiatePrefab(soulPrefab);
                soulInstance.name = $"SoulCollectible_{i + 1}";
                soulInstance.transform.position = soulPositions[i];
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[W1SPL1NGS] ArtSandbox scene rebuilt and saved at {scenePath}");
        }

        private static Dictionary<string, Sprite> LoadSprites(string atlasPath)
        {
            Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    dict[sprite.name] = sprite;
                }
            }
            return dict;
        }

        private static List<Sprite> LoadSpritesList(string atlasPath)
        {
            List<Sprite> list = new List<Sprite>();
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    list.Add(sprite);
                }
            }
            return list;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
