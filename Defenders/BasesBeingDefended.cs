using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.UX;

namespace MissileDefence.Defenders
{
    /// <summary>
    /// 
    /// </summary>
    internal class BasesBeingDefended
    {
        /// <summary>
        /// Where the base is located
        /// </summary>
        protected PointA locationInMCcoordinates;

        /// <summary>
        /// 
        /// </summary>
        internal PointA LocationInMCCoordinates
        {
            get { return locationInMCcoordinates; }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool isDestroyed = false;

        /// <summary>
        /// 
        /// </summary>
        internal bool IsDestroyed
        {
            get
            {
                return isDestroyed;
            }
            set
            {
                isDestroyed = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal virtual void BaseDestroyed()
        {
            if (isDestroyed) return;

            isDestroyed = true;
        }

        /// <summary>
        /// Constructor. Assigns a location to the base, and adds to list of bases.
        /// </summary>
        /// <param name="x"></param>
        internal BasesBeingDefended(int horizInMCCoordinates, int altitudeInMCCoordinates)
        {
            locationInMCcoordinates = new PointA(horizInMCCoordinates, altitudeInMCCoordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        internal virtual void Draw(Graphics g)
        {
        }
    }
}