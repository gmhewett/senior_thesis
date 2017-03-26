//
//  FindHelpViewController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import MapKit
import CoreLocation

final class FindHelpViewController: UIViewController, CLLocationManagerDelegate, MKMapViewDelegate, EmergencyServiceDelegate
{
    var emergencyType: EmergencyType!
    var emergencyService: EmergencyService?
    var locationService: LocationService?
    
    var ownerPacket: EmergencyOwnerPacket?
    var activityIndicator: ProgressHUD?
    var mapIsInited: Bool = false
    
    @IBOutlet weak var mapView: MKMapView!
    
    override func viewDidLoad()
    {
        super.viewDidLoad()
        
        if self.ownerPacket == nil {
            self.initEmergencyInstance()
        }
        
        if !mapIsInited {
            self.mapIsInited = true
            self.initMapView()
        }
    }
    
    override func viewDidAppear(_ animated: Bool)
    {
        super.viewDidAppear(animated)
    }
    
    // MARK: - CLLocationManagerDelegate
    
    func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation])
    {
        let lastLocation: CLLocation = locations.last!
        print("Latitude: \(lastLocation.coordinate.latitude) Longitude: \(lastLocation.coordinate.longitude)\n")

    }
    
    func locationManager(_ manager: CLLocationManager, didFailWithError error: Error)
    {
        print("Location error: \(error)\n")
    }
    
    // MARK: - EmergencyServiceDelegate
    
    func didUpdateEmergencyInstance(_ ownerPacket: EmergencyOwnerPacket)
    {
        //
    }
    
    // MARK: - Helpers
    
    private func initEmergencyInstance()
    {
        self.emergencyService?.delegate = self
        self.emergencyService?.createEmergencyInstance(type: self.emergencyType, completion: { (ownerPacket, error) in
            if error != nil
            {
                let alertController = UIAlertController(title: "Error", message: "Could not create emergency instance. Please call 911.", preferredStyle: .alert)
                alertController.addAction(UIAlertAction(title: "Ok", style: .default, handler: nil))
                self.present(alertController, animated: true, completion: nil)
            }
            else
            {
                self.ownerPacket = ownerPacket
                if !self.mapIsInited
                {
                    self.mapIsInited = true
                    self.initMapView()
                }
            }
        })
    }
    
    private func initMapView()
    {
//        if let initialLocation = locationManager.location
//        {
//            self.centerMapOnLocation(location: initialLocation)
//        }
        
        self.mapView.showsUserLocation = true
    }
    
    private func centerMapOnLocation(location: CLLocation)
    {
//        let coordinateRegion = MKCoordinateRegionMakeWithDistance(
//            location.coordinate,
//            self.regionRadius * 2.0,
//            self.regionRadius * 2.0)
//        
//        self.mapView.setRegion(coordinateRegion, animated: true)
    }
}
