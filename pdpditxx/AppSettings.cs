using System;
using System.Collections.Generic;
using System.Reflection;

namespace pdpditxx
{
    internal class AppSettings
    {
        public class Root
        {
            public string Author { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
            public bool EnableDebug { get; set; } = false;
            public string Notes { get; set; } = "No JSON has been read in";
            public Processingactions ProcessingActions { get; set; } = new Processingactions();
            public Settings Settings { get; set; } = new Settings();
        }
        public class Processingactions
        {
            public bool Split { get; set; } = false;
            public bool Concatenate { get; set; } = false;
            public bool MakeCopies { get; set; } = false;
            public bool ScaleAndRotate { get; set; } = false;
            public bool SmartSave { get; set; } = false;
            public bool TextConvert { get; set; } = false;
        }
        public class Settings
        {
            public Concatenation Concatenation { get; set; } = new Concatenation();
            public MakeCopies MakeCopies { get; set; } = new MakeCopies();
            public SmartSaving SmartSaving { get; set; } = new SmartSaving();
            public TargetPageSize TargetPageSize { get; set; } = new TargetPageSize();
        }

        public class Concatenation
        {
            public bool AddDocBreak { get; set; }
            public string BreakText { get; set; }
        }

        public class MakeCopies
        {
            public int NumberOfCopies { get; set; }
        }

        public class SmartSaving
        {
            public bool StripComments { get; set; } = false;
            public bool FlattenAcroforms { get; set; } = false;
            public bool RemovePassword { get; set; } = false;
        }

        public class TargetPageSize
        {
            public float PageHeight { get; set; }
            public float PageWidth { get; set; }
        }

        public static void SetAllProcessingActionsFalse(Root appSettings)
        {
            foreach (PropertyInfo action in appSettings.ProcessingActions.GetType().GetProperties())
            {
                action.SetValue(appSettings.ProcessingActions, false);
            }
        }

        public static string GetTrueProcessingAction(Root appSettings)
        {
            foreach (PropertyInfo action in appSettings.ProcessingActions.GetType().GetProperties())
            {
                if (bool.Parse(action.GetValue(appSettings.ProcessingActions).ToString()) == true)
                {
                    return action.Name;
                }
            }
            return null;
        }

        public static List<ErrorMessage> ValidateConfig(Root appSettings, List<ErrorMessage> messageList)
        {
            int trueCount = 0;
            List<string> actionList = new List<string>();

            foreach (PropertyInfo action in appSettings.ProcessingActions.GetType().GetProperties())
            {
                if (bool.Parse(action.GetValue(appSettings.ProcessingActions).ToString()) == true)
                {
                    Console.WriteLine($"{action.Name} : {action.GetValue(appSettings.ProcessingActions)}");
                    actionList.Add(action.Name);
                    trueCount++;
                }
            }

            if (trueCount < 1)
            {
                // If all booleans are false, throw exception and end now
                Console.WriteLine($"All processing actions are false.");
                throw new ArgumentOutOfRangeException("All Processing actions are false. Ensure variables are correct or check previous errors.");
            }

            if (trueCount > 1)
            {
                string eachAction = string.Join(" & ", actionList.ToArray());
                Console.WriteLine($"You can only call a single action. Your config file calls {trueCount}.");
                throw new ArgumentOutOfRangeException($"You can only call a single action. Your config file has {trueCount} actions set to true : {eachAction}");
            }

            return messageList;
        }
    }
}