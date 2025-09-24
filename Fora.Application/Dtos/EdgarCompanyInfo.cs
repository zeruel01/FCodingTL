using System.Text.Json.Serialization;

namespace Fora.Application;

public class EdgarCompanyInfo {
    public int Cik { get; set; }
    public string? EntityName { get; set; }
    public InfoFact? Facts { get; set; }
    public class InfoFact {
        [JsonPropertyName("us-gaap")]
        public InfoFactUsGaap? UsGaap { get; set; }
    }
    public class InfoFactUsGaap {
        [JsonPropertyName("NetIncomeLoss")]
        public InfoFactUsGaapNetIncomeLoss? NetIncomeLoss { get; set; }
    }
    public class InfoFactUsGaapNetIncomeLoss {
        public InfoFactUsGaapIncomeLossUnits? Units { get; set; }
    }
    public class InfoFactUsGaapIncomeLossUnits {
        [JsonPropertyName("USD")]
        public InfoFactUsGaapIncomeLossUnitsUsd[]? Usd { get; set; }
    }
    public class InfoFactUsGaapIncomeLossUnitsUsd {
        /// YOU ARE INTERESTED ONLY IN 10-K DATA!
        [JsonPropertyName("form")]
        public string? Form { get; set; }
        /// YOU ARE INTERESTED ONLY IN YEARLY INFORMATION (Frame like "CY2021")!
        [JsonPropertyName("frame")]
        public string? Frame { get; set; }
        /// The income/loss amount.
        [JsonPropertyName("val")]
        public decimal Val { get; set; }
    }
}
