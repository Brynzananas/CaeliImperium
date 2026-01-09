using CaeliImperium.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperium.CaeliImperiumContent.Items;

namespace CaeliImperium
{
    public static class Language
    {
        public static void Init()
        {
            AddLanguageToken(CaeliImperiumPlugin.expansionDef.nameToken, "Caeli Imperium");
            AddLanguageToken(CaeliImperiumPlugin.expansionDef.descriptionToken, "Adds content from the 'Caeli Imperium' expansion to the game.");
            AddLanguageToken(CaeliImperiumPlugin.expansionDef.descriptionToken, "Добавляет в игру контент из дополнения «Caeli Imperium».", "ru");
            InitHealReceivedDamage();
            InitDrawSpeedPath();
            InitInfiniteSecondarySkillCharges();
        }
        public static void InitHealReceivedDamage()
        {
            AddLanguageToken(HealReceivedDamage.nameToken, "Emergency Medical Treatment");
            AddLanguageToken(HealReceivedDamage.nameToken, "Неотложенная Медецинская Помощь", "ru");
            AddLanguageToken(HealReceivedDamage.pickupToken, "Heal received damage over the time course.");
            AddLanguageToken(HealReceivedDamage.pickupToken, "Исцелите полученный урон на протяжении некоторого времени.", "ru");
            float seconds =  HealReceivedDamageEvents.HealReceivedDamageTime;
            string secondsStr = seconds == 1f ? "second" : "seconds";
            string secondsStrRu = GetSecondsRuString(seconds);
            AddLanguageToken(HealReceivedDamage.descriptionToken, $"Upon {damagePrefix}taking damage{endPrefix}, {healingPrefix}heal{endPrefix} for {HealReceivedDamageEvents.HealReceivedHealCoefficient * 100f}%{GetStackString(HealReceivedDamageEvents.HealReceivedHealCoefficientPerStack * 100f, true, false)} of the {damagePrefix}damage taken{endPrefix} over the course of {seconds} {secondsStr}{GetStackString(-HealReceivedDamageEvents.HealReceivedDamageStackTimeReduction, true, true)}");
            AddLanguageToken(HealReceivedDamage.descriptionToken, $"При {damagePrefix}получении урона{endPrefix}, {healingPrefix}исцели{endPrefix} 100% полученного урона на протяжении {seconds} {secondsStrRu}{GetStackStringRu(-HealReceivedDamageEvents.HealReceivedDamageStackTimeReduction, true, true)}", "ru");
        }
        public static void InitInfiniteSecondarySkillCharges()
        {
            AddLanguageToken(InfiniteSecondarySkillCharges.nameToken, "Infinite Magazine");
            AddLanguageToken(InfiniteSecondarySkillCharges.nameToken, "Бесконечный магазин", "ru");
            AddLanguageToken(InfiniteSecondarySkillCharges.pickupToken, "Increase damage of your Secondary skill and gain infinite charges. Convert excessive charges into extra Secondary skill damage.");
            AddLanguageToken(InfiniteSecondarySkillCharges.pickupToken, "Увеличьте урон вторичного навыка и получите бесконечное количество зарядов. Превратите излишние заряды в дополнительный урон вторичного навыка.", "ru");
            AddLanguageToken(InfiniteSecondarySkillCharges.descriptionToken, $"Use {utilityPrefix}Secondary skill{endPrefix} without minimal charge requirement. Consume all {utilityPrefix}Secondary skill{endPrefix} charges on skill execution, increasing {utilityPrefix}Secondary skill{endPrefix} {damagePrefix}damage{endPrefix} by {damagePrefix}{InfiniteSecondarySkillChargesEvents.buffDamage * 100f}%{endPrefix} for each consumed charge. Extra {utilityPrefix}Secondary skill{endPrefix} {damagePrefix}damage{endPrefix} {deathPrefix}disappears{endPrefix} on dealing any {utilityPrefix}Secondary skill{endPrefix} {damagePrefix}damage{endPrefix}. Increase {utilityPrefix}Secondary skill{endPrefix} {damagePrefix}damage{endPrefix} by {damagePrefix}{InfiniteSecondarySkillChargesEvents.itemDamage * 100f}%{endPrefix}{GetStackString(InfiniteSecondarySkillChargesEvents.itemDamagePerStack * 100f, true, true)}");
            AddLanguageToken(InfiniteSecondarySkillCharges.descriptionToken, $"Используйте {utilityPrefix}вторичный навык{endPrefix} без требования минимального количества зарядов. Поглотите все заряды {utilityPrefix}вторичного навыка{endPrefix} при использовании навыка, увеличивая {damagePrefix}урон{endPrefix} {utilityPrefix}вторичного навыка{endPrefix} на {damagePrefix}{InfiniteSecondarySkillChargesEvents.buffDamage * 100f}%{endPrefix} за каждый поглощенный заряд. Дополнительный {damagePrefix}урон{endPrefix} {utilityPrefix}вторичного навыка{endPrefix} {deathPrefix}пропадает{endPrefix} при нанесения любого {damagePrefix}урона{endPrefix} {utilityPrefix}вторичным навыком{endPrefix}. Увеличьте {damagePrefix}урона{endPrefix} {utilityPrefix}вторичного навыка{endPrefix} на {damagePrefix}{InfiniteSecondarySkillChargesEvents.itemDamage * 100f}%{endPrefix}{GetStackStringRu(InfiniteSecondarySkillChargesEvents.itemDamagePerStack * 100f, true, true)}", "ru");
        }
        public static void InitDrawSpeedPath()
        {
            AddLanguageToken(DrawSpeedPath.nameToken, "Chalk");
            AddLanguageToken(DrawSpeedPath.nameToken, "Мел", "ru");
            AddLanguageToken(DrawSpeedPath.pickupToken, "Draw a path as you move that increases movement speed and grants flight.");
            AddLanguageToken(DrawSpeedPath.pickupToken, "Рисуйте путь пока передвигаетесь дающий свободное хождение с повышенной скоростью передвижения.", "ru");
            AddLanguageToken(DrawSpeedPath.descriptionToken, $"Draw a path as you move. While on this path, increase {utilityPrefix}movement speed{endPrefix} by {utilityPrefix}{DrawSpeedPathEvents.SpeedPathSpeedBonusCoefficient * 100f}%{endPrefix}{GetStackString(DrawSpeedPathEvents.SpeedPathSpeedBonusStackCoefficient * 100f, true, false)} and {utilityPrefix}gain flight{endPrefix}.");
            AddLanguageToken(DrawSpeedPath.descriptionToken, $"Рисуйте путь пока передвигаетесь. Хождение по проложенному пути повышает {utilityPrefix}скорость передвижения{endPrefix} на {utilityPrefix}{DrawSpeedPathEvents.SpeedPathSpeedBonusCoefficient * 100f}%{endPrefix}{GetStackStringRu(DrawSpeedPathEvents.SpeedPathSpeedBonusStackCoefficient * 100f, true, false)} и {utilityPrefix}даёт полет{endPrefix}.", "ru");
        }
        public static void AddLanguageToken(string token, string text) => AddLanguageToken(token, text, "en");
        public static void AddLanguageToken(string token, string text, string lang)
        {
            RoR2.Language language = RoR2.Language.GetOrCreateLanguage(lang);
            if (language == null) return;
            if (language.stringsByToken.ContainsKey(token))
            {
                language.stringsByToken[token] = text;
            }
            else
            {
                language.stringsByToken.Add(token, text);
            }
        }
        public const string damagePrefix = "<style=cIsDamage>";
        public const string keywordPrefix = "<style=cKeywordName>";
        public const string subPrefix = "<style=cSub>";
        public const string stackPrefix = "<style=cStack>";
        public const string deathPrefix = "<style=cDeath>";
        public const string utilityPrefix = "<style=cIsUtility>";
        public const string healingPrefix = "<style=cIsHealing>";
        public const string endPrefix = "</style>";
        public static string GetSecondsRuString(float time)
        {
            float floored = MathF.Floor(time);
            if (time - floored != 0f) return "секунды";
            float flooredAbs = Mathf.Abs(floored);
            if ((flooredAbs >= 5 && flooredAbs <= 10f) || floored % 10f == 0f) return "секунд";
            if (flooredAbs == 1f) return "секунду";
            return "секунды";
        }
        public static string GetStackString(float value, bool percentage, bool end)
        {
            if (value == 0f) return "";
            return " " + stackPrefix + "(" + (value > 0f ? "+" : "") + value + (percentage ? "%" : "") + " per stack)" + endPrefix + (end ? "." : "");
        }
        public static string GetStackStringRu(float value, bool percentage, bool end)
        {
            if (value == 0f) return "";
            return " " + stackPrefix + "(" + (value > 0f ? "+" : "") + value + (percentage ? "%" : "") + " за шт.)" + endPrefix + (end ? "." : "");
        }
    }
}
