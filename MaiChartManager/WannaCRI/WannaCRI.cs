using System.Globalization;

namespace MaiChartManager.WannaCRI;

public static class WannaCRI
{
    private const string DefaultKey = "0x7F4551499DF55E68";

    public static int RunHelper(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing WannaCRI command.");
            return 2;
        }

        try
        {
            var operation = args[0].ToLowerInvariant();
            return operation switch
            {
                "extractusm" => RunExtractUsm(args),
                "createusm" => RunCreateUsm(args),
                _ => throw new ArgumentException($"Unsupported WannaCRI command: {args[0]}")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunExtractUsm(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("extractusm requires an input file path.");
        }

        var input = args[1];
        var output = "./output";
        var key = DefaultKey;

        for (var i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":
                case "--output":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for --output.");
                    }

                    output = args[++i];
                    break;
                case "-k":
                case "--key":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for --key.");
                    }

                    key = args[++i];
                    break;
            }
        }

        UnpackUsm(input, output, key);
        return 0;
    }

    private static int RunCreateUsm(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("createusm requires an input file path.");
        }

        var input = args[1];
        string? output = null;
        var key = DefaultKey;

        for (var i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":
                case "--output":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for --output.");
                    }

                    output = args[++i];
                    break;
                case "-k":
                case "--key":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for --key.");
                    }

                    key = args[++i];
                    break;
            }
        }

        CreateUsm(input, output, key);
        return 0;
    }

    public static void CreateUsm(string src, string key = DefaultKey)
    {
        CreateUsm(src, output: null, key);
    }

    public static void CreateUsm(string src, string? output, string key = DefaultKey)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            throw new ArgumentException("Input path is required.", nameof(src));
        }

        if (!File.Exists(src))
        {
            throw new FileNotFoundException("Input file not found.", src);
        }

        if (!TryParseHexKey(key, out var keyValue))
        {
            throw new ArgumentException($"Invalid key format: {key}", nameof(key));
        }

        UsmCreator.Create(src, output, keyValue);
    }

    public static void UnpackUsm(string src, string output, string key = DefaultKey)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            throw new ArgumentException("Input path is required.", nameof(src));
        }

        if (!File.Exists(src))
        {
            throw new FileNotFoundException("USM input file not found.", src);
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentException("Output directory is required.", nameof(output));
        }

        Directory.CreateDirectory(output);

        if (!TryParseHexKey(key, out var keyValue))
        {
            throw new ArgumentException($"Invalid key format: {key}", nameof(key));
        }

        UsmExtractor.Extract(src, output, keyValue);
    }

    private static bool TryParseHexKey(string key, out ulong keyValue)
    {
        var normalized = key.Trim();
        if (normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[2..];
        }

        return ulong.TryParse(normalized, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out keyValue);
    }
}
