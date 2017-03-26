//
//  EmergencyInstanceRequest.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import EVReflection

final class EmergencyInstanceRequest: EVNetworkingObject
{
    var emergencyInstanceId: String?
    
    var emergencyType: EmergencyType?
    
    var ownerLocation: ExactLocation?
    
    var updatedTime: NSDate?
    
    var createdTime: NSDate?
    
    convenience init(_ emergencyType: EmergencyType, _ ownerLocation: ExactLocation)
    {
        self.init()
        self.emergencyType = emergencyType
        self.ownerLocation = ownerLocation
    }
}
