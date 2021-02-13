using System.Linq;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;

namespace Desu
{
    public class MpCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        public MpCombatHandler()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.DazzleWithLights, StarfallPerk);
            RegisterPerkProcessor(PerkHash.Combust, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ThermalDetonation, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Supernova, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.NanoFeast, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.BotConfinement, TargetedDamagePerk);

            RegisterSpellProcessor(RelevantNanos.CompositeNano, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeAttributes, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRanged, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRangedSpecial, GenericBuff);

            RegisterSpellProcessor(RelevantNanos.EyeoftheTigress, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.OdinOtherEye, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.OneMindOnePurpose, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.MochamNeuralInterfaceWeb, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Cm, GenericBuff);

            //Spells
            RegisterSpellProcessor(RelevantNanos.WrathAbatement, MpDebuff, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.TaintofWill, MpDebuff, CombatActionPriority.Medium);

            RegisterSpellProcessor(RelevantNanos.TaintofResolve, MpDebuff, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.MindQuake, SingleTargetNuke, CombatActionPriority.Low);


            _menu = new Menu("CombatHandler.Mp", "CombatHandler.Mp");
            _menu.AddItem(new MenuBool("UseDebuff", "Mp Debuffing", true));



            RegisterSpellProcessor(RelevantNanos.CompositeNano, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeAttributes, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRanged, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeRangedSpecial, GenericBuff);


        }

        private bool MpDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseDebuff"))
                return false;

            //Check the remaining time on debuffs. On the enemy target
            foreach (Buff buff in fightingTarget.Buffs.AsEnumerable())
            {
                //Chat.WriteLine(buff.Name);
                if (buff.Name == spell.Name && buff.RemainingTime > 1)
                    return false;


            }

            return true;

        }


        private bool SingleTargetNuke(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            return true;
        }

        private static class RelevantNanos
        {
            public const int MindQuake = 125761;
            public const int WrathAbatement = 99113;
            public const int TaintofWill = 227134;
            public const int TaintofResolve = 227138;



            // Buffs
            

            public static readonly int CompositeAttributes = 223372;
            public static readonly int CompositeNano = 223380;
            public static readonly int CompositeRanged = 223364;
            public static readonly int CompositeRangedSpecial = 223348;

            public static readonly int Cm = 220343;
            public static readonly int OneMindOnePurpose = 95522;
            public static readonly int EyeoftheTigress = 302257;
            public static readonly int MochamNeuralInterfaceWeb = 95409;
            public static readonly int OdinOtherEye = 273379;
        }
    }
}

