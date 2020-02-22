using System.Collections.Generic;

namespace Gimic.Models
{
    public class TokenState
    {
        public List<(string token, string id)> CapturedTokens { get; } = new List<(string token, string id)>();
        public string PartialToken { get; set; } = string.Empty;
    }
}
