﻿using InnerLibs.LINQ;
using InnerLibs.Locations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InnerLibs
{
    /// <summary>
    /// Geradores de conteudo
    /// </summary>
    /// <remarks></remarks>
    public static class Generate
    {
        #region Private Fields

        private static readonly Random init_rnd = new Random();

        #endregion Private Fields

        #region Public Methods

        /// <inheritdoc cref="BarcodeCheckSum(string)"/>
        public static string BarcodeCheckSum(long Code) => BarcodeCheckSum(Code.ToString());

        /// <inheritdoc cref="BarcodeCheckSum(string)"/>
        public static string BarcodeCheckSum(int Code) => BarcodeCheckSum(Code.ToString());

        /// <summary>
        /// Gera um digito verificador usando Mod10 em um numero
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static string BarcodeCheckSum(string Code)
        {
            if (Code.IsNotNumber())
            {
                throw new FormatException("Code is not number");
            }

            int i = 0;
            int j;
            int p = 0;
            int T;
            T = Code.Length;
            for (j = 1; j <= T; j++)
            {
                if ((j & ~-2) == 0)
                {
                    p += Code.Substring(j - 1, 1).ToInt();
                }
                else
                {
                    i += Code.Substring(j - 1, 1).ToInt();
                }
            }
            if (T == 7 | T == 11)
            {
                i = i * 3 + p;
                p = Converter.ToInt((i + 9) / 10) * 10;
                T = p - i;
            }
            else
            {
                p = p * 3 + i;
                i = Converter.ToInt((p + 9) / 10) * 10;
                T = i - p;
            }
            return T.ToString();
        }

        public static string EAN(int ContryCode, int ManufacturerCode, int ProductCode) => EANFromNumbers(ContryCode, ManufacturerCode, ProductCode);

        /// <summary>
        /// Gera um numero de EAN válido a aprtir da combinação de vários numeros
        /// </summary>
        /// <param name="Numbers"></param>
        /// <returns></returns>
        public static string EANFromNumbers(params string[] Numbers) => Numbers.Where(x => x.IsNumber()).SelectJoinString(InnerLibs.Text.Empty).AppendBarcodeCheckSum();

        /// <inheritdoc cref="EANFromNumbers(string[])"/>
        public static string EANFromNumbers(params int[] Numbers) => EANFromNumbers(Numbers.Select(x => x.ToString()).ToArray());

        /// <summary>
        /// Generate a password with specific lenght for each char type
        /// </summary>
        /// <param name="AlphaLenght"></param>
        /// <param name="NumberLenght"></param>
        /// <param name="SpecialLenght"></param>
        /// <returns></returns>
        public static string Password(int AlphaLenght, int NumberLenght, int SpecialLenght) => Password((AlphaLenght / 2d).CeilInt(), (AlphaLenght / 2d).FloorInt(), NumberLenght, SpecialLenght);

        /// <summary>
        /// Generate a password with specific lenght for each char type
        /// </summary>
        /// <returns></returns>
        public static string Password(int AlphaUpperLenght, int AlphaLowerLenght, int NumberLenght, int SpecialLenght)
        {
            string pass = InnerLibs.Text.Empty;
            if (AlphaLowerLenght > 0)
            {
                string ss = InnerLibs.Text.Empty;
                while (ss.Length < AlphaLowerLenght)
                {
                    ss = ss.Append(PredefinedArrays.AlphaLowerChars.RandomItem());
                }

                pass = pass.Append(ss);
            }

            if (AlphaUpperLenght > 0)
            {
                string ss = InnerLibs.Text.Empty;
                while (ss.Length < AlphaUpperLenght)
                {
                    ss = ss.Append(PredefinedArrays.AlphaUpperChars.RandomItem());
                }

                pass = pass.Append(ss);
            }

            if (NumberLenght > 0)
            {
                string ss = InnerLibs.Text.Empty;
                while (ss.Length < NumberLenght)
                {
                    ss = ss.Append(PredefinedArrays.NumberChars.RandomItem());
                }

                pass = pass.Append(ss);
            }

            if (SpecialLenght > 0)
            {
                string ss = InnerLibs.Text.Empty;
                while (ss.Length < SpecialLenght)
                {
                    ss = ss.Append(PredefinedArrays.PasswordSpecialChars.RandomItem());
                }

                pass = pass.Append(ss);
            }

            return pass.Shuffle();
        }

        /// <summary>
        /// Generate a password with specific <paramref name="Lenght"/>
        /// </summary>
        /// <param name="Lenght"></param>
        /// <returns></returns>
        public static string Password(int Lenght = 8)
        {
            var basenumber = Lenght / 3d;
            return Password(basenumber.CeilInt(), basenumber.FloorInt(), basenumber.FloorInt()).PadRight(Lenght, Convert.ToChar(PredefinedArrays.AlphaChars.RandomItem())).GetFirstChars(Lenght);
        }

        /// <summary>
        /// Gera um valor boolean aleatorio considerando uma porcentagem de chance
        /// </summary>
        /// <returns>TRUE ou FALSE.</returns>
        public static bool RandomBool(int Percent) => RandomBool(x => x <= Percent, 0, 100);

        /// <summary>
        /// Gera um valor boolean aleatorio considerando uma condição de comparação com um numero
        /// gerado aleatóriamente
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0</param>
        /// <param name="Max">Numero Maximo, Padrão 999999</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool RandomBool(Func<int, bool> Condition, int Min = 0, int Max = int.MaxValue) => Condition(RandomNumber(Min, Max));

        /// <summary>
        /// Gera um valor boolean aleatorio
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public static bool RandomBool() => RandomNumber(0, 1).ToBool();

        /// <summary>
        /// Gera uma lista com <paramref name="Quantity"/> cores diferentes
        /// </summary>
        /// <param name="Quantity">Quantidade máxima de cores</param>
        /// <param name="Red"></param>
        /// <param name="Green"></param>
        /// <param name="Blue"></param>
        /// <remarks></remarks>
        /// <returns></returns>
        public static List<Color> RandomColorList(int Quantity, int Red = -1, int Green = -1, int Blue = -1)
        {
            Red = Red.SetMinValue(-1);
            Green = Green.SetMinValue(-1);
            Blue = Blue.SetMinValue(-1);

            var l = new List<Color>();
            if (Red == Green && Green == Blue && Blue != -1)
            {
                l.Add(Color.FromArgb(Red, Green, Blue));
                return l;
            }

            int errorcount = 0;
            while (l.Count < Quantity)
            {
                var r = ColorExtensions.RandomColor(Red, Green, Blue);
                if (l.Any(x => (x.ToHexadecimal() ?? InnerLibs.Text.Empty) == (r.ToHexadecimal() ?? InnerLibs.Text.Empty)))
                {
                    errorcount++;
                    if (errorcount == Quantity)
                    {
                        return l;
                    }
                }
                else
                {
                    errorcount = 0;
                    l.Add(r);
                }
            }

            return l;
        }

        /// <summary>
        /// Gera uma data aleatória a partir de componentes nulos de data
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0</param>
        /// <param name="Max">Numero Maximo, Padrão <see cref="int.MaxValue"/></param>
        /// <returns>Um numero Inteiro</returns>
        public static DateTime RandomDateTime(int? Year = null, int? Month = null, int? Day = null, int? Hour = null, int? Minute = null, int? Second = null)
        {
            Year = (Year ?? RandomNumber(DateTime.MinValue.Year, DateTime.MaxValue.Year)).ForcePositive().LimitRange(DateTime.MinValue.Year, DateTime.MaxValue.Year);
            Month = (Month ?? RandomNumber(DateTime.MinValue.Month, DateTime.MaxValue.Month)).ForcePositive().LimitRange(1, 12);
            Day = (Day ?? RandomNumber(DateTime.MinValue.Day, DateTime.MaxValue.Day)).ForcePositive().LimitRange(1, 31);
            Hour = (Hour ?? RandomNumber(DateTime.MinValue.Hour, DateTime.MaxValue.Hour)).ForcePositive().LimitRange(1, 31);
            Minute = (Minute ?? RandomNumber(DateTime.MinValue.Minute, DateTime.MaxValue.Minute)).ForcePositive().LimitRange(0, 59);
            Second = (Second ?? RandomNumber(DateTime.MinValue.Second, DateTime.MaxValue.Second)).ForcePositive().LimitRange(0, 59);

            DateTime randomCreated = DateTime.Now;
            while (Misc.TryExecute(() => randomCreated = new DateTime(Year.Value, Month.Value, Day.Value, Hour.Value, Minute.Value, Second.Value)) != null)
            {
                Day--;
            }

            return randomCreated;
        }

        /// <summary>
        /// Gera uma data aleatória entre 2 datas
        /// </summary>
        /// <param name="Min">Data Minima</param>
        /// <param name="Max">Data Maxima</param>
        /// <returns>Um numero Inteiro</returns>
        public static DateTime RandomDateTime(DateTime? MinDate, DateTime? MaxDate = null)
        {
            var Min = (MinDate ?? RandomDateTime()).Ticks;
            var Max = (MaxDate ?? RandomDateTime()).Ticks;
            Misc.FixOrder(ref Min, ref Max);
            return new DateTime(RandomNumber(Min, Max));
        }

        /// <summary>
        /// Gera um EAN aleatório com digito verificador válido
        /// </summary>
        /// <param name="Len"></param>
        /// <returns></returns>
        public static string RandomEAN(int Len) => RandomFixLenghtNumber(Len.SetMinValue(2) - 1).ToString().AppendBarcodeCheckSum();

        /// <summary>
        /// Gera um numero aleatório de comprimento fixo
        /// </summary>
        /// <param name="Len"></param>
        /// <returns></returns>
        public static string RandomFixLenghtNumber(int Len = 8)
        {
            var n = InnerLibs.Text.Empty;
            for (int i = 0; i < Len; i++)
            {
                n += PredefinedArrays.NumberChars.RandomItem();
            }
            return n;
        }

        /// <summary>
        /// Gera um texto aleatorio
        /// </summary>
        /// <param name="ParagraphCount">Quantidade de paragrafos</param>
        /// <param name="SentenceCount">QUantidade de sentenças por paragrafo</param>
        /// <param name="MinWordCount"></param>
        /// <param name="MaxWordCount"></param>
        /// <returns></returns>
        public static TextStructure RandomIpsum(int ParagraphCount = 5, int SentenceCount = 3, int MinWordCount = 10, int MaxWordCount = 50, int IdentSize = 0, int BreakLinesBetweenParagraph = 0) => new TextStructure(Enumerable.Range(1, ParagraphCount.SetMinValue(1)).SelectJoinString(pp => Enumerable.Range(1, SentenceCount.SetMinValue(1)).SelectJoinString(s => Enumerable.Range(1, RandomNumber(MinWordCount.SetMinValue(1), MaxWordCount.SetMinValue(1))).SelectJoinString(p => RandomBool(20).AsIf(RandomWord(RandomNumber(2, 6)).ToUpper(), RandomWord()) + RandomBool(30).AsIf(","), " "), PredefinedArrays.EndOfSentencePunctuation.TakeRandom() + " "), Environment.NewLine)) { Ident = IdentSize, BreakLinesBetweenParagraph = BreakLinesBetweenParagraph };

        /// <summary>
        /// Gera um numero Aleatório entre 2 números
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0</param>
        /// <param name="Max">Numero Maximo, Padrão <see cref="int.MaxValue"/></param>
        /// <returns>Um numero Inteiro</returns>
        public static int RandomNumber(int Min = 0, int Max = int.MaxValue)
        {
            Misc.FixOrder(ref Min, ref Max);
            return Min == Max ? Min : init_rnd.Next(Min, Max == int.MaxValue ? int.MaxValue : Max + 1);
        }

        /// <summary>
        /// Gera um numero Aleatório entre 2 números
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0</param>
        /// <param name="Max">Numero Maximo, Padrão <see cref="long.MaxValue"/></param>
        /// <returns>Um numero Inteiro</returns>
        public static long RandomNumber(long Min, long Max = long.MaxValue)
        {
            Misc.FixOrder(ref Min, ref Max);
            if (Min == Max)
            {
                return Min;
            }
            else
            {
                Max = Max == long.MaxValue ? long.MaxValue : Max + 1;
                byte[] buf = new byte[8];
                init_rnd.NextBytes(buf);
                long longRand = BitConverter.ToInt64(buf, 0);
                return Math.Abs(longRand % (Max - Min)) + Min;
            }
        }

        /// <summary>
        /// Gera uma Lista com numeros Aleatórios entre 2 números
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0</param>
        /// <param name="Max">Numero Maximo, Padrão <see cref="int.MaxValue"/></param>
        /// <returns>Um numero Inteiro</returns>
        public static IEnumerable<int> RandomNumberList(int Count, int Min = 0, int Max = int.MaxValue, bool UniqueNumbers = true)
        {
            if (Count > 0)
            {
                if (Max == Min) return new int[] { Min };
                if (UniqueNumbers)
                {
                    if (Max < int.MaxValue) Max++;
                    Misc.FixOrder(ref Min, ref Max);
                    var l = Enumerable.Range(Min, Max - Min).OrderByRandom().ToList();
                    while (l.Count > Count) l.RemoveAt(0);
                    return l;
                }
                else
                {
                    return Enumerable.Range(1, Count).Select(e => Generate.RandomNumber(Min, Max));
                }
            }
            return Array.Empty<int>();
        }

        /// <summary>
        /// Gera uma palavra aleatória com o numero de caracteres entre <paramref name="MinLength"/>
        /// e <paramref name="MaxLenght"/>
        /// </summary>
        /// <returns>Uma string contendo uma palavra aleatória</returns>
        public static string RandomWord(int MinLength, int MaxLenght) => RandomWord(RandomNumber(MinLength.SetMinValue(1), MaxLenght.SetMinValue(1)));

        /// <summary>
        /// Gera uma palavra aleatória com o numero de caracteres
        /// </summary>
        /// <param name="Length">Tamanho da palavra</param>
        /// <returns>Uma string contendo uma palavra aleatória</returns>
        public static string RandomWord(int Length = 0)
        {
            Length = Length < 1 ? RandomNumber(2, 15) : Length;
            string word = InnerLibs.Text.Empty;
            if (Length == 1)
            {
                return Text.RandomItem(PredefinedArrays.AlphaLowerChars.ToArray());
            }

            // Generate the word in consonant / vowel pairs
            while (word.Length < Length)
            {
                // Add the consonant
                string consonant = PredefinedArrays.LowerConsonants.RandomItem();
                if (consonant == "q" && word.Length + 3 <= Length)
                {
                    // check +3 because we'd add 3 characters in this case, the "qu" and the vowel.
                    // Change 3 to 2 to allow words that end in "qu"
                    word += "qu";
                }
                else
                {
                    while (consonant == "q")
                    {
                        // ReplaceFrom an orphaned "q"
                        consonant = PredefinedArrays.LowerConsonants.RandomItem();
                    }

                    if (word.Length + 1 <= Length)
                    {
                        // Only add a consonant if there's enough room remaining
                        word += consonant;
                    }
                }

                if (word.Length + 1 <= Length)
                {
                    // Only add a vowel if there's enough room remaining
                    word += PredefinedArrays.LowerVowels.RandomItem();
                }
            }

            return word;
        }

        /// <summary>
        /// Gera uma URL do google MAPs baseado na localização
        /// </summary>
        /// <param name="local">
        /// Uma variavel do tipo InnerLibs.Location onde estão as informações como endereço e as
        /// coordenadas geográficas
        /// </param>
        /// <param name="LatLong">
        /// Gerar URL baseado na latitude e Longitude. Padrão FALSE retorna a URL baseada no Logradouro
        /// </param>
        /// <returns>Uma URI do Google Maps</returns>
        public static Uri ToGoogleMapsURL(this AddressInfo local, bool LatLong = false)
        {
            string s;
            if (LatLong == true && local.Latitude.HasValue && local.Longitude.HasValue)
            {
                s = Uri.EscapeUriString(local.LatitudeLongitude().TrimBetween());
            }
            else
            {
                s = Uri.EscapeUriString(local.FullAddress.FixText());
            }

            return new Uri("https://www.google.com.br/maps/search/" + s);
        }

        #endregion Public Methods
    }
}