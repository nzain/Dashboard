using System;
using System.Collections.Generic;
using NLog;
using NLog.Conditions;
using NLog.Targets;
using NLog.Web;

namespace nZain.Dashboard.Host
{
    internal static class DebugLoggingConfig
    {
        internal static void ActivateDebugLogging()
        {
            var config = LogManager.Configuration;
            // Step 2. Create targets
            var debugTarget = new ColoredConsoleTarget("debugTarget")
            {
                Layout = @"${date:format=HH\:mm\:ss.fff}|${uppercase:${level:format=FirstCharacter}}|${logger:shortName=true}|${message} ${exception:format=tostring} | ${aspnet-request-url}"
            };

            debugTarget.RowHighlightingRules.Clear();
            debugTarget.RowHighlightingRules.Add(MakeRule(LogLevel.Fatal, ConsoleOutputColor.Magenta));
            debugTarget.RowHighlightingRules.Add(MakeRule(LogLevel.Error, ConsoleOutputColor.Red));
            debugTarget.RowHighlightingRules.Add(MakeRule(LogLevel.Warn, ConsoleOutputColor.Yellow));
            debugTarget.RowHighlightingRules.Add(MakeRule(LogLevel.Info, ConsoleOutputColor.White));
            debugTarget.RowHighlightingRules.Add(MakeRule(LogLevel.Debug, ConsoleOutputColor.Gray));
            debugTarget.RowHighlightingRules.Add(MakeRule(LogLevel.Trace, ConsoleOutputColor.DarkGray));
            
            config.AddTarget(debugTarget);
            config.AddRuleForAllLevels(debugTarget); // all to console
            LogManager.Configuration = config;
        }

        private static ConsoleRowHighlightingRule MakeRule(LogLevel level, ConsoleOutputColor fg)
        {
            ConsoleOutputColor bg = ConsoleOutputColor.Black;
            ConditionExpression condition = ConditionParser.ParseExpression("level == LogLevel." + level);
            return new ConsoleRowHighlightingRule(condition, fg, bg);
        }
    }
}