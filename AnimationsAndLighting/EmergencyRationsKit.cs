﻿using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using System.Collections.Generic;
using VRage.ModAPI;
using Sandbox.ModAPI;
using VRageMath;
using Sandbox.Game.Entities;
using System;
using Sandbox.Game;
using Sandbox.Definitions;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.Game.Lights;
using Sandbox.Game.EntityComponents;

namespace Stollie.DailyNeeds
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), false, "EmergencyRationsKitSmall")]
    public class EmergencyRationsKit : MyGameLogicComponent
    {
        private int TranslationTimeLeftArm = 0;
        private int TranslationTimeRightArm = 0;

        private int RotationTimeGenerator = 0;

        private int AnimationLoopGenerator = 0;

        private bool playAnimation = true;
        private bool firstTime = true;
        private MyLight _light;
        public Dictionary<string, MyEntitySubpart> subparts;
        private static Guid ColorCheckStorageGUID = new Guid("0A9A3146-F8D1-40FD-A664-D0B9D071B0AC");

        private MyObjectBuilder_EntityBase objectBuilder = null;
        private IMyCubeBlock emergencyRationsKit = null;

        private static ConfigDataStore mConfigDataStore = new ConfigDataStore();
        private static bool RECOLOR_BLOCKS = true;
        
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                var _light = new MyLight();
                base.Init(objectBuilder);
                this.objectBuilder = objectBuilder;
                emergencyRationsKit = Entity as IMyCubeBlock;
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;

                mConfigDataStore.Load();
                RECOLOR_BLOCKS = mConfigDataStore.get_RECOLOR_BLOCKS();

                if (emergencyRationsKit.Storage == null)
                {
                    emergencyRationsKit.Storage = new MyModStorageComponent();
                }

            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Init Error" + e, 10000, "Red");
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return objectBuilder;
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                var isHost = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE ||
                             MyAPIGateway.Multiplayer.IsServer;

                var isDedicatedHost = isHost && MyAPIGateway.Utilities.IsDedicated;

                if (isDedicatedHost)
                    return;

                if (!emergencyRationsKit.Storage.ContainsKey(ColorCheckStorageGUID) && RECOLOR_BLOCKS)
                {
                    emergencyRationsKit.Storage[ColorCheckStorageGUID] = "ColorChanged";
                    emergencyRationsKit.CubeGrid.ColorBlocks(emergencyRationsKit.Min, emergencyRationsKit.Max, new Color(new Vector3(1.0f, 0.0f, 0.0f)).ColorToHSVDX11());
                }

                subparts = (emergencyRationsKit as MyEntity).Subparts;
                if (emergencyRationsKit.IsWorking)
                {
                    var lightColorRed = Color.YellowGreen.R;
                    var lightColorGreen = Color.YellowGreen.G;
                    var lightColorBlue = Color.YellowGreen.B;
                    var lightColorAlpha = Color.YellowGreen.A;

                    /*
                    var emEmissiveness = 0.5f;
                    var emColorRed = 0.604f;
                    var emColorGreen = 0.804f;
                    var emColorBlue = 0.196f;
                    var emColorAlpha = 1.0f;
                    */

                    //CreateLight((MyEntity)emergencyRationsKit, lightColorRed, lightColorGreen, lightColorBlue, lightColorAlpha);
                    //MyCubeBlockEmissive.SetEmissiveParts(emergencyRationsKit as MyEntity, emEmissiveness, Color.FromNonPremultiplied(new Vector4(emColorRed, emColorGreen, emColorBlue, emColorAlpha)), Color.White);
                    CreateLight((MyEntity)emergencyRationsKit, Color.Red);
                    
                    if (_light != null)
                    {
                        _light.LightOn = true;
                        _light.UpdateLight();
                    }
                    if (subparts != null)
                    {
                        foreach (var subpart in subparts)
                        {
                            subpart.Value.SetEmissiveParts("Emissive", Color.Green, 1.0f);
                        }
                    }
                    
                    RotateRationGenerator();
                }
                else
                {
                    if (_light != null)
                    {
                        _light.LightOn = false;
                        _light.UpdateLight();
                    }

                    if (subparts != null)
                    {
                        foreach (var subpart in subparts)
                        {
                            subpart.Value.SetEmissiveParts("Emissive", Color.Red, 1.0f);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.SendChatMessage("Update Error" + e.ToString());
            }
        }
        
        public void CreateLight(MyEntity entity, Color color)
        {
            //These control the light settings on spawn.
            var lightPosition = entity.WorldMatrix.Translation; //Sets the light to the center of the block you are spawning it on, if you need it elsehwere you will need help.
            var lightRange = 0.5f; //Range of light
            var lightIntensity = 5.0f; //Light intensity
            var lightFalloff = 1.5f; //Light falloff
            var lightAdjustment = new Vector3D(0.0f, 0.0f, 0.0f);

            if (emergencyRationsKit.BlockDefinition.SubtypeName.Contains("Small"))
            {
                lightRange = 0.4f;
                //lightAdjustment = emergencyRationsKit.WorldMatrix.Forward * 0.05;
            }

            if (_light == null)//Ignore - checks if there is a light and if not makes it.
            {
                _light = MyLights.AddLight(); //Ignore - adds the light to the games lighting system
                _light.Start(entity.WorldMatrix.Translation, color.ToVector4(), lightRange, ""); // Ignore- Determines the lights position, initial color and initial range.
                _light.Intensity = lightIntensity; //Ignore - sets light intensity from above values.
                _light.Falloff = lightFalloff; //Ignore - sets light fall off from above values.
                //_light.PointLightOffset = lightOffset; //Ignore - sets light offset from above values.
                _light.LightOn = true; //Ignore - turns light on
            }
            else
            {
                _light.Position = entity.WorldMatrix.Translation + lightAdjustment; //Updates the lights position constantly. You'll need help if you want it somewhere else.
                _light.UpdateLight(); //Ignore - tells the game to update the light.
            }
        }

        public void RotateRationGenerator()
        {
            try
            {
                if (subparts != null)
                {
                    foreach (var subpart in subparts)
                    {
                        if (subpart.Key == "EmergencyRationsKit_Generator")
                        {
                            var rotationX = 0.005f;
                            var rotationY = 0.005f;
                            var rotationZ = 0.005f;
                            var initialMatrix = subpart.Value.PositionComp.LocalMatrix;
                            var rotationMatrix = MatrixD.CreateRotationX(rotationX) * MatrixD.CreateRotationY(rotationY) * MatrixD.CreateRotationZ(rotationZ);
                            var matrix = rotationMatrix * initialMatrix;
                            subpart.Value.PositionComp.LocalMatrix = matrix;
                            AnimationLoopGenerator++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Animation Error" + e, 2500, "Red");
            }
        }

        public override void Close()
        {
            if (_light != null)
            {
                MyLights.RemoveLight(_light);
                _light = null;
            }
        }

    }
}