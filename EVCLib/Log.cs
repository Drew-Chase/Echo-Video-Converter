using ChaseLabs.Echo.Video_Converter.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace ChaseLabs.Echo.Video_Converter.Resources
{

    internal class Log<String> : List<string>
    {

        public enum LogType
        {
            Info,
            Warning,
            Error,
            Debug
        }

        private TextBlock LogBlock = null;
        private ScrollViewer ConsoleOutputScrView;
        private static Log<string> _singleton;
        public static Log<string> Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new Log<string>();
                }

                return _singleton;
            }
        }


        public void setLogBlock(TextBlock block)
        {
            LogBlock = block;
        }

        public TextBlock getLogBlock()
        {
            return LogBlock;
        }

        public void setScrollView(ScrollViewer view)
        {
            ConsoleOutputScrView = view;
        }

        public ScrollViewer getScrollView()
        {
            return ConsoleOutputScrView;
        }

        public void Close()
        {
            try
            {
                string path = Path.Combine(Directory.GetParent(Values.Singleton.LogFileLocation).FullName, $"{Values.Singleton.FormattedTime}.log");
                File.Move(Values.Singleton.LogFileLocation, path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Adds a Log a Single Message with Info Log Type
        /// </summary>
        /// <param name="message">Message</param>
        public new void Add(string message)
        {
            Add(LogType.Info, message);
        }

        /// <summary>
        /// Adds a Log Message(s) with Log Type
        /// </summary>
        /// <param name="type">Log Type</param>
        /// <param name="message">Message</param>
        public void Add(LogType? type, params string[] message)
        {
            if (getLogBlock() == null)
            {
                return;
            }
            string prefix = "INFO";
            switch (type)
            {
                case LogType.Info:
                    prefix = "INFO";
                    break;
                case LogType.Warning:
                    prefix = "WARNING";
                    break;
                case LogType.Error:
                    prefix = "ERROR";
                    break;
                case LogType.Debug:
                    prefix = "DEBUG";
                    break;
                default:
                    prefix = "INFO";
                    break;
            }

            try
            {
                foreach (string obj in message)
                {
                    if (obj.GetType().Equals(typeof(string)))
                    {
                        base.Add(obj);
                        StringWriter consoleOut = new StringWriter();
                        Console.SetOut(consoleOut);
                        base.Add(consoleOut.ToString());
                        string data = $"[ {prefix}: {Values.Singleton.UnformattedTime} ]: {obj}";
                        Console.WriteLine(data);
                        getLogBlock().Text += consoleOut.ToString() + "\n";
                        getScrollView().ScrollToBottom();
                        ConfigUtilities config = ConfigUtilities.Singleton;
                        string log_path = Values.Singleton.LogLocation;
                        config.Write(data, "latest.log", log_path, false);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(Values.Singleton.UnformattedTime + ": " + e.Message);
                Console.WriteLine(Values.Singleton.UnformattedTime + ": " + e.StackTrace);
            }
        }
    }
}
