using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperium.Main;
using static CaeliImperium.ContentPacks;
using System.Runtime.CompilerServices;
using R2API;
using static R2API.DotAPI;
using UnityEngine.EventSystems;
using static CaeliImperium.Utils;
using System.Linq;
using static RoR2.CombatDirector;
using RoR2.UI;

namespace CaeliImperium
{
    public static class Utils
    {
        public const string NamePrefix = "_NAME";
        public const string PickupPrefix = "_PICKUP";
        public const string DescriptionPrefix = "_DESCRIPTION";
        public const string LorePrefix = "_LORE";
        public delegate void OnItemAdded(ItemDef itemDef);
        public static int GetEquipmentCount(this CharacterBody characterBody, EquipmentDef equipmentDef) => characterBody.GetEquipmentCount(equipmentDef.equipmentIndex);
        public static int GetEquipmentCount(this CharacterBody characterBody, EquipmentIndex equipmentIndex)
        {
            int count = 0;
            Inventory inventory = characterBody.inventory;
            if (inventory) foreach (EquipmentState equipmentState in inventory.equipmentStateSlots) if (equipmentState.equipmentIndex == equipmentIndex) count++;
            ExtraEquipmentSlotBehaviour extraEquipmentSlotBehaviour = characterBody.GetComponent<ExtraEquipmentSlotBehaviour>();
            if (extraEquipmentSlotBehaviour) foreach (EquipmentIndex equipmentIndex1 in extraEquipmentSlotBehaviour.equipments) if (equipmentIndex == equipmentIndex1) count++;
            return count;
        }
        public static ItemDef CreateItem(string name, Sprite pickupIcon, GameObject pickupObject, bool canRemove, ItemTier itemTier, ItemTag[] itemTags = null, ExpansionDef expansionDef = null, OnItemAdded onItemAdded = null)
        {
            ItemDef itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = ModPrefix + name;
            string name2 = name.ToUpper().Replace(" ", "");
            string nameToken = ModPrefix + "_" + name2 + NamePrefix;
            LanguageAPI.Add(nameToken, name);
            itemDef.nameToken = nameToken;
            itemDef.descriptionToken = ModPrefix + "_" + name2 + DescriptionPrefix;
            itemDef.loreToken = ModPrefix + "_" + name2 + LorePrefix;
            itemDef.pickupToken = ModPrefix + "_" + name2 + PickupPrefix;
            itemDef.deprecatedTier = itemTier;
            itemDef.pickupIconSprite = pickupIcon;
            itemDef.pickupModelPrefab = pickupObject;
            itemDef.canRemove = canRemove;
            itemDef.requiredExpansion = expansionDef;
            itemDef.tags = itemTags;
            items.Add(itemDef);
            if (onItemAdded != null) onItemAdded(itemDef);
            return itemDef;
        }
        public delegate void OnEquipmentAdded(EquipmentDef equipmentDef);
        public static EquipmentDef CreateEquipment(string name, Sprite pickupIcon, GameObject pickupObject, bool appearsInMultiplayer, bool appearsInSinglePlayer, float cooldown, bool canDrop = true, ExpansionDef expansionDef = null, OnEquipmentAdded onEquipmentAdded = null)
        {
            EquipmentDef equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
            equipmentDef.name = ModPrefix + name;
            string name2 = name.ToUpper().Replace(" ", "");
            string nameToken = ModPrefix + "_" + name2 + NamePrefix;
            LanguageAPI.Add(nameToken, name);
            equipmentDef.nameToken = nameToken;
            equipmentDef.pickupToken = ModPrefix + "_" + name2 + PickupPrefix;
            equipmentDef.descriptionToken = ModPrefix + "_" + name2 + DescriptionPrefix;
            equipmentDef.loreToken = ModPrefix + "_" + name2 + LorePrefix;
            equipmentDef.pickupIconSprite = pickupIcon;
            equipmentDef.pickupModelPrefab = pickupObject;
            equipmentDef.appearsInMultiPlayer = appearsInMultiplayer;
            equipmentDef.appearsInSinglePlayer = appearsInSinglePlayer;
            equipmentDef.cooldown = cooldown;
            equipmentDef.requiredExpansion = expansionDef;
            equipmentDef.canDrop = canDrop;
            equipments.Add(equipmentDef);
            if (onEquipmentAdded != null) onEquipmentAdded(equipmentDef);
            return equipmentDef;
        }
        public delegate void OnBuffAdded(BuffDef buffDef);
        public static BuffDef CreateBuff(string name, Sprite icon, Color color, bool canStack, bool isDebuff, bool isCooldown, bool isHidden, bool ignoreGrowthNectar, OnBuffAdded onBuffAdded = null)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = ModPrefix + name;
            buffDef.buffColor = color;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.ignoreGrowthNectar = ignoreGrowthNectar;
            buffDef.iconSprite = icon;
            buffDef.isHidden = isHidden;
            buffDef.isCooldown = isCooldown;
            buffs.Add(buffDef);
            return buffDef;
        }
        public delegate void OnEliteAdded(EliteDef eliteDef);
        public static EliteDef CreateElite(string name, BuffDef buffDef, Sprite affixIcon, float equipmentCooldown, float healthBoostCoefficient, float damageBoostCoefficient, int tier, ExpansionDef expansionDef = null, OnEliteAdded onEliteAdded = null)
        {
            string name2 = name.ToUpper().Replace(" ", "");
            EliteDef eliteDef = ScriptableObject.CreateInstance<EliteDef>();
            EquipmentDef equipmentDef = CreateEquipment("Affix" + name, affixIcon, null, true, true, equipmentCooldown, false, expansionDef);
            eliteDef.color = Color.white;
            eliteDef.eliteEquipmentDef = equipmentDef;
            string modifierToken = ModPrefix + name2 + "_MODIFIER";
            eliteDef.modifierToken = modifierToken;
            LanguageAPI.Add(modifierToken, name + " {0}");
            eliteDef.healthBoostCoefficient = healthBoostCoefficient;
            eliteDef.damageBoostCoefficient = damageBoostCoefficient;
            buffDef.eliteDef = eliteDef;
            elites.Add(eliteDef);
            if (onEliteAdded != null) onEliteAdded(eliteDef);
            Hooks.OnCombatDirectorInit += Hooks_OnCombatDirectorInitAfter;
            void Hooks_OnCombatDirectorInitAfter()
            {
                EliteDef index = RoR2Content.Elites.Ice;
                switch (tier)
                {
                    case 1: index = RoR2Content.Elites.Ice; break;
                    case 2: index = DLC2Content.Elites.Aurelionite; break;
                    case 3: index = RoR2Content.Elites.Poison; break;
                    default: index = RoR2Content.Elites.Ice; break;

                }
                foreach (EliteTierDef eliteIndex in eliteTiers)
                {
                    if (eliteIndex.eliteTypes.Contains(index))
                    {
                        EliteTierDef targetTier = eliteIndex;
                        List<EliteDef> elites = targetTier.eliteTypes.ToList();
                        elites.Add(eliteDef);
                        targetTier.eliteTypes = elites.ToArray();
                    }
                }
            }
            return eliteDef;
        }

        public delegate void OnDOTAdded(DotController.DotDef dotDef);
        public static DotController.DotDef CreateDOT(BuffDef buffDef, out DotController.DotIndex dotIndex , bool resetTimerOnAdd, float interval, float damageCoefficient, DamageColorIndex damageColorIndex, CustomDotBehaviour customDotBehaviour, CustomDotVisual customDotVisual = null, CustomDotDamageEvaluation customDotDamageEvaluation = null, OnDOTAdded onDOTAdded = null)
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
            EquipmentPicker equipmentPicker = GameObject.Instantiate(Assets.EquipmentPicker, HUD.instancesList[0].mainContainer.transform).GetComponent<EquipmentPicker>();
            return equipmentPicker;
        }
    }
    public static class Extensions
    {
        public static T RegisterItemDef<T>(this T itemDef, Action<T> onItemDefAdded = null) where T : ItemDef
        {
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
        public static T RegisterExpansionDef<T>(this T expansionsDef, Action<T> onExpansionDefAdded = null) where T : ExpansionDef
        {
            expansions.Add(expansionsDef);
            onExpansionDefAdded?.Invoke(expansionsDef);
            return expansionsDef;
        }
    }
}
