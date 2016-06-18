using UnityEngine;
using System.Collections;

namespace LemonSpawn { 
public class BillboardClouds : LemonSpawn.Environment
    {

        public BillboardClouds(PlanetSettings ps) {
            planetSettings = ps;
            maxCount = 50;
            environmentTypes.Add(new EnvironmentType("PSystem", null, 300, 0.5f, 0.0f, 0.45f, 10000));
//            environmentTypes.Add(new EnvironmentType("PSystem", null));

            calculateMaxMaxDist();
        }

    }
}
