using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ProcGenTiles;



namespace Tests 
{
    public class NewTestScript
    {

        Map map;
        LayerTerrain lt;
        GameManager gm;


        [Test]
        public void Tile_69()
        {
            gm.GetComponent<GameManager>();

            gm.loadNewData();

            Tile t = map.GetTile(6, 9);
            Assert.AreEqual(t.is69, true);
        }


    }
}
