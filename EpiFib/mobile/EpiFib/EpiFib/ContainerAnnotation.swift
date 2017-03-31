//
//  ContainerAnnotation.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/26/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import MapKit

class ContainerAnnotation: NSObject, MKAnnotation {
    
    var title: String?
    var deviceId: String?
    let coordinate: CLLocationCoordinate2D
    
    init(title: String, locationName: String, coordinate: CLLocationCoordinate2D) {
        self.title = title
        self.deviceId = locationName
        self.coordinate = coordinate
        
        super.init()
    }
    
    var subtitle: String? {
        return self.deviceId
    }
    
}
