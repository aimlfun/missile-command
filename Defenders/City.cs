using MissileDefence.Controllers.Game;
using MissileDefence.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Defenders
{
    /// <summary>
    /// 
    /// </summary>
    internal class City : BasesBeingDefended
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly int imageWidthInPXallowingForScaling;

        /// <summary>
        /// 
        /// </summary>
        private readonly int imageHeightInPXallowingForScaling;

        /// <summary>
        /// 
        /// </summary>
        private bool visible = true;

        /// <summary>
        /// Setter/Getter whether city is visible or not.
        /// </summary>
        internal bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="horiz"></param>
        /// <param name="alt"></param>
        internal City(int horiz, int alt) : base(horiz, alt)
        {
            imageWidthInPXallowingForScaling = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].City.Width;
            imageHeightInPXallowingForScaling = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].City.Height;
            visible = true;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void BaseDestroyed()
        {
            base.BaseDestroyed();
        }

        /// <summary>
        /// Draws the city.
        /// </summary>
        /// <param name="g"></param>
        internal override void Draw(Graphics g)
        {
            base.Draw(g);

            // we "hide" when counting cities.
            if (!IsDestroyed && !visible) return;

            Point p = locationInMCcoordinates.MCCoordsToDeviceCoordinatesP();

            Bitmap city;

            if (IsDestroyed)
            {
                // city rendered solid floor colour
                city = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].CityDestroyed;
            }
            else
            {
                city = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].City;
            }

            g.DrawImageUnscaled(city, new Point(p.X - imageWidthInPXallowingForScaling / 2,
                                                p.Y - imageHeightInPXallowingForScaling / 2
                                                ));
        }
    }
}