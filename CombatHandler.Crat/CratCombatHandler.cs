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
        private Menu _menu;
        private const float PostZonePetCheckBuffer = 5;
        private double _lastZonedTime = 0;
        private bool _removeDebuffs = false;
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
            RegisterPerkProcessor(PerkHash.Feel, DamagePerk);
            RegisterPerkProcessor(PerkHash.Overrule, DamagePerk, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.Succumb, DamagePerk);
            RegisterPerkProcessor(PerkHash.ConfoundWithRules, DamagePerk);

            RegisterSpellProcessor(RelevantNanos.MalaiseOfZeal, Malaise, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.WastefulArmMovements, CratDebuff);
            RegisterSpellProcessor(RelevantNanos.InefficientArmMovements, CratDebuff1);

            RegisterSpellProcessor(RelevantNanos.WorkplaceDepression, WorkplaceDepression, CombatActionPriority.High);



            RegisterSpellProcessor(RelevantNanos.PistolMastery, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.InsightIntoTheShadowlands, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.BotReproduction, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.NeuronalStimulator, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.MotivationalSpeechImprovedHeroicMeasures, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Icrt, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.Icrt, TeamBuff);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder(), GenericBuff);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDeltaBuffs).OrderByStackingOrder(), GenericBuff);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDeltaBuffs).OrderByStackingOrder(), TeamBuff);

            //Pet Spawners
            RegisterSpellProcessor(RelevantNanos.Pets.Where(x => x.Value.PetType == PetType.Attack).Select(x => x.Key).ToArray(), PetSpawner);
            RegisterSpellProcessor(RelevantNanos.Pets.Where(x => x.Value.PetType == PetType.Support).Select(x => x.Key).ToArray(), PetSpawner);

            //Pet Shells
            foreach (int shellId in RelevantNanos.Pets.Values.Select(x => x.ShellId))
            {
                if (shellId != 0)
                    RegisterItemProcessor(shellId, shellId, PetSpawnerItem);
            }

            RegisterSpellProcessor(RelevantNanos.DroidDamageMatrix, DroidDamageMatrix);
            RegisterSpellProcessor(RelevantNanos.DroidPressureMatrix, DroidPressureMatrix);
            
            RegisterSpellProcessor(RelevantNanos.NaniteRobotProtection, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.GreaterCorporateInsurancePolicy, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.CorporateStrategy, CorporateStrategy);
            RegisterSpellProcessor(RelevantNanos.Defamation101, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.Icrt, PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.ImprovedRedTape, CratDebuff);
            RegisterSpellProcessor(RelevantNanos.IntensifyStress, CratDebuff1);
        

        _menu = new Menu("CombatHandler.Crat", "CombatHandler.Crat");
            _menu.AddItem(new MenuBool("UseDebuff", "Crat Debuffing", true));
            _menu.AddItem(new MenuBool("UseMalaise", "UseMalaise", true));
            _menu.AddItem(new MenuBool("UseWorkplaceDepression", "WorkplaceDepression", true));



            OptionPanel.AddMenu(_menu);
            Game.TeleportEnded += OnZoned;
            Game.OnUpdate += OnUpdate;

        }

        private bool WorkplaceDepression(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingtarget == null || !_menu.GetBool("UseWorkplaceDepression"))
                return false;

            return true;
        }
        private void OnUpdate(object sender, float e)
        {
            if (_removeDebuffs == true)
            {
                _removeDebuffs = false;
                if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.DemotivationalSpeeches, out Buff buff))
                {
                    buff.Remove();
                }
            }
        }
        private void OnZoned(object s, EventArgs e)
        {
            _lastZonedTime = Time.NormalTime;
            _removeDebuffs = true;

        }
        private bool HasMalaise(Spell spell, SimpleChar target)
        {
            Buff ubt;
            if (target.Buffs.Find(RelevantNanos.Ubt, out ubt))
                return true;
            foreach (Buff buff in target.Buffs.AsEnumerable())
            {
                if (spell.Nanoline == buff.Nanoline && spell.StackingOrder <= buff.StackingOrder)
                {
                    return true;
                }
            }
            return false;
        }




        private bool SelfBuffPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name)
                {
                    //Chat.WriteLine(buff.Name+" "+perk.Name);
                    return false;
                }
            }
            return true;
        }

        private bool Malaise(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseMalaise"))
                return false;
            Buff buff = null;



             SimpleChar mobWithoutDebuff = DynelManager.Characters
                .Where(c => !HasMalaise(spell, c))
                .Where(c => c.FightingTarget != null && Team.Members.Select(t => t.Identity).Contains(c.FightingTarget.Identity))
                .OrderBy(c => c.IsPet)
                .ThenByDescending(c => c.IsInAttackRange())
                .FirstOrDefault();
            if (mobWithoutDebuff != null)
            {
                actionTarget.Target = mobWithoutDebuff;
                return true;
            }

       

            if (fightingTarget.Buffs.Find(RelevantNanos.MalaiseOfZeal, out buff))
            {
                if (buff.RemainingTime > 5)
                {
                    return false;
                }
                //if (DynelManager.LocalPlayer.IsAttacking)
                //    DynelManager.LocalPlayer.Pets.Attack(fightingTarget.Identity);
                return true;
            }
            return false;
        }


        private bool CratDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !_menu.GetBool("UseDebuff"))
                return false;


            if (fightingTarget.Buffs.Find(RelevantNanos.WastefulArmMovements, out Buff buff))
            {
                if (buff.RemainingTime > 5)
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
                if (buff.RemainingTime > 5)
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



        protected bool PetSpawner(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (Time.NormalTime < _lastZonedTime + PostZonePetCheckBuffer)
                return false;
            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
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
            return true;
        }

        protected virtual bool PetSpawnerItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
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

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
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

        private static class RelevantNanos
        {
            public const int PinkSlip = 273307;
            public const int WorkplaceDepression = 273631;
            public const int MalaiseOfZeal = 275824;
            public const int WastefulArmMovements = 302150;
            public const int ImprovedRedTape = 222687;
            public const int IntensifyStress = 267311;
            public static readonly int[] Ubt = { 99577, 301843, 301844 };
            public const int InefficientArmMovements = 302143;
            public const int BotReproduction = 222857;
            public const int NeuronalStimulator = 220345;
            public const int MotivationalSpeechImprovedHeroicMeasures = 270783;
            public const int PistolMastery = 29246;
            public const int InsightIntoTheShadowlands = 268610;
            public static readonly int Icrt = 222695;
            public static readonly int CompositeAttributes = 223372;
            public static readonly int CompositeNano = 223380;
            public static readonly int CompositeRanged = 223364;
            public static readonly int CompositeRangedSpecial = 223348;

            public const int NaniteRobotProtection = 267612;
            public const int DroidDamageMatrix = 267916;
            public const int GreaterCorporateInsurancePolicy = 267605;
            public const int CorporateStrategy = 267611;
            public const int DroidPressureMatrix = 302247;
            public const int Defamation101  = 155616;
            


            public static Dictionary<int, PetSpellData> Pets = new Dictionary<int, PetSpellData>
            {
                { 258580, new PetSpellData(0, PetType.Support) },
                { 273300, new PetSpellData(273301, PetType.Attack) }
            };

        }
        protected bool DroidDamageMatrix(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (!DynelManager.LocalPlayer.Pets.Where(x => x.Character != null)
                                            .Where(x => x.Type == PetType.Attack)
                                            .Any(x => !x.Character.Buffs.Find(285696, out _)))
                return false;

            actionTarget.ShouldSetTarget = false;
            return true;
        }
        protected bool CorporateStrategy(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (!DynelManager.LocalPlayer.Pets.Where(x => x.Character != null)
                                            .Where(x => x.Type == PetType.Attack)
                                            .Any(x => !x.Character.Buffs.Find(285695, out _)))
                return false;

            actionTarget.ShouldSetTarget = false;
            return true;
        }
        protected bool DroidPressureMatrix(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {

            if (!DynelManager.LocalPlayer.Pets.Where(x => x.Character != null)
                                            .Where(x => x.Type == PetType.Attack)
                                            .Any(x => !x.Character.Buffs.Find(302246, out _)))
                return false;

            actionTarget.ShouldSetTarget = false;
            return true;
        }
        private class PetSpellData
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




