/// Based on code borrowed from Edokan.KaiZen.Colors by Erdogan Kurtur
/// github: https://github.com/edokan/Edokan.KaiZen.Colors

using System;
using System.Collections.Generic;
using System.Text;

namespace Edokan.KaiZen.Colors
{
    public static class ColorsExtension
    {
        public struct ColorWrap
        {
            public byte Start { get; private set; }
            public byte End { get; private set; }

            public ColorWrap(byte start, byte end)
                : this()
            {
                Start = start;
                End = end;
            }
        }

        public static readonly Dictionary<string, ColorWrap> DefaultTheme = new Dictionary<string, ColorWrap>
            {
                {"bold", new ColorWrap(1, 22)},
                {"italic", new ColorWrap(3, 23)},
                {"underline", new ColorWrap(4, 24)},
                {"inverse", new ColorWrap(7, 27)},
                
                {"reset", new ColorWrap(39, 49)},

                {"darkgrey", new ColorWrap(90, 39)},
                {"red", new ColorWrap(91, 39)},
                {"green", new ColorWrap(92, 39)},
                {"yellow", new ColorWrap(93, 39)},
                {"blue", new ColorWrap(94, 39)},
                {"magenta", new ColorWrap(95, 39)},
                {"cyan", new ColorWrap(96, 39)},
                {"grey", new ColorWrap(97, 39)},
                
                {"black", new ColorWrap(30, 39)},
                {"darkred", new ColorWrap(31, 39)},
                {"darkgreen", new ColorWrap(32, 39)},
                {"darkyellow", new ColorWrap(33, 39)},
                {"darkblue", new ColorWrap(34, 39)},
                {"darkmagenta", new ColorWrap(35, 39)},
                {"darkcyan", new ColorWrap(36, 39)},
                {"white", new ColorWrap(37, 39)}
            };

        private static Dictionary<string, ColorWrap> Theme = DefaultTheme;

        public static string Bold(this string s) { return Wrap(s, "bold"); }
        public static string Italic(this string s) { return Wrap(s, "italic"); }
        public static string Underline(this string s) { return Wrap(s, "underline"); }
        public static string Inverse(this string s) { return Wrap(s, "inverse"); }
        public static string White(this string s) { return Wrap(s, "white"); }
        public static string Grey(this string s) { return Wrap(s, "grey"); }
        public static string Black(this string s) { return Wrap(s, "black"); }
        public static string Blue(this string s) { return Wrap(s, "blue"); }
        public static string Cyan(this string s) { return Wrap(s, "cyan"); }
        public static string Green(this string s) { return Wrap(s, "green"); }
        public static string Magenta(this string s) { return Wrap(s, "magenta"); }
        public static string Red(this string s) { return Wrap(s, "red"); }
        public static string Yellow(this string s) { return Wrap(s, "yellow"); }

        public static string DarkGrey(this string s) { return Wrap(s, "darkgrey"); }
        public static string DarkRed(this string s) { return Wrap(s, "darkred"); }
        public static string DarkGreen(this string s) { return Wrap(s, "darkgreen"); }
        public static string DarkYellow(this string s) { return Wrap(s, "darkyellow"); }
        public static string DarkBlue(this string s) { return Wrap(s, "darkblue"); }
        public static string DarkMagenta(this string s) { return Wrap(s, "darkmagenta"); }
        public static string DarkCyan(this string s) { return Wrap(s, "darkcyan"); }

        public static string Reset(this string s) { return Wrap("", "reset") + s; }


        private static bool isBg = false;
        public static string On(this string s)
        {
            isBg = true;
            return s;
        }

        public static string Color(this string str, string color)
        {
            return Wrap(str, color);
        }

        private static string Wrap(string str, string color)
        {
            var w = Theme[color];

            int start = w.Start;
            int end = w.End;
            if (isBg && start >= 30 && start <= 37)
            {
                isBg = false;
                start += 10;
                end += 10;
            }

            isBg = false;
            return string.Format("\x1b[{0}m{1}\x1b[{2}m", start, str, end);
        }

        private static string Wrap(char c, string color)
        {
            return Wrap(c.ToString(), color);
        }

        public static string RunSequencer(string s, Func<string, int, char, string> sequencer)
        {
            if (null == s)
                return null;

            var sb = new StringBuilder();
            for (int n = 0; n < s.Length; n++)
            {
                sb.Append(sequencer(s, n, s[n]));
            }

            return sb.ToString();
        }

        public static string Zebra(this string s)
        {
            return RunSequencer(
                s,
                (str, i, c) => (0 == i % 2) ? Wrap(c, "inverse") : c.ToString()
                );
        }

        public static string Rainbow(this string s)
        {
            var rainbowColors = new[] { "red", "yellow", "green", "blue", "magenta" }; //RoY G BiV
            return RunSequencer(
                s,
                (str, i, c) => (char.IsWhiteSpace(c) ? c.ToString() : Wrap(c, rainbowColors[(i + 1) % rainbowColors.Length]))
                );
        }
    }
}