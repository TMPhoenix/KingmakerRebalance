﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild
{
    class NewRagePowers
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintFeatureSelection rage_powers_selection => Main.library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e");
        static BlueprintFeatureSelection extra_rage_power_selection => Main.library.Get<BlueprintFeatureSelection>("0c7f01fbbe687bb4baff8195cb02fe6a");
        static internal BlueprintBuff rage_buff => library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
        static BlueprintActivatableAbility reckless_stance => library.Get<BlueprintActivatableAbility>("4ee08802b8a2b9b448d21f61e208a306");
        static BlueprintCharacterClass barbarian_class => ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");

        static internal BlueprintBuff rage_marker_caster;

        static internal BlueprintFeature taunting_stance;
        static internal BlueprintFeature terrifying_howl_feature;
        static internal BlueprintAbility terrifying_howl_ability;
        static internal BlueprintFeature quick_reflexes_feature;

        static internal BlueprintFeature lesser_atavism_totem;
        static internal BlueprintFeature atavism_totem;
        static internal BlueprintFeature greater_atavism_totem;


        static internal BlueprintFeature lesser_spirit_totem;
        static BlueprintBuff lesser_spirit_totem_buff;
        static internal BlueprintFeature spirit_totem;
        static internal BlueprintFeature greater_spirit_totem;
        static internal BlueprintItemWeapon lesser_spirit_totem_slam_attack;
        static internal BlueprintItemWeapon greater_spirit_totem_slam_attack;

        static internal BlueprintFeature unrestrained_rage_feature;


        static internal BlueprintFeature lesser_celestial_totem;
        static internal BlueprintBuff lesser_celestial_totem_buff;
        static internal BlueprintFeature celestial_totem;
        static internal BlueprintFeature greater_celestial_totem;
        static internal BlueprintBuff greater_celestial_totem_buff;


        static internal BlueprintFeature lesser_daemon_totem;
        static internal BlueprintBuff lesser_daemon_totem_buff;
        static internal BlueprintFeature daemon_totem;
        static internal BlueprintFeature greater_daemon_totem;

        static internal List<BlueprintFeature> totems = new List<BlueprintFeature>(new BlueprintFeature[] { library.Get<BlueprintFeature>("d99dfc9238a8a6646b32be09057c1729") });



        static internal void load()
        {
            createRageMarker();
            createTauntingStance();
            createTerrefyingHowl();
            createQuickReflexes();

            createLesserAtavismTotem();
            createAtavismTotem();
            createGreaterAtavismTotem();

            createUnrestrainedRage();

            createLesserSpiritTotem();
            createSpiritTotem();
            createGreaterSpiritTotem();

            createLesserCelestialTotem();
            createCelestialTotem();
            createGreaterCelestialTotem();

            createLesserDaemonTotem();
            createDaemonTotem();
            createGreaterDaemonTotem();
        }


        static void createRageMarker()
        {
            rage_marker_caster = Helpers.CreateBuff("RageMarkerBuff",
                                                     "",
                                                     "",
                                                     "",
                                                     null,
                                                     null);
            rage_marker_caster.SetBuffFlags(BuffFlags.HiddenInUi);
            var conditional_caster = Helpers.CreateConditional(Common.createContextConditionIsCaster(),
                                                               Common.createContextActionApplyBuff(rage_marker_caster, Helpers.CreateContextDuration(),
                                                                                                   is_child: true, dispellable: false, is_permanent: true)
                                                              );                                                                                                                          
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, conditional_caster);
        }


        static void addToSelection(BlueprintFeature rage_power, bool is_totem = false)
        {
            extra_rage_power_selection.AllFeatures = extra_rage_power_selection.AllFeatures.AddToArray(rage_power);
            rage_powers_selection.AllFeatures = rage_powers_selection.AllFeatures.AddToArray(rage_power);

            if (!is_totem)
            {
                return;
            }
            
            foreach (var t in totems)
            {
                t.AddComponent(Helpers.PrerequisiteNoFeature(rage_power));
                rage_power.AddComponent(Helpers.PrerequisiteNoFeature(t));
            }
            totems.Add(rage_power);
        }


        static internal void createLesserDaemonTotem()
        {
            lesser_daemon_totem = Helpers.CreateFeature("LesserDaemonTotemFeature",
                                                           "Daemon Totem, Lesser",
                                                           "While raging, the barbarian gains a +2 bonus on saving throws against acid damage, death effects, disease, and poison. This bonus increases by 1 for each daemon totem rage power the barbarian has, excluding this one.",
                                                           "",
                                                           library.Get<BlueprintProgression>("e76a774cacfb092498177e6ca706064d").Icon, //infernal
                                                           FeatureGroup.RagePower);
            lesser_daemon_totem_buff = Helpers.CreateBuff("LesserDaemonTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus), 
                                                                                                ModifierDescriptor.UntypedStackable, 
                                                                                                SpellDescriptor.Acid),
                                          Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                ModifierDescriptor.UntypedStackable,
                                                                                                SpellDescriptor.Disease),
                                          Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                ModifierDescriptor.UntypedStackable,
                                                                                                SpellDescriptor.Poison),
                                          Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                ModifierDescriptor.UntypedStackable,
                                                                                                SpellDescriptor.Death)
                                          );
            lesser_daemon_totem_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, lesser_daemon_totem_buff, lesser_daemon_totem);
            addToSelection(lesser_daemon_totem, is_totem: true);
        }


        static internal void createDaemonTotem()
        {

            daemon_totem = Helpers.CreateFeature("DaemonTotemFeature",
                                                           "Daemon Totem",
                                                           "While raging, the barbarian gains a +2 bonus on saving throws against acid damage, death effects, disease, and poison. This bonus increases by 1 for each daemon totem rage power the barbarian has, excluding this one.",
                                                           "",
                                                           lesser_daemon_totem.Icon,
                                                           FeatureGroup.RagePower,
                                                           Helpers.PrerequisiteClassLevel(barbarian_class, 6),
                                                           Helpers.PrerequisiteFeature(lesser_daemon_totem));
            var undead = library.Get<BlueprintUnitFact>("734a29b693e9ec346ba2951b27987e33");
            var construct = library.Get<BlueprintUnitFact>("fd389783027d63343b4a5634bd81645f");

            var energy_drain = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Common.createSimpleContextValue(1)),
                                                               Helpers.CreateContextDuration(bonus: Common.createSimpleContextValue(1), rate: DurationRate.Hours),
                                                               Kingmaker.RuleSystem.Rules.EnergyDrainType.Temporary);
            var effect_action = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasFact(undead, not: true), Helpers.CreateConditionHasFact(construct, not: true) },
                                                                    energy_drain);
            var action = Helpers.CreateActionList(effect_action);

            var buff = Helpers.CreateBuff("DaemonTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(action, critical_hit: true)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, buff, daemon_totem);
            addToSelection(daemon_totem);
        }


        static internal void createGreaterDaemonTotem()
        {

            greater_daemon_totem = Helpers.CreateFeature("GreaterDaemonTotemFeature",
                                                           "Daemon Totem, Greater",
                                                           "If the barbarian kills a creature while raging, she heals 5 hit points.",
                                                           "",
                                                           lesser_daemon_totem.Icon,
                                                           FeatureGroup.RagePower,
                                                           Helpers.PrerequisiteClassLevel(barbarian_class, 10),
                                                           Helpers.PrerequisiteFeature(daemon_totem));

            var effect_action = Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.Zero, bonus: Common.createSimpleContextValue(5)));
            var action = Helpers.CreateActionList(effect_action);

            var buff = Helpers.CreateBuff("GreaterDaemonTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddInitiatorAttackWithWeaponTrigger(action, reduce_hp_to_zero: true, on_initiator: true)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, buff, greater_daemon_totem);
            lesser_daemon_totem_buff.AddComponent(Helpers.CreateContextRankConfig(baseValueType: Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.FeatureListRanks,
                                                                                  type: AbilityRankType.StatBonus,
                                                                                  progression: Kingmaker.UnitLogic.Mechanics.Components.ContextRankProgression.AsIs,
                                                                                  featureList: new BlueprintFeature[] { lesser_daemon_totem, lesser_daemon_totem, daemon_totem, greater_daemon_totem }
                                                                                  )
                                                 );
                                                                                    
            addToSelection(greater_daemon_totem);
        }



        static internal void createLesserCelestialTotem()
        {
            lesser_celestial_totem = Helpers.CreateFeature("LesserCelestialTotemFeature",
                                                           "Celestial Totem, Lesser",
                                                           "Whenever barbarian is subject to a spell that cures hit point damage, she heals 1 additional point of damage per caster level. In the case of non-spell healing effects (such as channeled energy or lay on hands), she heals a number of additional points equal to the class level of the character performing the magical healing. This does not affect fast healing or regeneration.",
                                                           "",
                                                           library.Get<BlueprintAbility>("75a10d5a635986641bfbcceceec87217").Icon, //angelic aspect
                                                           FeatureGroup.RagePower);
            lesser_celestial_totem_buff = Helpers.CreateBuff("LesserCelestialTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null
                                          );
            lesser_celestial_totem_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, lesser_celestial_totem_buff, lesser_celestial_totem);
            addToSelection(lesser_celestial_totem, is_totem: true);
        }


        static internal void createCelestialTotem()
        {
            var invisibility = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var invisibility_greater = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var halo_buff = library.Get<BlueprintBuff>("0b1c9d2964b042e4aadf1616f653eb95"); //asimar halo

            var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
            area_effect.name = "CelestialTotemArea";
            area_effect.AffectEnemies = true;
            area_effect.AggroEnemies = true;
            area_effect.Size = 5.Feet();
            area_effect.Shape = AreaEffectShape.Cylinder;

            var remove_invisibility = new GameAction[] { Common.createContextActionRemoveBuff(invisibility), Common.createContextActionRemoveBuff(invisibility_greater) };
            var remove_invisibility_on_condition = Helpers.CreateConditional(Helpers.CreateContextConditionAlignment(AlignmentComponent.Good, check_caster: false, not: true),
                                                                             remove_invisibility);
            area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(unitEnter: remove_invisibility_on_condition, 
                                                                           round: remove_invisibility_on_condition));
            area_effect.Fx = new PrefabLink();
            library.AddAsset(area_effect, "");

            var buff = Helpers.CreateBuff("CelestialTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          halo_buff.FxOnStart,
                                          Common.createAddAreaEffect(area_effect)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            celestial_totem = Helpers.CreateFeature("CelestialTotemFeature",
                                                    "Celestial Totem",
                                                    "This effect bestows upon the barbarian a halo of gleaming light that shines as if it were daylight and triggers an invisibility purge effect in the barbarian’s square and each adjacent square. The invisibility purge only affects non-good creatures.",
                                                    "",
                                                    lesser_celestial_totem.Icon,
                                                    FeatureGroup.RagePower,
                                                    Helpers.PrerequisiteClassLevel(barbarian_class, 8),
                                                    Helpers.PrerequisiteFeature(lesser_celestial_totem)
                                                    );
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, buff, celestial_totem);
            addToSelection(celestial_totem);
        }


        static internal void createGreaterCelestialTotem()
        {
            greater_celestial_totem_buff = Helpers.CreateBuff("GreaterCelestialTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createSavingThrowBonusAgainstAlignment(2, ModifierDescriptor.UntypedStackable, AlignmentComponent.Evil),
                                          Common.createSpellResistanceAgainstSpellDescriptor(Helpers.CreateContextValue(AbilityRankType.StatBonus), SpellDescriptor.Evil),
                                          Helpers.CreateContextRankConfig(baseValueType: Kingmaker.UnitLogic.Mechanics.Components.ContextRankBaseValueType.ClassLevel,
                                                                          progression: Kingmaker.UnitLogic.Mechanics.Components.ContextRankProgression.BonusValue,
                                                                          type: AbilityRankType.StatBonus,
                                                                          stepLevel: 11,
                                                                          classes: new BlueprintCharacterClass[]{ barbarian_class }
                                                                          )
                                          );
            greater_celestial_totem_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            greater_celestial_totem = Helpers.CreateFeature("GreaterCelestialTotemFeature",
                                                    "Celestial Totem, Greater",
                                                    "While raging, the barbarian gains spell resistance equal to 11 + the barbarian’s class level against spells with the evil descriptor. She also gains a +2 bonus on all saving throws against spells and effects from evil creatures.",
                                                    "",
                                                    lesser_celestial_totem.Icon,
                                                    FeatureGroup.RagePower,
                                                    Helpers.PrerequisiteClassLevel(barbarian_class, 12),
                                                    Helpers.PrerequisiteFeature(celestial_totem)
                                                    );
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, greater_celestial_totem_buff, greater_celestial_totem);
            addToSelection(greater_celestial_totem);
        }


        static internal void createLesserSpiritTotem()
        {
            var blur = library.Get<BlueprintAbility>("14ec7a4e52e90fa47a4c8d63c69fd5c1");

            lesser_spirit_totem_slam_attack = library.CopyAndAdd<BlueprintItemWeapon>("7445b0b255796d34495a8bca81b2e2d4", "LesserSpiritTotemSlam", "");
            Helpers.SetField(lesser_spirit_totem_slam_attack, "m_OverrideDamageDice", true);
            Helpers.SetField(lesser_spirit_totem_slam_attack, "m_DamageDice", new DiceFormula(1, DiceType.D4));
            Helpers.SetField(lesser_spirit_totem_slam_attack, "m_OverrideDamageType", true);
            Helpers.SetField(lesser_spirit_totem_slam_attack, "m_DamageType", Common.createEnergyDamageDescription(Kingmaker.Enums.Damage.DamageEnergyType.NegativeEnergy));

            var enchant = Common.createWeaponEnchantment("LesserSpiritTotemSlamEnchantment",
                                                         "Spirit", 
                                                         "Spirit weapon uses wielders Charisma modifier for attack and damage bonus.", 
                                                         "Spirit", "",
                                                         "",
                                                         0,
                                                         null,
                                                         Common.createWeaponAttackStatReplacementEnchantment(StatType.Charisma),
                                                         Common.createWeaponDamageStatReplacementEnchantment(StatType.Charisma)
                                                         );

            Common.addEnchantment(lesser_spirit_totem_slam_attack, enchant);

            lesser_spirit_totem_buff = Helpers.CreateBuff("LesserSpiritTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddSecondaryAttacks(lesser_spirit_totem_slam_attack)
                                          );
            lesser_spirit_totem_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            lesser_spirit_totem = Helpers.CreateFeature("LesserSpiritTotemFeature",
                                                        "Sprit Totem, Lesser",
                                                        "While raging, the barbarian is surrounded by spirit wisps that harass her foes. These spirits make one slam attack each round against a living foe that is adjacent to the barbarian. This slam attack is made using the barbarian’s full base attack bonus, plus the barbarian’s Charisma modifier. The slam deals 1d4 points of negative energy damage, plus the barbarian’s Charisma modifier.\n"
                                                        + "Note: Totem rage powers grant powers related to a theme.A barbarian cannot select from more than one group of totem rage powers; for example, a barbarian who selects a beast totem rage power cannot later choose to gain any of the dragon totem rage powers(any rage power with “dragon totem” in its title).",
                                                        "",
                                                        blur.Icon,
                                                        FeatureGroup.RagePower
                                                        );

            addToSelection(lesser_spirit_totem, is_totem: true);
        }


        static internal void createSpiritTotem()
        {
            var chameleon_stride_buff = library.Get<BlueprintBuff>("49786ccc94a5ee848a5637b4145b2092");


            var buff = Helpers.CreateBuff("SpiritTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          chameleon_stride_buff.FxOnStart,
                                          chameleon_stride_buff.GetComponent<AddConcealment>()
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            spirit_totem = Helpers.CreateFeature("SpiritTotemFeature",
                                                "Sprit Totem",
                                                "While raging, the spirits that surround the barbarian make it difficult for her enemies to see her. The spirits grant the barbarian a 20% miss chance against ranged attacks and melee attacks made by creatures that are not adjacent to the barbarian (typically due to reach).",
                                                "",
                                                lesser_spirit_totem.Icon,
                                                FeatureGroup.RagePower,
                                                Helpers.PrerequisiteClassLevel(barbarian_class, 6),
                                                Helpers.PrerequisiteFeature(lesser_spirit_totem)
                                                );

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, buff, spirit_totem);
            addToSelection(spirit_totem);
        }


        static internal void createGreaterSpiritTotem()
        {

            var area_effect = Helpers.Create<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbilityAreaEffect>();
            area_effect.name = "GreaterSpiritTotemAura";
            area_effect.AffectEnemies = true;
            area_effect.AggroEnemies = true;
            area_effect.Size = 5.Feet();
            area_effect.Shape = AreaEffectShape.Cylinder;
            var damage = Helpers.CreateContextDiceValue(DiceType.D8, Common.createSimpleContextValue(1));
            var damage_action = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, damage, isAoE: true);
            var conditional_damage = Helpers.CreateConditional(new Condition[] { Helpers.Create<ContextConditionIsEnemy>() },
                                                                                 damage_action);
            area_effect.AddComponent(Helpers.CreateAreaEffectRunAction(round: conditional_damage));
            area_effect.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            library.AddAsset(area_effect, "");

            greater_spirit_totem_slam_attack = library.CopyAndAdd<BlueprintItemWeapon>(lesser_spirit_totem_slam_attack.AssetGuid, "GreaterSpiritTotemSlam", "");
            Helpers.SetField(greater_spirit_totem_slam_attack, "m_DamageDice", new DiceFormula(1, DiceType.D6));
            var buff = Helpers.CreateBuff("GreaterSpiritTotemBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddSecondaryAttacks(greater_spirit_totem_slam_attack),
                                          Common.createAddAreaEffect(area_effect)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);


            greater_spirit_totem = Helpers.CreateFeature("GreaterSpiritTotemFeature",
                                                "Sprit Totem, Greater",
                                                "While raging, the spirits that surround the barbarian become dangerous to any enemy adjacent to the barbarian. Living enemies adjacent to the barbarian at the start of her turn take 1d8 points of negative energy damage. In addition slam attack deals 1d6 points of negative energy damage.",
                                                "",
                                                lesser_spirit_totem.Icon,
                                                FeatureGroup.RagePower,
                                                Helpers.PrerequisiteClassLevel(barbarian_class, 10),
                                                Helpers.PrerequisiteFeature(spirit_totem)
                                                );

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, buff, greater_spirit_totem);

            var conditional_lesser = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(greater_spirit_totem, false),
                                                                              Common.createContextConditionHasFact(lesser_spirit_totem) },
                                            Common.createContextActionApplyBuff(lesser_spirit_totem_buff, Helpers.CreateContextDuration(),
                                                                                 is_child: true, is_permanent: true, dispellable: false)
                                           );
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, conditional_lesser);
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, buff, greater_spirit_totem);
            addToSelection(greater_spirit_totem);
        }


        static internal void createUnrestrainedRage()
        {
            var buff = Helpers.CreateBuff("UnrestrainedRageEffectBuff",
                                          "",
                                          "",
                                          "",
                                          null,
                                          null,
                                          Common.createAddConditionImmunity(Kingmaker.UnitLogic.UnitCondition.Paralyzed),
                                          Common.createBuffDescriptorImmunity(SpellDescriptor.Paralysis)
                                          );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            unrestrained_rage_feature = Helpers.CreateFeature("UnrestrainedRageFeature",
                                                              "Unrestrained Rage",
                                                              "While raging, the barbarian is immune to paralysis.",
                                                              "",
                                                              null,
                                                              FeatureGroup.RagePower,
                                                              Helpers.PrerequisiteClassLevel(barbarian_class, 12)
                                                              );
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff,buff, unrestrained_rage_feature);
            addToSelection(unrestrained_rage_feature);
        }


        static internal void createLesserAtavismTotem()
        {
            var animal_fury_buff = library.Get<BlueprintBuff>("a67b51a8074ae47438280be0a87b01b6");
            var animal_fury = library.Get<BlueprintFeature>("25954b1652bebc2409f9cb9d5728bceb");
            var acid_maw = library.Get<BlueprintAbility>("75de4ded3e731dc4f84d978fe947dc67");

            var lesser_atavism_buff_size = Helpers.CreateBuff("LesserAtavismTotemBiteBuff",
                                           "",
                                           "",
                                           "",
                                           null,
                                           null,
                                           Common.createWeaponTypeSizeChange(1, library.Get<BlueprintWeaponType>("952e30e6cb40b454789a9db6e5f6dd09")) //animal fury bite
                                           );

            lesser_atavism_buff_size.SetBuffFlags(BuffFlags.HiddenInUi);
            lesser_atavism_totem = Helpers.CreateFeature("LesserAtavismTotemFeature",
                                                         "Atavism Totem, Lesser",
                                                         "The barbarian gains a bite attack; if she already has a bite attack, it deals damage as if the barbarian were one size larger."
                                                         + " Note: Totem rage powers grant powers related to a theme.A barbarian cannot select from more than one group of totem rage powers; for example, a barbarian who selects a beast totem rage power cannot later choose to gain any of the dragon totem rage powers(any rage power with \"dragon totem\" in its title)",
                                                         "",
                                                         acid_maw.Icon,
                                                         FeatureGroup.RagePower
                                                         );

            var conditional_size = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(animal_fury),
                                                                         Common.createContextConditionHasFact(lesser_atavism_totem) },
                                                        Common.createContextActionApplyBuff(lesser_atavism_buff_size, Helpers.CreateContextDuration(),
                                                                                             is_child: true, is_permanent: true, dispellable: false)
                                                       );
            var conditional_bite = Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(animal_fury, has: false),
                                                                         Common.createContextConditionHasFact(lesser_atavism_totem) },
                                            Common.createContextActionApplyBuff(animal_fury_buff, Helpers.CreateContextDuration(),
                                                                                 is_child: true, is_permanent: true, dispellable: false)
                                           );
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, conditional_size);
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(rage_buff, conditional_bite);

            addToSelection(lesser_atavism_totem, is_totem: true);
        }


        static internal void createAtavismTotem()
        {
            var ferocity = library.Get<BlueprintUnitFact>("955e356c813de1743a98ab3485d5bc69");
            var atavism_totem_buff = Helpers.CreateBuff("AtavismTotemBuff",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        null,
                                                        Helpers.CreateAddFact(ferocity)
                                                        );
            atavism_totem_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            atavism_totem = Helpers.CreateFeature("AtavismTotemFeature",
                                                  "Atavism Totem",
                                                  "The barbarian gains ferocity.",
                                                  "",
                                                  lesser_atavism_totem.Icon,
                                                  FeatureGroup.RagePower,
                                                  Helpers.PrerequisiteClassLevel(barbarian_class, 6),
                                                  Helpers.PrerequisiteFeature(lesser_atavism_totem)
                                                  );

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, atavism_totem_buff, atavism_totem);
            addToSelection(atavism_totem);
        }


        static internal void createGreaterAtavismTotem()
        {
            var overrun = library.CopyAndAdd<BlueprintAbility>("1a3b471ecea51f7439a946b23577fd70", "GreaterAtavismTrample", "");
            var trample = library.Get<BlueprintFeature>("9292099e5fd70f84fb07fbb9b8b6a5a5");

            greater_atavism_totem = Helpers.CreateFeature("AtavismTotemGreaterFeature",
                                                  "Atavism Totem, Greater",
                                                  "The barbarian gains trample.",
                                                  "",
                                                  lesser_atavism_totem.Icon,
                                                  FeatureGroup.RagePower,
                                                  Helpers.CreateAddFact(overrun),
                                                  Helpers.CreateAddFact(trample),
                                                  Helpers.PrerequisiteClassLevel(barbarian_class, 10),
                                                  Helpers.PrerequisiteFeature(atavism_totem)
                                                  );

            overrun.AddComponent(Common.createAbilityCasterHasFacts(rage_marker_caster));
            addToSelection(greater_atavism_totem);
        }




        static void createQuickReflexes()
        {
            var quick_reflexes_buff = Helpers.CreateBuff("QuickReflexesEffectBuff",
                                                         "Quick Reflexes",
                                                         "While raging, the barbarian can make one additional attack of opportunity per round.",
                                                         "",
                                                         null,
                                                         null,
                                                         Helpers.CreateAddStatBonus(StatType.AttackOfOpportunityCount, 1, Kingmaker.Enums.ModifierDescriptor.UntypedStackable)
                                                         );
            quick_reflexes_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            
            quick_reflexes_feature = Helpers.CreateFeature("QuickReflexesEffectFeature",
                                                           quick_reflexes_buff.Name,
                                                           quick_reflexes_buff.Description,
                                                           "",
                                                           null,
                                                           FeatureGroup.RagePower);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rage_buff, quick_reflexes_buff, quick_reflexes_feature);
            addToSelection(quick_reflexes_feature);
        }


        static void createTerrefyingHowl()
        {
            var dazzling_display = library.Get<BlueprintAbility>("5f3126d4120b2b244a95cb2ec23d69fb");
            terrifying_howl_ability = library.CopyAndAdd<BlueprintAbility>("08cb5f4c3b2695e44971bf5c45205df0", "TerrifyingHowlAbility", "");
            terrifying_howl_ability.SetName("Terrifying Howl");
            terrifying_howl_ability.SetDescription("The barbarian unleashes a terrifying howl as a standard action. All shaken enemies within 30 feet must make a Will save (DC equal to 10 + 1/2 the barbarian’s level + the barbarian’s Strength modifier) or be frightened for 1d4+1 rounds.\n"
                                            + "Once an enemy has made a save versus terrifying howl (successful or not), it is immune to this power for 24 hours.");
            terrifying_howl_ability.Type = AbilityType.Extraordinary;
            terrifying_howl_ability.RemoveComponents<SpellComponent>();
            terrifying_howl_ability.RemoveComponents<SpellListComponent>();
            terrifying_howl_ability.Range = AbilityRange.Personal;

            var frighteneed_buff = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var shaken_buff = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var cooldown_buff = Helpers.CreateBuff("TerrifyingHowlCooldownBuff",
                                                   "Cooldown: Terrifying Howl",
                                                   terrifying_howl_ability.Description,
                                                   "",
                                                   terrifying_howl_ability.Icon,
                                                   null);
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest);
            var on_failed_save = Common.createContextSavedApplyBuff(frighteneed_buff,
                                                                    Helpers.CreateContextDuration(Common.createSimpleContextValue(1),
                                                                                                  Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds,
                                                                                                  Kingmaker.RuleSystem.DiceType.D4,
                                                                                                  Common.createSimpleContextValue(1)
                                                                                                  ),
                                                                    is_dispellable: false
                                                                    );

            var apply_cooldown = Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1),
                                                                                                                  Kingmaker.UnitLogic.Mechanics.DurationRate.Days),
                                                                    dispellable: false
                                                                    );
            PrefabLink p = new PrefabLink();
            p.AssetId = "cbfe312cb8e63e240a859efaad8e467c";
            var fx = Common.createContextActionSpawnFx(p);


            var condition = Helpers.CreateConditional(new Condition[] { Helpers.CreateConditionHasBuffFromCaster(cooldown_buff, true),
                                                                        Helpers.CreateConditionHasFact(shaken_buff),
                                                                        Helpers.Create<ContextConditionIsEnemy>()
                                                                        },
                                                       Common.createContextActionSavingThrow(SavingThrowType.Will, Helpers.CreateActionList(on_failed_save, apply_cooldown, fx)));
            condition.ConditionsChecker.Operation = Operation.And;

            terrifying_howl_ability.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(condition));
            terrifying_howl_ability.AddComponent(Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[]{barbarian_class}, StatType.Strength));
            terrifying_howl_ability.AddComponent(dazzling_display.GetComponent<AbilitySpawnFx>());
            terrifying_howl_ability.AddComponent(Common.createAbilityCasterHasFacts(rage_marker_caster)); // allow to use only on rage
            terrifying_howl_feature = Common.AbilityToFeature(terrifying_howl_ability, false);
            terrifying_howl_feature.Groups = new FeatureGroup[] { FeatureGroup.RagePower };
            terrifying_howl_feature.AddComponent(Helpers.PrerequisiteClassLevel(barbarian_class, 8));
            addToSelection(terrifying_howl_feature);
        }


        static void createTauntingStance()
        {
            var shout = library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162");



            var buff = Helpers.CreateBuff("TauntingStanceEffectBuff",
                                          "Taunting Stance",
                                          "The barbarian can leave herself open to attacks while preparing devastating counterattacks. Enemies gain a +4 bonus on attack and damage rolls against the barbarian while she’s in this stance, but every attack against the barbarian provokes an attack of opportunity from her. This is a stance rage power.",
                                          "",
                                          shout.Icon,
                                          null,
                                          Common.createComeAndGetMe()
                                          );

            taunting_stance = Common.createSwitchActivatableAbilityBuff("TauntingStance", "", "", "",
                                                      buff, rage_buff,
                                                      reckless_stance.ActivateWithUnitAnimation,
                                                      ActivatableAbilityGroup.BarbarianStance,
                                                      command_type: CommandType.Standard);

            taunting_stance.AddComponent(Helpers.PrerequisiteClassLevel(barbarian_class, 12));
            taunting_stance.Groups = new FeatureGroup[] { FeatureGroup.RagePower };
            addToSelection(taunting_stance);
        }
    }

    [Harmony12.HarmonyPatch(typeof(ContextActionHealTarget))]
    [Harmony12.HarmonyPatch("RunAction", Harmony12.MethodType.Normal)]
    class Patch_ContextActionHealTarget_RunAction_Postfix
    {

        static public void Postfix(ContextActionHealTarget __instance)
        {
            var tr = Harmony12.Traverse.Create(__instance);
            var context = tr.Property("Context").GetValue<MechanicsContext>();
            var target = tr.Property("Target").GetValue<TargetWrapper>().Unit;
            if (target == null || context.MaybeCaster == null)
                return;

            if (target.Descriptor.Buffs.HasFact(NewRagePowers.lesser_celestial_totem_buff) && context.SourceAbility != null)
            {
                int bonus = context.Params.CasterLevel;
                context.TriggerRule<RuleHealDamage>(new RuleHealDamage(target, target, DiceFormula.Zero, bonus));
            }
        }
    }
}
