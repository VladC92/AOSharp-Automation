using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Desu
{
    class DocCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        private List<PerkHash> BattleGroupHeals = new List<PerkHash> {
            PerkHash.BattlegroupHeal1,
            PerkHash.BattlegroupHeal2,
            PerkHash.BattlegroupHeal3,
            PerkHash.BattlegroupHeal4,
        };
        public DocCombatHandler()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.HostileTakeover, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ChaoticAssumption, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.DazzleWithLights, StarfallPerk);
            RegisterPerkProcessor(PerkHash.Combust, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ThermalDetonation, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.QuickShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.DoubleShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Deadeye, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Collapser, DamagePerk);
            RegisterPerkProcessor(PerkHash.Implode, DamagePerk);
            RegisterPerkProcessor(PerkHash.MaliciousProhibition, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.LEProcDoctorAnatomicBlight, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcDoctorAntiseptic, LEProc);
            BattleGroupHeals.ForEach(p => RegisterPerkProcessor(p, MajorHealPerk));

            //Spells
            RegisterSpellProcessor(RelevantNanos.BODILY_INV, BI, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.IMPROVED_CH, CompleteHeal);
            // RegisterSpellProcessor(RelevantNanos.IMPROVED_LC, ILC, CombatActionPriority.Medium);
            RegisterSpellProcessor(RelevantNanos.SuperiorTeamHealthPlan, TeamHeal);
            RegisterSpellProcessor(RelevantNanos.TeamEnhancedDeathlessBlessing, TEDB);
            //  RegisterSpellProcessor(RelevantNanos.IMPROVED_LC, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedNanoRepulsor, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.IronCircle, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.SuperiorFirstAid, GenericBuff);

            RegisterSpellProcessor(RelevantNanos.InstinctiveControl, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ContinuousReconstruction, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.SuperiorOmniMedEnhancement, SuperiorOmniMedEnhancement);

            RegisterSpellProcessor(RelevantNanos.ContinuousReconstruction, DocTeamBuff);
            RegisterSpellProcessor(RelevantNanos.UBT, DebuffTarget);


            //This needs work 
            //RegisterSpellProcessor(RelevantNanos.UBT, DebuffTarget);

            _menu = new Menu("CombatHandler.Doc", "CombatHandler.Doc");
            _menu.AddItem(new MenuBool("UseDebuff", "Doc Debuffing", true));
            _menu.AddItem(new MenuBool("buffTeam", "Buff team", true));
            OptionPanel.AddMenu(_menu);
        }
        public bool DocTeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("buffTeam"))
                return false;

            return TeamBuff(spell, fightingTarget, ref actionTarget);
        }


        private bool DebuffTarget(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseDebuff"))
                return false;

            //Check if the target has the ubt buff running
            foreach (Buff buff in fightingTarget.Buffs.AsEnumerable())
                if (buff.Name == spell.Name)
                    return false;

            //Check if you are low hp dont debuff
            if (DynelManager.LocalPlayer.HealthPercent <= 70)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return false;
            }

            //Check if we're in a team and someone is low hp , dont debuff
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => c.IsAlive)
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent < 70)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    return false;
                }
            }
            return true;
        }

        private bool MajorHealPerk(PerkAction perkAction, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
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
                    .Where(c => c.HealthPercent < 30)
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


        private bool BI(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 80)
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

        private bool TeamHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {

                foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
                {

                    SimpleChar dyingTeamMember = DynelManager.Characters
                        .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                        .Where(c => c.HealthPercent <= 80)
                        .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                        .FirstOrDefault();
                }
        
            }

            return false;
        }

        private bool TEDB(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
               
                if (spell.Name == buff.Name && buff.RemainingTime / buff.TotalTime > 0.1)
                    return false;

            }
            if (fightingTarget == null)
                actionTarget.ShouldSetTarget = false;
           
            return true;
        }
  
        private bool CompleteHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 40)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 40)
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

        protected bool SuperiorOmniMedEnhancement(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (!DynelManager.LocalPlayer.Buffs.Find(95673, out _))
            {
                actionTarget.ShouldSetTarget = false;
                return true;
            }
            return false;

        }
        private static class RelevantNanos
        {
            public static readonly int IMPROVED_LC = 275011;
            public const int IMPROVED_CH = 270747;
            public const int BODILY_INV = 223299;
            public const int UBT = 99577;
            public const int UBT_MONSTER = 301844;
            public const int UBT_HUMAN = 301843;
            public const int SuperiorTeamHealthPlan = 273312;
            public const int TeamEnhancedDeathlessBlessing = 269455;
            public static readonly int ImprovedInstinctiveControl = 222856;
            public static readonly int ImprovedNanoRepulsor = 222823;
            public static readonly int ContinuousReconstruction = 222824;
            public const int SuperiorOmniMedEnhancement = 95709;
            public const int InstinctiveControl = 28669;
            public const int IronCircle = 42400;
            public const int SuperiorFirstAid = 28675;
            




        }
    }
}
