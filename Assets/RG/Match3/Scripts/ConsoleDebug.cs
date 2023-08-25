using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConsoleDebug : MonoBehaviour
{
    private static ConsoleDebug instance;

    [SerializeField]
    [Tooltip("Should we print all the debug messages?")]
    private bool _debug;

    public static ConsoleDebug Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<ConsoleDebug>();

            if (instance != null) return instance;

            var singletonObject = new GameObject();
            instance = singletonObject.AddComponent<ConsoleDebug>();
            singletonObject.name = typeof(ConsoleDebug) + " (Singleton)";

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Log(string message)
    {
        if (_debug)
            Debug.Log(message);
    }

    public void Log<T>(IEnumerable<T> list, string header)
    {
        var enumerable = list.ToList();
        if (_debug && enumerable.Any())
            Debug.Log($"{header}: {string.Join(", ", enumerable)}");
    }
}