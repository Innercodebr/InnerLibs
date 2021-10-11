﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace InnerLibs
{

    /// <summary>
    /// Verifica determinados valores como Arquivos, Numeros e URLs
    /// </summary>
    /// <remarks></remarks>
    public static class Verify
    {

        /// <summary>
        /// Verifica se a string é um CNH válido
        /// </summary>
        /// <param name="Text">CNH</param>
        /// <returns></returns>
        public static bool IsValidCNH(this string cnh)
        {
            bool isValid = false;
            char firstChar = cnh[0];
            if (cnh.Length == 11 && (cnh ?? "") != (new string('1', 11) ?? ""))
            {
                int dsc = 0;
                int v = 0;
                int i = 0;
                int j = 9;
                while (i < 9)
                {
                    v += Convert.ToInt32(cnh[i].ToString()) * j;
                    i += 1;
                    j -= 1;
                }

                int vl1 = v % 11;
                if (vl1 >= 10)
                {
                    vl1 = 0;
                    dsc = 2;
                }

                v = 0;
                i = 0;
                j = 1;
                while (i < 9)
                {
                    v += Convert.ToInt32(cnh[i].ToString()) * j;
                    i += 1;
                    j += 1;
                }

                int x = v % 11;
                int vl2 = x >= 10 ? 0 : x - dsc;
                isValid = (vl1.ToString() + vl2.ToString() ?? "") == (cnh.Substring(cnh.Length - 2, 2) ?? "");
            }

            return isValid;
        }

        /// <summary>
        /// Verifica se a string é um CPF ou CNPJ válido
        /// </summary>
        /// <param name="Text">CPF ou CNPJ</param>
        /// <returns></returns>
        public static bool IsValidCPFOrCNPJ(this string Text)
        {
            return Text.IsValidCPF() || Text.IsValidCNPJ();
        }

        /// <summary>
        /// Verifica se a string é um CPF válido
        /// </summary>
        /// <param name="Text">CPF</param>
        /// <returns></returns>
        public static bool IsValidCPF(this string Text)
        {
            try
            {
                Text = Text.RemoveAny(".", "-");
                string digito = "";
                int k;
                int j;
                int soma;
                for (k = 0; k <= 1; k++)
                {
                    soma = 0;
                    var loopTo = 9 + (k - 1);
                    for (j = 0; j <= loopTo; j++)
                        soma += int.Parse(Text[j].ToString()) * (10 + k - j);
                    digito += (soma % 11 == 0 || soma % 11 == 1 ? 0 : 11 - soma % 11).ToString();
                }

                return digito[0] == Text[9] & digito[1] == Text[10];
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se uma string é um cep válido
        /// </summary>
        /// <param name="CEP"></param>
        /// <returns></returns>
        public static bool IsValidCEP(this string CEP)
        {
            return new Regex(@"^\d{5}-\d{3}$").IsMatch(CEP) || CEP.RemoveAny("-").IsNumber() && CEP.RemoveAny("-").Length == 8;
        }

        /// <summary>
        /// Verifica se a string é um CNPJ válido
        /// </summary>
        /// <param name="Text">CPF</param>
        /// <returns></returns>
        public static bool IsValidCNPJ(this string Text)
        {
            try
            {
                var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int soma;
                int resto;
                string digito;
                string tempCnpj;
                Text = Text.Trim();
                Text = Text.Replace(".", "").Replace("-", "").Replace("/", "");
                if (Text.Length != 14)
                    return false;
                tempCnpj = Text.Substring(0, 12);
                soma = 0;
                for (int i = 0; i <= 12 - 1; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
                resto = soma % 11;
                if (resto < 2)
                {
                    resto = 0;
                }
                else
                {
                    resto = 11 - resto;
                }

                digito = resto.ToString();
                tempCnpj = tempCnpj + digito;
                soma = 0;
                for (int i = 0; i <= 13 - 1; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
                resto = soma % 11;
                if (resto < 2)
                {
                    resto = 0;
                }
                else
                {
                    resto = 11 - resto;
                }

                digito = digito + resto.ToString();
                return Text.EndsWith(digito);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se uma string é um caminho de arquivo válido
        /// </summary>
        /// <param name="Text">Texto</param>
        /// <returns>TRUE se o caminho for válido</returns>
        public static bool IsFilePath(this string Text)
        {
            Text = Text.Trim();
            try
            {
                if (Directory.Exists(Text))
                {
                    return false;
                }

                if (File.Exists(Text))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
            }

            // if has extension then its a file; directory otherwise
            return !Text.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar)) & Path.GetExtension(Text).IsNotBlank();
        }

        /// <summary>
        /// Verifica se uma string é um caminho de diretório válido
        /// </summary>
        /// <param name="Text">Texto</param>
        /// <returns>TRUE se o caminho for válido</returns>
        public static bool IsDirectoryPath(this string Text)
        {
            Text = Text.Trim();
            try
            {
                if (Directory.Exists(Text))
                {
                    return true;
                }

                if (File.Exists(Text))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
            }

            // if has trailing slash then it's a directory
            if (new string[] { Conversions.ToString(Path.DirectorySeparatorChar), Conversions.ToString(Path.AltDirectorySeparatorChar) }.Any(x => Text.EndsWith(x)))
            {
                return true;
            }
            // ends with slash if has extension then its a file; directory otherwise
            return string.IsNullOrWhiteSpace(Path.GetExtension(Text));
        }

        /// <summary>
        /// Verifica se uma string é um caminho de diretóio válido
        /// </summary>
        /// <param name="Text">Texto</param>
        /// <returns>TRUE se o caminho for válido</returns>
        public static bool IsPath(this string Text)
        {
            return Text.IsDirectoryPath() | Text.IsFilePath();
        }

        /// <summary>
        /// Verifica se a string é um endereço IP válido
        /// </summary>
        /// <param name="IP">Endereco IP</param>
        /// <returns>TRUE ou FALSE</returns>
        public static bool IsIP(this string IP)
        {
            return Regex.IsMatch(IP, @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b");
        }

        /// <summary>
        /// Valida se a string é um telefone
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static bool IsTelephone(this string Text)
        {
            return new Regex(@"\(?\+[0-9]{1,3}\)? ?-?[0-9]{1,3} ?-?[0-9]{3,5} ?-?[0-9]{4}( ?-?[0-9]{3})? ?(\w{1,10}\s?\d{1,6})?", (RegexOptions)((int)RegexOptions.Singleline + (int)RegexOptions.IgnoreCase)).IsMatch(Text.RemoveAny("(", ")"));
        }

        /// <summary>
        /// Verifica se o arquivo está em uso por outro procedimento
        /// </summary>
        /// <param name="File">o Arquivo a ser verificado</param>
        /// <returns>TRUE se o arquivo estiver em uso, FALSE se não estiver</returns>

        public static bool IsInUse(this FileInfo File)
        {
            try
            {
                File.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException ex)
            {
                return true;
            }
        }

        /// <summary>
        /// Verifica se o valor é um numero
        /// </summary>
        /// <param name="Value">Valor a ser verificado, pode ser qualquer objeto</param>
        /// <returns>TRUE se for um numero, FALSE se não for um numero</returns>
        public static bool IsNumber(this object Value)
        {
            try
            {
                Convert.ToDecimal(Value);
                return !Value.ToString().IsIP() & !(Value.GetType() == typeof(DateTime));
            }
            catch
            {
                return false;
            }
        }




        /// <summary>
        /// Verifica se o valor é um numero ou pode ser convertido em numero
        /// </summary>
        /// <param name="Value">Valor a ser verificado, pode ser qualquer objeto</param>
        /// <returns>TRUE se for um numero, FALSE se não for um numero</returns>
        public static bool CanBeNumber(this object Value)
        {
            try
            {
                Convert.ToDecimal(Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se o valor não é um numero
        /// </summary>
        /// <param name="Value">Valor a ser verificado, pode ser qualquer objeto</param>
        /// <returns>FALSE se for um numero, TRUE se não for um numero</returns>
        public static bool IsNotNumber(this object Value)
        {
            return !Value.IsNumber();
        }

        /// <summary>
        /// Verifica se um determinado texto é um email
        /// </summary>
        /// <param name="Text">Texto a ser validado</param>
        /// <returns>TRUE se for um email, FALSE se não for email</returns>

        public static bool IsEmail(this string Text)
        {
            var emailExpression = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
            return Text.IsNotBlank() && emailExpression.IsMatch(Text);
        }

        /// <summary>
        /// Verifica se um determinado texto é uma URL válida
        /// </summary>
        /// <param name="Text">Texto a ser verificado</param>
        /// <returns>TRUE se for uma URL, FALSE se não for uma URL válida</returns>

        public static bool IsURL(this string Text)
        {
            Uri u;
            return Uri.TryCreate(Text, UriKind.Absolute, out u) && !Text.Contains(" ");
        }

        /// <summary>
        /// Verifica se o dominio é válido (existe) em uma URL ou email
        /// </summary>
        /// <param name="DomainOrEmail">Uma String contendo a URL ou email</param>
        /// <returns>TRUE se o dominio existir, FALSE se o dominio não existir</returns>
        /// <remarks>Retornara sempre false quando nao houver conexao com a internet</remarks>

        public static bool IsValidDomain(this string DomainOrEmail)
        {
            System.Net.IPHostEntry ObjHost;
            if (DomainOrEmail.IsEmail() == true)
            {
                DomainOrEmail = "Http://" + DomainOrEmail.GetAfter("@");
            }

            try
            {
                string HostName = new Uri(DomainOrEmail).Host;
                ObjHost = System.Net.Dns.GetHostEntry(HostName);
                return (ObjHost.HostName ?? "") == (HostName ?? "");
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Tenta retornar um index de um IEnumerable a partir de um valor especifico. retorna -1 se o index nao existir
        /// </summary>
        /// <typeparam name="T">Tipo do IEnumerable e do valor</typeparam>
        /// <param name="Arr">Array</param>
        /// <returns></returns>
        public static int GetIndexOf<T>(this IEnumerable<T> Arr, T item)
        {
            try
            {
                return Arr.ToList().IndexOf(item);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Tenta retornar um valor de um IEnumerable a partir de um Index especifico. retorna um valor default se o index nao existir
        /// </summary>
        /// <typeparam name="T">Tipo do IEnumerable e do valor</typeparam>
        /// <param name="Arr">Array</param>
        /// <param name="Index">Posicao</param>
        /// <param name="ValueIfNoIndex">Valor se o mesmo nao existir</param>
        /// <returns></returns>
        public static T IfNoIndex<T>(this IEnumerable<T> Arr, int Index, T ValueIfNoIndex = default)
        {
            try
            {
                return Arr.ElementAtOrDefault(Index);
            }
            catch (Exception ex)
            {
                return ValueIfNoIndex;
            }
        }

        /// <summary>
        /// Tenta retornar um valor de um IEnumerable a partir de um Index especifico. retorna um valor default se o index nao existir ou seu valor for branco ou nothing
        /// </summary>
        /// <typeparam name="T">Tipo do IEnumerable e do valor</typeparam>
        /// <param name="Arr">Array</param>
        /// <param name="Index">Posicao</param>
        /// <param name="ValueIfBlankOrNoIndex">Valor se o mesmo nao existir</param>
        /// <returns></returns>
        public static T IfBlankOrNoIndex<T>(this IEnumerable<T> Arr, int Index, T ValueIfBlankOrNoIndex)
        {
            return (Arr ?? Array.Empty<T>()).IfNoIndex(Index).IfBlank(ValueIfBlankOrNoIndex);
        }

        /// <summary>
        /// Verifica se um aray está vazio ou nula e retorna um outro valor caso TRUE
        /// </summary>
        /// <typeparam name="T">Tipo da Variavel</typeparam>
        /// <param name="Value">       Valor</param>
        /// <param name="ValuesIfBlank">Valor se estiver em branco</param>
        /// <returns></returns>
        public static T[] IfNullOrEmpty<T>(this object[] Value, params T[] ValuesIfBlank)
        {
            if (Value is null || !Value.Any())
            {
                return ValuesIfBlank ?? Array.Empty<T>();
            }
            else
            {
                return Value.ChangeArrayType<T, object>();
            }
        }

        /// <summary>
        /// Verifica se um aray está vazio ou nula e retorna um outro valor caso TRUE
        /// </summary>
        /// <typeparam name="T">Tipo da Variavel</typeparam>
        /// <param name="Value">       Valor</param>
        /// <param name="ValuesIfBlank">Valor se estiver em branco</param>
        /// <returns></returns>
        public static IEnumerable<T> IfNullOrEmpty<T>(this IEnumerable<object[]> Value, params T[] ValuesIfBlank)
        {
            if (Value is null || Value.Count() == 0)
            {
                return ValuesIfBlank;
            }
            else
            {
                return Value.ChangeIEnumerableType<T, object[]>();
            }
        }

        /// <summary>
        /// Verifica se um aray está vazio ou nula e retorna um outro valor caso TRUE
        /// </summary>
        /// <typeparam name="T">Tipo da Variavel</typeparam>
        /// <param name="Value">       Valor</param>
        /// <param name="ValueIfBlank">Valor se estiver em branco</param>
        /// <returns></returns>
        public static IEnumerable<T> IfNullOrEmpty<T>(this IEnumerable<object[]> Value, IEnumerable<T> ValueIfBlank)
        {
            if (Value is null || Value.Count() == 0)
            {
                return ValueIfBlank;
            }
            else
            {
                return Value.ChangeIEnumerableType<T, object[]>();
            }
        }


        /// <summary>
        /// Verifica se uma variavel está vazia, em branco ou nula e retorna um outro valor caso TRUE
        /// </summary>
        /// <typeparam name="T">Tipo da Variavel</typeparam>
        /// <param name="Value">       Valor</param>
        /// <param name="ValueIfBlank">Valor se estiver em branco</param>
        /// <returns></returns>
        public static T IfBlank<T>(this object Value, T ValueIfBlank = default)
        {
            if (Value == null)
            {
                return ValueIfBlank;
            }
            else
            {
                bool blankas = false;
                if (Value.GetType() == typeof(string))
                {
                    blankas = Value.ToString().IsBlank();
                }
                else if (Value.GetType() == typeof(char))
                {
                    blankas = Value.ToString().IsBlank();
                }
                else if (Value.GetType() == typeof(long))
                {
                    blankas = ((long)Value) == 0;

                }
                else if (Value.GetType() == typeof(decimal))
                {
                    blankas = ((decimal)Value) == 0.00M;

                }
                else if (Value.GetType() == typeof(short))
                {
                    blankas = ((short)Value) == 0;

                }
                else if (Value.GetType() == typeof(double))
                {
                    blankas = ((double)Value) == 0d;
                }
                else if (Value.GetType() == typeof(int))
                {
                    blankas = ((int)Value) == 0d;
                }
                else if (Value.GetType() == typeof(char?))
                {
                    blankas = ((char?)Value).HasValue && ((char?)Value).ToString().IsBlank();
                }
                else if (Value.GetType() == typeof(bool?))
                {
                    blankas = ((bool?)Value).HasValue;
                }
                else if (Value.GetType() == typeof(long?))
                {
                    blankas = ((long?)Value).HasValue;
                }
                else if (Value.GetType() == typeof(decimal?))
                {
                    blankas = ((decimal?)Value).HasValue;
                }
                else if (Value.GetType() == typeof(short?))
                {
                    blankas = ((short?)Value).HasValue;
                }
                else if (Value.GetType() == typeof(double?))
                {
                    blankas = ((double?)Value).HasValue;
                }
                else if (Value.GetType() == typeof(int?))
                {
                    blankas = ((int?)Value).HasValue;

                }
                else if (Value.GetType() == typeof(DateTime?))
                {
                    blankas = ((DateTime?)Value).HasValue;
                }

                else if (Value.GetType() == typeof(DateTime))
                {
                    blankas = ((DateTime)Value).Equals(DateTime.MinValue);
                }

                else if (Value.GetType() == typeof(TimeSpan))
                {
                    blankas = ((TimeSpan)Value).Equals(TimeSpan.MinValue);
                }
                else
                {
                    blankas = Value.Equals(null);
                }

                return blankas ? ValueIfBlank : Value.ChangeType<T>();
            }
        }

        /// <summary>
        /// Anula o valor de um objeto se ele for igual a outro objeto
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <param name="TestExpression">Outro Objeto</param>
        /// <returns></returns>
        public static T NullIf<T>(this T Value, Func<T, bool> TestExpression)
        {
            if (TestExpression(Value))
            {
                return default;
            }

            return Value;
        }

        /// <summary>
        /// Anula o valor de um objeto se ele for igual a outro objeto
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <param name="TestValue">Outro Objeto</param>
        /// <returns></returns>
        public static T NullIf<T>(this T Value, T TestValue) where T : class
        {
            if (Value.Equals(TestValue))
                return null;
            return Value;
        }

        /// <summary>
        /// Anula o valor de um objeto se ele for igual a outro objeto
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <param name="TestValue">Outro Objeto</param>
        /// <returns></returns>
        public static T? NullIf<T>(this T? Value, T? TestValue) where T : struct
        {
            if (Value.HasValue)
            {
                if (Value.Equals(TestValue))
                {
                    Value = default;
                }
            }

            return Value;
        }

        /// <summary>
        /// Anula o valor de uma string se ela for igual a outra string
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <param name="TestValue">Outro Objeto</param>
        /// <returns></returns>
        public static string NullIf(this string Value, string TestValue, StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            if (Value is null || Value.Equals(TestValue, ComparisonType))
                return null;
            return Value;
        }

        public static bool IsDate(this string Obj)
        {
            try
            {
                DateTime d;
                return DateTime.TryParse(Obj, out d);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool IsDate<T>(this T Obj)
        {
            return ReferenceEquals(ClassTools.GetNullableTypeOf(Obj), typeof(DateTime)) || Obj?.ToString().IsDate() == true;
        }

        public static bool IsBoolean<T>(this T Obj)
        {
            return ReferenceEquals(ClassTools.GetNullableTypeOf(Obj), typeof(bool)) || Obj?.ToString().ToLower().IsIn("true", "false") == true;
        }

        public static bool IsArray<T>(T Obj)
        {
            try
            {
                var ValueType = Obj.GetType();
                return !ReferenceEquals(ValueType, typeof(string)) && ValueType.IsArray; // AndAlso GetType(T).IsAssignableFrom(ValueType.GetElementType())
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        /// <summary>
        /// Verifica se uma String está em branco
        /// </summary>
        /// <param name="Text">Uma string</param>
        /// <returns>TRUE se estivar vazia ou em branco, caso contrario FALSE</returns>
        public static bool IsBlank(this string Text)
        {
            return string.IsNullOrEmpty(Text) || string.IsNullOrWhiteSpace(Text.RemoveAny(Environment.NewLine));
        }

        /// <summary>
        /// Verifica se uma String está em branco
        /// </summary>
        /// <param name="Text">Uma string</param>
        /// <returns>TRUE se estivar vazia ou em branco, caso contrario FALSE</returns>
        public static bool IsBlank(this FormattableString Text)
        {
            return Information.IsNothing(Text) || Text.ToString().IsBlank();
        }

        /// <summary>
        /// Verifica se uma String não está em branco
        /// </summary>
        /// <param name="Text">Uma string</param>
        /// <returns>FALSE se estivar vazia ou em branco, caso contrario TRUE</returns>
        public static bool IsNotBlank(this string Text)
        {
            return !Text.IsBlank();
        }

        public static bool IsNotBlank(this FormattableString Text)
        {
            return !Information.IsNothing(Text) && Text.ToString().IsNotBlank();
        }

        /// <summary>
        /// Verifica se um numero é par
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsEven(this decimal Value)
        {
            return Value % 2m == 0m;
        }

        /// <summary>
        /// Verifica se um numero é par
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsEven(this int Value)
        {
            return Value.ChangeType<decimal, int>().IsEven();
        }

        /// <summary>
        /// Verifica se um numero é par
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsEven(this long Value)
        {
            return Value.ChangeType<decimal, long>().IsEven();
        }

        /// <summary>
        /// Verifica se um numero é par
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsEven(this double Value)
        {
            return Value.ChangeType<decimal, double>().IsEven();
        }

        /// <summary>
        /// Verifica se um numero é impar
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsOdd(this decimal Value)
        {
            return !Value.IsEven();
        }

        /// <summary>
        /// Verifica se um numero é impar
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsOdd(this int Value)
        {
            return !Value.IsEven();
        }

        /// <summary>
        /// Verifica se um numero é impar
        /// </summary>
        /// <param name="Value">Valor</param>
        /// <returns></returns>
        public static bool IsOdd(this long Value)
        {
            return !Value.IsEven();
        }
    }
}