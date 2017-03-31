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
    let regionRadius = 500.0
    var turnContainerOn = true
    
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
        self.mapView.delegate = self
        
        if let initialLocation = self.locationService?.currentLocation
        {
            self.centerMapOnLocation(location: CLLocation(latitude: CLLocationDegrees(initialLocation.latitude!), longitude: CLLocationDegrees(initialLocation.longitude!)))
        }
        
        self.mapView.showsUserLocation = true
        
        if self.ownerPacket?.nearbyContainers != nil {
            for container in (self.ownerPacket?.nearbyContainers)! {
                let annotation = ContainerAnnotation(title: "Container", locationName: container.deviceId ?? "", coordinate: CLLocationCoordinate2D(latitude: CLLocationDegrees((container.location?.latitude)!), longitude: CLLocationDegrees((container.location?.longitude)!)))
                self.mapView.addAnnotation(annotation)
            }
        }
    }
    
    private func centerMapOnLocation(location: CLLocation)
    {
        let coordinateRegion = MKCoordinateRegionMakeWithDistance(
            location.coordinate,
            self.regionRadius * 2.0,
            self.regionRadius * 2.0)
        
        self.mapView.setRegion(coordinateRegion, animated: true)
    }
    
    func mapView(_ mapView: MKMapView, viewFor annotation: MKAnnotation) -> MKAnnotationView? {
        if let annotation = annotation as? ContainerAnnotation {
            let identifier = "pin"
            var view: MKPinAnnotationView
            if let dequeuedView = mapView.dequeueReusableAnnotationView(withIdentifier: identifier)
                as? MKPinAnnotationView { // 2
                dequeuedView.annotation = annotation
                view = dequeuedView
            } else {
                // 3
                view = MKPinAnnotationView(annotation: annotation, reuseIdentifier: identifier)
                view.canShowCallout = true
                view.calloutOffset = CGPoint(x: -5, y: 5)
                view.rightCalloutAccessoryView = UIButton(type: .detailDisclosure) as UIView
            }
            return view
        }
        return nil
    }
    
    func mapView(_ mapView: MKMapView, annotationView view: MKAnnotationView, calloutAccessoryControlTapped control: UIControl) {
        print(control)
        print(view)
        
        if control == view.rightCalloutAccessoryView {
            if let container = view.annotation as? ContainerAnnotation {
                self.emergencyService?.toggleContainer(on: self.turnContainerOn, containerId: container.deviceId!, emergencyInstanceId: (self.ownerPacket?.emergencyInstanceId)!)
                self.turnContainerOn = !self.turnContainerOn
            }
        }
    }

}
