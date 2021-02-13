using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desu
{
    public class CratCombatHandler : GenericCombatHandler
    {
        private const float PostZonePetCheckBuffer = 5;
        private Menu _menu;
        private double _lastZonedTime = 0;


        public CratCombatHandler()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.DazzleWithLights, StarfallPerk);
            RegisterPerkProcessor(PerkHash.Combust, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ThermalDetonation, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Supernova, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.QuickShot, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.DoubleShot, DamagePerk);
            RegisterPerkProcessor(PerkHash.Deadeye, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Antitrust, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.NanoFeast, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.BotConfinement, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.DodgeTheBlame, SelfDefPerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.EvasiveStance, SelfDefPerk, CombatActionPriority.Medium);
            RegisterPerkProcessor(PerkHash.Implode, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.Collapser, DamagePerk);
            RegisterPerkProcessor(PerkHash.Sphere, HealPerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratFormsInTriplicate, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratTaxAudit, LEProc);
            RegisterPerkProcessor(PerkHash.Feel, DamagePerk);
            RegisterPerkProcessor(PerkHash.Overrule, DamagePerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.Succumb, DamagePerk);
            RegisterPerkProcessor(PerkHash.ConfoundWithRules, DamagePerk);



            RegisterSpellProcessor(RelevantNanos.Heroic, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Icrt, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Icrt, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.DroidDamageMatrix, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.DroidPreasureMatrix, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.CorporateStrategy, PetTargetBuff);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDeltaBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Psy_IntBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDeltaDebuff).OrderByStackingOrder(), NanoDeltaDebuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SkillLockModifierDebuff1053).OrderByStackingOrder(), SkillLockDebuff1);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SkillLockModifierDebuff847).OrderByStackingOrder(), SkillLockDebuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetDamageOverTimeResistNanos).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetDefensiveNanos).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetShortTermDamageBuffs).OrderByStackingOrder(), PetTargetBuff);

            RegisterSpellProcessor(RelevantNanos.Pets.Where(x => x.Value.PetType == PetType.Attack).Select(x => x.Key).ToArray(), PetSpawner);
            RegisterSpellProcessor(RelevantNanos.Pets.Where(x => x.Value.PetType == PetType.Support).Select(x => x.Key).ToArray(), PetSpawner);


            //Spells


            RegisterSpellProcessor(RelevantNanos.MalaiseOfZeal, Malaise, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.WastefulArmMovements, CratDebuff);
            RegisterSpellProcessor(RelevantNanos.InefficientArmMovements, CratDebuff1);

            foreach (int shellId in RelevantNanos.Pets.Values.Select(x => x.ShellId))
                RegisterItemProcessor(shellId, shellId, PetSpawnerItem);

            _menu = new Menu("CombatHandler.Crat", "CombatHandler.Crat");
            _menu.AddItem(new MenuBool("UseDebuff", "Crat Debuffing", true));
            _menu.AddItem(new MenuBool("UseMalaise", "UseMalaise", true));
            _menu.AddItem(new MenuBool("SpawnPets", "Spawn Pets?", true));
            _menu.AddItem(new MenuBool("BuffPets", "Buff Pets?", true));


            OptionPanel.AddMenu(_menu);

            Game.TeleportEnded += OnZoned;

        }
        private void OnZoned(object s, EventArgs e)
        {
            _lastZonedTime = Time.NormalTime;
        }

        private bool Malaise(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseMalaise"))
                return false;


            if (fightingTarget.Buffs.Find(RelevantNanos.MalaiseOfZeal, out Buff buff))
            {
                if (buff.RemainingTime > 5 || spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }


            }
            return true;
        }

        protected bool PetSpawner(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (!_menu.GetBool("SpawnPets"))
                return false;

            if (Time.NormalTime < _lastZonedTime + PostZonePetCheckBuffer)
                return false;

            //Do not attempt any pet spawns if we have a pet not loaded as it could be the pet we think we need to replace.
            if (DynelManager.LocalPlayer.Pets.Any(x => x.Type == PetType.Unknown))
                return false;

            if (!RelevantNanos.Pets.ContainsKey(spell.Identity.Instance))
                return false;

            //Ignore spell if we already have this type of pet out
            if (DynelManager.LocalPlayer.Pets.Any(x => x.Type == RelevantNanos.Pets[spell.Identity.Instance].PetType))
                return false;

            //Ignore spell if we already have the shell in our inventory
            if (Inventory.Find(RelevantNanos.Pets[spell.Identity.Instance].ShellId, out Item shell))
                return false;

            actionTarget.ShouldSetTarget = false;


            return false;
        }
        protected virtual bool PetSpawnerItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("SpawnPets"))
                return false;

            if (Time.NormalTime < _lastZonedTime + PostZonePetCheckBuffer)
                return false;

            if (!RelevantNanos.Pets.Values.Any(x => (x.ShellId == item.LowId || x.ShellId == item.HighId) && !DynelManager.LocalPlayer.Pets.Any(p => p.Type == x.PetType)))
                return false;

            actionTarget.ShouldSetTarget = false;
            return true;
        }

        protected bool PetTargetBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("BuffPets"))
                return false;

            if (Time.NormalTime < _lastZonedTime + PostZonePetCheckBuffer)
                return false;

            bool petsNeedBuff = false;

            foreach (Pet pet in DynelManager.LocalPlayer.Pets.Where(x => x.Character != null && (x.Type == PetType.Attack)))
            {
                if (pet.Character.Buffs.Find(spell.Nanoline, out Buff buff))
                {
                    //Don't cast if weaker than existing
                    if (spell.StackingOrder < buff.StackingOrder)
                        continue;

                    //Don't cast if greater than 10% time remaining
                    if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                        continue;
                }

                actionTarget.Target = pet.Character;
                petsNeedBuff = true;
                break;
            }

            if (!petsNeedBuff)
                return false;

            actionTarget.ShouldSetTarget = true;
            return true;
        }

        private bool CratDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseDebuff"))
                return false;


            if (fightingTarget.Buffs.Find(RelevantNanos.WastefulArmMovements, out Buff buff))
            {
                if (buff.RemainingTime > 5 || spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }
            }

            return true;

        }
        private bool CratDebuff1(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseDebuff"))
                return false;


            if (fightingTarget.Buffs.Find(RelevantNanos.InefficientArmMovements, out Buff buff))
            {
                if (buff.RemainingTime > 5 || spell.StackingOrder < buff.StackingOrder)
                {

                    return false;
                }
            }

            return true;

        }
        private bool NanoDeltaDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
            {
                return false;
            }

            if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.NanoDeltaDebuff, out Buff buff))
            {
                if (buff.RemainingTime > 5 || spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }
            }
            return true;
        }
        private bool SkillLockDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
            {
                return false;
            }

            if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.SkillLockModifierDebuff847, out Buff buff))
            {
                if (buff.RemainingTime > 5 || spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }
            }
            return true;
        }
        private bool SkillLockDebuff1(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
            {
                return false;
            }

            if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.SkillLockModifierDebuff1053, out Buff buff))
            {
                if (buff.RemainingTime > 5 || spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }
            }
            return true;
        }

        private bool SelfDefPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.HealthPercent <= 50)

                return true;

            return false;
        }
        private bool HealPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.HealthPercent <= 50)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }
            return false;
        }




        private static class RelevantNanos
        {
            public const int PinkSlip = 273307;
            public const int WorkplaceDepression = 273631;
            public const int MalaiseOfZeal = 275824;
            public const int WastefulArmMovements = 302150;
            public const int ImprovedRedTape = 222687;
            public const int IntensifyStress = 267311;
            public const int Ubt = 99577;
            public const int InefficientArmMovements = 302143;
            public const int DroidDamageMatrix = 267916;
            public const int DroidPreasureMatrix = 302247;
            public const int Guardian = 302247;
            public const int Pinetti = 258580;
            public const int CorporateStrategy = 267611;

            public static Dictionary<int, PetSpellData> Pets = new Dictionary<int, PetSpellData>
            {
                { 273300, new PetSpellData(273300, PetType.Attack) },
                { 258580, new PetSpellData(258580, PetType.Support) }
            };


            // Buffs
            // Buffs are Improved Cut Red Tape and Motivational Speech: Improved Heroic Measures only


            public static readonly int Heroic = 270783;
            public static readonly int Icrt = 222695;

        }
        public class PetSpellData
        {
            public int ShellId;
            public PetType PetType;

            public PetSpellData(int shellId, PetType petType)
            {
                ShellId = shellId;
                PetType = petType;
            }
        }


    }
}




