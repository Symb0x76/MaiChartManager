namespace MaiChartManager.Utils;

public static class MaiUtils
{
    public static int GetLevelId(int levelX10)
    {
        return levelX10 switch
        {
            >= 156 => 24,
            >= 150 => 23,
            >= 146 => 22,
            >= 140 => 21,
            >= 136 => 20,
            >= 130 => 19,
            >= 126 => 18,
            >= 120 => 17,
            >= 116 => 16,
            >= 110 => 15,
            >= 106 => 14,
            >= 100 => 13,
            >= 96 => 12,
            >= 90 => 11,
            >= 86 => 10,
            >= 80 => 9,
            >= 76 => 8,
            >= 0 => levelX10 / 10,
            _ => 0
        };
    }
}