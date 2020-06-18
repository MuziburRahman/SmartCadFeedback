using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCadFeedback.Extensions
{
    public struct StringMatchData
    {
        public readonly int MatchCount;
        public readonly float MatchPercentage;

        public StringMatchData(int matchCount, float matchPercentage)
        {
            MatchCount = matchCount;
            MatchPercentage = matchPercentage;
        }
    }

    public static class StringExtension
    {

        /// <summary>
        /// returns how accurately the shorter string matches with the longer string,
        /// keeping the characters of the shorter string in sequence as they are
        /// </summary>
        /// <param name="key">the keyword</param>
        /// <param name="target">reference</param>
        /// <returns> percentage of the accuracy of match </returns>
        public static StringMatchData SequenceMatch(this string key, string target)
        {
            /// 
            key = key.ToLower();
            target = target.ToLower();

            int a_len = key.Length;
            int b_len = target.Length;
            int match_idx = -1;
            int total_match = 0;

            for (int i = 0; i < b_len; i++)
            {
                char c = target[i];
                for (int j = match_idx == -1 ? 0 : match_idx + 1; j < a_len; j++)
                {
                    if (key[j] == c)
                    {
                        match_idx = j;
                        total_match++;
                        break;
                    }
                }

                if (match_idx + 1 >= a_len)
                    break;
            }

            return new StringMatchData(total_match, (float)total_match / b_len);
        }

    }
}
