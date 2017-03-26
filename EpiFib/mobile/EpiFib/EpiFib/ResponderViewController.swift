//
//  ResponderViewController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/23/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import MapKit
import UIKit

class ResponderViewController: UIViewController, MKMapViewDelegate {

    var emergencyType: EmergencyType?
    var locationService: LocationService?
    
    var activityIndicator: ProgressHUD?
    var emergencyResponderPacket: EmergencyResponderPacket?
    
    @IBOutlet weak var mapView: MKMapView!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.initEmergency()
        self.mapView.delegate = self
    }
    
    override func viewDidAppear(_ animated: Bool) {
        super.viewDidAppear(animated)
        
        self.activityIndicator = ProgressHUD(text: "Getting Directions")
        self.view.addSubview(self.activityIndicator!)
        self.initMap()
    }
    
    @IBAction func endButtonPressed(_ sender: Any) {
        self.dismiss(animated: true, completion: nil)
    }
    
    func initEmergency()
    {
        
    }
    
    func initMap()
    {
        self.mapView.showsUserLocation = true
        //let ownerLat = self.emergencyResponderPacket?.ownerLocation.latitude
        //let ownerLong = self.emergencyResponderPacket?.ownerLocation.longitude
        // Mass Ave 42.373071, -71.117941
        //let ownerLocation = CLLocationCoordinate2D(latitude: ownerLat!, longitude: ownerLong!)
        let ownerLocation = CLLocationCoordinate2D(latitude: 42.373071, longitude: -71.117941)
        
        let currentLocation = self.locationService?.currentLocation2D
        
        self.showDirections(source: currentLocation!, destination: ownerLocation)
    }
    
    func showDirections(source: CLLocationCoordinate2D, destination: CLLocationCoordinate2D)
    {
        let request: MKDirectionsRequest = MKDirectionsRequest()
        request.source = MKMapItem(placemark: MKPlacemark(coordinate: source))
        request.destination = MKMapItem(placemark: MKPlacemark(coordinate: destination))
        request.requestsAlternateRoutes = true
        request.transportType = .walking
        
        let directions = MKDirections(request: request)
        directions.calculate { (response, error) in
            if let routes = response?.routes {
                let route: MKRoute = routes.sorted(by: {$0.expectedTravelTime < $1.expectedTravelTime})[0]
                self.mapView.add(route.polyline)
                self.mapView.setVisibleMapRect(route.polyline.boundingMapRect,
                                          edgePadding: UIEdgeInsetsMake(30.0, 30.0, 30.0, 30.0),
                                          animated: true)
                self.activityIndicator?.removeFromSuperview()
            } else {
                let alert = UIAlertController(title: nil, message: "Directions not available.", preferredStyle: .alert)
                let okButton = UIAlertAction(title: "Ok", style: .default, handler: nil)
                
                alert.addAction(okButton)
                self.present(alert, animated: true, completion: nil)
            }
        }
    }
    
    // MARK: - MKMapViewDelegate
    
    func mapView(_ mapView: MKMapView, rendererFor overlay: MKOverlay) -> MKOverlayRenderer {
        let polylineRenderer = MKPolylineRenderer(overlay: overlay)
        if (overlay is MKPolyline) {
            if mapView.overlays.count == 1 {
                polylineRenderer.strokeColor =
                    UIColor.blue.withAlphaComponent(0.75)
            } else if mapView.overlays.count == 2 {
                polylineRenderer.strokeColor =
                    UIColor.green.withAlphaComponent(0.75)
            } else if mapView.overlays.count == 3 {
                polylineRenderer.strokeColor =
                    UIColor.red.withAlphaComponent(0.75)
            }
            polylineRenderer.lineWidth = 5
        }
        return polylineRenderer
    }
}
