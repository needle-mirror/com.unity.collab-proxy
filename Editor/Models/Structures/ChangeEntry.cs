using System;

namespace Unity.Cloud.Collaborate.Models.Structures
{
    internal struct ChangeEntry : IChangeEntry
    {
        public ChangeEntry(string path = default, string originalPath = default, ChangeEntryStatus status = default, bool staged = default, bool unmerged = default, object tag = default)
        {
            Path = path;
            OriginalPath = originalPath;
            Status = status;
            Staged = staged;
            Unmerged = unmerged;
            Tag = tag;
        }

        public string Path { get; }
        public string OriginalPath { get; }
        public ChangeEntryStatus Status { get; }
        public bool Staged { get; }
        public bool Unmerged { get; }
        public object Tag { get; }

        /// <inheritdoc />
        public string StatusToString()
        {
            switch (Status)
            {
                case ChangeEntryStatus.Added:
                case ChangeEntryStatus.Untracked:
                    return "added";
                case ChangeEntryStatus.Modified:
                case ChangeEntryStatus.TypeChange:
                    return "edited";
                case ChangeEntryStatus.Deleted:
                    return "deleted";
                case ChangeEntryStatus.Renamed:
                case ChangeEntryStatus.Copied:
                    return "moved";
                case ChangeEntryStatus.Unmerged:
                    return "conflicted";
                case ChangeEntryStatus.None:
                    break;
                case ChangeEntryStatus.Ignored:
                    break;
                case ChangeEntryStatus.Unknown:
                    break;
                case ChangeEntryStatus.Broken:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // TODO: find a way to handle/display the unexpected/broken status types.
            return null;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // Calculate hash based on the path and the status. 397 is just a magic prime number to ensure the
                // likelihood of a collision is as low as possible.
                return ((Path?.GetHashCode() ?? 0) * 397) ^ (int)Status;
            }
        }
    }
}
