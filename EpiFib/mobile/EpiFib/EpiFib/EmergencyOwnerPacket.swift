//
//  EmergencyOwnerPacket.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import EVReflection

class EmergencyOwnerPacket: EVNetworkingObject
{
    var emergencyInstanceId: String?
    
    var emergencyType: String?
        
    var ownerLocation: ExactLocation?
        
    var updatedTime: NSDate?
        
    var createdTime: NSDate?
        
    var ownerId: String?
        
    var nearbyContainers: [EmergencyContainer]?
        
    var numUsersNotified: NSNumber?
    
    var responderInfo: EmergencyUserInfo?

    convenience init(_
        emergencyInstanceId: String,
        emergencyType: String,
        ownerLocation: ExactLocation,
        updatedTime: NSDate,
        createdTime: NSDate,
        ownerId: String,
        nearbyContainers: [EmergencyContainer],
        numUsersNotified: NSNumber,
        responderInfo: EmergencyUserInfo)
    {
        self.init()
        self.emergencyInstanceId = emergencyInstanceId
        self.emergencyType = emergencyType
        self.ownerLocation = ownerLocation
        self.updatedTime = updatedTime
        self.createdTime = createdTime
        self.ownerId = ownerId
        self.nearbyContainers = nearbyContainers
        self.numUsersNotified = numUsersNotified
        self.responderInfo = responderInfo
    }
}
