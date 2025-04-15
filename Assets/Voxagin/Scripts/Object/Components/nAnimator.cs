using UnityEngine;

namespace Ngin {
    using System.Collections.Generic;
    public class nAnimator : nComponent {
        public Animation animator;
        public List<string> animations;
        public List<AnimationClip> clips;

        bool playAutomatically;
        bool loop;
        
        protected override void AddClasses() {
            animator = ComponentCheck<Animation>(true);
            animator.playAutomatically = playAutomatically;
        }
        protected override void StoreData(Lexicon data) {
            animations = data.Get<List<string>>("animations", new List<string>());  

            playAutomatically = data.Get<bool>("playAutomatically", false);
            loop = data.Get<bool>("loop", false);
        }

        protected override void Launch() {
            clips = new List<AnimationClip>();
            foreach (string animation in animations) {
                Debug.Log("Animation/" + animation);
                AnimationClip clip = Resources.Load<AnimationClip>("Animation/" + animation);
                if (clip == null) continue;
                clip.legacy = true;
                clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

                if (clip == null) {
                    Debug.LogError("Animation not found.");
                } else {
                    Debug.Log("Animation found - about to add clip: " + clip.name);
                    animator.AddClip(clip, animation);
                    clips.Add(clip);
                }
            }
            if (clips.Count > 0) {
                animator.clip = clips[0];
            }
        }

        public Transform FindBone(string name) {
            return FindBoneRecursive(transform, name);
        }
        private Transform FindBoneRecursive(Transform parent, string name) {
            if (parent.name == name) {
                return parent;
            }
            foreach (Transform child in parent) {
                Transform result = FindBoneRecursive(child, name);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }
    }
}