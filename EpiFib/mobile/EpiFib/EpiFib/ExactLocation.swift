//
//  Location.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import Foundation
import EVReflection

final class ExactLocation: EVNetworkingObject
{
    var latitude: NSNumber?
    
    var longitude: NSNumber?
    
    convenience init(latitude: Double, longitude: Double)
    {
        self.init()
        self.latitude = NSNumber(value: latitude)
        self.longitude = NSNumber(value: longitude)
    }
}
