using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;
using System.Linq;

namespace Desu
{
    public class AdvCombathandler : GenericCombatHandler
    {
        private readonly Menu _menu;
        public AdvCombathandler()
        {


            //Procs
            RegisterPerkProcessor(PerkHash.LEProcAdventurerAesirAbsorption, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcAdventurerCombustion, LEProc);

            RegisterPerkProcessor(PerkHash.NocturnalStrike, DamagePerk);
            RegisterPerkProcessor(PerkHash.LightBullet, DamagePerk);
            RegisterPerkProcessor(PerkHash.Devour, DamagePerk);
            RegisterPerkProcessor(PerkHash.Deadeye, DamagePerk);
            RegisterPerkProcessor(PerkHash.BleedingWounds, DamagePerk);
            RegisterPerkProcessor(PerkHash.QuickShot, DamagePerk);
            RegisterPerkProcessor(PerkHash.DoubleShot, DamagePerk);
            RegisterPerkProcessor(PerkHash.Collapser, DamagePerk);
            RegisterPerkProcessor(PerkHash.Implode, DamagePerk);
            RegisterPerkProcessor(PerkHash.Opening, DamagePerk);

            //Spells

            RegisterSpellProcessor(RelevantNanos.PlayfulCub, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.LycanthropicDexterity, GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.RunspeedBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SelfRoot_SnareResistBuff).OrderByStackingOrder(), GenericBuff);
         //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PerceptionBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.OtherRoot_SnareResistBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MultiwieldBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageShields).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageShieldUpgrades).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ArmorBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AimedShotBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageBuffs_LineA).OrderByStackingOrder(), GenericBuff);
          //  RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SingleTargetHealing).OrderByStackingOrder(), Heal);
            RegisterSpellProcessor(RelevantNanos.InvocationofthePhoenix, CompleteHeal, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.BeautyofLife, Heal);





            _menu = new Menu("CombatHandler.Adv", "CombatHandler.Adv");

            OptionPanel.AddMenu(_menu);

        }
        private bool Heal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 70)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 80)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }

        private bool CompleteHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 30)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 30)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }

        private static class RelevantNanos
        {
            public const int InvocationofthePhoenix = 136672;
            public const int OneWithNature = 136674;
            public const int PlayfulCub = 85062;
            public const int LycanthropicDexterity = 302235;
            public const int BeautyofLife = 223167;

        }

    }
}
