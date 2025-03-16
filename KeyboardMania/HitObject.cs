namespace KeyboardMania
{
    public class HitObject
    {
        public int Lane { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public bool IsHeldNote { get; set; }
        public double HoldDuration { get; set; }
    }
}
