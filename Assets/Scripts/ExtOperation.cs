using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtOperation
{
	//--------------------------------------------------------------------------------
	// Listから要素をランダムで1つ取得する
	//--------------------------------------------------------------------------------
	public static T GetRandom<T>(List<T> list)
	{
		return list[Random.Range(0, list.Count)];
	}
}
