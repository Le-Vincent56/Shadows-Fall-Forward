using UnityEditor;
using UnityEngine;

namespace ShadowsFallForward.Editors.PostProcessors
{
    [CreateAssetMenu(fileName = "AnimationPostProcessorSettings", menuName = "Post Processors/Animation Settings")]
    public class AnimationPostProcessorSettings : ScriptableObject
    {
        public bool enabled = true;
        public Avatar referenceAvatar;
        public GameObject referenceFBXPrefab;

        public bool enableTranslationDoF = true;
        public ModelImporterAnimationType animationType = ModelImporterAnimationType.Human;
        public bool loopTime = true;
        public bool renameClips = true;
        public bool forceEditorApply = true;
        public bool extractTextures = true;
    }
}
