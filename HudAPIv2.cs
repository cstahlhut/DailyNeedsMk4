using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Draygo.API
{
    public class HudAPIv2
    {
        private static HudAPIv2 instance;
        private const long REGISTRATIONID = 573804956;
        private bool registered = false;

        private Func<int, object> MessageFactory;
        private Action<object, int, object> MessageSetter;
        private Func<object, int, object> MessageGetter;
        private Action<object> RemoveMessage;

        public enum TextOrientation : byte
        {
            ltr = 1,
            center = 2,
            rtl = 3
        }

        //call in init. 
        public HudAPIv2()
        {
            if (instance != null)
            {

                return;
            }
            instance = this;

            MyAPIGateway.Utilities.RegisterMessageHandler(REGISTRATIONID, RegisterComponents);
        }

        public void Close()
        {
            Unload();
        }
        public void Unload()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(REGISTRATIONID, RegisterComponents);
            MessageFactory = null;
            MessageSetter = null;
            MessageGetter = null;
            RemoveMessage = null;
            registered = false;
        }

        private void RegisterComponents(object obj)
        {
            if (registered)
                return;
            if (obj is MyTuple<Func<int, object>, Action<object, int, object>, Func<object, int, object>, Action<object>>)
            {
                var Handlers = (MyTuple<Func<int, object>, Action<object, int, object>, Func<object, int, object>, Action<object>>)obj;
                MessageFactory = Handlers.Item1;
                MessageSetter = Handlers.Item2;
                MessageGetter = Handlers.Item3;
                RemoveMessage = Handlers.Item4;
                registered = true;
            }
        }

        /// <summary>
        /// If Heartbeat is true you may call any constructor in this class. Do not call any constructor or set properties if this is false.
        /// </summary>
        public bool Heartbeat
        {
            get
            {
                return registered;
            }
        }


        #region Intercomm
        private void DeleteMessage(object BackingObject)
        {
            if (BackingObject != null)
                RemoveMessage(BackingObject);
        }
        private object CreateMessage(MessageTypes type)
        {
            return MessageFactory((int)type);
        }
        private object MessageGet(object BackingObject, int Member)
        {
            if (BackingObject != null)
                return MessageGetter(BackingObject, Member);
            else
                return null;
        }
        private void MessageSet(object BackingObject, int Member, object Value)
        {
            if (BackingObject != null)
                MessageSetter(BackingObject, Member, Value);
        }
        private void RegisterCheck()
        {
            if (instance.registered == false)
            {
                throw new InvalidOperationException("HudAPI: Failed to create backing object. Do not instantiate without checking if heartbeat is true.");
            }
        }
        #endregion

        private enum MessageTypes : int
        {
            HUDMessage = 0,
            BillBoardHUDMessage,
            EntityMessage,
            SpaceMessage
        }


        public enum Options : byte
        {
            None = 0x0,
            HideHud = 0x1,
            Shadowing = 0x2,
            Fixed = 0x4
        }

        private enum MessageBaseMembers : int
        {
            Message = 0,
            Visible,
            TimeToLive,
            Scale,
            TextLength,
            Offset,
            BlendType
        }

        public abstract class MessageBase
        {
            internal object BackingObject;

            #region Properties
            /// <summary>
            /// Note that if you update the stringbuilder anywhere it will update the message automatically. Use this property to set the stringbuilder object to your own or use the one generated by the constructor.
            /// </summary>
            public StringBuilder Message
            {
                get
                {
                    return (StringBuilder)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.Message));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)MessageBaseMembers.Message, value);
                }
            }


            /// <summary>
            /// True if HUD Element is visible, note that this will still be true if the player has their hud activated and HideHud option is set. 
            /// </summary>
            public bool Visible
            {
                get
                {
                    return (bool)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.Visible));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)MessageBaseMembers.Visible, value);
                }
            }

            /// <summary>
            /// Time to live in Draw ticks. At 0 class will close itself and will no longer update.
            /// </summary>
            public int TimeToLive
            {
                get
                {
                    return (int)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.TimeToLive));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)MessageBaseMembers.TimeToLive, value);
                }
            }


            /// <summary>
            /// Scale of the text elements or billboard
            /// </summary>
            public double Scale
            {
                get
                {
                    return (double)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.Scale));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)MessageBaseMembers.Scale, value);
                }
            }


            /// <summary>
            /// Offset the text element by this amount. Note this takes the result of GetTextLength, be sure to clear Offset.Y if you do not want to start at the lower left corner of the previous element
            /// </summary>
            public Vector2D Offset
            {
                get
                {
                    return (Vector2D)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.Offset));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)MessageBaseMembers.Offset, value);
                }
            }

            /// <summary>
            /// put using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum; on top of your script to use this property.
            /// </summary>
            public BlendTypeEnum Blend
            {
                get
                {
                    return (BlendTypeEnum)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.BlendType));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)MessageBaseMembers.BlendType, value);
                }
            }
            #endregion

            public abstract void DeleteMessage();

            /// <summary>
            /// Gets the offset of the lower right corner of the text element from the upper left. The value returned is a local translation. Screen space for screen messages, world space for world messages. Please note that the Y value is negative in screen space. 
            /// </summary>
            /// <returns>Lower Right Corner</returns>
            public Vector2D GetTextLength()
            {
                return (Vector2D)(instance.MessageGet(BackingObject, (int)MessageBaseMembers.TextLength));
            }
        }
        public class EntityMessage : MessageBase
        {
            private enum EntityMembers : int
            {
                Entity = 10,
                LocalPosition,
                Up,
                Forward,
                Orientation,
                Max,
                TransformMatrix
            }

            #region Properties
            /// <summary>
            /// Entity text will be centered on / attached to. 
            /// </summary>
            public IMyEntity Entity
            {
                get
                {
                    return instance.MessageGet(BackingObject, (int)EntityMembers.Entity) as IMyEntity;
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Entity, value);
                }
            }


            /// <summary>
            /// Local translation of where the text will be in relation to the Entity it is attached to. Used to construct the TransformMatrix
            /// </summary>
            public Vector3D LocalPosition
            {
                get
                {
                    return (Vector3D)instance.MessageGet(BackingObject, (int)EntityMembers.LocalPosition);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.LocalPosition, value);
                }
            }

            /// <summary>
            ///  Up, value used to construct the TransformMatrix
            /// </summary>
            public Vector3D Up
            {
                get
                {
                    return (Vector3D)instance.MessageGet(BackingObject, (int)EntityMembers.Up);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Up, value);
                }
            }

            /// <summary>
            /// Forward, value used to construct the TransformMatrix
            /// </summary>
            public Vector3D Forward
            {
                get
                {
                    return (Vector3D)instance.MessageGet(BackingObject, (int)EntityMembers.Forward);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Forward, value);
                }
            }

            /// <summary>
            /// Flag that sets from what direction text is written
            /// </summary>
            public TextOrientation Orientation
            {
                get
                {
                    return (TextOrientation)instance.MessageGet(BackingObject, (int)EntityMembers.Orientation);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Orientation, (byte)value);
                }
            }


            /// <summary>
            /// World Boundries
            /// </summary>
            public Vector2D Max
            {
                get
                {
                    return (Vector2D)instance.MessageGet(BackingObject, (int)EntityMembers.Max);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Max, value);
                }
            }

            /// <summary>
            /// Sets the transformation matrix directly, use instead of LocalPosition, Up, Forward
            /// </summary>
            public MatrixD TransformMatrix
            {
                get
                {
                    return (MatrixD)instance.MessageGet(BackingObject, (int)EntityMembers.TransformMatrix);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.TransformMatrix, value);
                }
            }
            #endregion
            public EntityMessage(StringBuilder Message, IMyEntity Entity, MatrixD TransformMatrix, int TimeToLive = -1, double Scale = 1, TextOrientation Orientation = TextOrientation.ltr, Vector2D? Offset = null, Vector2D? Max = null)
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.EntityMessage);
                if (BackingObject != null)
                {
                    if (Max.HasValue)
                        this.Max = Max.Value;
                    this.Message = Message;
                    this.Entity = Entity;
                    this.TransformMatrix = TransformMatrix;
                    this.TimeToLive = TimeToLive;
                    this.Scale = Scale;
                    this.Visible = true;
                    this.Orientation = Orientation;
                    if (Offset.HasValue)
                    {
                        this.Offset = Offset.Value;
                    }
                    else
                    {
                        this.Offset = Vector2D.Zero;
                    }

                }

            }
            public EntityMessage(StringBuilder Message, IMyEntity Entity, Vector3D LocalPosition, Vector3D Forward, Vector3D Up, int TimeToLive = -1, double Scale = 1, TextOrientation Orientation = TextOrientation.ltr, Vector2D? Offset = null, Vector2D? Max = null, BlendTypeEnum Blend = BlendTypeEnum.Standard)
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.EntityMessage);
                if (BackingObject != null)
                {
                    if (Max.HasValue)
                        this.Max = Max.Value;
                    this.Message = Message;
                    this.Entity = Entity;
                    this.LocalPosition = LocalPosition;
                    this.Forward = Forward;
                    this.Up = Up;
                    this.TimeToLive = TimeToLive;
                    this.Scale = Scale;
                    this.Visible = true;
                    this.Orientation = Orientation;
                    this.Blend = Blend;
                    if (Offset.HasValue)
                    {
                        this.Offset = Offset.Value;
                    }
                    else
                    {
                        this.Offset = Vector2D.Zero;
                    }

                }

            }

            public EntityMessage()
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.EntityMessage);

            }

            /// <summary>
            /// Do not use this class after deleting it. 
            /// </summary>
            public override void DeleteMessage()
            {
                instance.DeleteMessage(BackingObject);
                BackingObject = null;
            }
        }



        public class HUDMessage : MessageBase
        {
            private enum EntityMembers : int
            {
                Origin = 10,
                Options,
                ShadowColor,
            }
            #region Properties
            /// <summary>
            /// top left is -1, 1, bottom right is 1 -1
            /// </summary>
            public Vector2D Origin
            {
                get
                {
                    return (Vector2D)(instance.MessageGet(BackingObject, (int)EntityMembers.Origin));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Origin, value);
                }
            }


            /// <summary>
            /// HideHud - hides when hud is hidden, shadow draw a shadow behind the text. 
            /// </summary>
            public Options Options
            {
                get
                {
                    return (Options)(instance.MessageGet(BackingObject, (int)EntityMembers.Options));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Options, (byte)value);
                }
            }

            /// <summary>
            /// Color of shadow behind the text
            /// </summary>
            public Color ShadowColor
            {
                get
                {
                    return (Color)(instance.MessageGet(BackingObject, (int)EntityMembers.ShadowColor));
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.ShadowColor, value);
                }
            }
            #endregion

            public HUDMessage(StringBuilder Message, Vector2D Origin, Vector2D? Offset = null, int TimeToLive = -1, double Scale = 1.0d, bool HideHud = true, bool Shadowing = false, Color? ShadowColor = null, BlendTypeEnum Blend = BlendTypeEnum.SDR)
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.HUDMessage);
                if (BackingObject != null)
                {
                    this.TimeToLive = TimeToLive;
                    this.Origin = Origin;
                    this.Options = Options.None;
                    if (HideHud)
                        Options |= Options.HideHud;
                    if (Shadowing)
                        Options |= Options.Shadowing;
                    var blackshadow = Color.Black;
                    if (ShadowColor.HasValue)
                        ShadowColor = ShadowColor.Value;
                    this.Scale = Scale;
                    this.Message = Message;
                    this.Blend = Blend;
                    if (Offset.HasValue)
                    {
                        this.Offset = Offset.Value;
                    }
                    else
                    {
                        this.Offset = Vector2D.Zero;
                    }
                }
            }
            public HUDMessage()
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.HUDMessage);
            }

            public override void DeleteMessage()
            {
                instance.DeleteMessage(BackingObject);
                BackingObject = null;
            }

        }
        public class BillBoardHUDMessage : MessageBase
        {

            private enum EntityMembers : int
            {
                Origin = 10,
                Options,
                BillBoardColor,
                Material,
                Rotation,
                Width,
                Height
            }

            #region Properties
            /// <summary>
            /// top left is -100, 100, bottom right is 100 -100
            /// </summary>
            public Vector2D Origin
            {
                get
                {
                    return (Vector2D)instance.MessageGet(BackingObject, (int)EntityMembers.Origin);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Origin, value);
                }
            }

            /// <summary>
            /// Use MyStringId.GetOrCompute to turn a string into a MyStringId. 
            /// </summary>
            public MyStringId Material
            {
                get
                {
                    return (MyStringId)instance.MessageGet(BackingObject, (int)EntityMembers.Material);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Material, value);
                }
            }


            /// <summary>
            /// Set Options, HideHud to true will hide billboard when hud is hidden. Shadowing will draw the element on the shadow layer (behind the text layer)
            /// </summary>
            public Options Options
            {
                get
                {
                    return (Options)instance.MessageGet(BackingObject, (int)EntityMembers.Options);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Options, (byte)value);
                }
            }


            /// <summary>
            /// Sets the color mask of the billboard, not all billboards support this parameter. 
            /// </summary>
            public Color BillBoardColor
            {
                get
                {
                    return (Color)instance.MessageGet(BackingObject, (int)EntityMembers.BillBoardColor);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.BillBoardColor, value);
                }
            }

            /// <summary>
            /// Rotate billboard in radians.
            /// </summary>
            public float Rotation
            {
                get
                {
                    return (float)instance.MessageGet(BackingObject, (int)EntityMembers.Rotation);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Rotation, value);
                }
            }


            /// <summary>
            /// Multiplies the width of the billboard by this amount. Set Scale to 1 if you want to use this to finely control the width of the billboard, such as a value from GetTextLength
            /// You might need to multiply the result of GetTextLength by 250 or maybe 500 if Scale is 1. Will need experiementing
            /// </summary>
            public float Width
            {
                get
                {
                    return (float)instance.MessageGet(BackingObject, (int)EntityMembers.Width);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Width, value);
                }
            }


            /// <summary>
            /// Multiplies the height of the billboard by this amount. Set Scale to 1 if you want to use this to finely control the height of the billboard, such as a value from GetTextLength
            /// </summary>
            public float Height
            {
                get
                {
                    return (float)instance.MessageGet(BackingObject, (int)EntityMembers.Height);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Height, value);
                }
            }
            #endregion

            public BillBoardHUDMessage(MyStringId Material, Vector2D Origin, Color BillBoardColor, Vector2D? Offset = null, int TimeToLive = -1, double Scale = 1d, float Width = 1f, float Height = 1f, float Rotation = 0, bool HideHud = true, bool Shadowing = true, BlendTypeEnum Blend = BlendTypeEnum.SDR)
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.BillBoardHUDMessage);

                if (BackingObject != null)
                {
                    this.TimeToLive = TimeToLive;
                    this.Origin = Origin;
                    this.Options = Options.None;
                    if (HideHud)
                        this.Options |= Options.HideHud;
                    if (Shadowing)
                        this.Options |= Options.Shadowing;
                    this.BillBoardColor = BillBoardColor;
                    this.Scale = Scale;
                    this.Material = Material;
                    this.Rotation = Rotation;
                    this.Blend = Blend;
                    if (Offset.HasValue)
                    {
                        this.Offset = Offset.Value;
                    }
                    else
                    {
                        this.Offset = Vector2D.Zero;
                    }
                    this.Width = Width;
                    this.Height = Height;
                }


            }

            public BillBoardHUDMessage()
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.BillBoardHUDMessage);
            }

            public override void DeleteMessage()
            {
                instance.DeleteMessage(BackingObject);
                BackingObject = null;
            }
        }
        public class SpaceMessage : MessageBase
        {
            private enum EntityMembers : int
            {
                WorldPosition = 10,
                Up,
                Left,
                TxtOrientation

            }
            #region Properties
            /// <summary>
            /// Position
            /// </summary>
            public Vector3D WorldPosition
            {
                get
                {
                    return (Vector3D)instance.MessageGet(BackingObject, (int)EntityMembers.WorldPosition);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.WorldPosition, value);
                }
            }


            /// <summary>
            /// Up vector for textures
            /// </summary>
            public Vector3D Up
            {
                get
                {
                    return (Vector3D)instance.MessageGet(BackingObject, (int)EntityMembers.Up);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Up, value);
                }
            }


            /// <summary>
            /// Left Vector for Textures
            /// </summary>
            public Vector3D Left
            {
                get
                {
                    return (Vector3D)instance.MessageGet(BackingObject, (int)EntityMembers.Left);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.Left, value);
                }
            }


            /// <summary>
            /// Text orientation, from what edge text is aligned.
            /// </summary>
            public TextOrientation TxtOrientation
            {
                get
                {
                    return (TextOrientation)instance.MessageGet(BackingObject, (int)EntityMembers.TxtOrientation);
                }
                set
                {
                    instance.MessageSet(BackingObject, (int)EntityMembers.TxtOrientation, (byte)value);
                }
            }
            #endregion


            public SpaceMessage(StringBuilder Message, Vector3D WorldPosition, Vector3D Up, Vector3D Left, double Scale = 1, Vector2D? Offset = null, int TimeToLive = -1, TextOrientation TxtOrientation = TextOrientation.ltr, BlendTypeEnum Blend = BlendTypeEnum.Standard)
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.SpaceMessage);
                if (BackingObject != null)
                {
                    this.TimeToLive = TimeToLive;
                    this.Scale = Scale;
                    this.WorldPosition = WorldPosition;
                    this.Up = Up;
                    this.Left = Left;
                    this.TxtOrientation = TxtOrientation;
                    this.Message = Message;
                    this.Blend = Blend;
                    if (Offset.HasValue)
                    {
                        this.Offset = Offset.Value;
                    }
                    else
                    {
                        this.Offset = Vector2D.Zero;
                    }
                }

            }

            public SpaceMessage()
            {
                instance.RegisterCheck();
                BackingObject = instance.CreateMessage(MessageTypes.SpaceMessage);
            }

            public override void DeleteMessage()
            {
                instance.DeleteMessage(BackingObject);
                BackingObject = null;
            }
        }
    }
}