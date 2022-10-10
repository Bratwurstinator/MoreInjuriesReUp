using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using Verse.Sound;
using HarmonyLib;
using HarmonyMod;
using Verse;
using UnityEngine;
namespace Oof
{
    public class OofMod : Mod
    {
        ToggleSettings settings;
        public OofMod(ModContentPack content) : base(content)
        {
           

            this.settings = GetSettings<ToggleSettings>();

            var harmony = new Harmony("Caulaflower.Extended_Injuries.oof");
            try
            {
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Log.Error("Failed to apply 1 or more harmony patches! See error:");
                Log.Error(e.ToString());
            }
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            if (settings.AdrenalineBool)
            {
                listingStandard.CheckboxLabeled("Turn off adrenaline mechanics", ref settings.AdrenalineBool, "Turn off adrenaline mechanics");
            }
            else
            {
                listingStandard.CheckboxLabeled("Turn on adrenaline mechanics", ref settings.AdrenalineBool, "Turn on adrenaline mechanics");
            }
            if (settings.HydroStaticShockBool)
            {
                listingStandard.CheckboxLabeled("Turn off hyrdostatic shock mechanics (they're quite unrealstic, so up to you whether you want them)", ref settings.HydroStaticShockBool, "Turn off hyrdostatic shock mechanics");
            }
            else
            {
                listingStandard.CheckboxLabeled("Turn on hyrdostatic shock mechanics (they're quite unrealstic, so up to you whether you want them)", ref settings.HydroStaticShockBool, "Turn on hyrdostatic shock mechanics");
            }
            if (settings.BruiseStroke)
            {
                listingStandard.CheckboxLabeled("Turn off bruise shock mechanics", ref settings.BruiseStroke, "Turn off bruise shock mechanics");
            }
            else
            {
                listingStandard.CheckboxLabeled("Turn on bruise shock mechanics", ref settings.BruiseStroke, "Turn on bruise shock mechanics");
            }
            if (settings.chocking)
            {
                listingStandard.CheckboxLabeled("Turn off chocking on blood mechanics", ref settings.BruiseStroke, "Turn off chocking on blood mechanics");
            }
            else
            {
                listingStandard.CheckboxLabeled("Turn on chocking on blood mechanics", ref settings.chocking, "Turn on chocking on blood mechanics");
            }
            if (settings.somesound)
            {
                listingStandard.CheckboxLabeled("Turn off chocking sounds", ref settings.somesound, "Turn off chocking sounds");
            }
            else
            {
                listingStandard.CheckboxLabeled("Turn on chocking sounds", ref settings.somesound, "Turn on chocking sounds");
            }





            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "More Injuries";
        }
    }
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class Patch_Thing_TakeDamage
    {
        public static bool Active = false;

        static void Postfix(Thing __instance, DamageWorker.DamageResult __result)
        {
            if (!Active)
                return;

            if (__instance is Pawn compHolder)
            {
                var comp = compHolder.GetComp<InjuriesComp>();
                comp.PostDamageFull(__result);
            }

            Active = false;
        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerCarryAdder
    {
        // Token: 0x06000009 RID: 9 RVA: 0x00002150 File Offset: 0x00000350
        [HarmonyPostfix]
        public static void AddHumanlikeOrdersPostfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            bool flag = !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Caring) || pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) || !pawn.workSettings.WorkIsActive(WorkTypeDefOf.Doctor);
            if (!flag)
            {
                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn)))
                {
                    Pawn target = (Pawn)localTargetInfo.Thing;
                    bool flag2 = !target.health.hediffSet.HasHediff(Caula_DefOf.ChockingOnBlood);
                    if(pawn.inventory.innerContainer.ToList().FindAll(AAA => AAA.def == Caula_DefOf.suctiondevice) != null)
                    {
                        bool flag69nice = pawn.inventory.innerContainer.ToList().FindAll(AAA => AAA.def == Caula_DefOf.suctiondevice).Count > 0;
                        if (flag69nice)
                        {
                            if (!flag2)
                            {
                                bool flag3 = !pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true);
                                if (!flag3)
                                {
                                    JobDef stabilizeJD = Caula_DefOf.ClearAirway;
                                    Action action = delegate ()
                                    {
                                        Job job = new Job(stabilizeJD, target);
                                        job.count = 1;
                                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                    };
                                    string label = "Clear airways";
                                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.RescueOrCapture, null, target, 0f, null, null), pawn, target, "ReservedBy"));
                                }
                            }
                        }
                        
                    }
                    
                   
                }
            }
        }
    }
    public class InjuriesComp : ThingComp
    {
        public int zupa = 1;
       
        //public void Tourniqueting()
        //{
            //Pawn myself = (Pawn)this.parent;
            //List<New_Injury> injuries = myself.health.hediffSet.GetHediffs<New_Injury>().ToList();
            //List<New_Injury> bleeding_injuries_overall = injuries.FindAll(BleedingInjury => BleedingInjury.Bleeding == true);
            //List<New_Injury> bleeding_injuries_on_armone = injuries.FindAll(BodyPart => BodyPart.Part);

        //}//
       
        public void NeedGetting(Need need)
        {
            pawns_need = need;
        }
        public InjuriesCompProps Props => (InjuriesCompProps)this.props;
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            Pawn papa = this.parent as Pawn;
            //List<New_Injury> BreathingWaysInjuries = papa.health.hediffSet.GetInjuriesTendable().ToList().Where(ABC => ABC.source == ThingDefOf.)
            foreach (Hediff_Injury injury in papa.health.hediffSet.GetInjuriesTendable())
            {
                //injury.StoppedBleeding = true; 
               // Log.Error(injury.Part.def.defName);
                if(injury.Part.def.tags.Contains(BodyPartTagDefOf.BreathingSource) | injury.Part.def.tags.Contains(BodyPartTagDefOf.BreathingPathway) && injury.Bleeding && injury.BleedRate >= 0.20f)
                {
                    Log.Error("Cough");
                    papa.health.AddHediff(Caula_DefOf.ChockingOnBlood);
                }


            }
           
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
           

            Oof.Patch_Thing_TakeDamage.Active = true;
            this.pope = dinfo;
            base.PostPreApplyDamage(dinfo, out absorbed);
            
        }
        public Need pawns_need;
        public override void Initialize(CompProperties props)
        {
            
            Pawn lowan = this.parent as Pawn;
            //pawns_need.CurLevelPercentage = 0.5f;
           
            //lowan.needs.TryGetNeed<Need_Adrenaline>.
            base.Initialize(props);
        }

        public void DumpAdrenaline(float DealtDamageChance)
        {
            Pawn lowan = this.parent as Pawn;
            //Log.Error("lul?");
           
            if (Rand.Chance(DealtDamageChance))
            {
                if (!lowan.health.hediffSet.HasHediff(Caula_DefOf.adrenalinedump))
                {
                    lowan.health.AddHediff(HediffMaker.MakeHediff(Caula_DefOf.adrenalinedump, lowan));
                    Hediff AdrenalineOnPawn = lowan.health.hediffSet.GetFirstHediffOfDef(Caula_DefOf.adrenalinedump);
                    float bloat = Rand.Range(DealtDamageChance * - 10f, DealtDamageChance * 2);
                    AdrenalineOnPawn.Severity = bloat;
                   
                    

                }
                else
                {
                    float bloat = Rand.Range(DealtDamageChance * -10f, DealtDamageChance * 2);
                    lowan.health.hediffSet.GetFirstHediffOfDef(Caula_DefOf.adrenalinedump).Severity += bloat;
                }
            }
            
        }
        public void Bruise()
        {
            Pawn myself = this.parent as Pawn;
            
            List<Hediff> Bruises = myself.health.hediffSet.hediffs.FindAll(Bruise => Bruise.def == HediffDefOf.Bruise);
            List<Hediff> SevereBruises = Bruises.FindAll(SevereBruise => SevereBruise.Severity >= 14);
            List<Hediff> LegBruises = Bruises.FindAll(LegBruise => LegBruise.sourceBodyPartGroup == BodyPartGroupDefOf.Legs);
            if (Bruises?.Count > 15 | SevereBruises?.Count > 10 | LegBruises.Count > 5)
            {
                if (Rand.Chance(0.07f))
                {
                    myself.health.AddHediff(Caula_DefOf.hemorrhagicstroke, myself.health.hediffSet.GetBrain());
                }
            }
           

        }
        public void PuttingOnTourniqet()
        {
            Pawn myself = this.parent as Pawn;
            List<New_Injury> TendableInjuries = new List<New_Injury>();
            if (myself.health.hediffSet.GetInjuriesTendable() != null)
            {
                TendableInjuries = myself.health.hediffSet.GetInjuriesTendable() as List<New_Injury>;
            }
            else
            {
                Log.Error("Tendable Injuries is null");
            }
           
            //List<New_Injury> BleedingInjuries = TendableInjuries.FindAll(Bleed => Bleed.Bleeding == true);
            //if (myself.health.hediffSet.hediffs.FindAll(Tourniqet => Tourniqet.def == Caula_DefOf.turnip) != null)
            //{
                //List<Hediff> TourniqetHediffs = myself.health.hediffSet.hediffs.FindAll(Tourniqet => Tourniqet.def == Caula_DefOf.turnip);
            //}
            //else
            //{
                //Log.Error("Turnip is null");
            //}
           
            
            List<New_Injury> Injuries_to_Turnip = new List<New_Injury>();
           
            

        }
        public DamageInfo pope;
        

        public ToggleSettings Settings
        {
            get
            {
                return LoadedModManager.GetMod<OofMod>().GetSettings<ToggleSettings>();
            }
        }
        public void PostDamageFull(DamageWorker.DamageResult damage)
        {


            
            Pawn myself = (Pawn)this.parent;
            if (Settings.BruiseStroke)
            {
                Bruise();
            }
            //PuttingOnTourniqet();
            if (damage.LastHitPart != null && damage != null) 
            {
                BodyPartRecord dahitpart = damage.LastHitPart;
            }
            
            if (LoadedModManager.GetMod<OofMod>().GetSettings<ToggleSettings>().HydroStaticShockBool)
            {
                if (!damage.diminished && damage != null)
                {
                    if (damage.totalDamageDealt > 31)
                    {
                        if (pope.Def == DamageDefOf.Bullet)
                        {
                            if (Rand.Chance(0.10f))
                            {
                                myself.health.AddHediff(HediffMaker.MakeHediff(Caula_DefOf.hemorrhagicstroke, myself));
                            }
                        }
                    }

                }
            }
            
            List<BodyPartRecord> list = new List<BodyPartRecord>();
            foreach (BodyPartRecord bodyPartRecord in myself.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Inside, BodyPartTagDefOf.BreathingSource, null))
            {
                //Log.Error("Sus");

                if (bodyPartRecord.def.tags.Contains(BodyPartTagDefOf.BreathingSource))
                {
                    if (myself.health.hediffSet.GetPartHealth(bodyPartRecord) < bodyPartRecord.def.GetMaxHealth(myself) - 5)
                    {
                        if (Rand.Chance(0.2f))
                        {
                            myself.health.AddHediff(Caula_DefOf.LungCollapse, bodyPartRecord);
                        }
                        
                    }
                    
                   

                }
            }
            if (BodyPartTagDefOf.BreathingSource == null)
            {
                Log.Error("WTF GAME WHY");
            }
            if (Caula_DefOf.Shock == null)
            {
                Log.Error("Shock is null?");
            }
            if (Caula_DefOf.LungCollapse == null)
            {
                Log.Error("Shock is null?");
            }
            Hediff lol = HediffMaker.MakeHediff(HediffDefOf.Stab, myself) as New_Injury;
            
            //calls my own function
            if (LoadedModManager.GetMod<OofMod>().GetSettings<ToggleSettings>().AdrenalineBool)
            {
                DumpAdrenaline(damage.totalDamageDealt);
            }
           
               
            if (damage == null)
            {
                Log.Message("damage is null");
            }
           
            //checks if there concussion hediffdef exists
            if (myself != null && Props.Concussion != null && damage != null)
            {
                if (Rand.Chance(0.3f))
                {
                    if (myself.HitPoints < myself.MaxHitPoints / 2)
                    {
                        if (myself.health.hediffSet.GetInjuredParts().Count() > 10)
                        {
                            if (myself.health.hediffSet.HasHediff(HediffDefOf.BloodLoss))
                            {
                                if (myself.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss).Severity > 0.20f)
                                {
                                    myself.health.AddHediff(HediffMaker.MakeHediff(Props.Shock, myself));
                                }

                            }
                        }
                    }
                }
                

                if (damage.LastHitPart?.groups?.Contains(BodyPartGroupDefOf.FullHead) ?? false)
                {
                   
                    if (Rand.Chance(20 + damage.totalDamageDealt /100))
                    {
                        if (!myself.health.hediffSet.HasHediff(Props.Concussion))
                        {
                            myself.health.AddHediff(HediffMaker.MakeHediff(Props.Concussion, myself) );
                        }
                    }
                }
                
               
                    
                    
                
            }

        }
       

    }
    public class ToggleSettings : ModSettings
    {

        public bool AdrenalineBool;
        public bool HydroStaticShockBool;
        public bool BruiseStroke;
        public bool chocking;
        public bool somesound;
       
      

       
        public override void ExposeData()
        {
            Scribe_Values.Look(ref AdrenalineBool, "ModSetting");
            Scribe_Values.Look(ref HydroStaticShockBool, "ModSetting");
            Scribe_Values.Look(ref BruiseStroke, "ModSetting");
            Scribe_Values.Look(ref chocking, "ModSetting");
            Scribe_Values.Look(ref somesound, "ModSetting");
            


            base.ExposeData();
        }
    }

    public class InjuriesCompProps : CompProperties
    {
        public HediffDef Concussion;
        public HediffDef Shock;
        public NeedDef polak;
        

        public InjuriesCompProps()
        {
            this.compClass = typeof(Oof.InjuriesComp);
        }

        public InjuriesCompProps(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }

    }

    [DefOf]
    public static class Caula_DefOf
    {



        
        public static HediffDef Shock;
        public static HediffDef Concussion;
        public static HediffDef adrenalinedump;
        public static HediffDef LungCollapse;
        public static HediffDef hemorrhagicstroke;
        public static BodyPartGroupDef Arms;
        public static HediffDef ChockingOnBlood;
        public static JobDef ClearAirway;
        public static ThingDef suctiondevice;





    }
    public class fixlung : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
          
            usedBy.health.hediffSet.hediffs.RemoveAll(async => async.def == Caula_DefOf.ChockingOnBlood);
            
        }
    }
    public class New_Injury : Hediff_Injury
    {
       
        public override int UIGroupKey
        {
            get
            {
                int num = base.UIGroupKey;
                if (this.IsTended())
                {
                    num = Gen.HashCombineInt(num, 152235495);
                }
                return num;
            }
        }

    
        public override string LabelBase
        {
            get
            {
                HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
                if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent)
                {
                    if (base.Part.def.delicate && !hediffComp_GetsPermanent.Props.instantlyPermanentLabel.NullOrEmpty())
                    {
                        return hediffComp_GetsPermanent.Props.instantlyPermanentLabel;
                    }
                    if (!hediffComp_GetsPermanent.Props.permanentLabel.NullOrEmpty())
                    {
                        return hediffComp_GetsPermanent.Props.permanentLabel;
                    }
                }
                return base.LabelBase;
            }
        }

       
        public override string LabelInBrackets
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.LabelInBrackets);
                if (this.sourceHediffDef != null)
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Append(this.sourceHediffDef.label);
                }
                else if (this.source != null)
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Append(this.source.label);
                    if (this.sourceBodyPartGroup != null)
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(this.sourceBodyPartGroup.LabelShort);
                    }
                }
                HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
                if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent && hediffComp_GetsPermanent.PainCategory != PainCategory.Painless)
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Append(("PainCategory_" + hediffComp_GetsPermanent.PainCategory.ToString()).Translate());
                }
                return stringBuilder.ToString();
            }
        }

        public override Color LabelColor
        {
            get
            {
                if (this.IsPermanent())
                {
                    return New_Injury.PermanentInjuryColor;
                }
                return Color.white;
            }
        }

        public override string SeverityLabel
        {
            get
            {
                if (this.Severity == 0f)
                {
                    return null;
                }
                return this.Severity.ToString("F1");
            }
        }

      
        public override float SummaryHealthPercentImpact
        {
            get
            {
                if (this.IsPermanent() || !this.Visible)
                {
                    return 0f;
                }
                return this.Severity / (75f * this.pawn.HealthScale);
            }
        }

      
        public override float PainOffset
        {
            get
            {
                if (this.pawn.Dead || this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part) || this.causesNoPain)
                {
                    return 0f;
                }
                HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
                if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent)
                {
                    return this.Severity * this.def.injuryProps.averagePainPerSeverityPermanent * hediffComp_GetsPermanent.PainFactor;
                }
                return this.Severity * this.def.injuryProps.painPerSeverity;
            }
        }
        public bool StoppedBleeding;
       
        public override float BleedRate
        {
            get
            {
                if (StoppedBleeding)
                {
                    return 0f;
                }
                if (this.pawn.Dead)
                {
                    return 0f;
                }
                if (this.BleedingStoppedDueToAge)
                {
                    return 0f;
                }
                if (base.Part.def.IsSolid(base.Part, this.pawn.health.hediffSet.hediffs) || this.IsTended() || this.IsPermanent())
                {
                    return 0f;
                }
                if (this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part))
                {
                    return 0f;
                }
                float num = this.Severity * this.def.injuryProps.bleedRate;
                if (base.Part != null)
                {
                    num *= base.Part.def.bleedRate;
                }
                return num;
            }
        }

      
        private int AgeTicksToStopBleeding
        {
            get
            {
                int num = 90000;
                float t = Mathf.Clamp(Mathf.InverseLerp(1f, 30f, this.Severity), 0f, 1f);
                return num + Mathf.RoundToInt(Mathf.Lerp(0f, 90000f, t));
            }
        }

      
        private bool BleedingStoppedDueToAge
        {
            get
            {
                return this.ageTicks >= this.AgeTicksToStopBleeding;
            }
        }

        public override void Tick()
        {
            bool bleedingStoppedDueToAge = this.BleedingStoppedDueToAge;
            base.Tick();
            bool bleedingStoppedDueToAge2 = this.BleedingStoppedDueToAge;
            if (bleedingStoppedDueToAge != bleedingStoppedDueToAge2)
            {
                this.pawn.health.Notify_HediffChanged(this);
            }
        }

        
        public override void Heal(float amount)
        {
            
            this.Severity -= amount;
            if (this.comps != null)
            {
                for (int i = 0; i < this.comps.Count; i++)
                {
                    this.comps[i].CompPostInjuryHeal(amount);
                }
            }
            this.pawn.health.Notify_HediffChanged(this);
        }

       
        public override bool TryMergeWith(Hediff other)
        {
            New_Injury New_Injury = other as New_Injury;
            return New_Injury != null && New_Injury.def == this.def && New_Injury.Part == base.Part && !New_Injury.IsTended() && !New_Injury.IsPermanent() && !this.IsTended() && !this.IsPermanent() && this.def.injuryProps.canMerge && base.TryMergeWith(other);
        }

      
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (base.Part != null && base.Part.coverageAbs <= 0f)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Added injury to ",
                    base.Part.def,
                    " but it should be impossible to hit it. pawn=",
                    this.pawn.ToStringSafe<Pawn>(),
                    " dinfo=",
                    dinfo.ToStringSafe<DamageInfo?>()
                }), false);
            }
        }

        
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
            {
                Log.Error("New_Injury has null part after loading.", false);
                this.pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }

       
        private static readonly Color PermanentInjuryColor = new Color(0.72f, 0.72f, 0.72f);
    }
}

