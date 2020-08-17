using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerate : MonoBehaviour
{
    enum direction_def
    {
        Forward = 0,
        Back,
        Right,
        Left
    }
    enum map_chip_g
    {
        g_Plane = 0,
        g_Goal,
        g_Start,
    }
    enum map_chip_p
    {
        p_Enable = 0,
        p_Disable,
    }
    enum map_chip_w
    {
        w_None = 0,
        w_On
    }

    private GameObject LevelObj;
    private GameLevel level;
    private GameObject m_cell_pri;
    private GameObject m_wall;
    private GameObject m_goal;
    private int mapLen_x = 9; /* Only Odd number(desire cell num+1) */
    private int mapLen_y = 9; /* Only Odd number(desire cell num+1) */
    //private static int poll_num_max = (mapLen_x + 1) / 2 + (mapLen_y + 1) / 2; /* 偶数*偶数のマス目の数 */
    private int[,] map;
    /* map配列の考え方 */
    /* 偶数*偶数 = 壁生成の起点となるマス　map_chip_p */
    /* 偶数*奇数 = 壁の有無を示すマス　map_chip_w */
    /* 奇数*奇数 = 地面またはゴールを示すマス map_chip_g */

    // Start is called before the first frame update
    void Awake()
    {
        LevelObj = GameObject.Find("GameLevel");
        level = LevelObj.GetComponent<GameLevel>();
        setLevel(level.Level);

        m_cell_pri = (GameObject)Resources.Load("Cell_prim");
        m_wall = (GameObject)Resources.Load("Wall");
        m_goal = (GameObject)Resources.Load("Goal");
        map = mapArraySet();
        dumpMap(map);
        if (map != null)
        {
            makeCells(map);
        }
    }

    private void setLevel(int level_def)
    {
        switch (level_def)
        {
            case (int)GameLevel.LevelDef.Easy:
                mapLen_x = 9;
                mapLen_y = 9;
                break;
            case (int)GameLevel.LevelDef.Normal:
                mapLen_x = 13;
                mapLen_y = 13;
                break;
            case (int)GameLevel.LevelDef.Hard:
                mapLen_x = 21;
                mapLen_y = 21;
                break;
            case (int)GameLevel.LevelDef.God:
                mapLen_x = 31;
                mapLen_y = 31;
                break;
            default:
                Debug.Log("unexpected error: level is invalid");
                mapLen_x = 9;
                mapLen_y = 9;
                break;
        }
    }

    /* generate randam map array */
    private int[,] mapArraySet()
    {
        int[,] ret;

        /* 壁のばし法 */
        /* 生成&外周を全て壁にする */
        ret = preinitMap();

        /* 起点の候補を洗い出し */
        /* 候補 = 偶数*偶数 壁生成の起点となるマス　map_chip_p */
        List<Vector2> enablePolls = new List<Vector2>();
        for (int i = 0; i < mapLen_x; i++)
        {
            for (int j = 0; j < mapLen_y; j++)
            {
                if ( i % 2 == 0 &&
                     j % 2 == 0 &&
                     ret[i, j] == (int)map_chip_p.p_Enable)
                {
                    enablePolls.Add(new Vector2(i, j));
                }
            }
        }
        if (enablePolls.Count <= 0)
        {
            Debug.Log("cannot make walls");
            return null;
        }

        List<Vector2> move_cache = new List<Vector2>();
        Vector2 cur_position;
        /* 起点の決定と新規壁作成 */
        while (setStartPos(enablePolls, out cur_position))
        {
            //dumpMap(ret);
            //Debug.Log("cur_position = " + cur_position);
            move_cache.Clear();
            move_cache.Add(cur_position);
            /* 既存の壁に当たるか新規の壁に囲まれるまで進み続ける */
            if (tryMakeWall(ret, ref move_cache, cur_position))
            {
                /* 既存の壁に当たることができたらmapに反映 */
                updateMapWall(ref ret, ref enablePolls, move_cache);
            }
        }

        ret[mapLen_x - 2, mapLen_y - 2] = (int)map_chip_g.g_Goal; //暫定 右上をゴールに
        //ret[1, 3] = (int)map_chip_g.g_Goal; //デバッグ用　スタートすぐの位置もゴールに
        return ret;
    }

    /* 生成&外周を全て壁にする */
    private int[,] preinitMap()
    {
        int[,]ret = new int[mapLen_x, mapLen_y]; //all zero
        for (int i = 0; i < mapLen_x; i++)
        {
            if (i % 2 == 0)
            {
                ret[i, 0] = (int)map_chip_p.p_Disable;
                ret[i, mapLen_y - 1] = (int)map_chip_p.p_Disable;
            }
            else
            {
                ret[i, 0] = (int)map_chip_w.w_On;
                ret[i, mapLen_y - 1] = (int)map_chip_w.w_On;
            }
        }
        for (int j = 0; j < mapLen_y; j++)
        {
            if (j % 2 == 0)
            {
                ret[0, j] = (int)map_chip_p.p_Disable;
                ret[mapLen_x - 1, j] = (int)map_chip_p.p_Disable;
            }
            else
            {
                ret[0, j] = (int)map_chip_w.w_On;
                ret[mapLen_x - 1, j] = (int)map_chip_w.w_On;
            }
        }
        return ret;
    }

    /* 候補からランダムに起点を決定する */
    /* 候補 = 偶数*偶数 壁生成の起点となるマス　map_chip_p */
    private bool setStartPos(List<Vector2> enablePolls, out Vector2 cur_position)
    {
        if (enablePolls.Count <= 0)
        {
            Debug.Log("there are no enable polls. end");
            cur_position = Vector2.zero;
            return false;
        }

        cur_position = ExtOperation.GetRandom<Vector2>(enablePolls);
        return true;
    }

    /* 既存の壁に当たるか新規の壁に囲まれるまで進み続ける */
    private bool tryMakeWall(int[,] map_ref, ref List<Vector2> move_cache, Vector2 cur_pos)
    {
        int next_dir;
        byte checked_dir = 0;
        /* 移動方向決定 */
        while(getNextRandamDir(out next_dir, ref checked_dir))
        {
            Vector2 next_pos = getVectorByDir(next_dir, cur_pos);

            /* 移動できるか確認 */
            int result = checkPosition(map_ref, move_cache, next_pos);

            switch (result)
            {
                /* 移動できてかつ終了ではない->方向確定させて記録し次へ */
                case 0:
                    move_cache.Add(next_pos);
                    return tryMakeWall(map_ref, ref move_cache, next_pos);
                    break;
                /* 移動できてかつ終了->方向確定させて終了 */
                case 1:
                    move_cache.Add(next_pos);
                    return true;
                    break;
                /* 移動失敗->違う方向を調べる */
                case 2:
                    continue;
                    break;
                default:
                    Debug.Log("unexpect error: checkPosition result is invalid.");
                    return false;
                    break;
            }
        }
        /* 全方向を探索し失敗だった */
        return false;
    }

    private bool getNextRandamDir(out int next_dir, ref byte checked_dir)
    {
        if ((checked_dir & 0x0F) == 0x0F)
        {
            /* 全方向探索済み */
            next_dir = -1;
            return false;
        }

        int first_p = Random.Range(0, 3 + 1); /* 0-3の整数を得る */
        byte checkbit = (byte)(0x01 << first_p); /* 0001 or 0010 or 0100 or 1000 */
        while ((checkbit & checked_dir) != 0)
        {
            checkbit = (byte)(checkbit << 1);
            if ((checkbit & 0x0F) == 0)
            {
                checkbit = 0x01;
            }
        }

        /* ループ抜け=未チェックの方向を発見（成功） */
        checked_dir |= checkbit;
        if ((checkbit & 0x01) != 0)
        {
            next_dir = (int)direction_def.Forward;
        }
        else if ((checkbit & 0x02) != 0)
        {
            next_dir = (int)direction_def.Right;
        }
        else if ((checkbit & 0x04) != 0)
        {
            next_dir = (int)direction_def.Back;
        }
        else if ((checkbit & 0x08) != 0)
        {
            next_dir = (int)direction_def.Left;
        }
        else
        {
            /* Its bug if enter here... */
            Debug.Log("unexpect error: checkbit is invalid...");
            next_dir = -1;
            return false;
        }

        return true;
    }

    private Vector2 getVectorByDir(int dir, Vector2 cur)
    {
        Vector2 move = Vector2.zero;

        switch (dir)
        {
            case (int)direction_def.Forward:
                move.y = 2;
                break;
            case (int)direction_def.Right:
                move.x = 2;
                break;
            case (int)direction_def.Back:
                move.y = -2;
                break;
            case (int)direction_def.Left:
                move.x = -2;
                break;
            default:
                Debug.Log("unexpect error: direction is invalid");
                return move;
        }

        return (cur + move);
    }

    private int checkPosition(int[,] map_ref, List<Vector2> move_cache, Vector2 pos)
    {
        /* 指定された位置が今回の探索で一度通った柱 */
        foreach (Vector2 one in move_cache)
        {
            if (one == pos)
            {
                return 2;
            }
        }

        int x = (int)pos.x;
        int y = (int)pos.y;

        /* 指定された位置がまだ壁のない柱 */
        if (map_ref[x, y] == (int)map_chip_p.p_Enable)
        {
            return 0;
        }
        /* 指定された位置が既に壁のある柱 */
        else if (map_ref[x, y] == (int)map_chip_p.p_Disable)
        {
            return 1;
        }
        else
        {
            Debug.Log("unexpect error: mapdata is invalid");
            return -1;
        }
    }

    /* cacheをmapと起点候補に反映する */
    private void updateMapWall(ref int[,] map_ref, ref List<Vector2> enablePolls, List<Vector2> move_cache)
    {
        Vector2 cache_pre = Vector2.zero;
        foreach (Vector2 cache_one in move_cache)
        {
            /* 柱を使用済みにする */
            int x = (int)cache_one.x;
            int y = (int)cache_one.y;
            map_ref[x, y] = (int)map_chip_p.p_Disable;

            /* 壁を生成する */
            if (cache_pre.magnitude != 0)
            {
                Vector2 diff = cache_pre - cache_one;
                int w_x = (int)(cache_one.x + (diff.x / 2));
                int w_y = (int)(cache_one.y + (diff.y / 2));
                map_ref[w_x, w_y] = (int)map_chip_w.w_On;
            }

            /* 起点候補を更新する */
            enablePolls.Remove(cache_one);

            cache_pre = cache_one;
        }
    }

    private void dumpMap(int[,] map)
    {
        string log = "---- DEBUG_DUMP_START ----\n";
        for (int j = mapLen_y - 1; j >= 0; j--)
        {
            log += j.ToString("D2") + ": ";
            for (int i = 0; i < mapLen_x; i++)
            {
                log += map[i, j].ToString("D2");
                log += " ";
            }
            log += "\n";
        }
        for (int i = 0; i < mapLen_x; i++) { log += "--"; }
        log += "\n00: ";
        for (int i = 0; i < mapLen_x; i++)
        {
            log += i.ToString("D2") + " ";
        }
        log += "\n";
        log += "---- DEBUG_DUMP_END ----";
        Debug.Log(log);
    }

    private void makeCells(int[,] map)
    {
        for (int i = 0; i < mapLen_x; i++)
        {
            for (int j = 0; j < mapLen_y; j++)
            {
                /* If Even number */
                /* 偶数*偶数 = 壁にも地面にもならない無視するマス　中身は何が入っていてもいい */
                /* 偶数*奇数 = 壁の有無を示すマス　PlaneまたはWallが格納される */
                if (i % 2 == 0 || j % 2 == 0)
                {
                    /* skip */
                    continue;
                }

                /* If Odd number (either i and j) */
                /* 奇数*奇数 = 地面またはゴールを示すマス PlaneまたはStart,Goalが格納される */

                /* 性能改善　ゲームスタート時に最大数を生成しておく */
                //GameObject cell = Instantiate(m_cell_pri, LocalPosBy2d(i/2, j/2), Quaternion.identity);
                //cell.transform.parent = this.transform;

                Vector3 localPos = LocalPosBy2d(i / 2, j / 2);

                /* If Goal */
                if (map[i, j] == (int)map_chip_g.g_Goal)
                {
                    Debug.Log("generate goal");
                    GameObject goal = Instantiate(m_goal, localPos + new Vector3(0, 0.9f, 0), Quaternion.identity);
                    //goal.transform.localPosition = new Vector3(0, 0.2f, 0);
                }

                /* If has wall in front */
                if (j >= (mapLen_y - 2))
                {
                    makeWall(direction_def.Forward, localPos, Quaternion.identity);
                }
                else if (map[i, j+1] == (int)map_chip_w.w_On)
                {
                    makeWall(direction_def.Forward, localPos, Quaternion.identity);
                }

                /* If has wall in back */
                if (j <= 1)
                {
                    makeWall(direction_def.Back, localPos, Quaternion.identity);
                }
                else if (map[i, j-1] == (int)map_chip_w.w_On)
                {
                    makeWall(direction_def.Back, localPos, Quaternion.identity);
                }

                /* If has wall in right */
                if (i >= (mapLen_x - 2))
                {
                    makeWall(direction_def.Right, localPos, Quaternion.identity);
                }
                else if (map[i+1, j] == (int)map_chip_w.w_On)
                {
                    makeWall(direction_def.Right, localPos, Quaternion.identity);
                }

                /* If has wall in left */
                if (i <= 1)
                {
                    makeWall(direction_def.Left, localPos, Quaternion.identity);
                }
                else if (map[i-1, j] == (int)map_chip_w.w_On)
                {
                    makeWall(direction_def.Left, localPos, Quaternion.identity);
                }
            }
        }
    }

    private void makeWall(direction_def dir, Vector3 cell_position, Quaternion cell_quaternion)
    {
        Vector3 LocalPos;
        Quaternion LocalRot;
        GameObject wall;
        switch (dir)
        {
            case direction_def.Forward:
                LocalPos = new Vector3(0.0f, 3.0f, 2.5f);
                LocalRot = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                wall = Instantiate(m_wall, cell_position + LocalPos, cell_quaternion);
                //wall = Instantiate(m_wall, cell_position, cell_quaternion);
                //wall.transform.localPosition = LocalPos;
                wall.transform.rotation = LocalRot;
                break;
            case direction_def.Back:
                LocalPos = new Vector3(0.0f, 3.0f, -2.5f);
                LocalRot = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                wall = Instantiate(m_wall, cell_position + LocalPos, cell_quaternion);
                //wall = Instantiate(m_wall, cell_position, cell_quaternion);
                //wall.transform.localPosition = LocalPos;
                wall.transform.rotation = LocalRot;
                break;
            case direction_def.Left:
                LocalPos = new Vector3(-2.5f, 3.0f, 0.0f);
                LocalRot = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                wall = Instantiate(m_wall, cell_position + LocalPos, cell_quaternion);
                //wall = Instantiate(m_wall, cell_position, cell_quaternion);
                //wall.transform.localPosition = LocalPos;
                wall.transform.rotation = LocalRot;
                break;
            case direction_def.Right:
                LocalPos = new Vector3(2.5f, 3.0f, 0.0f);
                LocalRot = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                wall = Instantiate(m_wall, cell_position + LocalPos, cell_quaternion);
                //wall = Instantiate(m_wall, cell_position, cell_quaternion);
                //wall.transform.localPosition = LocalPos;
                wall.transform.rotation = LocalRot;
                break;
        }
    }

    Vector3 LocalPosBy2d(int input_x, int input_y)
    {
        float wall_len = 5;
        return new Vector3(input_x * wall_len, -0.2f, input_y * wall_len);
    }

    /* -------old------------------------------------------------------------- */

    //private float nowAngle = 0.0f;
    //private Vector2 nowPos = new Vector2(0, 0);
    //private GameObject m_first, m_line_v, m_turn_r, m_turn_l, m_trans_t;

    //void Awake()
    //{
    //    m_first = (GameObject)Resources.Load("Cell_holl_onlyf");
    //    m_line_v = (GameObject)Resources.Load("Cell_holl_v");
    //    m_turn_r = (GameObject)Resources.Load("Cell_holl_r");
    //    m_turn_l = (GameObject)Resources.Load("Cell_holl_l");
    //    m_trans_t = (GameObject)Resources.Load("Cell_trans_t");
    //}

    //private void makeFirstPlace()
    //{
    //    Vector3 direction = Quaternion.AngleAxis(nowAngle, Vector3.up) * Vector3.forward;
    //    Instantiate(m_first, LocalPosBy2d(nowPos), Quaternion.LookRotation(direction));
    //    UpdateNowPos();
    //}

    //private void makeTransT(int x, int y, float angle)
    //{
    //    Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
    //    Instantiate(m_trans_t, LocalPosBy2d(new Vector2((float)x, (float)y)), Quaternion.LookRotation(direction));
    //}

    //private void makeMapForwards(int? x = -1, int? y = -1)
    //{
    //    Vector3 direction = Quaternion.AngleAxis(nowAngle, Vector3.up) * Vector3.forward;

    //    if (x != -1 && y != -1)
    //    {
    //        Instantiate(m_line_v, LocalPosBy2d(new Vector2((float)x, (float)y)), Quaternion.LookRotation(direction));
    //    }
    //    else
    //    {
    //        Instantiate(m_line_v, LocalPosBy2d(nowPos), Quaternion.LookRotation(direction));
    //        UpdateNowPos();
    //    }
    //}
    //private void makeMapTurnRight(int? x = -1, int? y = -1)
    //{
    //    Vector3 direction = Quaternion.AngleAxis(nowAngle, Vector3.up) * Vector3.forward;
    //    nowAngle = Mathf.Repeat(nowAngle + 90, 360);
    //    if (x != -1 && y != -1)
    //    {
    //        Instantiate(m_turn_r, LocalPosBy2d(new Vector2((float)x, (float)y)), Quaternion.LookRotation(direction));
    //    }
    //    else
    //    {
    //        Instantiate(m_turn_r, LocalPosBy2d(nowPos), Quaternion.LookRotation(direction));
    //        UpdateNowPos();
    //    }
    //}

    //private void makeMapTurnLeft(int? x = -1, int? y = -1)
    //{
    //    Vector3 direction = Quaternion.AngleAxis(nowAngle, Vector3.up) * Vector3.forward;
    //    nowAngle = Mathf.Repeat(nowAngle + 270, 360);
    //    if (x != -1 && y != -1)
    //    {
    //        Instantiate(m_turn_l, LocalPosBy2d(new Vector2((float)x, (float)y)), Quaternion.LookRotation(direction));
    //    }
    //    else
    //    {
    //        Instantiate(m_turn_l, LocalPosBy2d(nowPos), Quaternion.LookRotation(direction));
    //        UpdateNowPos();
    //    }
    //}
    //private void UpdateNowPos()
    //{
    //    if (Mathf.Approximately(nowAngle, 0) || Mathf.Approximately(nowAngle, 360))
    //    {
    //        nowPos.y += 1;
    //    }
    //    else if (Mathf.Approximately(nowAngle, 90))
    //    {
    //        nowPos.x += 1;
    //    }
    //    else if (Mathf.Approximately(nowAngle, 180))
    //    {
    //        nowPos.y -= 1;
    //    }
    //    else if (Mathf.Approximately(nowAngle, 270))
    //    {
    //        nowPos.x -= 1;
    //    }
    //}
}
