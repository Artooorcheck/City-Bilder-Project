using System;
using System.Globalization;
using UnityEngine;

namespace Serilog
{
    /// <summary>
    /// Minimal Serilog-compatible logger interface used to integrate with Unity's logging backend.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes a debug level log entry.
        /// </summary>
        /// <param name="messageTemplate">The message template to render.</param>
        /// <param name="propertyValues">Optional template values.</param>
        void Debug(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Writes an information level log entry.
        /// </summary>
        /// <param name="messageTemplate">The message template to render.</param>
        /// <param name="propertyValues">Optional template values.</param>
        void Information(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Writes a warning level log entry.
        /// </summary>
        /// <param name="messageTemplate">The message template to render.</param>
        /// <param name="propertyValues">Optional template values.</param>
        void Warning(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Writes an error level log entry.
        /// </summary>
        /// <param name="messageTemplate">The message template to render.</param>
        /// <param name="propertyValues">Optional template values.</param>
        void Error(string messageTemplate, params object[] propertyValues);
    }

    /// <summary>
    /// Provides access to the global logger instance.
    /// </summary>
    public static class Log
    {
        private static ILogger _logger = new UnityLogger(UnityEngine.Debug.unityLogger);

        /// <summary>
        /// Assigns a custom logger implementation.
        /// </summary>
        /// <param name="logger">Logger to be used globally.</param>
        public static void SetLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Writes a debug level log entry.
        /// </summary>
        public static void Debug(string messageTemplate, params object[] propertyValues) =>
            _logger.Debug(messageTemplate, propertyValues);

        /// <summary>
        /// Writes an information level log entry.
        /// </summary>
        public static void Information(string messageTemplate, params object[] propertyValues) =>
            _logger.Information(messageTemplate, propertyValues);

        /// <summary>
        /// Writes a warning level log entry.
        /// </summary>
        public static void Warning(string messageTemplate, params object[] propertyValues) =>
            _logger.Warning(messageTemplate, propertyValues);

        /// <summary>
        /// Writes an error level log entry.
        /// </summary>
        public static void Error(string messageTemplate, params object[] propertyValues) =>
            _logger.Error(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Logger implementation that forwards messages to Unity's logging system.
    /// </summary>
    public sealed class UnityLogger : ILogger
    {
        private readonly UnityEngine.ILogger _unityLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityLogger"/> class.
        /// </summary>
        /// <param name="unityLogger">Unity logger used as the output backend.</param>
        public UnityLogger(UnityEngine.ILogger unityLogger)
        {
            _unityLogger = unityLogger ?? throw new ArgumentNullException(nameof(unityLogger));
        }

        /// <inheritdoc />
        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            _unityLogger.Log(LogType.Log, Format(messageTemplate, propertyValues));
        }

        /// <inheritdoc />
        public void Information(string messageTemplate, params object[] propertyValues)
        {
            _unityLogger.Log(LogType.Log, Format(messageTemplate, propertyValues));
        }

        /// <inheritdoc />
        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            _unityLogger.Log(LogType.Warning, Format(messageTemplate, propertyValues));
        }

        /// <inheritdoc />
        public void Error(string messageTemplate, params object[] propertyValues)
        {
            _unityLogger.Log(LogType.Error, Format(messageTemplate, propertyValues));
        }

        private static string Format(string messageTemplate, params object[] propertyValues)
        {
            if (propertyValues == null || propertyValues.Length == 0)
            {
                return messageTemplate;
            }

            return string.Format(CultureInfo.InvariantCulture, messageTemplate, propertyValues);
        }
    }
}
