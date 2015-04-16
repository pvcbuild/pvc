/// Based on code borrowed from Edokan.KaiZen.Colors by Erdogan Kurtur
/// github: https://github.com/edokan/Edokan.KaiZen.Colors

using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Edokan.KaiZen.Colors
{
    /// <summary>
    /// Dummy escape sequence parser.
    /// Can only parse color codes. Anything other than color will probably will 
    /// Must be able to parse other stuff.
    /// 
    /// </summary>
    public class EscapeSequencer : TextWriter
    {
        private static EscapeSequencer Instance;

        private readonly TextWriter textWriter;
        private enum States
        {
            Text,
            Signaled,
            Started
        }

        private readonly ConsoleColor defaultForegroundColor;
        private readonly ConsoleColor defaultBackgroundColor;
        private EscapeSequencer(TextWriter textWriter)
        {
            Instance = this;
            this.textWriter = textWriter;
            defaultForegroundColor = Console.ForegroundColor;
            defaultBackgroundColor = Console.BackgroundColor;
        }

        private States state = States.Text;
        private string escapeBuffer;
        private const char ESC = '\x1b';

        public override void WriteLine(string value)
        {
            base.WriteLine(string.Format("{0} {1}{2}", PvcConsole.Tag, PvcConsole.TaskOutputTag, value));
        }

        public override void Write(char value)
        {
            switch (state)
            {
                case States.Text:
                    if (value == ESC)
                    {
                        state = States.Signaled;
                        escapeBuffer = "";
                    }
                    else
                    {
                        textWriter.Write(value);
                    }
                    break;
                case States.Signaled:
                    if (value != '[')
                    {
                        textWriter.Write(ESC);
                        textWriter.Write(value);
                        state = States.Text;
                    }
                    else
                    {
                        state = States.Started;
                    }
                    break;
                case States.Started:
                    if (value != 'm')
                        escapeBuffer += value;
                    else
                    {
                        byte val;
                        if (byte.TryParse(escapeBuffer, out val))
                        {
                            if (val >= 30 && val <= 37 || val >= 90 && val <= 97)
                                SetForeColor(val);
                            else if (val == 39)
                                SetDefaultForeColor();
                            else if (val == 7 || val == 27)
                                SetInverse();
                            else if (val >= 40 && val <= 47)
                                SetBackColor(val);
                            else if (val == 49)
                                SetDefaultBackColor();
                        }
                        state = States.Text;
                    }
                    break;
            }

        }

        private bool isInverted;
        private void SetInverse()
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = c;
            isInverted = !isInverted;
        }
        
        private void SetDefaultBackColor()
        {
            if (isInverted)
                Console.BackgroundColor = defaultBackgroundColor;
            else
                Console.ForegroundColor = defaultBackgroundColor;
        }

        private void SetDefaultForeColor()
        {
            if (isInverted)
                Console.BackgroundColor = defaultForegroundColor;
            else
                Console.ForegroundColor = defaultForegroundColor;
        }

        private readonly ConsoleColor[] ColorMap = {
            ConsoleColor.Black,
            ConsoleColor.DarkRed, 
            ConsoleColor.DarkGreen, 
            ConsoleColor.DarkYellow, 
            ConsoleColor.DarkBlue, 
            ConsoleColor.DarkMagenta, 
            ConsoleColor.DarkCyan, 
            ConsoleColor.Gray,

            ConsoleColor.Black,
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan,
            ConsoleColor.White
        };

        private readonly Dictionary<byte, ConsoleColor> ColorDict = new Dictionary<byte, ConsoleColor>()
        {
            { 90, ConsoleColor.DarkGray },
            { 91, ConsoleColor.Red },
            { 92, ConsoleColor.Green },
            { 93, ConsoleColor.Yellow },
            { 94, ConsoleColor.Blue },
            { 95, ConsoleColor.Magenta },
            { 96, ConsoleColor.Cyan },
            { 97, ConsoleColor.Gray },

            { 30, ConsoleColor.Black },
            { 31, ConsoleColor.DarkRed },
            { 32, ConsoleColor.DarkGreen },
            { 33, ConsoleColor.DarkYellow },
            { 34, ConsoleColor.DarkBlue },
            { 35, ConsoleColor.DarkMagenta },
            { 36, ConsoleColor.DarkCyan },
            { 37, ConsoleColor.White }
        };

        private void SetBackColor(byte val)
        {
            if (isInverted)
                Console.ForegroundColor = ColorDict[val];
            else
                Console.BackgroundColor = ColorDict[val];
        }

        private void SetForeColor(byte val)
        {
            if (isInverted)
                Console.BackgroundColor = ColorDict[val];
            else
                Console.ForegroundColor = ColorDict[val];
        }

        public static void Install(bool convertANSI = true)
        {
            if (convertANSI)
            {
                Console.SetOut(new EscapeSequencer(Console.Out));
            }
            else
            {
                var stdout = Console.OpenStandardOutput();
                Console.SetOut(new StreamWriter(stdout)
                {
                    AutoFlush = true
                });
            }
        }

        public override Encoding Encoding
        {
            get { return textWriter.Encoding; }
        }
    }
}