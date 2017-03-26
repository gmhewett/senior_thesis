//
//  NotificationService.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/25/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import UserNotifications

class NotificationService: NSObject {

    let authService: AuthService
    
    init(_ authService: AuthService) {
        self.authService = authService
    }
    
    func requestAuthorization() {
        let center = UNUserNotificationCenter.current()
        center.requestAuthorization(options: [.badge, .alert, .sound]) { (granted, error) in
            print(granted)
            // Enable or disable features based on authorization.
        }
        UIApplication.shared.registerForRemoteNotifications()
    }
}
