namespace Gimic.Models
{
    public class Line
    {
        public string Key { get; }
        public dynamic Value { get; set; }
        public int TabCount { get; set; }
        public Line(string key, dynamic value, int tabCount)
        {
            Key = key;
            Value = value;
            TabCount = tabCount;
        }
    }
}
