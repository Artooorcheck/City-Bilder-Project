using UnityEngine;

namespace CityBuilder.Presentation.Presenters
{
    public static class PresentationBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindObjectOfType<PresentationRoot>() != null)
            {
                return;
            }

            var rootObject = new GameObject("PresentationRoot");
            Object.DontDestroyOnLoad(rootObject);
            rootObject.AddComponent<PresentationRoot>();
        }
    }
}
