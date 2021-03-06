﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    class NewSpells
    {
        static LibraryScriptableObject library => Main.library;
        static internal BlueprintAbility shillelagh;
        static internal BlueprintAbility flame_blade;
        static internal BlueprintAbility virtuoso_performance;
        static internal BlueprintAbility deadly_juggernaut;
        static internal BlueprintAbility invisibility_purge;
        static internal BlueprintAbility sanctuary;
        static internal BlueprintBuff sanctuary_buff;


        static public void load()
        {
            createShillelagh();
            createFlameBlade();
            createVirtuosoPerformance();
            createDeadlyJuggernaut();
            createInvisibilityPurge();
            createSanctuary();
        }


        static void createSanctuary()
        {
            var lesser_restoration = library.Get<BlueprintAbility>("e84fc922ccf952943b5240293669b171");
            var sancturay_logic = Helpers.Create<NewMechanics.Sanctuary>(c =>
                                                                         {
                                                                             c.save_type = SavingThrowType.Will;
                                                                             c.offensive_action_effect = NewMechanics.Sanctuary.OffensiveActionEffect.REMOVE_FROM_OWNER;
                                                                         }
                                                                         );
            sanctuary_buff = library.CopyAndAdd<BlueprintBuff>("525f980cb29bc2240b93e953974cb325", "SanctuaryBuff", "");//invisibility

            sanctuary_buff.ComponentsArray = new BlueprintComponent[] { sancturay_logic };

            var apply_buff = Common.createContextActionApplyBuff(sanctuary_buff,
                                                                Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Rounds),
                                                                is_from_spell: true);

            sanctuary = Helpers.CreateAbility("SanctuaryAbility",
                                                "Sanctuary",
                                                "Any opponent attempting to directly attack the warded creature, even with a targeted spell, must attempt a Will save. If the save succeeds, the opponent can attack normally and is unaffected by that casting of the spell. If the save fails, the opponent can’t follow through with the attack, that part of its action is lost, and it can’t directly attack the warded creature for the duration of the spell. Those not attempting to attack the subject remain unaffected. This spell does not prevent the warded creature from being attacked or affected by area of effect spells. The subject cannot attack without breaking the spell but may use non-attack spells or otherwise act.",
                                                "",
                                                lesser_restoration.Icon,
                                                AbilityType.Spell,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Touch,
                                                Helpers.roundsPerLevelDuration,
                                                "Will negates",
                                                Helpers.CreateSpellComponent(SpellSchool.Abjuration),
                                                Helpers.CreateRunActions(apply_buff)
                                                );

            sanctuary_buff.SetDescription(sanctuary.Description);
            sanctuary_buff.SetIcon(sanctuary.Icon);

            sanctuary.CanTargetSelf = true;
            sanctuary.CanTargetPoint = false;
            sanctuary.CanTargetFriends = true;
            sanctuary.CanTargetEnemies = false;
            sanctuary.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            sanctuary.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionTouch;
            sanctuary.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten;
            sanctuary.AddToSpellList(Helpers.clericSpellList, 1);
            sanctuary.AddToSpellList(Helpers.inquisitorSpellList, 1);

            sanctuary.AddSpellAndScroll("c0af0b5277e91e347ade3aa8994b0d17"); //invisibility
        }



        static internal void createShillelagh()
        {
            var boneshaker = library.Get<BlueprintAbility>("b7731c2b4fa1c9844a092329177be4c3");
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var enchant_dice = Helpers.Create<NewMechanics.WeaponDamageChange>();
            enchant_dice.dice_formula = new DiceFormula(2, DiceType.D6);
            enchant_dice.bonus_damage = 0;
            enchant_dice.damage_type_description = null;
            var enchantment_size = Common.createWeaponEnchantment("ShillelaghEnchantment",
                                                                  "Shillelagh",
                                                                  "Your own non - magical club or quarterstaff becomes a weapon with a + 1 enhancement bonus on attack and damage rolls. A quarterstaff gains this enhancement for both ends of the weapon.It deals damage as if it were two size categories larger(a Small club or quarterstaff so transmuted deals 1d8 points of damage, a Medium 2d6, and a Large 3d6), +1 for its enhancement bonus. If you stop wielding it, the weapon loses magical properties.",
                                                                  "Shillelagh",
                                                                  "",
                                                                  "",
                                                                  0,
                                                                  null,
                                                                  enchant_dice);
            var enhantment1 = library.Get<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050");

            BlueprintWeaponType[] shillelagh_types = new BlueprintWeaponType[] {library.Get<BlueprintWeaponType>("26aa0672af2c7d84ba93bec37758c712"), // club
                                                                                library.Get<BlueprintWeaponType>("629736dabac7f9f4a819dc854eaed2d6")  // quarterstaff
                                                                               };
            var buff = Helpers.CreateBuff("ShillelaghBuff",
                                          enchantment_size.Name,
                                          enchantment_size.Description,
                                          "",
                                          boneshaker.Icon,
                                          null,
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), true, true, shillelagh_types, enchantment_size),
                                          Common.createBuffContextEnchantPrimaryHandWeapon(Common.createSimpleContextValue(1), true, true, shillelagh_types, enhantment1)
                                          );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;


            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
                                                                );

            shillelagh = Helpers.CreateAbility("ShillelaghAbility",
                                               buff.Name,
                                               buff.Description,
                                               "",
                                               buff.Icon,
                                               AbilityType.Spell,
                                               Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                               AbilityRange.Personal,
                                               Helpers.minutesPerLevelDuration,
                                               "",
                                               bless_weapon.GetComponent<AbilitySpawnFx>(),
                                               bless_weapon.GetComponent<ContextRankConfig>(),
                                               Helpers.CreateRunActions(apply_buff),
                                               Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Transmutation),
                                               Common.createAbilityCasterMainWeaponCheck(shillelagh_types[0].Category, shillelagh_types[1].Category)
                                               );
            shillelagh.CanTargetSelf = true;
            shillelagh.CanTargetPoint = false;
            shillelagh.CanTargetFriends = false;
            shillelagh.CanTargetEnemies = false;
            shillelagh.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            shillelagh.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
            shillelagh.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Extend | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Heighten;
            shillelagh.AddToSpellList(Helpers.druidSpellList, 1);
            shillelagh.AddSpellAndScroll("98abe0fd52e9d7d49a4a94615acbbc60"); //boneshacker
        }


        static void createFlameBlade()
        {
            var bless_weapon = library.Get<BlueprintAbility>("831e942864e924846a30d2e0678e438b");
            var flaming_enchatment = library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121");
            var scimitar_type = library.Get<BlueprintWeaponType>("d9fbec4637d71bd4ebc977628de3daf3");
            var immaterial = Helpers.Create<NewMechanics.Immaterial>();
            BlueprintWeaponEnchantment[] flame_blade_enchantments = new BlueprintWeaponEnchantment[11];
            var fire_damage = Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.Fire);

            for (int i = 0; i < flame_blade_enchantments.Length; i++)
            {
                var flame_blade_enchant = Helpers.Create<NewMechanics.WeaponDamageChange>(w =>
                                                                                    {
                                                                                        w.bonus_damage = i;
                                                                                        w.dice_formula = new DiceFormula(1, DiceType.D8);
                                                                                        w.damage_type_description = fire_damage;
                                                                                    });
                flame_blade_enchantments[i] = Common.createWeaponEnchantment($"FlameBlade{i}Enchantment",
                                                                             "Flame Blade",
                                                                             "You transform a non-magical scimitar into a 3-foot-long, blazing beam of red-hot fire springs. Attacks with the flame blade are melee touch attacks. The blade deals 1d8 points of fire damage + 1 point per two caster levels (maximum +10). Since the blade is immaterial, your Strength modifier does not apply to the damage. If you stop wielding it, the weapon loses magical properties.",
                                                                             "",
                                                                             "",
                                                                             "",
                                                                             0,
                                                                             flaming_enchatment.WeaponFxPrefab,
                                                                             immaterial,
                                                                             flame_blade_enchant
                                                                             );
            }


            var empower = Common.createWeaponEnchantment("EmpowerWeaponEnchantment",
                                                         "Empowered",
                                                         "All variable, numeric effects of an empowered spell are increased by half including bonuses to those dice rolls.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.WeaponMetamagicDamage>(w => w.empower = true)
                                                         );
            var maximize = Common.createWeaponEnchantment("MaximizeWeaponEnchantment",
                                                         "Maximized",
                                                         "All variable, numeric effects of a spell are maximized.",
                                                         "",
                                                         "",
                                                         "",
                                                         0,
                                                         null,
                                                         Helpers.Create<NewMechanics.WeaponMetamagicDamage>(w => w.maximize = true)
                                                         );


            var empower_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Empower,
                                                                                                  true, true,
                                                                                                  new BlueprintWeaponType[] { scimitar_type }, empower);

            var maximize_buff = Common.createBuffContextEnchantPrimaryHandWeaponIfHasMetamagic(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                                                                                                  true, true,
                                                                                                  new BlueprintWeaponType[] { scimitar_type }, maximize);


            var buff = Helpers.CreateBuff("FlameBladeBuff",
                                            flame_blade_enchantments[0].Name,
                                            flame_blade_enchantments[0].Description,
                                            "",
                                            bless_weapon.Icon,
                                            null,
                                            Common.createBuffContextEnchantPrimaryHandWeapon(Helpers.CreateContextValue(AbilityRankType.DamageBonus), true, true,
                                                                                            new BlueprintWeaponType[] { scimitar_type }, flame_blade_enchantments),
                                            empower_buff,
                                            maximize_buff,
                                            Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CasterLevel, progression: ContextRankProgression.OnePlusDivStep,
                                                                            type: AbilityRankType.DamageBonus, stepLevel: 2)
                                            );
            buff.Stacking = Kingmaker.UnitLogic.Buffs.Blueprints.StackingType.Replace;

            flame_blade = library.CopyAndAdd<BlueprintAbility>(shillelagh.AssetGuid, "FlameBladeAbility", "");
            flame_blade.SetIcon(bless_weapon.Icon);
            flame_blade.SetName(buff.Name);
            flame_blade.SetDescription(buff.Description);

            flame_blade.ReplaceComponent<AbilityCasterMainWeaponCheck>(Common.createAbilityCasterMainWeaponCheck(scimitar_type.Category));
            flame_blade.ReplaceComponent<SpellComponent>(Helpers.CreateSpellComponent(Kingmaker.Blueprints.Classes.Spells.SpellSchool.Evocation));

            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                    Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes)
                                                );
            flame_blade.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(apply_buff));
            flame_blade.AvailableMetamagic = flame_blade.AvailableMetamagic | Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize;
            flame_blade.AddComponent(Helpers.CreateSpellDescriptor(SpellDescriptor.Fire));

            flame_blade.AddToSpellList(Helpers.druidSpellList, 2);
            flame_blade.AddSpellAndScroll("fbdd06f0414c3ef458eb4b2a8072e502"); //bless weapon
        }


        static void createVirtuosoPerformance()
        {
            var performance_resource = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var inspire_competence = library.Get<BlueprintActivatableAbility>("430ab3bb57f2cfc46b7b3a68afd4f74e");
            var increase_group_size = Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.BardicPerformance);
            var consume_additional_resource = Helpers.Create<NewMechanics.ConsumeResourceIfAbilitiesFromGroupActivated>(c =>
                                                                                                                        {
                                                                                                                            c.group = ActivatableAbilityGroup.BardicPerformance;
                                                                                                                            c.num_abilities_activated = 2;
                                                                                                                            c.resource = performance_resource;
                                                                                                                        }
                                                                                                                        );
            var deactivate_performance = Helpers.Create<NewMechanics.DeactivatedAbilityFromGroup>(c =>
                                                                                                    {
                                                                                                        c.group = ActivatableAbilityGroup.BardicPerformance;
                                                                                                        c.num_abilities_activated = 1;
                                                                                                    }
                                                                                                  );
            virtuoso_performance = library.CopyAndAdd<BlueprintAbility>("20b548bf09bb3ea4bafea78dcb4f3db6", "VirtuosoPerformanceAbility", ""); //echolocation
            virtuoso_performance.SetIcon(inspire_competence.Icon);
            virtuoso_performance.SetName("Virtuoso Performance");
            virtuoso_performance.SetDescription("While this spell is active, you may start a second bardic performance while maintaining another. Starting the second performance costs 2 rounds of bardic performance instead of 1. Maintaining both performances costs a total of 3 rounds of bardic performance for each round they are maintained. When this spell ends, one of the performances ends immediately.");
            virtuoso_performance.RemoveComponents<SpellListComponent>();
            virtuoso_performance.RemoveComponents<AbilityEffectRunAction>();
            virtuoso_performance.LocalizedDuration = Helpers.roundsPerLevelDuration;

            var buff = Helpers.CreateBuff("VirtuosoPerformanceBuff",
                                          virtuoso_performance.Name,
                                          virtuoso_performance.Description,
                                          "",
                                          virtuoso_performance.Icon,
                                          null,
                                          increase_group_size,
                                          Helpers.CreateAddFactContextActions(newRound: consume_additional_resource, deactivated: deactivate_performance)
                                          );
            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                                 Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Rounds),
                                                                 is_from_spell: true);

            virtuoso_performance.AddComponent(Helpers.CreateRunActions(apply_buff));
            virtuoso_performance.AddToSpellList(Helpers.bardSpellList, 4);
            virtuoso_performance.AddSpellAndScroll("33770ff24b320e343bb767815f800fc4"); //echolocation
        }


        static internal void createDeadlyJuggernaut()
        {
            var sneak_attack = library.Get<BlueprintFeature>("df4f34f7cac73ab40986bc33f87b1a3c");
            var false_life = library.Get<BlueprintAbility>("7a5b5bf845779a941a67251539545762");
            deadly_juggernaut = library.CopyAndAdd<BlueprintAbility>("779179912e6c6fe458fa4cfb90d96e10", "DeadlyJuggernautAbility", "");
            deadly_juggernaut.RemoveComponents<SpellListComponent>();
            deadly_juggernaut.ReplaceComponent<AbilitySpawnFx>(false_life.GetComponent<AbilitySpawnFx>());
            deadly_juggernaut.ReplaceComponent<SpellComponent>(false_life.GetComponent<SpellComponent>());

            deadly_juggernaut.SetIcon(sneak_attack.Icon);
            deadly_juggernaut.SetName("Deadly Juggernaut");
            deadly_juggernaut.SetDescription("With every enemy life you take, you become increasingly dangerous and difficult to stop. During the duration of the spell, you gain a cumulative +1 luck bonus on melee attack rolls, melee weapon damage rolls, Strength checks, and Strength-based skill checks as well as DR 2/— each time you reduce a qualifying opponent to 0 or few hit points (maximum +5 bonus and DR 10/—) with a melee attack.");
            deadly_juggernaut.RemoveComponents<AbilityEffectRunAction>();


            BlueprintBuff[] buffs = new BlueprintBuff[5];

            for (int i = 0; i < buffs.Length; i++)
            {
                int bonus = i + 1;
                buffs[i] = Helpers.CreateBuff($"DeadlyJuggernaut{i + 1}Buff",
                                              $"Deadly Juggernaut (+{i + 1})",
                                              deadly_juggernaut.Description,
                                              "",
                                              deadly_juggernaut.Icon,
                                              null,
                                              Common.createAttackTypeAttackBonus(Common.createSimpleContextValue(bonus), AttackTypeAttackBonus.WeaponRangeType.Melee, ModifierDescriptor.Luck),
                                              Helpers.CreateAddStatBonus(StatType.AdditionalDamage, bonus, ModifierDescriptor.Luck),
                                              Common.createAbilityScoreCheckBonus(Common.createSimpleContextValue(bonus), ModifierDescriptor.Luck, StatType.Strength),
                                              Helpers.CreateAddStatBonus(StatType.SkillAthletics, bonus, ModifierDescriptor.Luck),
                                              Common.createPhysicalDR(bonus * 2)
                                              );
            }

            var conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(buffs[0]),
                                                        new GameAction[] {Common.createContextActionApplyBuff(buffs[1], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true),
                                                                           Common.createContextActionRemoveBuff(buffs[0])
                                                                          },
                                                        new GameAction[] { Common.createContextActionApplyBuff(buffs[0], Helpers.CreateContextDuration(), is_from_spell: true, is_child: true, is_permanent: true) }
                                                       );
            for (int i = 1; i < buffs.Length; i++)
            {
                GameAction[] if_true = null;
                if (i < buffs.Length - 1)
                {
                    if_true = new GameAction[] {Common.createContextActionRemoveBuff(buffs[i]),
                                                Common.createContextActionApplyBuff(buffs[i+1],
                                                                                    Helpers.CreateContextDuration(),
                                                                                    is_from_spell: true,
                                                                                    is_child: true,
                                                                                    is_permanent:true)
                                               };
                }
                conditional = Helpers.CreateConditional(Helpers.CreateConditionHasBuff(buffs[i]),
                                                        if_true,
                                                        new GameAction[] { conditional });

            }




            var on_kill = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(conditional),
                                                                           reduce_hp_to_zero: true, 
                                                                           check_weapon_range_type: true, 
                                                                           on_initiator: true,
                                                                           range_type: AttackTypeAttackBonus.WeaponRangeType.Melee);

            var buff = Helpers.CreateBuff("DeadlyJuggernautBuff",
                                          deadly_juggernaut.Name,
                                          deadly_juggernaut.Description,
                                          "",
                                          deadly_juggernaut.Icon,
                                          null,
                                          on_kill);
            buff.Stacking = StackingType.Replace;
            var apply_buff = Common.createContextActionApplyBuff(buff,
                                                     Helpers.CreateContextDuration(bonus: Helpers.CreateContextValue(AbilityRankType.Default), rate: DurationRate.Minutes),
                                                     is_from_spell: true);
            deadly_juggernaut.AddComponent(Helpers.CreateRunActions(apply_buff));


            deadly_juggernaut.AddToSpellList(Helpers.clericSpellList, 3);
            deadly_juggernaut.AddToSpellList(Helpers.inquisitorSpellList, 3);
            deadly_juggernaut.AddToSpellList(Helpers.paladinSpellList, 3);
            deadly_juggernaut.AddSpellAndScroll("539ff89add7d8e4409ab92df30e6afee"); //lead_blades
        }


        static internal void createInvisibilityPurge()
        {
            var invisibility = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var invisibility_greater = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var divination_area = library.Get<BlueprintAbilityAreaEffect>("4ba26a4641c911d4487e3f7f11bcf801");
            var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
            area_effect.name = "InvisibilityPurgeArea";
            area_effect.AffectEnemies = true;
            area_effect.AggroEnemies = true;
            area_effect.Size = 30.Feet();
            area_effect.Shape = AreaEffectShape.Cylinder;

            var remove_invisibility = new GameAction[] { Common.createContextActionRemoveBuff(invisibility), Common.createContextActionRemoveBuff(invisibility_greater) };

            area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(unitEnter: remove_invisibility, round: remove_invisibility));
            area_effect.Fx = divination_area.Fx;
            library.AddAsset(area_effect, "");

            var see_invisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");
            var buff = Helpers.CreateBuff("InvisibilityPurgeBuff",
                                          "Invisibility Purge",
                                          "You surround yourself with a sphere of power with a radius of 30 feet that negates all forms of invisibility.\n" +
                                          "Anything invisible becomes visible while in the area.",
                                          "",
                                          see_invisibility.Icon,
                                          null,
                                          Common.createAddAreaEffect(area_effect)
                                          );

            var apply_buff = Helpers.CreateApplyBuff(buff, 
                                                     Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), 
                                                     fromSpell: true, dispellable: true, asChild: true);
            invisibility_purge = Helpers.CreateAbility("InvisibilityPurgeAbility",
                                                       buff.Name,
                                                       buff.Description,
                                                       "",
                                                       buff.Icon,
                                                       AbilityType.Spell,
                                                       Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                       AbilityRange.Personal,
                                                       Helpers.minutesPerLevelDuration,
                                                       Helpers.savingThrowNone,
                                                       Helpers.CreateRunActions(apply_buff),
                                                       Helpers.CreateSpellComponent(SpellSchool.Evocation),
                                                       see_invisibility.GetComponent<AbilitySpawnFx>(),
                                                       see_invisibility.GetComponent<ContextRankConfig>()
                                                       );
            invisibility_purge.Animation = see_invisibility.Animation;
            invisibility_purge.AnimationStyle = see_invisibility.AnimationStyle;
            invisibility_purge.CanTargetSelf = true;
            invisibility_purge.CanTargetPoint = false;
            invisibility_purge.CanTargetEnemies = false;
            invisibility_purge.CanTargetFriends = false;
            invisibility_purge.AvailableMetamagic = see_invisibility.AvailableMetamagic;

            invisibility_purge.AddToSpellList(Helpers.inquisitorSpellList, 3);
            invisibility_purge.AddToSpellList(Helpers.clericSpellList, 3);

            invisibility_purge.AddSpellAndScroll("12f4ee72c02537244b5b2bacfa236bc7"); //see invisibility scroll
        }



    }
}
