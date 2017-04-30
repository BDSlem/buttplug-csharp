﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using Buttplug.Messages;

namespace Buttplug.Devices
{
    class XInputGamepadDevice : IButtplugDevice
    {
        Controller Device;
        public String Name { get; }

        public XInputGamepadDevice(Controller d)
        {
            Name = "XBox Compatible Gamepad (XInput)";
            Device = d;
        }

        public bool ParseMessage(IButtplugDeviceMessage aMsg)
        {
            switch (aMsg)
            {
                case SingleMotorVibrateMessage m:
                    var v = new Vibration();
                    v.LeftMotorSpeed = (ushort)(m.Speed * 65536);
                    v.RightMotorSpeed = (ushort)(m.Speed * 65536);
                    Device.SetVibration(v);
                    return true;
            }
            return false;
        }
    }
}