using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using TriInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
    /// <summary>
    /// Helper class to define the logger color.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]  
    public abstract class LogColor : Attribute
    {
        public readonly DB32Color Color;
        protected LogColor(DB32Color color) => Color = color;
    }

    /// <summary>
    /// Pretty game logger with configurable rules.
    /// Coloring rules can be enforced by creating a singleton object. Otherwise, it will log everything
    /// as white.
    /// </summary>
    [ExecuteInEditMode]
    public class GameLogger : MonoBehaviour
    {
        [Title("Parameters")]
        [SerializeField] DB32Color defaultColor = DB32Color.White;
        [SerializeField] GameLogLevel logLevel;
        [SerializeField] List<LoggingRule> logRules;
        
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static GameLogger I { get; private set; }


        #region Unity hooks

        private void Awake()
        {
            if (I != null && I != this)
                Destroy(gameObject);
            else
                I = this;
        }

        #endregion
        
        #region Public logging API
        
        /// <summary>
        /// Log a low-level debug message.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="sender">The object that invoked this log call.</param>
        public static void Debug(object message, Object sender)
        {
            if (I)
                I.Log(message, sender, GameLogLevel.Debug);
            else
                LogStatic(message, sender, GameLogLevel.Debug);
        }

        /// <summary>
        /// Log info.
        /// </summary>
        /// <param name="message">The info message.</param>
        /// <param name="sender">The object that invoked this log call.</param>
        public static void Info(object message, Object sender)
        {
            if (I)
                I.Log(message, sender, GameLogLevel.Info);
            else
                LogStatic(message, sender, GameLogLevel.Info);
        }
        
        /// <summary>
        /// Log a warning.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="sender"></param>
        public static void Warning(object message, Object sender)
        {
            if (I)
                I.Log(message, sender, GameLogLevel.Warning);
            else
                LogStatic(message, sender, GameLogLevel.Warning);
        }

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="sender">The object that invoked this log call.</param>
        public static void Error(object message, Object sender)
        {
            if (I)
                I.Log(message, sender, GameLogLevel.Error);
            else
                LogStatic(message, sender, GameLogLevel.Error);
        }
        
        /// <summary>
        /// Low-level debug message.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="senderType">The sender of the log message.</param>
        public static void Debug(object message, Type senderType)
        {
            if (I)
                I.Log(message, senderType, GameLogLevel.Debug);
            else
                LogStatic(message, senderType, GameLogLevel.Debug);
        }
        
        /// <summary>
        /// Log info.
        /// </summary>
        /// <param name="message">The info message.</param>
        /// <param name="senderType">The sender of the log message.</param>
        public static void Info(object message, Type senderType)
        {
            if (I)
                I.Log(message, senderType, GameLogLevel.Info);
            else
                LogStatic(message, senderType, GameLogLevel.Info);
        }
        
        /// <summary>
        /// Log a warning.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="senderType">The sender of the log message.</param>
        public static void Warning(object message, Type senderType)
        {
            if (I)
                I.Log(message, senderType, GameLogLevel.Warning);
            else
                LogStatic(message, senderType, GameLogLevel.Warning);
        }

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="senderType">The sender of the log message.</param>
        public static void Error(object message, Type senderType)
        {
            if (I)
                I.Log(message, senderType, GameLogLevel.Error);
            else
                LogStatic(message, senderType, GameLogLevel.Error);
        }
        
        #endregion

        #region Utilities

        /// <summary>
        /// Get the color associated with a registered logging type on the log rules.
        /// </summary>
        /// <param name="type">The logging type.</param>
        /// <returns>The color associated with the logging type. If none was defined will default to the default color.</returns>
        private Color GetColorFromAttribute(Type type)
        {
            Attribute[] attributes = Attribute.GetCustomAttributes(type);
            foreach (Attribute attr in attributes)  
            {  
                if (attr is LogColor logColor)
                    return logColor.Color.GetColor();
            } 
            return defaultColor.GetColor();
        }

        /// <summary>
        /// The the Unity logging function associated to the game log level.
        /// </summary>
        /// <param name="gameLogLevel">The game log level.</param>
        /// <returns>A callback of a Unity built-in logger function.</returns>
        private static Action<object> GetLoggerOfType(GameLogLevel gameLogLevel)
        {
            return gameLogLevel switch
            {
                GameLogLevel.Debug => UnityEngine.Debug.Log,
                GameLogLevel.Info => UnityEngine.Debug.Log,
                GameLogLevel.Warning => UnityEngine.Debug.LogWarning,
                GameLogLevel.Error => UnityEngine.Debug.LogError,
                _ => UnityEngine.Debug.Log
            };
        }
        
        #endregion

        #region Logger implementation
        
        /// <summary>
        /// Log call invoked by an object instance.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="sender">The object that emitted this log call.</param>
        /// <param name="gameLogLevel">The log level.</param>
        private void Log(object message, Object sender, GameLogLevel gameLogLevel)
        {
            if (gameLogLevel.ToInt() < logLevel.ToInt()) 
                return;
            
            Type senderType = sender.GetType();
            string senderName = sender.name;
            
            Action<object> unityLogger = GetLoggerOfType(gameLogLevel);
            
            string senderNamespace = senderType.Namespace;
            if (senderNamespace != null)
            {
                string senderTypeTxt = senderType.ToString();
                Color senderTypeColor = GetColorFromAttribute(senderType);
                LoggingRule selectedRule = logRules.Find(x => x.namespaceName == senderNamespace) ??
                                           new LoggingRule(namespaceName: null, color: defaultColor, silence: false);

                if (!selectedRule.silence)
                {
                    string hexNamespaceColor = "#" + ColorUtility.ToHtmlStringRGB(selectedRule.color.GetColor());
                    string hexSenderTypeColor = "#" + ColorUtility.ToHtmlStringRGB(senderTypeColor);
                    string senderTypeFormatted = senderTypeTxt.Split('.').Last();

                    string idTxt =
                        $"<b>[<color={hexNamespaceColor}>Module {senderNamespace}</color>, " + 
                        $"<color={hexSenderTypeColor}>Class {senderTypeFormatted}</color>, " + 
                        $"GameObject {senderName}]</b> ";
                    unityLogger.Invoke(idTxt + message);
                }
            }
        }
        
        /// <summary>
        /// Log call invoked by an object instance. Fallback static version.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="sender">The object that emitted this log call.</param>
        /// <param name="gameLogLevel">The log level.</param>
        private static void LogStatic(object message, Object sender, GameLogLevel gameLogLevel)
        {
            Type senderType = sender.GetType();
            string senderName = sender.name;
            
            Action<object> unityLogger = GetLoggerOfType(gameLogLevel);
            
            string senderNamespace = senderType.Namespace;
            if (senderNamespace != null)
            {
                string senderTypeTxt = senderType.ToString();
                string senderTypeFormatted = senderTypeTxt.Split('.').Last();

                string idTxt =
                    $"<b>[Module {senderNamespace}, " + 
                    $"Class {senderTypeFormatted}, " + 
                    $"GameObject {senderName}]</b> ";
                unityLogger.Invoke(idTxt + message);
            }
        }

        /// <summary>
        /// Log a call invoked from a static object.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="senderType">The log sender.</param>
        /// <param name="gameLogLevel">The log level.</param>
        private void Log(object message, Type senderType, GameLogLevel gameLogLevel)
        {
            if (gameLogLevel.ToInt() < logLevel.ToInt()) 
                return;
            
            Action<object> unityLogger = GetLoggerOfType(gameLogLevel);
            
            string senderNamespace = senderType.Namespace;
            if (senderNamespace != null)
            {
                string senderTypeTxt = senderType.ToString();
                Color senderTypeColor = GetColorFromAttribute(senderType);
                LoggingRule selectedRule = logRules.Find(x => x.namespaceName == senderNamespace) ??
                                           new LoggingRule(namespaceName: null, color: defaultColor, silence: false);
                
                if (!selectedRule.silence)
                {
                    string hexNamespaceColor = "#" + ColorUtility.ToHtmlStringRGB(selectedRule.color.GetColor());
                    string hexSenderTypeColor = "#" + ColorUtility.ToHtmlStringRGB(senderTypeColor);
                    string senderTypeFormatted = senderTypeTxt.Split('.').Last();

                    string idTxt =
                        $"<b>[<color={hexNamespaceColor}>Module {senderNamespace}</color>, " +
                        $"<color={hexSenderTypeColor}>Class {senderTypeFormatted}</color>]</b> ";
                    unityLogger.Invoke(idTxt + message);
                }
            }
        }
        
        /// <summary>
        /// Log a call invoked from a static object. Fallback static version.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="senderType">The log sender.</param>
        /// <param name="gameLogLevel">The log level.</param>
        private static void LogStatic(object message, Type senderType, GameLogLevel gameLogLevel)
        {
            Action<object> unityLogger = GetLoggerOfType(gameLogLevel);
            
            string senderNamespace = senderType.Namespace;
            if (senderNamespace != null)
            {
                string senderTypeTxt = senderType.ToString();
                string senderTypeFormatted = senderTypeTxt.Split('.').Last();

                string idTxt =
                    $"<b>[Module {senderNamespace}, " +
                    $"Class {senderTypeFormatted}]</b> ";
                unityLogger.Invoke(idTxt + message);
            }
        }
        
        #endregion
    }
}