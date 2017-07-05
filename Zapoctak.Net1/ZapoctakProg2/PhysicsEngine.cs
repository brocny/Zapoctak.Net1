﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace ZapoctakProg2
{
    public class PhysicsEngine
    {
        public PhysicsEngine(int gravityConst, int minGravity, int maxGravity, double maxDistance, Level level)
        {
            this.minGravity = minGravity;
            this.maxGravity = maxGravity;
            this.level = level;
            this.gravityConst = gravityConst;
            this.suns = level.Suns;
            this.planets = level.Planets;
            this.powerUps = level.PowerUps;
            this.maxDistance = maxDistance;
        }

        private List<Planet> planets;
        private List<Sun> suns;
        private List<PowerUp> powerUps;
        private Level level;

        private double maxDistance;
        public double MaxDistance => maxDistance;

        private int maxGravity;
        public int MaxGravity => maxGravity;

        private int minGravity;
        public int MinGravity => minGravity;

        private int gravityConst;
        public int GravityConst
        {
            get { return gravityConst; }
            set { gravityConst = value; }
        }

        //performs one tick of the physics simulation
        public void Tick()
        {
            UpdatePlanetsAndPowerUps();
            CheckRadii();
            CheckCrashes();
            CheckDistanceFromSuns();
        }

        //destroys planets that have become too small
        //can't use foreach because it breaks if you remove one of the objects during the loop
        private void CheckRadii()
        {
            foreach (Planet planet in planets)
            {
                if (planet.Radius < Planet.MinimumRadius)
                {
                    planet.IsDestroyed = true;
                }
            }
        }


        //recalculates the positions of all the planets after one tick
        private void UpdatePlanetsAndPowerUps()
        {
            UpdatePositions();
            UpdateVelocities();
        }

        private void UpdatePositions()
        {
            Parallel.ForEach(planets, p => 
            {
                if (!p.IsDestroyed) p.UpdatePos();
            });
            Parallel.ForEach(powerUps, p =>
            {
                if (!p.IsDestroyed) p.UpdatePos();
            });
        }

        private void UpdateVelocities()
        {
            foreach (var sun in suns)
            {
                if (sun.IsDestroyed) continue;

                Parallel.ForEach(planets,
                    p =>
                    {
                        if (!p.IsDestroyed)
                        {
                            p.ApplyBurns(sun);
                            p.UpdateVel(sun, gravityConst);
                        }
                    });
                Parallel.ForEach(powerUps,
                    p => {
                        if(!p.IsDestroyed) p.UpdateVel(sun, gravityConst);
                    });
            }
        }


        //destroys planets that are not close enough to any of the suns
        private void CheckDistanceFromSuns()
        {
            for(int i = 0; i < planets.Count; i++)
            {
                if(planets[i].IsDestroyed) continue;
                var numSunsCloseEnough = 0;
                foreach (var sun in suns)
                {
                    if(sun.IsDestroyed) continue;
                    if (planets[i].DistanceTo(sun) <= maxDistance)
                        numSunsCloseEnough++;
                }
                if (numSunsCloseEnough == 0)
                {
                    planets[i].IsDestroyed = true;
                }

            }

            for (int i = 0; i < powerUps.Count; i++)
            {
                if(powerUps[i].IsDestroyed) continue;
                var numSunsCloseEnough = 0;
                foreach (var sun in suns)
                {
                    if(sun.IsDestroyed) continue;
                    if (powerUps[i].DistanceTo(sun) <= maxDistance)
                        numSunsCloseEnough++;
                }
                if (numSunsCloseEnough == 0)
                {
                    powerUps[i].ApplyTooFar(level);
                    powerUps[i].IsDestroyed = true;
                }
                
            }
            
        }

        //checks if any of the planets have collided with a sun or another planets and destroys them if so
        private void CheckCrashes()
        {

            CheckPowerUps();
            //again no foreach because of removing
            for (var i = 0; i < planets.Count; i++)
            {
                foreach (var sun in suns)
                {
                    if (planets[i].HasCrashedWith(sun))
                    {
                        planets[i].IsDestroyed = true;
                        break;
                    }
                } 
            }
                
            //no foreach - removing
            for (var i = 0; i < planets.Count - 1; i++)
            {
                if (planets[i].IsDestroyed) continue;

                for (var j = i + 1; j < planets.Count; j++)
                {
                    if (planets[j].IsDestroyed) continue;
                    
                    if (planets[i].HasCrashedWith(planets[j]))
                    {
                        planets[i].IsDestroyed = true;
                        planets[j].IsDestroyed = true;
                        break;
                    }
                }
            }
        }

        private void CheckPowerUps()
        {
            for (var i = 0; i < powerUps.Count; i++)
            {
                PowerUp powerUp = powerUps[i];
                if (powerUp.IsDestroyed) continue;
                foreach (Sun sun in suns)
                {
                    if (sun.IsDestroyed) continue;
                    if (powerUp.HasCrashedWith(sun))
                    {
                        powerUp.ApplySun(level, sun);
                        powerUp.IsDestroyed = true;
                        goto Next;
                    }
                }

                foreach (Planet planet in planets)
                {
                    if (planet.IsDestroyed) continue;
                    if (powerUp.HasCrashedWith(planet))
                    {
                        powerUp.ApplyPlanet(level, planet);
                        powerUp.IsDestroyed = true;
                        goto Next;
                    }
                }

                for (var j = i + 1; j < powerUps.Count; j++)
                {
                    if(powerUps[j].IsDestroyed) continue;
                    if (powerUp.HasCrashedWith(powerUps[j]))
                    {
                        powerUp.ApplyPowerup(level, powerUps[j]);
                        powerUp.IsDestroyed = true;
                        goto Next;
                    }
                }

                Next:;
            }
        }

    }
}
