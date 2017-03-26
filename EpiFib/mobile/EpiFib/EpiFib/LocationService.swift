//
//  LocationService.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import Foundation
import CoreLocation

protocol LocationServiceDelegate
{
    func didUpdateLocationPermissions(_ status: CLAuthorizationStatus)
}

final class LocationService: NSObject, CLLocationManagerDelegate
{
    private let distanceFilter: CLLocationDistance = 30
    private let locationAccuracy: CLLocationAccuracy = kCLLocationAccuracyBest
    
    private var lastSentLocation: Date?
    
    private lazy var locationManager: CLLocationManager = {
        let manager = CLLocationManager()
        manager.pausesLocationUpdatesAutomatically = true
        manager.delegate = self
        manager.allowsBackgroundLocationUpdates = true
        manager.distanceFilter = self.distanceFilter
        manager.desiredAccuracy = self.locationAccuracy
        return manager
    }()
    
    private let epiFibApiService: EpiFibApiService
    
    var delegate: LocationServiceDelegate?
    
    init(_ epiFibApiService: EpiFibApiService)
    {
        self.epiFibApiService = epiFibApiService
        
        super.init()
    }
    
    var isAuthorized: Bool {
        get {
            return CLLocationManager.authorizationStatus() == .authorizedAlways
        }
    }
    
    var currentLocation: ExactLocation? {
        get {
            if let coordinates = self.locationManager.location?.coordinate {
                return ExactLocation(latitude: coordinates.latitude, longitude: coordinates.longitude)
            }
            
            return nil
        }
    }
    
    var currentLocation2D: CLLocationCoordinate2D? {
        get {
            if let curLoc = self.currentLocation {
                return CLLocationCoordinate2D(latitude: CLLocationDegrees(curLoc.latitude!), longitude: CLLocationDegrees(curLoc.longitude!))
            }
            
            return nil
        }
    }
    
    func requestAlwaysLocationAuthorization() -> Void
    {
        self.locationManager.requestAlwaysAuthorization()
    }
    
    func startMonitoringUserLocation()
    {
        self.locationManager.desiredAccuracy = self.locationAccuracy
        //self.locationManager.distanceFilter = self.distanceFilter
        self.locationManager.startUpdatingLocation()
    }
    
    func appWillTerminate()
    {
        if self.isAuthorized
        {
            self.locationManager.stopUpdatingLocation()
            self.locationManager.startMonitoringSignificantLocationChanges()
        }
    }
    
    func appIsLaunched()
    {
        self.locationManager.stopMonitoringSignificantLocationChanges()
        
        if self.isAuthorized
        {
            self.startMonitoringUserLocation()
        }
    }
    
    // MARK: - CLLocationManagerDelegate
    
    func locationManager(_ manager: CLLocationManager, didChangeAuthorization status: CLAuthorizationStatus)
    {
        self.delegate?.didUpdateLocationPermissions(status)
    }
    
    func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation])
    {
        if (self.lastSentLocation?.timeIntervalSinceNow ?? TimeInterval(11.0)) < TimeInterval(10.0)
        {
            return
        }
        
        self.lastSentLocation = Date(timeIntervalSinceNow: TimeInterval(0.0))
        
        let coord = locations.last?.coordinate
        let loc = ExactLocation()
        loc.latitude = coord?.latitude as NSNumber?
        loc.longitude = coord?.longitude as NSNumber?
        
        if let json = loc.toDictionary() as? [String : Any]
        {
            self.epiFibApiService.sendRequest(endpoint: "userlocation/update", method: .post, parameters: json)
            .response(completionHandler: { r in
                print("Sent location: \(json)\nStatus code: \(r.response?.statusCode)\n")
            })
        }
    }
    
    func locationManager(_ manager: CLLocationManager, didFailWithError error: Error) {
        print("Location error: \(error)")
    }
}
