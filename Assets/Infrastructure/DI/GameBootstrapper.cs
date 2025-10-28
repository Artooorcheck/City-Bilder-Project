using UnityEngine;

namespace CityBuilder.Infrastructure.DI
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindAnyObjectByType<GameLifetimeScope>() != null)
            {
                return;
            }

            var scopeObject = new GameObject("GameLifetimeScope");
            Object.DontDestroyOnLoad(scopeObject);
            scopeObject.AddComponent<GameLifetimeScope>();
        }
    }
}
