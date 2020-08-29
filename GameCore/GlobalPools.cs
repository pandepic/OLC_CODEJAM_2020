using GameCore.Entities;
using PandaMonogame;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public static class GlobalPools
    {
        public static ObjectPool<Asteroid> Asteroids = new ObjectPool<Asteroid>(10000);
        public static ObjectPool<Ship> Ships = new ObjectPool<Ship>(2000);
    }
}
