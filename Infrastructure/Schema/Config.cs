using System.Collections.Generic;

namespace Infrastructure.Schema
{
    public class Config
    {
        public int          ID             { get; set; }
        public List<string> Sources        { get; set; } = null!;
        public List<string> SearchDomains  { get; set; } = null!;
        public bool         DomainNeeded   { get; set; }
        public string       SpoofedA       { get; set; } = null!;
        public string       SpoofedAAAA    { get; set; } = null!;
        public string       SpoofedCNAME   { get; set; } = null!;
        public string       SpoofedDefault { get; set; } = null!;

        public override string ToString()
        {
            return
                $"{{"                                 +
                $"{string.Join(',', Sources)} "       +
                $"{string.Join(',', SearchDomains)} " +
                $"{DomainNeeded} "                    +
                $"{SpoofedA} "                        +
                $"{SpoofedAAAA} "                     +
                $"{SpoofedCNAME} "                    +
                $"{SpoofedDefault}"                   +
                $"}}";
        }
    }
}