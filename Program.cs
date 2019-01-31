using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MemberListParser
{
    class Program
    {
        static readonly int TargetCount = 80;
        static readonly string InputPath = "members.pdf";
        static readonly string[] RegexPatterns = new[]
        {
            // 2018 Regex Patterns
            @"\n.*, \d{4} (?<name>.*) (?<age>\d{1,2})\n",
            @"\n.*, \d{4} (?<age>\d{1,2})\n(?<name>.*)\n"
        };
        static readonly int[] ageRanges = { 32, 47, 67 };
        static readonly Random NumGenerator = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("Parsing members from PDF...");
            List<(string name, int age)>[] membersByAge = ParseMembersFromPdf(InputPath);

            var totalMemberCount = membersByAge.Select(g => g.Count).Sum();
            Console.WriteLine($"Found {totalMemberCount} members");
            Console.WriteLine($"Randomly selecting {TargetCount} members evenly distributed among age groups");

            for (int i = 0; i < ageRanges.Length; i++)
            {
                SelectMembers($"Members <= {ageRanges[i]}", membersByAge[i].Select(t => t.name).ToList(), totalMemberCount);
            }

            SelectMembers($"Members > {ageRanges.Last()}", membersByAge.Last().Select(t => t.name).ToList(), totalMemberCount);

            Console.WriteLine();
            Console.WriteLine("- Done -");
        }

        private static List<(string name, int age)>[] ParseMembersFromPdf(string inputPath)
        {
            var membersByAge = new List<(string name, int age)>[ageRanges.Length + 1];
            for (int i = 0; i < ageRanges.Length + 1; i++)
            {
                membersByAge[i] = new List<(string, int)>();
            }

            var textBuilder = new StringBuilder();

            using (var reader = new PdfReader(inputPath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    textBuilder.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }
            }

            var text = textBuilder.ToString();

            foreach (var pattern in RegexPatterns)
            {
                var matches = Regex.Matches(text, pattern);
                foreach (Match match in Regex.Matches(text, pattern))
                {
                    var name = match.Groups["name"].Value;
                    var age = int.Parse(match.Groups["age"].Value);

                    if (string.IsNullOrEmpty(name) || age == 0)
                    {
                        throw new InvalidOperationException("Bad Data");
                    }

                    membersByAge[GetIndexFromAge(age)].Add((name, age));
                }
            }

            return membersByAge;
        }

        private static void SelectMembers(string title, IList<string> members, int totalMemberCount)
        {
            var numToSelect = TargetCount * members.Count / totalMemberCount;
            Console.WriteLine();
            Console.WriteLine("-------------------------");
            Console.WriteLine(title);
            Console.WriteLine($"Selecting {numToSelect} out of {members.Count}");
            Console.WriteLine("-------------------------");
            for (int i = 0; i < numToSelect; i++)
            {
                var index = NumGenerator.Next(members.Count);
                Console.WriteLine(members[index]);
                members.RemoveAt(index);
            }
        }

        private static int GetIndexFromAge(int age)
        {
            int index;
            for (index = 0; index < ageRanges.Length; index++)
            {
                if (age <= ageRanges[index])
                {
                    break;
                }
            }

            return index;
        }
    }
}
