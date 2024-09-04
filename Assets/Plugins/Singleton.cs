using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (!_instance)
				_instance = new GameObject("singleton").AddComponent<T>();
			return _instance;
		}
	}

	public void Ping()
	{
		//Nothing, just to instantiate
	}

	public static bool Exists()
	{
		return _instance;
	}
}