using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ngin
{
    public class LexiconConverter
    {
        public static Lexicon ReadFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                Lexicon rootLexicon = new Lexicon();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath))
                {
                    Dictionary<int, Lexicon> binStack = new Dictionary<int, Lexicon> { [-1] = rootLexicon };
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                        int indent = line.TakeWhile(char.IsWhiteSpace).Count() / 4;
                        line = line.Trim();
                        int delimiterPos = line.IndexOf(':');
                        if (delimiterPos == -1) continue;

                        string key = line.Substring(0, delimiterPos);
                        string value = delimiterPos < line.Length - 1 ? line.Substring(delimiterPos + 1).Trim() : string.Empty;

                        if (string.IsNullOrEmpty(value))
                        {
                            binStack[indent] = ParseLexicon(binStack[indent - 1], key);
                        }
                        else
                        {
                            ParseLexiconElement(binStack[indent - 1], key, value);
                        }
                    }
                }
                return rootLexicon;
            }
            else
            {
                Debug.Log($"File not found: {filePath}");
                return null;
            }
        }
        public static Lexicon Read(TextAsset textAsset)
        {
            if (textAsset == null) {
                Debug.Log("Lexicon snorri converter reading text failed");
                return null;
            }
            //Debug.Log("Lexicon snorri converter reading text: \n" + textAsset.text);
            Lexicon rootLexicon = new Lexicon();
            using (System.IO.StringReader reader = new System.IO.StringReader(textAsset.text))
            {
                Dictionary<int, Lexicon> binStack = new Dictionary<int, Lexicon> { [-1] = rootLexicon };
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                    int indent = line.TakeWhile(char.IsWhiteSpace).Count() / 4;
                    line = line.Trim();
                    int delimiterPos = line.IndexOf(':');
                    if (delimiterPos == -1) continue;

                    string key = line.Substring(0, delimiterPos);
                    string value = delimiterPos < line.Length - 1 ? line.Substring(delimiterPos + 1).Trim() : string.Empty;

                    if (string.IsNullOrEmpty(value))
                    {
                        binStack[indent] = ParseLexicon(binStack[indent - 1], key);
                    }
                    else
                    {
                        ParseLexiconElement(binStack[indent - 1], key, value);
                    }
                }
            }
            return rootLexicon;
        }
        public static Lexicon ReadFromString(string text)
        {
            // Normalize line endings to prevent issues with different newline characters
            string normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");

            Lexicon rootLexicon = new Lexicon();
            using (System.IO.StringReader reader = new System.IO.StringReader(normalizedText))
            {
                // Stack to manage the hierarchy of bins based on indentation
                Dictionary<int, Lexicon> binStack = new Dictionary<int, Lexicon> { [-1] = rootLexicon };
                int currentIndent = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                    // Count leading spaces for indent
                    int indent = line.TakeWhile(c => c == ' ').Count() / 4;

                    // Trim leading and trailing whitespace from the line
                    line = line.Trim();

                    // Determine if the current line has a new or nested key-value pair
                    int delimiterPos = line.IndexOf(':');
                    if (delimiterPos == -1) continue;

                    string key = line.Substring(0, delimiterPos);
                    string value = delimiterPos < line.Length - 1 ? line.Substring(delimiterPos + 1).Trim() : string.Empty;

                    if (indent > currentIndent)
                    {
                        // New nested bin, so we need to update the bin stack
                        currentIndent = indent;
                    }
                    else if (indent < currentIndent)
                    {
                        // Decrease the current indent level to manage the bin hierarchy
                        currentIndent = indent;
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        // If the value is empty, this is a new bin that needs to be added
                        binStack[indent] = ParseLexicon(binStack[indent - 1], key);
                    }
                    else
                    {
                        // Otherwise, it's a key-value pair that should be added to the current bin
                        ParseLexiconElement(binStack[indent - 1], key, value);
                    }
                }
            }
            return rootLexicon;
        }
        public static void Write(Lexicon bin, string filePath)
        {
            // Ensure the directory exists and create it if necessary
            string directoryPath = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            // Create the file if it doesn't exist and write the data
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, false))
            {
                writer.Write(GetString(bin));
            }
        }


        public static string GetString(Lexicon bin, int indent = 0)
        {
            var result = string.Empty;
            foreach (var pair in bin)
            {
                result += new string(' ', indent) + pair.Key + ": ";
                switch (pair.Value)
                {
                    case Lexicon subLexicon:
                        result += "\n" + GetString(subLexicon, indent + 4);
                        break;
                    case List<int> intList:
                        result += "[" + string.Join(", ", intList) + "]\n";
                        break;
                    case List<float> floatList:
                        result += "[" + string.Join(", ", floatList) + "]\n";
                        break;
                    case List<string> stringList:
                        result += "[" + string.Join(", ", stringList.Select(s => $"\"{s}\"")) + "]\n";
                        break;
                    case List<bool> boolList:
                        result += "[" + string.Join(", ", boolList.Select(b => b ? "true" : "false")) + "]\n";
                        break;
                    case Vector3Int vector3Int:
                        result += $"[{vector3Int.x}, {vector3Int.y}, {vector3Int.z}]\n";
                        break;
                    case Vector2Int vector2Int:
                        result += $"[{vector2Int.x}, {vector2Int.y}]\n";
                        break;
                    case Vector3 vector3:
                        result += $"[{vector3.x}, {vector3.y}, {vector3.z}]\n";
                        break;
                    case Vector2 vector2:
                        result += $"[{vector2.x}, {vector2.y}]\n";
                        break;
                    case int intValue:
                        result += intValue + "\n";
                        break;
                    case float floatValue:
                        result += floatValue + "\n";
                        break;
                    case string strValue:
                        result += $"\"{strValue}\"\n";
                        break;
                    case bool boolValue:
                        result += boolValue ? "true\n" : "false\n";
                        break;
                    default:
                        result += "null\n";
                        break;
                }
            }
            return result;
        }


        private static void ParseLexiconElement(Lexicon bin, string key, string value)
        {
            bin.Set(key, ParseValue(value));
        }

        private static Lexicon ParseLexicon(Lexicon parentLexicon, string collectionName)
        {
            Lexicon newLexicon = new Lexicon(collectionName);
            parentLexicon.Set(collectionName, newLexicon);
            return newLexicon;
        }

        public static object ParseValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                var elements = value.Substring(1, value.Length - 2).Split(',').Select(s => s.Trim()).ToList();


                if (elements.All(e => int.TryParse(e, out _)))
                {
                    var intList = elements.Select(int.Parse).ToList();

                    return new List<int>(intList);
                }

                if (elements.All(e => float.TryParse(e, out _)))
                {
                    var floatList = elements.Select(float.Parse).ToList();

                    return new List<float>(floatList);
                }



                if (elements.All(e => e == "true" || e == "false"))
                    return new List<bool>(elements.Select(e => e == "true").ToList());

                return new List<string>(elements.Select(e => e.Trim('"')));
            }

            if (int.TryParse(value, out int intValue))
                return intValue;

            if (float.TryParse(value, out float floatValue))
                return floatValue;

            if (bool.TryParse(value, out bool boolValue))
                return boolValue;

            return value.Trim('"');
        }

    }
}
