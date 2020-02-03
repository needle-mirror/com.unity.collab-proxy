using System;
using Newtonsoft.Json;

namespace Unity.Cloud.Collaborate.Models.Providers.Client.Models
{
    internal enum ChangeType
    {
        Added,
        Modified,
        Moved,
        Renamed,
        Deleted,
        Unmodified,
        Gone,
        Ignored
    }

    [JsonObject]
    internal class ChangeWrapper
    {
        [JsonProperty]
        public string Path { get; set; }

        [JsonProperty]
        public ChangeType Status { get; set; }

        [JsonProperty]
        public string Hash { get; set; }

        [JsonProperty]
        public bool IsFolder { get; set; }

        public override string ToString()
        {
            return $"{Path} : {Status} : {Hash} : {(IsFolder ? "Folder" : "File")}";
        }
    }
}
