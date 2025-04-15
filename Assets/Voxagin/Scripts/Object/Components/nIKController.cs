namespace Ngin {
    using UnityEngine;
    using System.Collections.Generic;
    public class nIKController : nComponent {

        protected override void AddClasses() {

        }
        protected override void StoreData(Lexicon data) {
            
        }
        protected override void Launch() {
            base.Launch();

            List<nIK> iks = this.GetComponentsInChildren<nIK>();
            foreach (var ik in iks) {
                ik.Link(
                    controller: this,
                    effectorTarget: this.FindChildObject(ik.TargetName).transform,
                    effectorPole: this.FindChildObject(ik.PoleName).transform
                );
            }

            List<nIKJobs> ikJobs = this.GetComponentsInChildren<nIKJobs>();
            foreach (var ik in ikJobs) {
                ik.Link(
                    controller: this,
                    effectorTarget: this.FindChildObject(ik.TargetName).transform,
                    effectorPole: this.FindChildObject(ik.PoleName).transform
                );
            }
        }
    }
}