﻿using System;
using System.Text.RegularExpressions;

namespace InnerLibs
{
    internal class Like
    {
        private String sPattern;

        public String Pattern
        { get => sPattern; private set => sPattern = value; }

        private RegexOptions _Options;

        public RegexOptions Options
        { get => _Options; private set => _Options = value; }

        public Like(String sPattern, RegexOptions pOptions = RegexOptions.Multiline)
        {
            Pattern = sPattern;
            Options = pOptions;
        }

        public bool Matches(String test)
        {
            if (test == null) return false;
            return IsLike(test, Pattern, Options);
        }

        public static explicit operator Like(String Source)
        {
            return new Like(Source);
        }

        public static bool operator ==(String first, Like second) => second != null ? second.Matches(first) : false;

        public static bool operator !=(string First, Like second) => !(First == second);

        public override bool Equals(object obj) => obj?.ToString() == this;

        public override int GetHashCode() => 0;

        private static bool IsLike(String value, String mask, RegexOptions options = RegexOptions.Multiline & RegexOptions.IgnorePatternWhitespace)
        {
            String usepattern = "^" + Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(value, usepattern, options);
        }
    }
}