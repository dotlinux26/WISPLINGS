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
    /// prefabs, and the ArtSandbox test scene with exact specs and custom pivots.
    /// Supports HD 256x256 slicing (PPU=256) across all assets, cosmetic Hands layer,
    /// and multi-path animation curve bindings.
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
            if (!SessionState.GetBool("W1SPL1NGS_AutoSetupExecuted_v7", false))
            {
                SessionState.SetBool("W1SPL1NGS_AutoSetupExecuted_v7", true);
                SetupAll();
            }
        }

        [MenuItem("Tools/W1SPL1NGS/1. Setup All (Slice, Clips, Prefabs, Scene)")]
        public static void SetupAll()
        {
            Debug.Log("[W1SPL1NGS] Starting complete prototype setup v5 (All HD + Animation Fix)...");
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
            Debug.Log("[W1SPL1NGS] Complete prototype setup v5 finished successfully!");
        }

        public static Material GetOrCreateParticleMaterial()
        {
            EnsureDirectory(MaterialsDir);
            string matPath = $"{MaterialsDir}/M_URP_Particle.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            Texture2D dotTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Sprites/fx_particle_soft_dot.png");
            if (dotTex != null)
            {
                mat.mainTexture = dotTex;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureMaterials()
        {
            GetOrCreateParticleMaterial();
        }

        [MenuItem("Tools/W1SPL1NGS/2. Slice Atlases Only")]
        public static void SliceAllAtlases()
        {
            // All atlases rendered at HD 256x256 per cell with PPU=256
            SliceAtlas($"{SpritesDir}/ghost_main_8x4.png", 8, 4, 256, 256, new Vector2(0.5f, 0.18f), "Ghost", true, 256);
            SliceAtlas($"{SpritesDir}/cosmetic_hat_witch_8x4.png", 8, 4, 256, 256, new Vector2(0.5f, 0.18f), "Hat_Witch", true, 256);
            SliceAtlas($"{SpritesDir}/cosmetic_wings_moth_8x4.png", 8, 4, 256, 256, new Vector2(0.5f, 0.18f), "Wings_Moth", true, 256);
            SliceAtlas($"{SpritesDir}/cosmetic_hands_8x4.png", 8, 4, 256, 256, new Vector2(0.5f, 0.18f), "Hands", true, 256);
            SliceAtlas($"{SpritesDir}/shade_stalker_4x4.png", 4, 4, 256, 256, new Vector2(0.5f, 0.18f), "Stalker", false, 256);
            SliceAtlas($"{SpritesDir}/soul_collectible_6x1.png", 6, 1, 256, 256, new Vector2(0.5f, 0.5f), "Soul", false, 256);

            Debug.Log("[W1SPL1NGS] All 6 atlases sliced with exact HD dimensions and pivots.");
        }

        private static void SliceAtlas(string assetPath, int cols, int rows, int cellW, int cellH, Vector2 pivot, string prefix, bool isGhostLayout, int ppu)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[W1SPL1NGS] TextureImporter not found yet at {assetPath}, skipping slice.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
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
                        frameName = $"{prefix}_Chase_{dir}_{c}";
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

        [MenuItem("Tools/W1SPL1NGS/3. Create Animation Clips Only")]
        public static void CreateAnimationClips()
        {
            EnsureDirectory(AnimationsDir);
            var ghostSprites = LoadSprites($"{SpritesDir}/ghost_main_8x4.png");
            var stalkerSprites = LoadSprites($"{SpritesDir}/shade_stalker_4x4.png");
            var soulSprites = LoadSprites($"{SpritesDir}/soul_collectible_6x1.png");

            string[] directions = { "Down", "Left", "Right", "Up" };

            // 1. Ghost Clips: 4 Idle, 4 Walk at 6 FPS for soft pace
            // Bind both "BodyRenderer" (relative to VisualRoot Animator) and "" as well as "VisualRoot/BodyRenderer"
            string[] ghostPaths = new string[] { "BodyRenderer", "", "VisualRoot/BodyRenderer" };

            foreach (var dir in directions)
            {
                CreateClip($"{AnimationsDir}/Ghost_Idle_{dir}.anim", 6f, new Sprite[]
                {
                    GetSprite(ghostSprites, $"Ghost_{dir}_Idle_0"),
                    GetSprite(ghostSprites, $"Ghost_{dir}_Idle_1"),
                    GetSprite(ghostSprites, $"Ghost_{dir}_Idle_2"),
                    GetSprite(ghostSprites, $"Ghost_{dir}_Idle_3")
                }, ghostPaths);

                CreateClip($"{AnimationsDir}/Ghost_Walk_{dir}.anim", 6f, new Sprite[]
                {
                    GetSprite(ghostSprites, $"Ghost_{dir}_Walk_0"),
                    GetSprite(ghostSprites, $"Ghost_{dir}_Walk_1"),
                    GetSprite(ghostSprites, $"Ghost_{dir}_Walk_2"),
                    GetSprite(ghostSprites, $"Ghost_{dir}_Walk_3")
                }, ghostPaths);
            }

            // 2. ShadeStalker Clips: 4 Chase clips (Animator on stalker root -> path "")
            foreach (var dir in directions)
            {
                CreateClip($"{AnimationsDir}/Stalker_Chase_{dir}.anim", 6f, new Sprite[]
                {
                    GetSprite(stalkerSprites, $"Stalker_Chase_{dir}_0"),
                    GetSprite(stalkerSprites, $"Stalker_Chase_{dir}_1"),
                    GetSprite(stalkerSprites, $"Stalker_Chase_{dir}_2"),
                    GetSprite(stalkerSprites, $"Stalker_Chase_{dir}_3")
                }, new string[] { "" });
            }

            // 3. Soul Collectible Clip: 6 frames hover (Animator on soul root -> path "")
            Sprite[] soulFrames = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                soulFrames[i] = GetSprite(soulSprites, $"Soul_Hover_{i}");
            }
            CreateClip($"{AnimationsDir}/Soul_Hover.anim", 6f, soulFrames, new string[] { "" });

            Debug.Log("[W1SPL1NGS] Generated 13 Animation Clips at 6 FPS with multi-path bindings.");
        }

        private static Sprite GetSprite(Dictionary<string, Sprite> dict, string name)
        {
            if (dict.TryGetValue(name, out Sprite s)) return s;
            Debug.LogWarning($"[W1SPL1NGS] Missing sprite: {name}");
            return null;
        }

        private static void CreateClip(string clipPath, float frameRate, Sprite[] frames, string[] targetPaths)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = frameRate;

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = frames[i]
                };
            }

            foreach (var p in targetPaths)
            {
                EditorCurveBinding binding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer),
                    path = p,
                    propertyName = "m_Sprite"
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
        }

        [MenuItem("Tools/W1SPL1NGS/4. Create Animator Controllers Only")]
        public static void CreateAnimatorControllers()
        {
            EnsureDirectory(AnimationsDir);

            // 1. Ghost Controller
            string ghostCtrlPath = $"{AnimationsDir}/GhostAnimatorController.controller";
            AnimatorController ghostCtrl = AnimatorController.CreateAnimatorControllerAtPath(ghostCtrlPath);
            ghostCtrl.parameters = new AnimatorControllerParameter[0];
            ghostCtrl.AddParameter("Direction", AnimatorControllerParameterType.Int);
            ghostCtrl.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

            var rootSm = ghostCtrl.layers[0].stateMachine;

            string[] dirs = { "Down", "Left", "Right", "Up" };
            AnimatorState[] idleStates = new AnimatorState[4];
            AnimatorState[] walkStates = new AnimatorState[4];

            for (int i = 0; i < 4; i++)
            {
                string dir = dirs[i];
                AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Ghost_Idle_{dir}.anim");
                AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Ghost_Walk_{dir}.anim");

                idleStates[i] = rootSm.AddState($"Idle_{dir}");
                idleStates[i].motion = idleClip;

                walkStates[i] = rootSm.AddState($"Walk_{dir}");
                walkStates[i].motion = walkClip;
            }

            rootSm.defaultState = idleStates[0]; // Default Idle_Down

            for (int i = 0; i < 4; i++)
            {
                var idleToWalk = idleStates[i].AddTransition(walkStates[i]);
                idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
                idleToWalk.AddCondition(AnimatorConditionMode.Equals, i, "Direction");
                idleToWalk.hasExitTime = false;
                idleToWalk.duration = 0f;

                var walkToIdle = walkStates[i].AddTransition(idleStates[i]);
                walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
                walkToIdle.hasExitTime = false;
                walkToIdle.duration = 0f;

                for (int j = 0; j < 4; j++)
                {
                    if (i == j) continue;

                    var anyWalk = walkStates[i].AddTransition(walkStates[j]);
                    anyWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
                    anyWalk.AddCondition(AnimatorConditionMode.Equals, j, "Direction");
                    anyWalk.hasExitTime = false;
                    anyWalk.duration = 0f;

                    var anyIdle = idleStates[i].AddTransition(idleStates[j]);
                    anyIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
                    anyIdle.AddCondition(AnimatorConditionMode.Equals, j, "Direction");
                    anyIdle.hasExitTime = false;
                    anyIdle.duration = 0f;
                }
            }

            EditorUtility.SetDirty(ghostCtrl);

            // 2. ShadeStalker Controller
            string stalkerCtrlPath = $"{AnimationsDir}/ShadeStalkerController.controller";
            AnimatorController stalkerCtrl = AnimatorController.CreateAnimatorControllerAtPath(stalkerCtrlPath);
            stalkerCtrl.parameters = new AnimatorControllerParameter[0];
            stalkerCtrl.AddParameter("Direction", AnimatorControllerParameterType.Int);
            stalkerCtrl.AddParameter("IsChasing", AnimatorControllerParameterType.Bool);

            var stalkerSm = stalkerCtrl.layers[0].stateMachine;
            AnimatorState[] chaseStates = new AnimatorState[4];

            for (int i = 0; i < 4; i++)
            {
                string dir = dirs[i];
                AnimationClip chaseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Stalker_Chase_{dir}.anim");
                chaseStates[i] = stalkerSm.AddState($"Chase_{dir}");
                chaseStates[i].motion = chaseClip;
            }
            stalkerSm.defaultState = chaseStates[0];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == j) continue;
                    var t = chaseStates[i].AddTransition(chaseStates[j]);
                    t.AddCondition(AnimatorConditionMode.Equals, j, "Direction");
                    t.hasExitTime = false;
                    t.duration = 0f;
                }
            }
            EditorUtility.SetDirty(stalkerCtrl);

            // 3. Soul Controller
            string soulCtrlPath = $"{AnimationsDir}/SoulController.controller";
            AnimatorController soulCtrl = AnimatorController.CreateAnimatorControllerAtPath(soulCtrlPath);
            var soulSm = soulCtrl.layers[0].stateMachine;
            AnimationClip hoverClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsDir}/Soul_Hover.anim");
            var hoverState = soulSm.AddState("Hover");
            hoverState.motion = hoverClip;
            soulSm.defaultState = hoverState;
            EditorUtility.SetDirty(soulCtrl);

            Debug.Log("[W1SPL1NGS] Generated 3 Animator Controllers.");
        }

        [MenuItem("Tools/W1SPL1NGS/5. Create Prefabs Only")]
        public static void CreatePrefabs()
        {
            EnsureDirectory(PrefabsDir);

            var ghostSprites = LoadOrderedSpritesList($"{SpritesDir}/ghost_main_8x4.png", "Ghost");
            var hatSprites = LoadOrderedSpritesList($"{SpritesDir}/cosmetic_hat_witch_8x4.png", "Hat_Witch");
            var wingsSprites = LoadOrderedSpritesList($"{SpritesDir}/cosmetic_wings_moth_8x4.png", "Wings_Moth");
            var handsSprites = LoadOrderedSpritesList($"{SpritesDir}/cosmetic_hands_8x4.png", "Hands");
            var stalkerSprites = LoadSpritesList($"{SpritesDir}/shade_stalker_4x4.png");
            var soulSprites = LoadSpritesList($"{SpritesDir}/soul_collectible_6x1.png");

            var ghostCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/GhostAnimatorController.controller");
            var stalkerCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/ShadeStalkerController.controller");
            var soulCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimationsDir}/SoulController.controller");
            var particleMat = GetOrCreateParticleMaterial();

            // 1. GhostPlayer Prefab
            GameObject ghostRoot = new GameObject("GhostPlayer");
            var rb = ghostRoot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = ghostRoot.AddComponent<CircleCollider2D>();
            col.radius = 0.38f;
            col.offset = new Vector2(0f, 0.2f);

            var sortingGroup = ghostRoot.AddComponent<SortingGroup>();
            sortingGroup.sortingOrder = 0;

            var controller = ghostRoot.AddComponent<GhostController>();
            var ghostAnim = ghostRoot.AddComponent<GhostAnimator>();
            var loadout = ghostRoot.AddComponent<CosmeticLoadout>();
            var syncer = ghostRoot.AddComponent<CosmeticSynchronizer>();

            // Single moving VisualRoot
            GameObject visualRootGo = new GameObject("VisualRoot");
            visualRootGo.transform.SetParent(ghostRoot.transform, false);

            var ghostAnimatorComp = visualRootGo.AddComponent<Animator>();
            ghostAnimatorComp.runtimeAnimatorController = ghostCtrl;

            // Shadow (Sorting Order -20)
            GameObject shadowGo = new GameObject("Shadow");
            shadowGo.transform.SetParent(visualRootGo.transform, false);
            shadowGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            var shadowSr = shadowGo.AddComponent<SpriteRenderer>();
            shadowSr.sortingOrder = -20;

            // WingsRenderer (Sorting Order -10)
            GameObject wingsGo = new GameObject("WingsRenderer");
            wingsGo.transform.SetParent(visualRootGo.transform, false);
            wingsGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var wingsSr = wingsGo.AddComponent<SpriteRenderer>();
            wingsSr.sortingOrder = -10;
            if (wingsSprites.Count > 0) wingsSr.sprite = wingsSprites[0];

            // Dust Trail: EffectRenderer (Sorting Order -5: UNDER the character body)
            // Placed at center (x: 0) and at the bottom/tail "đít" of the ghost body (y: -0.05)
            GameObject effectGo = new GameObject("EffectRenderer");
            effectGo.transform.SetParent(visualRootGo.transform, false);
            effectGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            CreateWispParticleSystem(effectGo, particleMat);

            // BodyRenderer (Sorting Order 0)
            GameObject bodyGo = new GameObject("BodyRenderer");
            bodyGo.transform.SetParent(visualRootGo.transform, false);
            var bodySr = bodyGo.AddComponent<SpriteRenderer>();
            bodySr.sortingOrder = 0;
            if (ghostSprites.Count > 0) bodySr.sprite = ghostSprites[0];

            // Cute Hands Renderer (Sorting Order 2: ON TOP of the body silhouette)
            GameObject handsGo = new GameObject("HandsRenderer");
            handsGo.transform.SetParent(visualRootGo.transform, false);
            handsGo.transform.localPosition = Vector3.zero;
            var handsSr = handsGo.AddComponent<SpriteRenderer>();
            handsSr.sortingOrder = 2;
            if (handsSprites.Count > 0) handsSr.sprite = handsSprites[0];

            // HatRenderer (Sorting Order 10)
            GameObject hatGo = new GameObject("HatRenderer");
            hatGo.transform.SetParent(visualRootGo.transform, false);
            hatGo.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            var hatSr = hatGo.AddComponent<SpriteRenderer>();
            hatSr.sortingOrder = 10;
            if (hatSprites.Count > 0) hatSr.sprite = hatSprites[0];

            // Wire components
            var soController = new SerializedObject(controller);
            soController.FindProperty("visualRoot").objectReferenceValue = visualRootGo.transform;
            soController.ApplyModifiedProperties();

            var soGhostAnim = new SerializedObject(ghostAnim);
            soGhostAnim.FindProperty("animator").objectReferenceValue = ghostAnimatorComp;
            soGhostAnim.FindProperty("bodyRenderer").objectReferenceValue = bodySr;
            var propBodySprites = soGhostAnim.FindProperty("bodySprites");
            propBodySprites.arraySize = ghostSprites.Count;
            for (int i = 0; i < ghostSprites.Count; i++)
            {
                propBodySprites.GetArrayElementAtIndex(i).objectReferenceValue = ghostSprites[i];
            }
            soGhostAnim.ApplyModifiedProperties();

            var soLoadout = new SerializedObject(loadout);
            soLoadout.FindProperty("hatRenderer").objectReferenceValue = hatSr;
            soLoadout.FindProperty("wingsRenderer").objectReferenceValue = wingsSr;
            soLoadout.FindProperty("handsRenderer").objectReferenceValue = handsSr;
            soLoadout.ApplyModifiedProperties();

            // Wire and serialize all 32 sprite lookups for Hat, Wings, and Hands
            var soSyncer = new SerializedObject(syncer);
            soSyncer.FindProperty("bodyRenderer").objectReferenceValue = bodySr;
            soSyncer.FindProperty("hatRenderer").objectReferenceValue = hatSr;
            soSyncer.FindProperty("wingsRenderer").objectReferenceValue = wingsSr;
            soSyncer.FindProperty("handsRenderer").objectReferenceValue = handsSr;
            soSyncer.FindProperty("hatOffset").vector3Value = new Vector3(0f, 0.22f, 0f);
            soSyncer.FindProperty("wingsOffset").vector3Value = new Vector3(0f, 0.05f, 0f);
            soSyncer.FindProperty("handsOffset").vector3Value = Vector3.zero;

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

            var propHands = soSyncer.FindProperty("handsSprites");
            propHands.arraySize = handsSprites.Count;
            for (int i = 0; i < handsSprites.Count; i++)
            {
                propHands.GetArrayElementAtIndex(i).objectReferenceValue = handsSprites[i];
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

            PrefabUtility.SaveAsPrefabAsset(stalkerGo, $"{PrefabsDir}/ShadeStalker.prefab");
            GameObject.DestroyImmediate(stalkerGo);

            // 3. SoulCollectible Prefab
            GameObject soulGo = new GameObject("SoulCollectible");
            var soulSr = soulGo.AddComponent<SpriteRenderer>();
            soulSr.sortingOrder = 0;
            if (soulSprites.Count > 0) soulSr.sprite = soulSprites[0];

            var soulAnimator = soulGo.AddComponent<Animator>();
            soulAnimator.runtimeAnimatorController = soulCtrl;

            var soulCol = soulGo.AddComponent<CircleCollider2D>();
            soulCol.isTrigger = true;
            soulCol.radius = 0.35f; // Exact physical touch radius

            soulGo.AddComponent<SoulCollectible>();

            GameObject soulAura = new GameObject("GlowAura");
            soulAura.transform.SetParent(soulGo.transform, false);
            CreateGlowParticleSystem(soulAura, new Color(0.75f, 0.91f, 1f, 0.4f), particleMat);

            PrefabUtility.SaveAsPrefabAsset(soulGo, $"{PrefabsDir}/SoulCollectible.prefab");
            GameObject.DestroyImmediate(soulGo);

            Debug.Log("[W1SPL1NGS] Prefabs regenerated with HD sprites, multi-path animation clips, and exact collider sizes.");
        }

        private static ParticleSystem CreateWispParticleSystem(GameObject parent, Material particleMat)
        {
            var ps = parent.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 0.8f;
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.91f, 1f, 0.7f), new Color(0.78f, 0.76f, 1f, 0.5f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.06f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.75f, 0.91f, 1f), 0f), new GradientColorKey(new Color(0.78f, 0.76f, 1f), 1f) },
                new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 0.8f;

            var psr = parent.GetComponent<ParticleSystemRenderer>();
            psr.sortingOrder = -5; // Behind the character body (which is 0)
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
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var psr = parent.GetComponent<ParticleSystemRenderer>();
            psr.sortingOrder = -1;
            if (particleMat != null) psr.material = particleMat;

            return ps;
        }

        [MenuItem("Tools/W1SPL1NGS/6. Build ArtSandbox Scene Only")]
        public static void BuildSandboxScene()
        {
            EnsureDirectory(ScenesDir);
            string scenePath = $"{ScenesDir}/ArtSandbox.unity";

            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );

            var particleMat = GetOrCreateParticleMaterial();

            // 1. Main Camera with CameraFollow
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.10f, 1.0f); // Atmospheric dark ground
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            var camFollow = camGo.AddComponent<CameraFollow>();
            camGo.AddComponent<AudioListener>();

            // 2. Global 2D Light
            GameObject lightGo = new GameObject("Global Light 2D");
            var light2D = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2D.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light2D.color = new Color(0.90f, 0.90f, 0.96f, 1.0f);
            light2D.intensity = 1.0f;

            // 3. Atmospheric Particle Fog
            GameObject fogGo = new GameObject("Environment_Fog");
            fogGo.transform.position = Vector3.zero;
            var fogPs = fogGo.AddComponent<ParticleSystem>();
            var fMain = fogPs.main;
            fMain.loop = true;
            fMain.startLifetime = 6.0f;
            fMain.startSpeed = 0.15f;
            fMain.startSize = new ParticleSystem.MinMaxCurve(2.0f, 4.0f);
            fMain.startColor = new Color(0.15f, 0.15f, 0.20f, 0.08f);
            fMain.simulationSpace = ParticleSystemSimulationSpace.World;
            var fEmission = fogPs.emission;
            fEmission.rateOverTime = 12f;
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

        private static List<Sprite> LoadOrderedSpritesList(string atlasPath, string prefix)
        {
            Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
            foreach (var asset in assets)
            {
                if (asset is Sprite s && s.name.StartsWith(prefix))
                {
                    dict[s.name] = s;
                }
            }

            string[] dirs = { "Down", "Left", "Right", "Up" };
            List<Sprite> list = new List<Sprite>(32);
            for (int d = 0; d < 4; d++)
            {
                string dir = dirs[d];
                for (int f = 0; f < 4; f++)
                {
                    string name = $"{prefix}_{dir}_Idle_{f}";
                    if (dict.TryGetValue(name, out Sprite s)) list.Add(s);
                    else Debug.LogWarning($"[W1SPL1NGS] Missing sprite {name}");
                }
                for (int f = 0; f < 4; f++)
                {
                    string name = $"{prefix}_{dir}_Walk_{f}";
                    if (dict.TryGetValue(name, out Sprite s)) list.Add(s);
                    else Debug.LogWarning($"[W1SPL1NGS] Missing sprite {name}");
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
