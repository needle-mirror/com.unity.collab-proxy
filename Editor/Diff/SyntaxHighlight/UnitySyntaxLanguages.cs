using System;
using System.Collections.Generic;

using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.SyntaxHighlight
{
    internal static class UnitySyntaxLanguages
    {
        internal static readonly Dictionary<string, Language> AdditionalExtensions =
            BuildExtensions();

        static Dictionary<string, Language> BuildExtensions()
        {
            Dictionary<string, Language> result = new Dictionary<string, Language>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string ext in DiffViewerDataExtensions.SERIALIZED_EXTENSIONS)
                result[ext] = Language.YAML;

            // the .anim is not included in
            // serialized extensions, but we want
            // to syntax hightlight them as yaml
            result[".anim"] = Language.YAML;

            return result;
        }
    }
}
