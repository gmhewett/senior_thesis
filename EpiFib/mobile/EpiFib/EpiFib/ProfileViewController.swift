//
//  ProfileViewController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/18/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import UserNotifications

final class ProfileViewController: UIViewController
{
    var locationService: LocationService?
    var notificationService: NotificationService?
    
    @IBOutlet weak var menuButton: UIBarButtonItem!
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        if revealViewController() != nil
        {
            menuButton.target = self.revealViewController()
            menuButton.action = #selector(SWRevealViewController.revealToggle(_:))
        }
    }
    
    @IBAction func notificationsButtonPressed(_ sender: Any)
    {
        self.notificationService?.requestAuthorization()
    }
    
    @IBAction func locationButtonPressed(_ sender: Any)
    {
        self.locationService?.requestAlwaysLocationAuthorization()
        self.locationService?.startMonitoringUserLocation()
    }
}
