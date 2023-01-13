using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    enum gridTitle {empty, floor, wall};
    gridTitle[,] grid;
    struct walker
    {
        public Vector2 dir, pos;
    }
    private List<walker> walkers;
    private Tiles tile;
    public List<GameObject> ammoCrates;
    public List<GameObject> weaponCrates;
    public GameObject ammoCrate;
    public GameObject weaponCrate;
    float furthestAmmoDist = 0;
    float furthestWeaponDist = 0;
    public GameObject furthestAmmoCrate;
    public GameObject furthestWeaponCrate;
    [Header("Generation Parameters")]
    public int roomHeight;
    public int roomWidth;
    public int walkerXClamp;
    public int walkerYClamp;
    public int maxWalkers;
    [Range(0, 1)] public float changeDirChance = 0.5f, spawnChance = 0.05f, destroyChance = 0.05f;
    [Range(0, 1)] public float percentageFill;
    public int maxItterations = 100000;

    void Start()
    {
        tile = gameObject.GetComponent<Tiles>();
        Refresh();
        GenerateFloor();
        GenerateWalls();
        CleanMap();
        DrawMap();
    }
    //Creates a new grid and initialises the walkers
    void Refresh()
    {
        Vector2 centre = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f), Mathf.RoundToInt(roomHeight / 2.0f));
        if (walkerXClamp.Equals(null) || walkerYClamp.Equals(null))
        {
            walkerXClamp = roomHeight - 2;
            walkerYClamp = roomWidth - 2;
        }
        grid = new gridTitle[roomWidth, roomHeight];
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                grid[x, y] = gridTitle.empty;
            }
        }
        walkers = new List<walker>();
        walker initWalker = new walker();
        initWalker.dir = ChangeDirection();
        initWalker.pos = centre;
        walkers.Add(initWalker);
    }
    //Generates all the floor tiles
    void GenerateFloor()
    {
        int itt = 0;
        do
        {
            foreach (walker myWalker in walkers)
            {
                int i = Random.Range(0, 101);
                for (int j = 0; j < tile.special.Count; j++)
                {
                    if (i >= tile.special[j].min && i <= tile.special[j].max)
                    {
                        for (int x = 0; x < tile.special[j].x; x++)
                        {
                            for (int y = 0; y < tile.special[j].y; y++)
                            {
                                grid[(int)myWalker.pos.x + x, (int)myWalker.pos.y + y] = gridTitle.floor;
                            }
                        }
                    }
                    else
                    {
                        grid[(int)myWalker.pos.x, (int)myWalker.pos.y] = gridTitle.floor;
                    }
                }
            }

            for (int i = walkers.Count - 1; i > 0; i--)
            {
                if (Random.value < destroyChance && walkers.Count > 1)
                {
                    ammoCrate = Instantiate(tile.ammo.chestGO, new Vector2(walkers[i].pos.x - 0.5f, walkers[i].pos.y - 0.5f), Quaternion.identity);
                    ammoCrates.Add(ammoCrate);
                    walkers.RemoveAt(i);
                    break;
                }
            }


            for (int i = 0; i < walkers.Count; i++)
            {
                if (Random.value < changeDirChance)
                {
                    walker thisWalker = walkers[i];
                    thisWalker.dir = ChangeDirection();
                    walkers[i] = thisWalker;
                }
            }

            int walkerCount = walkers.Count;
            for (int i = 0; i < walkerCount; i++)
            {
                if (Random.value < spawnChance && walkers.Count < maxWalkers)
                {
                    walker newWalker = new walker();
                    newWalker.dir = ChangeDirection();
                    newWalker.pos = walkers[i].pos;
                    walkers.Add(newWalker);
                }
            }

            for (int i = 0; i < walkers.Count; i++)
            {
                walker thisWalker = walkers[i];
                thisWalker.pos += thisWalker.dir;
                walkers[i] = thisWalker;
            }

            for (int i = 0; i < walkers.Count; i++)
            {
                walker thisWalker = walkers[i];
                thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 1, walkerXClamp);
                thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 1, walkerYClamp);
                walkers[i] = thisWalker;
            }
            if (((float)NumberOfFloors() / (float)grid.Length) > percentageFill)
            {
                break;
            }
            itt++;
        } while (itt < maxItterations);

    }

    //Generates All Of The Walls
    void GenerateWalls()
    {
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                if (grid[x, y] == gridTitle.floor)
                {
                    if (grid[x, y + 1] == gridTitle.empty)
                    {
                        grid[x, y + 1] = gridTitle.wall;
                    }
                    if (grid[x, y - 1] == gridTitle.empty)
                    {
                        grid[x, y - 1] = gridTitle.wall;
                    }
                    if (grid[x + 1, y] == gridTitle.empty)
                    {
                        grid[x + 1, y] = gridTitle.wall;
                    }
                    if (grid[x - 1, y] == gridTitle.empty)
                    {
                        grid[x - 1, y] = gridTitle.wall;
                    }
                }
            }
        }
    }

    //Removes All 1 Block walls inside of the map
    void CleanMap()
    {
        GenerateWeaponCrates();
        Vector2 centre = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f), Mathf.RoundToInt(roomHeight / 2.0f));
        for (int i = 0; i < ammoCrates.Count; i++)
        {
            float curDist;
            curDist = Vector2.Distance(centre, ammoCrates[i].transform.position);
            if (curDist > furthestAmmoDist)
            {
                furthestAmmoCrate = ammoCrates[i];
                furthestAmmoDist = curDist;
            }
        }

        for (int i = 0; i < ammoCrates.Count; i++)
        {
            float curDist;
            curDist = Vector2.Distance(centre, ammoCrates[i].transform.position);
            if (curDist != furthestAmmoDist)
            {
                GameObject crateToDestroy = ammoCrates[i];
                Destroy(crateToDestroy);
            }
        }
        ammoCrates.Clear();
        ammoCrates.Add(furthestAmmoCrate);
        
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                if (grid[x,y] == gridTitle.wall)
                {
                    bool singleWall = true;

                    for (int checkX = -1; checkX <= 1; checkX++)
                    {
                        for (int checkY = -1; checkY <= 1; checkY++)
                        {
                            if (x + checkX < 0 || x + checkX > roomWidth - 1 ||
                                y + checkY < 0 || y + checkY > roomHeight - 1)
                            {
                                continue;
                            }
                            if ((checkX != 0 && checkY != 0) || (checkX == 0 && checkY == 0))
                            {
                                continue;
                            }
                            if (grid[x + checkX, y + checkY] != gridTitle.floor)
                            {
                                singleWall = false;
                            }
                        }
                    }
                    if (singleWall)
                    {
                        grid[x, y] = gridTitle.floor;
                    }
                }
            }
        }   
    }
    //Draws the assigned grid onto the screen
    void DrawMap()
    {
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                switch (grid[x, y])
                {
                    case gridTitle.empty:
                        break;
                    case gridTitle.floor:
                        int i = Random.Range(0, 101);
                        for (int j = 0; j < tile.floor.Count; j++)
                        {
                            if (i >= tile.floor[j].min && i <= tile.floor[j].max)
                            {
                                Spawn(x, y, tile.floor[j].tileGO);
                            }
                        }
                        break;
                    case gridTitle.wall:
                        int z = Random.Range(0, 101);
                        for (int j = 0; j < tile.wall.Count; j++)
                        {
                            if (z >= tile.wall[j].min && z <= tile.wall[j].max)
                            {
                                Spawn(x, y, tile.wall[j].tileGO);
                            }
                        }
                        break;
                }
            }
        }
    }
    //Generates Weapon Crates
    void GenerateWeaponCrates()
    {
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                if (grid[x, y] == gridTitle.floor && (grid[x - 1, y] == gridTitle.wall || grid[x + 1, y] == gridTitle.wall) && (grid[x, y - 1] == gridTitle.wall || grid[x, y + 1] == gridTitle.wall))
                {
                    GameObject weaponCratos = Instantiate(weaponCrate, new Vector2(x - 0.5f, y - 0.5f), Quaternion.identity);
                    weaponCrates.Add(weaponCratos);
                }
            }
        }
        
        Vector2 centre = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f), Mathf.RoundToInt(roomHeight / 2.0f));
        for (int i = 0; i < weaponCrates.Count; i++)
        {
            float curDist;
            curDist = Vector2.Distance(centre, weaponCrates[i].transform.position);
            if (curDist > furthestWeaponDist)
            {
                furthestWeaponCrate = weaponCrates[i];
                furthestWeaponDist = curDist;
            }
        }

        for (int i = 0; i < weaponCrates.Count; i++)
        {
            float curDist;
            curDist = Vector2.Distance(centre, weaponCrates[i].transform.position);
            if (curDist != furthestWeaponDist )
            {
                GameObject crateToDestroy = weaponCrates[i];
                Destroy(crateToDestroy);
            }
        }
        weaponCrates.Clear();
        weaponCrates.Add(furthestWeaponCrate);
        
    }
    Vector2 ChangeDirection()
    {
        int choice = Mathf.FloorToInt(Random.value * 3.99f);
        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            default:
                return Vector2.right;
        }
    }

    int NumberOfFloors()
    {
        int count = 0;
        foreach (gridTitle space in grid)
        {
            if (space == gridTitle.floor)
            {
                count++;
            }
        }
        return count;
    }
    void Spawn(float x, float y, GameObject toSpawn)
    {
        Vector2 offset = new Vector2(0.5f,0.5f);
        Vector2 spawnPos = new Vector2(x, y) - offset;
        Instantiate(toSpawn, spawnPos, Quaternion.identity);
    }
}
