using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace Oof
{
    public class chockingcomp : HediffComp
    {
        public int chocke_int;
        public override void CompPostMake()
        {
            chocke_int = Props.ABCD;
            this.Props.coughSound.PlayOneShot(SoundInfo.InMap(this.parent.pawn, MaintenanceType.None));
            base.CompPostMake();
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            
            if (chocke_int != 1)
            {
                --chocke_int;
            }
            if (Rand.Chance(0.7f))
            {
                if (chocke_int == 1)
                {
                    //this.Props.coughSound.PlayOneShot(SoundInfo.InMap(this.parent.pawn, MaintenanceType.None));
                    this.parent.Severity += 0.05f;
                    chocke_int = Props.ABCD;
                }
            }
            else
            {
                if (chocke_int == 1)
                {
                    
                    this.Props.coughSound.PlayOneShot(SoundInfo.InMap(this.parent.pawn, MaintenanceType.None));
                    this.parent.Severity -= 0.08f;
                    chocke_int = Props.ABCD;
                }
            }
          
            base.CompPostTick(ref severityAdjustment);
        }


        public chockingcompProperties Props => (chockingcompProperties)this.props;

       
    }

    public class chockingcompProperties : HediffCompProperties
    {
        public int ABCD;
        public SoundDef coughSound;

       
        public chockingcompProperties()
        {
            this.compClass = typeof(chockingcomp);
        }

        public chockingcompProperties(Type compClass) : base()
        {
            this.compClass = compClass;
        }
    }
	internal class ClearAirway : JobDriver
	{
		
		protected Pawn Patient
		{
			get
			{
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		
		protected Pawn Doctor
		{
			get
			{
				return this.pawn;
			}
		}

		
		public Thing suctiondevice
		{
			get
			{
				return this.job.targetB.Thing;
			}
		}

		
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.Doctor.Reserve(this.Patient, this.job, 1, -1, null, errorOnFailed);
		}

		
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);
			
			Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return toil;
			Toil toil2 = Toils_General.Wait(320);
			toil2.AddFinishAction(delegate
			{
				Patient.health.RemoveHediff(Patient.health.hediffSet.hediffs.Find(AAA => AAA.def == Caula_DefOf.ChockingOnBlood));
			});
			yield return toil2;
			yield break;
		}

		
		private const TargetIndex targetInd = TargetIndex.A;
	}
}
