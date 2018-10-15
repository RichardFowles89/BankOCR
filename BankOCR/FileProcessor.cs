using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BankOCR
{
    class FileProcessor
    {
        static IReadOnlyDictionary<string, char> numbers = new Dictionary<string, char>()
        {
            {
                " _ | ||_|",'0'
            },
            {
                "     |  |",'1'
            },
            {
                " _  _||_ ",'2'
            },
            {
                " _  _| _|",'3'
            },
            {
                "   |_|  |",'4'
            },
            {
                " _ |_  _|",'5'
            },
            {
                " _ |_ |_|",'6'
            },
            {
                " _   |  |",'7'
            },
            {
                " _ |_||_|",'8'
            },
            {
                " _ |_| _|",'9'
            }
        };

        private List<char[]> _data = new List<char[]>();
        private List<string> _accounts = new List<string>();
        private List<string> _processedAccounts = new List<string>();

        public void Read(string filePath)
        {
            foreach (var line in File.ReadLines(filePath))
            {
                _data.Add(Equalise(line.ToCharArray()));
            }
            Append();
        }

        private char[] Equalise(char[] line)
        {
            if (line.Length > 27)
                throw new ArgumentOutOfRangeException();

            int difference = 27 - line.Length;

            if (difference > 0)
            {
                char[] newLine = Enumerable.Repeat(' ', 27).ToArray();
                Array.Copy(line, newLine, line.Length);
                return newLine;
            }
            return line;
        }

        private void Append()
        {
            if (_data.Count % 4 != 0)
                throw new IndexOutOfRangeException();

            for (int i = 0; i < _data.Count - 1; i += 4)
            {
                StringBuilder sb = new StringBuilder();

                for (int j = 0; j < 27; j += 3)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(_data[i][j]);
                    stringBuilder.Append(_data[i][j + 1]);
                    stringBuilder.Append(_data[i][j + 2]);
                    stringBuilder.Append(_data[i + 1][j]);
                    stringBuilder.Append(_data[i + 1][j + 1]);
                    stringBuilder.Append(_data[i + 1][j + 2]);
                    stringBuilder.Append(_data[i + 2][j]);
                    stringBuilder.Append(_data[i + 2][j + 1]);
                    stringBuilder.Append(_data[i + 2][j + 2]);

                    sb.Append(stringBuilder.ToString());
                }
                _accounts.Add(sb.ToString());
            }
        }
        public void Process()
        {
            foreach (var line in _accounts)
            {
                StringBuilder sb = new StringBuilder();
                List<char> possibleNumbers = new List<char>();
                bool isNumeric = true;
                for (int i = 0; i < line.Length; i += 9)
                {
                    if (numbers.ContainsKey(line.Substring(i, 9)))
                        sb.Append(numbers[line.Substring(i, 9)]);
                    else
                    {
                        isNumeric = false;
                        possibleNumbers = CompareStringToKey(line.Substring(i, 9));
                        sb.Append("?");
                    }
                       

                }
                if (isNumeric && CheckSum(sb.ToString()))
                    _processedAccounts.Add(sb.ToString());

                if (isNumeric && !CheckSum(sb.ToString()))
                    _processedAccounts.Add(LegibilityChecker(sb.ToString()));

                if (!isNumeric)
                {
                    _processedAccounts.Add(CannotReadNumber(sb.ToString(), possibleNumbers));
                }
            }
        }

        private string CannotReadNumber(string account, List<char> possibleNumbers)
        {
            foreach (var number in possibleNumbers)
            {
                List<string> possibleAccounts = new List<string>();

                if (CheckSum(account.Replace('?', number)))
                {
                    possibleAccounts.Add(account.Replace('?', number));
                }
                if (possibleAccounts.Count == 1)
                    return possibleAccounts[0];
                if (possibleAccounts.Count > 1)
                    return AMBConcatenator(account, possibleAccounts);
            }
            return account + " ILL";
        }

        private string LegibilityChecker(string account)
        {
            List<string> possibleAccounts = new List<string>();
            if (CheckSum(account))
                return account;
            //look through each number in the account
            for (int i = 0; i < account.Length; i++)
            {//get the key for the number
                string key = numbers.FirstOrDefault(x => x.Value == account[i]).Key;

                List<char> possibleNumbers = CompareStringToKey(key);

                if (possibleNumbers.Count > 0)
                {
                    foreach (var number in possibleNumbers)
                    {
                        StringBuilder sb = new StringBuilder(account);
                        sb[i] = number;
                        if (CheckSum(sb.ToString()))
                            possibleAccounts.Add(sb.ToString());
                    }
                }
            }

            if (possibleAccounts.Count == 1)
                return possibleAccounts[0];
            if (possibleAccounts.Count > 1)
                return AMBConcatenator(account, possibleAccounts);
            return account + " ILL";
        }


        

        private string AMBConcatenator(string account, List<string> possibleAccounts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var value in possibleAccounts)
            {
                sb.Append("'" + value + "', ");
            }
            string concatAccounts = sb.ToString().Trim();
            concatAccounts = concatAccounts.Remove(concatAccounts.Length - 1);

            return account + " AMB [" + concatAccounts + "]";

        }

        private List<char> CompareStringToKey(string key)
        {
            List<char> possibleNumbers = new List<char>();

            foreach (var k in numbers.Keys)
            {
                int count = 0;
                for (int i = 0; i < k.Length; i++)
                {
                    if (k[i] != key[i])
                        count++;
                }
                if (count == 1)
                    possibleNumbers.Add(numbers[k]);
            }
            return possibleNumbers;
        }

        private bool CheckSum(string account)
        {
            int d9 = Int32.Parse(account[0].ToString());
            int d8 = Int32.Parse(account[1].ToString());
            int d7 = Int32.Parse(account[2].ToString());
            int d6 = Int32.Parse(account[3].ToString());
            int d5 = Int32.Parse(account[4].ToString());
            int d4 = Int32.Parse(account[5].ToString());
            int d3 = Int32.Parse(account[6].ToString());
            int d2 = Int32.Parse(account[7].ToString());
            int d1 = Int32.Parse(account[8].ToString());

            if (((1 * d1) + (2 * d2) + (3 * d3) + (4 * d4) + (5 * d5) +
                (6 * d6) + (7 * d7) + (8 * d8) + (9 * d9)) % 11 == 0)
            {
                return true;
            }
            return false;
        }
        public void Write(string path)
        {
            File.WriteAllLines(path, _processedAccounts);
        }
    }
}
