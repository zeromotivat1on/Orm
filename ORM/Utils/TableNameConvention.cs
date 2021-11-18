namespace ORM.Utils
{
    public static class TableNameConvention
    {
        public static string ToConventionTableName(this string initialTableName)
        {
            return Convert(initialTableName);
        }

        public static string Convert(string initialTableName)
        {
            int initialTableNameLength = initialTableName.Length;
            switch (initialTableName)
            {
                case "Person":
                case "Human":
                    {
                        return "People";
                    }
                case "Sheep":
                    {
                        return "Sheep";
                    }
                case "Deer":
                    {
                        return "Deer";
                    }
                case "Series":
                    {
                        return "Series";
                    }
                case "Species":
                    {
                        return "Species";
                    }
                case "Child":
                    {
                        return "Children";
                    }
                case "Goose":
                    {
                        return "Geese";
                    }
                case "Man":
                    {
                        return "Men";
                    }
                case "Woman":
                    {
                        return "Women";
                    }
                case "Tooth":
                    {
                        return "Teeth";
                    }
                case "Foot":
                    {
                        return "Feet";
                    }
                case "Mouse":
                    {
                        return "Mice";
                    }
                default:
                    {
                        string modifiedTableName = "";
                        if (initialTableName.EndsWith("s") ||
                            initialTableName.EndsWith("x") ||
                            initialTableName.EndsWith("z"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 1);
                            return $"{modifiedTableName}es";
                        }
                        if (initialTableName.EndsWith("ss") ||
                            initialTableName.EndsWith("sh") ||
                            initialTableName.EndsWith("ch"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 2);
                            return $"{modifiedTableName}es";
                        }
                        else if (initialTableName.EndsWith("f"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 1);
                            return $"{modifiedTableName}ves";
                        }
                        else if (initialTableName.EndsWith("fe"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 2);
                            return $"{modifiedTableName}ves";
                        }
                        else if (initialTableName.EndsWith("o"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 1);
                            return $"{modifiedTableName}es";
                        }
                        if (initialTableName.EndsWith("us"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 2);
                            return $"{modifiedTableName}i";
                        }
                        else if (initialTableName.EndsWith("on"))
                        {
                            modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 2);
                            return $"{modifiedTableName}a";
                        }
                        else if (initialTableName.EndsWith("y"))
                        {
                            if (IsVowel(initialTableName[initialTableNameLength - 2]))
                            {
                                return $"{initialTableName}s";
                            }
                            else
                            {
                                modifiedTableName = initialTableName.Substring(0, initialTableNameLength - 1);
                                return $"{modifiedTableName}ies";
                            }
                        }
                        else
                        {
                            return $"{initialTableName}s";
                        }
                    }
            }
        }

        private static bool IsVowel(char letter)
        {
            return "aeiouAEIOU".IndexOf(letter) >= 0;
        }
    }
}
