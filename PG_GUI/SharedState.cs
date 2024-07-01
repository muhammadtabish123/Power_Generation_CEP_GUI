using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;

public static class SharedState
{
    public static List<(float Duration, double Load)> LoadDurationCurve { get; set; } = new List<(float Duration, double Load)>();
    public static float ConnectedLoad { get; set; }
    public static float MaxDemand { get; set; }
    public static float DemandFactor { get; set; }
    public static float AverageLoad { get; set; }
    public static float LoadFactor { get; set; }
    public static float PlantCapacity { get; set; }
    public static float PlantCapacityFactor { get; set; }
    public static Series LoadDurationCurveSeries { get; set; }
}
