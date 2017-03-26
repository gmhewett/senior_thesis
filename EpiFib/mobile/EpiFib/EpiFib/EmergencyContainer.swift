//
//  EmergencyContainer.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import EVReflection

final class EmergencyContainer: EVNetworkingObject
{
    var emergencyTypesSupported: [EmergencyType]?
    
    var exactLocation: ExactLocation?
    
    convenience init(_ emergencyTypesSupported: [EmergencyType], exactLocation: ExactLocation)
    {
        self.init()
        self.emergencyTypesSupported = emergencyTypesSupported
        self.exactLocation = exactLocation        
    }
}
