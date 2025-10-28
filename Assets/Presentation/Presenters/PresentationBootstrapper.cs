using UnityEngine;

namespace CityBuilder.Presentation.Presenters
{
    public static class PresentationBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindAnyObjectByType<PresentationRoot>() != null)
            {
                return;
            }

            var rootObject = new GameObject("PresentationRoot");
            Object.DontDestroyOnLoad(rootObject);
            rootObject.AddComponent<PresentationRoot>();
        }
    }
}
