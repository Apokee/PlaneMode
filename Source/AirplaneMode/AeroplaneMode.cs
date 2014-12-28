using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using Toolbar;

namespace AeroplaneMode
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]

    public class AeroplaneMode : MonoBehaviour
    {
        public static KeyBinding TOGGLE_CONTROL_MODE = new KeyBinding(KeyCode.ScrollLock);
        public static KeyBinding HOLD_CONTROL_MODE = new KeyBinding(KeyCode.Home);

        private bool pitch_invert;
        private bool control_mode_state;

        private bool toolbar_installed;
        private IButton control_mode_button;

        private ScreenMessage control_mode_message_aeroplane = new ScreenMessage("Aeroplane mode", 5, ScreenMessageStyle.LOWER_CENTER);
        private ScreenMessage control_mode_message_rocket = new ScreenMessage("Rocket mode", 5, ScreenMessageStyle.LOWER_CENTER);

        internal AeroplaneMode()
        {

            if (ToolbarManager.ToolbarAvailable)
            {
                toolbar_installed = true;

                control_mode_button = ToolbarManager.Instance.add("AeroplaneMode", "control_mode_button");
                control_mode_button.TexturePath = "AeroplaneMode/rocket_mode";
                control_mode_button.ToolTip = "Toggle Aeroplane Mode";

                control_mode_button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);

                control_mode_button.OnClick += (e) =>
                {
                    control_mode_state = !control_mode_state;

                    ScreenMessages.RemoveMessage(control_mode_message_aeroplane);
                    ScreenMessages.RemoveMessage(control_mode_message_rocket);

                    update_interface();

                    if (control_mode_state)
                    {                        
                        ScreenMessages.PostScreenMessage(control_mode_message_aeroplane, true);
                    }

                    else
                    {
                        ScreenMessages.PostScreenMessage(control_mode_message_rocket, true);
                    }
                };
            }

            else
            {
                toolbar_installed = false;
            }
        }

        public void Start()
        {
            pitch_invert = false;
            control_mode_state = false;

            load_key_config();
            
            FlightGlobals.ActiveVessel.OnFlyByWire += toggle_control_mode;
        }

        public void Update()
        {
            if ( (TOGGLE_CONTROL_MODE.GetKeyDown()) ||
                 (HOLD_CONTROL_MODE.GetKeyDown()) ||
                 (HOLD_CONTROL_MODE.GetKeyUp()) )                
            {
                control_mode_state = !control_mode_state;
                update_interface();
            }
        }

        private void load_key_config()
        {
            try
            {
                foreach (ConfigNode config in GameDatabase.Instance.GetConfigNodes("aeroplane_mode_config"))
                {
                    if (config.HasNode("TOGGLE_CONTROL_MODE"))
                    {
                        TOGGLE_CONTROL_MODE.Load(config.GetNode("TOGGLE_CONTROL_MODE"));
                    }

                    if (config.HasNode("HOLD_CONTROL_MODE"))
                    {
                        HOLD_CONTROL_MODE.Load(config.GetNode("HOLD_CONTROL_MODE"));
                    }

                    if (config.HasValue("pitch_invert"))
                    {
                        pitch_invert = bool.Parse(config.GetValue("pitch_invert"));
                    }
                }
            }

            catch (Exception e)
            {
                Debug.Log("Config file loading failed: " + e.ToString());
            }
        }

        private void update_interface()
        {
            ScreenMessages.RemoveMessage(control_mode_message_aeroplane);
            ScreenMessages.RemoveMessage(control_mode_message_rocket);

            if (control_mode_state)
            {
                ScreenMessages.PostScreenMessage(control_mode_message_aeroplane, true);
                if (toolbar_installed) control_mode_button.TexturePath = "AeroplaneMode/aero_mode";             
            }

            else
            {
                ScreenMessages.PostScreenMessage(control_mode_message_rocket, true);
                if (toolbar_installed) control_mode_button.TexturePath = "AeroplaneMode/rocket_mode"; 
            }
        }

        private void toggle_control_mode(FlightCtrlState control_state)
        {            
            float pitch, yaw, roll;

            if (control_mode_state)
            {
                yaw = control_state.yaw;
                roll = control_state.roll;
                pitch = control_state.pitch;

                if ((yaw != 0) || (roll != 0) || ((pitch != 0) && pitch_invert))
                {
                    FlightGlobals.ActiveVessel.Autopilot.SAS.ManualOverride(true);
                    
                    control_state.yaw = roll;
                    control_state.roll = yaw;

                    if (pitch_invert) control_state.pitch = -pitch;
                }

                else
                {
                    FlightGlobals.ActiveVessel.Autopilot.SAS.ManualOverride(false);
                }
            }
        }

        public void OnDestroy()
        {
            if (toolbar_installed) control_mode_button.Destroy();
            FlightGlobals.ActiveVessel.OnFlyByWire -= toggle_control_mode;                
        }
    }
}