﻿using ChaseLabs.CLLogger;
using System.Collections.Generic;

namespace ChaseLabs.Echo.Video_Converter.Resources
{
    public enum SortType
    {
        Size,
        Alphabetical
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class MediaFiles : List<MediaFile>
    {
        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).SetMinimumLogType(Lists.LogTypes.All);

        public static string SortTypeToString(SortType value)
        {
            if (value == SortType.Alphabetical)
            {
                return "Alphabetical";
            }

            if (value == SortType.Size)
            {
                return "Size";
            }

            return "";
        }

        public static string SortOrderToString(SortOrder value)
        {
            if (value == SortOrder.Ascending)
            {
                return "Ascending";
            }

            if (value == SortOrder.Descending)
            {
                return "Descending";
            }

            return "";
        }

        public static SortType ParseType(string value)
        {
            if (value.ToLower().Contains("alphabetical"))
            {
                return SortType.Alphabetical;
            }

            if (value.ToLower().Contains("size"))
            {
                return SortType.Size;
            }

            return SortType.Alphabetical;
        }

        public static SortOrder ParseOrder(string value)
        {
            if (value.ToLower().Contains("ascending"))
            {
                return SortOrder.Ascending;
            }

            if (value.ToLower().Contains("descending"))
            {
                return SortOrder.Descending;
            }

            return SortOrder.Ascending;
        }

        public MediaFiles Get(SortType type, SortOrder order)
        {
            MediaFiles files = this;
            if (type == SortType.Alphabetical)
            {
                if (order == SortOrder.Descending)
                {
                    files.Sort((x, y) => x.FileName.CompareTo(y.FileName));
                }

                if (order == SortOrder.Ascending)
                {
                    files.Sort((x, y) => y.FileName.CompareTo(x.FileName));
                }
            }
            if (type == SortType.Size)
            {
                if (order == SortOrder.Descending)
                {
                    files.Sort((x, y) => x.Size.CompareTo(y.Size));
                }

                if (order == SortOrder.Ascending)
                {
                    files.Sort((x, y) => y.Size.CompareTo(x.Size));
                }
            }
            return files;
        }
    }

}
