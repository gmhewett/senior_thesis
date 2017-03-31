//
//  HomeController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 2/25/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit
import Alamofire
import KeychainSwift
import UserNotifications

final class HomeViewController: UIViewController, SWRevealViewControllerDelegate
{
    var notificationService: NotificationService?
    var emergencyService: EmergencyService?
    var currentEmergency: EmergencyOwnerPacket?
    var helpNeeded: EmergencyInstanceRequest?
    var responderPacket: EmergencyResponderPacket?
    
    @IBOutlet weak var menuButton: UIBarButtonItem!
    @IBOutlet weak var responderLabel: UILabel!
    @IBOutlet weak var respondButton: UIButton!
    @IBOutlet weak var findEpiButton: UIButton!
    @IBOutlet weak var findDefibButton: UIButton!
    @IBOutlet weak var currentEmergencyLabel: UILabel!
    @IBOutlet weak var goToCurrentEmergencyButton: UIButton!
    
    override func viewDidLoad()
    {
        super.viewDidLoad()
    
        if revealViewController() != nil
        {
            menuButton.target = self.revealViewController()
            menuButton.action = #selector(SWRevealViewController.revealToggle(_:))
            
            revealViewController().delegate = self
            
            revealViewController().tapGestureRecognizer()
            revealViewController().panGestureRecognizer()
        }
        
        self.initCurrentEmergencyButton(enable: false)
        self.initHelpNeededButton(enable: false)
        self.checkForCurrentEmergency()
        self.checkIfHelpNeeded()
        
        // self.responderLabel.isHidden = true
        // self.respondButton.isHidden = true
    }
    
    override func viewDidAppear(_ animated: Bool)
    {
        super.viewDidAppear(animated)
        
        if !UserDefaults.standard.bool(forKey: "isLoggedIn") {
            if let loginViewController = self.storyboard?.instantiateViewController(withIdentifier: "Login") {
                self.navigationController?.present(loginViewController, animated: true, completion: nil)
            }
        } else {
            self.notificationService?.requestAuthorization()
        }
        
        //this.checkForEmergenciesNearby()
    }
    
    @IBAction func goToCurrentEmergencyButtonPressed(_ sender: Any)
    {
        self.performSegue(withIdentifier: "goToEmergency", sender: self)
    }
    
    func checkForCurrentEmergency()
    {
        self.emergencyService?.getCurrentEmergency { (ownerPacket, error) in
            if error != nil
            {
                print("Error: \(error.debugDescription)\n")
            }
            else
            {
                self.currentEmergency = ownerPacket
                self.initCurrentEmergencyButton(enable: true)
            }
        }
    }
    
    func initCurrentEmergencyButton(enable: Bool)
    {
        self.currentEmergencyLabel.text = enable ? "Trying to find help!" : "No emergecies created."
        self.goToCurrentEmergencyButton.setTitle(enable ? "Go to map" : "", for: .normal)
        self.goToCurrentEmergencyButton.isHidden = !enable
    }
    
    func checkIfHelpNeeded()
    {
        self.emergencyService?.checkIfHelpNeeded(completion: { (emergencyRequest, error) in
            if error != nil {
                print(error!)
            } else {
                if emergencyRequest != nil {
                    self.helpNeeded = emergencyRequest
                    self.initHelpNeededButton(enable: true)
                }
            }
        })
    }
    
    func initHelpNeededButton(enable: Bool)
    {
        self.responderLabel.text = enable ? "Someone needs your help!" : "All is well"
        self.respondButton.setTitle(enable ? "Be a hero" : "", for: .normal)
        self.respondButton.isHidden = !enable
    }
    
    @IBAction func respondButtonPressed(_ sender: Any) {
        self.emergencyService?.respondToEmergency(emergencyInstanceId: (self.helpNeeded?.emergencyInstanceId)!, completion: { (responderPacket, error) in
            if error != nil {
                print(error!)
            } else {
                if responderPacket != nil {
                    self.responderPacket = responderPacket
                    self.performSegue(withIdentifier: "respondToEmergency", sender: self)
                }
            }
        })
    }
    
    
    // MARK: - Navigation
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if let vc = segue.destination as? Call911ViewController {
            if segue.identifier == "autoinjectorEmergency" {
                vc.emergencyType = EmergencyType.Autoinjector
            } else if segue.identifier == "defibEmergency" {
                vc.emergencyType = EmergencyType.Defib
            }
        } else if let vc = segue.destination as? FindHelpViewController {
            vc.ownerPacket = self.currentEmergency
        } else if let vc = segue.destination as? ResponderViewController {
            vc.emergencyResponderPacket = self.responderPacket
        }
    }
    
    // MARK: - SWRevealViewControllerDelegate
    
    func revealController(_ revealController: SWRevealViewController!, willMoveTo position: FrontViewPosition) {
        if position == FrontViewPosition.right {
            revealController.frontViewController.view.isUserInteractionEnabled = false
        }
        else {
            revealController.frontViewController.view.isUserInteractionEnabled = true
        }
    }
}
