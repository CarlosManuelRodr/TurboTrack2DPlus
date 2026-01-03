using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TriInspector;
using UnityEngine;

namespace Core.Logging
{
    /// <summary>
    /// Defines a logging rule that applies formatting coloring to a namespace logs.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [DeclareHorizontalGroup("color")]
    public class LoggingRule
    {
        [InfoBox("The module namespace to which this rule will apply.")]
        public string namespaceName;
        
        [Group("color")] [OnValueChanged("UpdateColorPreview")]
        public DB32Color color;

        // ReSharper disable once NotAccessedField.Global
        [Group("color")] [ReadOnly] [HideLabel]
        public Color colorPreview;
        
        [InfoBox("Flag to silence the logging rule.")]
        public bool silence;

        public LoggingRule(string namespaceName, DB32Color color, bool silence)
        {
            this.namespaceName = namespaceName;
            this.color = color;
            this.silence = silence;
        }

        private List<string> AllNamespaces => Assembly.GetExecutingAssembly().GetTypes().Select(t => t.Namespace).Distinct().ToList();
        
    #if UNITY_EDITOR
        private void UpdateColorPreview()
        {
            colorPreview = color.GetColor();
        }
    #endif
    }
}