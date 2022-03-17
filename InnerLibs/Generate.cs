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
        /// <summary>
        /// Gera uma palavra aleatória com o numero de caracteres entre <paramref name="MinLength"/> e<paramref name="MaxLenght"/>
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
            string word = "";
            if (Length == 1)
            {
                return Text.RandomItem(PredefinedArrays.LowerConsonants.Union(PredefinedArrays.LowerVowels).ToArray());
            }

            // Generate the word in consonant / vowel pairs
            while (word.Length < Length)
            {
                // Add the consonant
                string consonant = PredefinedArrays.LowerConsonants.RandomItem();
                if (consonant == "q" && word.Length + 3 <= Length)
                {
                    // check +3 because we'd add 3 characters in this case, the "qu" and the vowel.  Change 3 to 2 to allow words that end in "qu"
                    word += "qu";
                }
                else
                {
                    while (consonant == "q")
                        // ReplaceFrom an orphaned "q"
                        consonant = PredefinedArrays.LowerConsonants.RandomItem();
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

        public static string Password(int AlphaLenght, int NumberLenght, int SpecialLenght) => Password((AlphaLenght / 2d).CeilInt(), (AlphaLenght / 2d).FloorInt(), NumberLenght, SpecialLenght);

        /// <summary>
        /// Gera uma senha
        /// </summary>
        /// <returns></returns>
        public static string Password(int AlphaUpperLenght, int AlphaLowerLenght, int NumberLenght, int SpecialLenght)
        {
            string pass = "";
            if (AlphaLowerLenght > 0)
            {
                string ss = "";
                while (ss.Length < AlphaLowerLenght)
                    ss = ss.Append(PredefinedArrays.AlphaLowerChars.RandomItemOr());
                pass = pass.Append(ss);
            }

            if (AlphaUpperLenght > 0)
            {
                string ss = "";
                while (ss.Length < AlphaUpperLenght)
                    ss = ss.Append(PredefinedArrays.AlphaUpperChars.RandomItem());
                pass = pass.Append(ss);
            }

            if (NumberLenght > 0)
            {
                string ss = "";
                while (ss.Length < NumberLenght)
                    ss = ss.Append(PredefinedArrays.NumberChars.RandomItem());
                pass = pass.Append(ss);
            }

            if (SpecialLenght > 0)
            {
                string ss = "";
                while (ss.Length < SpecialLenght)
                    ss = ss.Append(PredefinedArrays.PasswordSpecialChars.RandomItem());
                pass = pass.Append(ss);
            }

            return pass.Shuffle();
        }

        public static string Password(int Lenght = 8)
        {
            var basenumber = Lenght / 3d;
            return Password(basenumber.CeilInt(), basenumber.FloorInt(), basenumber.FloorInt()).PadRight(Lenght, Convert.ToChar(PredefinedArrays.AlphaChars.RandomItem())).GetFirstChars(Lenght);
        }

        /// <summary>
        /// Gera uma URL do google MAPs baseado na localização
        /// </summary>
        /// <param name="local">Uma variavel do tipo InnerLibs.Location onde estão as informações como endereço e as coordenadas geográficas</param>
        /// <param name="LatLong">Gerar URL baseado na latitude e Longitude. Padrão FALSE retorna a URL baseada no Logradouro</param>
        /// <returns>Uma URI do Google Maps</returns>

        public static Uri ToGoogleMapsURL(this AddressInfo local, bool LatLong = false)
        {
            string s;
            if (LatLong == true && local.Latitude.HasValue && local.Longitude.HasValue)
            {
                s = Uri.EscapeUriString(local.LatitudeLongitude().AdjustWhiteSpaces());
            }
            else
            {
                s = Uri.EscapeUriString(local.FullAddress.AdjustWhiteSpaces());
            }

            return new Uri("https://www.google.com.br/maps/search/" + s);
        }

        /// <summary>
        /// Gera um valor boolean aleatorio considerando uma porcentagem de chance
        /// </summary>
        /// <returns>TRUE ou FALSE.</returns>
        public static bool RandomBoolean(int Percent) => RandomBoolean(x => x <= Percent, 0, 100);

        /// <summary>
        /// Gera um valor boolean aleatorio considerando uma condição de comparação com um numero gerado aleatóriamente
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0 </param>
        /// <param name="Max">Numero Maximo, Padrão 999999</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool RandomBoolean(Func<int, bool> Condition, int Min = 0, int Max = int.MaxValue) => Condition(RandomNumber(Min, Max));

        /// <summary>
        /// Gera um valor boolean aleatorio
        /// </summary>
        /// <returns>TRUE ou FALSE</returns>
        public static bool RandomBoolean() => RandomNumber(0, 1).ToBoolean();

        /// <summary>
        /// Gera um numero Aleatório entre 2 números
        /// </summary>
        /// <param name="Min">Numero minimo, Padrão 0 </param>
        /// <param name="Max">Numero Maximo, Padrão <see cref="int.MaxValue"/></param>
        /// <returns>Um numero Inteiro</returns>
        public static int RandomNumber(int Min = 0, int Max = int.MaxValue) => init_rnd.Next(Min, Max == int.MaxValue ? int.MaxValue : Max + 1);

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
                if (l.Any(x => (x.ToHexadecimal() ?? "") == (r.ToHexadecimal() ?? "")))
                {
                    errorcount = errorcount + 1;
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

        private static Random init_rnd = new Random();

        /// <summary>
        /// Gera um texto aleatorio
        /// </summary>
        /// <param name="ParagraphCount">Quantidade de paragrafos</param>
        /// <param name="SentenceCount">QUantidade de sentenças por paragrafo</param>
        /// <param name="MinWordCount"></param>
        /// <param name="MaxWordCount"></param>
        /// <returns></returns>
        public static StructuredText RandomIpsum(int ParagraphCount = 5, int SentenceCount = 3, int MinWordCount = 10, int MaxWordCount = 50, int IdentSize = 0, int BreakLinesBetweenParagraph = 0) => new StructuredText(Enumerable.Range(1, ParagraphCount.SetMinValue(1)).SelectJoinString(pp => Enumerable.Range(1, SentenceCount.SetMinValue(1)).SelectJoinString(s => Enumerable.Range(1, RandomNumber(MinWordCount.SetMinValue(1), MaxWordCount.SetMinValue(1))).SelectJoinString(p => RandomBoolean(20).AsIf(RandomWord(RandomNumber(2, 6)).ToUpper(), RandomWord()) + RandomBoolean(30).AsIf(","), " "), PredefinedArrays.EndOfSentencePunctuation.TakeRandom() + " "), Environment.NewLine)) { Ident = IdentSize, BreakLinesBetweenParagraph = BreakLinesBetweenParagraph };
    }
}