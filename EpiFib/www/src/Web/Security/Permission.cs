// <copyright file="Permission.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring solution</remarks>

namespace Web.Security
{
    public enum Permission
    {
        ViewDevices,
        EditDeviceMetadata,
        AddDevices,
        RemoveDevices,
        DisableEnableDevices,
        SendCommandToDevices,
        ViewDeviceSecurityKeys,
        ViewActions,
        AssignAction,
        ViewRules,
        EditRules,
        DeleteRules,
        ViewTelemetry,
        HealthBeat,
        LogicApps,
        CellularConn
    }
}