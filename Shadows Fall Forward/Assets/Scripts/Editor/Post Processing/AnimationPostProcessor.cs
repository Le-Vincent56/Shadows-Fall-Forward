using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ShadowsFallForward.Editors.PostProcessors
{
    public class AnimationPostProcessor : AssetPostprocessor
    {
        private static AnimationPostProcessorSettings settings;
        private static Avatar referenceAvatar;
        private static GameObject referenceFBXPrefab;
        private static ModelImporter referenceImporter;

        private void OnPreprocessModel()
        {
            // Load the settings
            LoadSettings();

            // Exit case - if there are no settings or they are not enabled
            if (settings == null || !settings.enabled) return;

            // Import the asset
            ModelImporter importer = assetImporter as ModelImporter;
            AssetDatabase.ImportAsset(importer.assetPath);

            // Check if we need to extract textures
            if(settings.extractTextures)
            {
                // Extract materials and textures
                importer.ExtractTextures(Path.GetDirectoryName(importer.assetPath));
                importer.materialLocation = ModelImporterMaterialLocation.External;
            }

            // Check if the reference avatar is not set
            if (referenceAvatar == null)
                // Set the reference avatar to the importer's source avatar
                referenceAvatar = referenceImporter.sourceAvatar;

            // Set the avatar and rig type of the imported model
            importer.sourceAvatar = referenceAvatar;
            importer.animationType = settings.animationType;

            // Check if the importer is null or the avatar is invalid
            if(referenceImporter == null || !referenceAvatar.isValid)
                // Set the animation type to Generic
                importer.animationType = ModelImporterAnimationType.Generic;

            // Use serialization to set the avatar correctly
            SerializedObject serializedObject = new SerializedObject(importer.sourceAvatar);
            using (SerializedObject sourceObject = new SerializedObject(referenceAvatar))
                CopyHumanDescriptionToDestination(sourceObject, serializedObject);
            serializedObject.ApplyModifiedProperties();
            importer.sourceAvatar = serializedObject.targetObject as Avatar;
            serializedObject.Dispose();

            // Check if enabling translation DoF
            if(settings.enableTranslationDoF)
            {
                // Enable translation DoF
                HumanDescription importerHumanDescription = importer.humanDescription;
                importerHumanDescription.hasTranslationDoF = true;
                importer.humanDescription = importerHumanDescription;
            }

            // Check if forcing an Editor apply
            if(settings.forceEditorApply)
            {
                // Use reflection to get the desired Editor type and set flags
                Type editorType = typeof(Editor).Assembly.GetType("UnityEditor.ModelImporterEditor");
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

                // Create the Editor using the importer ane deditor type
                Editor editor = Editor.CreateEditor(importer, editorType);

                // Invoke the Apply method
                editorType.GetMethod("Apply", flags).Invoke(editor, null);

                // destroy the editor
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        private void OnPreprocessAnimation()
        {
            // Load the settings
            LoadSettings();

            // Exit case - if there are no settings or they are not enabled
            if (settings == null || !settings.enabled) return;

            // Copy all Model Importer settings
            ModelImporter modelImporter = CopyModelImporterSettings(assetImporter as ModelImporter);

            // Import with a forced update
            AssetDatabase.ImportAsset(modelImporter.assetPath, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Copy the human description of the animation from a source object to a destined objcet
        /// </summary>
        private void CopyHumanDescriptionToDestination(SerializedObject sourceObject, SerializedObject destinationObject)
        {
            destinationObject.CopyFromSerializedProperty(sourceObject.FindProperty("m_HumanDescription"));
        }

        /// <summary>
        /// Copy the Model Importer settings
        /// </summary>
        private ModelImporter CopyModelImporterSettings(ModelImporter modelImporter)
        {
            // Copy the 'Model' tab
            modelImporter.globalScale = referenceImporter.globalScale;
            modelImporter.useFileScale = referenceImporter.useFileScale;
            modelImporter.meshCompression = referenceImporter.meshCompression;
            modelImporter.isReadable = referenceImporter.isReadable;
            modelImporter.optimizeMeshPolygons = referenceImporter.optimizeMeshPolygons;
            modelImporter.optimizeMeshVertices = referenceImporter.optimizeMeshVertices;
            modelImporter.importBlendShapes = referenceImporter.importBlendShapes;
            modelImporter.keepQuads = referenceImporter.keepQuads;
            modelImporter.indexFormat = referenceImporter.indexFormat;
            modelImporter.weldVertices = referenceImporter.weldVertices;
            modelImporter.importVisibility = referenceImporter.importVisibility;
            modelImporter.importCameras = referenceImporter.importCameras;
            modelImporter.importLights = referenceImporter.importLights;
            modelImporter.preserveHierarchy = referenceImporter.preserveHierarchy;
            modelImporter.swapUVChannels = referenceImporter.swapUVChannels;
            modelImporter.generateSecondaryUV = referenceImporter.generateSecondaryUV;
            modelImporter.importNormals = referenceImporter.importNormals;
            modelImporter.normalCalculationMode = referenceImporter.normalCalculationMode;
            modelImporter.normalSmoothingAngle = referenceImporter.normalSmoothingAngle;
            modelImporter.importTangents = referenceImporter.importTangents;

            // Copy the 'Rig' tab
            modelImporter.animationType = referenceImporter.animationType;
            modelImporter.optimizeGameObjects = referenceImporter.optimizeGameObjects;

            // Copy the 'Materials' tab
            modelImporter.materialImportMode = referenceImporter.materialImportMode;
            modelImporter.materialLocation = referenceImporter.materialLocation;
            modelImporter.materialName = referenceImporter.materialName;

            // Handle naming conventions
            // Get the filename of the FBX in case we want to use it for the animation name
            string fileName = Path.GetFileNameWithoutExtension(modelImporter.assetPath);

            // Copy the 'Animations' tab
            // Exit case - there are no clips to copy on the reference importer
            if (referenceImporter.clipAnimations.Length == 0) return modelImporter;

            // Get the first reference clip and its animations
            ModelImporterClipAnimation referenceClip = referenceImporter.clipAnimations[0];
            ModelImporterClipAnimation[] referenceClipAnimations = referenceImporter.defaultClipAnimations;

            // Get the default clip animations
            ModelImporterClipAnimation[] defaultClipAnimations = modelImporter.defaultClipAnimations;

            // Copy the first reference clip settings to all imported clips
            foreach (ModelImporterClipAnimation clipAnimation in defaultClipAnimations)
            {
                clipAnimation.hasAdditiveReferencePose = referenceClip.hasAdditiveReferencePose;
                if (referenceClip.hasAdditiveReferencePose)
                {
                    clipAnimation.additiveReferencePoseFrame = referenceClip.additiveReferencePoseFrame;
                }

                // Rename if needed
                if (settings.renameClips)
                {
                    if (referenceClipAnimations.Length == 1)
                    {
                        clipAnimation.name = fileName;
                    }
                    else
                    {
                        clipAnimation.name = fileName + "" + clipAnimation.name;
                    }
                }

                // Set settings
                clipAnimation.loopTime = settings.loopTime;
                clipAnimation.maskType = referenceClip.maskType;
                clipAnimation.maskSource = referenceClip.maskSource;
                clipAnimation.keepOriginalOrientation = referenceClip.keepOriginalOrientation;
                clipAnimation.keepOriginalPositionXZ = referenceClip.keepOriginalPositionXZ;
                clipAnimation.keepOriginalPositionY = referenceClip.keepOriginalPositionY;
                clipAnimation.lockRootRotation = referenceClip.lockRootRotation;
                clipAnimation.lockRootPositionXZ = referenceClip.lockRootPositionXZ;
                clipAnimation.lockRootHeightY = referenceClip.lockRootHeightY;
                clipAnimation.mirror = referenceClip.mirror;
                clipAnimation.wrapMode = referenceClip.wrapMode;
            }

            // Set the clip animations
            modelImporter.clipAnimations = defaultClipAnimations;

            return modelImporter;
        }

        /// <summary>
        /// Load the Animation Post Processor Settings
        /// </summary>
        private static void LoadSettings()
        {
            // Find GUIDS using the appropriate string
            string[] guids = AssetDatabase.FindAssets("t:AnimationPostProcessorSettings");

            // Exit case - there are no GUIDS retrieved from the string filter
            if (guids.Length <= 0) return;

            // Get the first asset path
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            
            // Load the asset as an AnimationPostProcessorSettings object
            settings = AssetDatabase.LoadAssetAtPath<AnimationPostProcessorSettings>(path);

            // Set data
            referenceAvatar = settings.referenceAvatar;
            referenceFBXPrefab = settings.referenceFBXPrefab;
            referenceImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(referenceFBXPrefab)) as ModelImporter;
        }
    }
}
