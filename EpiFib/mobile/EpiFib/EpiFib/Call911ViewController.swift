//
//  Call911ViewController.swift
//  EpiFib
//
//  Created by Gregory Hewett on 3/19/17.
//  Copyright © 2017 The Reach Lab, LLC. All rights reserved.
//

import UIKit

final class Call911ViewController: UIViewController
{
    var emergencyType: EmergencyType!
    
    @IBAction func call911ButtonPressed(_ sender: Any)
    {
        UIApplication.shared.open(URL(string: "tel://8064707311")!)
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if segue.identifier == "createEmergencyInstance" {
            if let vc = segue.destination as? FindHelpViewController {
                vc.emergencyType = emergencyType
            }
        }
    }
}
