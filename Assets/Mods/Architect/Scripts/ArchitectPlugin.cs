using BepInEx;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
//using R2API;
//using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using PromethiAPI;

namespace Architect
{
    [BepInDependency("com.Promethia.PromethiAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.Promethia.Architect", "Architect", "0.0.1")]
    //[R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PrefabAPI), nameof(BuffAPI))]
    public class ArchitectPlugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource _Logger;

        public static AssetBundle ArchitectPrefabs;
        public static AssetBundle ArchitectSprites;
        public static AssetBundle ArchitectMaterials;
        public static AssetBundle ArchitectSkills;

        public void Awake()
        {
            _Logger = Logger;

            ArchitectPrefabs = AssetBundle.LoadFromFile("ArchitectPrefabs");
            ArchitectSprites = AssetBundle.LoadFromFile("ArchitectSprites");
            ArchitectMaterials = AssetBundle.LoadFromFile("ArchitectMaterials");
            ArchitectSkills = AssetBundle.LoadFromFile("ArchitectSkills");





            AddLanguageTokens();

            AddBodies();

            AddSkins();

            AddCharacter();

            SkillCatalog.getAdditionalSkillDefs += AddSkills;

            SkillCatalog.getAdditionalSkillFamilies += AddSkillFamilies;

            MasterCatalog.getAdditionalEntries += AddMasterPrefabs;

            BuffCatalog.modHelper.getAdditionalEntries += AddBuffs;

            GlobalEventManager.onServerDamageDealt += NodeProcs;

            GlobalEventManager.onCharacterDeathGlobal += NodeOnDeath;

            SetUpStatRecalcHook();
        }

        private void AddLanguageTokens()
        {
            Dictionary<string, string> NewTokens = new Dictionary<string, string>();

            NewTokens.Add("ARCHITECT_NAME", "Architect");
            NewTokens.Add("ARCHITECT_DESCRIPTION", "The Architect");

            NewTokens.Add("ARCHITECT_PRIMARY_CROSSBOW_NAME", "Integrated Crossbow");
            NewTokens.Add("ARCHITECT_PRIMARY_CROSSBOW_DESCRIPTION", "Fire a crossbow made to draw power from the elements of the universe");

            NewTokens.Add("ARCHITECT_SECONDARY_PLACENODE_NAME", "Construct Node");
            NewTokens.Add("ARCHITECT_SECONDARY_PLACENODE_DESCRIPTION", "Conjure a physical program to execute your will.");

            NewTokens.Add("ARCHITECT_UTILITY_OVERLAY_NAME", "Overlay Link");
            NewTokens.Add("ARCHITECT_UTILITY_OVERLAY_DESCRIPTION", "Link nodes together to draw out powerful effects and create a network of nodes");

            NewTokens.Add("ARCHITECT_SPECIAL_SCOPESHIFT_NAME", "Scope Shift");
            NewTokens.Add("ARCHITECT_SPECIAL_SCOPESHIFT_DESCRIPTION", "Swap the active element ( MASS // DESIGN // BLOOD )");

            NewTokens.Add("ARCHITECT_NODE_NAME", "Node");
            NewTokens.Add("ARCHITECT_NODE_SUBTITLE", "Foreign Concept");

            Language.english.SetStringsByTokens(NewTokens);
        }

        static GameObject arBody;
        private void AddBodies()
        {
            BodyCatalog.getAdditionalEntries += (List<GameObject> list) =>
            {
                list.Add(Resources.Load<GameObject>("@Architect:Assets/Prefabs/ArchitectNodeBody.prefab"));

                //arBody = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/characterbodies/EngiBody"), "ArchitectBody");
                arBody = Instantiate(Resources.Load<GameObject>("prefabs/characterbodies/EngiBody"));
                arBody.name = "ArchitectBody";
                arBody.AddComponent<ArchitectData>();
                NetworkUtils.RegisterPrefab(arBody);
                
                list.Add(arBody);
            };
        }

        private void AddSkins()
        {
            var bodyPrefab = Resources.Load<GameObject>("@Architect:Assets/Prefabs/ArchitectNodeBody.prefab");

            var renderers = bodyPrefab.GetComponentsInChildren<Renderer>(true);
            var skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>(true);
            var mdl = skinController.gameObject;

            var skin = ScriptableObject.CreateInstance<SkinDef>();
            skin.icon = null; //TODO: FIX THIS
            skin.name = "ArchitectNodeSkin";
            skin.nameToken = "";
            skin.unlockableName = "";
            skin.rootObject = mdl;
            skin.baseSkins = new SkinDef[0];
            skin.gameObjectActivations = new SkinDef.GameObjectActivation[0];
            skin.rendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo()
                {
                    defaultMaterial = ArchitectMaterials.LoadAsset<Material>("DefaultMat"),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    renderer = renderers[0]
                }
            };
            skin.meshReplacements = new SkinDef.MeshReplacement[0];
            skin.projectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skin.minionSkinReplacements = new SkinDef.MinionSkinReplacement[0];

            Array.Resize(ref skinController.skins, 1);
            skinController.skins[0] = skin;
        }

        private void AddCharacter()
        {
            SurvivorCatalog.getAdditionalSurvivorDefs += (System.Collections.Generic.List<SurvivorDef> list) =>
            {
                var myCharacter = arBody;

                myCharacter.GetComponent<CharacterBody>().baseNameToken = "ARCHITECT_NAME";

                var mySurvivorDef = new SurvivorDef
                {
                    //We're finding the body prefab here,
                    bodyPrefab = myCharacter,
                    //Description
                    descriptionToken = "ARCHITECT_DESCRIPTION",
                    //Display 
                    displayPrefab = Instantiate(Resources.Load<GameObject>("prefabs/characterdisplays/EngiDisplay")),
                    //Color on select screen
                    primaryColor = new Color(0.8039216f, 0.482352942f, 0.843137264f),
                    //Unlockable name
                    unlockableName = ""
                };
                
                list.Add(mySurvivorDef);
            };
        }

        static SkillLocator skillLocator;
        private void AddSkills(List<SkillDef> list)
        {
            skillLocator = arBody.GetComponent<SkillLocator>();

            list.Add(ArchitectSkills.LoadAsset<SkillDef>("ArchitectCrossbow"));
            list.Add(ArchitectSkills.LoadAsset<SkillDef>("ArchitectInstance"));
            list.Add(ArchitectSkills.LoadAsset<SkillDef>("ArchitectLink"));
            list.Add(ArchitectSkills.LoadAsset<SkillDef>("ArchitectScopeShift"));
            list.Add(ArchitectSkills.LoadAsset<SkillDef>("DesignNodeFireTesla"));

            var skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = ArchitectSkills.LoadAsset<SkillDef>("ArchitectCrossbow"),
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(ArchitectSkills.LoadAsset<SkillDef>("ArchitectCrossbow").skillNameToken, false, null)
            };

            skillFamily = skillLocator.secondary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = ArchitectSkills.LoadAsset<SkillDef>("ArchitectInstance"),
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(ArchitectSkills.LoadAsset<SkillDef>("ArchitectInstance").skillNameToken, false, null)
            };

            skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = ArchitectSkills.LoadAsset<SkillDef>("ArchitectLink"),
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(ArchitectSkills.LoadAsset<SkillDef>("ArchitectLink").skillNameToken, false, null)
            };

            skillFamily = skillLocator.special.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = ArchitectSkills.LoadAsset<SkillDef>("ArchitectScopeShift"),
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(ArchitectSkills.LoadAsset<SkillDef>("ArchitectScopeShift").skillNameToken, false, null)
            };
        }

        private void AddSkillFamilies(List<SkillFamily> list)
        {
            list.Add(ArchitectSkills.LoadAsset<SkillFamily>("DesignNodePrimary"));
        }

        private void AddMasterPrefabs(List<GameObject> list)
        {
            list.Add(ArchitectPrefabs.LoadAsset<GameObject>("DesignNodeMaster"));
        }

        public static BuffIndex ReduceAttackSpeed5BuffIdx { get; private set; }
        public static BuffIndex ReduceMoveSpeed25BuffIdx { get; private set; }
        public static BuffIndex ReduceArmor30BuffIdx { get; private set; }
        private void AddBuffs(List<BuffDef> list)
        {
            var ReduceAttackSpeed5Buff = new BuffDef()
            {
                name = "ReducAtkSpd5Stacking",
                iconPath = "TODO: ",
                buffColor = Color.red,
                canStack = true,
                isDebuff = true
            };
            var ReduceMoveSpeed25Buff = new BuffDef()
            {
                name = "ReducMvSpd25Stacking",
                iconPath = "TODO: ",
                buffColor = Color.green,
                canStack = true,
                isDebuff = true
            };
            var ReduceArmor30BuffIdx = new BuffDef()
            {
                name = "ReducArm30Stacking",
                iconPath = "TODO: ",
                buffColor = Color.yellow,
                canStack = true,
                isDebuff = true
            };

            list.Add(ReduceAttackSpeed5Buff);
            list.Add(ReduceMoveSpeed25Buff);
            list.Add(ReduceArmor30BuffIdx);
        }

        static readonly int MassNodeBodyIndex = BodyCatalog.FindBodyIndex("MassNodeBody");
        static readonly int DesignNodeBodyIndex = BodyCatalog.FindBodyIndex("DesignNodeBody");
        static readonly int BloodNodeBodyIndex = BodyCatalog.FindBodyIndex("BloodNodeBody");
        static bool LoadedBuffIndices = false;

        public static void NodeProcs(DamageReport report)
        {
            if (!LoadedBuffIndices)
            {
                ReduceAttackSpeed5BuffIdx = BuffCatalog.FindBuffIndex("ReducAtkSpd5Stacking");
                ReduceMoveSpeed25BuffIdx = BuffCatalog.FindBuffIndex("ReducMvSpd25Stacking");
                ReduceArmor30BuffIdx = BuffCatalog.FindBuffIndex("ReducArm30Stacking");
                LoadedBuffIndices = true;
            }    
            if (!NetworkServer.active)
                return;
            if (report.attackerBodyIndex == MassNodeBodyIndex || report.attackerBodyIndex == DesignNodeBodyIndex || report.attackerBodyIndex == BloodNodeBodyIndex)
            {
                var nodeData = report.attacker.GetComponent<NodeData>();

                var MassGreenTier1 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Mass, 0);
                var MassGreenTier2 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Mass, 1);
                var MassRedTier1 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Mass, 0);
                var MassRedTier2 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Mass, 1);
                var DesignRedTier1 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Design, 0);
                var BloodRedTier1 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Blood, 0);
                var BloodRedTier2 = nodeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Blood, 1);

                var blackHoleProc = Util.CheckRoll(MassGreenTier2);
                if (blackHoleProc)
                    ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/GravSphere"), report.attacker.transform.position, Quaternion.identity, report.attacker, 0f, 0f, false, DamageColorIndex.Default, null, 0f);

                report.attackerBody.healthComponent.AddBarrier(report.attackerBody.maxHealth * 0.1f * MassGreenTier1);

                for (int i = 0; i < BloodRedTier2; i++)
                    DotController.InflictDot(report.victim.gameObject, report.attacker, DotController.DotIndex.Bleed, 3f * report.damageInfo.procCoefficient, 1f);

                for (int i = 0; i < BloodRedTier1; i++)
                    DotController.InflictDot(report.victim.gameObject, report.attacker, DotController.DotIndex.Burn, 4f * report.damageInfo.procCoefficient, 1f);

                if (MassRedTier1 > 0)
                    report.victimBody.AddTimedBuff(ReduceMoveSpeed25BuffIdx, 1.5f * MassRedTier1);

                if (MassRedTier2 > 0)
                    report.victimBody.AddTimedBuff(ReduceAttackSpeed5BuffIdx, 0.5f * MassRedTier2);

                if (DesignRedTier1 > 0)
                    report.victimBody.AddTimedBuff(ReduceArmor30BuffIdx, 1.5f * DesignRedTier1);
            }
            if (report.victimBodyIndex == MassNodeBodyIndex || report.victimBodyIndex == DesignNodeBodyIndex || report.victimBodyIndex == BloodNodeBodyIndex)
            {
                var nodeData = report.victim.GetComponent<NodeData>();

                var DesignGreenTier1 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Design, 0);

                if (DesignGreenTier1 > 0 && report.damageDealt > report.victimBody.maxHealth / 0.1f)
                {
                    report.victimBody.AddTimedBuff(BuffIndex.Cloak, 5f * DesignGreenTier1);
                }
            }
        }

        static GameObject explodePrefab = Resources.Load<GameObject>("prefabs/effects/WilloWispExplosion");

        public static void NodeOnDeath(DamageReport report)
        {
            if (!NetworkServer.active)
                return;
            if (report.attackerBodyIndex == MassNodeBodyIndex || report.attackerBodyIndex == DesignNodeBodyIndex || report.attackerBodyIndex == BloodNodeBodyIndex)
            {
                var nodeData = report.attacker.GetComponent<NodeData>();

                var BloodGreenTier1 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Blood, 0);
                var BloodGreenTier2 = nodeData.GetMatchingInputCount(OutputColor.Green, NodeElement.Blood, 1);

                if (Util.CheckRoll(30f * BloodGreenTier1))
                {
                    Vector3 corePosition = Util.GetCorePosition(report.victim.gameObject);
                    const float damageCoefficient = 3.5f;
                    float baseDamage = Util.OnKillProcDamage(report.attackerBody.damage, damageCoefficient);
                    GameObject explosionInstance = Instantiate(explodePrefab, corePosition, Quaternion.identity);
                    DelayBlast blast = explosionInstance.GetComponent<DelayBlast>();
                    blast.position = corePosition;
                    blast.baseDamage = baseDamage;
                    blast.baseForce = 2000f;
                    blast.bonusForce = Vector3.up * 1000f;
                    blast.radius = 12f;
                    blast.attacker = report.attacker;
                    blast.inflictor = null;
                    blast.crit = Util.CheckRoll(report.attackerBody.crit, report.attackerMaster);
                    blast.maxTimer = 0.5f;
                    blast.damageColorIndex = DamageColorIndex.Item;
                    blast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    explosionInstance.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;
                    NetworkServer.Spawn(explosionInstance);
                }

                if (Util.CheckRoll(10f * BloodGreenTier2))
                {
                    Util.TryToCreateGhost(report.victimBody, report.attackerBody, 30);
                }
            }
        }

        private void SetUpStatRecalcHook()
        {
            IL.RoR2.CharacterBody.RecalculateStats += PlaceILInfo;
            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseDamage;
            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseAttackSpeed;
            IL.RoR2.CharacterBody.RecalculateStats += GrantAttackSpeedDebuffPenalty;
            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseArmor;
            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseHP;
            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseCrit;
            IL.RoR2.CharacterBody.RecalculateStats += GrantBaseRegen;
            IL.RoR2.CharacterBody.RecalculateStats += GrantMoveSpeedDebuffPenalty;
        } //PRAISE HARB

        private void PlaceILInfo(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                instr => instr.MatchLdarg(0),
                instr => instr.MatchCall<TeamManager>("get_instance")
            );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<CharacterBody>>((body) =>
            {
                var dat = body.GetComponent<NodeData>();
                
                NodeData.calculating = dat;
                NodeData.activeData = dat;
            });
        }

        private void GrantBaseDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float damageModifier = 0.25f;

            var damageLoc = 57;
            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdloc(out damageLoc),
                x => x.MatchCallvirt<CharacterBody>("set_damage")
            );

            c.Emit(OpCodes.Ldloc, damageLoc);
            c.EmitDelegate<Func<float, float>>(damage =>
            {
                if (NodeData.calculating && NodeData.activeData.form == NodeForm.Program)
                {
                    damage += damageModifier * NodeData.activeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Design, 1);
                }
                return damage;
            });
            c.Emit(OpCodes.Stloc, damageLoc);
        }

        private void GrantBaseAttackSpeed(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            var attackSpeedMod = 0.25f;
            var attackSpeedPenalty = 0.1f;

            var loc61 = 61; //Attack speed base
            var loc30 = 30; //# of something that boosts attack speed and is not syringe
            c.GotoNext( 
                MoveType.Before,
                x => x.MatchLdloc(out loc61),
                x => x.MatchLdloc(out loc30),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(0.1f),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchStloc(loc61)
            );

            c.Emit(OpCodes.Ldloc, loc61);
            c.EmitDelegate<Func<float, float>>(attackSpeed =>
            { 
                if (NodeData.calculating && NodeData.activeData.form == NodeForm.Program)
                {
                    //DBC Buff
                    var multiplier = NodeData.activeData.GetMatchingInputCount(OutputColor.Blue, NodeElement.Design, 0);

                    attackSpeed += attackSpeedMod * multiplier;

                    //DRP Penalty
                    multiplier = NodeData.activeData.GetMatchingInputCount(OutputColor.Red, NodeElement.Design, 1);

                    attackSpeed -= attackSpeedPenalty * multiplier;
                }
                return attackSpeed;
            });
            c.Emit(OpCodes.Stloc, loc61);
        }

        private void GrantAttackSpeedDebuffPenalty(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float penalty = 0.05f;

            var attackSpeedModLoc = 61;
            c.GotoNext(
                MoveType.Before,
                x => x.MatchStloc(out attackSpeedModLoc),
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(5),
                x => x.MatchCallvirt<CharacterBody>("HasBuff")
            );
            c.Goto(c.Next, MoveType.Before);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, attackSpeedModLoc);
            c.EmitDelegate<Func<CharacterBody, float, float>>((self, attackSpeed) =>
            {
                return attackSpeed - (penalty * self.GetBuffCount(ReduceAttackSpeed5BuffIdx));
            });
            c.Emit(OpCodes.Stloc, attackSpeedModLoc);
        }

        private void GrantBaseArmor(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float armorBonus = 10f;
            const float armorPenalty = 30f;

            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCallvirt<CharacterBody>("get_armor"),
                x => x.MatchLdloc(out var locAmntDrizzleHelpers),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(out var armorFromDrizzle),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchCallvirt<CharacterBody>("set_armor")
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<CharacterBody>(OpCodes.Callvirt, "get_armor");
            c.EmitDelegate<Func<CharacterBody, float, float>>((self, armor) =>
            {
                armor -= armorPenalty * self.GetBuffCount(ReduceArmor30BuffIdx);
                if (NodeData.calculating)
                {
                    var multiplier = 0;
                    if (NodeData.activeData.form == NodeForm.Program)
                        multiplier = NodeData.activeData.GetMatchingInputCount(OutputColor.Blue, NodeElement.Mass, 1);
                    else
                        multiplier = NodeData.activeData.GetMatchingInputCount(OutputColor.Blue, NodeElement.Mass, 0);
                    armor += armorBonus * multiplier;
                }
                return armor;
            });
            c.Emit<CharacterBody>(OpCodes.Callvirt, "set_armor");
        }

        private void GrantBaseHP(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float healthMultiplier = 2f;

            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCallvirt<CharacterBody>("get_maxHealth"),
                x => x.MatchLdarg(0),
                x => x.MatchCallvirt<CharacterBody>("get_cursePenalty"),
                x => x.MatchDiv(),
                x => x.MatchCallvirt<CharacterBody>("set_maxHealth")
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<CharacterBody>(OpCodes.Callvirt, "get_maxHealth");
            c.EmitDelegate<Func<float, float>>(health =>
            {
                var power = 0f;
                if (NodeData.calculating)
                {
                    power = NodeData.activeData.GetMatchingInputCount(OutputColor.Blue, NodeElement.Mass, 0);
                    return health * Mathf.Pow(healthMultiplier, power);
                }
                return health;
            });
            c.Emit<CharacterBody>(OpCodes.Callvirt, "set_maxHealth");
        }

        private void GrantBaseCrit(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float critMod = 0.1f;

            var loc62 = 62; //Crit chance
            var loc27 = 27; //Probably crit glasses
            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdloc(out loc62),
                x => x.MatchLdloc(out loc27),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(10),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchStloc(loc62)
            );

            c.Emit(OpCodes.Ldloc, loc62);
            c.EmitDelegate<Func<float, float>>(crit =>
            {
                if (NodeData.calculating && NodeData.activeData.form == NodeForm.Program)
                {
                    crit += critMod * NodeData.activeData.GetMatchingInputCount(OutputColor.Blue, NodeElement.Blood, 1);
                }
                return crit;
            });
            c.Emit(OpCodes.Stloc, loc62);
        }

        private void GrantBaseRegen(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float baseRegenMultiplierMod = 1f;

            var locRegen = 52;
            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdloc(out locRegen),
                x => x.MatchCallvirt<CharacterBody>("set_regen")
            );

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, locRegen);
            c.EmitDelegate<Func<CharacterBody, float, float>>((self, regen) =>
            {
                if (NodeData.calculating)
                {
                    regen += baseRegenMultiplierMod * (self.baseRegen + (self.levelRegen * self.level)) * NodeData.activeData.GetMatchingInputCount(OutputColor.Blue, NodeElement.Blood, 0);
                }
                return regen;
            });
            c.Emit(OpCodes.Stloc, locRegen);
        }

        private void GrantMoveSpeedDebuffPenalty(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            var moveSpeedPenalty = 0.25f;

            var moveSpeedDivLoc = 55;
            c.GotoNext(
                MoveType.Before,
                x => x.MatchLdcR4(1),
                x => x.MatchStloc(out moveSpeedDivLoc),
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(0),
                x => x.MatchCallvirt<CharacterBody>("HasBuff")
            );
            c.Goto(c.Next.Next, MoveType.Before);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, moveSpeedDivLoc);
            c.EmitDelegate<Func<CharacterBody, float, float>>((self, moveSpdDiv) =>
            {
                return moveSpdDiv + (moveSpeedPenalty * self.GetBuffCount(ReduceMoveSpeed25BuffIdx));
            });
            c.Emit(OpCodes.Stloc, moveSpeedDivLoc);
        }
    }
}