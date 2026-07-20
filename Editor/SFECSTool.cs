using UnityEditor;

namespace SFramework.ECS.Editor
{
    public static class SFECSTool
    {
        [MenuItem("Edit/SFramework/Regenerate ECS Components")]
        private static void RegenerateComponent()
        {
            SFComponentsGenerator.Generate(true);
        }
    }
}