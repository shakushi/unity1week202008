using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    public enum LevelDef
    {
        Easy = 0,
        Normal,
        Hard,
        God
    }

    public int Level
    {
        get { return level; }
    }

    private int level = (int)GameLevel.LevelDef.Easy;


    public void SetLevelEasy()
    {
        setLevel(GameLevel.LevelDef.Easy);
    }
    public void SetLevelNormal()
    {
        setLevel(GameLevel.LevelDef.Normal);
    }
    public void SetLevelHard()
    {
        setLevel(GameLevel.LevelDef.Hard);
    }
    public void SetLevelGod()
    {
        setLevel(GameLevel.LevelDef.God);
    }

    private void setLevel(GameLevel.LevelDef def)
    {
        level = (int)def;
    }
}
