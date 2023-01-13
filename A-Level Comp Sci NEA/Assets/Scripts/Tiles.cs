using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    [Serializable]
    public struct tile
    {
        public GameObject tileGO;
        public int min;
        public int max;
    }
    [Serializable]
    public struct specialTile
    {
        public int x;
        public int y;
        public int min;
        public int max;
    }
    [Serializable]
    public struct chest
    {
        public GameObject chestGO;
    }
    public List<tile> floor;
    public List<tile> wall;
    public List<specialTile> special;
    public chest ammo;
    public chest weapon;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
