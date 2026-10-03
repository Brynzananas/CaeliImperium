using BepInEx;
using BepInEx.Configuration;
using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.Components.Refinery;
using CaeliImperium.Configs;
using CaeliImperium.Interactables;
using CaeliImperium.Items;
using EntityStates;
using Newtonsoft.Json;
using R2API;
using R2API.SpawnCardCloning;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static CaeliImperium.CaeliImperiumContent;
using static CaeliImperium.CaeliImperiumPlugin;
using static CaeliImperium.CaeliImperiumUtils;
using static R2API.DotAPI;
using static RoR2.CombatDirector;

namespace CaeliImperium;

public static class CaeliImperiumUtils
{
    public const string NamePrefix = "_NAME";
    public const string PickupPrefix = "_PICKUP";
    public const string DescriptionPrefix = "_DESCRIPTION";
    public const string LorePrefix = "_LORE";
    public delegate void OnItemAdded(ItemDef itemDef);
    public static int GetEquipmentCount(this CharacterBody characterBody, EquipmentDef equipmentDef) => characterBody.GetEquipmentCount(equipmentDef.equipmentIndex);
    public static int GetEquipmentCount(this CharacterBody characterBody, EquipmentIndex equipmentIndex)
    {
        return 0;
        /*int count = 0;
        Inventory inventory = characterBody.inventory;
        if (inventory) foreach (EquipmentState equipmentState in inventory.equipmentStateSlots) if (equipmentState.equipmentIndex == equipmentIndex) count++;
        ExtraEquipmentSlotBehaviour extraEquipmentSlotBehaviour = characterBody.GetComponent<ExtraEquipmentSlotBehaviour>();
        if (extraEquipmentSlotBehaviour) foreach (EquipmentIndex equipmentIndex1 in extraEquipmentSlotBehaviour.equipments) if (equipmentIndex == equipmentIndex1) count++;
        return count;*/
    }
    public static DotController.DotDef CreateDOT(BuffDef buffDef, out DotController.DotIndex dotIndex, bool resetTimerOnAdd, float interval, float damageCoefficient, DamageColorIndex damageColorIndex, CustomDotBehaviour customDotBehaviour = null, CustomDotVisual customDotVisual = null, CustomDotDamageEvaluation customDotDamageEvaluation = null, Action<DotController.DotDef> onDOTAdded = null)
    {
        DotController.DotDef dotDef = new DotController.DotDef
        {
            resetTimerOnAdd = resetTimerOnAdd,
            interval = interval,
            damageCoefficient = damageCoefficient,
            damageColorIndex = damageColorIndex,
            associatedBuff = buffDef
        };
        dotIndex = DotAPI.RegisterDotDef(dotDef, customDotBehaviour, customDotVisual, customDotDamageEvaluation);
        onDOTAdded?.Invoke(dotDef);
        return dotDef;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SuperRoll(float chance)
    {
        int rolls = (int)MathF.Floor(chance / 100);
        if (Util.CheckRoll(chance - (rolls * 100))) rolls++;
        return rolls;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertAmplificationPercentageIntoReductionPercentage(float amplificationPercentage, float maxChance)
    {
        return (1f - maxChance / (maxChance + amplificationPercentage)) * maxChance;
    }
    public static GenericSkill CopyGenericSkill(GenericSkill genericSkill, CharacterBody bodyToTransferCopiedGenericSkill, string entityStateMachineName, Type mainStateType = null)
    {
        EntityStateMachine entityStateMachine = bodyToTransferCopiedGenericSkill.gameObject.AddComponent<EntityStateMachine>();
        EntityStates.SerializableEntityStateType serializableEntityStateType = new EntityStates.SerializableEntityStateType(mainStateType ?? typeof(EntityStates.Idle));
        entityStateMachine.mainStateType = serializableEntityStateType;
        entityStateMachine.initialStateType = serializableEntityStateType;
        entityStateMachine.customName = entityStateMachineName;
        GenericSkill genericSkill1 = bodyToTransferCopiedGenericSkill.gameObject.AddComponent<GenericSkill>();
        if (genericSkill1 != null)
        {
            genericSkill1._skillFamily = genericSkill.skillFamily;
            genericSkill1.Awake();
            genericSkill1.AssignSkill(genericSkill.baseSkill, false);
            genericSkill1.stateMachine = entityStateMachine;
            genericSkill1.enabled = true;
        }
        //if (bodyToTransferCopiedGenericSkill.skillLocator) bodyToTransferCopiedGenericSkill.skillLocator.AddBonusSkill(genericSkill1);
        return genericSkill1;
    }
    public static EquipmentPicker CreateEquipmentPicker()
    {
        Transform transform = HUD.instancesList[0] && HUD.instancesList[0].mainContainer ? HUD.instancesList[0].mainContainer.transform : null;
        if (transform == null) return null;
        EquipmentPicker equipmentPicker = GameObject.Instantiate(CaeliImperiumAssets.EquipmentPicker, HUD.instancesList[0].mainContainer.transform).GetComponent<EquipmentPicker>();
        return equipmentPicker;
    }
    public static void ModifyCharacterGravityParams(this CharacterBody characterBody, int i)
    {
        ICharacterGravityParameterProvider component = characterBody.GetComponent<ICharacterGravityParameterProvider>();
        if (component != null)
        {
            CharacterGravityParameters gravityParameters = component.gravityParameters;
            gravityParameters.environmentalAntiGravityGranterCount += i;
            component.gravityParameters = gravityParameters;
        }
        ICharacterFlightParameterProvider component2 = characterBody.GetComponent<ICharacterFlightParameterProvider>();
        if (component2 != null)
        {
            CharacterFlightParameters flightParameters = component2.flightParameters;
            flightParameters.channeledFlightGranterCount += i;
            component2.flightParameters = flightParameters;
        }
    }
    public static PlayerCharacterMasterController GetPlayerCharacterMasterController()
    {
        ReadOnlyCollection<PlayerCharacterMasterController> playerCharacterMasterControllers = PlayerCharacterMasterController.instances;
        if (playerCharacterMasterControllers == null || playerCharacterMasterControllers.Count == 0) return null;
        foreach (PlayerCharacterMasterController playerCharacterMasterController in playerCharacterMasterControllers)
        {
            if (!playerCharacterMasterController || !playerCharacterMasterController.hasEffectiveAuthority) continue;
            return playerCharacterMasterController;
        }
        return null;
    }
    public static CharacterBody GetPlayerBody()
    {
        PlayerCharacterMasterController playerCharacterMasterController = GetPlayerCharacterMasterController();
        if (!playerCharacterMasterController) return null;
        return playerCharacterMasterController.body;
    }
    public static ConfigEntry<T> CreateConfig<T>(string section, string key, T defaultValue, string description) => CreateConfig(CaeliImperiumPlugin.configFile, section, key, defaultValue, description, true);
    public static ConfigEntry<T> CreateConfig<T>(string section, string key, T defaultValue, string description, bool overrideValueIfDefaultValueChanged) => CreateConfig(CaeliImperiumPlugin.configFile, section, key, defaultValue, description, overrideValueIfDefaultValueChanged);
    public static ConfigEntry<T> CreateConfig<T>(ConfigFile configFile, string section, string key, T defaultValue, string description, bool overrideValueIfDefaultValueChanged)
    {
        ConfigDefinition configDefinition = new ConfigDefinition(section, key);
        object value = null;
        if (overrideValueIfDefaultValueChanged && CaeliImperiumConfigs.OverrideConfigValuesOnUpdate.Value && BrynzaAPI.BrynzaAPI.defaultConfigValues.TryGetValue(configFile, out Dictionary<ConfigDefinition, string> keyValuePairs) &&
            keyValuePairs.TryGetValue(configDefinition, out string oldDefaultValue) && configFile.OrphanedEntries.TryGetValue(configDefinition, out string oldValue))
        {
            if (oldDefaultValue != defaultValue.ToString() && oldDefaultValue == oldValue) value = defaultValue;
        }
        ConfigDescription configDescription = new ConfigDescription(description);
        ConfigEntry<T> entry = configFile.Bind(configDefinition, defaultValue, configDescription);
        if (value != null) entry.Value = (T)value;
        if (CaeliImperiumPlugin.riskOfOptionsEnabled) ModCompatabilities.RiskOfOptionsCompatability.AddConfig(entry);
        return entry;
    }
    public static T RegisterItemDef<T>(this T itemDef) where T : ItemDef => RegisterItemDef(itemDef, null);
    public static T RegisterItemDef<T>(this T itemDef, Action<T> onItemDefAdded) where T : ItemDef
    {
        if (itemDef is CIItemDef ciItemDef)
        {
            string sectionName = ciItemDef.configName;
            if (!sectionName.IsNullOrWhiteSpace())
            {
                ConfigEntry<bool> enableConfig = CreateConfig(sectionName, "Enable", true, "Enable this item AKA \"" + (itemDef as ScriptableObject).name + "\"?");
                if (!enableConfig.Value) return itemDef;
                ConfigEntry<CIItemDef.ConfigItemTier> tierConfig = CreateConfig(sectionName, "Tier", ciItemDef.configItemTier, "Select tier for this item");
                Sprite sprite;
                ItemTier itemTier;
                switch (tierConfig.Value)
                {
                    case CIItemDef.ConfigItemTier.WhiteCommon:
                        itemTier = ItemTier.Tier1;
                        sprite = ciItemDef.commonTierSprite;
                        break;
                    case CIItemDef.ConfigItemTier.GreenUncommon:
                        itemTier = ItemTier.Tier2;
                        sprite = ciItemDef.uncommonTierSprite;
                        break;
                    case CIItemDef.ConfigItemTier.RedLegendary:
                        itemTier = ItemTier.Tier3;
                        sprite = ciItemDef.legendaryTierSprite;
                        break;
                    default:
                        itemTier = ciItemDef.deprecatedTier;
                        sprite = ciItemDef.pickupIconSprite;
                        break;
                }
                ciItemDef.deprecatedTier = itemTier;
                ciItemDef.pickupIconSprite = sprite;
            }
        }
        items.Add(itemDef);
        onItemDefAdded?.Invoke(itemDef);
        return itemDef;

    }
    public static T RegisterEquipmentDef<T>(this T equipmentDef, Action<T> onEquipmentDefAdded = null) where T : EquipmentDef
    {
        equipments.Add(equipmentDef);
        onEquipmentDefAdded?.Invoke(equipmentDef);
        return equipmentDef;
    }
    public static T RegisterSceneDef<T>(this T sceneDef, Action<T> onSceneDefAdded = null) where T : SceneDef
    {
        scenes.Add(sceneDef);
        onSceneDefAdded?.Invoke(sceneDef);
        return sceneDef;
    }
    public static void AddScene(this SceneCollection sceneCollection, SceneDef sceneDef, SceneDef fallBackSceneDef, float weight)
    {
        List<SceneCollection.SceneEntry> sceneEntries = [.. sceneCollection.sceneEntries];
        sceneEntries.Add(new SceneCollection.SceneEntry
        {
            weight = weight,
            sceneDef = sceneDef,
            fallbackSceneDef = fallBackSceneDef,
        });
        sceneCollection._sceneEntries = sceneEntries.ToArray();
    }
    public static T RegisterEliteDef<T>(this T eliteDef, Action<T> onEliteDefAdded = null) where T : EliteDef
    {
        elites.Add(eliteDef);
        onEliteDefAdded?.Invoke(eliteDef);
        return eliteDef;
    }
    public static T RegisterBuffDef<T>(this T buffDef, Action<T> onBuffDefAdded = null) where T : BuffDef
    {
        buffs.Add(buffDef);
        onBuffDefAdded?.Invoke(buffDef);
        return buffDef;
    }
    public static EffectDef RegisterEffect(this GameObject gameObject, Action<EffectDef> onEffectDefAdded = null)
    {
        EffectDef effectDef = new EffectDef
        {
            prefab = gameObject
        };
        effects.Add(effectDef);
        onEffectDefAdded?.Invoke(effectDef);
        return effectDef;
    }
    public static GameObject RegisterNetworkPrefab(this GameObject gameObject, Action<GameObject> onEffectDefAdded, string configSectionName)
    {
        ConfigEntry<bool> enableConfig = CreateConfig(configSectionName, "Enable", true, "Enable " + gameObject.name + "?");
        if (enableConfig.Value) return gameObject.RegisterNetworkPrefab(onEffectDefAdded);
        return gameObject;
    }
    public static GameObject RegisterNetworkPrefab(this GameObject gameObject, Action<GameObject> onEffectDefAdded = null)
    {
        networkPrefabs.Add(gameObject);
        onEffectDefAdded?.Invoke(gameObject);
        return gameObject;
    }
    public static GameObject RegisterProjectile(this GameObject gameObject, Action<GameObject> onproejctileAdded = null)
    {
        projectiles.Add(gameObject);
        onproejctileAdded?.Invoke(gameObject);
        return gameObject;
    }
    public static GameObject RegisterBody(this GameObject gameObject, Action<GameObject> onBodyAdded, string configSectionName)
    {
        ConfigEntry<bool> enableConfig = CreateConfig(configSectionName, "Enable", true, "Enable " + gameObject.name + " body?");
        if (enableConfig.Value) return gameObject.RegisterBody(onBodyAdded);
        return gameObject;
    }
    public static GameObject RegisterBody(this GameObject gameObject, Action<GameObject> onBodyAdded = null)
    {
        bodies.Add(gameObject);
        onBodyAdded?.Invoke(gameObject);
        return gameObject;
    }
    public static T RegisterSurvivor<T>(this T survivorDef, Action<T> onSurvivorAdded = null) where T : SurvivorDef
    {
        survivors.Add(survivorDef);
        onSurvivorAdded?.Invoke(survivorDef);
        return survivorDef;
    }
    public static void HandleBody(this CharacterBody characterBody) => characterBody.HandleBody(BaseCrosshairType.SimpleDot, BaseCCPType.Standard, BaseFootstepDustType.Generic);
    public static void HandleBody(this CharacterBody characterBody, BaseCrosshairType baseCrosshairType, BaseCCPType baseCCPType, BaseFootstepDustType baseFootstepDustType)
    {
        if (!characterBody._defaultCrosshairPrefab) characterBody.ApplyDefaultCrosshairPrefab(baseCrosshairType);
        CameraTargetParams cameraTargetParams = characterBody.GetComponent<CameraTargetParams>();
        if (cameraTargetParams && !cameraTargetParams.cameraParams) cameraTargetParams.ApplyCameraParams(baseCCPType);
        FootstepHandler footstepHandler = characterBody.GetFootstepHandler();
        if (footstepHandler && !footstepHandler.footstepDustPrefab) footstepHandler.ApplyFootstepDustPrefab(baseFootstepDustType);
    }
    public static CharacterBody HandleBody(this GameObject gameObject) => gameObject.HandleBody(BaseCrosshairType.SimpleDot, BaseCCPType.Standard, BaseFootstepDustType.Generic);
    public static CharacterBody HandleBody(this GameObject gameObject, BaseCrosshairType baseCrosshairType, BaseCCPType baseCCPType, BaseFootstepDustType baseFootstepDustType)
    {
        CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
        if (!characterBody) return null;
        characterBody.HandleBody(baseCrosshairType, baseCCPType, baseFootstepDustType);
        return characterBody;
    }
    public static void ApplyDefaultCrosshairPrefab(this CharacterBody characterBody, BaseCrosshairType baseCrosshairType)
    {
        GameObject gameObject = null;
        switch (baseCrosshairType)
        {
            case BaseCrosshairType.SimpleDot:
                gameObject = CaeliImperiumAssets.SimpleDotCrosshair;
                break;
            default:
                break;
        }
        characterBody._defaultCrosshairPrefab = gameObject;
    }
    public static void ApplyCameraParams(this CharacterBody characterBody, BaseCCPType baseCCPType)
    {
        CameraTargetParams cameraTargetParams = characterBody.GetComponent<CameraTargetParams>();
        if (!cameraTargetParams) return;
        cameraTargetParams.ApplyCameraParams(baseCCPType);
    }
    public static void ApplyCameraParams(this CameraTargetParams cameraTargetParams, BaseCCPType baseCCPType)
    {
        CharacterCameraParams characterCameraParams = null;
        switch (baseCCPType)
        {
            case BaseCCPType.Standard:
                characterCameraParams = CaeliImperiumAssets.StandardCharacterCameraParams;
                break;
            case BaseCCPType.StandardHuge:
                characterCameraParams = CaeliImperiumAssets.StandardHugeCharacterCameraParams;
                break;
            default:
                break;
        }
        cameraTargetParams.cameraParams = characterCameraParams;
    }
    public static void ApplyFootstepDustPrefab(this CharacterBody characterBody, BaseFootstepDustType baseFootstepDustType)
    {
        FootstepHandler footstepHandler = characterBody.GetFootstepHandler();
        if (!footstepHandler) return;
        footstepHandler.ApplyFootstepDustPrefab(baseFootstepDustType);
    }
    public static void ApplyFootstepDustPrefab(this FootstepHandler footstepHandler, BaseFootstepDustType baseFootstepDustType)
    {
        GameObject gameObject = null;
        switch (baseFootstepDustType)
        {
            case BaseFootstepDustType.Generic:
                gameObject = CaeliImperiumAssets.GenericFootstepDust;
                break;
            case BaseFootstepDustType.GenericHuge:
                gameObject = CaeliImperiumAssets.GenericHugeFootstepDust;
                break;
            case BaseFootstepDustType.GenericLarge:
                gameObject = CaeliImperiumAssets.GenericLargeFootstepDust;
                break;
            default:
                break;
        }
        footstepHandler.footstepDustPrefab = gameObject;
    }
    public static void CopyAKBank(this GameObject gameObject, GameObject toCopy)
    {
        AkBank akBank = toCopy.GetComponent<AkBank>();
        if (!akBank) return;
        gameObject.CopyComponent(akBank);
    }
    public static FootstepHandler GetFootstepHandler(this CharacterBody characterBody)
    {
        ModelLocator modelLocator = characterBody.GetComponent<ModelLocator>();
        if (!modelLocator || !modelLocator._modelTransform) return null;
        return modelLocator._modelTransform.GetComponent<FootstepHandler>();
    }
    public static GameObject RegisterMaster(this GameObject gameObject, Action<GameObject> onMasterAdded = null)
    {
        masters.Add(gameObject);
        onMasterAdded?.Invoke(gameObject);
        return gameObject;
    }
    public static T RegisterSkillDef<T>(this T skillDef, Action<T> onSkillDefAdded = null) where T : SkillDef
    {
        skills.Add(skillDef);
        onSkillDefAdded?.Invoke(skillDef);
        return skillDef;
    }
    public static T1 RegisterSpawnCardClone<T1, T2>(this T1 spawnCardClone, Action<T1, T2> onSpawnCardCloneRegistered) where T1 : BaseSpawnCardClone<T2> where T2 : SpawnCard
    {
        spawnCardClone.Register();
        return spawnCardClone;
    }
    public static T RegisterSkillFamily<T>(this T skillFamily, Action<T> onSkillDefAdded = null) where T : SkillFamily
    {
        skillFamilies.Add(skillFamily);
        onSkillDefAdded?.Invoke(skillFamily);
        return skillFamily;
    }
    public static T RegisterExpansionDef<T>(this T expansionsDef, Action<T> onExpansionDefAdded = null) where T : ExpansionDef
    {
        expansions.Add(expansionsDef);
        onExpansionDefAdded?.Invoke(expansionsDef);
        return expansionsDef;
    }
    public static Type RegisterEntityState(this Type entityState, Action<Type> onEntityStateAdded = null)
    {
        states.Add(entityState);
        onEntityStateAdded?.Invoke(entityState);
        return entityState;
    }
    public static float Stack(this int stack, float nonStackValue, float stackValue) => nonStackValue + ((stack - 1) * stackValue);
    public static int Stack(this int stack, int nonStackValue, int stackValue) => nonStackValue + ((stack - 1) * stackValue);
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }
    public static T GetOrAddComponent<T>(this Transform transform) where T : Component
    {
        return transform.gameObject.GetOrAddComponent<T>();
    }
    public static T GetOrAddComponent<T>(this Component component) where T : Component
    {
        return component.gameObject.GetOrAddComponent<T>();
    }
    public static uint PlaySound(string soundString, GameObject gameObject, AkCallbackManager.EventCallback callBack, object cookie)
    {
        if (string.IsNullOrEmpty(soundString))
        {
            return 0U;
        }
        if (gameObject == null)
        {
            return AkSoundEngine.PostEvent(soundString, ulong.MaxValue, (uint)AkCallbackType.AK_Marker, callBack, cookie);
        }
        return AkSoundEngine.PostEvent(soundString, gameObject, (uint)AkCallbackType.AK_Marker, callBack, cookie);
    }
    public static DamageSource GetDamageSource(this BaseSkillState baseSkillState) => baseSkillState.GetDamageSource(DamageSource.NoneSpecified);
    public static DamageSource GetDamageSource(this BaseSkillState baseSkillState, DamageSource fallbackDamageSource)
    {
        if (!baseSkillState.activatorSkillSlot || !baseSkillState.skillLocator) return fallbackDamageSource;
        if (baseSkillState.activatorSkillSlot == baseSkillState.skillLocator.primary) return DamageSource.Primary;
        if (baseSkillState.activatorSkillSlot == baseSkillState.skillLocator.secondary) return DamageSource.Secondary;
        if (baseSkillState.activatorSkillSlot == baseSkillState.skillLocator.utility) return DamageSource.Utility;
        if (baseSkillState.activatorSkillSlot == baseSkillState.skillLocator.special) return DamageSource.Special;
        return fallbackDamageSource;
    }
    public static void AddVelocity(this BaseState baseState, Vector3 velocity)
    {
        if (baseState.characterMotor)
        {
            baseState.characterMotor.velocity += velocity;
        }
        else if (baseState.rigidbody)
        {
            baseState.rigidbody.velocity += velocity;
        }
    }
    public static void SetVelocity(this BaseState baseState, Vector3 velocity)
    {
        if (baseState.characterMotor)
        {
            baseState.characterMotor.velocity = velocity;
        }
        else if (baseState.rigidbody)
        {
            baseState.rigidbody.velocity = velocity;
        }
    }
    public static Vector3 GetVelocity(this BaseState baseState)
    {
        if (baseState.characterMotor)
        {
            return baseState.characterMotor.velocity;
        }
        else if (baseState.rigidbody)
        {
            return baseState.rigidbody.velocity;
        }
        return Vector3.zero;
    }
    public static void AddRootMotion(this BaseState baseState, Vector3 vector3)
    {
        if (baseState.characterMotor)
        {
            baseState.characterMotor.rootMotion += vector3;
        }
        else if (baseState.rigidbody)
        {
            baseState.rigidbody.MovePosition(baseState.rigidbody.position + vector3);
        }
    }
    public static void AddInteger(this Animator animator, string name) => animator.SetInteger(name, animator.GetInteger(name) + 1);
    public static void SubstractInteger(this Animator animator, string name) => animator.SetInteger(name, animator.GetInteger(name) - 1);
    public static Vector3 ToVector3(this float value) => new Vector3(value, value, value);
    public static AnimationCurve QuadraticIn(float timeStart, float valueStart, float timeEnd, float valueEnd)
    {
        float tangent = 2 * (valueEnd - valueStart) / (timeEnd - timeStart);
        Keyframe startKey = new Keyframe(timeStart, valueStart, 0, 0);
        Keyframe endKey = new Keyframe(timeEnd, valueEnd, tangent, 0);
        return new AnimationCurve(startKey, endKey);
    }
    public static AnimationCurve QuadraticOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
    {
        float tangent = 2 * (valueEnd - valueStart) / (timeEnd - timeStart);
        Keyframe startKey = new Keyframe(timeStart, valueStart, 0, tangent);
        Keyframe endKey = new Keyframe(timeEnd, valueEnd, 0, 0);
        return new AnimationCurve(startKey, endKey);
    }
    public static string FindMostSimilar(string target, string[] candidates)
    {
        if (candidates == null || candidates.Length == 0) return null;
        if (target.IsNullOrWhiteSpace()) return candidates[0];
        string mostSimilar = candidates[0];
        int minDistance = int.MaxValue;
        foreach (string candidate in candidates)
        {
            if (candidate == null) continue;
            int distance = LevenshteinDistance(target.ToLower(), candidate.ToLower());
            if (distance < minDistance)
            {
                minDistance = distance;
                mostSimilar = candidate;
            }
        }
        return mostSimilar;
    }
    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length, m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        if (n == 0) return m;
        if (m == 0) return n;
        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
    public static string SHA256Encode(this string value)
    {
        StringBuilder Sb = new StringBuilder();

        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            Byte[] result = hash.ComputeHash(enc.GetBytes(value));

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));
        }

        return Sb.ToString();
    }
    public static string ConvertToString(this XDocument doc, bool disableFormatting = false)
    {
        if (doc == null) throw new ArgumentNullException(nameof(doc));
        return doc.ToString(disableFormatting ? SaveOptions.DisableFormatting : SaveOptions.None);
    }
    public static XDocument ConvertToXDocument(this string xmlString, bool preserveWhitespace = false)
    {
        if (xmlString.IsNullOrWhiteSpace()) throw new ArgumentException("XML string cannot be null or empty.", nameof(xmlString));
        return XDocument.Parse(xmlString, preserveWhitespace ? LoadOptions.PreserveWhitespace : LoadOptions.None);
    }
    public static Vector3 BezierGetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * oneMinusT * p0 + 3f * oneMinusT * oneMinusT * t * p1 + 3f * oneMinusT * t * t * p2 + t * t * t * p3;
    }
    public static Vector3 BezierGetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return 3f * oneMinusT * oneMinusT * (p1 - p0) + 6f * oneMinusT * t * (p2 - p1) + 3f * t * t * (p3 - p2);
    }
    public static Ray GetAimRay(this CharacterBody characterBody)
    {
        if (characterBody.inputBank) return new Ray(characterBody.inputBank.aimOrigin, characterBody.inputBank.aimDirection);
        return new Ray(characterBody.transform.position, characterBody.transform.forward);
    }
    public static void PlayCrossfade(this Animator modelAnimator, string layerName, string animationStateName, string playbackRateParam, float duration, float crossfadeDuration) => modelAnimator.PlayCrossfade(layerName, animationStateName, playbackRateParam, duration, crossfadeDuration, false);
    public static void PlayCrossfade(this Animator modelAnimator, string layerName, string animationStateName, string playbackRateParam, float duration, float crossfadeDuration, bool durationIsMultiplier)
    {
        if (modelAnimator)
        {
            modelAnimator.speed = 1f;
            modelAnimator.Update(0f);
            int layerIndex = modelAnimator.GetLayerIndex(layerName);
            modelAnimator.SetFloat(playbackRateParam, 1f);
            modelAnimator.CrossFadeInFixedTime(animationStateName, crossfadeDuration, layerIndex);
            modelAnimator.Update(0f);
            float length = modelAnimator.GetNextAnimatorStateInfo(layerIndex).length;
            modelAnimator.SetFloat(playbackRateParam, duration > 0 ? durationIsMultiplier ? duration : length / duration : 1f);
        }
    }
    public static void PlayCrossfade(this Animator modelAnimator, string layerName, int animationStateNameHash, int playbackRateParamHash, float duration, float crossfadeDuration) => modelAnimator.PlayCrossfade(layerName, animationStateNameHash, playbackRateParamHash, duration, crossfadeDuration, false);
    public static void PlayCrossfade(this Animator modelAnimator, string layerName, int animationStateNameHash, int playbackRateParamHash, float duration, float crossfadeDuration, bool durationIsMultiplier)
    {
        if (modelAnimator)
        {
            modelAnimator.speed = 1f;
            modelAnimator.Update(0f);
            int layerIndex = modelAnimator.GetLayerIndex(layerName);
            modelAnimator.SetFloat(playbackRateParamHash, 1f);
            modelAnimator.CrossFadeInFixedTime(animationStateNameHash, crossfadeDuration, layerIndex);
            modelAnimator.Update(0f);
            float length = modelAnimator.GetNextAnimatorStateInfo(layerIndex).length;
            modelAnimator.SetFloat(playbackRateParamHash, duration > 0 ? durationIsMultiplier ? duration : length / duration : 1f);
        }
    }
    public static void PlayCrossfade(this Animator modelAnimator, string layerName, string animationStateName, float crossfadeDuration)
    {
        if (modelAnimator)
        {
            modelAnimator.speed = 1f;
            modelAnimator.Update(0f);
            int layerIndex = modelAnimator.GetLayerIndex(layerName);
            modelAnimator.CrossFadeInFixedTime(animationStateName, crossfadeDuration, layerIndex);
        }
    }
    public static void PlayCrossfade(this Animator modelAnimator, string layerName, int animationStateHash, float crossfadeDuration)
    {
        if (modelAnimator)
        {
            modelAnimator.speed = 1f;
            modelAnimator.Update(0f);
            int layerIndex = modelAnimator.GetLayerIndex(layerName);
            modelAnimator.CrossFadeInFixedTime(animationStateHash, crossfadeDuration, layerIndex);
        }
    }
    public static void AddMusic(string playSystem, string bankName)
    {
        var e = new SoundAPI.Music.CustomMusicData();
        e.BanksFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "soundbanks");
        e.BepInPlugin = CaeliImperiumPlugin.PluginInfo.Metadata;
        e.InitBankName = "CaeliImperiumMusicInit";
        e.PlayMusicSystemEventName = playSystem;
        e.SoundBankName = bankName;
        SoundAPI.Music.Add(e);
    }
    public static void LogCurrentAkSoundEngineState(uint groupId)
    {
        AkSoundEngine.GetState(groupId, out var state);
        CaeliImperiumPlugin.instance.Logger.LogMessage("Group: " + groupId);
        CaeliImperiumPlugin.instance.Logger.LogMessage("State: " + state);
    }
    public static List<Transform> GetFilteredChildren(Transform parent, Predicate<Transform> filter = null, bool includeSelf = false, bool recursive = true)
    {
        List<Transform> result = [];
        if (!parent) return result;
        if (includeSelf) EvaluateAndAdd(parent, filter, result);
        CollectChildrenInternal(parent, filter, result, recursive);
        return result;
    }
    private static void CollectChildrenInternal(Transform currentParent, Predicate<Transform> filter, List<Transform> result, bool recursive)
    {
        foreach (Transform child in currentParent)
        {
            EvaluateAndAdd(child, filter, result);
            if (recursive) CollectChildrenInternal(child, filter, result, true);
        }
    }
    private static void EvaluateAndAdd(Transform target, Predicate<Transform> filter, List<Transform> result)
    {
        bool shouldAdd = filter == null || filter.Invoke(target);
        if (shouldAdd) result.Add(target);
    }
    public static BasicBezierSpline CreateSplineFromSegments(GameObject parentObject, BezierSegmentData[] bezierSegmentDatas, out List<GameObject> allControlPoints)
    {
        if (bezierSegmentDatas == null || bezierSegmentDatas.Length == 0)
        {
            allControlPoints = null;
            return null;
        }
        BasicBezierSpline basicBezierSpline = parentObject.GetOrAddComponent<BasicBezierSpline>();
        int count = bezierSegmentDatas.Length + 1;
        BasicBezierSplineControlPoint[] controlPoints = new BasicBezierSplineControlPoint[count];
        allControlPoints = [];
        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = new GameObject($"BasicBezierSplineControlPoint_{i}");
            allControlPoints.Add(gameObject);
            gameObject.transform.SetParent(parentObject.transform, false);
            BasicBezierSplineControlPoint basicBezierSplineControlPoint = gameObject.AddComponent<BasicBezierSplineControlPoint>();
            if (i == 0)
            {
                gameObject.transform.position = bezierSegmentDatas[0].p0;
                gameObject.transform.rotation = Quaternion.identity;
                basicBezierSplineControlPoint.forwardVelocity = bezierSegmentDatas[0].p1 - bezierSegmentDatas[0].p0;
                basicBezierSplineControlPoint.backwardVelocity = Vector3.zero;
            }
            else if (i == count - 1)
            {
                BezierSegmentData lastSegment = bezierSegmentDatas[bezierSegmentDatas.Length - 1];
                gameObject.transform.position = lastSegment.p3;
                gameObject.transform.rotation = Quaternion.identity;
                basicBezierSplineControlPoint.forwardVelocity = Vector3.zero;
                basicBezierSplineControlPoint.backwardVelocity = lastSegment.p2 - lastSegment.p3;
            }
            else
            {
                BezierSegmentData previousSegment = bezierSegmentDatas[i - 1];
                BezierSegmentData nextSegment = bezierSegmentDatas[i];
                gameObject.transform.position = nextSegment.p0;
                gameObject.transform.rotation = Quaternion.identity;
                basicBezierSplineControlPoint.backwardVelocity = previousSegment.p2 - previousSegment.p3;
                basicBezierSplineControlPoint.forwardVelocity = nextSegment.p1 - nextSegment.p0;
            }
            controlPoints[i] = basicBezierSplineControlPoint;
        }
        basicBezierSpline.controlPoints = controlPoints;
        basicBezierSpline.BuildKeyFrames();
        return basicBezierSpline;
    }
    //public static CaeliImperiumSave GetOrCreateCaeliImperiumSave(this UserProfile userProfile) => userProfile.GetOrCreateModdedUserProfileSaveData<CaeliImperiumSave>();
    //public static CaeliImperiumSave GetCaeliImperiumSave(this UserProfile userProfile) => userProfile.GetModdedUserProfileSaveData<CaeliImperiumSave>();
    public static void SimulateInteractableSpawnUsingSpawnRules(SceneDirector sceneDirector, InteractableSpawnRules interactableSpawnRules, InteractableSpawnRules defaultInteractableSpawnRules, Action<SceneDirector> actionPerSpawnCount, Action<SceneDirector, int> actionOneTime)
    {
        if (!Run.instance || !SceneInfo.instance) return;
        SceneDef sceneDef = SceneInfo.instance.sceneDef;
        if (!sceneDef) return;
        if (!(SceneInfo.instance.countsAsStage || sceneDef.allowItemsToSpawnObjects)) return;
        InteractableSpawnRules monsterChestSpawnRules = interactableSpawnRules;
        if (monsterChestSpawnRules == null) monsterChestSpawnRules = defaultInteractableSpawnRules;
        if (monsterChestSpawnRules.spawnRules == null) return;
        int spawnCount = 0;
        float spawmChance = 0f;
        Scene scene = SceneManager.GetActiveScene();
        InteractableSpawnRules.SpawnRule spawnRule = monsterChestSpawnRules.GetSpawnRule(Run.instance.stageClearCountInCurrentLoop + 1, scene == null ? null : scene.name, sceneDef);
        spawmChance = spawnRule.spawnChance;
        spawnCount = spawnRule.spawnCount;
        if (spawnCount <= 0 || spawmChance <= 0f) return;
        int successfullSpawnCount = 0;
        for (int i = 0; i < spawnCount; i++)
        {
            if (spawmChance < 100f && !Util.CheckRoll(spawmChance)) continue;
            actionPerSpawnCount?.Invoke(sceneDirector);
            successfullSpawnCount++;
        }
        actionOneTime?.Invoke(sceneDirector, successfullSpawnCount);
    }
    /*public static bool HasCompletedPipelineRefinery()
    {
        PlayerCharacterMasterController playerCharacterMasterController = CaeliImperiumUtils.GetPlayerCharacterMasterController();
        if (!playerCharacterMasterController) return false;
        NetworkUser networkUser = playerCharacterMasterController.networkUser;
        if (!networkUser) return false;
        LocalUser localUser = networkUser.localUser;
        if (localUser == null) return false;
        UserProfile userProfile = localUser.userProfile;
        if (userProfile == null) return false;
        CaeliImperiumSave caeliImperiumSave = userProfile.GetCaeliImperiumSave();
        if (caeliImperiumSave == null) return false;
        if (caeliImperiumSave.pipelineRefineriesCompletedCount > 0) return true;
        return false;
    }*/
    public static ClientToServerMessenger GetClientToServerMessenger(this PlayerCharacterMasterController playerCharacterMasterController)
    {
        if (ClientToServerMessenger.keyValuePairs.TryGetValue(playerCharacterMasterController, out ClientToServerMessenger clientToServerMessenger))
        {
            return clientToServerMessenger;
        }
        return null;
    }
    public static PlayerCharacterMasterController GetPlayerCharacterMasterController(this CharacterBody characterBody)
    {
        CharacterMaster characterMaster = characterBody.master;
        if (!characterMaster) return null;
        return characterMaster.playerCharacterMasterController;
    }
    public static ClientToServerMessenger GetClientToServerMessenger(this CharacterBody characterBody)
    {
        PlayerCharacterMasterController playerCharacterMasterController = characterBody.GetPlayerCharacterMasterController();
        if (!playerCharacterMasterController) return null;
        return playerCharacterMasterController.GetClientToServerMessenger();
    }
    public static ClientToServerMessenger GetClientToServerMessenger() => ClientToServerMessenger.authorityInstance;
    public static PipelineBuilder GetPipelineBuilder(this NetworkInstanceId networkInstanceId)
    {
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return null;
        return gameObject.GetComponent<PipelineBuilder>();
    }
    public static MonsterChestController GetMonsterChestController(this NetworkInstanceId networkInstanceId)
    {
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return null;
        return gameObject.GetComponent<MonsterChestController>();
    }
    public static WellExtractorController GetWellExtractorController(this NetworkInstanceId networkInstanceId)
    {
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return null;
        return gameObject.GetComponent<WellExtractorController>();
    }
    public static CharacterBody GetCharacterBody(this NetworkInstanceId networkInstanceId)
    {
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return null;
        return gameObject.GetComponent<CharacterBody>();
    }
    public static AnimationCurve CreateAnimationCurveEaseInOutPingPong(float timeStart, float valueStart, float timeEnd, float valueEnd)
    {
        AnimationCurve animationCurve = AnimationCurve.EaseInOut(timeStart, valueStart, timeEnd, valueEnd);
        animationCurve.postWrapMode = WrapMode.PingPong;
        animationCurve.preWrapMode = WrapMode.PingPong;
        return animationCurve;
    }
    public static Texture2D CreateReadableTexture(this Texture sourceTexture)
    {
        if (!sourceTexture) return null;
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            sourceTexture.width,
            sourceTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );
        Graphics.Blit(sourceTexture, renderTexture);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
        readableTexture.wrapMode = sourceTexture.wrapMode;
        readableTexture.wrapModeU = sourceTexture.wrapModeU;
        readableTexture.wrapModeV = sourceTexture.wrapModeV;
        readableTexture.wrapModeW = sourceTexture.wrapModeW;
        readableTexture.mipMapBias = sourceTexture.mipMapBias;
        readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTexture;
    }
    public static Texture2D ApplyGradientToTexture(this Texture2D sourceTexture, Gradient gradient)
    {
        if (!sourceTexture || gradient == null) return null;
        bool destroySourceTexture = false;
        if (!sourceTexture.isReadable)
        {
            destroySourceTexture = true;
            sourceTexture = sourceTexture.CreateReadableTexture();
        }
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        Color[] pixels = sourceTexture.GetPixels();
        Color[] resultPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color currentPixel = pixels[i];
            float luminance = (currentPixel.r * 0.299f) + (currentPixel.g * 0.587f) + (currentPixel.b * 0.114f);
            Color gradientColor = gradient.Evaluate(luminance);
            gradientColor.a *= currentPixel.a;
            resultPixels[i] = gradientColor;
        }
        Texture2D resultTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        resultTexture.wrapMode = sourceTexture.wrapMode;
        resultTexture.wrapModeU = sourceTexture.wrapModeU;
        resultTexture.wrapModeV = sourceTexture.wrapModeV;
        resultTexture.wrapModeW = sourceTexture.wrapModeW;
        resultTexture.mipMapBias = sourceTexture.mipMapBias;
        resultTexture.SetPixels(resultPixels);
        resultTexture.Apply();
        if (destroySourceTexture) GameObject.Destroy(sourceTexture);
        return resultTexture;
    }
    public static Vector3 GetColliderCenter(this Collider collider) => collider.bounds.center;
    public static ChildLocator GetModelChildLocator(this CharacterBody characterBody)
    {
        ModelLocator modelLocator = characterBody.modelLocator;
        if (!modelLocator) return null;
        return modelLocator.modelChildLocator;
    }
    public static void ReplaceChild(this ChildLocator childLocator, string name, Transform newTransform)
    {
        if (childLocator.FindChild(name))
        {
            for (int i = 0; i < childLocator.transformPairs.Length; i++)
            {
                ref ChildLocator.NameTransformPair pair = ref childLocator.transformPairs[i];
                if (!(pair.name == name)) continue;
                pair.transform = newTransform;
            }
        }
    }
    public static void SuperStun(this CharacterBody characterBody, float duration)
    {
        EntityStateMachine[] entityStateMachines = characterBody.gameObject.GetComponents<EntityStateMachine>();
        foreach (EntityStateMachine entityStateMachine in entityStateMachines)
        {
            if (entityStateMachine.customName == "Body")
            {
                if (entityStateMachine.state is StunState)
                {
                    StunState stunState = entityStateMachine.state as StunState;
                    if (stunState.timeRemaining < duration) stunState.ExtendStun(duration - stunState.timeRemaining);
                }
                else
                {
                    StunState stunState2 = new StunState();
                    stunState2.stunDuration = duration;
                    entityStateMachine.SetInterruptState(stunState2, InterruptPriority.Stun);
                }
            }
            else
            {
                entityStateMachine.SetStateToMain();
            }
        }
    }
    public static Collider GetClosestColliderOnLine(List<Collider> colliders, Vector3 direction, Vector3 origin)
    {
        if (colliders == null || colliders.Count == 0) return null;
        Vector3 normalizedDirection = direction.normalized;
        if (normalizedDirection == Vector3.zero) return null;
        Collider closestCollider = null;
        float minSquareDistance = float.MaxValue;
        foreach (Collider collider in colliders)
        {
            if (!collider) continue;
            Vector3 targetPosition = collider.bounds.center;
            Vector3 originToTarget = targetPosition - origin;
            float projectionDistance = Vector3.Dot(originToTarget, normalizedDirection);
            projectionDistance = Mathf.Max(0f, projectionDistance);
            Vector3 pointOnLine = origin + normalizedDirection * projectionDistance;
            float sqrDistance = (targetPosition - pointOnLine).sqrMagnitude;
            if (sqrDistance < minSquareDistance)
            {
                minSquareDistance = sqrDistance;
                closestCollider = collider;
            }
        }
        return closestCollider;
    }
    public static bool HasRequiredStockAndDelay(this GenericSkill genericSkill) => genericSkill.skillDef ? genericSkill.skillDef.HasRequiredStockAndDelay(genericSkill) : false;
    public static List<string> GetLayerNames(LayerMask mask)
    {
        List<string> layerNames = [];
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!layerName.IsNullOrWhiteSpace()) layerNames.Add(layerName);
            }
        }
        return layerNames;
    }
}

[Serializable]
public struct BezierSegmentData : IEquatable<BezierSegmentData>
{
    public static BezierSegmentData Empty => new BezierSegmentData();
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public BezierSegmentData()
    {

    }
    public BezierSegmentData(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }
    public bool Equals(BezierSegmentData other) => other.p0 == p0 && other.p1 == p1 && other.p2 == p2 && other.p3 == p3;
}
