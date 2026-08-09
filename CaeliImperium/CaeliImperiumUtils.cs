using BepInEx;
using BepInEx.Configuration;
using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.Items;
using EntityStates;
using Newtonsoft.Json;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using static CaeliImperium.CaeliImperiumContent;
using static CaeliImperium.CaeliImperiumPlugin;
using static CaeliImperium.CaeliImperiumUtils;
using static R2API.DotAPI;
using static RoR2.CombatDirector;

namespace CaeliImperium
{
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
        public static DotController.DotDef CreateDOT(BuffDef buffDef, out DotController.DotIndex dotIndex , bool resetTimerOnAdd, float interval, float damageCoefficient, DamageColorIndex damageColorIndex, CustomDotBehaviour customDotBehaviour = null, CustomDotVisual customDotVisual = null, CustomDotDamageEvaluation customDotDamageEvaluation = null, Action<DotController.DotDef> onDOTAdded = null)
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
            return PlayerCharacterMasterController.instances?[0];
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
            if (overrideValueIfDefaultValueChanged && BrynzaAPI.BrynzaAPI.defaultConfigValues.TryGetValue(configFile, out Dictionary<ConfigDefinition, string> keyValuePairs) &&
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
                    if (!enableConfig.Value) return null;
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
        public static CharacterBody HandleBody(GameObject gameObject)
        {
            CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
            if (!characterBody._defaultCrosshairPrefab) characterBody._defaultCrosshairPrefab = CaeliImperiumAssets.defaultCrosshair;
            CameraTargetParams cameraTargetParams = gameObject.GetComponent<CameraTargetParams>();
            if (cameraTargetParams && !cameraTargetParams.cameraParams) cameraTargetParams.cameraParams = CaeliImperiumAssets.defaultCharacterCameraParams;
            ModelLocator modelLocator = gameObject.GetComponent<ModelLocator>();
            if (!modelLocator)
            {
                FootstepHandler footstepHandler = modelLocator._modelTransform.GetComponent<FootstepHandler>();
                if (!footstepHandler && !footstepHandler.footstepDustPrefab) footstepHandler.footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
            }
            return characterBody;
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
            if (candidates == null || candidates.Length == 0)
                return null;

            if (string.IsNullOrEmpty(target))
                return candidates[0];

            string mostSimilar = candidates[0];
            int minDistance = int.MaxValue;

            foreach (string candidate in candidates)
            {
                if (candidate == null)
                    continue;

                int distance = LevenshteinDistance(target.ToLower(), candidate.ToLower());

                if (distance < minDistance)
                {
                    minDistance = distance;
                    mostSimilar = candidate;
                }
            }

            return mostSimilar;
        }
        public static string FindBestMatchBySimilarity(string[] sourceArray, string[] candidatesArray)
        {
            if (sourceArray == null || candidatesArray == null || candidatesArray.Length == 0)
                return string.Empty;
            string combinedSource = string.Join("_", sourceArray.Where(s => !string.IsNullOrEmpty(s)));
            return candidatesArray
                .Where(candidate => !string.IsNullOrEmpty(candidate))
                .OrderBy(candidate => LevenshteinDistance(combinedSource, candidate))
                .FirstOrDefault() ?? string.Empty;
        }
        private static int LevenshteinDistance(string s, string t)
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
        public static string FindBestMatch(string[] sourceArray, string[] candidatesArray)
        {
            if (sourceArray == null || candidatesArray == null || candidatesArray.Length == 0) return string.Empty;
            HashSet<char> targetChars = sourceArray
                .Where(s => s != null)
                .SelectMany(s => s)
                .ToHashSet();
            string bestMatch = candidatesArray
                .Where(candidate => candidate != null)
                .OrderByDescending(candidate => candidate.Distinct().Count(c => targetChars.Contains(c)))
                .ThenBy(candidate => candidate.Length)
                .FirstOrDefault();
            return bestMatch ?? string.Empty;
        }
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
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
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));
            return doc.ToString(disableFormatting ? SaveOptions.DisableFormatting : SaveOptions.None);
        }
        public static XDocument ConvertToXDocument(this string xmlString, bool preserveWhitespace = false)
        {
            if (xmlString.IsNullOrWhiteSpace())
                throw new ArgumentException("XML string cannot be null or empty.", nameof(xmlString));
            return XDocument.Parse(xmlString, preserveWhitespace ? LoadOptions.PreserveWhitespace : LoadOptions.None);
        }
    }
}
