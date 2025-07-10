using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Dorlf.CustomFunctions;
using static Dorlf.Plugin;
using static Dorlf.DescriptionFunctions;
using static Dorlf.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Dorlf
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();
            string traitName = traitData?.TraitName ?? "Missing Trait";
            string traitId = _trait;

            if (!IsLivingHero(_character))
            {
                return;
            }

            if (_trait == trait0)
            {
                // Taunt on you cannot be purged unless specified. At the start of combat, gain 1 Taunt, 1 Reinforce, and 2 Fortify
                _character.SetAuraTrait(_character, "taunt", 1);
                _character.SetAuraTrait(_character, "reinforce", 1);
                _character.SetAuraTrait(_character, "fortify", 2);
            }

            else if (_trait == trait2a)
            {
                // trait2a
                // Evasion +1. 
                // Evasion on you stacks and increases All Damage by 1 per charge. 
                // When you play a Defense card, gain 1 Energy and Draw 1. (2 times/turn)

            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Taunt on you can stack up to 10. 
                // Reduce the cost of your highest cost card by 1 until discarded, repeat for every 2 Taunt on you.


                int nIterations = 1 + _character.GetAuraCharges("taunt") / 2;
                for (int i = 0; i < nIterations; i++)
                {
                    CardData highestCostCard = GetRandomHighestCostCard(Enums.CardType.None);
                    if (highestCostCard != null)
                    {
                        LogDebug($"Handling Trait {traitId}: {traitName} - Reducing cost of {highestCostCard.CardName} by 1");
                        ReduceCardCost(ref highestCostCard, _character, 1);
                    }
                }

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // When you play a Defense, reduce the cost of your highest cost Defense by 3 until discarded. (2 times/turn)



                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (CanIncrementTraitActivations(traitId))
                {
                    CardData highestCostCard = GetRandomHighestCostCard(Enums.CardType.Defense);
                    if (highestCostCard != null)
                    {
                        ReduceCardCost(ref highestCostCard, _character, 3);
                    }
                    IncrementTraitActivations(traitId);
                }
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // Taunt +1. Taunt on you increases Physical and Lightning damage by 2 per charge. Taunt on you can stack to 15.


                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

            DisplayTraitScroll(ref _character, traitData);

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait0:
                // Taunt on you cannot be purged unless specified. At the start of combat, gain 1 Taunt, 1 Reinforce, and 2 Fortify

                // trait2b:
                // Taunt on you can stack up to 10. Reduce the cost of your highest cost card by 1 until discarded, repeat for every 2 Taunt on you.

                // trait 4b:
                // Taunt +1. Taunt on you increases Physical and Lightning damage by 2 per charge. Taunt on you can stack to 15.

                case "taunt":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.Removable = false;
                    }
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.GainCharges = true;
                        __result.MaxCharges = __result.MaxMadnessCharges = 10;
                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.GainCharges = true;
                        __result.MaxCharges = __result.MaxMadnessCharges = 15;
                        __result.AuraDamageType = Enums.DamageType.Slashing;
                        __result.AuraDamageType2 = Enums.DamageType.Blunt;
                        __result.AuraDamageType3 = Enums.DamageType.Piercing;
                        __result.AuraDamageType4 = Enums.DamageType.Lightning;
                        __result.AuraDamageIncreasedPerStack = __result.AuraDamageIncreasedPerStack2 = __result.AuraDamageIncreasedPerStack3 = __result.AuraDamageIncreasedPerStack4 = 2;
                    }
                    break;
                case "shield":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.ResistModified = Enums.DamageType.All;
                        __result.ResistModifiedPercentagePerStack = 0.2f;
                    }
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetAura")]
        public static void SetAuraPrefix(
            Character __instance,
            Character theCaster,
            ref AuraCurseData _acData,
            ref int charges,
            bool fromTrait = false,
            Enums.CardClass CC = Enums.CardClass.None,
            bool useCharacterMods = true,
            bool canBePreventable = true)
        {

            string traitOfInterest = trait2a;
            if (__instance.HaveTrait(traitOfInterest) && _acData.ACName.ToLower() == "block")
            {
                // LogInfo($"SetAuraPrefix {subclassName}");
                _acData = Globals.Instance.GetAuraCurseData("shield");
                charges = Functions.FuncRoundToInt((float)charges * 0.5f);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        public static void HealAuraCursePostfix(ref Character __instance, AuraCurseData AC, int __state)
        {
            LogInfo($"HealAuraCursePrefix {subclassName}");
            string traitOfInterest = trait4b;
            if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest) && AC == GetAuraCurseData("stealth") && __state > 0)
            {
                // __state = __instance.GetAuraCharges("stealth");
                __instance.SetAuraTrait(null, "stealth", __state);
            }

        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardData), nameof(CardData.SetDescriptionNew))]
        public static void SetDescriptionNewPostfix(ref CardData __instance, bool forceDescription = false, Character character = null, bool includeInSearch = true)
        {
            // LogInfo("executing SetDescriptionNewPostfix");
            if (__instance == null)
            {
                LogDebug("Null Card");
                return;
            }
            if (!Globals.Instance.CardsDescriptionNormalized.ContainsKey(__instance.Id))
            {
                LogError($"missing card Id {__instance.Id}");
                return;
            }


            if (__instance.CardName == "Mind Maze")
            {
                StringBuilder stringBuilder1 = new StringBuilder();
                LogDebug($"Current description for {__instance.Id}: {stringBuilder1}");
                string currentDescription = Globals.Instance.CardsDescriptionNormalized[__instance.Id];
                stringBuilder1.Append(currentDescription);
                // stringBuilder1.Replace($"When you apply", $"When you play a Mind Spell\n or apply");
                stringBuilder1.Replace($"Lasts one turn", $"Lasts two turns");
                BinbinNormalizeDescription(ref __instance, stringBuilder1);
            }
        }

    }
}

