/* The MIT License (MIT)
 *
 * Copyright (c) 2015 Mhoram Kerbin
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PB_MainMenuSkipper
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class MainMenuSkipper : MonoBehaviour
    {
        private static bool hasRun = false;
        private class MmsConfig
        {
            public bool active { get; set; }
            public string saveGameName { get; set; }
            public string sceneToLoad { get; set; }
        }

        public void Start()
        {
            if (hasRun)
            {
                return;
            }
            hasRun = true;
            try
            {
                var cfg = GameDatabase.Instance.GetConfigNodes("MAIN_MENU_SKIPPER_CONFIG").FirstOrDefault();
                if (cfg == null)
                {
                    mes("MainMenuSkipper: Could not load Confignode");
                    return;
                }
                MmsConfig mmsConfig = ResourceUtilities.LoadNodeProperties<MmsConfig>(cfg);
                if (mmsConfig == null)
                {
                    mes("MainMenuSkipper: Could not get Confignode properties");
                    return;
                }
                if (!mmsConfig.active)
                {
                    mes("MainMenuSkipper: Deactivating based on configuration", false);
                    return;
                }
                loadIt(mmsConfig);
            }
            catch (System.Exception x)
            {
                mes("Exception in MainMenuSkipper: " + x.ToString());
            }
        }

        private void loadIt(MmsConfig mmsConfig)
        {
            HighLogic.SaveFolder = mmsConfig.saveGameName;
            Game game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);
            if (game == null)
            {
                mes("MainMenuSkipper: Savegame could not be loaded. Please doublecheck the name of the savegame and search the logfile for errors."); 
                return;
            }
            if (!game.compatible)
            {
                mes("MainMenuSkipper: Savegame is not compatible.");
                return;
            }

            HighLogic.CurrentGame = game;

            switch (mmsConfig.sceneToLoad)
            {
                case "SpaceCenter":
                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                    break;
                case "TrackingStation":
                    HighLogic.LoadScene(GameScenes.TRACKSTATION);
                    break;
                case "FirstVessel":
                    loadFirstVessel(game);
                    break;
                default:
                    mes("MainMenuSkipper: Config Option for scene not recognized.");
                    break;
            }
        }
        private void loadFirstVessel(Game game)
        {
            int vesselId = 0;
            while (vesselId < game.flightState.protoVessels.Count)
            {
                if (game.flightState.protoVessels[vesselId].vesselType == VesselType.SpaceObject ||
                    game.flightState.protoVessels[vesselId].vesselType == VesselType.Unknown)
                {
                    vesselId++;
                }
                else
                {
                    FlightDriver.StartAndFocusVessel(game, vesselId);
                    return;
                }
            }
            mes("MainMenuSkipper: No Vessel found... Loading SpaceCenter");
            HighLogic.LoadScene(GameScenes.SPACECENTER);
        }

        private void mes(string txt, bool screen = true)
        {
            Debug.Log(txt);
            if (screen)
            {
                ScreenMessages.PostScreenMessage(txt, 5, ScreenMessageStyle.UPPER_RIGHT);
            }
        }
    }
}
