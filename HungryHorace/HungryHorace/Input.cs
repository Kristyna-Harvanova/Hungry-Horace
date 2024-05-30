using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungryHorace
{
    static class Input
    {
        // Array contains information, if arrow keys are pressed. Order: left, up, right, down.
        public static bool[] Pressed = new bool[4];  

        public static int ReadInt(System.IO.StreamReader textFile)
        {
            int character = textFile.Read();

            // Skipping unallowed characters.
            while (character < '0' || character > '9')
            {
                character = textFile.Read();
            }

            int number = 0;
            while ((character >= '0') && (character <= '9'))
            {
                number = 10 * number + character - '0';
                character = textFile.Read();
            }
            return number;
        }

        /// <summary>
        /// Read coherent text (without blank spaces).
        /// </summary>
        /// <param name="textFile"></param>
        /// <returns></returns>
        public static string ReadString(System.IO.StreamReader textFile)
        {
            StringBuilder sb = new StringBuilder();
            int character = textFile.Read();

            // Skipping invalid characters (0 - 32) for string.
            while (character < 33)
            {
                character = textFile.Read();
            }

            // Valid characters (' ' = 32, '!' = 33, ...).
            while (character >= 33)
            {
                sb.Append((char)character);
                character = textFile.Read();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Read valid game character from file.
        /// </summary>
        /// <param name="textFile"></param>
        /// <returns></returns>
        public static char ReadChar(System.IO.StreamReader textFile)
        {
            string tiles = TileChars();
            char character = (char)textFile.Read();
            while (!tiles.Contains(character)) character = (char)textFile.Read();
            return character;
        }

        /// <summary>
        /// Defines valid game characters and translate to TileType.
        /// </summary>
        /// <returns></returns>
        public static string TileChars()    
        {
            StringBuilder tiles = new StringBuilder();
            foreach (TileType obj in Enum.GetValues(typeof(TileType)))
            {
                tiles.Append((char)obj);
            }
            return tiles.ToString();
        }

        // Translation from 10 (decimal) system to 128 system. Characters for 128 systems are ASCII chars (according to their codes in ASCII table).
        // Score is int type (32 bits), score is not negative (16 bits) -> maximum: 128 * 128 * 128 * 128 - 1 = 268435455 (enough for potential score).

        /// <summary>
        /// Int value ecrypt to characters.
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public static string TranslateFromInt(int score)
        {   
            StringBuilder translation = new StringBuilder();

            // Every multiple of order 128 is written as a character according to ASCII
            for (int i = 0; i < 4; i++)   
            {
                translation.Append((char)(score % (int)Math.Pow(128, i + 1) / (int)Math.Pow(128, i)));
            }

            return translation.ToString();
        }

        /// <summary>
        /// Character value decrypt to int.
        /// </summary>
        /// <param name="textFile"></param>
        /// <returns></returns>
        public static int TranslateToInt(System.IO.StreamReader textFile) 
        {   
            // Translate number from 128 system to decimal.
            int translation = 0;

            for (int i = 0; i < 4; i++)
            {
                int value = textFile.Read();
                translation += (int)Math.Pow(128, i) * value;
            }
            
            return translation;
        }
    }
}