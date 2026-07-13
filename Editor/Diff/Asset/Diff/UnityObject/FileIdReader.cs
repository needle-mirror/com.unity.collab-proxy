using System;
using System.Collections.Generic;
using System.IO;

using Codice.LogWrapper;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal struct FileIdInfo
    {
        internal List<long> FileIds;
        internal Dictionary<long, long> HashByFileId;
    }

    internal static class FileIdReader
    {
        internal static FileIdInfo Read(string yamlFilePath)
        {
            FileIdInfo result = new FileIdInfo
            {
                FileIds = new List<long>(),
                HashByFileId = new Dictionary<long, long>()
            };

            try
            {
                using (StreamReader reader = new StreamReader(yamlFilePath))
                {
                    string line;
                    long currentFileId = 0;
                    long currentHash = 0;
                    bool inBlock = false;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("--- !u!", StringComparison.Ordinal))
                        {
                            if (inBlock)
                                result.HashByFileId[currentFileId] = currentHash;

                            inBlock = false;
                            currentFileId = 0;
                            currentHash = 0;

                            int ampIdx = line.IndexOf('&');
                            if (ampIdx < 0)
                                continue;

                            ReadOnlySpan<char> idSpan = line.AsSpan(ampIdx + 1).Trim();
                            if (long.TryParse(idSpan, out long fileId))
                            {
                                result.FileIds.Add(fileId);
                                currentFileId = fileId;
                                inBlock = true;
                            }

                            continue;
                        }

                        if (inBlock)
                        {
                            unchecked
                            {
                                currentHash = currentHash * 1099511628211 + line.GetHashCode();
                            }
                        }
                    }

                    if (inBlock)
                        result.HashByFileId[currentFileId] = currentHash;
                }
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat(
                    "Error reading file IDs from '{0}': {1}",
                    yamlFilePath, ex.Message);
                mLog.DebugFormat(
                    "Stack trace:{0}{1}",
                    Environment.NewLine, ex.StackTrace);

                result.FileIds = new List<long>();
                result.HashByFileId = new Dictionary<long, long>();
            }

            return result;
        }

        static readonly ILog mLog = PlasticApp.GetLogger("FileIdReader");
    }
}
