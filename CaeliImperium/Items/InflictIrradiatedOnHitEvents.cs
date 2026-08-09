using CaeliImperium.Bodies;
using CaeliImperium.Components;
using CaeliImperium.ItemBehaviours;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace CaeliImperium.Items
{
    public static class InflictIrradiatedOnHitEvents
    {
        public static float explosionDamageCoefficient = 20f;
        public static float explosionProcCoefficient = 1f;
        public static float explosionForce = 3000f;
        public static float explosionRadius = 32f;
        public static BlastAttack.FalloffModel explosionFalloff = BlastAttack.FalloffModel.None;
        public static float totalDamageThreshold = 12f;
        public static float dotDamageMultiplier = 1f;
        public static float dotDuration = 6f;
        public static float dotProcCoefficient = 0f;
        public static float dotForce = 0f;
        public static float dotRadius = 12f;
        public static BlastAttack.FalloffModel dotFalloff = BlastAttack.FalloffModel.None;
        public static float rechargeTime = 10f;
        public static bool resetTimerOnAdd = false;
        public static float dotDamageInterval = 1f;
        public static float dotDamageCoefficient = 1f;
        public static DotController.DotDef IrradiatedDotDef;
        public static DotController.DotIndex IrradiatedDotIndex;
        public static DamageAPI.ModdedDamageType IrradiatedDamageType;
        public static DamageAPI.ModdedDamageType IrradiateDamageType;
        public static BuffDef Irradiated;
        public static BuffDef Charge;
        public static BuffDef Ready;
        public static BuffDef Recharging;
        private static bool init;
        public static void Init(ItemDef itemDef)
        {
            CaeliImperiumPlugin.onPluginDestroyed += CaeliImperiumPlugin_onPluginDestroyed;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            if (init) return;
            init = true;
            Irradiated = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/Irradiated.asset").RegisterBuffDef();
            Charge = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/IrradiatedCharge.asset").RegisterBuffDef();
            Ready = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/InflictIrradiatedOnHitReady.asset").RegisterBuffDef();
            Recharging = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/InflictIrradiatedOnHitRecharge.asset").RegisterBuffDef();
            IrradiatedDotDef = CaeliImperiumUtils.CreateDOT(Irradiated, out IrradiatedDotIndex, resetTimerOnAdd, dotDamageInterval, dotDamageCoefficient, DamageColorIndex.Poison, null, null, IrradiatedDotEvaluation);
            IrradiatedDamageType = DamageAPI.ReserveDamageType();
            IrradiateDamageType = DamageAPI.ReserveDamageType();
        }
        private static void IrradiatedDotEvaluation(DotController self, DotController.PendingDamage pendingDamage)
        {
            BlastAttack blastAttack = new BlastAttack
            {
                attacker = pendingDamage.attackerObject,
                attackerFiltering = AttackerFiltering.Default,
                baseDamage = pendingDamage.totalDamage,
                baseForce = dotForce,
                crit = false,
                damageColorIndex = DamageColorIndex.Poison,
                falloffModel = dotFalloff,
                inflictor = pendingDamage.attackerObject,
                position = self.victimObject.transform.position,
                damageType = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.DOT),
                procCoefficient = dotDamageCoefficient,
                radius = dotRadius + (self.victimBody ? self.victimBody.radius : 0f),
                teamIndex = TeamComponent.GetObjectTeam(pendingDamage.attackerObject),
            };
            blastAttack.AddModdedDamageType(IrradiatedDamageType);
            blastAttack.Fire();
            EffectData effectData = new EffectData
            {
                origin = blastAttack.position,
                scale = blastAttack.radius
            };
            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, effectData, true);
        }
        public static void InflictIrradiated(CharacterBody victimBody, CharacterBody attackerBody, float damageMultiplier, float duration, HurtBox hitHurtbox, int stacks)
        {
            InflictDotInfo inflictDotInfo = new InflictDotInfo
            {
                victimObject = victimBody.gameObject,
                attackerObject = attackerBody.gameObject,
                damageMultiplier = damageMultiplier,
                duration = duration,
                dotIndex = IrradiatedDotIndex,
                hitHurtBox = hitHurtbox,
            };
            for (int i = 0; i < stacks; i++) DotController.InflictDot(ref inflictDotInfo);
        }
        private static void CaeliImperiumPlugin_onPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= CaeliImperiumPlugin_onPluginDestroyed;
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
        }
        private static void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj)
        {
            if (NetworkServer.active) obj.AddItemBehavior<InflictIrradiatedOnHitBehaviour>(obj.inventory.GetItemCountEffective(CaeliImperiumContent.Items.InflictIrradiatedOnHit));
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            CharacterBody victimBody = obj.victimBody;
            if (!victimBody) return;
            DamageInfo damageInfo = obj.damageInfo;
            if (damageInfo.HasModdedDamageType(IrradiatedDamageType))
            {
                for (int i = 0; i < victimBody.buffs.Length; i++)
                {
                    int buffCount = victimBody.buffs[i];
                    if (buffCount <= 0) continue;
                    BuffDef buffDef = BuffCatalog.buffDefs[i];
                    if (buffDef == null || !buffDef.canStack || !buffDef.isDebuff || buffDef.isDOT) continue;
                    victimBody.AddBuff(buffDef);
                }
                List<CharacterBody.TimedBuff> timedBuffs = [];
                foreach (CharacterBody.TimedBuff timedBuff in victimBody.timedBuffs)
                {
                    BuffDef buffDef = BuffCatalog.buffDefs[(int)timedBuff.buffIndex];
                    if (buffDef == null || !buffDef.canStack || !buffDef.isDebuff || buffDef.isDOT) continue;
                    timedBuff.timer = timedBuff.totalDuration;
                    timedBuffs.Add(timedBuff);
                }
                foreach (CharacterBody.TimedBuff timedBuff in timedBuffs)
                {
                    BuffDef buffDef = BuffCatalog.buffDefs[(int)timedBuff.buffIndex];
                    victimBody.AddTimedBuff(buffDef, timedBuff.totalDuration);
                }
                DotController dotController = DotController.FindDotController(victimBody.gameObject);
                if (dotController)
                {
                    foreach (DotController.DotStack dotStack in dotController.dotStackList)
                    {
                        if (dotStack.dotIndex == IrradiatedDotIndex) continue;
                        dotStack.timer = dotStack.totalDuration;
                    }
                }
            }
            Inventory victimInventory = victimBody.inventory;
            if (victimInventory && victimBody.HasBuff(Ready) && obj.damageDealt > 0f && !damageInfo.rejected)
            {
                int itemStacks = victimInventory.GetItemCountEffective(CaeliImperiumContent.Items.InflictIrradiatedOnHit);
                if (itemStacks > 0)
                {
                    victimBody.RemoveBuff(Ready);
                    for (int i = 0; i < rechargeTime; i++)
                    {
                        victimBody.AddTimedBuff(Recharging, i + 1f);
                    }
                    BlastAttack blastAttack = new BlastAttack
                    {
                        attacker = victimBody.gameObject,
                        attackerFiltering = AttackerFiltering.Default,
                        baseDamage = victimBody.damage * explosionDamageCoefficient,
                        baseForce = explosionForce,
                        crit = victimBody.RollCrit(),
                        damageColorIndex = DamageColorIndex.Item,
                        falloffModel = explosionFalloff,
                        inflictor = victimBody.gameObject,
                        position = victimBody.transform.position,
                        damageType = new DamageTypeCombo(DamageType.Stun1s, DamageTypeExtended.Generic, DamageSource.Equipment),
                        procCoefficient = explosionProcCoefficient,
                        radius = explosionRadius,
                        teamIndex = victimBody.teamComponent ? victimBody.teamComponent.teamIndex : TeamComponent.GetObjectTeam(victimBody.gameObject),
                    };
                    blastAttack.AddModdedDamageType(IrradiateDamageType);
                    blastAttack.Fire();
                    EffectData effectData = new EffectData
                    {
                        origin = blastAttack.position,
                        scale = blastAttack.radius
                    };
                    EffectManager.SpawnEffect(BomberWisp2Events.Explosion.index, effectData, true);
                }
            }
            CharacterBody attackerBody = obj.attackerBody;
            if (!attackerBody) return;
            if (damageInfo.HasModdedDamageType(IrradiateDamageType))
            {
                InflictIrradiated(victimBody, attackerBody, dotDamageCoefficient, dotDuration, damageInfo.inflictedHurtbox, 1);
            }
            Inventory attackerInventory = attackerBody.inventory;
            if (attackerInventory && attackerBody.HasBuff(Ready) && !damageInfo.HasModdedDamageType(IrradiatedDamageType))
            {
                int itemStacks = attackerInventory.GetItemCountEffective(CaeliImperiumContent.Items.InflictIrradiatedOnHit);
                if (itemStacks > 0)
                {
                    IrradiatedChargeController irradiatedChargeController = IrradiatedChargeController.FindIrradiatedChargeController(victimBody);
                    irradiatedChargeController.AddDamage(obj);
                }
            }
        }
    }
}
