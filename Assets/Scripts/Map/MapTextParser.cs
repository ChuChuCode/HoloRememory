using System.Collections.Generic;

namespace HR.Map{
public class MapParseResult
{
    public bool Success;
    public List<string> Errors = new List<string>();
    public int Width;
    public int Height;
    // Grid[row][col] - row 0 is the first line of the file, maps to the
    // most-negative Z (see MapLoader's cell->world conversion).
    public char[][] Grid;
}

// Pure parsing, no Unity scene/GameObject dependencies - takes raw text (not
// a TextAsset) specifically so a future Steam Workshop loader can call
// Parse(File.ReadAllText(path)) unchanged, without touching this class.
public static class MapTextParser
{
    public static MapParseResult Parse(string text)
    {
        var result = new MapParseResult();
        if (string.IsNullOrEmpty(text))
        {
            result.Errors.Add("Map text is empty.");
            return result;
        }

        // Split on \n and strip \r so the same file parses the same whether
        // it was saved with Windows or Unix line endings.
        string[] rawLines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        // Trailing blank lines (common with a final newline at EOF) are not
        // an error - just not counted as map rows.
        int lastNonEmpty = rawLines.Length - 1;
        while (lastNonEmpty >= 0 && rawLines[lastNonEmpty].Length == 0) lastNonEmpty--;

        if (lastNonEmpty < 0)
        {
            result.Errors.Add("Map text has no rows.");
            return result;
        }

        int height = lastNonEmpty + 1;
        int width = rawLines[0].Length;
        if (width == 0)
        {
            result.Errors.Add("Map row 0 is empty.");
            return result;
        }

        var grid = new char[height][];
        for (int row = 0; row < height; row++)
        {
            string line = rawLines[row];
            if (line.Length != width)
            {
                result.Errors.Add($"Row {row} has length {line.Length}, expected {width} (ragged rows are not allowed).");
                continue;
            }
            grid[row] = line.ToCharArray();
        }

        if (result.Errors.Count > 0) return result;

        result.Success = true;
        result.Width = width;
        result.Height = height;
        result.Grid = grid;
        return result;
    }
}
}
