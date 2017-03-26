// <copyright file="ParsingHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static class ParsingHelper
    {
        public static IEnumerable<string[]> ParseCsv(TextReader textReader)
        {
            EFGuard.NotNull(textReader, nameof(textReader));

            bool inQuote = false;
            var stringBuffer = new StringBuilder();
            var rowBuilder = new List<string>();

            Action addItem = () =>
            {
                rowBuilder.Add(stringBuffer.ToString());
                stringBuffer.Clear();
            };

            Func<string[]> produceRow = () =>
            {
                addItem();

                var row = rowBuilder.ToArray();
                rowBuilder.Clear();

                return row;
            };

            int currentRead;

            while ((currentRead = textReader.Read()) >= 0)
            {
                char currentChar = (char)currentRead;

                if (currentChar == '"')
                {
                    if (textReader.Peek() == '"')
                    {
                        textReader.Read();
                        stringBuffer.Append('"');
                    }
                    else
                    {
                        inQuote = !inQuote;
                    }
                }
                else if (inQuote)
                {
                    stringBuffer.Append(currentChar);
                }
                else
                {
                    switch (currentChar)
                    {
                        case '\r':
                            if (textReader.Peek() == '\n')
                            {
                                textReader.Read();
                            }

                            yield return produceRow();
                            break;

                        case '\n':
                            if (textReader.Peek() == '\r')
                            {
                                textReader.Read();
                            }

                            yield return produceRow();
                            break;

                        case ',':
                            addItem();
                            break;

                        default:
                            stringBuffer.Append(currentChar);
                            break;
                    }
                }
            }

            if (inQuote)
            {
                throw new ArgumentException("textReader's contents have an unmatched double-quote.", nameof(textReader));
            }

            if ((stringBuffer.Length != 0) || (rowBuilder.Count != 0))
            {
                yield return produceRow();
            }
        }

        public static IEnumerable<IDictionary<string, string>> ToDictionaries(this IEnumerable<string[]> parsedCsv)
        {
            if (parsedCsv == null)
            {
                throw new ArgumentNullException(nameof(parsedCsv));
            }

            parsedCsv = parsedCsv.Where(t => t != null);

            string[] firstRow = null;
            foreach (string[] row in parsedCsv)
            {
                if (firstRow == null)
                {
                    if (row.Any(t => ReferenceEquals(t, null)))
                    {
                        throw new ArgumentException(
                            "parsedCsv's first non-null item has an index that is a null reference.", 
                            nameof(parsedCsv));
                    }

                    firstRow = row;
                }
                else
                {
                    var currentItem = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                    for (int i = 0; (i < row.Length) && (i < firstRow.Length); ++i)
                    {
                        currentItem[firstRow[i]] = row[i];
                    }

                    yield return currentItem;
                }
            }

            if (firstRow == null)
            {
                throw new ArgumentException("parsedCsv has no header row item.", nameof(parsedCsv));
            }
        }

        public static IEnumerable<ExpandoObject> ToExpandoObjects(this IEnumerable<string[]> parsedCsv)
        {
            if (parsedCsv == null)
            {
                throw new ArgumentNullException(nameof(parsedCsv));
            }

            parsedCsv = parsedCsv.Where(t => t != null);

            string[] firstRow = null;
            foreach (string[] row in parsedCsv)
            {
                if (firstRow == null)
                {
                    if (row.Any(t => ReferenceEquals(t, null)))
                    {
                        throw new ArgumentException(
                            "parsedCsv's first non-null item has an index that is a null reference.",
                            nameof(parsedCsv));
                    }

                    firstRow = row;
                }
                else
                {
                    ExpandoObject currentItem = new ExpandoObject();
                    IDictionary<string, object> currentDictionary = currentItem;

                    for (int i = 0; (i < row.Length) && (i < firstRow.Length); ++i)
                    {
                        currentDictionary[firstRow[i]] = row[i];
                    }

                    yield return currentItem;
                }
            }

            if (firstRow == null)
            {
                throw new ArgumentException("parsedCsv has no header row item.", nameof(parsedCsv));
            }
        }
    }
}
