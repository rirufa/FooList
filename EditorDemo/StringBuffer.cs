﻿/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
//#define TEST_ASYNC
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;
using Foo=FooProject.Collection;
using FooProject.Collection;

namespace FooEditEngine
{
    /// <summary>
    /// 更新タイプを表す列挙体
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// ドキュメントが置き換えられたことを表す
        /// </summary>
        Replace,
        /// <summary>
        /// ドキュメント全体が削除されたことを表す
        /// </summary>
        Clear,
        /// <summary>
        /// レイアウトが再構築されたことを表す
        /// </summary>
        RebuildLayout,
        /// <summary>
        /// レイアウトの構築が必要なことを示す
        /// </summary>
        BuildLayout,
    }

    /// <summary>
    /// 更新タイプを通知するためのイベントデータ
    /// </summary>
    public sealed class DocumentUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 値が指定されていないことを示す
        /// </summary>
        public const int EmptyValue = -1;
        /// <summary>
        /// 更新タイプ
        /// </summary>
        public UpdateType type;
        /// <summary>
        /// 開始位置
        /// </summary>
        public int startIndex;
        /// <summary>
        /// 削除された長さ
        /// </summary>
        public int removeLength;
        /// <summary>
        /// 追加された長さ
        /// </summary>
        public int insertLength;
        /// <summary>
        /// 更新イベントが発生した行。行が不明な場合や行をまたぐ場合はnullを指定すること。
        /// </summary>
        public int? row;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="type">更新タイプ</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="removeLength">削除された長さ</param>
        /// <param name="insertLength">追加された長さ</param>
        /// <param name="row">開始行。nullを指定することができる</param>
        public DocumentUpdateEventArgs(UpdateType type, int startIndex = EmptyValue, int removeLength = EmptyValue, int insertLength = EmptyValue, int? row = null)
        {
            this.type = type;
            this.startIndex = startIndex;
            this.removeLength = removeLength;
            this.insertLength = insertLength;
            this.row = row;
        }
    }

    public delegate void DocumentUpdateEventHandler(object sender, DocumentUpdateEventArgs e);

    /// <summary>
    /// ランダムアクセス可能な列挙子を提供するインターフェイス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRandomEnumrator<T>
    {
        /// <summary>
        /// インデクサーを表す
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>Tを返す</returns>
        T this[int index] { get; }
    }

    sealed class StringBuffer : IEnumerable<char>, IRandomEnumrator<char>
    {
        Foo.BigList<char> buf = new Foo.BigList<char>();

        internal DocumentUpdateEventHandler Update;

        public StringBuffer()
        {
            BigList<char>.BlockSize = 32768;
            this.Update = (s, e) => { };
        }

        public StringBuffer(StringBuffer buffer)
            : this()
        {
            buf.AddRange(buffer.buf);
        }


        public char this[int index]
        {
            get
            {
                char c = buf[index];
                return c;
            }
        }

        public string ToString(int index, int length)
        {
            StringBuilder temp = new StringBuilder();
            temp.Clear();
            for (int i = index; i < index + length; i++)
                temp.Append(buf[i]);
            return temp.ToString();
        }

        public int Length
        {
            get { return this.buf.Count; }
        }

        internal void Replace(Foo.BigList<char> buf)
        {
            this.Clear();
            this.buf = buf;

            this.Update(this, new DocumentUpdateEventArgs(UpdateType.Replace, 0, 0, buf.Count));
        }

        internal void Replace(int index, int length, IEnumerable<char> chars, int count)
        {
            if (length > 0)
                this.buf.RemoveRange(index, length);
            this.buf.InsertRange(index, chars);
            this.Update(this, new DocumentUpdateEventArgs(UpdateType.Replace, index, length, count));
        }

        internal void ReplaceAll(string target, string pattern, bool ci = false)
        {
            TextSearch ts = new TextSearch(target, ci);
            char[] pattern_chars = pattern.ToCharArray();
            int left = 0, right = this.buf.Count;
            while(right != -1)
            {
                while ((right = ts.IndexOf(this.buf, left, this.buf.Count - 1)) != -1)
                {
                    this.buf.RemoveRange(right, target.Length);
                    this.buf.InsertRange(right, pattern_chars);
                    left = right + pattern.Length;
                }
            }

        }

        internal int IndexOf(string target, int start, bool ci = false)
        {
            TextSearch ts = new TextSearch(target, ci);
            int patternIndex = ts.IndexOf(this.buf, start, this.buf.Count);
            return patternIndex;
        }

        /// <summary>
        /// 文字列を削除する
        /// </summary>
        internal void Clear()
        {
            this.buf.Clear();
            this.Update(this, new DocumentUpdateEventArgs(UpdateType.Clear, 0, this.buf.Count, 0));
        }

        internal IEnumerable<char> GetEnumerator(int start, int length)
        {
            for (int i = start; i < start + length; i++)
                yield return this.buf[i];
        }

        #region IEnumerable<char> メンバー

        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < this.Length; i++)
                yield return this.buf[i];
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return buf.GetEnumerator();
        }

        #endregion
    }

    sealed class TextSearch
    {
        char[] pattern;
        int patternLength;
        Dictionary<char, int> qsTable = new Dictionary<char, int>();
        bool caseInsenstive;
        public TextSearch(string pattern, bool ci = false)
        {
            this.patternLength = pattern.Length;
            this.caseInsenstive = ci;
            if (ci)
            {
                this.CreateQSTable(pattern.ToLower());
                this.CreateQSTable(pattern.ToUpper());
                this.pattern = new char[pattern.Length];
                for (int i = 0; i < pattern.Length; i++)
                    this.pattern[i] = CharTool.ToUpperFastIf(pattern[i]);
            }
            else
            {
                this.CreateQSTable(pattern);
                this.pattern = pattern.ToCharArray();
            }
        }
        void CreateQSTable(string pattern)
        {
            int len = pattern.Length;
            for (int i = 0; i < len; i++)
            {
                if (!this.qsTable.ContainsKey(pattern[i]))
                    this.qsTable.Add(pattern[i], len - i);
                else
                    this.qsTable[pattern[i]] = len - i;
            }
        }
        public int IndexOf(Foo.BigList<char> buf, int start, int end)
        {
            //QuickSearch法
            int buflen = buf.Count - 1;
            int plen = this.patternLength;
            int i = start;
            int search_end = end - plen;
            //最適化のためわざとコピペした
            if (this.caseInsenstive)
            {
                while (i <= search_end)
                {
                    int j = 0;
                    while (j < plen)
                    {
                        if (CharTool.ToUpperFastIf(buf[i + j]) != this.pattern[j])
                            break;
                        j++;
                    }
                    if (j == plen)
                    {
                        return i;
                    }
                    else
                    {
                        int k = i + plen;
                        if (k <= buflen)	//buffer以降にアクセスする可能性がある
                        {
                            int moveDelta;
                            if (this.qsTable.TryGetValue(buf[k], out moveDelta))
                                i += moveDelta;
                            else
                                i += plen;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }
            else
            {
                while (i <= search_end)
                {
                    int j = 0;
                    while (j < plen)
                    {
                        if (buf[i + j] != this.pattern[j])
                            break;
                        j++;
                    }
                    if (j == plen)
                    {
                        return i;
                    }
                    else
                    {
                        int k = i + plen;
                        if (k <= buflen)	//buffer以降にアクセスする可能性がある
                        {
                            int moveDelta;
                            if (this.qsTable.TryGetValue(buf[k], out moveDelta))
                                i += moveDelta;
                            else
                                i += plen;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return -1;
        }
    }
    static class CharTool
    {
        /// <summary>
        /// Converts characters to lowercase.
        /// </summary>
        const string _lookupStringL =
        "---------------------------------!-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";

        /// <summary>
        /// Converts characters to uppercase.
        /// </summary>
        const string _lookupStringU =
        "---------------------------------!-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";

        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        public static char ToLower(char c)
        {
            return _lookupStringL[c];
        }

        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        public static char ToUpper(char c)
        {
            return _lookupStringU[c];
        }

        /// <summary>
        /// Translate uppercase ASCII characters to lowercase.
        /// </summary>
        public static char ToLowerFastIf(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return (char)(c + 32);
            }
            else
            {
                return c;
            }
        }

        /// <summary>
        /// Translate lowercase ASCII characters to uppercase.
        /// </summary>
        public static char ToUpperFastIf(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return (char)(c - 32);
            }
            else
            {
                return c;
            }
        }
    }
}