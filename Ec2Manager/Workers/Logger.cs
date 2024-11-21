using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Ec2Manager.Workers
{
    /// <summary>
    /// Provides logging functionality using log4net.
    /// </summary>
    public static class Logger
    {
        private static readonly string LOG_CONFIG_FILE = @"log4net.config";
        private static readonly ILog _log = GetLogger(typeof(Logger));
        private static bool _isConfigured = false;

        /// <summary>
        /// Gets a logger instance for the specified type.
        /// </summary>
        /// <param name="type">The type for which to get the logger.</param>
        /// <returns>An instance of <see cref="ILog"/> for the specified type.</returns>
        public static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
            if (!_isConfigured)
            {
                SetLog4NetConfiguration();
                _isConfigured = true;
            }
            _log.Debug(message);
        }

        /// <summary>
        /// Configures log4net using the configuration file.
        /// </summary>
        private static void SetLog4NetConfiguration()
        {
            XmlDocument log4netConfig = new();
            using (FileStream fs = File.OpenRead(LOG_CONFIG_FILE))
            {
                log4netConfig.Load(fs);
            }

            log4net.Repository.ILoggerRepository repo = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }
    }
}