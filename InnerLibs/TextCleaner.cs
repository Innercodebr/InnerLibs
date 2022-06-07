﻿using InnerLibs.LINQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InnerLibs
{
    public class Paragraph : List<Sentence>
    {
        internal Paragraph(string Text, TextStructure StructuredText)
        {
            this.StructuredText = StructuredText;
            if (Text.IsNotBlank())
            {
                char sep0 = '.';
                char sep1 = '!';
                char sep2 = '?';
                string pattern = string.Format("[{0}{1}{2}]|[^{0}{1}{2}]+", sep0, sep1, sep2);
                var regex = new Regex(pattern);
                var matches = regex.Matches(Text);
                foreach (Match match in matches)
                {
                    Add(new Sentence(match.ToString(), this));
                }
            }
        }

        public TextStructure StructuredText { get; set; }

        public int WordCount => Words.Count();

        public IEnumerable<string> Words => this.SelectMany(x => x.Words);

        public static implicit operator string(Paragraph paragraph) => paragraph.ToString();

        public override string ToString() => ToString(0);

        public string ToString(int Ident)
        {
            string ss = "";
            foreach (var s in this)
            {
                ss += s.ToString() + " ";
            }

            //ss = ss.TrimBetween();
            return ss.PadLeft(ss.Length + Ident);
        }
    }

    /// <summary>
    /// Sentença de um texto (uma frase ou oração)
    /// </summary>
    public class Sentence : List<SentencePart>
    {
        internal Sentence(string Text, Paragraph Paragraph)
        {
            this.Paragraph = Paragraph;
            if (Text.IsNotBlank())
            {
                var charlist = Text.Trim().ToArray().ToList();
                string palavra = "";
                var listabase = new List<string>();

                // remove quaisquer caracteres nao desejados do inicio da frase
                while (charlist.Count > 0 && charlist.First().ToString().IsIn(PredefinedArrays.EndOfSentencePunctuation))
                {
                    charlist.Remove(charlist.First());
                }

                // processa caractere a caractere
                foreach (var p in charlist)
                {

                    switch (true)
                    {
                        // caso for algum tipo de pontuacao, wrapper ou virgula
                        case object _ when PredefinedArrays.OpenWrappers.Contains(Convert.ToString(p)):
                        case object _ when PredefinedArrays.CloseWrappers.Contains(Convert.ToString(p)):
                        case object _ when PredefinedArrays.EndOfSentencePunctuation.Contains(Convert.ToString(p)):
                        case object _ when PredefinedArrays.MidSentencePunctuation.Contains(Convert.ToString(p)):
                            {
                                if (palavra.IsNotBlank())
                                {
                                    listabase.Add(palavra); // adiciona a plavra atual
                                    palavra = "";
                                }

                                listabase.Add(Convert.ToString(p)); // adiciona a virgula, wrapper ou pontuacao
                                break;
                            }
                        // caso for espaco
                        case object _ when Convert.ToString(p) == " ":
                            {
                                if (palavra.IsNotBlank())
                                {
                                    listabase.Add(palavra); // adiciona a plavra atual
                                    palavra = "";
                                }
                                // senao, adiciona o proximo caractere a palavra atual
                                palavra = "";
                                break;
                            }

                        default:
                            {
                                palavra += Convert.ToString(p);
                                break;
                            }
                    }
                }

                // e entao adiciona ultima palavra se existir
                if (palavra.IsNotBlank())
                {
                    listabase.Add(palavra);
                    palavra = "";
                }

                if (listabase.Count > 0)
                {
                    if (listabase.Last() == ",") // se a ultima sentenca for uma virgula, substituimos ela por ponto
                    {
                        listabase.RemoveAt(listabase.Count - 1);
                        listabase.Add(".");
                    }

                    // se a ultima sentecao nao for nenhum tipo de pontuacao, adicionamos um ponto a ela
                    if (!listabase.Last().IsInAny(new[] { PredefinedArrays.EndOfSentencePunctuation, PredefinedArrays.MidSentencePunctuation }))
                    {
                        listabase.Add(".");
                    }

                    // processar palavra a palavra
                    for (int index = 0, loopTo = listabase.Count - 1; index <= loopTo; index++)
                    {
                        if (listabase[index].IsNotBlank())
                        {
                            Add(new SentencePart(listabase[index], this));
                        }
                    }
                }
                else
                {
                    this.Paragraph.Remove(this);
                }
            }
        }

        public Paragraph Paragraph { get; private set; }

        public int WordCount => Words.Count();

        public IEnumerable<string> Words => this.Where(x => x.IsWord).Select(x => x.Text);

        public static implicit operator string(Sentence sentence) => sentence.ToString();


        public override string ToString()
        {
            string sent = "";
            foreach (var s in this)
            {
                if (s.IsClosingQuote)
                {
                    sent += s.GetMatchQuote().ToString();
                }
                else
                {
                    sent += s.ToString();
                }

                if (s.NeedSpaceOnNext)
                {
                    sent += " ";
                }

            }

            return sent;
        }
    }

    /// <summary>
    /// Parte de uma sentença. Pode ser uma palavra, pontuaçao ou qualquer caractere de encapsulamento
    /// </summary>
    public class SentencePart
    {
        internal SentencePart(string Text, Sentence Sentence)
        {
            this.Text = Text.Trim();
            this.Sentence = Sentence;
        }

        public Sentence Sentence { get; private set; }

        /// <summary>
        /// Texto desta parte de sentença
        /// </summary>
        /// <returns></returns>
        public string Text { get; set; }

        public static implicit operator string(SentencePart sentencePart) => sentencePart.ToString();

        /// <summary>
        /// Retorna TRUE se esta parte de senteça for um caractere de fechamento de encapsulamento
        /// </summary>
        /// <returns></returns>
        public bool IsCloseWrapChar => PredefinedArrays.CloseWrappers.Contains(Text) && !IsOpeningQuote;

        /// <summary>
        /// Retorna TRUE se esta parte de sentença é uma vírgula
        /// </summary>
        /// <returns></returns>
        public bool IsComma => Text == ",";
        public bool IsQuote => IsSingleQuote || IsDoubleQuote;
        public bool IsSingleQuote => Text == "'";
        public bool IsDoubleQuote => Text == "\"";

        /// <summary>
        /// Retorna TRUE se esta parte de senteça for um caractere de encerramento de frase (pontuaçao)
        /// </summary>
        /// <returns></returns>
        public bool IsEndOfSentencePunctuation => PredefinedArrays.EndOfSentencePunctuation.Contains(Text);

        /// <summary>
        /// Retorna TRUE se esta parte de senteça for um caractere de de meio de sentença (dois
        /// pontos ou ponto e vírgula)
        /// </summary>
        /// <returns></returns>
        public bool IsMidSentencePunctuation => PredefinedArrays.MidSentencePunctuation.Contains(Text);

        /// <summary>
        /// Retorna TRUE se esta parte de senteça não for uma palavra
        /// </summary>
        /// <returns></returns>
        public bool IsNotWord => IsOpenWrapChar || IsCloseWrapChar || IsComma || IsEndOfSentencePunctuation || IsMidSentencePunctuation || IsQuote;

        /// <summary>
        /// Retorna TRUE se esta parte de senteça for um caractere de abertura de encapsulamento
        /// </summary>
        /// <returns></returns>
        public bool IsOpenWrapChar => PredefinedArrays.OpenWrappers.Contains(Text) && !IsClosingQuote;

        /// <summary>
        /// Retorna TRUE se esta parte de senteça for qualquer tipo de pontuaçao
        /// </summary>
        /// <returns></returns>
        public bool IsPunctuation => IsEndOfSentencePunctuation || IsMidSentencePunctuation;

        /// <summary>
        /// Retorna TRUE se esta parte de senteça for uma palavra
        /// </summary>
        /// <returns></returns>
        public bool IsWord => !IsNotWord;

        /// <summary>
        /// Retorna true se é nescessário espaço andes da proxima sentença
        /// </summary>
        /// <returns></returns>
        public bool NeedSpaceOnNext => GetNextPart() != null && !IsOpeningQuote && !IsOpenWrapChar && !GetNextPart().IsClosingQuote && (IsClosingQuote || IsCloseWrapChar || GetNextPart().IsWord || GetNextPart().IsOpenWrapChar);

        /// <summary>
        /// Parte da próxima sentença
        /// </summary>
        /// <returns></returns>
        public SentencePart GetNextPart() => Sentence.IfNoIndex(Sentence.IndexOf(this) + 1);

        /// <summary>
        /// Parte de sentença anterior
        /// </summary>
        /// <returns></returns>
        public SentencePart GetPreviousPart() => Sentence.IfNoIndex(Sentence.IndexOf(this) - 1);

        public bool IsOpeningQuote => IsQuote && Sentence.Where(x => x.IsQuote).GetIndexOf(this).IsEven();
        public bool IsClosingQuote => IsQuote && Sentence.Where(x => x.IsQuote).GetIndexOf(this).IsOdd();

        public SentencePart GetMatchQuote()
        {
            var quotes = Sentence.Where(x => x.IsQuote).ToList();

            if (IsOpeningQuote)
            {
                return quotes.IfNoIndex(quotes.GetIndexOf(this) + 1);
            }

            if (IsClosingQuote)
            {
                return quotes.IfNoIndex(quotes.GetIndexOf(this) - 1);
            }

            return null;
        }


        public override string ToString()
        {
            int indexo = Sentence.IndexOf(this);
            if (indexo < 0)
            {
                return "";
            }

            if (indexo == 0 || indexo == 1 && PredefinedArrays.OpenWrappers.Contains(Sentence[0].Text))
            {
                return Text.ToProperCase();
            }

            return Text;
        }
    }

    /// <summary>
    /// Texto estruturado (Dividido em parágrafos)
    /// </summary>
    public class TextStructure : List<Paragraph>
    {
        private string _originaltext = "";

        /// <summary>
        /// Cria um novo texto estruturado (dividido em paragrafos, sentenças e palavras)
        /// </summary>
        /// <param name="OriginalText"></param>
        public TextStructure(string OriginalText)
        {
            Text = OriginalText;
        }



        public int BreakLinesBetweenParagraph { get; set; } = 0;
        public int Ident { get; set; } = 0;

        public string OriginalText => _originaltext;

        public string Text
        {
            get => ToString();

            set
            {
                _originaltext = value;
                Clear();
                if (OriginalText.IsNotBlank())
                {
                    foreach (var p in OriginalText.Split(PredefinedArrays.BreakLineChars.ToArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (p.IsNotBlank())
                        {
                            Add(new Paragraph(p, this));
                        }
                    }
                }
            }
        }

        public int WordCount => Words.Count();

        public IEnumerable<string> Words => this.SelectMany(x => x.Words);

        public static implicit operator int(TextStructure s) => s.Count;

        public static implicit operator long(TextStructure s) => s.LongCount();

        public static implicit operator string(TextStructure s) => s.ToString();

        public static TextStructure operator +(TextStructure a, TextStructure b) => new TextStructure($"{a}{Environment.NewLine}{b}");

        public Paragraph GetParagraph(int Index) => this.IfNoIndex(Index, null);

        public Sentence GetSentence(int Index) => GetSentences().IfNoIndex(Index, null);

        public IEnumerable<Sentence> GetSentences() => this.SelectMany(x => x.AsEnumerable());

        /// <summary>
        /// Retorna o texto corretamente formatado
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.SelectJoinString(parag => parag.ToString(Ident), Enumerable.Range(1, 1 + BreakLinesBetweenParagraph.SetMinValue(0)).SelectJoinString(x => Environment.NewLine));
    }
}