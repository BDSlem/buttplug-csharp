﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buttplug.Core;
using Buttplug.Core.Messages;

namespace Buttplug.Server.Test
{
    internal class TestDevice : ButtplugDevice
    {
        public double V1 = 0;
        public double V2 = 0;

        public TestDevice(IButtplugLogManager aLogManager, string aName, string aIdentifier = null)
            : base(aLogManager, aName, aIdentifier ?? aName)
        {
            MsgFuncs.Add(typeof(SingleMotorVibrateCmd), new ButtplugDeviceWrapper(HandleSingleMotorVibrateCmd));
            MsgFuncs.Add(typeof(VibrateCmd), new ButtplugDeviceWrapper(HandleVibrateCmd, new MessageAttributes() { FeatureCount = 2 }));
            MsgFuncs.Add(typeof(StopDeviceCmd), new ButtplugDeviceWrapper(HandleStopDeviceCmd));
        }

        private Task<ButtplugMessage> HandleStopDeviceCmd(ButtplugDeviceMessage aMsg)
        {
            V1 = V2 = 0;
            return Task.FromResult<ButtplugMessage>(new Ok(aMsg.Id));
        }

        private Task<ButtplugMessage> HandleVibrateCmd(ButtplugDeviceMessage aMsg)
        {
            var cmdMsg = aMsg as VibrateCmd;
            if (cmdMsg is null)
            {
                return Task.FromResult<ButtplugMessage>(BpLogger.LogErrorMsg(aMsg.Id, Error.ErrorClass.ERROR_DEVICE, "Wrong Handler"));
            }

            if (cmdMsg.Speeds.Count < 1 || cmdMsg.Speeds.Count > 2)
            {
                Task.FromResult<ButtplugMessage>(new Error(
                    "VibrateCmd requires between 1 and 2 vectors for this device.",
                    Error.ErrorClass.ERROR_DEVICE,
                    cmdMsg.Id));
            }

            foreach (var vi in cmdMsg.Speeds)
            {
                if (vi.Index == 0)
                {
                    V1 = vi.Speed;
                }
                else if (vi.Index == 1)
                {
                    V2 = vi.Speed;
                }
                else
                {
                    Task.FromResult<ButtplugMessage>(new Error(
                        $"Index {vi.Index} is out of bounds for VibrateCmd for this device.",
                        Error.ErrorClass.ERROR_DEVICE,
                        cmdMsg.Id));
                }
            }

            return Task.FromResult<ButtplugMessage>(new Ok(aMsg.Id));
        }

        private Task<ButtplugMessage> HandleSingleMotorVibrateCmd(ButtplugDeviceMessage aMsg)
        {
            var cmdMsg = aMsg as SingleMotorVibrateCmd;

            if (cmdMsg is null)
            {
                return Task.FromResult<ButtplugMessage>(BpLogger.LogErrorMsg(aMsg.Id, Error.ErrorClass.ERROR_DEVICE, "Wrong Handler"));
            }

            var speeds = new List<VibrateCmd.VibrateSubcommand>();
            for (uint i = 0; i < 2; i++)
            {
                speeds.Add(new VibrateCmd.VibrateSubcommand(i, cmdMsg.Speed));
            }

            return HandleVibrateCmd(new VibrateCmd(cmdMsg.DeviceIndex, speeds, cmdMsg.Id));
        }

        public void RemoveDevice()
        {
            InvokeDeviceRemoved();
        }

        public override void Disconnect()
        {
            RemoveDevice();
        }
    }
}
