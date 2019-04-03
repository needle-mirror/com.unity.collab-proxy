using System;
using System.Runtime.Serialization;

namespace CollabProxy.Models
{
    internal enum ChangeType
    {
        Added,
        Modified,
        Moved,
        Renamed,
        Deleted
    }

    [DataContract(Namespace = "", Name = "ChangeWrapper")]
    internal class ChangeWrapper
    {
        [DataMember(Name = "Path")]
        public string Path { get; set; }
        [DataMember(Name = "Status")]
        public ChangeType Status { get; set; }
        [DataMember(Name = "Hash")]
        public string Hash { get; set; }
        [DataMember(Name = "IsFolder")]
        public bool IsFolder { get; set; }
    }
}
