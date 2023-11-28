using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
	private float _time = 0;

	private void Update()
	{
		_time += Time.deltaTime;
		if(_time > 1.0f)
		{
			Debug.Log("Prefab Update");
			_time = 0;
		}
	}
}
