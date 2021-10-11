﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InnerLibs.ConsoleTables
{
    public class ConsoleTable
    {
        public List<string> Columns
        {
            get
            {
                return Options?.Columns;
            }

            set
            {
                if (value != null)
                {
                    Options.Columns = value;
                }
            }
        }

        public List<object[]> Rows { get; set; } = new List<object[]>();
        public ConsoleTableOptions Options { get; set; } = new ConsoleTableOptions();
        public Type[] ColumnTypes { get; set; }

        public ConsoleTable(params string[] columns)
        {
            Options.Columns.AddRange(columns ?? Array.Empty<string>());
        }

        public ConsoleTable(ConsoleTableOptions options = null)
        {
            Options = options ?? Options;
        }

        public ConsoleTable AddColumn(IEnumerable<string> names)
        {
            foreach (var name in names)
                Options.Columns.Add(name);
            return this;
        }

        public ConsoleTable AddValue(string Key, object obj)
        {
            if (Columns.Contains(Key))
            {
                var l = new List<object>();
                foreach (var item in Columns)
                {
                    if ((item ?? "") == (Key ?? ""))
                    {
                        l.Add(obj);
                    }
                    else
                    {
                        l.Add(string.Empty);
                    }
                }

                AddRow(l.ToArray());
            }

            return this;
        }

        public ConsoleTable AddRow(params object[] Values)
        {
            var v = (Values ?? Array.Empty<object>()).ToList();
            if (!Columns.Any())
            {
                Columns = Enumerable.Range(0, v.Count).Select(x => "Col" + x.ToString()).ToList();
            }

            while (v.Count < Columns.Count)
                v.Add(string.Empty);
            Rows.Add(v.Take(Columns.Count).ToArray());
            return this;
        }

        public ConsoleTable Configure(Action<ConsoleTableOptions> action)
        {
            action(Options);
            return this;
        }

        public static ConsoleTable From<T>(IEnumerable<T> values)
        {
            var table = new ConsoleTable() { ColumnTypes = GetColumnsType<T>().ToArray() };
            var columns = GetColumns<T>(false);
            table.AddColumn(GetColumns<T>());
            foreach (var propertyValues in values.Select(value => columns.Select(column => GetColumnValue<T>(value, column))))
                table.AddRow(propertyValues.ToArray());
            return table;
        }

        public string ToString(Format format)
        {
            switch (format)
            {
                case ConsoleTables.Format.MarkDown:
                    {
                        return ToMarkDownString();
                    }

                case ConsoleTables.Format.Alternative:
                    {
                        return ToStringAlternative();
                    }

                case ConsoleTables.Format.Minimal:
                    {
                        return ToMinimalString();
                    }

                default:
                    {
                        return ToString();
                    }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var columnLengths = ColumnLengths();
            var columnAlignment = Enumerable.Range(0, Columns.Count).Select(x => GetNumberAlignment(x)).ToList();
            string format = Enumerable.Range(0, Columns.Count).Select(i => " | {" + i + "," + columnAlignment[i] + columnLengths[i] + "}").Aggregate((s, a) => s + a) + " |";
            int maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
            string columnHeaders = string.Format(format, Columns.ToArray());
            int longestLine = Math.Max(maxRowLength, columnHeaders.Length);
            var results = Rows.Select(row => string.Format(format, row)).ToList();
            string divider = " " + string.Join("", Enumerable.Repeat("-", longestLine - 1)) + " ";
            builder.AppendLine(divider);
            builder.AppendLine(columnHeaders);
            foreach (var row in results)
            {
                builder.AppendLine(divider);
                builder.AppendLine(row);
            }

            builder.AppendLine(divider);
            if (Options.EnableCount)
            {
                builder.AppendLine("");
                builder.AppendFormat(" Count: {0}", Rows.Count);
            }

            return builder.ToString();
        }

        public string ToMarkDownString()
        {
            return ToMarkDownString('|');
        }

        private string ToMarkDownString(char delimiter)
        {
            var builder = new StringBuilder();
            var columnLengths = ColumnLengths();
            string format = Format(columnLengths, delimiter);
            string columnHeaders = string.Format(format, Columns.ToArray());
            var results = Rows.Select(row => string.Format(format, row)).ToList();
            string divider = Regex.Replace(columnHeaders, "[^|]", "-");
            builder.AppendLine(columnHeaders);
            builder.AppendLine(divider);
            results.ForEach(row => builder.AppendLine(row));
            return builder.ToString();
        }

        public string ToMinimalString()
        {
            return ToMarkDownString(char.MinValue);
        }

        public string ToStringAlternative()
        {
            var builder = new StringBuilder();
            var columnLengths = ColumnLengths();
            string format = Format(columnLengths);
            string columnHeaders = string.Format(format, Columns.ToArray());
            var results = Rows.Select(row => string.Format(format, row)).ToList();
            string divider = Regex.Replace(columnHeaders, "[^|]", "-");
            string dividerPlus = divider.Replace("|", "+");
            builder.AppendLine(dividerPlus);
            builder.AppendLine(columnHeaders);
            foreach (var row in results)
            {
                builder.AppendLine(dividerPlus);
                builder.AppendLine(row);
            }

            builder.AppendLine(dividerPlus);
            return builder.ToString();
        }

        private string Format(List<int> columnLengths, char delimiter = '|')
        {
            var columnAlignment = Enumerable.Range(0, Columns.Count).Select(x => GetNumberAlignment(x)).ToList();
            string delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
            return (Enumerable.Range(0, Columns.Count).Select(i => " " + delimiterStr + " {" + i + "," + columnAlignment[i] + columnLengths[i] + "}").Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();
        }

        private string GetNumberAlignment(int i)
        {
            return Options.NumberAlignment == Alignment.Right && ColumnTypes != null && Arrays.PrimitiveNumericTypes.Contains(ColumnTypes[i]) ? "" : "-";
        }

        private List<int> ColumnLengths()
        {
            return Columns.Select((t, i) => Rows.Select(x => x[i]).Union(new[] { Columns[i] }).Where(x => x != null).Select(x => x.ToString().Length).Max()).ToList();
        }

        public void Write(Format format = ConsoleTables.Format.Default)
        {
            Options.OutputTo.WriteLine(ToString(format));
        }

        private static IEnumerable<string> GetColumns<T>(bool FixCase = true)
        {
            return typeof(T).GetProperties().Select(x => !FixCase ? x.Name : x.Name.ToNormalCase().ToTitle()).ToArray();
        }

        private static object GetColumnValue<T>(object target, string column)
        {
            return typeof(T).GetProperty(column).GetValue(target, null);
        }

        private static IEnumerable<Type> GetColumnsType<T>()
        {
            return typeof(T).GetTypeOf().GetProperties().Select(x => x.PropertyType).ToArray();
        }
    }

    public class ConsoleTableOptions
    {
        public List<string> Columns { get; set; } = new List<string>();
        public bool EnableCount { get; set; } = true;
        public Alignment NumberAlignment { get; set; } = Alignment.Left;
        public TextWriter OutputTo { get; set; } = System.Console.Out;
    }

    public enum Format
    {
        Default = 0,
        MarkDown = 1,
        Alternative = 2,
        Minimal = 3
    }

    public enum Alignment
    {
        Left,
        Right
    }
}