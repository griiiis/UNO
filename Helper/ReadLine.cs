namespace Helper;

public static class ReadLine
{
    public static string[] ReadLineHelper(string sentence, string type, int wordLength, List<int> containsInt,
        List<string> containsString)
    {
        while (true)
        {
            Console.WriteLine(sentence);
            var input = Console.ReadLine();

            if (type == "int")
            {
                if (int.TryParse(input, out var intValue) && containsInt.Contains(intValue))
                {
                    return new[] { input };
                }

                if (int.TryParse(input, out var intValue1) && containsInt.Count == 0)
                {
                    return new[] { input };
                }
            }

            if (type == "string" && !string.IsNullOrWhiteSpace(input))
            {
                if (containsString.Count == 0 && input.Length <= wordLength)
                {
                    return new[] { input };
                }

                if (containsString.Contains(input.ToUpper()))
                {
                    return new[] { input };
                }
            }

            if (type == "string/int" && !string.IsNullOrWhiteSpace(input))
            {
                if (int.TryParse(input, out var intValue) && containsInt.Contains(intValue))
                {
                    return new[] { input };
                }

                if (containsString.Contains(input.ToUpper()))
                {
                    return new[] { input };
                }
            }

            if (type == "null/string")
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return new[] { "" };
                }

                if (input.Contains(','))
                {
                    var newInput = input.Split(", ");
                    if (containsString.Contains(newInput[0].ToUpper()) &&
                        containsString.Contains(newInput[1].ToUpper()))
                    {
                        return new[] { input };
                    }
                }

                if (containsString.Contains(input.ToUpper()) && !input.Contains(','))
                {
                    return new[] { input };
                }
            }

            Console.WriteLine("Parse error...");
        }
    }
}