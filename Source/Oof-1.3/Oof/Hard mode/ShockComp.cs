using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Oof
{
    [DefOf]
    public class ShockDefOf
    {
        public static HediffDef HypovolemicShock;

        public static HediffDef InternalSuffocation;
    }

    [StaticConstructorOnStartup]
    public class patchidk
    {
        static patchidk()
        {
            if (OofMod.settings.advancedShock)
            {
                if (HediffDefOf.BloodLoss.comps == null)
                {
                    HediffDefOf.BloodLoss.comps = new List<HediffCompProperties>();
                }
                HediffDefOf.BloodLoss.comps.Add(new HediffCompProperties { compClass = typeof(ShockMakerComp) });

                HediffDefOf.BloodLoss.hediffClass = typeof(HediffWithComps);
            }
        }
    }

    public class ShockMakerComp : HediffComp
    {
        int ticks = 0;

        public bool addedOne = false;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (this.parent.pawn.RaceProps?.Humanlike ?? false)
            {
                ++ticks;
                if (this.parent.Severity > 0.15f)
                {
                    if (ticks >= 600 && !addedOne)
                    {
                        if (Rand.Chance(curve.Evaluate(this.parent.Severity)) && !this.parent.pawn.health.hediffSet.HasHediff(ShockDefOf.HypovolemicShock))
                        {
                            this.parent.pawn.health.AddHediff(HediffMaker.MakeHediff(ShockDefOf.HypovolemicShock, this.parent.pawn));
                            addedOne = true;
                        }
                        ticks = 0;
                    }
                }
               
            }
        }

        public SimpleCurve curve = new SimpleCurve(new CurvePoint[]
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(15f, 5f),
            new CurvePoint(50f, 8f),
            new CurvePoint(70f, 10f),
            new CurvePoint(90f, 15f)
        });
    }

    public class VariableTendable : HediffWithComps
    {
        public VariableTendableModExt propsExt => this.def.GetModExtension<VariableTendableModExt>();

        public override bool TendableNow(bool ignoreTimer = false)
        {

            return base.TendableNow(ignoreTimer) && (new FloatRange(propsExt.minSeverityToTendable, propsExt.maxSeverityToTendable)).Includes(this.Severity);
        }
    }

    public class VariableTendableModExt : DefModExtension
    {
        public float minSeverityToTendable;

        public float maxSeverityToTendable;
    }

    public class ShockComp : HediffComp
    {
        public ShockCompProps Props => this.props as ShockCompProps;

        public bool PastFixedPoint
        {
            get
            {
                return this.parent.Severity >= 0.75f;
            }
        }

        public bool fixedNow = false;

        public override void CompTended(float quality, float maxQuality, int batchPosition = 0)
        {
            base.CompTended(quality, maxQuality, batchPosition);
            if (quality >= this.Props.BleedSeverityCurve.Evaluate(this.parent.Severity))
            {
                fixedNow = true;
            }
        }

        public Hediff BloodLoss
        {
            get
            {
                return this.parent.pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDefOf.BloodLoss);
            }
        }

        public int ticks = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticks++;
            base.CompPostTick(ref severityAdjustment);
            if ( (BloodLoss == null && !PastFixedPoint) | fixedNow)
            {
                //this.parent.pawn.health.RemoveHediff(this.parent);
                this.parent.Severity -= 0.000025f;
            }
            if (!fixedNow)
            {
                if (!PastFixedPoint)
                {
                    this.parent.Severity = BloodLoss.Severity;
                }
                else
                {
                    this.parent.Severity += 0.000025f;

                    if (ticks >= OofMod.settings.hypoxiaInterval)
                    {
                        if (Rand.Chance(OofMod.settings.hypoxiaChance))
                        {
                            var part = InternalBps.Where(x => x.def.bleedRate > 0f && !x.def.IsSolid(x, Pawn.health.hediffSet.hediffs)).RandomElement();

                            var hediff = HediffMaker.MakeHediff(ShockDefOf.InternalSuffocation, this.parent.pawn, part);

                            hediff.Severity = Rand.Range(1f, 3f);

                            this.parent.pawn.health.AddHediff(hediff, part);

                            ticks = 0;
                        }
                    }
                }
            }
        }

        public IEnumerable<BodyPartRecord> InternalBps
        {
            get
            {
                return this.parent.pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Middle, BodyPartDepth.Inside).Where(x => x.def != BodyPartDefOf.Heart);
            }
        }
    }

    public class ShockCompProps : HediffCompProperties
    {
        public ShockCompProps() : base()
        {
            this.compClass = typeof(ShockComp);
        }

        public SimpleCurve BleedSeverityCurve;
    }
}
