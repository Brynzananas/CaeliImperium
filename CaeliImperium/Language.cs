using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperium.Items;

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
        }
        public static void InitHealReceivedDamage()
        {
            AddLanguageToken(HealReceivedDamage.nameToken, "Emergency Medical Treatment");
            AddLanguageToken(HealReceivedDamage.nameToken, "Неотложенная Медецинская Помощь", "ru");
            AddLanguageToken(HealReceivedDamage.pickupToken, "Heal received damage over the time course.");
            AddLanguageToken(HealReceivedDamage.pickupToken, "Исцелите полученный урона на протяжении некоторого времени.", "ru");
            float seconds = Events.HealReceivedDamageTime;
            string secondsStr = seconds == 1f ? "second" : "seconds";
            string secondsStrRu = GetSecondsRuString(seconds);
            AddLanguageToken(HealReceivedDamage.descriptionToken, $"Upon {damagePrefix}taking damage{endPrefix}, {healingPrefix}heal{endPrefix} for 100% of the {damagePrefix}damage taken{endPrefix} over the course of {seconds} {secondsStr} {stackPrefix}(-{Events.HealReceivedDamageStackTimeReduction}% per stack){endPrefix}.");
            AddLanguageToken(HealReceivedDamage.descriptionToken, $"При {damagePrefix}получении урона{endPrefix}, {healingPrefix}исцели{endPrefix} 100% полученного урона на протяжении {seconds} {secondsStrRu} {stackPrefix}(-{Events.HealReceivedDamageStackTimeReduction}% за шт.){endPrefix}.", "ru");
        }
        public static void InitDrawSpeedPath()
        {
            AddLanguageToken(DrawSpeedPath.nameToken, "Chalk");
            AddLanguageToken(DrawSpeedPath.nameToken, "Мел", "ru");
            AddLanguageToken(DrawSpeedPath.pickupToken, "Draw a path as you move that increases movement speed and grants flight.");
            AddLanguageToken(DrawSpeedPath.pickupToken, "Рисуйте путь пока передвигаетесь дающий свободное хождение с повышенной скоростью передвижения.", "ru");
            AddLanguageToken(DrawSpeedPath.descriptionToken, $"Draw a path as you move. While on this path, increase {utilityPrefix}movement speed{endPrefix} by {utilityPrefix}{Events.SpeedPathSpeedBonusCoefficient * 100f}%{endPrefix} {stackPrefix}(+{Events.SpeedPathSpeedBonusStackCoefficient * 100f}% per stack.){endPrefix} and {utilityPrefix}gain flight{endPrefix}.");
            AddLanguageToken(DrawSpeedPath.descriptionToken, $"Рисуйте путь пока передвигаетесь. Хождение по проложенному пути повышает {utilityPrefix}скорость передвижения{endPrefix} на {utilityPrefix}{Events.SpeedPathSpeedBonusCoefficient * 100f}%{endPrefix} {stackPrefix}(+{Events.SpeedPathSpeedBonusStackCoefficient * 100f}% за шт.){endPrefix} и {utilityPrefix}даёт полет{endPrefix}.", "ru");
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
    }
}
