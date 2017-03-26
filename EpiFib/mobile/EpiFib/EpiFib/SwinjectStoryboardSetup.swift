//
//  SwinjectStoryboard.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/26/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import SwinjectStoryboard

extension SwinjectStoryboard {
    
    class func setup()
    {
        defaultContainer.storyboardInitCompleted(UINavigationController.self) { _ in }
        
        defaultContainer.storyboardInitCompleted(HomeViewController.self, name: "home") { (r, c) in
            c.emergencyService = r.resolve(EmergencyService.self)
            c.notificationService = r.resolve(NotificationService.self)
        }
        
        defaultContainer.storyboardInitCompleted(MenuViewController.self, name: "menu") { (r, c) in
            c.authService = r.resolve(AuthService.self)
        }
        
        defaultContainer.storyboardInitCompleted(SWRevealViewController.self, name:"reveal") { _ in }
                
        defaultContainer.storyboardInitCompleted(LoginViewController.self, name: "login") { (r, c) in
            c.authService = r.resolve(AuthService.self)
        }
        
        defaultContainer.storyboardInitCompleted(RegisterViewController.self, name: "register") { (r, c) in
            c.authService = r.resolve(AuthService.self)
        }
        
        defaultContainer.storyboardInitCompleted(FindHelpViewController.self, name: "findHelp") { (r, c) in
            c.emergencyService = r.resolve(EmergencyService.self)
            c.locationService = r.resolve(LocationService.self)
        }
        
        defaultContainer.storyboardInitCompleted(ProfileViewController.self, name:"profile") { (r, c) in
            c.locationService = r.resolve(LocationService.self)
            c.notificationService = r.resolve(NotificationService.self)
        }
        
        defaultContainer.storyboardInitCompleted(ResponderViewController.self, name:"responder") { (r, c) in
            c.locationService = r.resolve(LocationService.self)
        }
        
        defaultContainer.storyboardInitCompleted(Call911ViewController.self, name: "call911") { _ in }

        defaultContainer.register(ConfigManager.self) { r -> ConfigManager in
            ConfigManager()
        }.inObjectScope(.container)
        
        defaultContainer.register(EpiFibApiService.self) { r -> EpiFibApiService in
            EpiFibApiService(r.resolve(ConfigManager.self)!, r.resolve(AuthService.self)!)
        }.inObjectScope(.container)
        
        defaultContainer.register(AuthService.self) { _ in AuthService() }
            .initCompleted { (r, s) in
                s.epiFibApiService = r.resolve(EpiFibApiService.self)
            }.inObjectScope(.container)
        
        defaultContainer.register(EmergencyService.self) { r -> EmergencyService in
            EmergencyService(r.resolve(EpiFibApiService.self)!, r.resolve(LocationService.self)!)
        }.inObjectScope(.container)
        
        defaultContainer.register(LocationService.self) { r -> LocationService in
            LocationService(r.resolve(EpiFibApiService.self)!)
        }.inObjectScope(.container)
        
        defaultContainer.register(NotificationService.self) { r -> NotificationService in
            NotificationService(r.resolve(AuthService.self)!)
        }.inObjectScope(.container)
    }
    
    class func getLocationService() -> LocationService
    {
        return defaultContainer.resolve(LocationService.self)!
    }
}
