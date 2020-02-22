using Gimic.Extensions;
using Gimic.Interfaces;
using Gimic.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace GimicTests.Feature
{
    [TestFixture]
    public class GimicParserBehavior
    {
        #region Test Strings

        private const string EMPTY_TEST = "";
        private const string EMPTY_LINES_TEST = "\n\n\n\nKey:Value\n\n\n\n\nSecond:Pair\n\n\n\n";
        private const string LINES_WITH_NO_KEYS_TEST = "No key\nNo key\nKey:Value\nNo key\nNo key";
        private const string LINES_WITH_NO_VALUES_TEST = "No Value1:\nNo Value2:\nKey:Value\nNo Value3:\nNo Value4:";
        private const string EXTRA_COLONS_TEST = "Key:This:is:all:a:part:of:the:value.";
        private const string KEYS_AND_VALUES_PADDED_WITH_WHITESPACE_TEST = "So many spaces                  :        So many spaces                   ";
        private const string NESTED_MAPS_TEST = "Root:\n\tIn:Root\n\tLevel1:\n\t\tIn:Level1\n\t\tLevel2:\n\t\t\tIn:Level2\n\t\tBackIn:Level1\n\tBackIn:Root";
        private const string NESTED_WITH_EXTRA_TABS_TEST = "Root:\n\t\tIn:Root\n\t\tLevel1:\n\t\t\t\tIn:Level1\n\t\t\t\tLevel2:\n\t\t\t\t\t\tIn:Level2\n\tBackIn:Level1\nBackIn:Root";
        private const string LIST_VALUE_TEST = "Key:value,value,value,value";
        private const string ESCAPED_VALUE_TEST = @"Key:Some things in this value will be escaped``,`` and therefore ignored by the other parsing rules. For Example``:
    1. This
    2. Entire
    3. Enumerated
    4. List``";
        private const string LITTLE_TEST = "Key:Value";
        private const string BIG_TEST = @"Sentence: The quick brown fox ``jumped over ``the lazy ``dog.



The`` quick ``brown fox`` jumped over the lazy dog.
Lines without a key are not parsed.
Not even a little.
Empty Before Empty Field:
Empty Field:
Name: Jane Smith
Dependents: Jim Smith, Jenny Smith, Jared Smith
Empty Before Map:
Address:
    Empty Field At Begining Of Map:
    City: ``Omaha, NE``
    Street:
        Value with no key at begining of map.
        House Number: 1234
        Prefix: N 
        Name: Wonder
        Suffix: Rd.
        Empty Field At End Of Map:
    Apt: 87654
    Value with no key at end of map.
Phone Number: 555-1234
Date Joined: 2019/01/31T15:00:00-6
Days of vacation accrued: 14.5
Days of vacation used: 8
Administrative Metadata: ``{""username"":""smijan01"", ""org"":""Data Processing""}``
Empty Field At End of Document:
Just kidding, there's actually a value with no key at the end of the document.";

        #endregion Test Strings

        private static readonly Dictionary<string, Map<string, dynamic>> EXPECTED_RESULTS = new Dictionary<string, Map<string, dynamic>>
        {
            [EMPTY_TEST] = new Map<string, dynamic>(null),
            [EMPTY_LINES_TEST] = new Map<string, dynamic>(null) { ["Key"] = "Value", ["Second"] = "Pair" },
            [LINES_WITH_NO_KEYS_TEST] = new Map<string, dynamic>(null) { ["Key"] = "Value" },
            [LINES_WITH_NO_VALUES_TEST] = new Map<string, dynamic>(null)
            {
                ["No Value1"] = string.Empty,
                ["No Value2"] = string.Empty,
                ["Key"] = "Value",
                ["No Value3"] = string.Empty,
                ["No Value4"] = string.Empty
            },
            [EXTRA_COLONS_TEST] = new Map<string, dynamic>(null) { ["Key"] = "This:is:all:a:part:of:the:value." },
            [KEYS_AND_VALUES_PADDED_WITH_WHITESPACE_TEST] = new Map<string, dynamic>(null)
            {
                ["So many spaces"] = "So many spaces"
            },
            [NESTED_MAPS_TEST] = new Map<string, dynamic>(null)
            {
                ["Root"] = new Map<string, dynamic>(null)
                {
                    ["In"] = "Root",
                    ["Level1"] = new Map<string, dynamic>(null)
                    {
                        ["In"] = "Level1",
                        ["Level2"] = new Map<string, dynamic>(null)
                        {
                            ["In"] = "Level2"
                        },
                        ["BackIn"] = "Level1"
                    },
                    ["BackIn"] = "Root"
                }
            },
            [NESTED_WITH_EXTRA_TABS_TEST] = new Map<string, dynamic>(null)
            {
                ["Root"] = new Map<string, dynamic>(null)
                {
                    ["In"] = "Root",
                    ["Level1"] = new Map<string, dynamic>(null)
                    {
                        ["In"] = "Level1",
                        ["Level2"] = new Map<string, dynamic>(null)
                        {
                            ["In"] = "Level2"
                        },
                        ["BackIn"] = "Level1"
                    },
                    ["BackIn"] = "Root"
                }
            },
            [LIST_VALUE_TEST] = new Map<string, dynamic>(null)
            {
                ["Key"] = new List<string> { "value", "value", "value", "value" }
            },
            [ESCAPED_VALUE_TEST] = new Map<string, dynamic>(null)
            {
                ["Key"] = Tabify(@"Some things in this value will be escaped, and therefore ignored by the other parsing rules. For Example:
    1. This
    2. Entire
    3. Enumerated
    4. List")
            },
            [LITTLE_TEST] = new Map<string, dynamic>(null) { ["Key"] = "Value" },
            [BIG_TEST] = new Map<string, dynamic>(null)
            {
                ["Sentence"] = @"The quick brown fox jumped over the lazy dog.



The quick brown fox jumped over the lazy dog.",
                ["Empty Before Empty Field"] = string.Empty,
                ["Empty Field"] = string.Empty,
                ["Name"] = "Jane Smith",
                ["Dependents"] = new List<string> { "Jim Smith", "Jenny Smith", "Jared Smith" },
                ["Empty Before Map"] = string.Empty,
                ["Address"] = new Map<string, dynamic>(null)
                {
                    ["Empty Field At Begining Of Map"] = string.Empty,
                    ["City"] = "Omaha, NE",
                    ["Street"] = new Map<string, dynamic>(null)
                    {
                        ["House Number"] = "1234",
                        ["Prefix"] = "N",
                        ["Name"] = "Wonder",
                        ["Suffix"] = "Rd.",
                        ["Empty Field At End Of Map"] = string.Empty
                    },
                    ["Apt"] = "87654"
                },
                ["Phone Number"] = "555-1234",
                ["Date Joined"] = "2019/01/31T15:00:00-6",
                ["Days of vacation accrued"] = "14.5",
                ["Days of vacation used"] = "8",
                ["Administrative Metadata"] = "{\"username\":\"smijan01\", \"org\":\"Data Processing\"}",
                ["Empty Field At End of Document"] = string.Empty
            }
        };

        #region string formatting helpers

        private static string Tabify(string str)
        {
            return str.Replace("    ", "\t");
        }

        private static string Newlineify(string str)
        {
            return str.Replace("\r\n", "\n");
        }

        #endregion string formatting helpers

        [TestCase(EMPTY_TEST)]
        [TestCase(EMPTY_LINES_TEST)]
        [TestCase(LINES_WITH_NO_KEYS_TEST)]
        [TestCase(LINES_WITH_NO_VALUES_TEST)]
        [TestCase(EXTRA_COLONS_TEST)]
        [TestCase(KEYS_AND_VALUES_PADDED_WITH_WHITESPACE_TEST)]
        [TestCase(NESTED_MAPS_TEST)]
        [TestCase(NESTED_WITH_EXTRA_TABS_TEST)]
        [TestCase(LIST_VALUE_TEST)]
        [TestCase(ESCAPED_VALUE_TEST)]
        [TestCase(LITTLE_TEST)]
        [TestCase(BIG_TEST)]
        public void It_should_parse_a_newline_delimited_list_of_key_value_pairs(string source)
        {
            // ARRANGE
            var stream = Tabify(source).GenerateStreamFromString();

            // ACT
            var results = stream.ParseGimic();

            // ASSERT
            DeepAssert(EXPECTED_RESULTS[source], results);
        }

        /// <summary>
        /// Thoroughly compares the expected results to the actual parsed results.
        /// </summary>
        /// <param name="expectedResults"></param>
        /// <param name="results"></param>
        private void DeepAssert(IMap<string, dynamic> expectedResults, Dictionary<string, dynamic> results)
        {
            CollectionAssert.AreEquivalent(results.Keys, expectedResults.Keys);
            var enumerator = expectedResults.Values.GetEnumerator();
            foreach (var result in results)
            {
                Assert.True(enumerator.MoveNext(), "There were more results than expected!");
                var expected = enumerator.Current;
                if (result.Value is string)
                {
                    Assert.That(result.Value, Is.EqualTo(Newlineify(expected)));
                }

                if (result.Value is List<dynamic>)
                {
                    CollectionAssert.AreEquivalent(result.Value, expected);
                }

                if (result.Value is Map<string, dynamic>)
                {
                    DeepAssert(expectedResults[result.Key], result.Value);
                }
            }
        }
    }
}
