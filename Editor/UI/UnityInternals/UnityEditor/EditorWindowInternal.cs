namespace Unity.PlasticSCM.Editor.UnityInternals.UnityEditor
{
    internal class EditorWindow
    {
        internal delegate void Internal_MakeModalDelegate(ContainerWindow window);
        internal static Internal_MakeModalDelegate Internal_MakeModal { get; set; }
    }
}
