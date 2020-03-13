using System.Collections.Generic;

namespace Infrastructure.Schema
{
    public class Config
    {
        public int          ID             { get; set; }
        public List<string> Sources        { get; set; }
        public List<string> SearchDomains  { get; set; }
        public bool         DomainNeeded   { get; set; }
        public string       SpoofedA       { get; set; }
        public string       SpoofedAAAA    { get; set; }
        public string       SpoofedCNAME   { get; set; }
        public string       SpoofedDefault { get; set; }

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