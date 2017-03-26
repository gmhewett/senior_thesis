//
//  EmergencyUserInfo.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import EVReflection

final class EmergencyUserInfo: EVNetworkingObject
{
    var firstName: String?
    
    var lastName: String?
    
    var base64Pic: String?
    
    var location: ExactLocation?
    
    convenience init(_ firstName: String, lastName: String, base64Pic: String, location: ExactLocation)
    {
        self.init()
        self.firstName = firstName
        self.lastName = lastName
        self.base64Pic = base64Pic
        self.location = location
    }
}
