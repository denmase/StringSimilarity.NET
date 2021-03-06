/*
 * The MIT License
 *
 * Copyright 2016 feature[23]
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using F23.StringSimilarity.Interfaces;
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable LoopCanBeConvertedToQuery

namespace F23.StringSimilarity
{
    /// Ratcliff/Obershelp pattern recognition
    /// The Ratcliff/Obershelp algorithm computes the similarity of two strings a
    /// the doubled number of matching characters divided by the total number of
    /// characters in the two strings. Matching characters are those in the longest
    /// common subsequence plus, recursively, matching characters in the unmatched
    /// region on either side of the longest common subsequence.
    /// The Ratcliff/Obershelp distance is computed as 1 - Ratcliff/Obershelp similarity.
    ///
    /// @author Ligi https://github.com/dxpux (as a patch for fuzzystring)
    /// Adapted to StringSimilarity.Net by denmase https://github.com/denmase
    public class RatcliffObershelp : INormalizedStringSimilarity, INormalizedStringDistance
    {

        /// <summary>
        /// Compute Ratcliff/Obershelp similarity.
        /// </summary>
        /// <param name="s1">The first string to compare.</param>
        /// <param name="s2">The second string to compare.</param>
        /// <returns>The Ratcliff/Obershelp similarity in the range [0, 1]</returns>
        /// <exception cref="ArgumentNullException">If s1 or s2 is null.</exception>
        public double Similarity(string s1, string s2)
        {
            if (s1 == null)
            {
                throw new ArgumentNullException(nameof(s1));
            }

            if (s2 == null)
            {
                throw new ArgumentNullException(nameof(s2));
            }

            if (s1.Equals(s2))
            {
                return 1d;
            }

            List<string> matches = GetMatchList(s1, s2);

            int sum_of_matches = matches.Sum(match => match.Length);

            return 2.0d * sum_of_matches / (s1.Length + s2.Length);
        }

        /// <summary>
        /// Return 1 - similarity.
        /// </summary>
        /// <param name="s1">The first string to compare.</param>
        /// <param name="s2">The second string to compare.</param>
        /// <returns>1 - similarity</returns>
        /// <exception cref="ArgumentNullException">If s1 or s2 is null.</exception>
        public double Distance(string s1, string s2)
            => 1.0 - Similarity(s1, s2);

        private static List<string> GetMatchList(string s1, string s2)
        {
            List<string> list = new List<string>();
            string match = FrontMaxMatch(s1, s2);

            if (match.Length > 0) {
                string frontsource = s1.Substring(0, s1.IndexOf(match, StringComparison.CurrentCulture));
                string fronttarget = s2.Substring(0, s2.IndexOf(match, StringComparison.CurrentCulture));
                List<string> frontqueue = GetMatchList(frontsource, fronttarget);

                string endsource = s1.Substring(s1.IndexOf(match, StringComparison.CurrentCulture) + match.Length);
                string endtarget = s2.Substring(s2.IndexOf(match, StringComparison.CurrentCulture) + match.Length);
                List<string> endqueue = GetMatchList(endsource, endtarget);

                list.Add(match);
                list.AddRange(frontqueue);
                list.AddRange(endqueue);
            }

            return list;
        }

        private static string FrontMaxMatch(string s1, string s2)
        {
            int index = 0;
            int length = 0;

            for (int i = 0; i < s1.Length; i++) {
                var lengths = Enumerable.Range(1, s1.Length - i).ToList();
                foreach(var len in lengths) {
                    if (len > length && s2.Contains(s1.Substring(i, len))) {
                        index = i;
                        length = len;
                    }
                }
            }

            return s1.Substring(index, length);
        }
    }
}