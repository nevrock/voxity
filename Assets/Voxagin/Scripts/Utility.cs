using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Ngin
{
    public static class Utility
    {
        public static bool IsHit(float probability) {
            // Ensure the probability is within a valid range
            if (probability < 0f || probability > 1f) {
                throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0 and 1.");
            }

            // Generate a random number between 0 and 1
            float randomValue = UnityEngine.Random.Range(0f, 1f);

            // Return true if the random value is less than or equal to the probability
            return randomValue <= probability;
        }

        public static Type GetType(string typeName)
        {
            Type componentType = Type.GetType(typeName);
            return componentType;
        }
        public static Type GetNginType(string typeString)
        {
            if (!typeString.Contains("Ngin."))
            {
                typeString = "Ngin." + typeString;                
            }
            Type componentType = Type.GetType(typeString);
            return componentType;
        }
        public static string ConvertStringToMethod(string typeString)
        {
            string[] words = typeString.Split("_");
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1).ToLower();
                }
            }
            return string.Concat(words);
        }
        public static string ConvertNginStringToUnity(string typeString)
        {
            string[] words = typeString.Split("_");
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1).ToLower();
                }
            }
            return string.Concat(words);
        }
        public static object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating instance of {type.Name} using default constructor: {ex.Message}");
                return null;
            }
        }
        public static object CreateInstanceWithConstructor(Type type, Lexicon parameters)
        {
            if (parameters.Length == 0)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating instance of {type.Name} using default constructor: {ex.Message}");
                    return null;
                }
            }

            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var constructorParams = constructor.GetParameters();
                var args = new object[constructorParams.Length];

                int i = 0;
                bool allParamsMatch = true;

                foreach (var param in constructorParams)
                {
                    if (parameters.Has(param.Name) && 
                        TryConvertParameter(parameters.GetObject(param.Name), param.ParameterType, out args[i]))
                    {
                        i++;
                    }
                    else
                    {
                        allParamsMatch = false;
                        break;
                    }
                }

                if (allParamsMatch)
                {
                    return constructor.Invoke(args);
                }
            }

            return null;
        }

        public static Color GetColorFromHex(string colorHex)
        {
            Color colorHexU;
            if (ColorUtility.TryParseHtmlString(colorHex, out colorHexU))
            {
                return colorHexU;
            }
            return Color.black;
        }

        public static int Convert2DTo1DIndex(int x, int y, int width)
        {
            return y * width + x;
        }

        public static Vector3 ColorToVector(Color color) {
            return new Vector3(color.r, color.g, color.b);
        }

        private static bool TryConvertParameter(object input, Type targetType, out object result)
        {
            try
            {
                if (targetType.IsAssignableFrom(input.GetType()))
                {
                    result = input;
                    return true;
                }

                result = Convert.ChangeType(input, targetType);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static void StringToPosition(string input, out int x, out int y)
        {
            // Split the string based on the '.' delimiter
            string[] parts = input.Split('.');

            // Parse the first part as x
            x = int.Parse(parts[0]);

            // Parse the second part as y, if it exists
            if (parts.Length > 1)
            {
                y = int.Parse(parts[1]);
            }
            else
            {
                // If there's no second part, set y to 0 or some default value
                y = 0;
            }
        }
        public static string PositionToString(int x, int y)
        {
            // Using string interpolation to concatenate x and y into the desired format
            return $"{x}.{y}";
        }

        public static float GetVelocity(float initial, float time)
        {
            return initial + time*UnityEngine.Physics.gravity.y;
        }

        public static float Sqrt(float val)
        {
            return Mathf.Sqrt(val);
        }

        public static RenderTexture ConvertToRenderTexture(Texture2D texture)
        {
            RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            // Copy Texture2D to RenderTexture
            UnityEngine.Graphics.Blit(texture, renderTexture);

            return renderTexture;
        }
        public static Texture2D ConvertToTexture2D(RenderTexture renderTexture)
        {
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = null;

            return texture;
        }

        public static Color Vector3ToColor(Vector3 v)
        {
            // Ensure the vector values are clamped between 0 and 1
            return new Color(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y), Mathf.Clamp01(v.z), 1.0f);
            return Color.black;
        }

        // Method to transform a date string ("YYYY-MM-DD") to a string array [year, month, day]
        public static string[] DateStringToArray(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                throw new ArgumentException("Input date string cannot be null or empty.");
            }

            string[] dateParts = dateString.Split('-');
            if (dateParts.Length != 3)
            {
                throw new ArgumentException("Input date string is not in the correct format (YYYY-MM-DD).");
            }

            return dateParts;
        }

        // Method to transform a string array [year, month, day] back to a date string ("YYYY-MM-DD")
        public static string ArrayToDateString(string[] dateArray)
        {
            if (dateArray == null || dateArray.Length != 3)
            {
                throw new ArgumentException("Input date array must have exactly three elements (year, month, day).");
            }

            string year = dateArray[0];
            string month = dateArray[1];
            string day = dateArray[2];

            if (year.Length != 4 || month.Length != 2 || day.Length != 2)
            {
                throw new ArgumentException("Year must be 4 digits, month and day must be 2 digits each.");
            }

            return $"{year}-{month}-{day}";
        }
        public static Vector3 ColorToVector3(Color color) {
            return new Vector3(color.r, color.g, color.b);
        }
        public static string GetParentLexiconBin(string binVarName) {
            if (binVarName.Contains(":")) {
                string[] nameComponents = binVarName.Split(":");
                string binNameOut = "";
                for (int k = 0; k < nameComponents.Length - 1; k++) {
                    if (k > 0) {
                        binNameOut += ":";
                    }
                    binNameOut += nameComponents[k];
                }
                return binNameOut;
            } return "";
        }
    }
}