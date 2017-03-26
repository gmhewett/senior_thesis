//
//  EmergencyResponderPacket.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import EVReflection

final class EmergencyResponderPacket: EVNetworkingObject
{
    var emergencyInstanceId: String?
    
    var emergencyType: EmergencyType?
    
    var ownerLocation: ExactLocation?
    
    var updatedTime: NSDate?
    
    var createdTime: NSDate?
    
    var ownerInfo: EmergencyUserInfo?
    
    convenience init(
        emergencyInstanceId: String,
        emergencyType: EmergencyType,
        ownerLocation: ExactLocation,
        updatedTime: NSDate,
        createdTime: NSDate,
        ownerInfo: EmergencyUserInfo)
    {
        self.init()
        self.emergencyInstanceId = emergencyInstanceId
        self.emergencyType = emergencyType
        self.ownerLocation = ownerLocation
        self.updatedTime = updatedTime
        self.createdTime = createdTime
        self.ownerInfo = ownerInfo
    }
}
