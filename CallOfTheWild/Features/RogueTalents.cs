﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallOfTheWild
{
    public class RogueTalents
    {
        static public BlueprintFeatureSelection minor_magic;
        static public BlueprintFeatureSelection major_magic;
        static public BlueprintFeatureSelection feat;
        static public BlueprintFeature bleeding_attack;
        static LibraryScriptableObject library => Main.library;

        static public void load()
        {
            createMinorMagic();
            createMajorMagic();

            createFeatAndFixCombatTrick();
            createBleedingAttack();
        }


        static void createBleedingAttack()
        {
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var icon = NewSpells.deadly_juggernaut.Icon;
            var effect_buff = Helpers.CreateBuff("RogueBleedingAttackEffectBuff",
                                          "Bleeding Attack Effect",
                                          "A rogue with this ability can cause living opponents to bleed by hitting them with a sneak attack. This attack causes the target to take 1 additional point of damage each round for each die of the rogue’s sneak attack (e.g., 4d6 equals 4 points of bleed). Bleeding creatures take that amount of damage every round at the start of each of their turns. The bleeding can be stopped by a successful DC 15 Heal check or the application of any effect that heals hit point damage. Bleed damage from this ability does not stack with itself. Bleed damage bypasses any damage reduction the creature might possess.",
                                          "",
                                          icon,
                                          null,
                                          Helpers.Create<BleedMechanics.BleedBuff>(b => b.dice_value = Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default))),
                                          Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, customProperty: NewMechanics.SneakAttackDiceGetter.Blueprint.Value),
                                          Helpers.CreateSpellDescriptor(SpellDescriptor.Bleed),
                                          bleed1d6.GetComponent<CombatStateTrigger>(),
                                          bleed1d6.GetComponent<AddHealTrigger>()
                                          );

            var apply_buff = Common.createContextActionApplyBuff(effect_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true);
            var buff = Helpers.CreateBuff("RogueBleedingAttackBuff",
                                            "Bleeding Attack",
                                            effect_buff.Description,
                                            "",
                                            icon,
                                            null,
                                            Helpers.Create<AddInitiatorAttackRollTrigger>(a =>
                                                                                            {
                                                                                                a.OnlyHit = true;
                                                                                                a.SneakAttack = true;
                                                                                                a.Action = Helpers.CreateActionList(apply_buff);
                                                                                            }
                                                                                          )
                                            );

            var toggle = Helpers.CreateActivatableAbility("RogueBleedingAttackToggleAbility",
                                                          buff.Name,
                                                          buff.Description,
                                                          "",
                                                          buff.Icon,
                                                          buff,
                                                          AbilityActivationType.Immediately,
                                                          UnitCommand.CommandType.Free,
                                                          null
                                                          );
            toggle.Group = ActivatableAbilityGroupExtension.SneakAttack.ToActivatableAbilityGroup();
            toggle.DeactivateImmediately = true;

            bleeding_attack = Common.ActivatableAbilityToFeature(toggle, false);
            addToTalentSelection(bleeding_attack);

            var medical_discoveries = library.Get<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");
            medical_discoveries.AllFeatures = medical_discoveries.AllFeatures.AddToArray(bleeding_attack);
        }


        static void createFeatAndFixCombatTrick()
        {
            var combat_trick = library.Get<BlueprintFeatureSelection>("c5158a6622d0b694a99efb1d0025d2c1");
            combat_trick.AddComponent(Helpers.PrerequisiteNoFeature(combat_trick));

            feat = library.CopyAndAdd<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45", "RogueTalentFeat", "");
            feat.SetDescription(" A rogue can gain any feat that she qualifies for in place of a rogue talent.");
            feat.AddComponents(Helpers.PrerequisiteNoFeature(feat),
                               Helpers.PrerequisiteFeature(library.Get<BlueprintFeature>("a33b99f95322d6741af83e9381b2391c"))
                               );
            addToTalentSelection(feat);
        }


        static void addToTalentSelection(BlueprintFeature f)
        {
            var selections =
                new BlueprintFeatureSelection[]
                {
                    library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93"), //rogue talent
                    library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118"), //slayer talent2
                    library.Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8"), //slayer talent6
                    library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66"), //slayerTalent10
                    Investigator.investigator_talent_selection
                };

            foreach (var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(f);
            }
        }


        static void createMinorMagic()
        {
            var spells = Helpers.wizardSpellList.SpellsByLevel[0].Spells;

            minor_magic = Helpers.CreateFeatureSelection("MinorMagicRogueTalent",
                                                         "Minor Magic",
                                                         "A rogue with this talent gains the ability to cast a 0-level spell from the sorcerer/wizard spell list. This spell can be cast three times a day as a spell-like ability. The caster level for this ability is equal to the rogue’s level. The save DC for this spell is 10 + the rogue’s Intelligence modifier.",
                                                         "",
                                                         Helpers.GetIcon("16e23c7a8ae53cc42a93066d19766404"), //jolt
                                                         FeatureGroup.RogueTalent,
                                                         Helpers.PrerequisiteStatValue(StatType.Intelligence, 10)
                                                         );

            var classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"), //rogue
                                                         library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"), //slayer
                                                         Investigator.investigator_class };


            foreach (var s in spells)
            {
                BlueprintFeature feature = null;
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "MinorMagic", classes, StatType.Intelligence, no_resource: true, no_scaling: true,
                                                               guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid));                   
                    feature = Common.AbilityToFeature(spell_like, false);
                    feature.AddComponent(Helpers.Create<BindAbilitiesToClass>(b =>
                                                                                {
                                                                                    b.Abilites = new BlueprintAbility[] { spell_like };
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = true;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                }
                                                                                )
                                                                            );
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        spell_likes.Add(Common.convertToSpellLike(v, "MinorMagic", classes, StatType.Intelligence, no_resource: true, no_scaling: true,
                                                                  guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", v.AssetGuid)));
                    }
                    var wrapper = Common.createVariantWrapper("MinorMagic" + s.name, Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid), spell_likes.ToArray());
                    wrapper.SetNameDescriptionIcon(s.Name, s.Description, s.Icon);
                    feature = Common.AbilityToFeature(wrapper, false);
                    feature.AddComponent(Helpers.Create<BindAbilitiesToClass>(b =>
                                                                                {
                                                                                    b.Abilites = spell_likes.ToArray();
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = true;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                }
                                                                                )
                                                                             );
                }
                feature.SetName("Minor Magic: " + feature.Name);
                feature.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent };
                minor_magic.AllFeatures = minor_magic.AllFeatures.AddToArray(feature);
            }
            addToTalentSelection(minor_magic);                                                       
        }


        static void createMajorMagic()
        {
            var spells = Helpers.wizardSpellList.SpellsByLevel[1].Spells;

            major_magic = Helpers.CreateFeatureSelection("MajorMagicRogueTalent",
                                                         "Major Magic",
                                                         "A rogue with this talent gains the ability to cast a 1st-level spell from the sorcerer/wizard spell list once per day as a spell-like ability for every 2 rogue levels she possesses. The rogue’s caster level for this ability is equal to her rogue level. The save DC for this spell is 11 + the rogue’s Intelligence modifier. A rogue must have the minor magic rogue talent and an Intelligence score of at least 11 to select this talent.",
                                                         "",
                                                         Helpers.GetIcon("4ac47ddb9fa1eaf43a1b6809980cfbd2"), //magic missile
                                                         FeatureGroup.RogueTalent,
                                                         Helpers.PrerequisiteStatValue(StatType.Intelligence, 11),
                                                         Helpers.PrerequisiteFeature(minor_magic)
                                                         );

            var classes = new BlueprintCharacterClass[] {library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"), //rogue
                                                         library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"),
                                                         Investigator.investigator_class };


            foreach (var s in spells)
            {
                var resource = Helpers.CreateAbilityResource("MajorMagic" + s.name + "Resource", "", "", Helpers.MergeIds("27fea41a99cd46609f8ab2283d1afce0", s.AssetGuid), null);
                resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 0, 0.0f, classes);
                BlueprintFeature feature = null;
                if (!s.HasVariants)
                {
                    var spell_like = Common.convertToSpellLike(s, "MajorMagic", classes, StatType.Intelligence, resource, no_scaling: true,
                                                               guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid));
                    feature = Common.AbilityToFeature(spell_like, false);
                    spell_like.AddComponent(Helpers.Create<NewMechanics.BindAbilitiesToClassFixedLevel>(b =>
                                                                                {
                                                                                    b.Abilites = new BlueprintAbility[] { spell_like };
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = false;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.fixed_level = 1;
                                                                                }
                                                                                )
                                                                            );
                }
                else
                {
                    List<BlueprintAbility> spell_likes = new List<BlueprintAbility>();
                    foreach (var v in s.Variants)
                    {
                        spell_likes.Add(Common.convertToSpellLike(v, "MajorMagic", classes, StatType.Intelligence, resource, no_scaling: true,
                                                                  guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", v.AssetGuid)));
                    }
                    var wrapper = Common.createVariantWrapper("MajorMagic" + s.name, guid: Helpers.MergeIds("0e6a36ac029049c98a682f688e419f45", s.AssetGuid), spell_likes.ToArray());
                    wrapper.SetNameDescriptionIcon(s.Name, s.Description, s.Icon);
                    feature = Common.AbilityToFeature(wrapper, false);
                    feature.AddComponent(Helpers.Create<NewMechanics.BindAbilitiesToClassFixedLevel>(b =>
                                                                                {
                                                                                    b.Abilites = spell_likes.ToArray();
                                                                                    b.CharacterClass = classes[0];
                                                                                    b.AdditionalClasses = classes.Skip(1).ToArray();
                                                                                    b.Cantrip = false;
                                                                                    b.Stat = StatType.Intelligence;
                                                                                    b.fixed_level = 1;
                                                                                }
                                                                                )
                                                                             );
                }
                feature.SetName("Major Magic: " + feature.Name);
                feature.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent };
                feature.AddComponent(resource.CreateAddAbilityResource());
                major_magic.AllFeatures = major_magic.AllFeatures.AddToArray(feature);
            }
            addToTalentSelection(major_magic);
        }




    }
}
