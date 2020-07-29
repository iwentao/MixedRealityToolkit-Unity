﻿//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Experimental.Physics;
using System;

namespace Microsoft.MixedReality.Toolkit.Experimental.Inspectors
{
    [CustomEditor(typeof(BoundsControl), true)]
    [CanEditMultipleObjects]
    public class BoundsControlInspector : UnityEditor.Editor
    {
        private SerializedProperty targetObject;
        private SerializedProperty boundsOverride;
        private SerializedProperty boundsCalculationMethod;
        private SerializedProperty activationType;
        private SerializedProperty controlPadding;
        private SerializedProperty flattenAxis;

        private SerializedProperty smoothingActive;
        private SerializedProperty rotateLerpTime;
        private SerializedProperty scaleLerpTime;

        // Elastics
        private SerializedProperty elasticTypes;
        private SerializedProperty translationElasticConfigurationObject;
        private SerializedProperty rotationElasticConfigurationObject;
        private SerializedProperty scaleElasticConfigurationObject;
        private SerializedProperty translationElasticExtent;
        private SerializedProperty rotationElasticExtent;
        private SerializedProperty scaleElasticExtent;
        

        // Configs
        private SerializedProperty boxDisplayConfiguration;
        private SerializedProperty linksConfiguration;
        private SerializedProperty scaleHandlesConfiguration;
        private SerializedProperty rotationHandlesConfiguration;
        private SerializedProperty translationHandlesConfiguration;
        private SerializedProperty proximityEffectConfiguration;

        // Debug
        private SerializedProperty hideElementsInHierarchyEditor;

        // Events
        private SerializedProperty rotateStartedEvent;
        private SerializedProperty rotateStoppedEvent;
        private SerializedProperty scaleStartedEvent;
        private SerializedProperty scaleStoppedEvent;

        private BoundsControl boundsControl;

        private static bool showBoxConfiguration = false;
        private static bool showScaleHandlesConfiguration = false;
        private static bool showRotationHandlesConfiguration = false;
        private static bool showTranslationHandlesConfiguration = false;
        private static bool showLinksConfiguration = false;
        private static bool showProximityConfiguration = false;

        private static bool elasticsFoldout = true;
        private static bool translationElasticFoldout = false;
        private static bool rotationElasticFoldout = false;
        private static bool scaleElasticFoldout = false;

        private static HandleType rotationType = HandleType.Basic;
        private static HandleType translationType = HandleType.Basic;

        // Used to manage user input for basic/precision
        // ScriptableObject management.
        // Hardcoded values for GUILayout.Toolbar.
        private enum HandleType
        {
            Basic = 0, Precision = 1
        }

        private void OnEnable()
        {
            boundsControl = (BoundsControl)target;

            targetObject = serializedObject.FindProperty("targetObject");
            activationType = serializedObject.FindProperty("activation");
            boundsOverride = serializedObject.FindProperty("boundsOverride");
            boundsCalculationMethod = serializedObject.FindProperty("boundsCalculationMethod");
            flattenAxis = serializedObject.FindProperty("flattenAxis");
            controlPadding = serializedObject.FindProperty("boxPadding");

            smoothingActive = serializedObject.FindProperty("smoothingActive");
            rotateLerpTime = serializedObject.FindProperty("rotateLerpTime");
            scaleLerpTime = serializedObject.FindProperty("scaleLerpTime");

            boxDisplayConfiguration = serializedObject.FindProperty("boxDisplayConfiguration");
            linksConfiguration = serializedObject.FindProperty("linksConfiguration");
            scaleHandlesConfiguration = serializedObject.FindProperty("scaleHandlesConfiguration");
            rotationHandlesConfiguration = serializedObject.FindProperty("rotationHandlesConfiguration");
            translationHandlesConfiguration = serializedObject.FindProperty("translationHandlesConfiguration");
            proximityEffectConfiguration = serializedObject.FindProperty("handleProximityEffectConfiguration");

            hideElementsInHierarchyEditor = serializedObject.FindProperty("hideElementsInInspector");

            rotateStartedEvent = serializedObject.FindProperty("rotateStarted");
            rotateStoppedEvent = serializedObject.FindProperty("rotateStopped");
            scaleStartedEvent = serializedObject.FindProperty("scaleStarted");
            scaleStoppedEvent = serializedObject.FindProperty("scaleStopped");

            // Elastic configuration (ScriptableObject)
            translationElasticConfigurationObject = serializedObject.FindProperty("translationElasticConfigurationObject");
            rotationElasticConfigurationObject = serializedObject.FindProperty("rotationElasticConfigurationObject");
            scaleElasticConfigurationObject = serializedObject.FindProperty("scaleElasticConfigurationObject");
            translationElasticExtent = serializedObject.FindProperty("translationElasticExtent");
            rotationElasticExtent = serializedObject.FindProperty("rotationElasticExtent");
            scaleElasticExtent = serializedObject.FindProperty("scaleElasticExtent");
            elasticTypes = serializedObject.FindProperty("elasticTypes");
        }

        public override void OnInspectorGUI()
        {
            if (target != null)
            {
                // Notification section - first thing to show in bounds control component
                DrawRigidBodyWarning();

                // Help url
                InspectorUIUtility.RenderHelpURL(target.GetType());

                // Data section
                {
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.PropertyField(targetObject);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Behavior"), EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(activationType);
                    EditorGUILayout.PropertyField(boundsOverride);
                    EditorGUILayout.PropertyField(boundsCalculationMethod);
                    EditorGUILayout.PropertyField(controlPadding);
                    EditorGUILayout.PropertyField(flattenAxis);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Smoothing"), EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(smoothingActive);
                    EditorGUILayout.PropertyField(scaleLerpTime);
                    EditorGUILayout.PropertyField(rotateLerpTime);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Visuals", "Bounds Control Visual Configurations"), EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    using (new EditorGUI.IndentLevelScope())
                    {

                        showBoxConfiguration = InspectorUIUtility.DrawScriptableFoldout<BoxDisplayConfiguration>(boxDisplayConfiguration, 
                                                                                                                 "Box Configuration", 
                                                                                                                 showBoxConfiguration);
                        showScaleHandlesConfiguration = InspectorUIUtility.DrawScriptableFoldout<ScaleHandlesConfiguration>(scaleHandlesConfiguration, 
                                                                                                                            "Scale Handles Configuration", 
                                                                                                                            showScaleHandlesConfiguration);
                        EditorGUILayout.Separator();
                        showRotationHandlesConfiguration = DrawMultiTypeConfigSlot<RotationHandlesConfiguration, PrecisionRotationHandlesConfiguration>(
                            "Basic Rotation",
                            "Precision Rotation",
                            rotationHandlesConfiguration,
                            ref rotationType,
                            showRotationHandlesConfiguration);

                        EditorGUILayout.Separator();
                        showTranslationHandlesConfiguration = DrawMultiTypeConfigSlot<TranslationHandlesConfiguration, PrecisionTranslationHandlesConfiguration>(
                            "Basic Translation",
                            "Precision Translation",
                            translationHandlesConfiguration,
                            ref translationType,
                            showTranslationHandlesConfiguration);
                        EditorGUILayout.Separator();

                        showLinksConfiguration = InspectorUIUtility.DrawScriptableFoldout<LinksConfiguration>(linksConfiguration, 
                                                                                                              "Links Configuration", 
                                                                                                              showLinksConfiguration);
                        showProximityConfiguration = InspectorUIUtility.DrawScriptableFoldout<ProximityEffectConfiguration>(proximityEffectConfiguration, 
                                                                                                                            "Proximity Configuration", 
                                                                                                                            showProximityConfiguration);
                    }
                    EditorGUILayout.Space();
                    elasticsFoldout = EditorGUILayout.Foldout(elasticsFoldout, "Elastics", true);

                    if (elasticsFoldout)
                    {
                        // This two-way enum cast is required because EnumFlagsField does not play nicely with
                        // SerializedProperties and custom enum flags.
                        var newElasticTypesValue = EditorGUILayout.EnumFlagsField("Manipulation types using elastic feedback: ", (TransformFlags)elasticTypes.intValue);
                        elasticTypes.intValue = (int)(TransformFlags)newElasticTypesValue;

                        // If the particular elastic type is requested, we offer the user the ability
                        // to configure the elastic system.
                        TransformFlags currentFlags = (TransformFlags)elasticTypes.intValue;

                        translationElasticFoldout = DrawElasticConfiguration<ElasticConfiguration>(
                            "Translation Elastic",
                            translationElasticFoldout,
                            translationElasticConfigurationObject,
                            translationElasticExtent,
                            TransformFlags.Move,
                            currentFlags);

                        rotationElasticFoldout = DrawElasticConfiguration<ElasticConfiguration>(
                            "Rotation Elastic",
                            rotationElasticFoldout,
                            rotationElasticConfigurationObject,
                            rotationElasticExtent,
                            TransformFlags.Rotate,
                            currentFlags);

                        scaleElasticFoldout = DrawElasticConfiguration<ElasticConfiguration>(
                            "Scale Elastic",
                            scaleElasticFoldout,
                            scaleElasticConfigurationObject,
                            scaleElasticExtent,
                            TransformFlags.Scale,
                            currentFlags);
                    }


                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Events", "Bounds Control Events"), EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.PropertyField(rotateStartedEvent);
                        EditorGUILayout.PropertyField(rotateStoppedEvent);
                        EditorGUILayout.PropertyField(scaleStartedEvent);
                        EditorGUILayout.PropertyField(scaleStoppedEvent);
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Debug", "Bounds Control Debug Section"), EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.PropertyField(hideElementsInHierarchyEditor);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        /// <summary>
        /// Draws a multi-type selection box that will draw one of two
        /// ScriptableObjectFoldouts, depending on whether the user has selected
        /// the basic or precision variant of the handle config.
        /// </summary>
        /// <typeparam name="BasicType">The HandlesBaseConfiguration representing the "basic" option.</typeparam>
        /// <typeparam name="PrecisionType">The HandlesBaseConfiguration representing the "precision" option.</typeparam>
        /// <param name="basicString">Description string for the basic option</param>
        /// <param name="precisionString">Description string for the precision option</param>
        /// <param name="property">SerializedProperty holding the configuration reference to be modified</param>
        /// <param name="toolbarSelection">Result of the user's selection (Basic or Precision) from the toolbar</param>
        /// <param name="showFoldout">Result of the ScriptableObject foldout itself.</param>
        /// <returns>Result of the ScriptableObject foldout.</returns>
        private bool DrawMultiTypeConfigSlot<BasicType,PrecisionType>(
            string basicString,
            string precisionString,
            SerializedProperty property,
            ref HandleType toolbarSelection,
            bool showFoldout) where BasicType : HandlesBaseConfiguration
                              where PrecisionType : HandlesBaseConfiguration
        {
            // Allow the user to pick whether the ScriptableObject slot will specify a basic or precision affordance/handle config.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Handle type: ");
            toolbarSelection = (HandleType)GUILayout.Toolbar((int)toolbarSelection, new string[] { basicString, precisionString });
            EditorGUILayout.EndHorizontal();
            // Verify that the config has an assigned handle prefab, and warn the user if not.
            // HandleConfigurations will misbehave if no prefab is assigned.
            if (property.objectReferenceValue != null && (property.objectReferenceValue as HandlesBaseConfiguration).HandlePrefab == null)
            {
                EditorGUILayout.HelpBox("No handle prefab assigned! Assign a prefab, or consider using a shared configuration asset.", MessageType.Info);
            }

            if (toolbarSelection == HandleType.Basic)
            {
                // If the user currently has a precision manipulation config assigned,
                // but has selected the basic config option in the toolbar,
                // we will generate a new "fresh" basic config object.
                if (property.objectReferenceValue == null || property.objectReferenceValue.GetType() == typeof(PrecisionType))
                {
                    property.objectReferenceValue = CreateInstance<BasicType>();
                }

                

                return InspectorUIUtility.DrawScriptableFoldout<BasicType>(
                    property,
                    basicString + " Configuration",
                    showFoldout);
            }
            else
            {
                // If the user currently has a basic manipulation config assigned,
                // but has selected the precision config option in the toolbar,
                // we will generate a new "fresh" precision config object.
                if (property.objectReferenceValue == null || property.objectReferenceValue.GetType() == typeof(BasicType))
                {
                    property.objectReferenceValue = CreateInstance<PrecisionType>();
                }

                return InspectorUIUtility.DrawScriptableFoldout<PrecisionType>(
                    property,
                    precisionString + " Configuration",
                    showFoldout);
            }
        }

        private void DrawRigidBodyWarning()
        {
            // Check if rigidbody is attached - if so show warning in case input profile is not configured for individual collider raycast
            Rigidbody rigidBody = boundsControl.GetComponent<Rigidbody>();

            if (rigidBody != null)
            {
                MixedRealityInputSystemProfile profile = Microsoft.MixedReality.Toolkit.CoreServices.InputSystem?.InputSystemProfile;
                if (profile != null && profile.FocusIndividualCompoundCollider == false)
                {
                    EditorGUILayout.Space();
                    // Show warning and button to reconfigure profile
                    EditorGUILayout.HelpBox($"When using Bounds Control in combination with Rigidbody 'Focus Individual Compound Collider' must be enabled in Input Profile.", UnityEditor.MessageType.Warning);
                    if (GUILayout.Button($"Enable 'Focus Individual Compound Collider' in Input Profile"))
                    {
                        profile.FocusIndividualCompoundCollider = true;
                    }

                    EditorGUILayout.Space();
                }
            }
        }
        private bool DrawElasticConfiguration<T>(
            string name,
            bool expanded,
            SerializedProperty elasticProperty,
            SerializedProperty extentProperty,
            TransformFlags requiredFlag,
            TransformFlags providedFlags) where T : ElasticConfiguration
        {
            if (providedFlags.HasFlag(requiredFlag))
            {
                bool result = false;
                using (new EditorGUI.IndentLevelScope())
                {
                    result = InspectorUIUtility.DrawScriptableFoldout<T>(
                        elasticProperty,
                        name,
                        expanded);
                    EditorGUILayout.PropertyField(extentProperty, includeChildren: true);
                }
                return result;
            }
            else
            {
                return false;
            }
        }

    }
}
